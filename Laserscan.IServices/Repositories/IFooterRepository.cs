namespace Kogerent.Services.Interfaces
{
    /// <summary>
    /// Контракт на создание экземпляров сервиса для логгирования в лог интерфейса и установки названия модуля
    /// </summary>
    public interface IFooterRepository
    {
        /// <summary>
        /// Текст сообщения
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Название модуля, написавшего лог
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// Название модуля
        /// </summary>
        string Title { get; set; }
    }
}
