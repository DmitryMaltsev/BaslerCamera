using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public enum ErrorCode
    {
        ///// <summary>
        ///// Служебный код функции, не являющийся ошибкой
        ///// </summary>
        //Service,

        /// <summary>
        ///     Успешное выполнение (нет ошибок)
        /// </summary>
        Success = 0,

        /// <summary>
        ///     Неизвестный тип файла
        /// </summary>
        UnknownFileType = -100,

        /// <summary>
        ///     Ошибка создания сокета для обмена данными с датчиком
        /// </summary>
        CreatingSocket = -101,

        /// <summary>
        ///     Ошибка чтения/записи данных из внешнего оборудования
        /// </summary>
        ReadWriteData = -102,

        /// <summary>
        ///     Нехватка памяти
        /// </summary>
        OutOfMemory = -103,

        /// <summary>
        ///     Датчик не инициализирован
        /// </summary>
        SensorNotInitialized = -104,

        /// <summary>
        ///     Неверный входной параметр
        /// </summary>
        InvalidInputParameter = -105,

        /// <summary>
        ///     Ошибка фотоматрицы
        /// </summary>
        Matrix = -106,

        /// <summary>
        ///     Не удалось включить режим
        /// </summary>
        FailedEnableMode = -107,

        /// <summary>
        ///     Ошибка в функции более низкого уровня
        /// </summary>
        LowLevel = -108,

        /// <summary>
        ///     Ошибка останова потока
        /// </summary>
        StopStream = -109,

        /// <summary>
        ///     Ошибка запуска потока
        /// </summary>
        StartingThread = -110,

        /// <summary>
        ///     Превышено время ожидания
        /// </summary>
        TimeOut = -111,

        /// <summary>
        ///     Ошибка записи данных в файл
        /// </summary>
        WritingDataToFile = -112,

        /// <summary>
        ///     Неизвестная ошибка или исключение
        /// </summary>
        Unknown = -113,

        /// <summary>
        ///     Ошибка чтения данных из файла
        /// </summary>
        ReadingDataToFile = -114,

        /// <summary>
        ///     Файл не существует
        /// </summary>
        FileNotExist = -115,

        /// <summary>
        ///     Вычислительная ошибка
        /// </summary>
        Calculate = -116,

        /// <summary>
        ///     Не удалось остановить датчик
        /// </summary>
        FailedStopSensor = -117,

        /// <summary>
        ///     Датчик не создан
        /// </summary>
        SensorNotCreated = -118,

        /// <summary>
        ///     Одинаковые порты у разных датчиков
        /// </summary>
        SameDataPorts = -122,

        /// <summary>
        ///     Несоответствие размеру матрицы
        /// </summary>
        MatrixSizeMismatch = -119
    }
}
