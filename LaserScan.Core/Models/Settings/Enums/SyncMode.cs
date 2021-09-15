using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public enum SyncMode
    {
        /// <summary>
        ///     Режим внешней синхронизации
        /// </summary>
        SyncExt,

        /// <summary>
        ///     Режим непрерывной работы с заданной частотой
        /// </summary>
        SyncNone,

        /// <summary>
        ///     Режим синхронизации по команде
        /// </summary>
        SyncCmd,

        /// <summary>
        ///     Режим синхронизации от аппаратных выходов Q0-7
        /// </summary>
        SyncFromStrobe
    }
}
