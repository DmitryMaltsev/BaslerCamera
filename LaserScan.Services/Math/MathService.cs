using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

using Kogerent.Core;
using Kogerent.Logger;
using Kogerent.Services.Interfaces;

using MathNet.Numerics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Kogerent.Services.Implementation
{
    public class MathService : IMathService
    {
        public IDiscretizeService DiscretizeService { get; }
        public IPointService PointService { get; }
        public IImageProcessingService ImageProcessingService { get; }
        public INumericService NumericService { get; }

        public MathService(IDiscretizeService discretizeService, IPointService pointService, IImageProcessingService imageProcessingService, INumericService numericService)
        {
            DiscretizeService = discretizeService;
            PointService = pointService;
            ImageProcessingService = imageProcessingService;
            NumericService = numericService;
        }

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
        public List<PointF> DiscretizeWithSubstitution(List<PointF> source, float discrete, float leftBorder, float rightBorder, bool substitute = true) =>
            DiscretizeService.DiscretizeWithSubstitution(source, discrete, leftBorder, rightBorder, substitute);

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
        public List<PointF> DiscretizeWithSubstitutionAndCorrection(List<PointF> source, float discrete, float leftBorder, float rightBorder, float kCoef, float bCoef, bool substitute = true) =>
            DiscretizeService.DiscretizeWithSubstitutionAndCorrection(source, discrete, leftBorder, rightBorder, kCoef, bCoef, substitute);

        public List<PointF> XyzwToPointF(float[] xyzw) => PointService.XyzwToPointF(xyzw);

        public Task<IEnumerable<PointF>> XyzwToPointFAsync(float[] xyzw) => PointService.XyzwToPointFAsync(xyzw);

        public List<PointF> XyzwToPointFSimple(float[] xyzw) => PointService.XyzwToPointFSimple(xyzw);

        public List<PointF> XyzwToPointFList(Span<float> xyzw) => PointService.XyzwToPointFList(xyzw);

        public List<PointF> XyzwToPointFList(float[] xyzw) => PointService.XyzwToPointFList(xyzw);

        public Tuple<List<PointF>, double[], double[]> XyzwToPointFTuple(Span<float> xyzw) => PointService.XyzwToPointFTuple(xyzw);

        public Span<PointF> XyzwToPointFSpan(Span<float> xyzw) => PointService.XyzwToPointFSpan(xyzw);

        public IEnumerable<PointF> Sum(IEnumerable<PointF> first, IEnumerable<PointF> second)
        {
            return first.Zip(second, (f, s) => new PointF(f.X, Math.Abs(f.Y - s.Y)));
        }

        public List<PointF> SumList(List<PointF> first, List<PointF> second)
        {
            var result = new List<PointF>();

            foreach (PointF topPoint in first)
            {
                PointF bottomPoint = second.FirstOrDefault(x => ApproxEquals(x.X, topPoint.X, 0.1));
                if (bottomPoint != default)
                {
                    PointF sumPoint = new() { X = topPoint.X, Y = Math.Abs(topPoint.Y - bottomPoint.Y) };
                    result.Add(sumPoint);
                }

            }
            return result;
        }

        public List<PointF> Sum(ObservableCollection<PointF> topSensorPoints, ObservableCollection<PointF> bottomSensorPoints)
        {
            var result = new List<PointF>();

            foreach (PointF topPoint in topSensorPoints)
            {
                PointF bottomPoint = bottomSensorPoints.FirstOrDefault(x => ApproxEquals(x.X, topPoint.X, 0.1));
                if (bottomPoint != default)
                {
                    PointF sumPoint = new() { X = topPoint.X, Y = Math.Abs(topPoint.Y - bottomPoint.Y) };
                    result.Add(sumPoint);
                }

            }
            return result;
        }

        //public List<PointF> Sum(List<PointF> topSensorPoints, List<PointF> bottomSensorPoints)
        //{
        //    return (from PointF topPoint in topSensorPoints
        //            let bottomPoint = bottomSensorPoints.FirstOrDefault(x => ApproxEquals(x.X, topPoint.X, 0.1))
        //            where bottomPoint != default
        //            let sumPoint = new PointF() { X = topPoint.X, Y = Math.Abs(topPoint.Y - bottomPoint.Y) }
        //            select sumPoint).ToList();
        //}

        public List<T> ConcatLists<T>(List<List<T>> list)
        {
            var result = new List<T>();
            foreach (List<T> l in list)
            {
                result.AddRange(l.ToArray());
            }
            return result;
        }

        public List<T> ConcatLists<T>(Queue<List<T>> queue)
        {
            var result = new List<T>();
            foreach (List<T> l in queue)
            {
                result.AddRange(l.ToArray());
            }
            return result;
        }

        public Image<Gray, byte> FillImageUpThreshold(float up, List<List<PointF>> sourceList, int width, int height) =>
            ImageProcessingService.FillImageUpThreshold(up, sourceList, width, height);

        public Image<Gray, byte> FillImageUpThreshold(float up, List<List<PointF>> sourceList, int width, int height, INonControlZonesRepository zones) =>
            ImageProcessingService.FillImageUpThreshold(up, sourceList, width, height, zones);

        public Image<Gray, byte> FillImageDnThreshold(float dn, List<List<PointF>> sourceList, int width, int height) =>
            ImageProcessingService.FillImageDnThreshold(dn, sourceList, width, height);

        public Image<Gray, float> ProfilesToImageUp(float up, List<IEnumerable<PointF>> sourceList, int width, int height) =>
            ImageProcessingService.ProfilesToImageUp(up, sourceList, width, height);

        public Image<Gray, float> ProfilesToImageDn(float up, List<IEnumerable<PointF>> sourceList, int width, int height) =>
            ImageProcessingService.ProfilesToImageDn(up, sourceList, width, height);

        public List<PointF> Get2DChart(List<List<PointF>> sourceList) => ImageProcessingService.Get2DChart(sourceList);

        public BitmapImage BitmapToImageSource(Bitmap bitmap) => ImageProcessingService.BitmapToImageSource(bitmap);

       // public void GetContours(Image<Gray, byte> img, out List<ContourData> result) => ImageProcessingService.GetContours(img, out result);


        public void GetContours(Image<Gray, float> img, out List<ContourData> result) => ImageProcessingService.GetContours(img, out result);


        //public Task<Bitmap> AnalyzeDefectsAsync(Image<Gray, float> imgUp, Image<Gray, float> imgDn, float widthThreshold,
        //                                        float heightThreshold, float widthDiscrete, float heightDiscrete,
        //                                        int strobe)
        //{
        //    return Task.Run(() =>
        //    {
        //        GetContours(imgUp, out List<ContourData> upContours);
        //        GetContours(imgDn, out List<ContourData> dnContours);
        //        var imgWidth = Math.Max(imgDn.Width, imgUp.Width);
        //        var imgHeight = Math.Max(imgDn.Height, imgUp.Height);
        //        var tempBmp = new Bitmap(imgWidth, imgHeight);

        //        if (upContours != null && upContours.Count > 0)
        //        {
        //            DrawDefectProperties(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete, imgWidth, imgHeight, strobe,
        //                                upContours, tempBmp, true);
        //        }

        //        if (dnContours != null && dnContours.Count > 0)
        //        {
        //            DrawDefectProperties(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete, imgWidth, imgHeight, strobe,
        //                                 dnContours, tempBmp, false);
        //        }
        //        return tempBmp;
        //    });
        //}

        public (Bitmap, IOrderedEnumerable<DefectProperties>) AnalyzeDefectsAsync(Image<Gray, byte> imgUp,
                                                                                  Image<Gray, byte> imgDn,
                                                                                  float widthThreshold,
                                                                                  float heightThreshold,
                                                                                  float widthDiscrete,
                                                                                  float heightDiscrete, int strobe) =>
        ImageProcessingService.AnalyzeDefectsAsync(imgUp, imgDn, widthThreshold, heightThreshold, widthDiscrete, heightDiscrete, strobe);

        public (Image<Bgr, byte>, IOrderedEnumerable<DefectProperties>) AnalyzeDefects(Image<Gray, byte> imgUp,
                                                                                       Image<Gray, byte> imgDn,
                                                                                       float widthThreshold,
                                                                                       float heightThreshold,
                                                                                       float widthDiscrete,
                                                                                       float heightDiscrete, int strobe, float Shift) =>
            ImageProcessingService.AnalyzeDefects(imgUp, imgDn, widthThreshold, heightThreshold, widthDiscrete, heightDiscrete, strobe,  Shift);


        public bool ApproxEquals(float a, float b, double precision)
        {
            if (Math.Abs(a - b) <= precision)
                return true;
            else
                return false;
        }

    }
}
