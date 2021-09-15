using System;
using System.Collections.Concurrent;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public interface ICallbackPool
    {
        int Count { get; }

        CallbackData Get(bool inVideo, IntPtr data, int dataLength);
        void Return(CallbackData item);
    }

    public class CallbackPool : ICallbackPool
    {
        private readonly ConcurrentBag<CallbackData> _objects;
        public int Count => _objects.Count;
        public CallbackPool()
        {
            _objects = new ConcurrentBag<CallbackData>();
        }

        public CallbackData Get(bool inVideo, IntPtr data, int dataLength)
        {
            if (_objects.TryTake(out CallbackData result))
            {
                result.FromVideoCallback = inVideo;
                result.ReadData(data, dataLength);
                return result;
            }
            else
                return new CallbackData(inVideo, data, dataLength);
        }

        public void Return(CallbackData item) => _objects.Add(item);
    }
}
