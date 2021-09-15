using Kogerent.Core;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;

namespace Kogerent.Services.Interfaces
{
    /// <summary>
    /// Контракт на создания сервиса, хранящего свойства профилометров
    /// </summary>
    public interface ISensorRepository
    {
        /// <summary>
        /// выбраннык объект профилометра
        /// </summary>
        SensorSettings CurrentSensor { get; set; }

        /// <summary>
        /// Коллекция объектов профилометров
        /// </summary>
        ObservableCollection<SensorSettings> Sensors { get; set; }

        /// <summary>
        /// Коллекция системных настроек
        /// </summary>
        ObservableCollection<SystemSettings> SystemSettings { get; set; }

        /// <summary>
        /// Выбранная системная настройка
        /// </summary>
        SystemSettings CurrentSystem { get; }

        /// <summary>
        /// Верхний порог дефектования
        /// </summary>
        float UpperThreshold { get; set; }

        /// <summary>
        /// Нижний порог дефектования
        /// </summary>
        float DownThreshold { get; set; }

        /// <summary>
        /// Коллекция точек ширины
        /// </summary>
        ObservableCollection<IntXFloatYPoint> TotalSumsPoints { get; set; }

        /// <summary>
        /// Порог дефектования по ширине
        /// </summary>
        float WidthThreshold { get; set; }

        /// <summary>
        /// Порог дефектования по длине
        /// </summary>
        float HeightThreshold { get; set; }

        /// <summary>
        /// Общий счетчик
        /// </summary>
        int GenericCounter { get; set; }

        /// <summary>
        /// Количество необработанных элементов в очереди
        /// </summary>
        int QueueCount { get; set; }

        /// <summary>
        /// Пары верхних и нижних профилометров
        /// </summary>
        List<SensorPair> SensorPairs { get; set; }

        /// <summary>
        /// Количество найденных дефектов
        /// </summary>
        int DefectBufferCounter { get; set; }

        /// <summary>
        /// Лимит на добавление профилей в очередь
        /// </summary>
        int PipelineLimit { get; set; }

        /// <summary>
        /// Время обработки
        /// </summary>
        double Elapsed { get; set; }

        /// <summary>
        /// Пул коллекций точек
        /// </summary>
        ObjectPool<List<IntXFloatYPoint>> ListPool { get; set; }

        /// <summary>
        /// Счетчик данных
        /// </summary>
        int DataCounter { get; set; }

        /// <summary>
        /// Скорость конвейерв
        /// </summary>
        float ConveyerSpeed { get; set; }

        ObjectPool<List<List<IntXFloatYPoint>>> GenericMapPool { get; set; }

        /// <summary>
        /// Событие изменения свойств
        /// </summary>
        event PropertyChangedEventHandler PropertyChanged;
    }
}
