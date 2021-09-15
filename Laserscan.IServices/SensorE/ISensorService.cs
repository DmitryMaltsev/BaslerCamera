using Kogerent.Core;
using Kogerent.Logger;

using System;
using System.Threading.Tasks;

namespace Kogerent.Services.Interfaces
{
    /// <summary>
    /// Контракт на создание службы работы с профилометром
    /// </summary>
    public interface ISensorService
    {
        /// <summary>
        /// Флаг получения данных
        /// </summary>
        bool DataRecieved { get; set; }

        /// <summary>
        /// Название файла библиотеки СенсорЕ
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Сервис для записей в лог интерфейса
        /// </summary>
        IFooterRepository FooterRepository { get; }

        /// <summary>
        /// Код ошибок
        /// </summary>
        ErrorCode GetLastErrorCode { get; }

        /// <summary>
        /// Коллбэки, приходящие от профилометра
        /// </summary>
        CallbackData LastCallbackData { get; set; }

        /// <summary>
        /// Пул для создания коллбэков
        /// </summary>
        ICallbackPool CallbackPool { get; set; }

        /// <summary>
        /// Код последней ошибки
        /// </summary>
        int LastErrorCode { get; set; }

        /// <summary>
        /// Логгер для дебага
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Номер кадра из азу
        /// </summary>
        int NFrameDownloadedFromRam { get; set; }

        /// <summary>
        /// Количество кадров в азу
        /// </summary>
        ushort RamFrameCount { get; set; }

        /// <summary>
        /// размер азу буфера
        /// </summary>
        byte[] RawRamDataBuffer { get; set; }

        /// <summary>
        /// сколько осталось в азу
        /// </summary>
        int RemainingBytesInRam { get; set; }

        /// <summary>
        /// Объект настроек профилометра
        /// </summary>
        SensorSettings SensorSettings { get; set; }

        /// <summary>
        /// Стректура параметров границ
        /// </summary>
        BordersParamsSet StatusBordersParamsSet { get; set; }

        /// <summary>
        /// Структура параметров для коррекции
        /// </summary>
        CorrectionParamsSet StatusCorrectionParamsSet { get; set; }

        /// <summary>
        /// Структура общих параметров
        /// </summary>
        GeneralParamsSet StatusGeneralParamsSet { get; set; }

        /// <summary>
        /// Структура параметров для матрицы
        /// </summary>
        MatrixParamsSet StatusMatrixParamsSet { get; set; }

        /// <summary>
        /// Режим синхронизации по стробу
        /// </summary>
        int SyncFromStrobeMode { get; set; }

        /// <summary>
        /// Объект настроек системы
        /// </summary>
        SystemSettings SystemSettings { get; set; }

        /// <summary>
        /// Счетчик пришедших данных
        /// </summary>
        int DataCounter { get; set; }

        CallbackData LastCallBack { get; set; }

        //se.VideoDelegate VideoDelegate { get; set; }

        /// <summary>
        /// Событие прихода данных
        /// </summary>
        event EventHandler<CallbackData> CallbackDataReceived;

        /// <summary>
        /// Включает или выключает коррекцию уровня черного в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int AllowBlackReference();
        /// <summary>
        /// Включает или выключает применение границ рабочей зоны для профиля в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int AllowBorders();

        /// <summary>
        /// Включает или выключает пересчет выдаваемого профиля из пикселей в миллиметры, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int AllowCalibration();

        /// <summary>
        /// Включает или выключает коррекцию профиля в соответствие с настройками, не изменяя уже заданные значения.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int AllowCorrection();

        /// <summary>
        /// Включает или выключает передискретизацию профиля в линейный массив в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int AllowDiscretize();

        /// <summary>
        /// Включает или выключает удаление отдельных шумовых точек методом окна (аппаратно) в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int AllowNoiseFixedWindowRemover();

