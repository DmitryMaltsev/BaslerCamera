using System.Collections.Generic;

namespace Kogerent.Core.Models.Queues
{
    public static class ListHelper
    {
        public static List<T> GetFirstRange<T>(this List<T> list, int width)
        {
            int count = list.Count;
            if (count < width) return null;

            List<T> range = list.GetRange(0, width);
            list.RemoveRange(0, width);

            return range;
        }
    }
}
