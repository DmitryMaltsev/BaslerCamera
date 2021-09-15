using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public enum SyncSource
    {
        /// <summary>
        ///     Команды с ПК
        /// </summary>
        PC,

        /// <summary>
        ///     Внутренний генератор
        /// </summary>
        Internal,

        /// <summary>
        ///     Внешний генератор
        /// </summary>
        External,

        /// <summary>
        ///     Специализированный внешний сигнал
        /// </summary>
        Special
    }
}
