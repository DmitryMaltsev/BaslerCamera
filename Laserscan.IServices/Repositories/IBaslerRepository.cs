
using System.Collections.ObjectModel;

using LaserScan.Core.NetStandart.Models;

namespace Kogerent.Services.Interfaces
{
    public interface IBaslerRepository
    {
        ObservableCollection<BaslerCameraModel> BaslerCamerasCollection { get; set; }
        BaslerCameraModel CurrentCamera { get; set; }
        bool AllCamerasInitialized { get; set; }
        int TotalCount { get; set; }
    }
}
