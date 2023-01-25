using LaserScan.Core.NetStandart.Models;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Kogerent.Services.Interfaces
{
    public interface ICalibrateService
    {
        void AddCalibrateSettingsToMaterial(ObservableCollection<BaslerCameraModel> baslerCameraCollection, MaterialModel currentMaterial);
        (double[], int[]) Calibrate(byte[] data);
        (int, double) CalibrateExposureTimeRaw(byte[] data, int currentExposure);
        double[] CalibrateMultiRaw(byte[] data);
        sbyte[] CalibrateRaw(byte[] data);
        List<byte> CreateAverageDataForCalibration(ConcurrentQueue<BufferData> _concurentBuffer, int countArraysInSection, int width);
        List<byte> CreateAverageElementsForCalibration(byte[,] _arrayOfRawPoints, int countArraysInSection, int width);
        ObservableCollection<MaterialModel> CreateDefaultMaterialCollection();
        sbyte[] DefaultCalibration(double[] p, int count);
        bool NeedChangeExposition(ConcurrentQueue<BufferData> _concurentBuffer, int countArraysInSection, int width, int xMinIndex, int xMaxIndex, int minBoundsLight, int maxBoundsLight, out int changeExpoisitionValue);
    }
}
