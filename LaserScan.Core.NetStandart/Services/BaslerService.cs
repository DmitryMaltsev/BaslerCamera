using System.Collections.Generic;

using Basler.Pylon;

using LaserScan.Core.NetStandart.Models;

namespace LaserScan.Core.NetStandart.Services
{
    public class BaslerService : IBaslerService
    {
        public List<BaslerCameraModel> CreateCameras(string[] ips)
        {
            List<BaslerCameraModel> result = new List<BaslerCameraModel>();
            List<ICameraInfo> allCameras = CameraFinder.Enumerate();
            foreach (var camera in allCameras)
            {
                foreach (var ip in ips)
                {
                    if (camera[CameraInfoKey.DeviceIpAddress] == ip)
                    {
                        result.Add(new BaslerCameraModel
                        {
                            Ip = ip
                        });
                        break;
                    }
                }
            }
            return result;
        }
    }
}
