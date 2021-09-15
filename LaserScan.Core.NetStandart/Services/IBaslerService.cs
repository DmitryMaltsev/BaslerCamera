using System.Collections.Generic;

using LaserScan.Core.NetStandart.Models;

namespace LaserScan.Core.NetStandart.Services
{
    public interface IBaslerService
    {
        List<BaslerCameraModel> CreateCameras(string[] ips);
    }
}