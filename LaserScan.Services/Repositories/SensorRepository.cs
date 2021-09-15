using Kogerent.Core;
using Kogerent.Services.Interfaces;
using Kogerent.Utilities;
using Microsoft.Extensions.ObjectPool;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;

namespace Kogerent.Services.Implementation
{
    /// <summary>
    /// Репозиторий свойств профилометров
    /// </summary>
    public class SensorRepository : BindableBase, ISensorRepository
    {
        private int _pipelineLimit = 1000;
        /// <summary>
        /// Лимит на добавление профилей в очередь
        /// </summary>
        public int PipelineLimit
        {
            get { return _pipelineLimit; }
            set { SetProperty(ref _pipelineLimit, value); }
        }

        private int _queueCount;
        /// <summary>
        /// Количество необработанных элементов в очереди
        /// </summary>
        public int QueueCount
        {
            get { return _queueCount; }
            set { SetProperty(ref _queueCount, value); }
        }

        private int _dataCounter;
        /// <summary>
        /// Счетчик данных
        /// </summary>
        public int DataCounter
        {
            get { return _dataCounter; }
            set { SetProperty(ref _dataCounter, value); }
        }

        private ObservableCollection<SensorSettings> _sensors = new();
        /// <summary>
        /// Коллекция объектов профилометров
        /// </summary>
        public ObservableCollection<SensorSettings> Sensors
        {
            get { return _sensors; }
            set { SetProperty(ref _sensors, value); }
        }

        private SensorSettings _currentSensor;
        /// <summary>
        /// Текущий (выбранный) объект профилометра
        /// </summary>
        public SensorSettings CurrentSensor
        {
            get { return _currentSensor; }
            set
            {
                SetProperty(ref _currentSensor, value);
                Sensors.ForEach(x => x.ProfileColor = Colors.Red);
                if (_currentSensor == null) return;
                _currentSensor.ProfileColor = Colors.AliceBlue;
            }
        }

        private ObservableCollection<SystemSettings> _systemSettings = new();
        /// <summary>
        /// Коллекция системных настроек
        /// </summary>
        public ObservableCollection<SystemSettings> SystemSettings
        {
            get { return _systemSettings; }
            set { SetProperty(ref _systemSettings, value); }
        }

        /// <summary>
        /// Выбранная системная настройка
        /// </summary>
        public SystemSettings CurrentSystem => SystemSettings[0];

        private float _upperThreshold;
        /// <summary>
        /// Верхний порог дефектования
        /// </summary>
        public float UpperThreshold
        {
            get { return _upperThreshold; }
            set { SetProperty(ref _upperThreshold, value); }
        }

        private float _downThreshold;
        /// <summary>
        /// Нижний порог дефектования
        /// </summary>
        public float DownThreshold
        {
            get { return _downThreshold; }
            set { SetProperty(ref _downThreshold, value); }
        }

        private ObservableCollection<IntXFloatYPoint> _totalSumsPoints = new();
        /// <summary>
        /// Коллекция точек ширины
        /// </summary>
        public ObservableCollection<IntXFloatYPoint> TotalSumsPoints
        {
            get { return _totalSumsPoints; }
            set { SetProperty(ref _totalSumsPoints, value); }
        }

        private float _widthThreshold;
        /// <summary>
        /// Порог дефектования по ширине
        /// </summary>
        public float WidthThreshold
        {
            get { return _widthThreshold; }
            set { SetProperty(ref _widthThreshold, value); }
        }

        private float _heightThreshold;
        /// <summary>
        /// Порог дефектования по длине
        /// </summary>
        public float HeightThreshold
        {
            get { return _heightThreshold; }
            set { SetProperty(ref _heightThreshold, value); }
        }

        private int _genericCounter;
        /// <summary>
        /// Общий счетчик
        /// </summary>
        public int GenericCounter { get => _genericCounter; set => SetProperty(ref _genericCounter, value); }

        private int _defectsBufferCounter;
        /// <summary>
        /// Количество найденных дефектов
        /// </summary>
        public int DefectBufferCounter { get => _defectsBufferCounter; set => SetProperty(ref _defectsBufferCounter, value); }

        private double _elapsed;
        /// <summary>
        /// Время обработки
        /// </summary>
        public double Elapsed
        {
            get => _elapsed;
            set => _ = SetProperty(ref _elapsed, value);
        }

        private float _coveyerSpeed = 0.5f;
        /// <summary>
        /// Скорость конвейера
        /// </summary>
        public float ConveyerSpeed
        {
            get { return _coveyerSpeed; }
            set { SetProperty(ref _coveyerSpeed, value); }
        }

        /// <summary>
        /// Пары верхних и нижних профилометров
        /// </summary>
        public List<SensorPair> SensorPairs { get; set; } = new();

        /// <summary>
        /// Пул коллекций точек
        /// </summary>
        public ObjectPool<List<IntXFloatYPoint>> ListPool { get; set; } = ObjectPool.Create<List<IntXFloatYPoint>>();

        /// <summary>
        /// Пул коллекций точек
        /// </summary>
        public ObjectPool<List<List<IntXFloatYPoint>>> GenericMapPool { get; set; } = ObjectPool.Create<List<List<IntXFloatYPoint>>>();

    }
}
