
using Kogerent.Collections;
using Kogerent.Services.Interfaces;

using MathNet.Numerics;

using System;
using System.Linq;

namespace Kogerent.Services.Implementation
{
    public class CalibrateService : ICalibrateService
    {
        public (double[], int[]) Calibrate(byte[] data)
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
            var deltas = new int[data.Length];
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
                deltas[i] = (int)(p0 + p1 + p2 + p3);
            }
            return (p, deltas);
        }


        public double[] CalibrateRaw(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException();
            }
            if (data.Length <= 0)
            {
                throw new ArgumentException();
            }
            double[] raw = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                raw[i] = 127 / (double)data[i];
            }
            return raw;
        }

        public (int, double) CalibrateExposureTimeRaw(byte[] data, int currentExposure)
        {
            double averageByte = data.Average();
            if (averageByte < 90) currentExposure += 500;
            else
            if (averageByte > 160) currentExposure = currentExposure -= 500;
            return (currentExposure, averageByte);
        }

        public sbyte[] DefaultCalibration(double[] p, int count)
        {
            sbyte[] result = new sbyte[count];
            for (int i = 0; i < count; i++)
            {
                double p0 = p[0];
                double p1 = p[1] * i;
                double p2 = p[2] * Math.Pow(i, 2);
                double p3 = p[3] * Math.Pow(i, 3);
                result[i] = (sbyte)(p0 + p1 + p2 + p3);

            }
            return result;
        }
    }
}
