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
    public class BaslerRepository:BindableBase, IBaslerRepository
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
    }
}
