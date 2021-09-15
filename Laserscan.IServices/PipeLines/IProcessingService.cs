using Kogerent.Core;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Kogerent.Services.Interfaces
{
    /// <summary>
    /// Контракт на создание обработчика данных со всех датчиков
    /// </summary>
    public interface IProcessingService
    {
        /// <summary>
        /// Общее количество обработанных профилей
        /// </summary>
        int GenericBufferCount { get; }

        /// <summary>
        /// Последний общий профиль
        /// </summary>
        List<IntXFloatYPoint> LastProfile { get; }

        /// <summary>
        /// Картинка для вывода найденых дефектов
        /// </summary>
        Bitmap Bmp { get; set; }

        /// <summary>
        /// Коллекция найденных дефектов
        /// </summary>
        List<DefectProperties> Defects { get; set; }

        /// <summary>
        /// Пора добавлять дефекты в таблицу?
        /// </summary>
        bool DefectsAnalyzed { get; set; }

        /// <summary>
        /// Счетчик заполнения карты
        /// </summary>
        int FillCount { get; }

        /// <summary>
        /// Уничтожает объект и освобождает все используемые ресурсы
        /// </summary>
        void Dispose();
    }
}