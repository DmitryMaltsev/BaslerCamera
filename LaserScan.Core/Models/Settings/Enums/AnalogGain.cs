using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    /// <summary>
    /// Перечисление для аналогового усиления
    /// </summary>
    /// <remarks>
    /// аналаговое усиление отличается от цифрового
    /// </remarks>
    public enum AnalogGain : byte
    {
        /// <summary>
        ///     Усиление X1
        /// </summary>
        X1dot0,

        /// <summary>
        ///     Усиление X1.2
        /// </summary>
        X1dot2,

        /// <summary>
        ///     Усиление X1.4
        /// </summary>
        X1dot4,

        /// <summary>
        ///     Усиление X1.6
        /// </summary>
        X1dot6,

        /// <summary>
        ///     Усиление X2
        /// </summary>
        X2dot0,

        /// <summary>
        ///     Усиление X2.4
        /// </summary>
        X2dot4,

        /// <summary>
        ///     Усиление X2.8
        /// </summary>
        X2dot8,

        /// <summary>
        ///     Усиление X3.2
        /// </summary>
        X3dot2
    }
}
