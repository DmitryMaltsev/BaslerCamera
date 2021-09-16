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
        Image<Gray, byte> img;
        private byte[] deltas;
        private Gray _white = new Gray(255);
        private Gray upThreshold;

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
        public ICalibrateService CalibrateService { get; }

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
            int index = lines.Count>=5?3:0;
            (CurrentCamera.P, deltas) = CalibrateService.Calibrate(lines[index].ToArray());
        }

        private  void ProcessImageAction()
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
            }
        }

        private void ProcessImage()
        {
            try
            {
                imgProcessingStopWatch.Restart();
                if (!BenchmarkRepository.RawImage)
                {
                    for (int y = 0; y < 1000; y++)
                    {
                        for (int x = 0; x < _width; x++)
                        {
                            img.Data[y, x, 0] += deltas[x];
                        }
                    }
                }
                var upImg = img.CopyBlank();

                CvInvoke.Threshold(img, upImg, CurrentCamera.DownThreshold, 255, Emgu.CV.CvEnum.ThresholdType.BinaryInv);

                var dnImg = img.CopyBlank();

                CvInvoke.Threshold(img, dnImg, CurrentCamera.UpThreshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

                (Image<Bgr, byte> img2, var defects) = ImageProcessing.AnalyzeDefects(upImg, dnImg,
                                                                                      CurrentCamera.WidthThreshold,
                                                                                      CurrentCamera.HeightThreshold,
                                                                                      CurrentCamera.WidthDescrete,
                                                                                      CurrentCamera.HeightDescrete,
                                                                                      _strobe);
                foreach (var defect in defects)
                {
                    defect.X += Shift;
                }

                //_resImage = img2.Clone();
                _resImage = img.Convert<Bgr, byte>();

                if (defects.Any()) _needToDrawDefects = true;
                _defects = defects;

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
            deltas = CalibrateService.DefaultCalibration(CurrentCamera.P, _width);
            img = new Image<Gray, byte>(_width, 1000);
        }
        #endregion
    }
}