        /// <summary>
        /// Включает или выключает удаление отдельных шумовых точек методом плавучего окна в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int AllowNoiseFloatWindowRemover();

        /// <summary>
        /// Включает или выключает кусочную экспозицию в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int AllowPiecewiseLinear();

        /// <summary>
        /// Включает или выключает режим слежения в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int AllowTracking();

        /// <summary>
        /// Универсальный метод для вызова функций с записью в лог
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Метода не существует</exception>
        int Call(Func<int> method, string methodName = null);

        /// <summary>
        ///     Считывает данные, пришедшие от Callback из библиотеки SensorE.dll и пробрасывает эти данные слушателям.
        /// </summary>
        /// <param name="sender">Дескриптор объекта датчика.</param>
        /// <param name="data">Массив данных.</param>
        /// <param name="dataLength">Количество данных.</param>
        /// <param name="inVideo">Режим видео? (иначе профиль)</param>
        void CallbackEvent(IntPtr sender, IntPtr data, int dataLength, bool inVideo);

        /// <summary>
        /// Создает объект датчика в библиотеке SensorE.dll в соответствие с настройками и возвращает его дескриптор.
        /// Помимо этого, выбирает порты для передачи данных с датчиков,
        /// устанавливает соединение с датчиком, выполняет команду останова <see cref="Stop"/>,
        /// и считывает необходимые характеристики (например, версия прошивки) датчиков для правильной работы с ним.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        int CreateSensor();

        /// <summary>
        /// Создает объект виртуального датчика в библиотеке SensorE.dll в соответствие с настройками и возвращает его дескриптор.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        int CreateVirtualSensor();

        /// <summary>
        /// Вызвать при удалении датчика из коллекции
        /// </summary>
        void Destroy();

        /// <summary>
        ///     Уничтожает объект датчика, который был создан методом <see cref="CreateSensor"/> или <see cref="CreateVirtualSensor"/>.
        /// Если объект датчика не был создан ранее, то ничего не произойдет.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        int DisposeSensor();

        /// <summary>
        /// Переводит датчик в режим измерения профиля, когда объектом обработки (колонкой) на изображении является строка.
        /// В процессе получения данных с профилометра не осуществляется контроль доставки пакетов (без подтверждения).
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int EnableProfileColumnsMode();

        /// <summary>
        /// Переводит датчик в режиме измерения профиля, когда объектом обработки (колонкой) на изображении является столбец.
        /// В процессе получения данных с профилометра не осуществляется контроль доставки пакетов (без подтверждения).
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int EnableProfileRowsMode();

        /// <summary>
        ///     Переводит датчик в режим однократного срабатывания по команде. Одна команда - одно срабатывание.
        ///     Командой, запускающей срабатывание датчика, является функция SendSync.
        ///     Установка режима синхронизации по команде может использоваться также для останова работы датчика.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int EnableSyncCmdMode();

        /// <summary>
        /// Переводит датчик в особый режим синхронизации по внешнему импульсу.
        /// Данный тип синхронизации используется, если профилометр аппаратно настроен на передачу импульса запуска непосредственно на фотоматрицу,
        /// что обеспечивает максимально быструю реакцию.
        /// Это стандартный режим внешней синхронизации. Аппаратная настройка выполнятся в процессе изготовления профилометра.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int EnableSyncExtMode();

        /// <summary>
        /// Переводит датчик в режим срабатывания по внутреннему таймеру с частотой <see cref="SensorSettings.SyncNoneHz"/> Гц.
        /// При этом профилометр начинает выполнять измерения с указанной частотой, которая не является синхронной с другими профилометрами в системе.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int EnableSyncNoneMode();

        /// <summary>
        /// Переключает профилометр в режим выдачи видеокадров и выделяет в памяти буфер,
        /// необходимый для временного хранения полученного кадра, в соответствии с размером матрицы + заголовок 64 байта.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int EnableVideoMode();

