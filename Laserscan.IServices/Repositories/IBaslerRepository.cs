
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
        float CanvasWidth { get; set; }
        float FullCamerasWidth { get; set; }
        ObservableCollection<MaterialModel> MaterialModelCollection { get; set; }
        MaterialModel CurrentMaterial { get; set; }
        bool AllCamerasStarted { get; set; }
    }
}
