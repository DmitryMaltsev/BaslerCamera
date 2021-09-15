using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public struct SubWindow
    {
        #region Fields

        /// <summary>
        ///     Левая граница окна.
        /// </summary>
        public int X1;

        /// <summary>
        ///     Правая граница окна.
        /// </summary>
        public int X2;

        /// <summary>
        ///     Верхняя граница окна.
        /// </summary>
        public int Y1;

        /// <summary>
        ///     Нижняя граница окна.
        /// </summary>
        public int Y2;

        #endregion
    }
}
