using Kogerent.Core;
using System.Collections.ObjectModel;

namespace Kogerent.Services.Interfaces
{
    /// <summary>
    /// Сервис хранящий данные о зонах неконтроля
    /// </summary>
    public interface INonControlZonesRepository
    {
        ObservableCollection<ObloyModel> Zones { get; set; }
        ObservableCollection<ObloyModel> Obloys { get; set; }
        float LeftBorder { get; set; }
        float RightBorder { get; set; }
        float FullCamerasWidth { get; set; }
        float LeftBorderMin { get; set; }
        float LeftBorderMax { get; set; }
        float RightBorderMin { get; set; }
        float RightBorderMax { get; set; }

        void AddZones(IBaslerRepository BaslerRepository);
        void SetNonControlledBorders(float leftBorder, float rightBorder);
    }
}
