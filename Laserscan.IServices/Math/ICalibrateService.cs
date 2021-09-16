namespace Kogerent.Services.Interfaces
{
    public interface ICalibrateService
    {
        (double[], byte[]) Calibrate(byte[] data);
        byte[] DefaultCalibration(double[] p, int count);
    }
}
