using Emgu.CV;
using Emgu.CV.Structure;

using Kogerent.Core;
using Kogerent.Logger;
using Kogerent.Services.Interfaces;

using LaserScan.Core.NetStandart.Models;

using Prism;
using Prism.Commands;
using Prism.Regions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class OneCameraContentViewModel : RegionViewModelBase, IActiveAware
    {
        #region IActiveAware
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
        #endregion

        #region Private Fields
        private bool _needToDrawDefects;
        private Queue<byte[]> _videoBuffer;
        private ConcurrentQueue<BufferData> _concurentVideoBuffer;
        private ConcurrentQueue<byte[,,]> _imageDataBuffer;
        private int _width;
        private int _strobe;
        private readonly Task _processVideoWork;
        private readonly Task _processImageTask;
        private bool _needToProcessImage;
        private Image<Bgr, byte> _resImage;
        private IOrderedEnumerable<DefectProperties> _defects;
        private DispatcherTimer _drawingTimer;
        private Stopwatch imgProcessingStopWatch = new();
        private Image<Gray, byte> img;
        private Image<Gray, byte> tempImage;
        private byte[,,] imgData;
        private bool _needToSave = true;
        private int _cnt;
        #endregion

        #region Delegate Commands
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

        public DelegateCommand ClearDefects =>
            _clearDefects ?? (_clearDefects = new DelegateCommand(ExecuteClearDefects));
        #endregion

        #region Properties
        public IApplicationCommands ApplicationCommands { get; }
        public IImageProcessingService ImageProcessing { get; }
        public IDefectRepository DefectRepository { get; }
        public IMathService MathService { get; }
        public IXmlService XmlService { get; }
        public IBaslerRepository BaslerRepository { get; }
        public ILogger Logger { get; }
        public IFooterRepository FooterRepository { get; }
        public IBenchmarkRepository BenchmarkRepository { get; }
        public ICalibrateService CalibrateService { get; }
        public int Shift { get; set; }
        #endregion

        public OneCameraContentViewModel(IRegionManager regionManager, IApplicationCommands applicationCommands,
                                         IImageProcessingService imageProcessing, IDefectRepository defectRepository,
                                         IMathService mathService, IXmlService xmlService, IBaslerRepository baslerRepository,
                                         ILogger logger, IFooterRepository footerRepository, IBenchmarkRepository benchmarkRepository,
                                         ICalibrateService calibrateService) : base(regionManager)
        {
            ApplicationCommands = applicationCommands;
            ApplicationCommands.Calibrate.RegisterCommand(Calibrate);
            ApplicationCommands.StartAllSensors.RegisterCommand(StartGrab);
            ApplicationCommands.SaveDefectsAndCrearTable.RegisterCommand(ClearDefects);
            ApplicationCommands.InitAllSensors.RegisterCommand(Init);
            ApplicationCommands.StopAllSensors.RegisterCommand(StopCamera);
            ImageProcessing = imageProcessing;
            DefectRepository = defectRepository;
            MathService = mathService;
            XmlService = xmlService;
            BaslerRepository = baslerRepository;
            Logger = logger;
            FooterRepository = footerRepository;
            BenchmarkRepository = benchmarkRepository;
            CalibrateService = calibrateService;
            //CurrentCamera.CameraImageEvent += ImageGrabbed;
            var uriSource = new Uri(@"/Defectoscope.Modules.Cameras;component/Images/ImageSurce_cam.png", UriKind.Relative);
            ImageSource = new BitmapImage(uriSource);
            _videoBuffer = new();
            _concurentVideoBuffer = new();
            _imageDataBuffer = new();

            _processVideoWork = new Task(() => ProceesBuffersAction());
            _processVideoWork.Start();

            _processImageTask = new Task(() => ProcessImageAction());
            _processImageTask.Start();

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
                    Bitmap bmp = _resImage.ToBitmap();
                    ImageSource = MathService.BitmapToImageSource(bmp);
                }
                if (_defects != null && _needToDrawDefects)
                {
                    DefectRepository.DefectsCollection.AddRange(_defects);
                    _needToDrawDefects = false;
                }
                BaslerRepository.TotalCount = _concurentVideoBuffer.Count;
                BenchmarkRepository.ImageProcessingSpeedCounter = imgProcessingStopWatch.ElapsedTicks / 10_000d;
                BenchmarkRepository.TempQueueCount = _imageDataBuffer.Count;
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
            if (CurrentCamera.Deltas == null)
            {
                FooterRepository.Text = "Cameras aren't calibrated";
                return;
            }
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
            int index = lines.Count >= 5 ? 4 : 0;
            //(CurrentCamera.P, deltas) = CalibrateService.Calibrate(lines[index].ToArray());
            CurrentCamera.Deltas = CalibrateService.CalibrateRaw(lines[index].ToArray());
        }

        private async void ProceesBuffersAction()
        {
            while (true)
            {
                int count = _concurentVideoBuffer.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var res = _concurentVideoBuffer.TryDequeue(out BufferData bufferData);
                        if (res && bufferData != default)
                        {
                            Buffer.BlockCopy(bufferData.Data, 0, tempImage.Data, _cnt * _width, bufferData.Data.Length);
                            _cnt += bufferData.Height;
                            _strobe += bufferData.Height;
                            bufferData.Dispose();
                            //GC.Collect();
                            if (_cnt == 1000)
                            {
                                byte[,,] data3Darray = new byte[1000, _width, 1];
                                Buffer.BlockCopy(tempImage.Data, 0, data3Darray, 0, tempImage.Data.Length);
                                _imageDataBuffer.Enqueue(data3Darray);
                                _cnt = 0;
                                _needToProcessImage = true;
                            }
                        }
                    }
                }
                await Task.Delay(1);
            }
        }

        private void ProcessImageAction()
        {
            while (true)
            {
                if (_needToProcessImage)
                {
                    try
                    {
                        if (_imageDataBuffer.TryDequeue(out var dataBuffer))
                        {
                            _imageDataBuffer.Clear();
                            imgProcessingStopWatch.Restart();
                            img.Data = dataBuffer;


                            if (!BenchmarkRepository.RawImage)
                            {
                                for (int y = 0; y < img.Height; y++)
                                {
                                    for (int x = 0; x < _width; x++)
                                    {
                                        //img.Data[y, x, 0] += deltas[x];
                                        img.Data[y, x, 0] = (byte)(img.Data[y, x, 0] + CurrentCamera.Deltas[x]);
                                    }
                                }
                            }
                            using (var upImg = img.CopyBlank())
                            using (var dnImg = img.CopyBlank())
                            {
                                CvInvoke.Threshold(img, upImg, CurrentCamera.DownThreshold, 255, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
                                CvInvoke.Threshold(img, dnImg, CurrentCamera.UpThreshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

                                //if (_needToSave)
                                //{
                                //    var dir = Directory.CreateDirectory($"{Environment.CurrentDirectory}\\ResultImages").FullName;
                                //    var path = Path.Combine(dir, $"result_{CurrentCamera.ID}_Up.png");
                                //    var path2 = Path.Combine(dir, $"result_{CurrentCamera.ID}_Dn.png");
                                //    upImg.ToBitmap().Save(path, System.Drawing.Imaging.ImageFormat.Png);
                                //    dnImg.ToBitmap().Save(path2, System.Drawing.Imaging.ImageFormat.Png);
                                //    _needToSave = false;
                                //}
                                (Image<Bgr, byte> img2, IOrderedEnumerable<DefectProperties> defects) = ImageProcessing.AnalyzeDefects(upImg, dnImg,
                                                                                                      CurrentCamera.WidthThreshold,
                                                                                                      CurrentCamera.HeightThreshold,
                                                                                                      CurrentCamera.WidthDescrete,
                                                                                                      CurrentCamera.HeightDescrete,
                                                                                                      _strobe);

                                foreach (DefectProperties defect in defects)
                                {
                                    defect.X += Shift;
                                }
                               // _resImage = img2.Clone();
                                _resImage = img.Convert<Bgr, byte>();

                                if (defects.Any()) _needToDrawDefects = true;
                                _defects = defects;
                            }
                            imgProcessingStopWatch.Stop();
                            _needToProcessImage = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = $"{ex.Message}";
                        Logger?.Error(msg);
                        FooterRepository.Text = msg;
                        ExecuteStopCamera();
                    }
                    finally
                    {
                        GC.Collect();
                    }
                }
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
            try
            {

                CurrentCamera.CameraInit();
                FooterRepository.Text = $"Initialized = {CurrentCamera.Initialized}";
                BaslerRepository.AllCamerasInitialized = BaslerRepository.BaslerCamerasCollection.All(c => c.Initialized);
            }
            catch (Exception ex)
            {
                string msg = $"{ex.Message}";
                Logger?.Error(msg);
                FooterRepository.Text = msg;
                ExecuteStopCamera();
            }

        }

        private void ExecuteStartGrab()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                if (!_drawingTimer.IsEnabled)
                {
                    _drawingTimer.Start();
                }
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
            BaslerRepository.AllCamerasInitialized = false;
            if (_drawingTimer.IsEnabled)
            {
                _drawingTimer.Stop();
                BaslerRepository.TotalCount = 0;
                BenchmarkRepository.ImageProcessingSpeedCounter = 0;
                BenchmarkRepository.TempQueueCount = 0;
            }     
            CurrentCamera.StopAndKill();
            FooterRepository.Text = $"Stopped = true";
        }

        public override void Destroy()
        {
            CurrentCamera.CameraImageEvent -= ImageGrabbed;
            ApplicationCommands.StartAllSensors.UnregisterCommand(StartGrab);
            ApplicationCommands.Calibrate.UnregisterCommand(Calibrate);
            ApplicationCommands.SaveDefectsAndCrearTable.UnregisterCommand(ClearDefects);
            ApplicationCommands.InitAllSensors.UnregisterCommand(Init);
            ApplicationCommands.StopAllSensors.UnregisterCommand(StopCamera);
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
            //deltas = CalibrateService.DefaultCalibration(CurrentCamera.P, _width);

            tempImage = new Image<Gray, byte>(_width, 1000);
            img = new Image<Gray, byte>(_width, 1000);
        }
        #endregion
    }
}
