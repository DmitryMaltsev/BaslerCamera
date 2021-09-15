using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kogerent.Services.Interfaces;

using MathNet.Numerics;

namespace Kogerent.Services.Implementation
{
    public class NumericService : INumericService
    {
        public NumericService(ISensorRepository sensorRepository)
        {
            SensorRepository = sensorRepository;
        }

        public ISensorRepository SensorRepository { get; }

        /// <summary>
        /// Находит угол отклонения от горизонтали в градусах
        /// </summary>
        public void ExecuteFindAngle()
        {
            if (SensorRepository.CurrentSensor.ProfilePoints.Count <= 0) return;

            var list = SensorRepository.CurrentSensor.ProfilePoints;

            double[] ys = list.Select(p => (double)p.Y).ToArray();
            double[] xs = list.Select(p => (double)p.X).ToArray();

            Tuple<double, double> t = Fit.Line(xs, ys);
            double k = t.Item2;
            double b = t.Item1;

            var radAngle = Math.Atan(k);

            var degrees = Trig.RadianToDegree(radAngle);

            SensorRepository.CurrentSensor.AngleCorrection += 0 - (float)degrees;
        }

        /// <summary>
        /// Находит сдвиг по вертикали от желаемого в мм
        /// </summary>
        /// <param name="desiredDistance"></param>
        public void ExecuteFindDistance(float desiredDistance)
        {
            if (SensorRepository.CurrentSensor.ProfilePoints.Count <= 0) return;

            var list = SensorRepository.CurrentSensor.ProfilePoints;

            var average = list.Average(p => p.Y);

            var shift = desiredDistance - average;

            SensorRepository.CurrentSensor.ShiftDCorrection += shift;
        }
    }
}
