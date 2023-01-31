using Kogerent.Core;
using Kogerent.Services.Interfaces;

using Prism.Mvvm;

using System;
using System.Collections.ObjectModel;

namespace Kogerent.Services.Implementation
{
    public class NonControlZonesRepository : BindableBase, INonControlZonesRepository
    {
        private ObservableCollection<ObloyModel> _zones = new();
        public ObservableCollection<ObloyModel> Zones
        {
            get { return _zones; }
            set { SetProperty(ref _zones, value); }
        }

        private ObservableCollection<ObloyModel> _obloys = new();
        public ObservableCollection<ObloyModel> Obloys
        {
            get { return _obloys; }
            set { SetProperty(ref _obloys, value); }
        }

        private float _leftBorder;
        public float LeftBorder
        {
            get { return _leftBorder; }
            set
            {
                SetProperty(ref _leftBorder, value);
            }
        }

        private float _leftBorderMin = 0;
        public float LeftBorderMin
        {
            get { return _leftBorderMin; }
            set { SetProperty(ref _leftBorderMin, value); }
        }


        private float _leftBorderMax = 1200;
        public float LeftBorderMax
        {
            get { return _leftBorderMax; }
            set { SetProperty(ref _leftBorderMax, value); }
        }


        private float _rightBorder;
        public float RightBorder
        {
            get { return _rightBorder; }
            set
            {
                SetProperty(ref _rightBorder, value);
            }
        }

        private float _rightBorderMin = 2000;
        public float RightBorderMin
        {
            get { return _rightBorderMin; }
            set { SetProperty(ref _rightBorderMin, value); }
        }

        private float _rightBorderMax = 3808;
        public float RightBorderMax
        {
            get { return _rightBorderMax; }
            set { SetProperty(ref _rightBorderMax, value); }
        }



        private float _fullCamerasWidth = 3808;
        public float FullCamerasWidth
        {
            get { return _fullCamerasWidth; }
            set
            {
                SetProperty(ref _fullCamerasWidth, value);
            }
        }

        public void SetNonControlledBorders(float leftBorder, float rightBorder)
        {
            LeftBorder = leftBorder;
            LeftBorderMin = 0;
            LeftBorderMax = 6144;
            RightBorder = rightBorder;
           
            RightBorderMin = LeftBorderMax + 6144;
            RightBorderMax = RightBorderMin + 6144;
            if (RightBorder < RightBorderMin)
            {
                RightBorder = RightBorderMin;
            }
        }

        public NonControlZonesRepository()
        {

            //for (int i = 1; i <= 20; i++)
            //{
            //    Zones.Add(new ObloyModel { Name = $"Зона {i}", MinimumY = 0, MaximumY = 300 });
            //}
            //Obloys.Add(new ObloyModel { Name = $"Облой л.", MinimumY = 0, MaximumY = 300, MinimumX = 0, MaximumX = 50 });
            //Obloys.Add(new ObloyModel { Name = $"Облой п.", MinimumY = 0, MaximumY = 300, MinimumX = 1261, MaximumX = 1311 });
        }


        public void AddZones(IBaslerRepository BaslerRepository)
        {

            float camera2Shift = 6144 * BaslerRepository.BaslerCamerasCollection[0].WidthDescrete;
            float camera3Shift = camera2Shift + 6144 * BaslerRepository.BaslerCamerasCollection[1].WidthDescrete;

            Obloys.Clear();
            Obloys.Add(new ObloyModel
            {
                Name = $"Облой л.",
                MinimumY = 0,
                MaximumY = 300,
                MinimumX = 0,
                MaximumX = LeftBorder
            });

            //float rightCameraMinimumX;
            //if (BaslerRepository.FullCamerasWidth / 2 + BaslerRepository.CanvasWidth * 1_000 / 2 > camera3Shift)
            //{
            //    //Отступ определения дефектов правой камеры. Доп отступ из-за пересечения камер
            //    float offsetRight = BaslerRepository.FullCamerasWidth / 2 + BaslerRepository.CanvasWidth * 1_000 / 2;              
            //    //+BaslerRepository.BaslerCamerasCollection[1].RightBoundWidth;
            //    rightCameraMinimumX = offsetRight > BaslerRepository.FullCamerasWidth ? BaslerRepository.FullCamerasWidth : offsetRight;
            //}
            //else
            //{
            //    float offsetRight = BaslerRepository.FullCamerasWidth / 2 + BaslerRepository.CanvasWidth * 1_000 / 2;
            //    rightCameraMinimumX = offsetRight > BaslerRepository.FullCamerasWidth ? BaslerRepository.FullCamerasWidth : offsetRight;
            //}

            //BaslerRepository.RightBorderStart = (int)(rightCameraMinimumX / BaslerRepository.BaslerCamerasCollection[0].WidthDescrete);

            Obloys.Add(new ObloyModel
            {
                Name = $"Облой п.",
                MinimumY = 0,
                MaximumY = 300,
                MinimumX = RightBorder,
                MaximumX = FullCamerasWidth
            });

            // Zones.Clear();
            //Zones.Add(new ObloyModel
            //{
            //    Name = $"Зона л.",
            //    MinimumY = 0,
            //    MaximumY = 300,
            //    MinimumX = camera2Shift,
            //    MaximumX = BaslerRepository.BaslerCamerasCollection[1].LeftBorder
            //});

            //Zones.Add(new ObloyModel
            //{
            //    Name = $"Зона п.",
            //    MinimumY = 0,
            //    MaximumY = 300,
            //    MinimumX = BaslerRepository.BaslerCamerasCollection[1].RightBorder,
            //    MaximumX = camera3Shift
            //});
        }
    }
}
