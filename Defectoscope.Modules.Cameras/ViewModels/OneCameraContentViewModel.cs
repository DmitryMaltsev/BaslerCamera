using Emgu.CV;
using Emgu.CV.Structure;

using Kogerent.Core;
using Kogerent.Logger;
using Kogerent.Services.Interfaces;

using LaserScan.Core.NetStandart.Models;

using OxyPlot;

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
using System.Windows.Media;
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
        private ConcurrentQueue<BufferData> _concurrentVideoBuffer;
        private ConcurrentQueue<BufferData> _concurrentEtalonPointsBuffer;
        private List<List<byte>> _rawPointsToRecord;
        private ConcurrentQueue<byte[,,]> _imageDataBuffer;
        private int _width;
        private int _strobe;
        private long _exposition;
        private Task _processVideoWork;
        private Task _processImageTask;
        private bool _needToProcessImage;
        private bool _overlayMode = false;
        private Image<Bgr, byte> _resImage;
        private IOrderedEnumerable<DefectProperties> _defects;
        private DispatcherTimer _drawingTimer;
        private Stopwatch imgProcessingStopWatch = new();
        private Stopwatch defectsProcessingStopWatch = new();
        private Stopwatch calibrationStopWatch = new();
        private Stopwatch _filesRecordingStopWatch = new();
        private int _countRawsToRecord = 0;
        private Image<Gray, byte> img;
        private Image<Gray, byte> _tempImage;
        private byte[,,] _currentCameraBaseArray;
        private byte[,,] imgData;
        private bool _needToSave = true;
        private bool _currentRawImage;
        private bool _currentVisualAnalizeIsActive;
        private bool _needIncreaseExposureTime = false;
        private int _currentHeight;
        private int _height;
        private double result;
        private int countArraysInSection;
        byte[,] _arrayToCalibrate;
        int _currentArrayCount = 0;
        int _arrayHeightToAnalize;
        private ImageGrabbedEnumModes imageGrabbedEnumModes = ImageGrabbedEnumModes.InActive;

        #endregion

        #region Rising properties
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

        private Stretch _imageStretch = Stretch.None;
        public Stretch ImageStretch
        {
            get { return _imageStretch; }
            set { SetProperty(ref _imageStretch, value); }
        }
        #endregion

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

        private DelegateCommand _startRecordRawData;
        public DelegateCommand StartRecordRawData =>
            _startRecordRawData ?? (_startRecordRawData = new DelegateCommand(ExecuteStartRecordRawData));

        private DelegateCommand _stopRecordRawData;
        public DelegateCommand StopRecordRawData =>
            _stopRecordRawData ?? (_stopRecordRawData = new DelegateCommand(ExecuteStopRecordRawData));


        void ExecuteCommandName()
        {

        }


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
        public IFilesRepository FilesRepository { get; }
        public ICalibrateService CalibrateService { get; }
        public INonControlZonesRepository NonControlZonesRepository { get; }
        public float Shift { get; set; }
        #endregion

        public OneCameraContentViewModel(IRegionManager regionManager, IApplicationCommands applicationCommands,
                                         IImageProcessingService imageProcessing, IDefectRepository defectRepository,
                                         IMathService mathService, IXmlService xmlService, IBaslerRepository baslerRepository,
                                         ILogger logger, IFooterRepository footerRepository, IBenchmarkRepository benchmarkRepository, IFilesRepository filesRepository,
                                         ICalibrateService calibrateService, INonControlZonesRepository nonControlZonesRepository) : base(regionManager)
        {
            ApplicationCommands = applicationCommands;
            ApplicationCommands.Calibrate.RegisterCommand(Calibrate);
            ApplicationCommands.StartAllSensors.RegisterCommand(StartGrab);
            ApplicationCommands.SaveDefectsAndCrearTable.RegisterCommand(ClearDefects);
            ApplicationCommands.InitAllSensors.RegisterCommand(Init);
            ApplicationCommands.StopAllSensors.RegisterCommand(StopCamera);
            ApplicationCommands.StartRecordRawData.RegisterCommand(StartRecordRawData);
            ApplicationCommands.CheckFilterAll.RegisterCommand(TakeFilteredData);
            ApplicationCommands.CheckCamerasOverLay.RegisterCommand(CamerasOverlayCommand);
            ApplicationCommands.ChangeMaterialCalibration.RegisterCommand(ChangeMaterialCalibrationCommand);
            ApplicationCommands.AutoExposition.RegisterCommand(AutoExpositionCommand);
            ApplicationCommands.FindBoundsIndexes.RegisterCommand(FindBoundsIndexesCommand);
            ApplicationCommands.StopRecordRawData.RegisterCommand(StopRecordRawData);
            ImageProcessing = imageProcessing;
            DefectRepository = defectRepository;
            MathService = mathService;
            XmlService = xmlService;
            BaslerRepository = baslerRepository;
            Logger = logger;
            FooterRepository = footerRepository;
            BenchmarkRepository = benchmarkRepository;
            FilesRepository = filesRepository;
            CalibrateService = calibrateService;
            NonControlZonesRepository = nonControlZonesRepository;
            var uriSource = new Uri(@"/Defectoscope.Modules.Cameras;component/Images/ImageSurce_cam.png", UriKind.Relative);
            ImageSource = new BitmapImage(uriSource);
            _videoBuffer = new();
            _concurrentVideoBuffer = new();
            _concurrentEtalonPointsBuffer = new();
            _rawPointsToRecord = new();
            _imageDataBuffer = new();


        }

        private void _drawingTimer_Tick(object sender, EventArgs e)
        {

            try
            {
                BaslerRepository.CurrentCamera.CurrentAverage = result;
                if (_resImage != null)
                {
                    Bitmap bmp = _resImage.ToBitmap();
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
                            float currentShift = 6144 * BaslerRepository.BaslerCamerasCollection[0].WidthDescrete;
                            BaslerRepository.BaslerCamerasCollection[1].LeftBorder = currentShift + (currentShift - (float)leftDefects[0].X);
                            //      BaslerRepository.BaslerCamerasCollection[1].LeftBoundWidth = currentShift - (float)leftDefects[0].X;
                            _overlayMode = false;
                        }
                        if (CurrentCamera.ID == "Правая камера")
                        {
                            List<DefectProperties> rightDefects = _defects.ToList();
                            BaslerRepository.BaslerCamerasCollection[1].RightBorder = Shift - ((float)(rightDefects[0].X + rightDefects[0].Ширина) - Shift);
                            //       BaslerRepository.BaslerCamerasCollection[1].RightBoundWidth = (float)(rightDefects[0].X + rightDefects[0].Ширина) - Shift;
                            _overlayMode = false;
                        }
                    }
                }
                _currentRawImage = BenchmarkRepository.RawImage;
                _currentVisualAnalizeIsActive = DefectRepository.VisualAnalizeIsActive;
                BaslerRepository.TotalCount = _concurrentVideoBuffer.Count;
                BenchmarkRepository.ImageProcessingSpeedCounter = imgProcessingStopWatch.ElapsedTicks / 10_000d;
                BenchmarkRepository.DefectsProcessingTimer = defectsProcessingStopWatch.ElapsedTicks / 10_000d;
                BaslerRepository.CalibrationTimer = calibrationStopWatch.ElapsedTicks / 10_000d;
                FilesRepository.FilesRecordingTime[CurrentCamera.Index] = _filesRecordingStopWatch.ElapsedTicks / 10_000_000d;
                FilesRepository.FilesRawCount[CurrentCamera.Index] = _countRawsToRecord;
                BenchmarkRepository.TempQueueCount = _imageDataBuffer.Count;
                CurrentCamera.CameraStatisticsData.StrobesCount = _strobe;
                CurrentCamera.ExposureTime = _exposition;

                //       if (imageGrabbedEnumModes == ImageGrabbedEnumModes.RecievePoints) FooterRepository.Text = "Обычный режим работы";
                if (_strobe > 500_000) _strobe = 0;
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
            _concurrentVideoBuffer.Enqueue(e);
            #region switch case убрать
            //switch (imageGrabbedEnumModes)
            //{
            //    case ImageGrabbedEnumModes.RecievePoints:
            //        {
            //            _concurrentVideoBuffer.Enqueue(e);
            //        }
            //        break;
            //    case ImageGrabbedEnumModes.FindExpositionLevel:
            //        {
            //            _concurrentVideoBuffer.Enqueue(e);
            //        }
            //        break;
            //    case ImageGrabbedEnumModes.Calibrate:
            //        {
            //            //Старый метод калибровки
            //            if (CurrentCamera.CalibrationMode)
            //            {
            //                PerformCalibration(e);
            //                CurrentCamera.CalibrationMode = false;
            //            }
            //        }
            //        break;
            //    case ImageGrabbedEnumModes.CreateFilterXml:
            //        {
            //            try
            //            {
            //                // XmlService.
            //                List<List<byte>> buffer = e.Data.SplitByCount(e.Width).ToList();
            //                _rawPointsToRecord.AddRange(buffer);
            //                if (_rawPointsToRecord.Count >= 5_000)
            //                {
            //                    List<List<byte>> resultFilterOptions = new();
            //                    List<List<byte>> resultMultipleFilterOptions = new();
            //                    for (int xpointsNum = 0; xpointsNum < _rawPointsToRecord.Count; xpointsNum++)
            //                    {
            //                        List<byte> resultFilterOption = new();
            //                        List<byte> resultMultipleFilterOption = new();

            //                        for (int yPointsNum = 0; yPointsNum < _rawPointsToRecord[0].Count; yPointsNum++)
            //                        {
            //                            resultFilterOption.Add(UsingCalibrationDeltas(_rawPointsToRecord[xpointsNum][yPointsNum], yPointsNum));
            //                            resultMultipleFilterOption.Add(UsingMultiCalibrationDeltas(_rawPointsToRecord[xpointsNum][yPointsNum], yPointsNum));
            //                        }
            //                        resultFilterOptions.Add(resultFilterOption);
            //                        resultMultipleFilterOptions.Add(resultMultipleFilterOption);
            //                    }
            //                    string path = Path.Combine("PointsData", "Filter", $"{_currentCamera.ID}_raw_{DateTime.Now.ToString("HH.mm.ss")}.txt");
            //                    string filterPath = Path.Combine("PointsData", "Filter", $"{_currentCamera.ID}_filter_{DateTime.Now.ToString("HH.mm.ss")}.txt");
            //                    string multipleFilterPath = Path.Combine("PointsData", "Filter", $"{_currentCamera.ID}_multiplefilter_{DateTime.Now.ToString("HH.mm.ss")}.txt");
            //                    XmlService.WriteListText(_rawPointsToRecord, path);
            //                    XmlService.WriteListText(resultFilterOptions, filterPath);
            //                    XmlService.WriteListText(resultMultipleFilterOptions, multipleFilterPath);
            //                    _rawPointsToRecord = new List<List<byte>>();
            //                    imageGrabbedEnumModes = ImageGrabbedEnumModes.RecievePoints;
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                string msg = $"{ex.Message}";
            //                Logger?.Error(msg);
            //                FooterRepository.Text = msg;
            //                ExecuteStopCamera();
            //            }
            //        }
            //        break;
            //    case ImageGrabbedEnumModes.FindBounds:
            //        {
            //            FooterRepository.Text = "Смотрим индексы";
            //            FindBoundsIndexes(e);
            //        }
            //        break;
            //    case ImageGrabbedEnumModes.InActive:
            //        break;
            //    default:
            //        break; 
            // }
            #endregion

            //Сохранеяем XML с обработанными данными
        }

        private async void ProceesBuffersAction()
        {
            while (true)
            {

                int count = _concurrentVideoBuffer.Count;
                if (count > _height / countArraysInSection)
                {

                    for (int i = 0; i < count; i++)
                    {
                        bool res = _concurrentVideoBuffer.TryDequeue(out BufferData bufferData);
                        if (res && bufferData != default)
                        {
                            Buffer.BlockCopy(bufferData.Data, 0, _currentCameraBaseArray, _currentHeight * _width, bufferData.Data.Length);
                            _currentHeight += bufferData.Height;
                            _strobe += bufferData.Height;
                            bufferData.Dispose();
                            //GC.Collect();
                            if (_currentHeight == _height)
                            {
                                if (CurrentCamera.GraphPoints.Count == 0)
                                {
                                    //   List .GraphPoints.Clear();
                                    for (int k = 0; k < _width; k++)
                                    {
                                        CurrentCamera.GraphPoints.Add(new DataPoint(k, _currentCameraBaseArray[0, k, 0]));
                                    }
                                    CurrentCamera.GraphPointsQueue.Enqueue(CurrentCamera.GraphPoints);
                                }
                                CheckIfNeedOptions(_currentCameraBaseArray);
                                // Buffer.BlockCopy(_tempImage.Data, 0, _currentCameraBaseArray, 0, _tempImage.Data.Length);
                                _imageDataBuffer.Enqueue(_currentCameraBaseArray);
                                _currentCameraBaseArray = new byte[_height, _width, 1];
                                if (_strobe > 500_000) _strobe = 0;
                                _currentHeight = 0;
                                //   _currentCameraBaseArray = new byte[_height, _width, 1];
                                _needToProcessImage = true;
                            }
                        }
                    }
                }

                await Task.Delay(1);
            }
        }

        #region Опции для калибровки и записи данных

        private void CheckIfNeedOptions(byte[,,] baseArray)
        {
            switch (imageGrabbedEnumModes)
            {
                case ImageGrabbedEnumModes.CreateEtalonPoints:
                    {
                        CreateEtalonPonts(_currentCameraBaseArray);
                    }
                    break;
                case ImageGrabbedEnumModes.RecordRawData:
                    {
                        RecordRawData(_currentCameraBaseArray);
                    }
                    break;
            }
            #region ChangeExpLevel
            //    case ImageGrabbedEnumModes.FindExpositionLevel:
            //        {
            //            int bufCount = _concurrentVideoBuffer.Count;
            //            int countArraysToAnalize = 5;
            //            if (bufCount > countArraysToAnalize / countArraysInSection)
            //            {
            //                int leftSideIndex = (int)(BaslerRepository.LeftBorder / BaslerRepository.BaslerCamerasCollection[0].WidthDescrete);
            //                int rightSideIndex = (int)(BaslerRepository.RightBorder / BaslerRepository.BaslerCamerasCollection[0].WidthDescrete);
            //                int _shift = (int)(Shift / CurrentCamera.WidthDescrete);
            //                if (leftSideIndex - _shift < 0)
            //                {
            //                    leftSideIndex = 0;
            //                }
            //                if (rightSideIndex - _shift > _width)
            //                {
            //                    rightSideIndex = _width;
            //                }
            //                else
            //                {
            //                    rightSideIndex -= _shift;
            //                }

            //                if (CalibrateService.NeedChangeExposition(_concurrentVideoBuffer, 5, _width,
            //                    leftSideIndex, rightSideIndex, 175, 185, out int changeExspositionValue) && CurrentCamera.ExposureTime < 40_000 && CurrentCamera.ExposureTime > 60)
            //                {
            //                    _exposition = CurrentCamera.ChangeExposureTime(changeExspositionValue);
            //                    //  _concurentVideoBuffer.Clear();
            //                }
            //                else
            //                {
            //                    imageGrabbedEnumModes = ImageGrabbedEnumModes.RecievePoints;
            //                    _strobe = 0;
            //                }
            //                // string path = Path.Combine(Directory.GetCurrentDirectory(), "PointsData", $"{_currentCamera.ID}_etalonData.xml");
            //                await Task.Delay(200);
            //            }
            //        }
            //        break; 
            #endregion
        }

        private void RecordRawData(byte[,,] baseArray)
        {
            //Сохраняем XML  с сырыми данными
            try
            {
                for (int i = 0; i < _height; i++)
                {
                    List<byte> bufferWidth = new List<byte>(baseArray[i, 0, 0]);
                    for (int j = 0; j < _width; j++)
                    {
                        bufferWidth.Add(baseArray[i, j, 0]);
                    }
                    _rawPointsToRecord.Add(bufferWidth);
                }
                _countRawsToRecord += _height;
                if (FilesRepository.IsRecordingRawData == false)
                {
                    string path = Path.Combine("PointsData", "Raw", $"{_currentCamera.ID}_raw_{DateTime.Now.ToString("HH.mm.ss")}.txt");
                    XmlService.WriteListText(_rawPointsToRecord, path);
                    imageGrabbedEnumModes = ImageGrabbedEnumModes.RecievePoints;
                    _rawPointsToRecord = new List<List<byte>>();
                    //   ExecuteStopCamera();
                    _filesRecordingStopWatch.Stop();
                }
            }
            catch (Exception ex)
            {
                string msg = $"{ex.Message}";
                Logger?.Error(msg);
                FooterRepository.Text = msg;
                ExecuteStopCamera();
            }
        }

        private void CreateEtalonPonts(byte[,,] baseArray)
        {
            //   int bufCount = _concurrentVideoBuffer.Count;
            Buffer.BlockCopy(baseArray, 0, _arrayToCalibrate, _currentArrayCount * _width, _height * _width);
            _currentArrayCount += _height;
            if (_currentArrayCount >= _arrayHeightToAnalize)
            {
                calibrationStopWatch.Restart();
                List<byte> calibratedPointsList = CalibrateService.CreateAverageElementsForCalibration(_arrayToCalibrate, countArraysInSection, _width);
                CurrentCamera.Deltas = CalibrateService.CalibrateRaw(calibratedPointsList.ToArray());
                CurrentCamera.MultipleDeltas = CalibrateService.CalibrateMultiRaw(calibratedPointsList.ToArray());
                imageGrabbedEnumModes = ImageGrabbedEnumModes.RecievePoints;
                _strobe = 0;
                calibrationStopWatch.Stop();
                _arrayToCalibrate = new byte[_arrayHeightToAnalize, _width];
                _currentArrayCount = 0;
                FooterRepository.Text = $"Калибровочные данные созданы, но будут сохранены ";
            }
        }
        #endregion

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

                        #region Analize defects
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

                            if (_currentVisualAnalizeIsActive)
                            {
                                ImageProcessing.DrawBoundsWhereDefectsCanDefined((int)(BaslerRepository.LeftBorder / CurrentCamera.WidthDescrete),
                                                  (int)(BaslerRepository.RightBorder / CurrentCamera.WidthDescrete), img2, CurrentCamera.ID);
                                _resImage = img2; //img2.Clone();
                            }
                            else
                            {

                                _resImage = img.Convert<Bgr, byte>();
                                ImageProcessing.DrawBoundsWhereDefectsCanDefined((int)(BaslerRepository.LeftBorder / CurrentCamera.WidthDescrete),
                                                (int)(BaslerRepository.RightBorder / CurrentCamera.WidthDescrete), _resImage, CurrentCamera.ID);
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
                        #endregion
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
            if ((currentByte * CurrentCamera.MultipleDeltas[i]) >= 255)
            {
                currentByte = 255;
            }
            else
                currentByte = (byte)(currentByte * CurrentCamera.MultipleDeltas[i]);
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
            imageGrabbedEnumModes = ImageGrabbedEnumModes.RecievePoints;
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
                //  CurrentCamera.ChangeExposureTime(0);
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
        /// <summary>
        /// Запуск камер для обычной работы
        /// </summary>
        private void ExecuteStartGrab()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                if (!_drawingTimer.IsEnabled)
                {
                    _drawingTimer.Start();
                }
                imageGrabbedEnumModes = ImageGrabbedEnumModes.RecievePoints;

                if (!CurrentCamera.IsGrabbing())
                {
                    CurrentCamera.Start();
                }
                FooterRepository.Text = "Обычный режим работы";
            }
        }


        private void ExecuteChangeMaterialCalibrationCommand()
        {
            if (BaslerRepository.CurrentMaterial == null)
            {
                BaslerRepository.CurrentMaterial = BaslerRepository.MaterialModelCollection[0];
                FooterRepository.Text = $"Для калибровки используется {BaslerRepository.CurrentMaterial.MaterialName} материал";
            }
            FooterRepository.Text = ChangeMaterialDeltas();
        }

        private string ChangeMaterialDeltas()
        {
            if (BaslerRepository.CurrentMaterial.CameraDeltaList != null && BaslerRepository.CurrentMaterial.CameraDeltaList.Count > 0)
            {
                CameraDelta materialCurrentCamera = BaslerRepository.CurrentMaterial.CameraDeltaList.Where(d => d.CameraId == CurrentCamera.ID).First();
                if (materialCurrentCamera != null)
                {
                    CurrentCamera.Deltas = materialCurrentCamera.Deltas;
                    CurrentCamera.MultipleDeltas = materialCurrentCamera.MultipleDeltas;
                    CurrentCamera.UpThreshold = materialCurrentCamera.UpThreshhold;
                    CurrentCamera.DownThreshold = materialCurrentCamera.DownThreshhold;
                    return $"Для калибровки используется {BaslerRepository.CurrentMaterial.MaterialName} материал";
                }
                return "";
            }
            else
            {
                return $"{BaslerRepository.CurrentMaterial.MaterialName} не откалиброван";
            }
        }

        /// <summary>
        /// Калибровка новым методом(Усреденение n-массивов точек) с последующим переходом в рабочее состояние
        /// </summary>
        private void ExecuteCalibrate()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {

                if (!_drawingTimer.IsEnabled)
                {
                    _drawingTimer.Start();
                }
                imageGrabbedEnumModes = ImageGrabbedEnumModes.CreateEtalonPoints;
                if (!CurrentCamera.IsGrabbing())
                {
                    CurrentCamera.Start();
                }
                FooterRepository.Text = "Создаем эталонные данные для калибровки";
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
                imageGrabbedEnumModes = ImageGrabbedEnumModes.FindBounds;
                CurrentCamera.OneShotForCalibration();
                FooterRepository.Text = "Данные для индексов";
            }
        }

        /// <summary>
        /// Увеличиваем экспозицию после того, как положили материал
        /// </summary>
        void ExecuteAutoExpositionCommand()
        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {

                if (!_drawingTimer.IsEnabled)
                {
                    _drawingTimer.Start();
                }
                imageGrabbedEnumModes = ImageGrabbedEnumModes.FindExpositionLevel;
                if (!CurrentCamera.IsGrabbing())
                {
                    CurrentCamera.Start();
                }
                FooterRepository.Text = "Находим уровень экспозиции для каждой камеры";
            }
        }

        /// <summary>
        /// Для проверки пересечения камер и отфильтровывания сдвоенных дефектов
        /// </summary>
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
        /// <summary>
        /// Команда начала записи сырых точек в файл
        /// Во время основной работы
        /// </summary>
        void ExecuteStartRecordRawData()
        {
            if (CurrentCamera == null) return;

            if (CurrentCamera.Initialized)
            {
                if (!CurrentCamera.IsGrabbing())
                {
                    CurrentCamera.Start();
                }
                _filesRecordingStopWatch.Restart();
                FilesRepository.FilesRawCount[CurrentCamera.Index] = 0;
                FilesRepository.FilesRecordingTime[CurrentCamera.Index] = 0;
                _countRawsToRecord = 0;
                imageGrabbedEnumModes = ImageGrabbedEnumModes.RecordRawData;
                FilesRepository.IsRecordingRawData = true;
                FilesRepository.IsRecordStopped = false;
                FooterRepository.Text = "Сохранение сырых данных";
            }
        }

        /// <summary>
        /// Команда конца записи сырых точек в файл
        /// Во время основной работы
        /// </summary>
        void ExecuteStopRecordRawData()
        {
            FilesRepository.IsRecordingRawData = false;
            FilesRepository.IsRecordStopped = true;
        }

        void ExecuteTakeFilteredData()

        {
            if (CurrentCamera == null) return;
            if (CurrentCamera.Initialized)
            {
                //ExecuteStartGrab();
                //CurrentCamera.OneShotForCalibration();
                imageGrabbedEnumModes = ImageGrabbedEnumModes.CreateFilterXml;
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
            imageGrabbedEnumModes = ImageGrabbedEnumModes.InActive;
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
            _height = 500;
            _tempImage = new Image<Gray, byte>(_width, _height);
            img = new Image<Gray, byte>(_width, _height);
            _currentCameraBaseArray = new byte[_height, _width, 1];
            _exposition = CurrentCamera.ExposureTime;
            countArraysInSection = CurrentCamera.CameraStatisticsData.StrobesHeight;
            _arrayHeightToAnalize = 1_000;
            _arrayToCalibrate = new byte[_arrayHeightToAnalize, _width];
            _processVideoWork = new Task(() => ProceesBuffersAction());
            _processVideoWork.Start();

            _processImageTask = new Task(() => ProcessImageAction());
            _processImageTask.Start();
            _drawingTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            _drawingTimer.Tick += _drawingTimer_Tick;
            _drawingTimer.Start();
        }
    }
}
