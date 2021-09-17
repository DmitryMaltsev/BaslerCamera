namespace Kogerent.Services.Interfaces
{
    public interface IBenchmarkRepository
    {
        double ImageProcessingSpeedCounter { get; set; }
        bool RawImage { get; set; }
        int TempQueueCount { get; set; }
    }
}
