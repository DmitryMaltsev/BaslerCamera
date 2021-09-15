using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    /// <summary>
    /// Модель пар профилометров
    /// </summary>
    public class SensorPair
    {
        public float K { get; set; }
        public float B { get; set; }

        /// <summary>
        /// Индекс пары
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Верхний профилометр
        /// </summary>
        public SensorSettings TopSensor { get; set; }

        /// <summary>
        /// Нижний профилометр
        /// </summary>
        public SensorSettings BottomSensor { get; set; }

        /// <summary>
        /// Дискретизированный массив точек верхнего профилометра
        /// </summary>
        public List<IntXFloatYPoint> UpDisretized { get; set; } = new();

        /// <summary>
        /// Левая граница для дискретизации
        /// </summary>
        public int LeftBorder { get; set; }

        /// <summary>
        /// Правая граница для дискретизации
        /// </summary>
        public int RightBorder { get; set; }

        /// <summary>
        /// Дискретизированный массив нижнего профилометра
        /// </summary>
        public List<IntXFloatYPoint> DnDiscretized { get; set; } = new();

        /// <summary>
        /// Разность высот верхнего и нижнего массивов точек
        /// </summary>
        public List<IntXFloatYPoint> SumPoints { get; set; } = new();

        /// <summary>
        /// Поток-потребитель
        /// </summary>
        public Task ConsumerTask { get; set; }

        /// <summary>
        /// Флаг четности/нечетности
        /// </summary>
        public bool CanSendSync { get; set; }
    }
}
