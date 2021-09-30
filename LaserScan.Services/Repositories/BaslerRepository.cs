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
                _leftObloy = _fullCamerasWidth / 2 - _canvasWidth * 1_000 / 2;
                _rightObloy = _fullCamerasWidth / 2 + _canvasWidth * 1_000 / 2;
            }
        }
        private float _fullCamerasWidth=3573.694F;
        public float FullCamerasWidth
        {
            get { return _fullCamerasWidth; }
            set { SetProperty(ref _fullCamerasWidth, value); }
        }

        private float _leftObloy;
        public float LeftObloy
        {
            get { return _leftObloy; }
            set { SetProperty(ref _leftObloy, value); }
        }

        private float _rightObloy;
        public float RightObloy
        {
            get { return _rightObloy; }
            set { SetProperty(ref _rightObloy, value); }
        }
    }
}
