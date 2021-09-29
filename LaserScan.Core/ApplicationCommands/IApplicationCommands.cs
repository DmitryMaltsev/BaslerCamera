using Prism.Commands;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public interface IApplicationCommands
    {
        CompositeCommand InitOneSensor { get; }
        CompositeCommand SetOneSensorMode { get; }
        CompositeCommand SendSyncOneSensor { get; }
        CompositeCommand ReInitOneSensor { get; }
        CompositeCommand DisposeOneSensor { get; }
        CompositeCommand SetSyncMode { get; }
        CompositeCommand PingOneSensor { get; }
        CompositeCommand LaserOneSensor { get; }
        CompositeCommand StopOneSensor { get; }
        CompositeCommand GetTemperatureOnseSensor { get; }
        CompositeCommand SetAllSensorsMode { get; }
        CompositeCommand SetAllSyncMode { get; }
        CompositeCommand InitAllSensors { get; }
        CompositeCommand SendSyncAllSensors { get; }
        CompositeCommand ReInitAllSensors { get; }
        CompositeCommand DisposeAllSensors { get; }
        CompositeCommand PingAllSensors { get; }
        CompositeCommand LaserAllSensors { get; }
        CompositeCommand StopAllSensors { get; }
        CompositeCommand SetExpositionOneSensor { get; }
        CompositeCommand SetExpositionAllSensor { get; }
        CompositeCommand SetAnalogGainOneSensor { get; }
        CompositeCommand SetAnalogGainAllSensors { get; }
        CompositeCommand SetDigitalGainOneSensor { get; }
        CompositeCommand SetDigitalGainAllSensors { get; }
        CompositeCommand SetCorrectionOneSensor { get; }
        CompositeCommand SetCorrectionAllSensors { get; }
        CompositeCommand SaveDefectsAndCrearTable { get; }
        CompositeCommand SetObloysApplicationCommand { get; }
        CompositeCommand SendSyncEvenUnEven { get; }
        CompositeCommand Destroy { get; }
        CompositeCommand StartOneSensor { get; }
        CompositeCommand StartAllSensors { get; }
        CompositeCommand Calibrate { get; }
        CompositeCommand CheckNoCalibrateAll { get; }
        CompositeCommand CheckFilterAll { get; }
    }
}
