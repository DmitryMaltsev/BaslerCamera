using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Emgu.CV;
using Emgu.CV.Structure;

using Kogerent.Core;

namespace Kogerent.Services.Interfaces
{
    /// <summary>
    /// Контракт на создание экземпляров сервиса мат обработки
    /// </summary>
    public interface IMathService
    {
        INumericService NumericService { get; }

        /// <summary>
        /// Анализирует дефекты на изображении
        /// </summary>
        /// <param name="imgUp">Изображение с выпуклыми дефектами</param>
        /// <param name="imgDn">Изображение со впуклыми дефектами</param>
        /// <param name="widthThreshold">Порог дефектования по ширине</param>
        /// <param name="heightThreshold">Порог дефектования по длине</param>
        /// <param name="widthDiscrete">Количество мм. в одном пикселе по ширине</param>
        /// <param name="heightDiscrete">Количество мм. в одном пикселе по длине</param>
        /// <param name="strobe">Номер строба</param>
        /// <returns>Кортеж с изображением дефектов и коллекцией дуфектов</returns>
        (Bitmap, IOrderedEnumerable<DefectProperties>) AnalyzeDefectsAsync(Image<Gray, byte> imgUp, Image<Gray, byte> imgDn, float widthThreshold, float heightThreshold, float widthDiscrete, float heightDiscrete, int strobe);

        /// <summary>
        /// Конвертирует Bitmap в BitmapImage
        /// </summary>
        /// <param name="bitmap">Картинка, которую надо сконвертировать</param>
        /// <returns>BitmapImage</returns>
        BitmapImage BitmapToImageSource(Bitmap bitmap);

        /// <summary>
        /// Создает бинарное изображение из коллекции коллекций точек
        /// </summary>
        /// <param name="dn">Порог бинаризации вниз</param>
        /// <param name="sourceList">Коллекция коллекций точек</param>
        /// <param name="width">Ширина выходного изображения</param>
        /// <param name="height">Высота выходного изображения</param>
        /// <returns>Объект типа Image с байтовой глубиной</returns>
        Image<Gray, byte> FillImageDnThreshold(float dn, List<List<PointF>> sourceList, int width, int height);

        /// <summary>
        /// Создает бинарное изображение из коллекции коллекций точек
        /// </summary>
        /// <param name="up">Порог бинаризации вверх</param>
        /// <param name="sourceList">Коллекция коллекций точек</param>
        /// <param name="width">Ширина выходного изображения</param>
        /// <param name="height">Высота выходного изображения</param>
        /// <returns>Объект типа Image с байтовой глубиной</returns>
        Image<Gray, byte> FillImageUpThreshold(float up, List<List<PointF>> sourceList, int width, int height);

        /// <summary>
        /// Создает бинарное изображение из коллекции коллекций точек
        /// </summary>
        /// <param name="dn">Порог бинаризации вниз</param>
        /// <param name="sourceList">Коллекция коллекций точек</param>
        /// <param name="width">Ширина выходного изображения</param>
        /// <param name="height">Высота выходного изображения</param>
        /// <returns>Объект типа Image с флотовой глубиной</returns>
        Image<Gray, float> ProfilesToImageDn(float dn, List<IEnumerable<PointF>> sourceList, int width, int height);

        /// <summary>
        /// Создает бинарное изображение из коллекции коллекций точек
        /// </summary>
        /// <param name="up">Порог бинаризации вверх</param>
        /// <param name="sourceList">Коллекция коллекций точек</param>
        /// <param name="width">Ширина выходного изображения</param>
        /// <param name="height">Высота выходного изображения</param>
        /// <returns>Объект типа Image с флотовой глубиной</returns>
        Image<Gray, float> ProfilesToImageUp(float up, List<IEnumerable<PointF>> sourceList, int width, int height);

        /// <summary>
        /// Поэлементно вычитает игрики верхних точек из игриков нижних точек
        /// </summary>
        /// <param name="first">Коллекция верхних точек</param>
        /// <param name="second">Коллекция нижних точек</param>
        /// <returns>Коллекция точек или нулл</returns>
        IEnumerable<PointF> Sum(IEnumerable<PointF> first, IEnumerable<PointF> second);

        /// <summary>
        /// Конвертирует xyzw массив в коллекцию точек
        /// </summary>
        /// <param name="xyzw">xyzw массив</param>
        /// <returns>Задачу с коллекцией точек</returns>
        Task<IEnumerable<PointF>> XyzwToPointFAsync(float[] xyzw);

        /// <summary>
        /// Конвертирует Span в коллекцию точек
        /// </summary>
        /// <param name="xyzw">Span массив</param>
        /// <returns>Коллекцию точек</returns>
        List<PointF> XyzwToPointFList(Span<float> xyzw);

        /// <summary>
        /// Конвертирует Span xyzw в Span точек
        /// </summary>
        /// <param name="xyzw">Span массив</param>
        /// <returns>Span точек</returns>
        Span<PointF> XyzwToPointFSpan(Span<float> xyzw);

        /// <summary>
        /// Конвертирует xyzw массив в коллекцию точек
        /// </summary>
        /// <param name="xyzw">xyzw массив</param>
        /// <returns>Коллекцию точек</returns>
        List<PointF> XyzwToPointFSimple(float[] xyzw);

        Image<Gray, byte> FillImageUpThreshold(float up, List<List<PointF>> sourceList, int width, int height, INonControlZonesRepository zones);

        List<PointF> XyzwToPointFList(float[] xyzw);

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

        List<PointF> SumList(List<PointF> first, List<PointF> second);
    }
}
