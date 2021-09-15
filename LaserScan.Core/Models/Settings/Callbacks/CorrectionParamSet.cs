using System.Runtime.InteropServices;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public struct CorrectionParamsSet
    {
        #region Fields

        /// <summary>
        ///     Угол корректирования.
        /// </summary>
        public float Angle;

        /// <summary>
        ///     Горизонтальное отражение.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool HorizontalFlip;

        /// <summary>
        ///     Коррекция включена?
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsEnabled;

        /// <summary>
        ///     Коэффициент по дальности.
        /// </summary>
        public float KDistance;

        /// <summary>
        ///     Коэффициент по ширине.
        /// </summary>
        public float KLatitude;

        /// <summary>
        ///     Смещение по дальности.
        /// </summary>
        public float ShiftDistance;

        /// <summary>
        ///     Смещение по ширине.
        /// </summary>
        public float ShiftLatitude;

        /// <summary>
        ///     Вертикальное отражение.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool VerticalFlip;

        #endregion
    }
}
