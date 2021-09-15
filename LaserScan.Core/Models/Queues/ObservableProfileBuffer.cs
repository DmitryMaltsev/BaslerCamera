using System;

namespace Kogerent.Core.Models.Queues
{
    public class ObservableProfileBuffer<T> : ReadPostprocessinger<T>
    {
        public event EventHandler<int> CollectionChanged;

        public ObservableProfileBuffer(int capacity, bool overflow = false) : base(capacity, overflow)
        {
        }

        public void Write(T data, int index)
        {
            Write(data);
            CollectionChanged?.Invoke(this, index);
        }
    }
}
