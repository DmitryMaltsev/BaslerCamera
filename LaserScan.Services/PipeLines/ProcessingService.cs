using Emgu.CV;
using Emgu.CV.Structure;
using Kogerent.Core;
using Kogerent.Logger;
using Kogerent.Services.Interfaces;
using MathNet.Numerics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Kogerent.Services.Implementation
{
    /// <summary>
    /// Обрабатывает данные со всех датчиков
    /// </summary>
    public class ProcessingService : IProcessingService, IDisposable
    {
        #region Private fields
        private DispatcherTimer _sqlTimer = new DispatcherTimer();
        private readonly Type _processingType = typeof(ProcessingService);
        private List<List<float>> floatMap = new();
        private readonly ISensorRepository _sensorRepository;
        private readonly ILogger _logger;
        private readonly INonControlZonesRepository _nonControlZonesRepository;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IDataService _sqlDataService;
        private readonly ISynchronizer _synchronizer;
        private SensorPair[] _sensorPairs;
        private List<IntXFloatYPoint>[] _lastProfiles;
        private Image<Gray, byte> _imgUp;
        private Image<Gray, byte> _imgDn;
        private readonly BlockingCollection<List<PointF>> _genericMap = new();
        private readonly List<PointF> _line = new();

        private int _genericBufferCount;
        private bool disposedValue;
        private List<ConcurrentBag<List<IntXFloatYPoint>>> _sumBuffer;
        private int _fillCount;
        private int _timesFilled;
        private float _discretizeStep;
        private int _imageWidth;
        private int _offset;
        private float _k;
        private float _b;

        private Gray _whiteColor = new Gray(255);
        private Gray _black = new Gray(0);

        private event EventHandler ThresholdReachedEvent;

        private event EventHandler SumBufferChanged;

        private event EventHandler DataBaseThreshold;

        private Task _dataBaseTask;
        bool needToWrite = false;
        #endregion

        #region Properties
        private float HeightDiscrete => 1000f / Height;

        private int Height => (int)((1000 / _synchronizer.TimerHz) / _sensorRepository.ConveyerSpeed);
        /// <summary>
        /// Пора добавлять дефекты в таблицу?
        /// </summary>
        public bool DefectsAnalyzed { get; set; } = false;

        /// <summary>
        /// Коллекция найденных дефектов
        /// </summary>
        public List<DefectProperties> Defects { get; set; }

        /// <summary>
        /// Картинка для вывода найденых дефектов
        /// </summary>
        public Bitmap Bmp { get; set; }

        /// <summary>
        /// Последний обобщенный профиль в буфере
        /// </summary>
        public List<IntXFloatYPoint> LastProfile
        {
            get
            {
                var lastProfile = _lastProfiles.SelectMany(p => p).ToList();
                //if (lastProfile == null || lastProfile.Count < 4) return lastProfile;
                //return _mathService.DiscretizeWithSubstitution(lastProfile, _discretizeStep, 0, _sensorPairs[^1].RightBorder);
                return lastProfile;
            }
        }

        /// <summary>
        /// Общее количество обработанных профилей
        /// </summary>
        public int GenericBufferCount => _genericBufferCount;

        /// <summary>
        /// Счетчик заполнения карты
        /// </summary>
        public int FillCount => _fillCount;
        #endregion

        public ProcessingService(ISensorRepository sensorRepository, ILogger logger,
                                 INonControlZonesRepository nonControlZonesRepository,
                                 IImageProcessingService imageProcessingService, IDataService sqlDataService,
                                 ISynchronizer synchronizer)
        {
            _sensorRepository = sensorRepository;
            _logger = logger;
            _nonControlZonesRepository = nonControlZonesRepository;
            _imageProcessingService = imageProcessingService;
            _sqlDataService = sqlDataService;
            _synchronizer = synchronizer;
            _sensorPairs = _sensorRepository.SensorPairs.ToArray();
            _sensorRepository.PipelineLimit = Height;
            _lastProfiles = new List<IntXFloatYPoint>[_sensorPairs.Length];
            _sumBuffer = new();
            //foreach (var sensor in _sensorRepository.Sensors)
            //{
            //    sensor.ProfileBuffer = new(Height);
            //    sensor.XyzwBuffer = new(Height);
            //}
            for (int i = 0; i < _sensorPairs.Length; i++)
            {
                SensorPair pair = _sensorPairs[i];
                _lastProfiles[i] = new List<IntXFloatYPoint>();
                pair.BottomSensor.ProfileBuffer.CollectionChanged += SensorQueueChanged;
                pair.TopSensor.ProfileBuffer.CollectionChanged += SensorQueueChanged;
                _discretizeStep = pair.BottomSensor.DiscretizeStep;
                pair.LeftBorder = (int)pair.BottomSensor.LeftBorder;
                pair.RightBorder = (int)pair.BottomSensor.RightBorder;
                _sumBuffer.Add(new ConcurrentBag<List<IntXFloatYPoint>>());
            }
            _imageWidth = _sensorPairs[^1].RightBorder - _sensorPairs[0].LeftBorder + 1;
            _imgUp = new Image<Gray, byte>(_imageWidth, Height);
            _imgDn = new Image<Gray, byte>(_imageWidth, Height);
            _offset = 0 - _sensorPairs[0].LeftBorder;

            ThresholdReachedEvent += ProcessingService_ThresholdReached;
            //DataBaseThreshold += WriteToDataBase;
            SumBufferChanged += ProcessingService_SumBufferChanged;
            SetCoef();

            _sqlTimer = new DispatcherTimer() { Interval = TimeSpan.FromMinutes(10) };
            _sqlTimer.Tick += SqlTimer_Tick;
            _sqlTimer.Start();

            _dataBaseTask = new Task(() => InsertDataToDb());
            _dataBaseTask.Start();
        }



        #region Methods
        private void SqlTimer_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                string[] tableNames = { DataBaseHelper.Table0Name, DataBaseHelper.Table1Name, DataBaseHelper.Table2Name, DataBaseHelper.Table3Name };
                _sqlDataService.Compress(tableNames);
            }).ConfigureAwait(false);
        }

        private void SetCoef()
        {
            foreach (var pair in _sensorPairs)
            {
                if (pair.Index == 0)
                {
                    double[] xs = new double[] { 200.68, 45.1 };
                    double[] ys = new double[] { -0.2, -0.1 };
                    Tuple<double, double> t = Fit.Line(xs, ys);
                    pair.K = (float)t.Item2;
                    pair.B = (float)t.Item1;
                }
                if (pair.Index == 1)
                {
                    double[] xs = new double[] { 200.68, 45.1 };
                    double[] ys = new double[] { -0.15, -0.15 };
                    Tuple<double, double> t = Fit.Line(xs, ys);
                    pair.K = (float)t.Item2;
                    pair.B = (float)t.Item1;
                }
                if (pair.Index == 2)
                {
                    double[] xs = new double[] { 200.68, 45.1 };
                    double[] ys = new double[] { -0.5, 0.2 };
                    Tuple<double, double> t = Fit.Line(xs, ys);
                    pair.K = (float)t.Item2;
                    pair.B = (float)t.Item1;
                }
                if (pair.Index == 3)
                {
                    double[] xs = new double[] { 200.68, 45.1 };
                    double[] ys = new double[] { -0.3, -0.15 };
                    Tuple<double, double> t = Fit.Line(xs, ys);
                    pair.K = (float)t.Item2;
                    pair.B = (float)t.Item1;
                }
                if (pair.Index == 4)
                {
                    double[] xs = new double[] { 200.68, 45.1 };
                    double[] ys = new double[] { -0.15, 0 };
                    Tuple<double, double> t = Fit.Line(xs, ys);
                    pair.K = (float)t.Item2;
                    pair.B = (float)t.Item1;
                }
            }

            //double offset = _k * 180 + _b; 
        }

        private void WriteToDataBase(object sender, EventArgs e)
        {
            _ = Task.Run(() =>
              {
                  IntXFloatYPoint[] points = LastProfile.ToArray();

                  List<List<IntXFloatYPoint>> chunks = new()
                  {
                      new List<IntXFloatYPoint>(),
                      new List<IntXFloatYPoint>(),
                      new List<IntXFloatYPoint>(),
                      new List<IntXFloatYPoint>()
                  };
                  foreach (var point in points)
                  {
                      if (point.X >= -500 && point.X < 0) chunks[0].Add(point);
                      if (point.X >= 0 && point.X < 500) chunks[1].Add(point);
                      if (point.X >= 500 && point.X < 1000) chunks[2].Add(point);
                      if (point.X >= 1000 && point.X < 1500) chunks[3].Add(point);
                  }

                  for (int i = 1; i < chunks.Count; i++)
                  {
                      if (chunks[i].Count > 0)
                          _sqlDataService.InsertData(chunks[i], $"PointsCollection{i}", "PirPoints");
                  }
              }).ConfigureAwait(false);

        }

        private void ProcessingService_SumBufferChanged(object sender, EventArgs e)
        {
            Task.Run(() => ProcessStep()).ConfigureAwait(false);
        }

        private void ProcessingService_ThresholdReached(object sender, EventArgs e)
        {
            Task.Run(() => ThresholdReached()).ConfigureAwait(false);
        }

        private void SensorQueueChanged(object sender, int e)
        {
            SumStep(e);
        }

        private int SumStep(int index)
        {
            if (_sensorPairs[index].TopSensor.ProfileBuffer.IsEmpty ||
                _sensorPairs[index].BottomSensor.ProfileBuffer.IsEmpty)
            {
                if (_sensorPairs[index].TopSensor.ProfileBuffer.Count >= 100)
                {
                    _sensorPairs[index].TopSensor.ProfileBuffer.Clear();
                }
                if (_sensorPairs[index].BottomSensor.ProfileBuffer.Count >= 100)
                {
                    _sensorPairs[index].BottomSensor.ProfileBuffer.Clear();
                }
                return -1;
            }

            if (_sensorPairs[index].TopSensor.ProfileBuffer.TryTake(out List<IntXFloatYPoint> up) &&
                _sensorPairs[index].BottomSensor.ProfileBuffer.TryTake(out List<IntXFloatYPoint> dn))
            {
                if (up != default && dn != default)
                {
                    List<IntXFloatYPoint> sum = new();
                    int leftBorder = Math.Max(up[0].X, dn[0].X);
                    int rightBorder = Math.Min(up[^1].X, dn[^1].X);
                    up.RemoveAll(point => point.X < leftBorder && point.X > rightBorder);
                    dn.RemoveAll(point => point.X < leftBorder && point.X > rightBorder);
                    int upIndex = 0; int dnIndex = 0;
                    var count = Math.Min(up.Count, dn.Count);
                    for (int x = leftBorder; x < _sensorPairs[index].RightBorder; x++)
                    {
                        IntXFloatYPoint pointUp = new(0, 0);
                        IntXFloatYPoint pointDn = new(0, 0);
                        for (int i = upIndex; i < count; i++)
                        {
                            if (up[i].X == x)
                            {
                                pointUp = up[i];
                                upIndex = i;
                                break;
                            }
                        }
                        for (int i = dnIndex; i < count; i++)
                        {
                            if (dn[i].X == x)
                            {
                                pointDn = dn[i];
                                dnIndex = i;
                                break;
                            }
                        }
                        bool emptyUp = pointUp.X == 0 && pointUp.Y == 0;
                        bool emptyDn = pointDn.X == 0 && pointDn.Y == 0;
                        if (!emptyUp && !emptyDn)
                        {
                            var y = Math.Abs(pointUp.Y - pointDn.Y);
                            var newY = y - (_sensorPairs[index].K * y + _sensorPairs[index].B);

                            sum.Add(new IntXFloatYPoint(x, (float)Math.Round(newY, 3)));
                        }
                    }

                    if (index == 0)
                    {
                        Interlocked.Increment(ref _genericBufferCount);
                    }

                    _sumBuffer[index].Add(sum);

                    SumBufferChanged?.Invoke(this, new EventArgs());

                    if (index == 0 && (int)(_genericBufferCount * HeightDiscrete) % 50 == 0)
                    {
                        //DataBaseThreshold?.Invoke(this, new EventArgs());
                        //_ = Task.Run(() => InsertDataToDb()).ConfigureAwait(false);
                        needToWrite = true;
                    }

                    _lastProfiles[index] = sum;

                    up = null;
                    dn = null;

                    _sensorPairs[index].TopSensor.ProfileBuffer.Clear();
                    _sensorPairs[index].BottomSensor.ProfileBuffer.Clear();

                    return 0;
                }

                return -1;
            }
            _sensorPairs[index].TopSensor.ProfileBuffer.Clear();
            _sensorPairs[index].BottomSensor.ProfileBuffer.Clear();
            return -1;
        }

        private void InsertDataToDb()
        {
            while (true)
            {
                if (needToWrite)
                {
                    IntXFloatYPoint[] points = LastProfile.ToArray();

                    List<List<IntXFloatYPoint>> chunks = new()
                    {
                        new List<IntXFloatYPoint>(),
                        new List<IntXFloatYPoint>(),
                        new List<IntXFloatYPoint>(),
                        new List<IntXFloatYPoint>()
                    };
                    foreach (var point in points)
                    {
                        if (point.X >= -500 && point.X < 0) chunks[0].Add(point);
                        if (point.X >= 0 && point.X < 500) chunks[1].Add(point);
                        if (point.X >= 500 && point.X < 1000) chunks[2].Add(point);
                        if (point.X >= 1000 && point.X < 1500) chunks[3].Add(point);
                    }

                    for (int i = 1; i < chunks.Count; i++)
                    {
                        if (chunks[i].Count > 0)
                            _sqlDataService.InsertData(chunks[i], $"PointsCollection{i}", "PirPoints");
                    }
                    needToWrite = false;
                }
                Thread.Sleep(1);
            }
        }

        private void ProcessStep()
        {
            lock (_sumBuffer)
            {
                int count = _sumBuffer.Max(l => l.Count);
                _fillCount = count;
            }

            if (_fillCount % Height == 0)
            {
                _ = Interlocked.Increment(ref _timesFilled);
                ThresholdReachedEvent?.Invoke(this, new EventArgs());
            }
        }

        private void ThresholdReached()
        {

            _fillCount = 0;

            _imgUp = new Image<Gray, byte>(_imageWidth, Height);
            _imgDn = new Image<Gray, byte>(_imageWidth, Height);

            for (int h = 0; h < Height; h++)
            {
                foreach (var buffer in _sumBuffer)
                {
                    if (buffer.TryTake(out var profile))
                    {
                        if (profile != default)
                        {
                            foreach (var point in profile)
                            {
                                int w = point.X + _offset;
                                if (w > 0 && w < _imageWidth)
                                {
                                    if (point.Y > _sensorRepository.UpperThreshold)
                                    {
                                        _imgUp.Data[h, w, 0] = 255;
                                    }
                                    if (point.Y < _sensorRepository.DownThreshold)
                                    {
                                        _imgDn.Data[h, w, 0] = 255;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            (var bmp, var defects) = _imageProcessingService.AnalyzeDefectsAsync(_imgUp, _imgDn,
                                                                                 _sensorRepository.WidthThreshold,
                                                                                 _sensorRepository.HeightThreshold,
                                                                                 _discretizeStep, HeightDiscrete,
                                                                                 GenericBufferCount);
            foreach (var defect in defects)
            {
                var w = defect.X - _offset;
                defect.X = Math.Round(w, 1);
            }

            var listOfDefects = defects.ToList();
            _imageProcessingService.FilterDefects(listOfDefects);

            Bmp = bmp; Defects = listOfDefects;
            DefectsAnalyzed = true;
        }

        /// <summary>
        /// Уничтожение объекта
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var pair in _sensorPairs)
                    {
                        pair.TopSensor.ProfileBuffer.CollectionChanged -= SensorQueueChanged;
                        pair.BottomSensor.ProfileBuffer.CollectionChanged -= SensorQueueChanged;
                    }
                    _synchronizer.SyncButtonIsChecked = false;
                    ThresholdReachedEvent -= ProcessingService_ThresholdReached;
                    //DataBaseThreshold -= WriteToDataBase;
                    SumBufferChanged -= ProcessingService_SumBufferChanged;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                _sumBuffer = null;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Уничтожает объект и освобождает все используемые ресурсы
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
