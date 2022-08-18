using LaserScan.Core.NetStandart.Models;

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Kogerent.Services.Interfaces
{
    public interface ICalibrateService
    {
        (double[], int[]) Calibrate(byte[] data);
        (int, double) CalibrateExposureTimeRaw(byte[] data, int currentExposure);
        double[] CalibrateMultiRaw(byte[] data);
        sbyte[] CalibrateRaw(byte[] data);
        List<byte> CreateAverageDataForCalibration(ConcurrentQueue<BufferData> _concurentBuffer, int countArraysInSection, int width);
        List<byte> CreateAveragElementsForCalibration(ConcurrentQueue<BufferData> _concurentBuffer, int countArraysInSection, int width);
        sbyte[] DefaultCalibration(double[] p, int count);
    }
}
