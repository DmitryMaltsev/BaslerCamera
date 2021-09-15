using Kogerent.Core;
using Kogerent.Core.Models;
using System.Collections.Generic;

namespace Kogerent.Services.Interfaces
{
    public interface IArchiveRepository
    {
        List<DateTimePoint> DateTimePointsCollection { get; set; }

        List<IntXFloatYPoint> IntXFloatYPointsCollection { get; set; }
    }
}
