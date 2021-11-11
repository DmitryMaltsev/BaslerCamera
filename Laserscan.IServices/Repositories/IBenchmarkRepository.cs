namespace Kogerent.Services.Interfaces
{
    public interface IBenchmarkRepository
    {
        double ImageProcessingSpeedCounter { get; set; }
        bool RawImage { get; set; }
        int TempQueueCount { get; set; }
        int LeftStrobe { get; set; }
        int CenterStrobe { get; set; }
        int RightStrobe { get; set; }
    }
}
