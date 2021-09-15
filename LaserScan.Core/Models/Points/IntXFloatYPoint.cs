using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    /// <summary>
    /// Точка, у которой Х - целочисленное, а Y - дробное число
    /// </summary>
    public struct IntXFloatYPoint
    {
        /// <summary>
        /// Х-координата точки
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y-координата точки
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Конструктор, создающий экземпляр точки
        /// </summary>
        /// <param name="x">Координата Х</param>
        /// <param name="y">Координата У</param>
        public IntXFloatYPoint(int x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
