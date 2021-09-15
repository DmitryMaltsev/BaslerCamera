using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    /// <summary>
    /// Статический класс-обертка для импорта функций СенсорЕ
    /// </summary>
    public static class SensorELibrary
    {
        #region Delegates

        /// <summary>
        ///     Делегат на функцию автоматической установки экспозиции.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="exposition">Вычисленная автоматически экспозиция</param>
        public delegate void AutoExpDelegate(IntPtr sender, ushort exposition);

        /// <summary>
        ///     Делегат на функцию обработки профиля.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mode">Текущий режим работы датчика</param>
        /// <param name="data">Передаваемые в делегат данные профилометра</param>
        /// <param name="dataLength">Объем данных (плюс размер заголовка), выраженный в количестве данных типа float</param>
        public delegate void ProfileDelegate(IntPtr sender, int mode, IntPtr data, int dataLength);

        /// <summary>
        ///     Делегат на функцию обработки видео.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="mode">Текущий режим работы датчика</param>
        /// <param name="data">Передаваемые в делегат данные профилометра</param>
        /// <param name="dataLength">Объем данных (плюс размер заголовка), выраженный в количестве байт</param>
        public delegate void VideoDelegate(IntPtr sender, int mode, IntPtr data, int dataLength);

        #endregion

        #region Static Fields

        /// <summary>
        ///     Имя файла библиотеки.
        /// </summary>
        public const string FileName = "SensorE.dll";

        /// <summary>
        ///     Соглашение о вызове функций dll.
        /// </summary>
        public const CallingConvention Convention = CallingConvention.Cdecl;

        /// <summary>
        ///     Полное имя файла библиотеки (с путем).
        /// </summary>
        public static readonly string FullName;

        /// <summary>
        ///     Тип объекта
        /// </summary>
        public static readonly Type TypeObject = typeof(SensorELibrary);

        #endregion

        #region Constructors

        /// <summary>
        ///     Подгружает необходимую библиотеку SensorE в зависимости от разрядности запускаемого приложения.
        /// </summary>
        static SensorELibrary()
        {
            var directoryName = Environment.CurrentDirectory;

            if (directoryName == null)
                throw new InvalidOperationException("Директории с библиотекой не существует");

            //Размером указателя мы определяем разрядность системы
            bool is64 = IntPtr.Size == 8;
            var folderName = is64 ? "x64" : "x86";

            //Формируем путь к файлу с библиотекой
            var path = Path.Combine(directoryName, folderName, FileName);

            //Прописываем путь к библиотеке
            FullName = path;

            //Загружаем библиотеку в память
            LoadLibrary(path);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Отображает заданный исполняемый модуль в адресное пространство вызывающего процесса.
        /// </summary>
        /// <param name="lpFileName">
        ///     Указатель на символьную строку с нулем в конце, которая именует исполняемый модуль (или  .dll или .exe файл).
        ///     Указанное имя - это имя файла модуля и оно не связано с именем самого сохраняемого в библиотеке модуля, как это
        ///     определено ключевым словом LIBRARY в (.def) файле определения модуля.
        ///     Если строка определяет путь, но файл не существует в указанном каталоге, функция завершается ошибкой.
        ///     Когда определяется путь, убедитесь, что использованы наклонные черты влево (обратные слэши (\)), а не прямые слэши
        ///     (/).
        ///     Если символьная строка не определяет путь, функция использует стандартную стратегию поиска файла.
        /// </param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        ///     Прописывает массив символов char в виде «ver x.xx (20xx.xx.xx)» в память по указателю pointer. Вызывающая функцию
        ///     программа должна заранее выделить память под строку и передать указатель на её начало. Длина строки фиксирована и
        ///     составляет 22 байта.
        /// </summary>
        /// <param name="pointer">Указатель область памяти, выделенной для хранения строки с версией</param>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern void VersionV2(IntPtr pointer);

        /// <summary>
        ///     Включает режим отладки dll, в котором ведётся файл журнала dll_log.txt (на английском языке). Файл хранится в том
        ///     же месте на жёстком диске, в котором находится exe-файл исполняемой программы. Функция не перезаписывает файл
        ///     журнала, а добавляет события в него. Рекомендуется включать режим отладки прежде, чем вызывать какие-либо другие
        ///     функции dll. Настоятельно рекомендуется использовать этот режим только для поиска и устранения неисправностей. В
        ///     нормальном режиме работы этот режим должен быть отключён. Следить за файлом журнала (вовремя удалять старые записи)
        ///     должен пользователь.
        /// </summary>
        /// <param name="en">Включение режима отладки? (по умолчанию режим выключен)</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int DebugMode(bool en);

        /// <summary>
        ///     Включает режим отладки потока приема данных, в котором ведутся файлы журналов N_dll_listener_log.txt (на английском
        ///     языке), где N – номер созданного потока. Файлы хранятся в том же месте на жёстком диске, в котором находится
        ///     exe-файл исполняемой программы. Функция не перезаписывает файлы журнала, а добавляет события в него. Рекомендуется
        ///     включать режим отладки перед запуском потоков. Настоятельно рекомендуется использовать этот режим только для поиска
        ///     и устранения неисправностей. В нормальном режиме работы этот режим должен быть отключён. Следить за файлами журнала
        ///     (вовремя удалять старые записи) должен пользователь.
        /// </summary>
        /// <param name="en">Включение режима отладки? (по умолчанию режим выключен)</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int DebugModeListener(bool en);

        /// <summary>
        /// </summary>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int Calculate(IntPtr sensor_handle, float[] source, int source_length, float[] result, int result_length);

        /// <summary>
        ///     Позволяет использовать заголовок расширенного формата, длина которого является переменной.
        /// </summary>
        /// <param name="en">Использовать заголовок расширенного формата? (по умолчанию используется стандантный заголовок)</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int EnhanceHeader(bool en);

        /// <summary>
        ///     Чтение версии прошивки контроллера
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="versionPointer">Указатель на массив int из трех элементов с версией датчика</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int VersionHwV2(IntPtr sensorHandle, IntPtr versionPointer);

        /// <summary>
        ///     Позволяет задать диапазон портов, который будет использоваться на данном ПК для соединения с профилометрами. Для
        ///     каждого профилометра требуется два отдельных порта. Порты выбираются автоматически в указанном диапазоне в процессе
        ///     создания объекта датчика функцией <see cref="CreateSensorV2" />. Если диапазон портов не указан, используется
        ///     диапазон портов по умолчанию от 34500 до 34699 включительно.
        /// </summary>
        /// <param name="firstPort">Номер порта, соответствующий началу диапазона (включительно)</param>
        /// <param name="lastPort">Номер порта, соответствующий концу диапазона (включительно)</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetPortRange(ushort firstPort, ushort lastPort);

        ///// <summary>
        ///// Разрешение ручного определения портов каждого датчика
        ///// </summary>
        ///// <param name="en">Разрешение режима</param>
        ///// <returns></returns>
        //[DllImport(DriverDllName, CallingConvention = Convention)]
        //public static extern int ManualPortEn(bool en);

        /// <summary>
        ///     Создает объекта датчика и возвращает дескриптор этого объекта, по которому к нему можно обращаться в дальнейшем.
        ///     Кроме того, функция выполняет следующие действия:
        ///     <para>•	автоматически выбирает номера портов для передачи команд и передачи данных;</para>
        ///     <para>•	создает сокет для передачи команд и сокет для передачи данных;</para>
        ///     <para>•	выполняет команду Stop (Останов профилометра);</para>
        ///     <para>•	связывается с устройством и считывает его характеристики для обеспечения правильности работы с ним.</para>
        /// </summary>
        /// <param name="ip">IP-адрес датчика</param>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int CreateSensorV2(string ip, out IntPtr sensorHandle);

        /// <summary>
        ///     Создает объекта датчика и возвращает дескриптор этого объекта, по которому к нему можно обращаться в дальнейшем.
        ///     Кроме того, функция выполняет следующие действия:
        ///     <para>•	автоматически выбирает номера портов для передачи команд и передачи данных;</para>
        ///     <para>•	создает сокет для передачи команд и сокет для передачи данных;</para>
        ///     <para>•	выполняет команду Stop (Останов профилометра);</para>
        ///     <para>•	связывается с устройством и считывает его характеристики для обеспечения правильности работы с ним.</para>
        /// </summary>
        /// <param name="ip">IP-адрес датчика</param>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="manualPortMode">Режим установки фиксированных портов, иначе автиматическая привязка</param>
        /// <param name="manualDataPort">
        ///     Фиксированный порт для передачи данных. Для передачи команд также необходим порт следующий
        ///     за этим (заняты оба)
        /// </param>
        /// <param name="versionHw">Указатель на память, куда пропишется версия прошивки датчика.</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int CreateSensorV3(string ip, out IntPtr sensorHandle, bool manualPortMode,
            ushort manualDataPort, IntPtr versionHw);

        /// <summary>
        ///     Создает объект виртуального датчика и возвращает дескриптор этого объекта, по которому к нему можно обращаться в
        ///     дальнейшем. Данная функция позволяет создать объект датчика, если физически датчик отсутствует.
        ///     Параметры создаваемого датчика: матрица cmv2000_v3 (1088 х 2048). Кроме того, функция выполняет следующие действия:
        ///     <para>•	автоматически выбирает номера портов для передачи команд и передачи данных;</para>
        ///     <para>•	создает сокет для передачи команд и сокет для передачи данных.</para>
        /// </summary>
        /// <param name="ip">IP-адрес датчика</param>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int CreateVirtualSensor(string ip, out IntPtr sensorHandle);

        /// <summary>
        ///     Управляет включением передачи в ПК неполных данных (данных, полученных через протокол UDP по каким-либо причинам
        ///     неполностью).
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="en">Разрешить передачу неполных данных? (по умолчанию передача неполных данных разрешена)</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int UncompletedData(IntPtr sensorHandle, bool en);

        /// <summary>
        ///     Запускает поток получения данных от датчиков. Функция связывает события получения данных с делегатами
        ///     CALLBACK-функций, что приводит к вызову соответствующей функции в управляющем приложении. В момент запуска потока
        ///     создается сокет для получения данных от профилометра. Как правило, каждый объект датчика должен иметь рабочий поток
        ///     приема данных.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="callbackEventVideo">Делегат на функцию обработки видео</param>
        /// <param name="callbackEventProfile">Делегат на функцию обработки профиля</param>
        /// <param name="callbackEventAutoExp">Делегат на функцию автоматической установки экспозиции</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int StartListener(IntPtr sensorHandle,
            VideoDelegate callbackEventVideo,
            ProfileDelegate callbackEventProfile,
            AutoExpDelegate callbackEventAutoExp);

        /// <summary>
        ///     Функция загружает файл калибровки, который необходим для пересчёта пикселей в миллиметры.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="calibrationFileName">Калибровочный файл с расширением «.bin» или с расширением «.fst»</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int LoadCalibrationV2(IntPtr sensorHandle, string calibrationFileName);

        /// <summary>
        ///     Позволяет включить или выключить пересчёт профиля, выдаваемого в CallBack, из плоскости матрицы (пиксели) в
        ///     миллиметровую плоскость. Если профиль должен выдаваться в миллиметрах, необходимо предварительно загрузить
        ///     калибровочный файл. Если профиль должен выдаваться в пикселях, загружать файл калибровки при помощи функции
        ///     <see cref="LoadCalibrationV2" /> не обязательно.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="mmEn">
        ///     Единицы измерения координат точек профиля: true, если координаты точек профиля необходимо выдавать в
        ///     миллиметрах; false – если координаты точек профиля необходимо выдавать в пикселях.
        /// </param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int CalibrationModeV2(IntPtr sensorHandle, bool mmEn);

        /// <summary>
        ///     Уничтожает объект датчика, который был создан функцией <see cref="CreateSensorV2" />, и все выделенные для него
        ///     ресурсы. После работы функции обращение к объекту датчика через дескриптор становится невозможным.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int DisposeSensor(IntPtr sensorHandle);

        /// <summary>
        ///     Позволяет получить количество кадров, хранящихся в ОЗУ устройства. Режим сохранения данных в ОЗУ предназначен
        ///     только для видеоданных.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="frameCount">Количество кадров</param>
        /// <param name="bufferSize">
        ///     Размер буфера. Данный параметр не является размером буфера и его можно не передавать и не
        ///     принимать.
        /// </param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int GetRamValue(IntPtr sensorHandle, out ushort frameCount, out uint bufferSize);

        /// <summary>
        ///     Передает все видеоданные, которые имеются в ОЗУ, в указанный пользователем буфер. Пользователь должен предоставить
        ///     буфер соответствующего (или максимального) размера, т.е. равного объему ОЗУ устройства. В процессе передачи новые
        ///     данные в буфере не накапливаются.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="buffer">Буфер для получения данных</param>
        /// <param name="bufferSize">Размер буфера для получения данных (не используется и его значение функцией не учитывается)</param>
        /// <param name="nFrameDownloaded">Количество полученных кадров</param>
        /// <param name="error">Погрешность - количество байт, оставшихся в ОЗУ после завершения загрузки</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int GetRawRamDataV2(IntPtr sensorHandle, out byte[] buffer, int bufferSize,
            out int nFrameDownloaded, out int error);

        /// <summary>
        ///     Передает в датчик размер рабочего окна. Пиксель с нулевыми координатами находится в верхнем левом углу матрицы, как
        ///     показано на рис. Пиксель с координатами (<paramref name="left" />, <paramref name="top" />) является верхним левым
        ///     пикселем
        ///     окна. Пиксель с координатами (<paramref name="left" /> + <paramref name="width" />, <paramref name="top" /> +
        ///     <paramref name="height" />) находится за пределами окна. Если необходимо работать по всей матрице, например, по
        ///     матрице
        ///     размером 2048x1088 следует задать окно с координатами: <paramref name="left" /> = 0, <paramref name="top" /> = 0,
        ///     <paramref name="width" /> = 2048, <paramref name="height" /> = 1088
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="left">
        ///     Координата левой границы окна на оси Х, выраженная в пикселях. Эта координата принадлежит рабочей
        ///     области окна.
        /// </param>
        /// <param name="top">
        ///     Координата верхней границы окна на оси Y, выраженная в пикселях. Эта координата принадлежит рабочей
        ///     области окна.
        /// </param>
        /// <param name="width">
        ///     Ширина окна. Последняя рабочая координата по оси Х определяется как <paramref name="left" /> +
        ///     <paramref name="width" /> - 1.
        /// </param>
        /// <param name="height">
        ///     Высота окна. Последняя рабочая координата по оси Y определяется как <paramref name="top" /> +
        ///     <paramref name="height" /> - 1.
        /// </param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetWindow(IntPtr sensorHandle, ushort left, ushort top, ushort width, ushort height);

        /// <summary>
        ///     Управляет включением лазера. Если работа лазера разрешена, лазер будет срабатывать в момент экспозиции кадра. Если
        ///     работа лазера запрещена, лазер не будет срабатывать в момент экспозиции кадра. Данная функция может быть аппаратно
        ///     отключена при изготовлении профилометра по заказу. В этом случае работа лазера всегда разрешена.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="state">0x01 - работа лазера разрешена. 0х00 - работа лазера запрещена</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int Laser(IntPtr sensorHandle, byte state);

        /// <summary>
        ///     Устанавливает параметры обработки пятен на изображении, полученном с фотоматрицы, с целью формирования данных
        ///     профиля. На режим передачи видео данная функция не оказывает влияния.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="maxSpotsCount">Максимальное количество точек на столбце (от 0 до 255)</param>
        /// <param name="level">Пороговый уровень, сигнал ниже которого не является пятном (от 0 до 1023)</param>
        /// <param name="minWidth">Минимальная ширина пятна (от 0 до 1087)</param>
        /// <param name="maxWidth">Максимальная ширина пятна (от 0 до 1087)</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetParameters(IntPtr sensorHandle, ushort maxSpotsCount, ushort level, ushort minWidth,
            ushort maxWidth);

        /// <summary>
        ///     Управляет включением режима сортировки координат L. Если данная функция включена, массив точек, передаваемый в
        ///     управляющее ПО, будет отсортирован в порядке увеличения координат L, выраженных в миллиметрах. Это может облегчить
        ///     дальнейшую работу с профилем. Сортировка происходит непосредственно в момент формирования массива данных, поэтому
        ///     создает лишь незначительную дополнительную нагрузку на процессор.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="ascendingEn">Сортировка включена?</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int AscendingL(IntPtr sensorHandle, bool ascendingEn);

        ///// <summary>
        ///// Возвращает значение счётчика импульсов запуска и счётчика полученных кадров.
        ///// </summary>
        ///// <param name="sensorHandle">Дескриптор объекта датчика</param>
        ///// <param name="frameCount">Количество кадров</param>
        ///// <param name="strobeCount">Количество импульсов запуска</param>
        ///// <returns></returns>
        //[DllImport(DriverDllName, CallingConvention = Convention)]
        //public static extern int FrameStrobeCount(IntPtr sensorHandle, out int frameCount, out int strobeCount);

        /// <summary>
        ///     Устанавливает математические параметры, необходимые для формирования таблицы коэффициентов, которая затем
        ///     используется для прямого преобразования пикселей в миллиметры. Так как таблица коэффициентов нуждается в создании
        ///     только для bin-файлов, данная функция также необходима только при работе с bin-файлами. Файлы fst не требуют
        ///     установки математических параметров, так как они уже содержат в себе таблицу необходимых коэффициентов. Данная
        ///     функция требуется также для создания fst-файла по файлу bin.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="data">
        ///     массив с математическими параметрами в следующем формате (все значения запаса выражены в пикселях):
        ///     <para>[0] = порядок полинома (0 - полином 6-го порядка, 1 - полином 3-го порядка);</para>
        ///     <para>[1] = запас по высоте (Z) вверх;</para>
        ///     <para>[2] = запас по высоте (Z) вниз;</para>
        ///     <para>[3] = запас по ширине (W) вверх;</para>
        ///     <para>[4] = запас по ширине (W) вниз;</para>
        ///     <para>[5] = запас по ширине (W) влево;</para>
        ///     <para>[6] = запас по ширине (W) влево;</para>
        /// </param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetGlobalMathParam(IntPtr sensorHandle, float[] data);

        /// <summary>
        ///     Управляет включением режима антидребезга сигнала внешней синхронизации. Если фронт сигнала синхронизации зашумлен,
        ///     данная функция может помочь избавиться от срабатывания фотоматрицы несколько раз подряд по одному фронту, задав
        ///     время задержки, в течение которого переходные процессы на фронте должны прекратиться. Время задержки задается в
        ///     периодах тактовой частоты:
        ///     <para>•	для режима «профиль» - тактовая частота равна 32 МГц;</para>
        ///     <para>•	для режима «видео» - тактовая частота равна 30 МГц.</para>
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="value">Время задержки. Значение ограничено величиной 2^24 – 1 (три байта).</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetDebounce(IntPtr sensorHandle, uint value);

        /// <summary>
        ///     Считывает значение цифрового усиления видеосигнала.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="gain">Значение усиления</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int GetGain(IntPtr sensorHandle, out byte gain);

        /// <summary>
        ///     Задает значение цифрового усиления видеосигнала.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="gain">Значение усиления</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetGain(IntPtr sensorHandle, byte gain);

        /// <summary>
        ///     Считывает значение аналогового усиления видеосигнала.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="gain">Значение усиления (от 0 до 7)</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int GetAnalogGain(IntPtr sensorHandle, out byte gain);

        /// <summary>
        ///     Задает значение аналогового усиления видеосигнала.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="gain">Значение усиления (от 0 до 7)</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetAnalogGain(IntPtr sensorHandle, byte gain);

        /// <summary>
        ///     Считывает значение времени экспозиции в условных единицах, одна условная единица приблизительно равна 4 мкс.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="exposition">Значение экспозиции</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int GetExposition(IntPtr sensorHandle, out uint exposition);

        /// <summary>
        ///     Задает значение времени экспозиции в условных единицах, одна условная единица приблизительно равна 4 мкс.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="exposition">Значение экспозиции</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetExposition(IntPtr sensorHandle, uint exposition);

        /// <summary>
        /// Установка количества кадров, выполняемых камерой на один строб (любой, внутренний, внешний или программный)
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="numberFrames">Количество кадров</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetNumFrames(IntPtr sensorHandle, ushort numberFrames);

        /// <summary>
        /// Управляет включением конвейерного (pipeline) режима формирования кадров.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="en">Разрешение pipeline-режима</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int PipelineModeEn(IntPtr sensorHandle, bool en);

        /// <summary>
        ///     Коррекция опорного уровня черного
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="en">Разрешение коррекции</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int BlackReferenceEn(IntPtr sensorHandle, bool en);

        ///// <summary>
        /////     Установка параметров автоэксозици
        ///// </summary>
        ///// <param name="sensorHandle">Дескриптор объекта датчика</param>
        ///// <param name="minExp">Минимальная экспозиция</param>
        ///// <param name="maxExp">Максимальная экспозиция</param>
        ///// <param name="stepExp">Шаг перестройки экспозиции</param>
        ///// <param name="nSkip">Количество пропускаемых кадров на один икремент экспозиции</param>
        ///// <param name="wOpt">Целевое значение ширины пятна</param>
        ///// <param name="minNPoints">Минимально допустимое количество точек</param>
        ///// <returns></returns>
        //[DllImport(FileName, CallingConvention = Convention)]
        //public static extern int AutoExposition(IntPtr sensorHandle, uint minExp, uint maxExp, ushort stepExp,
        //    ushort nSkip, float wOpt, uint minNPoints);

        ///// <summary>
        /////     Разрешение работы автоэкспозиции
        ///// </summary>
        ///// <param name="sensorHandle">Дескриптор объекта датчика</param>
        ///// <param name="en">Разрешение работы</param>
        ///// <returns></returns>
        //[DllImport(FileName, CallingConvention = Convention)]
        //public static extern int AutoExpositionEn(IntPtr sensorHandle, bool en);

        /// <summary>
        ///     Устанавливает коэффициент прореживания, который применяется к колонкам в процессе вычисления профиля.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="subsampling">Коэффициент прореживания профиля</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SubsamplingProfile(IntPtr sensorHandle, byte subsampling);

        /// <summary>
        ///     Устанавливает коэффициент прореживания, который применяется к строкам матрицы в процессе вычисления профиля.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="subsampling">Количество пропускаемых строк матрицы</param>
        /// <param name="advMode">
        ///     Используется расширенный режим работы матрицы с учетом паттерна Байера? (по умолчанию
        ///     используется обычный режим работы матрицы)
        /// </param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SubsamplingMatrix(IntPtr sensorHandle, byte subsampling, bool advMode);

        /// <summary>
        ///     Считывает значение температуры, измеренной датчиком, встроенным в модуль цифровой обработки видеризображения
        ///     АВШБ.467469.003 (ЦСП профилометра). Полученное значение выражено в градусах Цельсия.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="temper">Значение температуры</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int GetTemper(IntPtr sensorHandle, out float temper);

        /// <summary>
        ///     Задает параметры коррекции профиля. При помощи этих коэффициентов можно настроить профилометр таким образом, чтобы
        ///     измеренные им координаты совпадали с выбранной пользователем физической системой координат. Функция справедлива
        ///     только в том случае, если профиль измеряется в миллиметрах. Коэффициенты коррекции, поворот и смещения применяются
        ///     к нулевой точке датчика (точка с координатами 0;0 [мм]) в локальной системе координат датчика.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="angleCorrection">Угол поворота профиля, заданный в градусах</param>
        /// <param name="ratioLCorrection">
        ///     Коэффициент коррекции по ширине. Координата L [мм] умножается на этот коэффициент, что позволяет сжать
        ///     или растянуть профиль вдоль оси L (значение по умолчанию: 1)
        /// </param>
        /// <param name="ratioDCorrection">
        ///     Коэффициент коррекции по дальности. Координата D [мм] умножается на этот коэффициент, что позволяет
        ///     сжать или растянуть профиль вдоль оси D (значение по умолчанию: 1)
        /// </param>
        /// <param name="shiftLCorrection">
        ///     Величина смещения, выраженная в миллиметрах, позволяющая сместить весь профиль сторону увеличения
        ///     (+) или уменьшения (-) координаты L (значение по умолчанию: 0)
        /// </param>
        /// <param name="shiftDCorrection">
        ///     Величина смещения, выраженная в миллиметрах, позволяющая сместить весь профиль сторону увеличения
        ///     (+) или уменьшения (-) координаты D (значение по умолчанию: 0)
        /// </param>
        /// <param name="hFlipCorrection">Отражение профиля по горизонтали (относительно вертикальной оси). По умолчанию отключено.</param>
        /// <param name="cFlipCorrection">Отражение профиля по вертикали (относительно горизонтальной оси). По умолчанию отключено.</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int Correction(IntPtr sensorHandle,
                                            float angleCorrection,
                                            float ratioLCorrection,
                                            float ratioDCorrection,
                                            float shiftLCorrection,
                                            float shiftDCorrection,
                                            bool hFlipCorrection,
                                            bool cFlipCorrection);

        /// <summary>
        ///     Управляет включением коррекции профиля, не изменяя уже заданные значения.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="en">
        ///     Разрешение применения коррекции. Если коррекция запрещена (false), коэффициенты коррекции не будут
        ///     применены к профилю даже в том случае, если они заданы.
        /// </param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int CorrectionEn(IntPtr sensorHandle, bool en);

        /// <summary>
        ///     Устанавливает границы рабочей зоны для профиля. Если точка итогового профиля (после применения коррекции) выйдет за
        ///     указанные границы, она будет исключена из передаваемого в управляющее ПО массива. Значения границ не входят в
        ///     рабочий диапазон. Другими словами, число, точно равное границе, не будет включено в массив передаваемых данных.
        ///     Значения границ задаются в миллиметрах.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="left">Горизонтальная граница рабочей зоны слева</param>
        /// <param name="right">Горизонтальная граница рабочей зоны справа</param>
        /// <param name="top">Вертикальная граница рабочей зоны сверху</param>
        /// <param name="bottom">Вертикальная граница рабочей зоны снизу</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int Borders(IntPtr sensorHandle, float left, float right, float top, float bottom);

        /// <summary>
        ///     Управляет применением установленных границ.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="latEn">Применение границ по горизонтали?</param>
        /// <param name="distEn">Применение границ по вертикали?</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int BordersEn(IntPtr sensorHandle, bool latEn, bool distEn);

        /// <summary>
        ///     Устанавливает параметры для режима слежения.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="minPoints">Минимальное количество точек объекта</param>
        /// <param name="mode">Режим слежения</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int Tracking(IntPtr sensorHandle, int minPoints, byte mode);

        //public static extern int Tracking(IntPtr sensorHandle, int nTargets, int criteria, int boxH, int boxW, int nFullRefresh, int nSubRefresh, int interval, int minPoints, int refPoint, byte mode);

        /// <summary>
        ///     Управляет включением режима слежения.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="en">Включить режим слежения?</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int TrackingEn(IntPtr sensorHandle, bool en);

        /// <summary>
        ///     Кусочная экспозиция.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="en">Необходимо использовать?</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int PiecewizeLinearModeEn(IntPtr sensorHandle, bool en);

        /// <summary>
        ///     Задает параметры кусочной экспозиции.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="nSlopes">Количество точек излома кусочной экспозиции</param>
        /// <param name="xKp1">Положение первой точки излома на оси времени, в процентах, где 0 - крайнее левое положение</param>
        /// <param name="xKp2">Положение второй точки излома на оси времени, в процентах, где 0 - крайнее левое положение</param>
        /// <param name="slopeKp1">Наклон характеристики после первой точки излома, в процентах, где 0 - минимальный наклон</param>
        /// <param name="slopeKp2">Наклон характеристики после второй точки излома, в процентах, где 0 - минимальный наклон</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int PiecewizeLinearMode(IntPtr sensorHandle, byte nSlopes, byte xKp1, byte xKp2,
            byte slopeKp1, byte slopeKp2);

        ///// <summary>
        ///// Режим мастер-слейв слежения
        ///// </summary>
        ///// <param name="sensorHandle"></param>
        ///// <param name="master">режим мастера</param>
        ///// <param name="slave">режим слейва</param>
        ///// <param name="corrX">Коррекция окна по Х (доп. смещение)</param>
        ///// <param name="corrY">Коррекция окна по Y (доп. смещение)</param>
        ///// <returns></returns>
        //[DllImport(DllName, CallingConvention = Convention)]
        //public static extern int TrackingMasterSlave(IntPtr sensorHandle, bool master, bool slave, int corrX, int corrY);

        /// <summary>
        ///     Управляет включением инверсии лазера.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="en">Включить инверсию лазера?</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int LaserInversion(IntPtr sensorHandle, bool en);

        /// <summary>
        ///     Переводит датчик в режим измерения профиля, когда объектом обработки (колонкой) на изображении является строка. В
        ///     процессе получения данных с профилометра не осуществляется контроль доставки пакетов (без подтверждения).
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SpotsNoAckColumn(IntPtr sensorHandle);

        /// <summary>
        ///     Переводит датчик в режиме измерения профиля, когда объектом обработки (колонкой) на изображении является столбец. В
        ///     процессе получения данных с профилометра не осуществляется контроль доставки пакетов (без подтверждения).
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SpotsNoAckRow(IntPtr sensorHandle);

        /// <summary>
        ///     Переключает профилометр в режим выдачи видеокадров и выделяет в памяти буфер, необходимый для временного хранения
        ///     полученного кадра, в соответствии с размером матрицы + заголовок 64 байта.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int Video(IntPtr sensorHandle);

        /// <summary>
        ///     Включает режим сохранения видеокадра в ОЗУ устройства. При этом при получении кадра вызов Callback-функций
        ///     происходить не будет. Вместо этого видеокадр будет сохраняться в ОЗУ устройства. Считать содержимое ОЗУ можно
        ///     отдельной командой. Включение режима сбрасывает работу фотоматрицы, т.е. приводит к сбросу установленной ранее
        ///     экспозиции и усиления. Данная команда также сбрасывает указатель текущего кадра, хранящегося в ОЗУ.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int VideoToRam(IntPtr sensorHandle);

        /// <summary>
        ///     Записывает значение в произвольный регистр по указанному адресу. Данная функция не является безопасной, поэтому
        ///     использовать ее не рекомендуется. В процессе записи необходимо понимать, к какому регистру осуществляется обращение
        ///     и как этот регистр влияет на режим работы. Запись неверных значений может повлиять на работу устройства, вплоть до
        ///     выхода его из строя. В общем случае использовать данную функцию не требуется, так как все необходимые действия
        ///     можно выполнить при помощи других, специально предназначенных для этого функций.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="reg">Адрес регистра</param>
        /// <param name="value">Значение, записываемое в регистр</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetRegisterValue(IntPtr sensorHandle, byte reg, ushort value);

        /// <summary>
        ///     Переводит датчик в режим однократного срабатывания по команде. Одна команда - одно срабатывание. Командой,
        ///     запускающей срабатывание датчика, является функция SendSync. Установка режима синхронизации по команде может
        ///     использоваться также для останова работы датчика.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SyncCmd(IntPtr sensorHandle);

        /// <summary>
        ///     Считывает значение из произвольного регистра по указанному адресу. В процессе чтения необходимо понимать, к какому
        ///     регистру осуществляется обращение и как его информация может быть интерпретирована. В общем случае использовать
        ///     данную функцию не требуется, так как все необходимые действия можно выполнить при помощи других, специально
        ///     предназначенных для этого функций.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="reg">Адрес регистра</param>
        /// <param name="value">Значение, записываемое в регистр</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int GetRegisterValue(IntPtr sensorHandle, byte reg, out ushort value);

        /// <summary>
        ///     Переводит датчик в режим срабатывания по внутреннему таймеру. При этом профилометр начинает выполнять измерения с
        ///     указанной частотой, которая не является синхронной с другими профилометрами в системе.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="frameRate">Частота кадров, выраженная в [Гц]</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SyncNone(IntPtr sensorHandle, int frameRate);

        /// <summary>
        ///     Переводит датчик в особый режим синхронизации по внешнему импульсу. Данный тип синхронизации используется, если
        ///     профилометр аппаратно настроен на передачу импульса запуска непосредственно на фотоматрицу, что обеспечивает
        ///     максимально быструю реакцию.  Это стандартный режим внешней синхронизации. Аппаратная настройка выполнятся в
        ///     процессе изготовления профилометра.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SyncExt(IntPtr sensorHandle);

        /// <summary>
        ///     Переводит датчик в режим синхронизации по внешнему импульсу, поступающему на один из входов Q0-Q7. Данный тип
        ///     внешней синхронизации следует выключать, если профилометр аппаратно настроен для передачи импульса запуска через
        ///     центральный сигнальный процессор (ЦСП), что позволяет контролировать запуск микропрограммой процессора. Это
        ///     нестандартный режим, для работы в котором профилометр настраивается по отдельному заказу. Аппаратная настройка
        ///     выполнятся в процессе изготовления профилометра.
        /// </summary>
        /// <param name="handle">Дескриптор объекта датчика</param>
        /// <param name="strobeMode">
        ///     Выбор аппаратного входа синхронизации и фронта срабатывания. Возможные значения:
        ///     <para>0x00: аппаратный вход Q0, срабатывание по положительному фронту;</para>
        ///     <para>0x01: аппаратный вход Q0, срабатывание по отрицательному фронту;</para>
        ///     <para>0x02: аппаратный вход Q1, срабатывание по положительному фронту;</para>
        ///     <para>0x03: аппаратный вход Q1, срабатывание по отрицательному фронту;</para>
        ///     <para>0x04: аппаратный вход Q2, срабатывание по положительному фронту;</para>
        ///     <para>0x05: аппаратный вход Q2, срабатывание по отрицательному фронту;</para>
        ///     <para>0x06: аппаратный вход Q3, срабатывание по положительному фронту;</para>
        ///     <para>0x07: аппаратный вход Q3, срабатывание по отрицательному фронту;</para>
        ///     <para>0x08: аппаратный вход Q4, срабатывание по положительному фронту;</para>
        ///     <para>0x09: аппаратный вход Q4, срабатывание по отрицательному фронту;</para>
        ///     <para>0x0А: аппаратный вход Q5, срабатывание по положительному фронту;</para>
        ///     <para>0x0B: аппаратный вход Q5, срабатывание по отрицательному фронту;</para>
        ///     <para>0x0C: аппаратный вход Q6, срабатывание по положительному фронту;</para>
        ///     <para>0x0D: аппаратный вход Q6, срабатывание по отрицательному фронту;</para>
        ///     <para>0x0E: аппаратный вход Q7, срабатывание по положительному фронту;</para>
        ///     <para>0x0F: аппаратный вход Q7, срабатывание по отрицательному фронту.</para>
        /// </param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SyncFromStrobe(IntPtr handle, int strobeMode);

        /// <summary>
        ///     Вызывает однократное срабатывание датчика, при условии, что тот находится в режиме «Синхронизация по команде».
        ///     Получив данные, библиотека вызывает соответствующую Callback-функцию.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SendSync(IntPtr sensorHandle);

        /// <summary>
        ///     Отправка на профилометр команды «стоп». При этом оборудование профилометра перестаёт посылать данные (останова
        ///     потока получения данных при этом не происходит). Функция не возвращает управление до тех пор, пока профилометр не
        ///     прекратит передачу данных или не произойдет ошибка таймаута (1 секунда).
        ///     Данная функция необходима для гарантированного успешного изменения параметров профилометра, если тот занят
        ///     передачей больших массивов данных в потоке. В противном случае возможно аппаратное зависание профилометра, которое
        ///     можно будет устранить только пересбросом питания.
        ///     <para>
        ///         Приводит к сбросу следующих параметров в значения по умолчанию:
        ///         <para>Усиление (функция SetGain) в значение 40;</para>
        ///     </para>
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Код ошибки</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int Stop(IntPtr sensorHandle);

        /// <summary>
        ///     Создает калибровочный файл в виде таблицы быстрого пересчета (.fst) по исходному калибровочному файлу (.bin). В
        ///     процессе работы для каждой точки матрицы вычисляется соответствующее ей значение, выраженное в миллиметрах. Набор
        ///     значений в миллиметрах представляет собой таблицу быстрого пересчета. Попутно создаются текстовые служебные файлы
        ///     fast_table_width.txt и fast_table_dist.txt.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="binFileName">Путь и имя исходного бинарного калибровочного файла (.bin)</param>
        /// <param name="fastTableFileName">Путь и имя полученного в результате файла с таблицей быстрого пересчета (.fst)</param>
        /// <param name="typeMatrix">
        ///     Тип фотоматрицы:
        ///     <para>0: матрица размером 1088 точек по шкале дальности и 2048 точек по шкале ширины;</para>
        ///     <para>1: матрица размером 2048 х 2048;</para>
        ///     <para>2: матрица размером 2048 точек по шкале дальности и 1088 точек по шкале ширины.</para>
        /// </param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int CreateFastTable(IntPtr sensorHandle, string binFileName, string fastTableFileName,
            int typeMatrix);

        /// <summary>
        ///     Считывает текущие параметры объекта датчика общего характера, представленные в виде структуры типа
        ///     <see cref="GeneralParamsSet" />. Функция возвращает указатель на структуру.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Указатель на структуру</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern IntPtr StatusGen(IntPtr sensorHandle);

        /// <summary>
        ///     Считывает текущие параметры коррекции профиля, представленные в виде структуры типа
        ///     <see cref="CorrectionParamsSet" />
        ///     . Функция возвращает указатель на структуру.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Указатель на структуру</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern IntPtr StatusCorr(IntPtr sensorHandle);

        /// <summary>
        ///     Считывает текущие границы рабочей области, представленные в виде структуры типа <see cref="BordersParamsSet" />.
        ///     Функция возвращает указатель на структуру.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Указатель на структуру</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern IntPtr StatusBord(IntPtr sensorHandle);

        /// <summary>
        ///     Считывает текущие параметры видеоматрицы, представленные в виде структуры типа <see cref="MatrixParamsSet" />.
        ///     Функция возвращает указатель на структуру.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <returns>Указатель на структуру</returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern IntPtr StatusMatrix(IntPtr sensorHandle);

        /// <summary>
        ///     Пересчитывает одну точку, представленную в виде координат на матрице (x и y в пикселях) в точку на реальной
        ///     миллиметровой плоскости,
        ///     представленную в виде координат по дальности и ширине, по загруженной в объект датчика калибровочной таблице.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="x">Координата X точки на матрице, в пикселях</param>
        /// <param name="y">Координата Y точки на матрице, в пикселях</param>
        /// <param name="latitude">Координата по ширине, в миллиметрах</param>
        /// <param name="distance">Координата по дальности, в миллиметрах</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int OnePointToMm(IntPtr sensorHandle, int x, float y, out float latitude, out float distance);

        ///// <summary>
        ///// Фильтрация бликов методом опорного профиля
        ///// </summary>
        ///// <param name="sensorHandle"></param>
        ///// <param name="en">Работа функции разрешена</param>
        ///// <returns></returns>
        //[DllImport(DllName, CallingConvention = Convention)]
        //public static extern int ReferenceProfileEn(IntPtr sensorHandle, bool en);

        ///// <summary>
        /////     Пересчитывает одну точку, представленную в виде координат на матрице (x и y в пикселях) в точку на реальной
        /////     миллиметровой плоскости,
        /////     представленную в виде координат по дальности и ширине, по загруженной в объект датчика калибровочной таблице.
        ///// </summary>
        ///// <param name="sensorHandle">Дескриптор объекта датчика</param>
        ///// <param name="vertical">Размер окна по вертикали (поперек строк матрицы), в пикселях</param>
        ///// <param name="horizontal">Размер окна по горизонтали (вдоль строк матрицы), в пикселях</param>
        ///// <param name="nInArea">
        /////     Минимальное количество точек в пределах окна матрицы, не считая текущей,
        /////     при котором принимается решение "Точка является полезной"
        ///// </param>
        ///// <returns></returns>
        //[DllImport(FileName, CallingConvention = Convention)]
        //public static extern int NoiseRemover(IntPtr sensorHandle, int vertical, int horizontal, int nInArea);

        /// <summary>
        ///     Управляет включением удаления отдельных шумовых точек методом окна.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="en">Разрешение работы функции</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int NoiseRemoverEn(IntPtr sensorHandle, bool en);

        /// <summary>
        ///     Задает параметры удаления шумовых точек методом плавучего окна.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="vertical">Размер окна по вертикали (поперек строк матрицы), в пикселях</param>
        /// <param name="horizontal">Размер окна по горизонтали (вдоль строк матрицы), в пикселях</param>
        /// <param name="nInArea">
        ///     Минимальное количество точек в пределах окна матрицы, не считая текущей,
        ///     при котором принимается решение "Точка является полезной"
        /// </param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int NoiseFloatRemover(IntPtr sensorHandle, int vertical, int horizontal, int nInArea);

        /// <summary>
        ///     Управляет включением удаления шумовых точек методом плавучего окна.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="en">Разрешение работы функции</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int NoiseFloatRemoverEn(IntPtr sensorHandle, bool en);

        /// <summary>
        ///     Задает параметры дискретизации, позволяющие получить линейный массив в формате "y[i]",
        ///     данные в котором следуют строго с заданным шагом дискретизации.
        ///     Длина такого массива известна и определяется левой и правой границей рабочей зоны датчика.
        ///     Если в текущую ячейку массива в процессе дискретизации профиля не попадает ни одной точки, значение в ячейке равно
        ///     0.
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="step">Шаг передискретизации, в миллиметрах</param>
        /// <param name="mode">
        ///     Режим усреднения точек, попадающих в одну ячейку массива: 0 - математическое усреднение, 1 -
        ///     приоритет максимуму, 2 - приоритет минимуму
        /// </param>
        /// <param name="nearValue">Режим заполнения пустот ближайшим значением: 0 - заполнение не выполняется</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int Discretize(IntPtr sensorHandle, float step, byte mode, byte nearValue);

        /// <summary>
        ///     Установка значения-заменителя отсутствующих значений в массиве дискретизированных данных
        /// </summary>
        /// <param name="sensorHandle"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int SetMissingDesignator(IntPtr sensorHandle, float value);

        /// <summary>
        ///     Управляет включением вычитания постоянной составляющей, измеренной медианой
        /// </summary>
        /// <param name="sensorHandle"></param>
        /// <param name="en">Разрешение режима</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int ZeroDC(IntPtr sensorHandle, bool en);

        /// <summary>
        ///     Управляет включением передискретизации профиля в линейный массив
        /// </summary>
        /// <param name="sensorHandle">Дескриптор объекта датчика</param>
        /// <param name="en">Разрешение передискретизации</param>
        /// <returns></returns>
        [DllImport(FileName, CallingConvention = Convention)]
        public static extern int DiscretizeEn(IntPtr sensorHandle, bool en);

        #endregion
    }
}
