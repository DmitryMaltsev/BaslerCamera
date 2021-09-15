using Kogerent.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Kogerent.Services.Interfaces
{
    public interface IDiscretizeService
    {
        void Discretize(float[] xyzw, float discrete, float leftBorder, float rightBorder, float zone1Left, float zone1Right, float zone2Left, float zone2Right, out List<PointF> finalList);

        Task<List<PointF>> DiscretizeAsyncMean(float[] xyzw, float discrete, float leftBorder, float rightBorder, float zone1Left, float zone1Right, float zone2Left, float zone2Right);
        List<IntXFloatYPoint> DiscretizeIntXFloatYMagnet(List<IntXFloatYPoint> source);
        List<IntXFloatYPoint> DiscretizeIntXFloatYPointsSimple(List<IntXFloatYPoint> source, float kCoef = 0, float bCoef = 0);
        List<PointF> DiscretizeMean(IEnumerable<PointF> list, float discrete, float leftBorder, float rightBorder);

        List<PointF> DiscretizeMean(List<PointF> list, float discrete, float leftBorder, float rightBorder = 0);

        Task<List<PointF>> DiscretizeMeanAsync(IEnumerable<PointF> list, float discrete, float leftBorder, float rightBorder = 0);

        void DiscretizeSimple(List<PointF> list, List<PointF> finalList, float discrete, float leftBorder, float rightBorder = 0);

        List<PointF> DiscretizeSimple(Span<PointF> list, float discrete, float leftBorder, float rightBorder = 0);

        List<PointF> DiscretizeSimple(List<PointF> list, float discrete, float leftBorder, float rightBorder = 0);

        /// <summary>
        /// Дискретизирует массив от левой границы до правой с подстановкой значений
        /// </summary>
        /// <remarks>
        /// Значение подстанавливаются по формуле линейной регрессии k*x+b. Кооэффициенты получаются методом вписывания точек в прямую.
        /// </remarks>
        /// <param name="source">Исходная коллекция точек</param>
        /// <param name="discrete">Шаг дискретизации</param>
        /// <param name="leftBorder">Левая граница</param>
        /// <param name="rightBorder">правая граница</param>
        /// <param name="substitute">флаг заполнения пустого дискрета (по умолчанию=true)</param>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">"Правая граница должна быть больше левой границы хотя бы на один дискрет"</exception>
        /// </list>
        /// <returns>Массив дискретизированных точек</returns>
        List<PointF> DiscretizeWithSubstitution(List<PointF> source, float discrete, float leftBorder, float rightBorder, bool substitute = true);

        /// <summary>
        ///  Дискретизирует массив от левой границы до правой с подстановкой значений и коррекцией высоты на величину отклонения
        ///  <para>Значение подстанавливаются по формуле линейной регрессии k*x+b. Кооэффициенты получаются методом вписывания точек в прямую.</para>
        ///  <para>Отклонения рассчитываются по формуле линейной регрессии k*x+b.</para>
        /// </summary>
        /// <param name="source">Исходная коллекция точек</param>
        /// <param name="discrete">Шаг дискретизации</param>
        /// <param name="leftBorder">Левая граница</param>
        /// <param name="rightBorder">правая граница</param>
        /// <param name="kCoef">коэффициент наклона</param>
        /// <param name="bCoef">коэффициент смещения прямой</param>
        /// <param name="substitute">флаг заполнения пустого дискрета (по умолчанию=true)</param>
        ///  /// <example>
        /// Пример вызова кода
        /// <code>
        /// double[] xs = new double[] { 20d, 30d, 100d, 120d, 180d };
        /// double[] ys = new double[] { 0.38, 0.37, 0.02, -0.15, -0.55 };
        /// 
        /// //Вписываем точки в прямую, чтоб получить коэффициенты.
        /// Tuple&lt;double, double&gt; t = Fit.Line(xs, ys);
        /// _k = (float) t.Item2;
        /// _b = (float) t.Item1;
        /// 
        /// float leftBorder = 0;
        /// if (CurrentSensor == SensorRepository.Sensors[0] ||
        ///     CurrentSensor == SensorRepository.Sensors[1])
        ///     leftBorder = Math.Max(CurrentSensor.LeftBorder, profile[0].X);
        /// else
        ///     leftBorder = CurrentSensor.LeftBorder;
        /// List&lt;PointF&gt; discretized = DiscretizeWithSubstitutionAndCorrection(profile, CurrentSensor.DiscretizeStep, leftBorder, CurrentSensor.RightBorder, _k, _b);
        /// </code>
        /// </example>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">"Правая граница должна быть больше левой границы хотя бы на один дискрет"</exception>
        /// </list>
        /// <returns>массив дискретезированных точек</returns>
        List<PointF> DiscretizeWithSubstitutionAndCorrection(List<PointF> source, float discrete, float leftBorder, float rightBorder, float kCoef, float bCoef, bool substitute = true);
    }
}
