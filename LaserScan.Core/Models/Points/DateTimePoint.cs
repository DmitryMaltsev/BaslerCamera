using System;

namespace Kogerent.Core.Models
{
    public struct DateTimePoint
    {
        public DateTime X { get; set; }
        public float Y { get; set; }

        public DateTimePoint(DateTime x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
