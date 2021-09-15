using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public enum DiscretizeMode : byte
    {
        /// <summary>
        ///     Усреднение совпадающих показаний
        /// </summary>
        Mean,

        /// <summary>
        ///     Максимум
        /// </summary>
        Maximum,

        /// <summary>
        ///     Минимум
        /// </summary>
        Minimum,

        /// <summary>
        ///     Близость к опорному профилю, представляющему собой результат линейной аппроксимации
        /// </summary>
        LinearReference,

        /// <summary>
        ///     LinearReference + антиблик методом производной
        /// </summary>
        LinearReferenceDerivate,

        /// <summary>
        ///     LinearReference + антиблие методом ширины пятна
        /// </summary>
        LinearReferenceWidth
    }
}