        /// <summary>
        ///     Включает режим сохранения видеокадра в ОЗУ устройства.
        ///     При этом при получении кадра вызов Callback-функций происходить не будет.
        ///     Вместо этого видеокадр будет сохраняться в ОЗУ устройства.
        ///     Считать содержимое ОЗУ можно отдельной командой.
        ///     Включение режима сбрасывает работу фотоматрицы,
        ///     т.е. приводит к сбросу установленной ранее экспозиции и усиления.
        ///     Данная команда также сбрасывает указатель текущего кадра, хранящегося в ОЗУ.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int EnableVideoToRamMode();

        /// <summary>
        /// Считывает значение аналогового усиления видеосигнала в настройки.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetAnalogGain();

        /// <summary>
        /// Считывает значение цифрового усиления видеосигнала в настройки.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetDigitalGain();

        /// <summary>
        /// Считывает значение времени экспозиции в условных единицах в настройки.
        /// Одна условная единица приблизительно равна 4 мкс.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetExposition();

        /// <summary>
        /// Считывает версию прошивки датчика в <see cref="SensorSettings.HWVersion"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetFirmwareVersion();

        /// <summary>
        /// Считывает количество кадров, хранящихся в ОЗУ датчика, в <see cref="RamFrameCount"/>.
        /// Режим сохранения данных в ОЗУ предназначен только для видеоданных.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetRamFramesCount();

        /// <summary>
        /// Считывает все видеоданные, которые имеются в ОЗУ,
        /// в <see cref="RawRamDataBuffer"/>, <see cref="NFrameDownloadedFromRam"/> и <see cref="RemainingBytesInRam"/>.
        /// В процессе передачи, новые данные в буфере не накапливаются.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetRawRamData();

        /// <summary>
        ///     Считывает значение из регистра датчика по указанному адресу.
        /// ВНИМАНИЕ! Не рекомендуется использовать данный метод! Только для низкоуровневой отладки.
        /// [Безопасный вызов аналогичный вызову <see cref="Call"/>]
        /// </summary>
        /// <param name="nReg">Номер регистра датчика, значение которого необходимо считать.</param>
        /// <param name="value">Считанное значение из регистра датчика.</param>
        /// <returns>Успешное выполнение?</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetRegisterValue(byte nReg, out ushort value);

        /// <summary>
        ///     Считывает версию используемой библиотеки SensorE.dll в <see cref="SystemSettings.DllVersion"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        int GetSensorEVersion();

        /// <summary>
        /// Считывает текущие границы рабочей области в <see cref="StatusBordersParamsSet"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetStatusBordersParamsSet();

        /// <summary>
        /// Считывает текущие параметры коррекции профиля в <see cref="StatusCorrectionParamsSet"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetStatusCorrectionParamsSet();

        /// <summary>
        /// Считывает текущие параметры объекта датчика общего характера в <see cref="StatusGeneralParamsSet"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetStatusGeneralParamsSet();

        /// <summary>
        /// Считывает текущие параметры видеоматрицы в <see cref="StatusMatrixParamsSet"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetStatusMatrixParamsSet();

        /// <summary>
        /// Считывает значение температуры, измеренной датчиком, в <see cref="SensorSettings.Temperature"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int GetTemper();

        /// <summary>
        ///     Инициализация датчика.
        /// Если идентификатор датчика существует, то будет произведена частичная инициализация, иначе полная.
        /// Частичная инициализация подразумевает под собой пропуск "тяжелых" функций, которые делаются один раз после включения питания.
        /// Таким образом, частичная инициализация - это в основном изменение настроек и режима работы.
        /// </summary>
        Task<int> Init();

        /// <summary>
        ///     Инициализирует уровень библиотеки SensorE.dll (версия, отладка, режим заголовка и диапазон портов при создании датчиков).
        /// [Безопасный вызов через <see cref="Call"/>]
        /// </summary>
        /// <returns>Успешное выполнение?</returns>
        int InitDllStage();

