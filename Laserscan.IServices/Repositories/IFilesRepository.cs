namespace Kogerent.Services.Interfaces
{
    public interface IFilesRepository
    {
        int[] FilesRawCount { get; set; }
        float[] FilesRecordingTime { get; set; }
    }
}