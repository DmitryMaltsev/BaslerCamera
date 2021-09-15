using System.Runtime.InteropServices;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public struct BordersParamsSet
    {
        #region Fields

        /// <summary>
        ///     Разрешение границ в миллиметрах по дальности?
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool DistanceBorder;

        /// <summary>
        ///     Разрешение границ в миллиметрах по ширине?
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool LatitudeBorder;

        /// <summary>
        ///     Нижняя граница рабочей области в миллиметрах.
        /// </summary>
        public float MmBottom;

        /// <summary>
        ///     Левая граница рабочей области в миллиметрах.
        /// </summary>
        public float MmLeft;

        /// <summary>
        ///     Правая граница рабочей области в миллиметрах.
        /// </summary>
        public float MmRight;

        /// <summary>
        ///     Верхняя граница рабочей области в миллиметрах.
        /// </summary>
        public float MmTop;

        /// <summary>
        ///     Левая граница окна на матрице в пикселях.
        /// </summary>
        public ushort WinX1;

        /// <summary>
        ///     Правая граница окна на матрице в пикселях.
        /// </summary>
        public ushort WinX2;

        /// <summary>
        ///     Верхняя граница окна на матрице в пикселях.
        /// </summary>
        public ushort WinY1;

        /// <summary>
        ///     Нижняя граница окна на матрице в пикселях.
        /// </summary>
        public ushort WinY2;

        #endregion
    }
}