        /// <summary>
        /// Загружает файл калибровки в датчик, который необходим для пересчета пикселей в миллиметры, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int LoadCalibration();

        /// <summary>
        /// Пишет в сообщение в лог интерфейса
        /// </summary>
        /// <param name="message">Сообщение</param>
        void Log(string message);

        /// <summary>
        ///     Пересчитывает одну точку, представленную в виде координат на матрице (x и y в пикселях) в точку на реальной
        ///     миллиметровой плоскости,
        ///     представленную в виде координат по дальности и ширине, по загруженной в объект датчика калибровочной таблице.
        /// [Безопасный вызов аналогичный вызову <see cref="Call"/>]
        /// </summary>
        /// <param name="x">Координата X точки на матрице, в пикселях</param>
        /// <param name="y">Координата Y точки на матрице, в пикселях</param>
        /// <param name="latitude">Координата по ширине, в миллиметрах</param>
        /// <param name="distance">Координата по дальности, в миллиметрах</param>
        /// <returns>Успешное выполнение?</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int OnePointToMm(int x, float y, out float latitude, out float distance);

        /// <summary>
        /// Вызывает однократное срабатывание датчика, при условии, что тот находится в режиме «Синхронизация по команде».
        ///     Получив данные, библиотека вызывает соответствующую Callback-функцию.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SendSync();

        /// <summary>
        /// Задает значение аналогового усиления видеосигнала в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetAnalogGain();

        /// <summary>
        /// Включает или выключает режим сортировки координат по L в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetAscendingL();

        /// <summary>
        /// Задает границы рабочей зоны для профиля в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetBorders();

        /// <summary>
        /// Задает параметры коррекции профиля в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetCorrection();

        /// <summary>
        /// Задает режим антидребезга сигнала внешней синхронизации в соответствие с настройками.
        /// Если фронт сигнала синхронизации зашумлен,
        ///     данная функция может помочь избавиться от срабатывания фотоматрицы несколько раз подряд по одному фронту, задав
        ///     время задержки, в течение которого переходные процессы на фронте должны прекратиться. Время задержки задается в
        ///     периодах тактовой частоты:
        ///     <para>1. для режима «профиль» - тактовая частота равна 32 МГц;</para>
        ///     <para>2. для режима «видео» - тактовая частота равна 30 МГц.</para>
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetDebounce();

        /// <summary>
        /// Включает или выключает режим отладки в библиотеке SensorE.dll в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        int SetDebugMode();

        /// <summary>
        /// Включает или выключает режим отладки потока приема данных в библиотеке SensorE.dll в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        int SetDebugModeListener();

        /// <summary>
        /// Задает значение цифрового усиления видеосигнала в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetDigitalGain();

        /// <summary>
        /// Задает параметры дискретизации в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetDiscretizeMode();

        /// <summary>
        /// Включает или выключает расширенный формат заголовка принимаемых данных из библиотеки SensorE.dll в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        int SetEnhanceHeader();

        /// <summary>
        /// Задает значение времени экспозиции в условных единицах в соответствие с настройками.
        /// Одна условная единица приблизительно равна 4 мкс.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetExposition();

        /// <summary>
        /// Включает или выключает срабатывание лазера в момент экспозиции кадра, в соответствие с настройками.
        /// Данная функция может быть аппаратно отключена при изготовлении профилометра по заказу,
        /// в этом случае работа лазера всегда будет разрешена.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetLaser();

        /// <summary>
        /// Включает или нет инверсию лазера, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetLaserInversion();

        /// <summary>
        /// Задает значение-заменитель для отсутствуюших значений в массиве дискретизированных данных в соответствие с настройками.
        /// Имеет смысл только в режиме включенной дискретизации.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetMissingDesignator();

