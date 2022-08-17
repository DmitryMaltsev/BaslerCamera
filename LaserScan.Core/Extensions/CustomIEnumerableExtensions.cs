using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    /// <summary>
    /// Помощник по работе с IEnumerable
    /// </summary>
    public static class CustomIEnumerableExtensions
    {
        /// <summary>
        /// Dequeue chunkSize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<T> DequeueChunk<T>(this Queue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                yield return queue.Dequeue();
            }
        }

        /// <summary>
        /// Dequeue chunkSize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<T> DequeueChunkConcurrent<T>(this ConcurrentQueue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                T result;
                queue.TryDequeue(out result);
                //while (!queue.TryDequeue(out result)) { }
                yield return result;
            }
        }

        /// <summary>
        /// Dequeue chunkSize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue"></param>
        /// <returns>If not timed out returns T result, else returns default(T)</returns>
        public static T GaranteedDequeue<T>(this ConcurrentQueue<T> queue, TimeSpan timeout)
        {
            T result;
            var sw = Stopwatch.StartNew();
            while (!queue.TryDequeue(out result) || sw.Elapsed < timeout) { Console.WriteLine(sw.Elapsed); }
            return result;
        }

        /// <summary>
        /// Разделяет коллекцию на равномерные сегменты
        /// </summary>
        /// <typeparam name="T">Тип коллекции</typeparam>
        /// <param name="collection">Коллекция</param>
        /// <param name="chunksize">количество элементов в сегменте</param>
        /// <returns>Коллекцию подколлекций</returns>
        public static IEnumerable<List<T>> SplitByCount<T>(this T[] collection, int chunksize)
        {
            IEnumerator enumerator = collection.GetEnumerator();
            int position = 0;
            while (position < collection.Length)
            {
                yield return SubCollection().ToList();

                IEnumerable<T> SubCollection()
                {
                    for (int i = 0; i < chunksize; i++)
                    {
                        if (enumerator.MoveNext())
                        {
                            yield return (T)enumerator.Current;
                        }
                        position++;
                    }
                }
            }
        }
    }
}
