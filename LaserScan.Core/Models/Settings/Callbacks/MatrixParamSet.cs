using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public struct MatrixParamsSet
    {
        #region Fields

        /// <summary>
        ///     Горизонтальное разрешение.
        /// </summary>
        public ushort HorizontalResolution;

        /// <summary>
        ///     Тип матрицы.
        /// </summary>
        public ushort TypeMatrix;

        /// <summary>
        ///     Вертикальное разрешение.
        /// </summary>
        public ushort VerticalResolution;

        #endregion
    }
}
