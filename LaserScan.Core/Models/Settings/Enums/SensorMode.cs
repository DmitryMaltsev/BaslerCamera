using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public enum SensorMode
    {
        /// <summary>
        ///     Режим профилей по столбцам
        /// </summary>
        ProfileColumn,

        ///// <summary>
        ///// Режим профилей по столбцам с аппаратным слежением
        ///// </summary>
        //ProfileColumnTracking,

        /// <summary>
        ///     Режим профилей по строкам
        /// </summary>
        ProfileRow,

        /// <summary>
        ///     Режим видео
        /// </summary>
        Video,

        /// <summary>
        ///     Режим видео через ОЗУ
        /// </summary>
        VideoRam
    }
}
