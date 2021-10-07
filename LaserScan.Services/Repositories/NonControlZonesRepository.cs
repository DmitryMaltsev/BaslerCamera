using Kogerent.Core;
using Kogerent.Services.Interfaces;

using Prism.Mvvm;

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
            float maximumX;
            if (BaslerRepository.FullCamerasWidth / 2 - BaslerRepository.CanvasWidth * 1_000 / 2 < camera2Shift)
            {
                float offsetLeft = (BaslerRepository.FullCamerasWidth / 2 - BaslerRepository.CanvasWidth * 1_000 / 2 -
                    BaslerRepository.BaslerCamerasCollection[1].LeftBoundWidth);
                maximumX = offsetLeft <= 0 ? 0 : offsetLeft;
            }
            else
            {
                float offsetLeft = BaslerRepository.FullCamerasWidth / 2 - BaslerRepository.CanvasWidth * 1_000 / 2;
                maximumX = offsetLeft <= 0 ? 0 : offsetLeft;
            }
            Obloys.Clear();
            Obloys.Add(new ObloyModel
            {
                Name = $"Облой л.",
                MinimumY = 0,
                MaximumY = 300,
                MinimumX = 0,
                MaximumX = maximumX
            });

            float minimumX;
            if (BaslerRepository.FullCamerasWidth / 2 + BaslerRepository.CanvasWidth * 1_000 / 2 > camera3Shift)
            {
                float offsetRight = BaslerRepository.FullCamerasWidth / 2 + BaslerRepository.CanvasWidth * 1_000 / 2 +
                    BaslerRepository.BaslerCamerasCollection[1].RightBoundWidth;
                minimumX = offsetRight > BaslerRepository.FullCamerasWidth ? BaslerRepository.FullCamerasWidth : offsetRight;
            }
            else
            {
                float offsetRight = BaslerRepository.FullCamerasWidth / 2 + BaslerRepository.CanvasWidth * 1_000 / 2;
                minimumX = offsetRight > BaslerRepository.FullCamerasWidth ? BaslerRepository.FullCamerasWidth : offsetRight;                 
            }
            Obloys.Add(new ObloyModel
            {
                Name = $"Облой п.",
                MinimumY = 0,
                MaximumY = 300,
                MinimumX = minimumX,
                MaximumX = BaslerRepository.FullCamerasWidth
            });
            Zones.Clear();
            Zones.Add(new ObloyModel
            {
                Name = $"Зона л.",
                MinimumY = 0,
                MaximumY = 300,
                MinimumX = camera2Shift,
                MaximumX = BaslerRepository.BaslerCamerasCollection[1].LeftBorder
            });

            Zones.Add(new ObloyModel
            {
                Name = $"Зона п.",
                MinimumY = 0,
                MaximumY = 300,
                MinimumX = BaslerRepository.BaslerCamerasCollection[1].RightBorder,
                MaximumX = camera3Shift
            });
        }
    }
}
