using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class MultiConcurrentBuffer<T>
    {
        public ConcurrentQueue<T>[] Collections { get; }

        public MultiConcurrentBuffer(ConcurrentQueue<T>[] collections)
        {
            Collections = collections;
        }

        public IEnumerable<T> Dequeue()
        {
            IEnumerable<T> result = Collections.Select((q, i) => { return Collections[i].TryDequeue(out T res) ? res : default; })
                                               .Where(a => !Equals(a, default(T)));

            return result.Count() == Collections.Length ? result : null;
        }
    }
}
