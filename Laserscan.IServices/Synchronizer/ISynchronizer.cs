using Kogerent.Components;

namespace Kogerent.Services.Interfaces
{
    public interface ISynchronizer
    {
        bool SyncButtonIsChecked { get; set; }
        double TimerHz { get; set; }
    }
}
