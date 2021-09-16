﻿using Emgu.CV;
using Emgu.CV.Structure;

using Kogerent.Core;
using Kogerent.Logger;
using Kogerent.Services.Interfaces;

using LaserScan.Core.NetStandart.Models;

using MathNet.Numerics;

using Prism;
using Prism.Commands;
using Prism.Regions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class OneCameraContentViewModel : RegionViewModelBase, IActiveAware
    {
        public event EventHandler IsActiveChanged;

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                SetProperty(ref _isActive, value);
                if (_isActive)
                {
                    OnNavigatedTo();
                }
                OnIsActiveChanged();
            }
        }

        private void OnIsActiveChanged()
        {

            IsActiveChanged?.Invoke(this, new EventArgs());
        }


        private bool _needToDrawDefects;
        private Queue<byte[]> _videoBuffer;
        private ConcurrentQueue<BufferData> _concurentVideoBuffer;
        private int _width;
        private int _strobe;
        private readonly Task _processVideoWork;
        private Image<Bgr, byte> _resImage;
        private IOrderedEnumerable<DefectProperties> _defects;
        private DispatcherTimer _drawingTimer;
        private object _locker = new();
        private Bgr _red = new Bgr(0, 0, 255);
        private Bgr _blue = new Bgr(255, 0, 0);
        private Stopwatch imgProcessingStopWatch = new();
        Image<Gray, byte> img = new Image<Gray, byte>(6144, 1000);
        private byte[] deltas = new byte[6144];

        public int Shift { get; set; }

        private BaslerCameraModel _currentCamera;
        public BaslerCameraModel CurrentCamera
        {
            get { return _currentCamera; }
            set { SetProperty(ref _currentCamera, value); }
        }

        private BitmapImage _imageSource;
        public BitmapImage ImageSource
        {
            get { return _imageSource; }
            set { SetProperty(ref _imageSource, value); }
        }

        private DelegateCommand _init;
        public DelegateCommand Init => _init ??= new DelegateCommand(Executeinit);

        private DelegateCommand _startGrab;
        public DelegateCommand StartGrab => _startGrab ??= new DelegateCommand(ExecuteStartGrab);

        private DelegateCommand _stopCamera;
        public DelegateCommand StopCamera => _stopCamera ??= new DelegateCommand(ExecuteStopCamera);

        private DelegateCommand _calibrate;
        public DelegateCommand Calibrate =>
            _calibrate ?? (_calibrate = new DelegateCommand(ExecuteCalibrate));

        private DelegateCommand _clearDefects;
        private int _cnt = 0;

        public DelegateCommand ClearDefects =>
            _clearDefects ?? (_clearDefects = new DelegateCommand(ExecuteClearDefects));



        public IApplicationCommands ApplicationCommands { get; }
        public IImageProcessingService ImageProcessing { get; }
        public IDefectRepository DefectRepository { get; }
        public IMathService MathService { get; }
        public IXmlService XmlService { get; }
        public IBaslerRepository BaslerRepository { get; }
        public ILogger Logger { get; }
        public IFooterRepository FooterRepository { get; }
        public IBenchmarkRepository BenchmarkRepository { get; }

        public OneCameraContentViewModel(IRegionManager regionManager, IApplicationCommands applicationCommands,
                                         IImageProcessingService imageProcessing, IDefectRepository defectRepository,
                                         IMathService mathService, IXmlService xmlService, IBaslerRepository baslerRepository,
                                         ILogger logger, IFooterRepository footerRepository, IBenchmarkRepository benchmarkRepository) : base(regionManager)
        {
            ApplicationCommands = applicationCommands;
            ApplicationCommands.Calibrate.RegisterCommand(Calibrate);
            ApplicationCommands.StartAllSensors.RegisterCommand(StartGrab);
            ApplicationCommands.SaveDefectsAndCrearTable.RegisterCommand(ClearDefects);
            ImageProcessing = imageProcessing;
            DefectRepository = defectRepository;
            MathService = mathService;
            XmlService = xmlService;
            BaslerRepository = baslerRepository;
            Logger = logger;
            FooterRepository = footerRepository;
            BenchmarkRepository = benchmarkRepository;
            //CurrentCamera.CameraImageEvent += ImageGrabbed;
            var uriSource = new Uri(@"/Defectoscope.Modules.Cameras;component/Images/ImageSurce_cam.png", UriKind.Relative);
            ImageSource = new BitmapImage(uriSource);
            _videoBuffer = new();
            _concurentVideoBuffer = new();
            _processVideoWork = new Task(() => ProcessImageAction());
            _processVideoWork.Start();
            _drawingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _drawingTimer.Tick += _drawingTimer_Tick;
            _drawingTimer.Start();
        }



        private void _drawingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (_resImage != null)
                {
                    ImageSource = MathService.BitmapToImageSource(_resImage.ToBitmap());

                }
                if (_defects != null && _needToDrawDefects)
                {
                    DefectRepository.DefectsCollection.AddRange(_defects);
                    _needToDrawDefects = false;
                }
                BaslerRepository.TotalCount = _concurentVideoBuffer.Count;
                BenchmarkRepository.ImageProcessingSpeedCounter = imgProcessingStopWatch.ElapsedTicks / 10_000d;
            }
            catch (Exception ex)
            {
                string msg = $"{ex.Message}";
                Logger?.Error(msg);
                FooterRepository.Text = msg;
                ExecuteStopCamera();
            }

        }

        private void ImageGrabbed(object sender, BufferData e)
        {
            if (!CurrentCamera.CalibrationMode)
            {
                _concurentVideoBuffer.Enqueue(e);
            }
            else
            {
                PerformCalibration(e);
                CurrentCamera.CalibrationMode = false;
            }
        }

        private void PerformCalibration(BufferData e)
        {
            List<List<byte>> lines = e.Data.SplitByCount(e.Width).ToList();
            if (lines != null && lines.Count > 0)
            {
                var line = lines[0];
                var xs = new double[line.Count];
                var ys = new double[line.Count];
                for (int i = 0; i < line.Count; i++)
                {
                    xs[i] = i;
                    ys[i] = 127 - line[i];
                }

                double[] p = Fit.Polynomial(xs, ys, 3);
                if (p != null)
                {
                    CurrentCamera.P = p;
                }

                for (int i = 0; i < deltas.Length; i++)
                {
                    double p0 = CurrentCamera.P[0];
                    double p1 = CurrentCamera.P[1] * i;
                    double p2 = CurrentCamera.P[2] * Math.Pow(i, 2);
                    double p3 = CurrentCamera.P[3] * Math.Pow(i, 3);
                    deltas[i] = (byte)(p0 + p1 + p2 + p3);
                }
            }
        }

        private /*async*/ void ProcessImageAction()
        {
            while (true)
            {
                int count = _concurentVideoBuffer.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var res = _concurentVideoBuffer.TryDequeue(out var bufferData);
                        if (res && bufferData != default)
                        {
                            //Span<List<byte>> lines = bufferData.Data.SplitByCount(bufferData.Width).ToArray();
                            Span<byte> lines = bufferData.Data;

                            //for (int n = 0, offset = 0; n < bufferData.Height; n++, offset += bufferData.Width)
                            //{
                            //    Span<byte> slice = lines.Slice(offset, bufferData.Width);

                            //    for (int j = 0; j < slice.Length; j++)
                            //    {
                            //        double p0 = CurrentCamera.P[0];
                            //        double p1 = CurrentCamera.P[1] * j;
                            //        double p2 = CurrentCamera.P[2] * Math.Pow(j, 2);
                            //        double p3 = CurrentCamera.P[3] * Math.Pow(j, 3);
                            //        double y = p0 + p1 + p2 + p3;
                            //        byte newY = (byte)(slice[j] + y);
                            //        slice[j] = newY;
                            //    }
                            //    //Parallel.For(0, line.Count, (j) => 
                            //    //{
                            //    //    double p0 = CurrentCamera.P[0];
                            //    //    double p1 = CurrentCamera.P[1] * j;
                            //    //    double p2 = CurrentCamera.P[2] * Math.Pow(j, 2);
                            //    //    double p3 = CurrentCamera.P[3] * Math.Pow(j, 3);
                            //    //    double y = p0 + p1 + p2 + p3;
                            //    //    byte newY = (byte)(line[j] + y);
                            //    //    line[j] = newY;
                            //    //});
                            //    //_videoBuffer.Enqueue(slice.ToArray());
                            //    Buffer.BlockCopy(slice.ToArray(), 0, img.Data, offset * _cnt, slice.Length);
                            //    _strobe++;
                            //    if (_cnt == 200)
                            //    {
                            //        _cnt = 1;
                            //        ProcessImage();
                            //    }
                            //}
                            Buffer.BlockCopy(bufferData.Data, 0, img.Data, _cnt * _width, bufferData.Data.Length);
                            _cnt += bufferData.Height;
                            if (_cnt == 1000)
                            {
                                _cnt = 0;
                                ProcessImage();
                            }
                        }
                    }
                }

                //await Task.Delay(TimeSpan.FromTicks(1_000));
                Thread.Sleep(TimeSpan.FromTicks(1000));
            }
        }

        private void ProcessImage()
        {
            try
            {
                imgProcessingStopWatch.Restart();
                for (int y = 0; y < 1000; y++)
                {
                    for (int x = 0; x < _width; x++)
                    { 
                        img.Data[y, x, 0] += deltas[x];
                    }
                }

                //(Image<Bgr, byte> img, var defects) = ImageProcessing.AnalyzeDefects(imgUp, imgDn, CurrentCamera.WidthThreshold,
                //                                                        CurrentCamera.HeightThreshold,
                //                                                        CurrentCamera.WidthDescrete,
                //                                                        CurrentCamera.HeightDescrete, _strobe);
                //foreach (var defect in defects)
                //{
                //    defect.X += Shift;
                //}
                //_resImage = img.Clone(); /*imgUp.Convert<Bgr, byte>();*/
                //var imgTotal = new Image<Gray, byte>(imgTotalData);
                _resImage = img.Convert<Bgr, byte>();
                //if (defects.Any()) _needToDrawDefects = true;
                //_defects = defects;

                //_resImage = imgTotal.Clone();
                imgProcessingStopWatch.Stop();
            }
            catch (Exception ex)
            {
                string msg = $"{ex.Message}";
                Logger?.Error(msg);
                FooterRepository.Text = msg;
                ExecuteStopCamera();
            }
        }

        #region Execute methods
        private void ExecuteClearDefects()
        {
            DefectRepository.DefectsCollection.Clear();
        }

        private void Executeinit()
        {
            if (CurrentCamera == null) return;
            CurrentCamera.CameraInit();

        }

        private void ExecuteStartGrab()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                CurrentCamera.Start();
            }
        }

        private void ExecuteCalibrate()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                CurrentCamera.CalibrationMode = true;
                CurrentCamera.OneShotForCalibration();
            }
        }

        private void ExecuteStopCamera()
        {
            if (CurrentCamera == null) return;
            CurrentCamera.Initialized = false;
            CurrentCamera.StopAndKill();
        }

        public override void Destroy()
        {
            CurrentCamera.CameraImageEvent -= ImageGrabbed;
            ApplicationCommands.StartAllSensors.UnregisterCommand(StartGrab);
            ApplicationCommands.Calibrate.UnregisterCommand(Calibrate);
            ApplicationCommands.SaveDefectsAndCrearTable.UnregisterCommand(ClearDefects);
            base.Destroy();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        private void OnNavigatedTo()
        {
            CurrentCamera.CameraImageEvent += ImageGrabbed;

            _width = CurrentCamera.RightBorder - CurrentCamera.LeftBorder;

        }
        #endregion

    }
}
