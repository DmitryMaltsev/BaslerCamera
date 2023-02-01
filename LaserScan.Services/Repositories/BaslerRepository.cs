using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using Kogerent.Services.Interfaces;

using LaserScan.Core.NetStandart.Models;

using Prism.Mvvm;

namespace Kogerent.Services.Implementation
{
    public class BaslerRepository : BindableBase, IBaslerRepository
    {

        private double _calibrationTimer;
        public double CalibrationTimer
        {
            get { return _calibrationTimer; }
            set { SetProperty(ref _calibrationTimer, value); }
        }

        public BaslerRepository(INonControlZonesRepository nonControlZonesRepository)
        {
            NonControlZonesRepository = nonControlZonesRepository;
        }

        public INonControlZonesRepository NonControlZonesRepository { get; }

        private bool _allCamerasInitialized;
        public bool AllCamerasInitialized
        {
            get { return _allCamerasInitialized; }
            set { SetProperty(ref _allCamerasInitialized, value); }
        }

        private BaslerCameraModel _currentCamera;
        public BaslerCameraModel CurrentCamera
        {
            get { return _currentCamera; }
            set
            {
                SetProperty(ref _currentCamera, value);
            }
        }

        private ObservableCollection<BaslerCameraModel> _baslerCameraModel;
        public ObservableCollection<BaslerCameraModel> BaslerCamerasCollection
        {
            get { return _baslerCameraModel; }
            set { SetProperty(ref _baslerCameraModel, value); }
        }

        private MaterialModel _currentMaterial;
        public MaterialModel CurrentMaterial
        {
            get { return _currentMaterial; }
            set
            {
                SetProperty(ref _currentMaterial, value);
            }
        }

        private ObservableCollection<MaterialModel> _materialModelCollection;
        public ObservableCollection<MaterialModel> MaterialModelCollection
        {
            get { return _materialModelCollection; }
            set
            {
                SetProperty(ref _materialModelCollection, value);
            }
        }

        private bool _graphsIsActive = false;
        public bool GraphsIsActive
        {
            get { return _graphsIsActive; }
            set { SetProperty(ref _graphsIsActive, value); }
        }

        private int _totalCount;
        public int TotalCount
        {
            get { return _totalCount; }
            set { SetProperty(ref _totalCount, value); }
        }

        private bool _allDefectsFound;
        public bool AllDefectsFound
        {
            get { return _allDefectsFound; }
            set { SetProperty(ref _allDefectsFound, value); }
        }
    }
}