        /// <summary>
        ///     Переводит датчик в какой-то из режимов измерения в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Режим работы датчика в настройках имеет недопустимое значение.</exception>
        int SetMode();

        /// <summary>
        /// Задает параметры удаления шумовых точек методом плавучего окна в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetNoiseFloatWindowRemoverMode();

        /// <summary>
        /// Задает количество кадров, получаемых на один строб.
        /// </summary>
        /// <returns>Код ошибки</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetNumFramesPerStrobe();

        /// <summary>
        /// Задает параметры обработки пятен на изображении датчиком, с целью формирования данных профиля, в соответствие с настройками.
        /// На режим передачи видео данный метод не оказывает влияния.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetParameters();

        /// <summary>
        /// Задает параметры кусочной экспозиции в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetPiecewiseLinearMode();

        /// <summary>
        /// Управляет включением конвейерного (pipeline) режима формирования кадров.
        /// </summary>
        /// <returns>Код ошибки</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetPipeline();

        /// <summary>
        /// Задает диапазон портов, который будет использоваться в запускаемом приложении для соединения с датчиками, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        int SetPortRange();

        /// <summary>
        /// Создает объект датчика в библиотеке SensorE.dll в соответствие с настройками и возвращает его дескриптор.
        /// Помимо этого, выбирает порты для передачи данных с датчиков,
        /// устанавливает соединение с датчиком, выполняет команду останова <see cref="Stop"/>,
        /// и считывает необходимые характеристики (например, версия прошивки) датчиков для правильной работы с ним.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        int SetRegisterValue(byte nReg, ushort value);

        /// <summary>
        /// Задает коэффициент прореживания, который применяется к строкам матрицы, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetSubsamplingMatrix();

        /// <summary>
        /// Задает коэффициент прореживания, который применяется к колонкам в процессе вычисления профиля, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetSubsamplingProfile();

        /// <summary>
        ///     Переводит датчик в какой-то из режимов синхронизации в соответствие с <see cref="SyncMode"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Режим синхронизации датчика в поле данных имеет недопустимое значение.</exception>
        int SetSyncMode();

        /// <summary>
        /// Задает параметры для режима слежения в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetTracking();

        /// <summary>
        /// Включает или выключает режим передачи в ПК неполных данных в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetUncompletedData();

        /// <summary>
        /// Задает размер рабочего окна датчика в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetWindow();

        /// <summary>
        /// Включает или выключает вычитание постоянной составляющей (измеренной медианой) в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SetZeroDC();

        /// <summary>
        /// Запускает поток получения данных от датчика.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int StartListener();

        /// <summary>
        /// Отправка на профилометр команды «стоп».
        /// При этом оборудование профилометра перестаёт посылать данные (останова потока получения данных при этом не происходит).
        /// Функция не возвращает управление до тех пор, пока профилометр не прекратит передачу данных или не произойдет ошибка таймаута (1 секунда).
        /// Данная функция необходима для гарантированного успешного изменения параметров профилометра,
        /// если тот занят передачей больших массивов данных в потоке.
        /// В противном случае возможно аппаратное зависание профилометра, которое можно будет устранить только пересбросом питания.
        ///     <para>
        ///         Приводит к сбросу следующих параметров в значения по умолчанию:
        ///         <para>Усиление (функция SetGain) в значение 40;</para>
        ///     </para>
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int Stop();

        /// <summary>
        /// Переводит датчик в режим синхронизации по внешнему импульсу, поступающему на один из входов Q0-Q7.
        /// Данный тип внешней синхронизации следует выключать,
        /// если профилометр аппаратно настроен для передачи импульса запуска через центральный сигнальный процессор (ЦСП),
        /// что позволяет контролировать запуск микропрограммой процессора.
        /// Это нестандартный режим, для работы в котором профилометр настраивается по отдельному заказу.
        /// Аппаратная настройка выполнятся в процессе изготовления профилометра.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        int SyncFromStrobe();
    }
}
