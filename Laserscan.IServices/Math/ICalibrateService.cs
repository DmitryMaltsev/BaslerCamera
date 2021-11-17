namespace Kogerent.Services.Interfaces
{
    public interface ICalibrateService
    {
        (double[], int[]) Calibrate(byte[] data);
        (int, double) CalibrateExposureTimeRaw(byte[] data, int currentExposure);
        double[] CalibrateRaw(byte[] data);
        sbyte[] DefaultCalibration(double[] p, int count);
    }
}
