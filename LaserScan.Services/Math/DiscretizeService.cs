using Kogerent.Core;
using Kogerent.Services.Interfaces;
using MathNet.Numerics;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Kogerent.Services.Implementation
{
    /// <summary>
    /// Служба дискретизации массивов
    /// </summary>
    public class DiscretizeService : IDiscretizeService
    {
        private ObjectPool<List<PointF>> _pool = ObjectPool.Create<List<PointF>>();

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
        /// <example>
        /// Пример вызова кода
        /// <code>
        /// float leftBorder = 0;
        /// if (CurrentSensor == SensorRepository.Sensors[0] ||
        ///     CurrentSensor == SensorRepository.Sensors[1])
        ///     leftBorder = Math.Max(CurrentSensor.LeftBorder, profile[0].X);
        /// else
        ///     leftBorder = CurrentSensor.LeftBorder;
        /// List&lt;PointF&gt; discretized = DiscretizeWithSubstitution(profile, CurrentSensor.DiscretizeStep, leftBorder, CurrentSensor.RightBorder);
        /// </code>
        /// </example>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">"Правая граница должна быть больше левой границы хотя бы на один дискрет"</exception>
        /// </list>
        /// <returns>Массив дискретизированных точек</returns>
        public List<PointF> DiscretizeWithSubstitution(List<PointF> source, float discrete, float leftBorder, float rightBorder, bool substitute = true)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), "Входный массив не может быть нулл.");
            }
            if (discrete <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discrete), "дискрет должен быть больше нуля");
            }
            if (rightBorder - leftBorder < discrete)
            {
                throw new ArgumentException("Правая граница должна быть больше левой границы хотя бы на один дискрет");
            }


            double[] ys = source.Select(p => (double)p.Y).ToArray();
            double[] xs = source.Select(p => (double)p.X).ToArray();
            Tuple<double, double> t = Fit.Line(xs, ys);
            float k = (float)t.Item2;
            float b = (float)t.Item1;

            float end = leftBorder + discrete; int index = 0;
            int count = source.Count;
            var tempList = new List<PointF>();
            var finalList = new List<PointF>();
            for (float x = leftBorder; x < rightBorder; x += discrete)
            {
                tempList.Clear();
                for (int i = index; i < count; i++)
                {
                    if (source[i].X >= x + discrete)
                    {
                        index = i;
                        break;
                    }
                    if (source[i].X >= x && source[i].X < x + discrete)
                    {
                        tempList.Add(source[i]);
                    }
                }
                if (tempList.Count > 0)
                {
                    var average = tempList.Average(p => p.Y);
                    finalList.Add(new PointF(x, average));
                }
                else
                {
                    if (substitute)
                    {
                        var point = new PointF(x, k * x + b);
                        finalList.Add(point);
                    }
                }
            }
            return finalList.OrderBy(p => p.X).ToList();
        }


        /// <summary>
        /// Дискретизирует массив от левой границы до правой без подстановки начений
        /// </summary> 
        /// <remarks>
        /// Коррекция высоты происходит по формуле y=k*x+b
        /// </remarks>
        /// <param name="source">Исходная коллекция точек</param>
        /// <param name="kCoef">Тангенс угла наклона</param>
        /// <param name="bCoef">Коэффициент смещения</param>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <returns>Массив дискретизированных точек</returns>
        public List<IntXFloatYPoint> DiscretizeIntXFloatYPointsSimple(List<IntXFloatYPoint> source, float kCoef = 0, float bCoef = 0)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            ParallelQuery<IntXFloatYPoint> result = source.GroupBy(p => p.X).AsParallel().Select(g => new IntXFloatYPoint(g.Key, g.Average(p => p.Y)));

            return result.OrderBy(p => p.X).ToList();
        }

        public List<IntXFloatYPoint> DiscretizeIntXFloatYMagnet(List<IntXFloatYPoint> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), "Входный массив не может быть нулл.");
            }

            double[] ys = source.Select(p => (double)p.Y).ToArray();
            double[] xs = source.Select(p => (double)p.X).ToArray();
            Tuple<double, double> t = Fit.Line(xs, ys);
            float k = (float)t.Item2;
            float b = (float)t.Item1;

            return source.GroupBy(p => p.X).Select(g =>
               {
                   float y = g.Average(p => p.Y);
                   float avgY = (k * g.Key) + b;
                   if (Math.Abs(y - avgY) > 1)
                   {
                       y = avgY;
                   }
                   return new IntXFloatYPoint(g.Key, y);
               }).ToList();
        }

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
        public List<PointF> DiscretizeWithSubstitutionAndCorrection(List<PointF> source, float discrete, float leftBorder,
                                                                    float rightBorder, float kCoef, float bCoef, bool substitute = true)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source), "Входный массив не может быть нулл.");
            }
            if (discrete <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discrete), "дискрет должен быть больше нуля");
            }
            if (rightBorder - leftBorder < discrete)
            {
                throw new ArgumentException("Правая граница должна быть больше левой границы хотя бы на один дискрет");
            }
            double[] ys = source.Select(p => (double)p.Y).ToArray();
            double[] xs = source.Select(p => (double)p.X).ToArray();
            Tuple<double, double> t = Fit.Line(xs, ys);
            float k = (float)t.Item2;
            float b = (float)t.Item1;

            float end = leftBorder + discrete; int index = 0;
            int count = source.Count;
            var tempList = new List<PointF>();
            var finalList = new List<PointF>();
            for (float x = leftBorder; x < rightBorder; x += discrete)
            {
                tempList.Clear();
                for (int i = index; i < count; i++)
                {
                    if (source[i].X >= x + discrete)
                    {
                        index = i;
                        break;
                    }
                    if (source[i].X >= x && source[i].X < x + discrete)
                    {
                        tempList.Add(source[i]);
                    }
                }
                if (tempList.Count > 0)
                {
                    var average = tempList.Average(p => p.Y);
                    var newY = average - (kCoef * average + bCoef);
                    finalList.Add(new PointF(x, newY));
                }
                else
                {
                    if (substitute)
                    {
                        var avg = k * x + b;
                        var newY = avg - (kCoef * avg + bCoef);
                        var point = new PointF(x, newY);
                        finalList.Add(point);
                    }
                }
            }
            return finalList.OrderBy(p => p.X).ToList();
        }

        /// <summary>
        /// Дискретизирует массив от левой границы до правой 
        /// </summary>
        /// <param name="xyzw"></param>
        /// <param name="discrete"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <param name="zone1Left"></param>
        /// <param name="zone1Right"></param>
        /// <param name="zone2Left"></param>
        /// <param name="zone2Right"></param>
        /// <param name="finalList"></param>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">Правая граница должна быть больше левой границы хотя бы на один дискрет</exception>
        /// <exception cref="System.ArgumentException">Граница zone1Right должна быть больше границы zone1Left хотя бы на один дискрет</exception>
        /// <exception cref="System.ArgumentException">Граница zone2Right должна быть больше границы zone2Left хотя бы на один дискрет</exception>
        /// </list>
        /// <returns>массив дискретизированных точек out finalList</returns>
        public void Discretize(float[] xyzw, float discrete, float leftBorder, float rightBorder, float zone1Left,
                            float zone1Right, float zone2Left, float zone2Right, out List<PointF> finalList)
        {
            if (xyzw is null)
            {
                throw new ArgumentNullException(nameof(xyzw), "Входный массив не может быть нулл.");
            }
            if (discrete <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discrete), "дискрет должен быть больше нуля");
            }
            if (rightBorder - leftBorder < discrete)
            {
                throw new ArgumentException("Правая граница должна быть больше левой границы хотя бы на один дискрет");
            }
            if (zone1Right - zone1Left < discrete)
            {
                throw new ArgumentException("zone1Right должна быть больше zone1Left хотя бы на один дискрет");
            }
            if (zone2Right - zone2Left < discrete)
            {
                throw new ArgumentException("zone2Right должна быть больше zone2Left хотя бы на один дискрет");
            }
            var list = new List<PointF>();
            var tempList = new List<PointF>();
            finalList = new List<PointF>();
            for (int i = 0; i < xyzw.Length; i += 4)
            {
                var x = xyzw[i];
                var y = xyzw[i + 1];
                if (x >= leftBorder && x < rightBorder)
                    list.Add(new PointF(x, y));
            }
            var orderedList = list.OrderBy(x => x.X).ToList();
            if (orderedList.Count == 0) return;
            var firstPoint = orderedList.First();

            float start = (float)Math.Round(leftBorder, 1);

            float end = (float)Math.Round(start + discrete, 1);

            for (int i = 0; i < orderedList.Count; i++)
            {
                PointF point = orderedList[i];
                //если точка в дискрете, то добавляем в лист
                if (point.X >= start && point.X < end)
                {
                    tempList.Add(point);
                    continue;
                }
                else
                {
                    if (finalList.Count == 0)
                    {
                        while (!(point.X >= start && point.X < end))
                        {
                            start = (float)Math.Round(start + discrete, 1);
                            end = (float)Math.Round(end + discrete, 1);

                            if (point.X >= start && point.X < end) break;

                        }
                        //tempList.Add(point);
                        finalList.Add(point);
                        continue;
                    }
                }

                if (tempList.Count == 0)
                {
                    var lastPoint = finalList.Last();
                    //двигаем дискрет, пока точка не окажется в дискрете
                    while (!(point.X >= start && point.X < end))
                    {
                        start = (float)Math.Round(start + discrete, 1);
                        end = (float)Math.Round(end + discrete, 1);

                        if (point.X >= start && point.X < end) break;

                        GetLine(lastPoint.X, lastPoint.Y, point.X, point.Y, out float k, out float b);
                        float aprY = k * start + b;
                        finalList.Add(new PointF(start, aprY));
                    }
                    tempList.Add(point);
                }
                else
                {
                    float avg = tempList.Average(x => x.Y);
                    PointF lastPoint = new(start, avg);
                    finalList.Add(lastPoint);
                    tempList.Clear();
                }
            }
            if (zone1Left > 0 && zone1Right > zone1Left)
                _ = finalList.RemoveAll(x => x.X >= zone1Left && x.X <= zone1Right);
            if (zone2Left > 0 && zone2Right > zone2Left)
                _ = finalList.RemoveAll(x => x.X >= zone2Left && x.X <= zone2Right);
        }

        /// <summary>
        /// Асинхронно дискретизирует массив от левой границы до правой 
        /// </summary>
        /// <Remarks>
        /// Так как возвращается Task, для неблокирующего ожидания, требуется вызывать этот метод из асинхронного метода.
        /// </Remarks>
        /// <param name="xyzw"></param>
        /// <param name="discrete"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <param name="zone1Left"></param>
        /// <param name="zone1Right"></param>
        /// <param name="zone2Left"></param>
        /// <param name="zone2Right"></param>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">Правая граница должна быть больше левой границы хотя бы на один дискрет</exception>
        /// <exception cref="System.ArgumentException">Граница zone1Right должна быть больше границы zone1Left хотя бы на один дискрет</exception>
        /// <exception cref="System.ArgumentException">Граница zone2Right должна быть больше границы zone2Left хотя бы на один дискрет</exception>
        /// </list>
        /// <returns>массив дискретизированных точек</returns>
        public Task<List<PointF>> DiscretizeAsyncMean(float[] xyzw, float discrete, float leftBorder, float rightBorder, float zone1Left,
                                    float zone1Right, float zone2Left, float zone2Right)
        {
            if (xyzw is null)
            {
                throw new ArgumentNullException(nameof(xyzw), "Входный массив не может быть нулл.");
            }
            if (discrete <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discrete), "дискрет должен быть больше нуля");
            }
            if (rightBorder - leftBorder < discrete)
            {
                throw new ArgumentException("Правая граница должна быть больше левой границы хотя бы на один дискрет");
            }
            if (zone1Right - zone1Left < discrete)
            {
                throw new ArgumentException("zone1Right должна быть больше zone1Left хотя бы на один дискрет");
            }
            if (zone2Right - zone2Left < discrete)
            {
                throw new ArgumentException("zone2Right должна быть больше zone2Left хотя бы на один дискрет");
            }
            return Task.Run(() =>
            {
                var list = new List<PointF>();
                var tempList = new List<PointF>();
                var finalList = new List<PointF>();

                for (int i = 0; i < xyzw.Length; i += 4)
                {
                    var x = xyzw[i];
                    var y = xyzw[i + 1];
                    if (x >= leftBorder && x < rightBorder)
                        list.Add(new PointF(x, y));
                }

                var orderedList = list.OrderBy(x => x.X).ToList();
                if (orderedList.Count == 0) return null;
                var firstPoint = orderedList.First();

                float start = (float)Math.Round(leftBorder, 1);

                float end = (float)Math.Round(start + discrete, 1);

                for (int i = 0; i < orderedList.Count; i++)
                {
                    PointF point = orderedList[i];
                    //если точка в дискрете, то добавляем в лист
                    if (point.X >= start && point.X < end)
                    {
                        tempList.Add(point);
                        continue;
                    }
                    else
                    {
                        if (finalList.Count == 0)
                        {
                            while (!(point.X >= start && point.X < end))
                            {
                                start = (float)Math.Round(start + discrete, 1);
                                end = (float)Math.Round(end + discrete, 1);

                                if (point.X >= start && point.X < end) break;

                            }
                            //tempList.Add(point);
                            finalList.Add(point);
                            continue;
                        }
                    }

                    if (tempList.Count == 0)
                    {
                        var lastPoint = finalList.Last();
                        //двигаем дискрет, пока точка не окажется в дискрете
                        while (!(point.X >= start && point.X < end))
                        {
                            start = (float)Math.Round(start + discrete, 1);
                            end = (float)Math.Round(end + discrete, 1);

                            if (point.X >= start && point.X < end) break;

                            GetLine(lastPoint.X, lastPoint.Y, point.X, point.Y, out float k, out float b);
                            float aprY = k * start + b;
                            finalList.Add(new PointF(start, aprY));
                        }
                        tempList.Add(point);
                    }
                    else
                    {
                        float avg = tempList.Average(x => x.Y);
                        PointF lastPoint = new(start, avg);
                        finalList.Add(lastPoint);
                        tempList.Clear();
                    }
                }
                if (zone1Left > 0 && zone1Right > zone1Left)
                    _ = finalList.RemoveAll(x => x.X >= zone1Left && x.X <= zone1Right);
                if (zone2Left > 0 && zone2Right > zone2Left)
                    _ = finalList.RemoveAll(x => x.X >= zone2Left && x.X <= zone2Right);

                return finalList;
            });
        }

        /// <summary>
        /// Асинхронно дискретизирует массив от левой границы до правой 
        /// </summary>
        /// <remarks>
        /// Так как возвращается Task, для неблокирующего ожидания, требуется вызывать этот метод из асинхронного метода.
        /// </remarks>
        /// <param name="list"></param>
        /// <param name="discrete"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">Правая граница должна быть больше левой границы хотя бы на один дискрет</exception>
        /// </list>
        /// <returns>массив дискретизированных точек</returns>
        public Task<List<PointF>> DiscretizeMeanAsync(IEnumerable<PointF> list, float discrete, float leftBorder, float rightBorder = 0)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list), "Входный массив не может быть нулл.");
            }
            if (discrete <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discrete), "дискрет должен быть больше нуля");
            }
            if (rightBorder - leftBorder < discrete)
            {
                throw new ArgumentException("Правая граница должна быть больше левой границы хотя бы на один дискрет");
            }
            return Task.Run(() =>
            {
                var tempList = new List<PointF>();
                var finalList = new List<PointF>();

                float end = leftBorder + discrete;

                foreach (PointF point in list)
                {
                    //если точка в дискрете, то добавляем в буфер
                    if (point.X >= leftBorder && point.X < end)
                    {
                        tempList.Add(point);
                        continue;
                    }

                    if (tempList.Count > 0)
                    {
                        float avg = tempList.Average(x => x.Y);
                        finalList.Add(new PointF { X = (float)Math.Round(leftBorder, 2), Y = avg });
                        var lastPoint = tempList.Last();
                        tempList.Clear();

                        //двигаем дискрет, пока точка не окажется в дискрете
                        while (!(point.X >= leftBorder && point.X < end))
                        {
                            leftBorder += discrete;
                            end += discrete;

                            if (point.X >= leftBorder && point.X < end) break;

                            GetLine(lastPoint.X, lastPoint.Y, point.X, point.Y, out float k, out float b);
                            float aprY = k * leftBorder + b;
                            finalList.Add(new PointF { X = (float)Math.Round(leftBorder, 2), Y = aprY });
                        }
                    }
                    else
                    {
                        //двигаем дискрет, пока точка не окажется в дискрете
                        while (!(point.X >= leftBorder && point.X < end))
                        {
                            leftBorder += discrete;
                            end += discrete;

                            if (point.X >= leftBorder && point.X < end) break;
                        }
                    }

                    tempList.Add(point);
                }

                if (rightBorder > 0) _ = finalList.RemoveAll(point => point.X > rightBorder);

                return finalList;
            });
        }

        /// <summary>
        /// Дискретизирует массив от левой границы до правой 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="discrete"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">Правая граница должна быть больше левой границы хотя бы на один дискрет</exception>
        /// </list>
        /// <returns>массив дискретизированных точек</returns>
        public List<PointF> DiscretizeMean(IEnumerable<PointF> list, float discrete, float leftBorder, float rightBorder)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list), "Входный массив не может быть нулл.");
            }
            if (discrete <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discrete), "дискрет должен быть больше нуля");
            }
            if (rightBorder - leftBorder < discrete)
            {
                throw new ArgumentException("Правая граница должна быть больше левой границы хотя бы на один дискрет");
            }
            var tempList = new List<PointF>();
            var finalList = new List<PointF>();

            float end = leftBorder + discrete;

            foreach (PointF point in list)
            {
                //если точка в дискрете, то добавляем в буфер
                if (point.X >= leftBorder && point.X < end)
                {
                    tempList.Add(point);
                    continue;
                }

                if (tempList.Count > 0)
                {
                    float avg = tempList.Average(x => x.Y);
                    finalList.Add(new PointF { X = (float)Math.Round(leftBorder, 2), Y = avg });
                    var lastPoint = tempList.Last();
                    tempList.Clear();

                    //двигаем дискрет, пока точка не окажется в дискрете
                    while (!(point.X >= leftBorder && point.X < end) && end <= rightBorder)
                    {
                        leftBorder += discrete;
                        end += discrete;

                        if (point.X >= leftBorder && point.X < end) break;

                        GetLine(lastPoint.X, lastPoint.Y, point.X, point.Y, out float k, out float b);
                        float aprY = k * leftBorder + b;
                        finalList.Add(new PointF { X = (float)Math.Round(leftBorder, 2), Y = aprY });
                    }
                }
                else
                {
                    //двигаем дискрет, пока точка не окажется в дискрете
                    while (!(point.X >= leftBorder && point.X < end) && end <= rightBorder)
                    {
                        leftBorder += discrete;
                        end += discrete;

                        if (point.X >= leftBorder && point.X < end) break;
                    }
                }

                tempList.Add(point);
            }

            return finalList;
        }

        /// <summary>
        /// Дискретизирует массив от левой границы до правой 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="discrete"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">Правая граница должна быть больше левой границы хотя бы на один дискрет</exception>
        /// </list>
        /// <returns>массив дискретизированных точек</returns>
        public List<PointF> DiscretizeMean(List<PointF> list, float discrete, float leftBorder, float rightBorder = 0)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list), "Входный массив не может быть нулл.");
            }
            if (discrete <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discrete), "дискрет должен быть больше нуля");
            }
            if (rightBorder - leftBorder < discrete)
            {
                throw new ArgumentException("Правая граница должна быть больше левой границы хотя бы на один дискрет");
            }
            list.RemoveAll(p => p.X < leftBorder);
            var tempList = new List<PointF>();
            var finalList = new List<PointF>();

            float end = leftBorder + discrete;

            foreach (PointF point in list)
            {
                //если точка в дискрете, то добавляем в буфер
                if (point.X >= leftBorder && point.X < end)
                {
                    tempList.Add(point);
                    continue;
                }

                if (tempList.Count > 0)
                {
                    float avg = tempList.Average(x => x.Y);
                    finalList.Add(new PointF { X = (float)Math.Round(leftBorder, 2), Y = avg });
                    var lastPoint = tempList.Last();
                    tempList.Clear();

                    //двигаем дискрет, пока точка не окажется в дискрете
                    while (!(point.X >= leftBorder && point.X < end))
                    {
                        leftBorder += discrete;
                        end += discrete;

                        if (end > rightBorder) break;
                        if (point.X >= leftBorder && point.X < end) break;

                        GetLine(lastPoint.X, lastPoint.Y, point.X, point.Y, out float k, out float b);
                        float aprY = k * leftBorder + b;
                        finalList.Add(new PointF { X = (float)Math.Round(leftBorder, 2), Y = aprY });
                    }
                }
                else
                {
                    //двигаем дискрет, пока точка не окажется в дискрете
                    while (!(point.X >= leftBorder && point.X < end))
                    {
                        leftBorder += discrete;
                        end += discrete;

                        if (end > rightBorder) break;
                        if (point.X >= leftBorder && point.X < end) break;
                    }
                }

                tempList.Add(point);
            }

            if (rightBorder > leftBorder) _ = finalList.RemoveAll(point => point.X > rightBorder);

            return finalList;
        }

        /// <summary>
        /// Дискретизирует массив от левой границы до правой 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="discrete"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <param name="finalList"></param>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">Правая граница должна быть больше левой границы хотя бы на один дискрет</exception>
        /// </list>
        /// <returns>массив дискретизированных точек</returns>
        public void DiscretizeSimple(List<PointF> list, List<PointF> finalList, float discrete, float leftBorder, float rightBorder = 0)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list), "Входный массив не может быть нулл.");
            }
            if (discrete <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discrete), "дискрет должен быть больше нуля");
            }
            if (rightBorder - leftBorder < discrete)
            {
                throw new ArgumentException("Правая граница должна быть больше левой границы хотя бы на один дискрет");
            }
            var tempList = _pool.Get();
            tempList.Clear(); finalList.Clear();
            float end = leftBorder + discrete;

            foreach (PointF point in list)
            {
                //если точка в дискрете, то добавляем в буфер
                if (point.X >= leftBorder && point.X < end)
                {
                    tempList.Add(point);
                    continue;
                }

                if (tempList.Count > 0)
                {
                    float avg = tempList.Average(x => x.Y);
                    finalList.Add(new PointF { X = (float)Math.Round(leftBorder, 2), Y = avg });
                    tempList.Clear();
                    continue;
                }
                else
                {
                    //двигаем дискрет, пока точка не окажется в дискрете
                    while (!(point.X >= leftBorder && point.X < end))
                    {
                        leftBorder += discrete;
                        end += discrete;

                        if (point.X >= leftBorder && point.X < end) break;
                    }
                }
                tempList.Add(point);
            }
            if (rightBorder > 0) _ = finalList.RemoveAll(point => point.X > rightBorder);
            _pool.Return(tempList);
        } //требуется уточнение

        /// <summary>
        /// Дискретизирует массив от левой границы до правой 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="discrete"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть пустым</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">Правая граница должна быть больше левой границы хотя бы на один дискрет</exception>
        /// </list>
        /// <returns>массив дискретизированных точек</returns>
        public List<PointF> DiscretizeSimple(Span<PointF> list, float discrete, float leftBorder, float rightBorder = 0)
        {
            if (list.IsEmpty)
            {
                throw new ArgumentNullException(nameof(list), "Входный массив не может быть пустым.");
            }
            if (discrete <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discrete), "дискрет должен быть больше нуля");
            }
            if (rightBorder - leftBorder < discrete)
            {
                throw new ArgumentException("Правая граница должна быть больше левой границы хотя бы на один дискрет");
            }
            var tempList = _pool.Get();
            tempList.Clear();
            var finalList = new List<PointF>();

            float end = leftBorder + discrete;

            foreach (PointF point in list)
            {
                //если точка в дискрете, то добавляем в буфер
                if (point.X >= leftBorder && point.X < end)
                {
                    tempList.Add(point);
                    continue;
                }

                if (tempList.Count > 0)
                {
                    float avg = tempList.Average(x => x.Y);
                    finalList.Add(new PointF { X = (float)Math.Round(leftBorder, 2), Y = avg });
                    tempList.Clear();
                    continue;
                }
                else
                {
                    //двигаем дискрет, пока точка не окажется в дискрете
                    while (!(point.X >= leftBorder && point.X < end))
                    {
                        leftBorder += discrete;
                        end += discrete;

                        if (point.X >= leftBorder && point.X < end) break;
                    }
                }

                tempList.Add(point);
            }

            if (rightBorder > 0) _ = finalList.RemoveAll(point => point.X > rightBorder);

            _pool.Return(tempList);

            return finalList;
        }

        /// <summary>
        /// Дискретизирует массив от левой границы до правой 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="discrete"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <list>
        /// <exception cref="System.ArgumentNullException">Входящий массив не может быть нулл</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Дискрет не может быть меньше нуля</exception>
        /// <exception cref="System.ArgumentException">Правая граница должна быть больше левой границы хотя бы на один дискрет</exception>
        /// </list>
        /// <returns>массив дискретизированных точек</returns>
        public List<PointF> DiscretizeSimple(List<PointF> list, float discrete, float leftBorder, float rightBorder = 0)
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list), "Входный массив не может быть нулл.");
            }
            if (discrete <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(discrete), "дискрет должен быть больше нуля");
            }
            if (rightBorder - leftBorder < discrete)
            {
                throw new ArgumentException("Правая граница должна быть больше левой границы хотя бы на один дискрет");
            }
            //var tempList = DefectRepository.ListPool.Get();
            list.RemoveAll(p => p.X < leftBorder);
            var tempList = new List<PointF>();
            var finalList = new List<PointF>();

            float end = leftBorder + discrete;

            foreach (PointF point in list)
            {
                //если точка в дискрете, то добавляем в буфер
                if (point.X >= leftBorder && point.X < end)
                {
                    tempList.Add(point);
                    continue;
                }

                if (tempList.Count > 0)
                {
                    float avg = tempList.Average(x => x.Y);
                    finalList.Add(new PointF { X = (float)Math.Round(leftBorder, 2), Y = avg });
                    tempList.Clear();
                    continue;
                }
                else
                {
                    //двигаем дискрет, пока точка не окажется в дискрете
                    while (!(point.X >= leftBorder && point.X < end))
                    {
                        leftBorder += discrete;
                        end += discrete;
                        if (end > rightBorder) break;

                        if (point.X >= leftBorder && point.X < end) break;
                    }
                }

                tempList.Add(point);
            }

            if (rightBorder > leftBorder) _ = finalList.RemoveAll(point => point.X > rightBorder);

            //DefectRepository.ListPool.Return(tempList);

            return finalList;
        }


        private void GetLine(float x1, float y1, float x2, float y2, out float k, out float b)
        {
            k = (y2 - y1) / (x2 - x1);
            b = (x2 * y1 - x1 * y2) / (x2 - x1);
        }

    }
}
