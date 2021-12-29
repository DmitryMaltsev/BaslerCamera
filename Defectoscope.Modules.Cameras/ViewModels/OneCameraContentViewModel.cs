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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
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
        private Stopwatch defectsProcessingStopWatch = new();
        private Image<Gray, byte> img;
        private Image<Gray, byte> tempImage;
        private byte[,,] imgData;
        private bool _needToSave = true;
        private bool _currentRawImage;
        private bool _currentVisualAnalizeIsActive;
        private bool _needIncreaseExposureTime = false;
        private int _cnt;
        private int _currentHeight;
        private List<byte> filteredLines;
        private double result;
        private byte _addBrightness = 0;
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

        private long _exposureTime;
        public long ExposureTime
        {
            get { return _exposureTime; }
            set { SetProperty(ref _exposureTime, value); }
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

        private DelegateCommand _changeMaterialCalibrationCommand;
        public DelegateCommand ChangeMaterialCalibrationCommand =>
            _changeMaterialCalibrationCommand ?? (_changeMaterialCalibrationCommand = new DelegateCommand(ExecuteChangeMaterialCalibrationCommand));

        private DelegateCommand _autoExpositionCommand;
        public DelegateCommand AutoExpositionCommand =>
            _autoExpositionCommand ?? (_autoExpositionCommand = new DelegateCommand(ExecuteAutoExpositionCommand));

        private DelegateCommand _findBounsIndexesCommand;
        public DelegateCommand FindBoundsIndexesCommand =>
            _findBounsIndexesCommand ?? (_findBounsIndexesCommand = new DelegateCommand(ExecuteFindBoundsIndexes));

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
            ApplicationCommands.CheckCamerasOverLay.RegisterCommand(CamerasOverlayCommand);
            ApplicationCommands.ChangeMaterialCalibration.RegisterCommand(ChangeMaterialCalibrationCommand);
            ApplicationCommands.AutoExposition.RegisterCommand(AutoExpositionCommand);
            ApplicationCommands.FindBoundsIndexes.RegisterCommand(FindBoundsIndexesCommand);
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
            filteredLines = new List<byte>();
            var uriSource = new Uri(@"/Defectoscope.Modules.Cameras;component/Images/ImageSurce_cam.png", UriKind.Relative);
            ImageSource = new BitmapImage(uriSource);
            _videoBuffer = new();
            _concurentVideoBuffer = new();
            _imageDataBuffer = new();

            _processVideoWork = new Task(() => ProceesBuffersAction());
            _processVideoWork.Start();

            _processImageTask = new Task(() => ProcessImageAction());
            _processImageTask.Start();

            _drawingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _drawingTimer.Tick += _drawingTimer_Tick;
            _drawingTimer.Start();
        }

        private void _drawingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                BaslerRepository.CurrentCamera.CurrentAverage = result;
                if (_resImage != null)
                {
                    Bitmap bmp = _resImage.ToBitmap();
                    bmp = _resImage.ToBitmap();
                    ImageSource = MathService.BitmapToImageSource(bmp);
                    bmp.Dispose();
                }
                if (_defects != null && _needToDrawDefects)
                {
                    DefectRepository.DefectsCollection.AddRange(_defects);

                    if (DefectRepository.DefectsCollection.Count > 500_000)
                    {
                        DefectRepository.DefectsCollection.Clear();
                    }
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
                _currentVisualAnalizeIsActive = DefectRepository.VisualAnalizeIsActive;
                BaslerRepository.TotalCount = _concurentVideoBuffer.Count;
                BenchmarkRepository.ImageProcessingSpeedCounter = imgProcessingStopWatch.ElapsedTicks / 10_000d;
                BenchmarkRepository.DefectsProcessingTimer = defectsProcessingStopWatch.ElapsedTicks / 10_000d;
                BenchmarkRepository.TempQueueCount = _imageDataBuffer.Count;
                if (CurrentCamera.ID == "Левая камера")
                {
                    BenchmarkRepository.LeftStrobe = _strobe;
                    if (_strobe > 500_000) _strobe = 0;
                }
                else
                 if (CurrentCamera.ID == "Центральная камера")
                {
                    BenchmarkRepository.CenterStrobe = _strobe;
                    if (_strobe > 500_000) _strobe = 0;
                }
                else
                     if (CurrentCamera.ID == "Правая камера")
                {
                    BenchmarkRepository.RightStrobe = _strobe;
                    if (_strobe > 500_000) _strobe = 0;
                }

            }
            catch (Exception ex)
            {
                string msg = $"{ex.Message} {ex.InnerException} {ex.StackTrace}";
                Logger?.Error(msg);
                FooterRepository.Text = msg;
                ExecuteStopCamera();

            }
            // Thread.Sleep(300);
        }

        private void ImageGrabbed(object sender, BufferData e)
        {
            if (CurrentCamera.Deltas == null)
            {
                FooterRepository.Text = "Cameras aren't calibrated";
                return;
            }
            if (!CurrentCamera.CalibrationMode && !CurrentCamera.FindBoundsMode)
            {
                _concurentVideoBuffer.Enqueue(e);
                // _strobe+= _concurentVideoBuffer.Count;
            }
            else
            if (CurrentCamera.CalibrationMode)
            {
                PerformCalibration(e);
                CurrentCamera.CalibrationMode = false;
            }
            else
            if (CurrentCamera.FindBoundsMode)
            {
                FooterRepository.Text = "Смотрим индексы";
                FindBoundsIndexes(e);
            }
            //Сохранеяем XML с обработанными данными
            if (_filterMode)
            {
                //List<byte> lines = e.Data.ToList();
                //filteredLines.AddRange(lines);
                //if (filteredLines.Count > 500_000)
                //{
                //    List<byte> newLine = new();
                //    string path = Path.Combine(SettingsDir, $"{_currentCamera.ID}_filterData.xml");
                //    for (int i = 0; i < filteredLines.Count; i++)
                //    {
                //        byte k = UsingCalibrationDeltas(filteredLines[i], i % 6144);
                //        newLine.Add(k);
                //    }
                //    List<List<byte>> resultList = newLine.
                //           Select((x, i) => new { Index = i, Value = x })
                //           .GroupBy(x => x.Index / 6144)
                //           .Select(x => x.Select(v => v.Value).ToList())
                //           .ToList();
                //    XmlService.Write(path, resultList);     
                //}
                try
                {
                    List<List<byte>> lines = e.Data.SplitByCount(e.Width).ToList();
                    List<byte> line = lines[0];
                    List<byte> filteredLine = new();
                    List<byte> multipleFilteredLine = new();
                    string path = Path.Combine("PointsData", $"{_currentCamera.ID}_filterData.xml");
                    string multiplePath = Path.Combine("PointsData", $"{_currentCamera.ID}_multipleFilteredData.xml");
                    // XmlService.
                    for (int i = 0; i < line.Count; i++)
                    {
                        byte currentByte = UsingCalibrationDeltas(line[i], i);
                        filteredLine.Add(currentByte);
                        byte currentByte2 = UsingMultiCalibrationDeltas(line[i], i);
                        multipleFilteredLine.Add(currentByte2);
                    }
                    XmlService.Write(path, filteredLine);
                    XmlService.Write(multiplePath, multipleFilteredLine);
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
            //Сохраняем XML  с сырыми данными
            if (_rawMode)
            {
                try
                {
                    List<List<byte>> lines = e.Data.SplitByCount(e.Width).ToList();
                    List<byte> line = lines[0];
                    string path = Path.Combine("PointsData", $"{_currentCamera.ID}_RawData.xml");
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
            CurrentCamera.Deltas = CalibrateService.CalibrateRaw(lines[index].ToArray());
            CurrentCamera.MultipleDeltas = CalibrateService.CalibrateMultiRaw(lines[index].ToArray());
            // CurrentCamera.Deltas = CalibrateService.CalibrateMultyRaw(lines[index].ToArray());
        }
        /// <summary>
        /// Находим границы для увеличения экспозиции
        /// </summary>
        /// <param name="e"></param>
        private void FindBoundsIndexes(BufferData e)
        {
            CurrentCamera.FindBoundsMode = false;
            List<List<byte>> lines = e.Data.SplitByCount(e.Width).ToList();
            byte currentByte;
            if (CurrentCamera.ID == "Левая камера")
            {
                for (int i = 0; i < lines[0].Count; i++)
                {
                    currentByte = UsingMultiCalibrationDeltas(lines[0][i], i);
                    if (currentByte < 100)
                    {
                        CurrentCamera.LeftBoundIndex = i;
                        break;
                    }
                }
            }

            if (CurrentCamera.ID == "Правая камера")
            {
                for (int i = lines[0].Count - 1; i > 0; i--)
                {
                    currentByte = UsingMultiCalibrationDeltas(lines[0][i], i);
                    if (currentByte < 100)
                    {
                        CurrentCamera.RightBoundIndex = i - 1;
                        break;
                    }
                }
                //  CorrectCalibratedPoints(lines[0], CurrentCamera.LeftBoundIndex, CurrentCamera.RightBoundIndex, 1700, 2000, 2);
                //  CorrectCalibratedPoints(lines[0], 1500, 1700, 1700, 2000,2);
            }
        }
        private void CorrectCalibratedPoints(List<byte> line, int leftBound, int rightBound, int leftChangePoint, int rightChangePoint, int cameraIndex)
        {
            List<byte> filteredLine = line;
            double average = 0;
            double res;
            for (int x = leftBound; x < rightBound; x++)
            {
                filteredLine[x] = UsingMultiCalibrationDeltas(filteredLine[x], x);
                average += filteredLine[x];
            }
            res = average / (rightBound - leftBound);
            for (int i = leftChangePoint; i < rightChangePoint; i++)
            {
                BaslerRepository.CurrentMaterial.CameraDeltaList[cameraIndex].MultipleDeltas[i] = res / line[i];
                BaslerRepository.CurrentCamera.MultipleDeltas[i] = BaslerRepository.CurrentMaterial.CameraDeltaList[0].MultipleDeltas[i];
            }
        }

        private async void ProceesBuffersAction()
        {
            while (true)
            {
                int count = _concurentVideoBuffer.Count;
                if (count > _currentHeight)
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
                            if (_cnt == _currentHeight)
                            {
                                byte[,,] data3Darray = new byte[_currentHeight, _width, 1];
                                Buffer.BlockCopy(tempImage.Data, 0, data3Darray, 0, tempImage.Data.Length);
                                _imageDataBuffer.Enqueue(data3Darray);
                                if (_strobe > 500_000) _strobe = 0;
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
                    if (_imageDataBuffer.TryDequeue(out byte[,,] dataBuffer))
                    {
                        _imageDataBuffer.Clear();
                        imgProcessingStopWatch.Restart();

                        img.Data = dataBuffer;
                        if (!_currentRawImage)
                        {
                            if (DefectRepository.KoefMultiplication)
                            {
                                for (int y = 0; y < img.Height; y++)
                                {
                                    for (int x = 0; x < _width; x++)
                                    {

                                        img.Data[y, x, 0] = UsingMultiCalibrationDeltas(img.Data[y, x, 0], x);
                                    }
                                }
                            }
                            else
                            {
                                for (int y = 0; y < img.Height; y++)
                                {
                                    for (int x = 0; x < _width; x++)
                                    {

                                        img.Data[y, x, 0] = UsingCalibrationDeltas(img.Data[y, x, 0], x);
                                    }
                                }
                            }

                        }
                        imgProcessingStopWatch.Stop();
                        defectsProcessingStopWatch.Restart();
                        using (Image<Gray, byte> upImg = img.CopyBlank())
                        using (Image<Gray, byte> dnImg = img.CopyBlank())
                        {
                            CvInvoke.Threshold(img, upImg, CurrentCamera.DownThreshold, 255, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
                            CvInvoke.Threshold(img, dnImg, CurrentCamera.UpThreshold, 255, Emgu.CV.CvEnum.ThresholdType.Binary);

                            (Image<Bgr, byte> img2, IOrderedEnumerable<DefectProperties> defects) = ImageProcessing.AnalyzeDefects(upImg, dnImg,
                                                                                                  CurrentCamera.WidthThreshold,
                                                                                                  CurrentCamera.HeightThreshold,
                                                                                                  CurrentCamera.WidthDescrete,
                                                                                                  CurrentCamera.HeightDescrete,
                                                                                                  _strobe, Shift);
                            defectsProcessingStopWatch.Stop();
                            if (_needIncreaseExposureTime)
                            {
                                double average = 0;
                                for (int i = CurrentCamera.LeftBoundIndex; i < CurrentCamera.RightBoundIndex; i++)
                                {
                                    average += img.Data[0, i, 0];
                                }
                                result = average / (CurrentCamera.RightBoundIndex - CurrentCamera.LeftBoundIndex);
                                if (result < 127)
                                {
                                    CurrentCamera.IncreaseExposureTime();
                                }
                                else
                                {
                                    _needIncreaseExposureTime = false;
                                }
                                // CurrentCamera.IncreaseCameraExposureTime();
                            }

                            if (_currentVisualAnalizeIsActive)
                            {
                                _resImage = img2; //img2.Clone();
                            }
                            else
                            {
                                _resImage = img.Convert<Bgr, byte>();
                            }
                            if (defects.Any())
                            {
                                _needToDrawDefects = true;
                                if (DefectRepository.CreateImages)
                                {
                                    ImageProcessing.DrawDefects(img2, CurrentCamera.ID);
                                }
                            }

                            _defects = defects;
                        }
                        _needToProcessImage = false;
                        GC.Collect();
                    }
                }
            }
        }

        /// <summary>
        /// Калибровка умножением
        /// </summary>
        /// <param name="currentByte"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private byte UsingMultiCalibrationDeltas(byte currentByte, int i)
        {
            if ((currentByte * CurrentCamera.MultipleDeltas[i] + _addBrightness) >= 255)
            {
                currentByte = 255;
            }
            else
                currentByte = (byte)(currentByte * CurrentCamera.MultipleDeltas[i] + _addBrightness);
            return currentByte;
        }
        /// <summary>
        /// Калибровка сложением
        /// </summary>
        /// <param name="currentByte"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private byte UsingCalibrationDeltas(byte currentByte, int i)
        {
            if ((currentByte + CurrentCamera.Deltas[i]) >= 255)
            {
                currentByte = 255;
            }
            else
                if (currentByte + CurrentCamera.Deltas[i] <= 0)
            {
                currentByte = 0;
            }
            else
            {
                currentByte = (byte)((sbyte)currentByte + CurrentCamera.Deltas[i]);
            }
            return currentByte;
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
                //if (ChangeMaterialDeltas())
                //{
                if (!_drawingTimer.IsEnabled)
                {
                    _drawingTimer.Start();
                }
                CurrentCamera.Start();
                //}
            }
        }

        private void ExecuteChangeMaterialCalibrationCommand()
        {
            if (BaslerRepository.CurrentMaterial == null)
            {
                BaslerRepository.CurrentMaterial = BaslerRepository.MaterialModelCollection[0];
                FooterRepository.Text = $"Для калибровки используется {BaslerRepository.CurrentMaterial.MaterialName} материал";
            }
            Task<string> changeMaterialsDeltasTask = new Task<string>(() => ChangeMaterialDeltas());
            changeMaterialsDeltasTask.Start();
            FooterRepository.Text = changeMaterialsDeltasTask.Result;
        }

        private string ChangeMaterialDeltas()
        {
            string footerMessage = "";
            if (BaslerRepository.CurrentMaterial.CameraDeltaList != null && BaslerRepository.CurrentMaterial.CameraDeltaList.Count > 0)

            {
                for (int i = 0; i < BaslerRepository.CurrentMaterial.CameraDeltaList.Count; i++)
                {
                    if (CurrentCamera.ID == BaslerRepository.CurrentMaterial.CameraDeltaList[i].CameraId)
                    {
                        CurrentCamera.Deltas = BaslerRepository.CurrentMaterial.CameraDeltaList[i].Deltas;
                        CurrentCamera.MultipleDeltas = BaslerRepository.CurrentMaterial.CameraDeltaList[i].MultipleDeltas;
                        CurrentCamera.UpThreshold = BaslerRepository.CurrentMaterial.CameraDeltaList[i].UpThreshhold;
                        CurrentCamera.DownThreshold = BaslerRepository.CurrentMaterial.CameraDeltaList[i].DownThreshhold;
                    }
                }
                footerMessage = $"Для калибровки используется {BaslerRepository.CurrentMaterial.MaterialName} материал";
                return footerMessage;
            }
            else
            {
                footerMessage = $"{BaslerRepository.CurrentMaterial.MaterialName} не откалиброван";
                return footerMessage;
            }
        }

        private void ExecuteCalibrate()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                CurrentCamera.CalibrationMode = true;
                CurrentCamera.OneShotForCalibration();
                FooterRepository.Text = "Нажмите стоп для калибровки камер";
            }
            else
            {
                FooterRepository.Text = "Инициализируйте камеры перед калибровкой";
            }
        }

        /// <summary>
        /// Находим границы элементов
        /// </summary>
        void ExecuteFindBoundsIndexes()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                CurrentCamera.FindBoundsMode = true;
                CurrentCamera.OneShotForCalibration();
                FooterRepository.Text = "Данные для индексов";
            }
        }

        /// <summary>
        /// Увеличиваем экспозицию после того, как положили материал
        /// </summary>
        void ExecuteAutoExpositionCommand()
        {
            _needIncreaseExposureTime = true;
            ExecuteStartGrab();
        }

        private void ExecuteCamerasOverlayCommand()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                //   CurrentCamera.CalibrationMode = true;
                ExecuteStartGrab();
                _overlayMode = true;
                FooterRepository.Text = "Наложение камер определено";
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
                FooterRepository.Text = "Сырые данные сохранены";
            }
        }

        void ExecuteTakeFilteredData()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                //ExecuteStartGrab();
                CurrentCamera.OneShotForCalibration();
                _filterMode = true;
                FooterRepository.Text = "Отфильтрованные данные сохранены";
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
            ApplicationCommands.ChangeMaterialCalibration.UnregisterCommand(ChangeMaterialCalibrationCommand);
            ApplicationCommands.StopAllSensors.UnregisterCommand(StopCamera);
            base.Destroy();
        }
        #endregion
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        private void OnNavigatedTo()
        {
            CurrentCamera.CameraImageEvent += ImageGrabbed;
            //_width = (int)(CurrentCamera.RightBorder - CurrentCamera.LeftBorder);
            _width = 6144;
            //deltas = CalibrateService.DefaultCalibration(CurrentCamera.P, _width);
            _currentHeight = 500;
            tempImage = new Image<Gray, byte>(_width, _currentHeight);
            img = new Image<Gray, byte>(_width, _currentHeight);
        }
    }
}
