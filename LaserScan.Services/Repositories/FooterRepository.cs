using Kogerent.Services.Interfaces;
using Prism.Mvvm;

namespace Kogerent.Services.Implementation
{
    /// <summary>
    /// Сервис для логгирования в лог интерфейса и установки названия модуля
    /// </summary>
    public class FooterRepository : BindableBase, IFooterRepository
    {
        private string _text;
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        private string _headerText;
        /// <summary>
        /// Название модуля, написавшего лог
        /// </summary>
        public string HeaderText
        {
            get { return _headerText; }
            set { SetProperty(ref _headerText, value); }
        }
        private string _title;
        /// <summary>
        /// Название модуля
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
    }
}
