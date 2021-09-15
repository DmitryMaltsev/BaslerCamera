using Kogerent.Core;
using Kogerent.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Kogerent.Services.Implementation
{
    /// <summary>
    /// Служба конвертации массива xyzw в точки и обратно
    /// </summary>
    public class PointService : IPointService
    {
        /// <summary>
        /// Конвертирует массив xyzw в коллекцию PointF
        /// </summary>
        /// <param name="xyzw">Входящий массив</param>
        /// <returns>Коллекцию точек</returns>
        public List<PointF> XyzwToPointF(float[] xyzw)
        {
            var list = new List<PointF>(xyzw.Length / 4 + 1);
            var cnt = xyzw.Length;
            for (int i = 0; i < cnt; i += 4)
            {
                list.Add(new PointF(xyzw[i], xyzw[i + 1]));
            }
            return list.OrderBy(x => x.X).ToList();
        }


        /// <summary>
        /// Конвертирует массив xyzw в коллекцию PointF
        /// </summary>
        /// <param name="xyzw">Входящий массив</param>
        /// <returns>Коллекцию точек</returns>
        public List<IntXFloatYPoint> XyzwToIntXFloatYPoint(float[] xyzw)
        {
            var list = new List<IntXFloatYPoint>(xyzw.Length / 4 + 1);
            var cnt = xyzw.Length;
            for (int i = 0; i < cnt; i += 4)
            {
                if (!float.IsNaN(xyzw[i]) && !float.IsNaN(xyzw[i + 1]))
                {
                    list.Add(new IntXFloatYPoint((int)xyzw[i], xyzw[i + 1]));
                }
            }
            xyzw = null;
            return list.OrderBy(x => x.X).ToList();
        }

        public Task<IEnumerable<PointF>> XyzwToPointFAsync(float[] xyzw)
        {
            return Task.Run(() =>
            {
                var xs = xyzw.Where((p, i) => i % 4 == 0);
                var ys = xyzw.Skip(1).Where((p, i) => i % 4 == 0);
                return xs.Zip(ys, (x, y) => new PointF(x, y));
            });
        }

        public List<PointF> XyzwToPointFSimple(float[] xyzw)
        {
            IEnumerable<float> xs = xyzw.Where((p, i) => i % 4 == 0);
            IEnumerable<float> ys = xyzw.Skip(1).Where((p, i) => i % 4 == 0);
            return xs.Zip(ys, (x, y) => new PointF(x, y)).ToList();
        }

        public List<PointF> XyzwToPointFList(Span<float> xyzw)
        {
            int length = xyzw.Length;
            int cnt = length / 4;
            List<PointF> result = new();

            for (int i = 0; i < length; i += 4)
            {
                float x = xyzw[i];
                float y = xyzw[i + 1];
                //float z = xyzw[i+2];
                //float w = xyzw[i+3];
                result.Add(new PointF(x, y));
            }
            return result;
        }

        public List<PointF> XyzwToPointFList(float[] xyzw)
        {
            int length = xyzw.Length;
            int cnt = length / 4;
            List<PointF> result = new();

            for (int i = 0; i < length; i += 4)
            {
                float x = xyzw[i];
                float y = xyzw[i + 1];
                //float z = xyzw[i+2];
                //float w = xyzw[i+3];
                result.Add(new PointF(x, y));
            }
            return result;
        }

        public Tuple<List<PointF>, double[], double[]> XyzwToPointFTuple(Span<float> xyzw)
        {
            int length = xyzw.Length;
            int cnt = length / 4;
            List<PointF> result = new();
            double[] ys = new double[cnt];
            double[] xs = new double[cnt];
            int count = 0;
            for (int i = 0; i < length; i += 4, count++)
            {
                float x = xyzw[i];
                float y = xyzw[i + 1];
                //float z = xyzw[i+2];
                //float w = xyzw[i+3];
                result.Add(new PointF(x, y));
                ys[count] = y;
                xs[count] = x;
            }
            return Tuple.Create(result, xs, ys);
        }

        public List<PointF> DiscretizedProfileToPoints(float[] xy, float leftBorder, float discrete)
        {

            List<PointF> result = new();
            foreach (var item in xy)
            {

                result.Add(new(leftBorder, item));
                leftBorder += discrete;
            }
            return result.OrderBy(p => p.X).ToList();
        }

        public Span<PointF> XyzwToPointFSpan(Span<float> xyzw)
        {
            if (xyzw == null || xyzw.Length < 4) return Span<PointF>.Empty;
            int length = xyzw.Length;
            int cnt = length / 4;
            var array = new PointF[cnt];
            var result = new Span<PointF>(array);
            for (int i = 0, j = 0; i < length; i += 4, j++)
            {
                float x = xyzw[i];
                float y = xyzw[i + 1];
                //float z = xyzw[i+2];
                //float w = xyzw[i+3];
                result[j] = new PointF(x, y);
            }
            return result;
        }
    }
}
