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
    }
}
