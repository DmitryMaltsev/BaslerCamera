using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Kogerent.Services.Interfaces
{
    public interface IFilesRepository
    {
       ObservableCollection<int> FilesRawCount { get; set; }
        ObservableCollection<double> FilesRecordingTime { get; set; }
        bool IsRecordingRawData { get; set; }
        bool IsRecordStopped { get; set; }
    }
}