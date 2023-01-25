using Prism.Commands;

using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    /// <summary>
    /// ApplicationCommands-команда видимая на уровне приложения
    /// </summary>
    public class ApplicationCommands : IApplicationCommands
    {
        /// <summary>
        /// Инициализация одного датчика
        /// </summary>
        public CompositeCommand InitOneSensor { get; } = new(true);
        /// <summary>
        /// Инициализация нескольких датчиков
        /// </summary>
        public CompositeCommand InitAllSensors { get; } = new();
        /// <summary>
        /// Установка режима для одного датчика
        /// </summary>
        public CompositeCommand SetOneSensorMode { get; } = new(true);
        /// <summary>
        /// Установка режима для всех датчиков
        /// </summary>
        public CompositeCommand SetAllSensorsMode { get; } = new();

        /// <summary>
        /// Вызывает срабатывание текущего датчика, при условии, что тот находится в режиме «Синхронизация по команде».
        /// Получив данные, библиотека вызывает соответствующую Callback-функцию.
        /// </summary>
        public CompositeCommand SendSyncOneSensor { get; } = new(true);

        /// <summary>
        /// Вызывает  срабатывание всех датчиков, при условии, что тот находится в режиме «Синхронизация по команде».
        /// Получив данные, библиотека вызывает соответствующую Callback-функцию.
        /// </summary>
        public CompositeCommand SendSyncAllSensors { get; } = new();

        /// <summary>
        /// Повторная инициализация одного датчика
        /// </summary>
        public CompositeCommand ReInitOneSensor { get; } = new(true);

        /// <summary>
        /// Повторная инициализация всех датчиков
        /// </summary>
        public CompositeCommand ReInitAllSensors { get; } = new();

        /// <summary>
        /// Останавливает работу и уничтажает объект текущего датчика
        /// </summary>
        public CompositeCommand DisposeOneSensor { get; } = new(true);

        /// <summary>
        /// Останавливает работу и уничтажает объекты всех датчиков
        /// </summary>
        public CompositeCommand DisposeAllSensors { get; } = new();

        /// <summary>
        ///  Переводит датчик в какой-то из режимов синхронизации в соответствие с <see cref="SyncMode"/>.
        /// </summary>
        public CompositeCommand SetSyncMode { get; } = new(true);

        /// <summary>
        ///  Переводит все датчики в какой-то из режимов синхронизации в соответствие с <see cref="SyncMode"/>.
        /// </summary>
        public CompositeCommand SetAllSyncMode { get; } = new();

        /// <summary>
        /// Попытка отправить проверочое сообщение текущему датчику и получить от него ответный сигнал
        /// </summary>
        public CompositeCommand PingOneSensor { get; } = new(true);

        /// <summary>
        /// Попытка отправить проверочое сообщение всем датчикам и получить от них ответный сигнал
        /// </summary>
        public CompositeCommand PingAllSensors { get; } = new();

        /// <summary>
        /// Включить лазер на одном датчике
        /// </summary>
        public CompositeCommand LaserOneSensor { get; } = new(true);

        /// <summary>
        /// Включить лазер на всех датчиках
        /// </summary>
        public CompositeCommand LaserAllSensors { get; } = new();

        public CompositeCommand StartOneSensor { get; } = new(true);

        public CompositeCommand StartAllSensors { get; } = new();

        /// <summary>
        /// Выключить один датчик
        /// </summary>
        public CompositeCommand StopOneSensor { get; } = new(true);

        /// <summary>
        /// Выключить все датчики
        /// </summary>
        public CompositeCommand StopAllSensors { get; } = new();

        /// <summary>
        /// Получение температуры текущего датчика
        /// </summary>
        public CompositeCommand GetTemperatureOnseSensor { get; } = new(true);

        /// <summary>
        /// Установка экспозиции для текущего датчика
        /// </summary>
        public CompositeCommand SetExpositionOneSensor { get; } = new(true);

        /// <summary>
        /// Установка экспозиции для всех датчиков
        /// </summary>
        public CompositeCommand SetExpositionAllSensor { get; } = new();

        /// <summary>
        /// Установка аналогового усиления для текущего датчика
        /// </summary>
        public CompositeCommand SetAnalogGainOneSensor { get; } = new(true);

        /// <summary>
        /// Установка аналогового усиления для текущего датчика
        /// </summary>
        public CompositeCommand SetAnalogGainAllSensors { get; } = new();

        /// <summary>
        /// Установка дискретного усиления для текущего датчика
        /// </summary>
        public CompositeCommand SetDigitalGainOneSensor { get; } = new(true);

        /// <summary>
        /// Установка дискретного усиления для всех датчиков
        /// </summary>
        public CompositeCommand SetDigitalGainAllSensors { get; } = new();

        /// <summary>
        /// Установка коррекции для одного датчика
        /// </summary>
        public CompositeCommand SetCorrectionOneSensor { get; } = new(true);

        /// <summary>
        /// Установка коррекции для всех датчиков
        /// </summary>
        public CompositeCommand SetCorrectionAllSensors { get; } = new();

        public CompositeCommand SendSyncEvenUnEven { get; } = new(true);

        /// <summary>
        /// Создание таблицы дефектов
        /// </summary>
        public CompositeCommand SaveDefectsAndCrearTable { get; } = new();

        /// <summary>
        /// Создания аблоя
        /// </summary>
        public CompositeCommand SetObloysApplicationCommand { get; } = new();

        public CompositeCommand Destroy { get; } = new();

        public CompositeCommand Calibrate { get; } = new();

        public CompositeCommand StartRecordRawData { get; } = new();

        public CompositeCommand StopRecordRawData { get; } = new();

        public CompositeCommand CheckFilterAll { get; } = new();

        public CompositeCommand CheckCamerasOverLay { get; } = new();

        public CompositeCommand AddNewMaterial { get; } = new();

        public CompositeCommand ChangeMaterialCalibration { get; } = new();

        public CompositeCommand AutoExposition { get; } = new();

        public CompositeCommand FindBoundsIndexes { get; } = new();

        /// <summary>
        /// Отображение графиков сырых данных
        /// </summary>
        public CompositeCommand ShowGraphs { get; } = new();
    }
}
