using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kogerent.Services.Interfaces;

using LaserScan.Core.NetStandart.Models;

using Prism.Mvvm;

namespace Kogerent.Services.Implementation
{
    public class BaslerRepository : BindableBase, IBaslerRepository
    {
        public BaslerRepository(INonControlZonesRepository nonControlZonesRepository)
        {
            NonControlZonesRepository = nonControlZonesRepository;
        }
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
            set { SetProperty(ref _currentCamera, value); }
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
            set { 
                SetProperty(ref _currentMaterial, value);
                for (int i = 0; i < _currentMaterial.CameraDeltaList.Count; i++)
                {
                    this.BaslerCamerasCollection[i].Deltas = _currentMaterial.CameraDeltaList[i].Deltas;
                }
            }
        }

        private ObservableCollection<MaterialModel> _materialModelCollection;
        public ObservableCollection<MaterialModel> MaterialModelCollection
        {
            get { return _materialModelCollection; }
            set { SetProperty(ref _materialModelCollection, value); }
        }

        private int _totalCount;
        public int TotalCount
        {
            get { return _totalCount; }
            set { SetProperty(ref _totalCount, value); }
        }

        private float _canvasWidth;
        public float CanvasWidth
        {
            get { return _canvasWidth; }
            set
            {
                SetProperty(ref _canvasWidth, value);
                if (NonControlZonesRepository.Obloys.Count > 1)
                {
                    NonControlZonesRepository.AddZones(this);
                }
            }
        }
        private float _fullCamerasWidth = 3810;
        public float FullCamerasWidth
        {
            get { return _fullCamerasWidth; }
            set { SetProperty(ref _fullCamerasWidth, value); }
        }

        public INonControlZonesRepository NonControlZonesRepository { get; }
    }
}
