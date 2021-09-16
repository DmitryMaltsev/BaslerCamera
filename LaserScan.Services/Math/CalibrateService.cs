
using Kogerent.Services.Interfaces;

using MathNet.Numerics;

using System;

namespace Kogerent.Services.Implementation
{
    public class CalibrateService : ICalibrateService
    {
        public (double[], byte[]) Calibrate(byte[] data)
        {

            if (data == null)
            {
                throw new ArgumentNullException();
            }
            if (data.Length <= 0)
            {
                throw new ArgumentException();
            }
            var xs = new double[data.Length];
            var ys = new double[data.Length];
            var deltas = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                xs[i] = i;
                ys[i] = 127 - data[i];
            }

            double[] p = Fit.Polynomial(xs, ys, 3);

            for (int i = 0; i < deltas.Length; i++)
            {
                double p0 = p[0];
                double p1 = p[1] * i;
                double p2 = p[2] * Math.Pow(i, 2);
                double p3 = p[3] * Math.Pow(i, 3);
                deltas[i] = (byte)(p0 + p1 + p2 + p3);
            }
            return (p, deltas);
        }


        public byte[] DefaultCalibration(double[] p, int count)
        {
            byte[] result = new byte[count];
            for (int i = 0; i < count; i++)
            {
                double p0 = p[0];
                double p1 = p[1] * i;
                double p2 = p[2] * Math.Pow(i, 2);
                double p3 = p[3] * Math.Pow(i, 3);
                result[i] = (byte)(p0 + p1 + p2 + p3);

            }
            return result;
        }
    }
}
