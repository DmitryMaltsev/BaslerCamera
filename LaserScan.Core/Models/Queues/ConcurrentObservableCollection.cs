using System;
using System.Collections.Concurrent;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class ConcurrentObservableCollection<T> : ConcurrentBag<T>
    {

        public event EventHandler<int> CollectionChanged;

        public void Enqueue(T item, int index)
        {
            Add(item);
            CollectionChanged?.Invoke(this, index);
        }
    }
}
