using Kogerent.Core;
using Kogerent.Services.Interfaces;

using Microsoft.Extensions.ObjectPool;

using Prism.Mvvm;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Kogerent.Services.Implementation
{
    /// <summary>
    /// Репозиторий найденных дефектов
    /// </summary>
    public class DefectRepository : BindableBase, IDefectRepository
    {
        private ObservableCollection<DefectProperties> _defectsCollection = new();
        /// <summary>
        /// Коллекция найденных дефектов
        /// </summary>
        public ObservableCollection<DefectProperties> DefectsCollection
        {
            get { return _defectsCollection; }
            set { SetProperty(ref _defectsCollection, value); }
        }
        /// <summary>
        /// Пул объектов дефектов
        /// </summary>
        public ObjectPool<DefectProperties> DefectsPool { get; set; } = ObjectPool.Create<DefectProperties>();

        /// <summary>
        /// Пул объектов профилей
        /// </summary>
        public ObjectPool<List<PointF>> ListPool { get; set; } = ObjectPool.Create<List<PointF>>();

        /// <summary>
        /// Активация отрисовки дефектов на картинках
        /// </summary>
        private bool _visualAnalizeIsActive = true;
        public bool VisualAnalizeIsActive
        {
            get { return _visualAnalizeIsActive; }
            set { SetProperty(ref _visualAnalizeIsActive, value); }
        }

        private bool _createImages=false;
        public bool CreateImages
        {
            get { return _createImages; }
            set { SetProperty(ref _createImages, value); }
        }
    }
}
