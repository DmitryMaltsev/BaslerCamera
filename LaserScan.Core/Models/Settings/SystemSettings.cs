using System.Windows.Markup;
using System.Xml.Serialization;
using Prism.Mvvm;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class SystemSettings : BindableBase
    {
        private string _ddlVersion = "ver 1.31";
        [XmlIgnore]
        public string DllVersion
        {
            get { return _ddlVersion; }
            set { SetProperty(ref _ddlVersion, value); }
        }

        private bool debugMode = false;
        public bool DebugMode { get => debugMode; set => SetProperty(ref debugMode, value); }
        private bool debugModeListener = false;
        public bool DebugModeListener { get => debugModeListener; set => SetProperty(ref debugModeListener, value); }
        private bool enhanceHeader = true;
        public bool EnhanceHeader { get => enhanceHeader; set => SetProperty(ref enhanceHeader, value); }
        private ushort firstPort = 34500;
        public ushort FirstPort { get => firstPort; set => SetProperty(ref firstPort, value); }
        private ushort lastPort = 34699;
        public ushort LastPort { get => lastPort; set => SetProperty(ref lastPort, value); }

        private bool _AutoStartEn = false;
        public bool AutoStartEn { get => _AutoStartEn; set => SetProperty(ref _AutoStartEn, value); }
        private bool _AutoTuning = false;
        public bool AutoTuning { get => _AutoTuning; set => SetProperty(ref _AutoTuning, value); }
        private int _BlackHoleDepthThreshold = -5;
        public int BlackHoleDepthThreshold { get => _BlackHoleDepthThreshold; set => SetProperty(ref _BlackHoleDepthThreshold, value); }
        private string _ClientName = "W1";
        public string ClientName { get => _ClientName; set => SetProperty(ref _ClientName, value); }
        private bool _DetectionSensorIsFirst = false;
        public bool DetectionSensorIsFirst { get => _DetectionSensorIsFirst; set => SetProperty(ref _DetectionSensorIsFirst, value); }
        private int _DilatationRadius = 5;
        public int DilatationRadius { get => _DilatationRadius; set => SetProperty(ref _DilatationRadius, value); }
        private int _ExpositionStep = 5;
        public int ExpositionStep { get => _ExpositionStep; set => SetProperty(ref _ExpositionStep, value); }
        private int _Gain = 100;
        public int Gain { get => _Gain; set => SetProperty(ref _Gain, value); }
        private int _GapMm = 2;
        public int GapMm { get => _GapMm; set => SetProperty(ref _GapMm, value); }
        private float _HoleDepthThreshold = 0.35f;
        public float HoleDepthThreshold { get => _HoleDepthThreshold; set => SetProperty(ref _HoleDepthThreshold, value); }
        private float _HoleDiameterThreshold = 1.12f;
        public float HoleDiameterThreshold { get => _HoleDiameterThreshold; set => SetProperty(ref _HoleDiameterThreshold, value); }
        private bool _LogDataEn = true;
        public bool LogDataEn { get => _LogDataEn; set => SetProperty(ref _LogDataEn, value); }
        private int _MapHeight = 300;
        public int MapHeight { get => _MapHeight; set => SetProperty(ref _MapHeight, value); }
        private int _MaxExposition = 200;
        public int MaxExposition { get => _MaxExposition; set => SetProperty(ref _MaxExposition, value); }
        private int _MaxGrayValue = 256;
        public int MaxGrayValue { get => _MaxGrayValue; set => SetProperty(ref _MaxGrayValue, value); }
        private bool _MeasEn = true;
        public bool MeasEn { get => _MeasEn; set => SetProperty(ref _MeasEn, value); }
        private int _Median1DWinStepWidth = 25;
        public int Median1DWinStepWidth { get => _Median1DWinStepWidth; set => SetProperty(ref _Median1DWinStepWidth, value); }
        private int _Median1DWinWidth = 50;
        public int Median1DWinWidth { get => _Median1DWinWidth; set => SetProperty(ref _Median1DWinWidth, value); }
        private int _Median2DShift = -7;
        public int Median2DShift { get => _Median2DShift; set => SetProperty(ref _Median2DShift, value); }
        private int _Median2DWin = 4;
        public int Median2DWin { get => _Median2DWin; set => SetProperty(ref _Median2DWin, value); }
        private int _MedianFullVerticalShiftDn = 0;
        public int MedianFullVerticalShiftDn { get => _MedianFullVerticalShiftDn; set => SetProperty(ref _MedianFullVerticalShiftDn, value); }
        private int _MeduanFullVerticalShiftUp = 0;
        public int MedianFullVerticalShiftUp { get => _MeduanFullVerticalShiftUp; set => SetProperty(ref _MeduanFullVerticalShiftUp, value); }
        private int _MinDataLength = 100;
        public int MinDataLength { get => _MinDataLength; set => SetProperty(ref _MinDataLength, value); }
        private int _MinDefectArea = 20;
        public int MinDefectArea { get => _MinDefectArea; set => SetProperty(ref _MinDefectArea, value); }
        private int _MinExposition = 5;
        public int MinExposition { get => _MinExposition; set => SetProperty(ref _MinExposition, value); }
        private int _NSensors = 4;
        public int NSensors { get => _NSensors; set => SetProperty(ref _NSensors, value); }
        private int _Offset = 126;
        public int Offset { get => _Offset; set => SetProperty(ref _Offset, value); }
        private int _OptExpositionW = 7;
        public int OptExpositionW { get => _OptExpositionW; set => SetProperty(ref _OptExpositionW, value); }
        private float _PeakThreshold = 0.5f;
        public float PeakThreshold { get => _PeakThreshold; set => SetProperty(ref _PeakThreshold, value); }
        private int _Port = 7003;
        public int Port { get => _Port; set => SetProperty(ref _Port, value); }
        private int _PreMedian2DStep = 2;
        public int PreMedian2DStep { get => _PreMedian2DStep; set => SetProperty(ref _PreMedian2DStep, value); }
        private int _PreMedian2DWin = 2;
        public int PreMedian2DWin { get => _PreMedian2DWin; set => SetProperty(ref _PreMedian2DWin, value); }
        private bool _Save3DDef = false;
        public bool Save3DDef { get => _Save3DDef; set => SetProperty(ref _Save3DDef, value); }
        private bool _Save3DLocDataEn = false;
        public bool Save3DLocDataEn { get => _Save3DLocDataEn; set => SetProperty(ref _Save3DLocDataEn, value); }
        private bool _Save3DRawDataEn = false;
        public bool Save3DRawDataEn { get => _Save3DRawDataEn; set => SetProperty(ref _Save3DRawDataEn, value); }
        private bool _Save3DSurfaceEn = false;
        public bool Save3DSurfaceEn { get => _Save3DSurfaceEn; set => SetProperty(ref _Save3DSurfaceEn, value); }
        private bool _SaveBigImgEn = false;
        public bool SaveBigImgEn { get => _SaveBigImgEn; set => SetProperty(ref _SaveBigImgEn, value); }
        private bool _SaveBmpSurfaceEn = false;
        public bool SaveBmpSurfaceEn { get => _SaveBmpSurfaceEn; set => SetProperty(ref _SaveBmpSurfaceEn, value); }
        private bool _SaveDefectMapEn = false;
        public bool SaveDefectMapEn { get => _SaveDefectMapEn; set => SetProperty(ref _SaveDefectMapEn, value); }
        private bool _SaveGradientEn = false;
        public bool SaveGradientEn { get => _SaveGradientEn; set => SetProperty(ref _SaveGradientEn, value); }
        private bool _SaveMapDef = false;
        public bool SaveMapDef { get => _SaveMapDef; set => SetProperty(ref _SaveMapDef, value); }
        private bool _SaveMaskEn = false;
        public bool SaveMaskEn { get => _SaveMaskEn; set => SetProperty(ref _SaveMaskEn, value); }
        private bool _SaveMedianEn = false;
        public bool SaveMedianEn { get => _SaveMedianEn; set => SetProperty(ref _SaveMedianEn, value); }
        private bool _SaveNdataOnMap = false;
        public bool SaveNdataOnMap { get => _SaveNdataOnMap; set => SetProperty(ref _SaveNdataOnMap, value); }
        private bool _SaveOnlyDefectMap = false;
        public bool SaveOnlyDefectMap { get => _SaveOnlyDefectMap; set => SetProperty(ref _SaveOnlyDefectMap, value); }
        private bool _SaveOriginalDefect = false;
        public bool SaveOriginalDefect { get => _SaveOriginalDefect; set => SetProperty(ref _SaveOriginalDefect, value); }
        private string _SensorESettingsId = "default";
        public string SensorESettingsId { get => _SensorESettingsId; set => SetProperty(ref _SensorESettingsId, value); }
        private SyncMode _SensorsSyncMode = SyncMode.SyncCmd;
        public SyncMode SensorsSyncMode { get => _SensorsSyncMode; set => SetProperty(ref _SensorsSyncMode, value); }
        private bool _ShowThresholds = false;
        public bool ShowThresholds { get => _ShowThresholds; set => SetProperty(ref _ShowThresholds, value); }
        private bool _ShowTimes = false;
        public bool ShowTimes { get => _ShowTimes; set => SetProperty(ref _ShowTimes, value); }
        private int _Sigma = 1;
        public int Sigma { get => _Sigma; set => SetProperty(ref _Sigma, value); }
        private float _SlotDepthThreshold = -0.14f;
        public float SlotDepthThreshold { get => _SlotDepthThreshold; set => SetProperty(ref _SlotDepthThreshold, value); }
        private int _SlotLengthThreshold = 7;
        public int SlotLengthThreshold { get => _SlotLengthThreshold; set => SetProperty(ref _SlotLengthThreshold, value); }
        private float _SlotWidthThreshold = 0.35f;
        public float SlotWidthThreshold { get => _SlotWidthThreshold; set => SetProperty(ref _SlotWidthThreshold, value); }
        private float _SobelSensitivityThreshold = 0.4f;
        public float SobelSensitivityThreshold { get => _SobelSensitivityThreshold; set => SetProperty(ref _SobelSensitivityThreshold, value); }
        private bool _StandAloneSensors = true;
        public bool StandAloneSensors { get => _StandAloneSensors; set => SetProperty(ref _StandAloneSensors, value); }
        private float _ThresholdDn = -0.1f;
        public float ThresholdDn { get => _ThresholdDn; set => SetProperty(ref _ThresholdDn, value); }
        private int _ThresholdUp = 1000;
        public int ThresholdUp { get => _ThresholdUp; set => SetProperty(ref _ThresholdUp, value); }
        private bool _UseStrobForMap = true;
        public bool UseStrobForMap { get => _UseStrobForMap; set => SetProperty(ref _UseStrobForMap, value); }
        private int _VibroMedianEstimationShift = 0;
        public int VibroMedianEstimationShift { get => _VibroMedianEstimationShift; set => SetProperty(ref _VibroMedianEstimationShift, value); }
        private int _VibroMedianStepHeight = 1;
        public int VibroMedianStepHeight { get => _VibroMedianStepHeight; set => SetProperty(ref _VibroMedianStepHeight, value); }
        private int _VibroMedianStepWidth = 25;
        public int VibroMedianStepWidth { get => _VibroMedianStepWidth; set => SetProperty(ref _VibroMedianStepWidth, value); }
        private int _VibroMedianWinHeight = 1;
        public int VibroMedianWinHeight { get => _VibroMedianWinHeight; set => SetProperty(ref _VibroMedianWinHeight, value); }
        private int _VibroMedianWinWidth = 150;
        public int VibroMedianWinWidth { get => _VibroMedianWinWidth; set => SetProperty(ref _VibroMedianWinWidth, value); }
        private float _YStep = 0.4f;
        public float YStep { get => _YStep; set => SetProperty(ref _YStep, value); }

        private string _name = "default";
        [XmlAttribute(AttributeName = "name")]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }
}
