using Kogerent.Core;
using Microsoft.Extensions.ObjectPool;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Kogerent.Services.Interfaces
{
    /// <summary>
    /// Контракт на создание экзепляров репозитория дефектов
    /// </summary>
    public interface IDefectRepository
    {
        /// <summary>
        /// Коллекция найденных дефектов
        /// </summary>
        ObservableCollection<DefectProperties> DefectsCollection { get; set; }

        /// <summary>
        /// Пул объектов дефектов
        /// </summary>
        ObjectPool<DefectProperties> DefectsPool { get; set; }

        /// <summary>
        /// Пул объектов профилей
        /// </summary>
        ObjectPool<List<PointF>> ListPool { get; set; }
    }
}
