using Kogerent.Core.Models.Queues;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

using Prism.Mvvm;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    /// <summary>
    /// Объект датчика со всеми настройками
    /// </summary>
    public class SensorSettings : BindableBase, IDisposable
    {
        #region Properties
        private bool _canSendSync;
        [JsonIgnore]
        [XmlIgnore]
        public bool CanSendSync
        {
            get { return _canSendSync; }
            set 
            { 
                SetProperty(ref _canSendSync, value); 
            }
        }

        [JsonIgnore]
        [XmlIgnore]
        public ObjectPool<List<PointF>> ListPool { get; set; } = ObjectPool.Create<List<PointF>>();

        private List<PointF> _discretizedProfile;
        [JsonIgnore]
        [XmlIgnore]
        public List<PointF> DiscretizedProfile
        {
            get { return _discretizedProfile; }
            set { SetProperty(ref _discretizedProfile, value); }
        }

        [JsonIgnore]
        [XmlIgnore]
        public int Index { get; set; }

        private ConcurrentObservableCollection<List<IntXFloatYPoint>> _profileBuffer = new();
        [JsonIgnore]
        [XmlIgnore]
        public ConcurrentObservableCollection<List<IntXFloatYPoint>> ProfileBuffer { get => _profileBuffer; set => _profileBuffer = value; }

        [JsonIgnore]
        [XmlIgnore]
        public ConcurrentBag<float[]> XyzwBuffer { get; set; } = new();

        [JsonIgnore]
        [XmlIgnore]
        public bool IsExist => Handle != (IntPtr)null;

        private int _frameDataLength = 0;
        [JsonIgnore]
        [XmlIgnore]
        public int FrameDataLength
        {
            get { return _frameDataLength; }
            set { SetProperty(ref _frameDataLength, value); }
        }

        private int _notFullFrame;
        [JsonIgnore]
        [XmlIgnore]
        public int NotFullFrame
        {
            get { return _notFullFrame; }
            set { SetProperty(ref _notFullFrame, value); }
        }

        private int _nFrame = 0;
        [JsonIgnore]
        [XmlIgnore]
        public int NFrame
        {
            get { return _nFrame; }
            set { SetProperty(ref _nFrame, value); }
        }

        private uint _nStrobe = 0;
        [JsonIgnore]
        [XmlIgnore]
        public uint NStrobe
        {
            get { return _nStrobe; }
            set { SetProperty(ref _nStrobe, value); }
        }

        private ObservableCollection<IntXFloatYPoint> _profilePoints = new();
        [JsonIgnore]
        [XmlIgnore]
        public ObservableCollection<IntXFloatYPoint> ProfilePoints
        {
            get => _profilePoints;
            set => SetProperty(ref _profilePoints, value);
        }

        private List<IntXFloatYPoint> _profilePointsSum = new();
        [JsonIgnore]
        [XmlIgnore]
        public List<IntXFloatYPoint> ProfilePointsSum
        {
            get => _profilePointsSum;
            set
            {
                SetProperty(ref _profilePointsSum, value);
            }
        }

        private float _upperThreshold = 20f;
        public float UpperThreshold
        {
            get => _upperThreshold;
            set
            {
                SetProperty(ref _upperThreshold, value);
            }
        }

        private float _downThreshold = 500;
        public float DownThreshold
        {
            get => _downThreshold;
            set
            {
                SetProperty(ref _downThreshold, value);
            }
        }

        private float _widthThreshold;
        public float WidthThreshold
        {
            get { return _widthThreshold; }
            set { SetProperty(ref _widthThreshold, value); }
        }

        private float _heightThreshold;
        public float HeightThreshold
        {
            get { return _heightThreshold; }
            set { SetProperty(ref _heightThreshold, value); }
        }

        private float _zone1Left;
        public float Zone1Left
        {
            get => _zone1Left;
            set
            {
                SetProperty(ref _zone1Left, value);
            }
        }

        private float _zone1Right;
        public float Zone1Right
        {
            get => _zone1Right;
            set
            {
                SetProperty(ref _zone1Right, value);
            }
        }

        private float _zone2Left;
        public float Zone2Left
        {
            get => _zone2Left;
            set
            {
                SetProperty(ref _zone2Left, value);
            }
        }

        private float _zone2Right;
        public float Zone2Right
        {
            get => _zone2Right;
            set
            {
                SetProperty(ref _zone2Right, value);
            }
        }


        private System.Windows.Media.Color _colors = Colors.Red;
        [JsonIgnore]
        [XmlIgnore]
        public System.Windows.Media.Color ProfileColor
        {
            get { return _colors; }
            set { SetProperty(ref _colors, value); }
        }

        private List<PointF> _offlineProfilePoints;
        [JsonIgnore]
        [XmlIgnore]
        public List<PointF> OfflineProfilePoints
        {
            get { return _offlineProfilePoints; }
            set { SetProperty(ref _offlineProfilePoints, value); }
        }

        private BitmapImage _imageSource;
        [JsonIgnore]
        [XmlIgnore]
        public BitmapImage ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        private bool _calibrationIsLoaded = false;
        [JsonIgnore]
        [XmlIgnore]
        public bool CalibrationIsLoaded
        {
            get { return _calibrationIsLoaded; }
            set { SetProperty(ref _calibrationIsLoaded, value); }
        }

        private IntPtr _handle = (IntPtr)null;
        [JsonIgnore]
        [XmlIgnore]
        public IntPtr Handle
        {
            get { return _handle; }
            set { SetProperty(ref _handle, value); }
        }

        private bool _isStopped;
        [JsonIgnore]
        [XmlIgnore]
        public bool IsStopped
        {
            get { return _isStopped; }
            set { SetProperty(ref _isStopped, value); }
        }

        private bool _isOk;
        [JsonIgnore]
        [XmlIgnore]
        public bool IsOk
        {
            get { return _isOk; }
            set { SetProperty(ref _isOk, value); }
        }

        private string _hwVersion = "0.0.0";
        [JsonIgnore]
        [XmlIgnore]
        public string HWVersion
        {
            get { return _hwVersion; }
            set { SetProperty(ref _hwVersion, value); }
        }

        private bool _initialized;
        [JsonIgnore]
        [XmlIgnore]
        public bool Initialized
        {
            get { return _initialized; }
            set { SetProperty(ref _initialized, value); }
        }

        private float _temperature = 0;
        [JsonIgnore]
        [XmlIgnore]
        public float Temperature
        {
            get => _temperature;
            set { SetProperty(ref _temperature, value); }
        }

        private int _syncNoneHz = 20;
        [JsonIgnore]
        [XmlIgnore]
        public int SyncNoneHz
        {
            get { return _syncNoneHz; }
            set { SetProperty(ref _syncNoneHz, value); }
        }

        [JsonIgnore]
        [XmlIgnore]
        public int ServiceCounter { get; set; }

        private int _dataCounter;
        [JsonIgnore]
        [XmlIgnore]
        public int DataCounter { get => _dataCounter; set => _ = SetProperty(ref _dataCounter, value); }

        [JsonIgnore]
        [XmlIgnore]
        public bool ProfileMode
        {
            get => Mode is SensorMode.ProfileColumn or SensorMode.ProfileRow;
        }

        [JsonIgnore]
        [XmlIgnore]
        public bool VideoMode
        {
            get => !ProfileMode;
        }
        #endregion

        #region Serializable Properties

        private AnalogGain _analogGain = AnalogGain.X1dot6;
        public AnalogGain AnalogGain { get => _analogGain; set => SetProperty(ref _analogGain, value); }
        private float _angleCorrection = 0;
        public float AngleCorrection { get => _angleCorrection; set => SetProperty(ref _angleCorrection, value); }
        private bool _ascendingL = false;
        public bool AscendingL { get => _ascendingL; set => SetProperty(ref _ascendingL, value); }
        private bool _autoExpositionEn = false;
        public bool AutoExpositionEn { get => _autoExpositionEn; set => SetProperty(ref _autoExpositionEn, value); }
        private bool _blackReference = false;
        public bool BlackReference { get => _blackReference; set => SetProperty(ref _blackReference, value); }
        private bool _bordersD = false;
        public bool BordersD { get => _bordersD; set => SetProperty(ref _bordersD, value); }
        private bool _bordersL = false;
        public bool BordersL { get => _bordersL; set => SetProperty(ref _bordersL, value); }
        private float _BottomBorder = 0;
        public float BottomBorder { get => _BottomBorder; set => SetProperty(ref _BottomBorder, value); }
        private int _BoxHTracking = 50;
        public int BoxHTracking { get => _BoxHTracking; set => SetProperty(ref _BoxHTracking, value); }
        private int _BoxWTracking = 100;
        public int BoxWTracking { get => _BoxWTracking; set => SetProperty(ref _BoxWTracking, value); }
        private string _CalibrationFileName;
        public string CalibrationFileName { get => _CalibrationFileName; set => SetProperty(ref _CalibrationFileName, value); }
        private bool _Correction = true;
        public bool Correction { get => _Correction; set => SetProperty(ref _Correction, value); }
        private string _CriteriaTracking = "Simple";
        public string CriteriaTracking { get => _CriteriaTracking; set => SetProperty(ref _CriteriaTracking, value); }
        private bool _Debounce = true;
        public bool Debounce { get => _Debounce; set => SetProperty(ref _Debounce, value); }
        private uint _DebounceTime = 10;
        public uint DebounceTime { get => _DebounceTime; set => SetProperty(ref _DebounceTime, value); }

        private bool _Discretize = false;
        public bool Discretize { get => _Discretize; set => SetProperty(ref _Discretize, value); }
        private DiscretizeMode _DiscretizeMode = DiscretizeMode.Mean;
        public DiscretizeMode DiscretizeMode { get => _DiscretizeMode; set => SetProperty(ref _DiscretizeMode, value); }
        private byte _DiscretizeNearValue = 0;
        public byte DiscretizeNearValue { get => _DiscretizeNearValue; set => SetProperty(ref _DiscretizeNearValue, value); }
        private float _DiscretizeStep = 0.1f;
        public float DiscretizeStep { get => _DiscretizeStep; set => SetProperty(ref _DiscretizeStep, value); }

        private uint _Exposition = 30;
        public uint Exposition { get => _Exposition; set => SetProperty(ref _Exposition, value); }
        private int _ExpositionStep = 3;
        public int ExpositionStep { get => _ExpositionStep; set => SetProperty(ref _ExpositionStep, value); }

        private byte _Gain = 45;
        public byte Gain
        {
            get => _Gain;
            set
            {
                if (value > 59) value = 59;
                if (value < 0) value = 0;
                SetProperty(ref _Gain, value);
            }
        }
        private byte _AGain = 3;
        public byte AGain { get => _AGain; set => SetProperty(ref _AGain, value); }
        private ushort _HeightWindow = 1088;
        public ushort HeightWindow { get => _HeightWindow; set => SetProperty(ref _HeightWindow, value); }
        private bool _HFlipCorrection = false;
        public bool HFlipCorrection { get => _HFlipCorrection; set => SetProperty(ref _HFlipCorrection, value); }
        private bool _InMm = true;
        public bool InMm { get => _InMm; set => SetProperty(ref _InMm, value); }
        private int _IntervalTracking = 50;
        public int IntervalTracking { get => _IntervalTracking; set => SetProperty(ref _IntervalTracking, value); }
        private string _Ip;
        public string Ip { get => _Ip; set => SetProperty(ref _Ip, value); }
        private int _KPixelsToMm = 1;
        public int KPixelsToMm { get => _KPixelsToMm; set => SetProperty(ref _KPixelsToMm, value); }
        private bool _Laser = true;
        public bool Laser { get => _Laser; set => SetProperty(ref _Laser, value); }
        private bool _LaserInversion = false;
        public bool LaserInversion { get => _LaserInversion; set => SetProperty(ref _LaserInversion, value); }

        private float _LeftBorder = 0;
        public float LeftBorder { get => _LeftBorder; set => SetProperty(ref _LeftBorder, value); }
        private ushort _LeftWindow = 0;
        public ushort LeftWindow { get => _LeftWindow; set => SetProperty(ref _LeftWindow, value); }
        private ushort _Level = 250;
        public ushort Level { get => _Level; set => SetProperty(ref _Level, value); }
        private ushort _ManualDataPort;
        public ushort ManualDataPort { get => _ManualDataPort; set => SetProperty(ref _ManualDataPort, value); }
        private bool _ManualPortMode = true;
        public bool ManualPortMode { get => _ManualPortMode; set => SetProperty(ref _ManualPortMode, value); }
        private int _MaxExposition = 70;
        public int MaxExposition { get => _MaxExposition; set => SetProperty(ref _MaxExposition, value); }
        private ushort _MaxSpotsCount = 15;
        public ushort MaxSpotsCount { get => _MaxSpotsCount; set => SetProperty(ref _MaxSpotsCount, value); }
        private ushort _MaxWidth = 255;
        public ushort MaxWidth { get => _MaxWidth; set => SetProperty(ref _MaxWidth, value); }
        private int _MinExposition = 5;
        public int MinExposition { get => _MinExposition; set => SetProperty(ref _MinExposition, value); }
        private int _MinExpositionPoints = 100;
        public int MinExpositionPoints { get => _MinExpositionPoints; set => SetProperty(ref _MinExpositionPoints, value); }
        private int _MinPointsTracking = 100;
        public int MinPointsTracking { get => _MinPointsTracking; set => SetProperty(ref _MinPointsTracking, value); }
        private ushort _MinWidth = 1;
        public ushort MinWidth { get => _MinWidth; set => SetProperty(ref _MinWidth, value); }
        private int _MissingDesignatorValue = 0;
        public int MissingDesignatorValue { get => _MissingDesignatorValue; set => SetProperty(ref _MissingDesignatorValue, value); }
        private SensorMode _Mode = SensorMode.Video;
        public SensorMode Mode
        {
            get => _Mode;
            set
            {
                if (!SetProperty(ref _Mode, value)) return;
                RaisePropertyChanged(nameof(ProfileMode));
                RaisePropertyChanged(nameof(VideoMode));
            }
        }
        private int _NExpositionSkip = 20;
        public int NExpositionSkip { get => _NExpositionSkip; set => SetProperty(ref _NExpositionSkip, value); }
        private int _NFullRefreshTracking = 5000;
        public int NFullRefreshTracking { get => _NFullRefreshTracking; set => SetProperty(ref _NFullRefreshTracking, value); }
        private bool _NoiseFloatRemover = true;
        public bool NoiseFloatRemover { get => _NoiseFloatRemover; set => SetProperty(ref _NoiseFloatRemover, value); }
        private bool _NoiseFloatRemoverEn = false;
        public bool NoiseFloatRemoverEn { get => _NoiseFloatRemoverEn; set => SetProperty(ref _NoiseFloatRemoverEn, value); }
        private bool _NoiseRemover = true;
        public bool NoiseRemover { get => _NoiseRemover; set => SetProperty(ref _NoiseRemover, value); }
        private int _NoiseRemoverHeightWindow = 6;
        public int NoiseRemoverHeightWindow { get => _NoiseRemoverHeightWindow; set => SetProperty(ref _NoiseRemoverHeightWindow, value); }
        private int _NoiseRemoverThreshold = 2;
        public int NoiseRemoverThreshold { get => _NoiseRemoverThreshold; set => SetProperty(ref _NoiseRemoverThreshold, value); }
        private int _NoiseRemoverWidthWindow = 6;
        public int NoiseRemoverWidthWindow { get => _NoiseRemoverWidthWindow; set => SetProperty(ref _NoiseRemoverWidthWindow, value); }
        private byte _NSlopes = 1;
        public byte NSlopes { get => _NSlopes; set => SetProperty(ref _NSlopes, value); }
        private int _NSubRefreshTracking = 100;
        public int NSubRefreshTracking { get => _NSubRefreshTracking; set => SetProperty(ref _NSubRefreshTracking, value); }
        private int _NTargetsTracking = 1;
        public int NTargetsTracking { get => _NTargetsTracking; set => SetProperty(ref _NTargetsTracking, value); }
        private ushort _NumFramesPerStrobe = 1;
        public ushort NumFramesPerStrobe { get => _NumFramesPerStrobe; set => SetProperty(ref _NumFramesPerStrobe, value); }
        private int _OptExpositionW = 9;
        public int OptExpositionW { get => _OptExpositionW; set => SetProperty(ref _OptExpositionW, value); }
        private bool _Piecewise = false;
        public bool Piecewise { get => _Piecewise; set => SetProperty(ref _Piecewise, value); }
        private bool _Pipeline = false;
        public bool Pipeline { get => _Pipeline; set => SetProperty(ref _Pipeline, value); }
        private float _RatioDCorrection = 1;
        public float RatioDCorrection { get => _RatioDCorrection; set => SetProperty(ref _RatioDCorrection, value); }
        private float _RatioLCorrection = 1;
        public float RatioLCorrection { get => _RatioLCorrection; set => SetProperty(ref _RatioLCorrection, value); }
        private string _RefPointTracking = "Center";
        public string RefPointTracking { get => _RefPointTracking; set => SetProperty(ref _RefPointTracking, value); }
        private float _RightBorder = 150;
        public float RightBorder { get => _RightBorder; set => SetProperty(ref _RightBorder, value); }
        private float _ShiftDCorrection = 0;
        public float ShiftDCorrection { get => _ShiftDCorrection; set => SetProperty(ref _ShiftDCorrection, value); }
        private float _ShiftLCorrection = 0;
        public float ShiftLCorrection { get => _ShiftLCorrection; set => SetProperty(ref _ShiftLCorrection, value); }
        private byte _SlopeKp1 = 0;
        public byte SlopeKp1 { get => _SlopeKp1; set => SetProperty(ref _SlopeKp1, value); }
        private byte _SlopeKp2 = 0;
        public byte SlopeKp2 { get => _SlopeKp2; set => SetProperty(ref _SlopeKp2, value); }
        private bool _SubsamplingAdvMode = false;
        public bool SubsamplingAdvMode { get => _SubsamplingAdvMode; set => SetProperty(ref _SubsamplingAdvMode, value); }
        private byte _SubsamplingMatrix = 0;
        public byte SubsamplingMatrix { get => _SubsamplingMatrix; set => SetProperty(ref _SubsamplingMatrix, value); }
        private byte _SubsamplingProfile = 0;
        public byte SubsamplingProfile { get => _SubsamplingProfile; set => SetProperty(ref _SubsamplingProfile, value); }
        private float _TopBorder = 10000;
        public float TopBorder { get => _TopBorder; set => SetProperty(ref _TopBorder, value); }
        private ushort _TopWindow = 0;
        public ushort TopWindow { get => _TopWindow; set => SetProperty(ref _TopWindow, value); }
        private bool _Tracking = false;
        public bool Tracking { get => _Tracking; set => SetProperty(ref _Tracking, value); }
        private string _TrackingMaster = "Normal";
        public string TrackingMaster { get => _TrackingMaster; set => SetProperty(ref _TrackingMaster, value); }
        private byte _TrackingMode = 1;
        public byte TrackingMode { get => _TrackingMode; set => SetProperty(ref _TrackingMode, value); }
        private int _TrackingSlaveCorrX = 0;
        public int TrackingSlaveCorrX { get => _TrackingSlaveCorrX; set => SetProperty(ref _TrackingSlaveCorrX, value); }
        private int _TrackingSlaveCorrY = 0;
        public int TrackingSlaveCorrY { get => _TrackingSlaveCorrY; set => SetProperty(ref _TrackingSlaveCorrY, value); }
        private bool _UncompletedData = true;
        public bool UncompletedData { get => _UncompletedData; set => SetProperty(ref _UncompletedData, value); }
        private bool _VFlipCorrection = true;
        public bool VFlipCorrection { get => _VFlipCorrection; set => SetProperty(ref _VFlipCorrection, value); }
        private ushort _WidthWindow = 2048;
        public ushort WidthWindow { get => _WidthWindow; set => SetProperty(ref _WidthWindow, value); }
        private byte _XKp1 = 0;
        public byte XKp1 { get => _XKp1; set => SetProperty(ref _XKp1, value); }
        private byte _XKp2 = 0;
        public byte XKp2 { get => _XKp2; set => SetProperty(ref _XKp2, value); }
        private bool _ZeroDC = false;
        public bool ZeroDC { get => _ZeroDC; set => SetProperty(ref _ZeroDC, value); }

        private string _Name;
        private bool disposedValue;

        [XmlAttribute(AttributeName = "name")]
        public string Name { get => _Name; set => SetProperty(ref _Name, value); }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Handle = (IntPtr)null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                ProfileBuffer = null;
                ProfilePoints = null;
                ProfilePointsSum = null;
                DiscretizedProfile = null;
                XyzwBuffer = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SensorSettings()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        #endregion


    }
}
