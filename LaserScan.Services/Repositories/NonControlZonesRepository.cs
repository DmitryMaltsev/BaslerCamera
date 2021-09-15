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


            for (int i = 1; i <= 20; i++)
            {
                Zones.Add(new ObloyModel { Name = $"Зона {i}", MinimumY = 0, MaximumY = 300 });
            }

            Obloys.Add(new ObloyModel { Name = $"Облой л.", MinimumY = 0, MaximumY = 300, MinimumX = 0, MaximumX = 50 });
            Obloys.Add(new ObloyModel { Name = $"Облой п.", MinimumY = 0, MaximumY = 300, MinimumX = 1261, MaximumX = 1311 });
        }
    }
}
