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
using System.IO;
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
        public string SettingsDir => Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Settings").FullName;
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
        private bool _rawMode = false;
        private bool _filterMode = false;
        private bool _overlayMode = false;
        private Image<Bgr, byte> _resImage;
        private IOrderedEnumerable<DefectProperties> _defects;
        private DispatcherTimer _drawingTimer;
        private Stopwatch imgProcessingStopWatch = new();
        private Image<Gray, byte> img;
        private Image<Gray, byte> tempImage;
        private byte[,,] imgData;
        private bool _needToSave = true;
        private bool _currentRawImage;
        private bool _currentVisualAnalizeIsActive;
        private int _cnt;
        #endregion

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

        #region Delegate Commands
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

        private DelegateCommand _takeRawData;
        public DelegateCommand TakeRawData =>
            _takeRawData ?? (_takeRawData = new DelegateCommand(ExecuteTakeRawData));

        private DelegateCommand _takeFilteredData;
        public DelegateCommand TakeFilteredData =>
            _takeFilteredData ?? (_takeFilteredData = new DelegateCommand(ExecuteTakeFilteredData));

        private DelegateCommand _camerasOverlayCommand;
     

        public DelegateCommand CamerasOverlayCommand =>
            _camerasOverlayCommand ?? (_camerasOverlayCommand = new DelegateCommand(ExecuteCamerasOverlayCommand));

        //private DelegateCommand _changeMaterialDeltasCommand;
        //public DelegateCommand ChangeMaterialDeltasCommand =>
        //    _changeMaterialDeltasCommand ?? (_changeMaterialDeltasCommand = new DelegateCommand(ExecuteChangeMaterialDeltas));
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
        public INonControlZonesRepository NonControlZonesRepository { get; }
        public float Shift { get; set; }
        #endregion

        public OneCameraContentViewModel(IRegionManager regionManager, IApplicationCommands applicationCommands,
                                         IImageProcessingService imageProcessing, IDefectRepository defectRepository,
                                         IMathService mathService, IXmlService xmlService, IBaslerRepository baslerRepository,
                                         ILogger logger, IFooterRepository footerRepository, IBenchmarkRepository benchmarkRepository,
                                         ICalibrateService calibrateService, INonControlZonesRepository nonControlZonesRepository) : base(regionManager)
        {
            ApplicationCommands = applicationCommands;
            ApplicationCommands.Calibrate.RegisterCommand(Calibrate);
            ApplicationCommands.StartAllSensors.RegisterCommand(StartGrab);
            ApplicationCommands.SaveDefectsAndCrearTable.RegisterCommand(ClearDefects);
            ApplicationCommands.InitAllSensors.RegisterCommand(Init);
            ApplicationCommands.StopAllSensors.RegisterCommand(StopCamera);
            ApplicationCommands.CheckNoCalibrateAll.RegisterCommand(TakeRawData);
            ApplicationCommands.CheckFilterAll.RegisterCommand(TakeFilteredData);
            // ApplicationCommands.ChangeMaterialDeltas.RegisterCommand(ChangeMaterialDeltasCommand);
            ImageProcessing = imageProcessing;
            DefectRepository = defectRepository;
            MathService = mathService;
            XmlService = xmlService;
            BaslerRepository = baslerRepository;
            Logger = logger;
            FooterRepository = footerRepository;
            BenchmarkRepository = benchmarkRepository;
            CalibrateService = calibrateService;
            NonControlZonesRepository = nonControlZonesRepository;
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
                    if (_overlayMode)
                    {
                        if (CurrentCamera.ID == "Левая камера")
                        {
                            List<DefectProperties> leftDefects = _defects.ToList();
                            float currentShift = 6144 * CurrentCamera.WidthDescrete;
                            BaslerRepository.BaslerCamerasCollection[1].LeftBorder = currentShift + (currentShift - (float)leftDefects[0].X);
                            BaslerRepository.BaslerCamerasCollection[1].LeftBoundWidth = currentShift - (float)leftDefects[0].X;
                            _overlayMode = false;
                        }
                        if (CurrentCamera.ID == "Правая камера")
                        {
                            List<DefectProperties> rightDefects = _defects.ToList();
                            BaslerRepository.BaslerCamerasCollection[1].RightBorder = Shift - ((float)(rightDefects[0].X + rightDefects[0].Ширина) - Shift);
                            BaslerRepository.BaslerCamerasCollection[1].RightBoundWidth = (float)(rightDefects[0].X + rightDefects[0].Ширина) - Shift;
                            _overlayMode = false;
                        }
                    }
                }
                _currentRawImage = BenchmarkRepository.RawImage;
                _currentVisualAnalizeIsActive=DefectRepository.VisualAnalizeIsActive;
                BaslerRepository.TotalCount = _concurentVideoBuffer.Count;
                BenchmarkRepository.ImageProcessingSpeedCounter = imgProcessingStopWatch.ElapsedTicks / 10_000d;
                BenchmarkRepository.TempQueueCount = _imageDataBuffer.Count;
            }
            catch (Exception ex)
            {
                string msg = $"{ex.Message} {ex.InnerException} {ex.StackTrace}";
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

            if (_filterMode)
            {
                try
                {
                    List<List<byte>> lines = e.Data.SplitByCount(e.Width).ToList();
                    List<byte> line = lines[0];
                    List<byte> newLine = new();
                    string path = Path.Combine(SettingsDir, $"{_currentCamera.ID}_filterData.xml");
                    // XmlService.
                    for (int i = 0; i < line.Count; i++)
                    {
                        if ((line[i] + CurrentCamera.Deltas[i]) >= 255)
                        {
                            newLine.Add(255);
                        }
                        else
                        {
                            newLine.Add((byte)((sbyte)line[i] + CurrentCamera.Deltas[i]));
                        }

                    }
                    XmlService.Write(path, newLine);
                    _filterMode = false;
                    //         ExecuteStopCamera();
                }
                catch (Exception ex)
                {
                    string msg = $"{ex.Message}";
                    Logger?.Error(msg);
                    FooterRepository.Text = msg;
                    ExecuteStopCamera();
                }
            }
            if (_rawMode)
            {
                try
                {
                    List<List<byte>> lines = e.Data.SplitByCount(e.Width).ToList();
                    List<byte> line = lines[0];
                    string path = Path.Combine(SettingsDir, $"{_currentCamera.ID}_RawData.xml");
                    // XmlService
                    XmlService.Write(path, line);
                    _rawMode = false;
                    //   ExecuteStopCamera();
                }
                catch (Exception ex)
                {
                    string msg = $"{ex.Message}";
                    Logger?.Error(msg);
                    FooterRepository.Text = msg;
                    ExecuteStopCamera();
                }
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
                        bool res = _concurentVideoBuffer.TryDequeue(out BufferData bufferData);
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
                                if (_strobe > 10_000_000) _strobe = 0;
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
                        if (_imageDataBuffer.TryDequeue(out byte[,,] dataBuffer))
                        {
                            _imageDataBuffer.Clear();
                            imgProcessingStopWatch.Restart();

                            img.Data = dataBuffer;
                            if (!_currentRawImage)
                            {
                                for (int y = 0; y < img.Height; y++)
                                {
                                    for (int x = 0; x < _width; x++)
                                    {
                                        //if (NonControlZonesRepository.Obloys[0].MaximumX < x + CurrentCamera.StartPixelPoint * CurrentCamera.WidthDescrete
                                        //    && NonControlZonesRepository.Obloys[1].MinimumX > x + CurrentCamera.StartPixelPoint * CurrentCamera.WidthDescrete)
                                        //{
                                        //img.Data[y, x, 0] += CurrentCamera.Deltas[x];
                                        if ((byte)(img.Data[y, x, 0] + CurrentCamera.Deltas[x]) >= 255)
                                        {
                                            img.Data[y, x, 0] = 255;
                                        }
                                        else
                                        {
                                            img.Data[y, x, 0] = (byte)(img.Data[y, x, 0] + CurrentCamera.Deltas[x]);
                                        }
                                    }
                                    //else
                                    //{
                                    //    img.Data[y, x, 0] = 127;
                                    //}
                                }
                            }

                            using (Image<Gray, byte> upImg = img.CopyBlank())
                            using (Image<Gray, byte> dnImg = img.CopyBlank())
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
                                                                                                      _strobe, Shift);
                                //foreach (DefectProperties defect in defects)
                                //{
                                //    defect.X += Shift;
                                //}

                                //    List<DefectProperties> _filteredDefects = ImageProcessing.FilterDefects(defects.ToList());

                                if (_currentVisualAnalizeIsActive)
                                {
                                    _resImage = img2; //img2.Clone();
                                }
                                else
                                {
                                    _resImage = img.Convert<Bgr, byte>();
                                }
                                if (defects.Any())
                                    _needToDrawDefects = true;
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
            BaslerRepository.AllCamerasStarted = false;
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                if (ChangeMaterialDeltas())
                {
                    if (!_drawingTimer.IsEnabled)
                    {
                        _drawingTimer.Start();
                    }
                    CurrentCamera.Start();
                    BaslerRepository.AllCamerasStarted = BaslerRepository.BaslerCamerasCollection.All(p => p.GrabOver == false);
                }
            }
        }

        private bool ChangeMaterialDeltas()
        {
            if (BaslerRepository.CurrentMaterial.CameraDeltaList != null && BaslerRepository.CurrentMaterial.CameraDeltaList.Count > 0)
            {
                for (int i = 0; i < BaslerRepository.CurrentMaterial.CameraDeltaList.Count; i++)
                {
                    if (CurrentCamera.ID == BaslerRepository.CurrentMaterial.CameraDeltaList[i].CameraId)
                    {
                        CurrentCamera.Deltas = BaslerRepository.CurrentMaterial.CameraDeltaList[i].Deltas;
                    }
                }
                FooterRepository.Text = $"Для калибровки используется {BaslerRepository.CurrentMaterial.MaterialName} материал";
                return true;
            }
            else
            {
                FooterRepository.Text = $"{BaslerRepository.CurrentMaterial.MaterialName} не откалиброван";
                return false;
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
            else
            {
                FooterRepository.Text = "Инициализируйте камеры перед калибровкой";
            }
        }

        private void ExecuteCamerasOverlayCommand()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                //   CurrentCamera.CalibrationMode = true;
                ExecuteStartGrab();
                _overlayMode = true;
            }
        }

        void ExecuteTakeRawData()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                // CurrentCamera.CalibrationMode = true;
                CurrentCamera.OneShotForCalibration();
                _rawMode = true;
            }
        }

        void ExecuteTakeFilteredData()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                // CurrentCamera.CalibrationMode = true;
                CurrentCamera.OneShotForCalibration();
                _filterMode = true;
            }
        }



        private void ExecuteStopCamera()
        {

            if (CurrentCamera == null) return;
            CurrentCamera.Initialized = false;
            BaslerRepository.AllCamerasInitialized = false;
            BaslerRepository.AllCamerasStarted = false;
            if (_drawingTimer.IsEnabled)
            {
                _drawingTimer.Stop();
                BaslerRepository.TotalCount = 0;
                BenchmarkRepository.ImageProcessingSpeedCounter = 0;
                BenchmarkRepository.TempQueueCount = 0;
            }
            BaslerRepository.AllCamerasStarted = false;
            CurrentCamera.StopAndKill();
            BaslerRepository.AllCamerasStarted = BaslerRepository.BaslerCamerasCollection.All(p => p.GrabOver == false);
            FooterRepository.Text = $"Stopped = true";
        }

        public override void Destroy()
        {
            CurrentCamera.CameraImageEvent -= ImageGrabbed;
            ApplicationCommands.StartAllSensors.UnregisterCommand(StartGrab);
            ApplicationCommands.Calibrate.UnregisterCommand(Calibrate);
            ApplicationCommands.SaveDefectsAndCrearTable.UnregisterCommand(ClearDefects);
            ApplicationCommands.InitAllSensors.UnregisterCommand(Init);
            // ApplicationCommands.ChangeMaterialDeltas.UnregisterCommand(ChangeMaterialDeltasCommand);
            ApplicationCommands.StopAllSensors.UnregisterCommand(StopCamera);
            base.Destroy();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        private void OnNavigatedTo()
        {
            BaslerRepository.AllCamerasStarted = BaslerRepository.BaslerCamerasCollection.All(p => p.GrabOver == false);

            ApplicationCommands.CheckCamerasOverLay.RegisterCommand(CamerasOverlayCommand);
            CurrentCamera.CameraImageEvent += ImageGrabbed;
            //_width = (int)(CurrentCamera.RightBorder - CurrentCamera.LeftBorder);
            _width = 6144;
            //deltas = CalibrateService.DefaultCalibration(CurrentCamera.P, _width);

            tempImage = new Image<Gray, byte>(_width, 1000);
            img = new Image<Gray, byte>(_width, 1000);
        }
        #endregion
    }
}
