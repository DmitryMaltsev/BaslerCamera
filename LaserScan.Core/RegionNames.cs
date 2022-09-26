using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    /// <summary>
    /// Хранит имена регионов
    /// </summary>
    public static class RegionNames
    {
        /// <summary>
        /// Центральный регион интерфейса. Отображает основное содержимое
        /// </summary>
        public const string ContentRegion = "ContentRegion";

        /// <summary>
        /// Панель управления интерфейса (лента). Содержит набор элементов для управления
        /// </summary>
        public const string RibbonRegion = "RibbonRegion";

        /// <summary>
        /// Регион для отображения лога интерфейса
        /// </summary>
        public const string FooterRegion = "FooterRegion";

        /// <summary>
        /// Суб-регион для отображения коллекции вкладок. 
        /// </summary>
        public const string LaserscanTabRegion = "LaserscanTabRegion";

        /// <summary>
        /// Субрегион для отображения настроек
        /// </summary>
        public const string SettingsTabRegion = "SettingsTabRegion";

        /// <summary>
        /// Субрегион для отображения вариантов ленты
        /// </summary>
        public const string RibbonSubRegion = "RibbonSubRegion";

        /// <summary>
        /// Субрегион для отображения вариантов настроек.
        /// </summary>
        public const string ContentTablesRegion = "ContentTablesRegion";

        /// <summary>
        /// Ключ для отображения RibbonRegion
        /// </summary>
        public const string CamerasRibbonKey = "CamerasRibbonRegion";

        public const string CamerasContentKey = "CamerasContentKey";

        public const string Camera1Region = "Camera1Region";

        public const string Camera2Region = "Camera2Region";

        public const string Camera3Region = "Camera3Region";

        public const string Graph1Region = "Graph1Region";

        public const string Graph2Region = "Graph2Region";

        public const string Graph3Region = "Graph3Region";

        public const string Graph1Key = "Graph1Key";

        public const string Graph2Key = "Graph2Key";

        public const string Graph3Key = "Graph3Key";

        public const string GraphsRegion = "GraphsRegion";
    }
}
