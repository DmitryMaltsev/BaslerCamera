using System.Runtime.InteropServices;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public struct GeneralParamsSet
    {
        #region Fields

        /// <summary>
        ///     Работа в миллиметрах?
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool InMm;

        /// <summary>
        ///     IP адрес контроллера.
        /// </summary>
        public ulong IpAddress;

        /// <summary>
        ///     Порт команд.
        /// </summary>
        public ushort PortCmd;

        /// <summary>
        ///     Порт данных.
        /// </summary>
        public ushort PortData;

        #endregion
    }
}
