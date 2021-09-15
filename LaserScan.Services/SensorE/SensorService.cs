using Kogerent.Core;
using Kogerent.Logger;
using Kogerent.Services.Interfaces;
using Kogerent.Utilities;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using se = Kogerent.Core.SensorELibrary;

namespace Kogerent.Services.Implementation
{
    /// <summary>
    /// Сервис для работы с профилометрами
    /// </summary>
    public class SensorService : ViewModelBase, ISensorService
    {
        #region Private fields
        private static readonly Type TypeObject = typeof(SensorSettings);
        private object Locker = new object();
        private readonly Stopwatch sw = new Stopwatch();
        #endregion

        #region Properties

        #region Callback Delegates

        /// <summary>
        /// Делегат на получение профиль-коллбэков от СенсорЕ
        /// </summary>
        public se.ProfileDelegate ProfileDelegate { get; set; }

        /// <summary>
        /// Делегат на получение видео-коллбэков от СенсорЕ
        /// </summary>
        public se.VideoDelegate VideoDelegate { get; set; }

        /// <summary>
        /// делегат на получение коллбэков по автоэкспоиции
        /// </summary>
        public se.AutoExpDelegate AutoExpDelegate { get; set; }
        #endregion

        /// <summary>
        /// Логгер для дебага
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Сервис для записей в лог интерфейса
        /// </summary>
        public IFooterRepository FooterRepository { get; }

        /// <summary>
        /// Флаг получения данных
        /// </summary>
        public bool DataRecieved { get; set; }

        /// <summary>
        /// Пул для создания коллбэков
        /// </summary>
        public ICallbackPool CallbackPool { get; set; }

        /// <summary>
        /// Коллбэки, приходящие от профилометра
        /// </summary>
        public CallbackData LastCallbackData { get; set; }

        #region Work With Ram

        /// <summary>
        /// Количество кадров в азу
        /// </summary>
        public ushort RamFrameCount { get; set; }

        /// <summary>
        /// размер азу буфера
        /// </summary>
        public byte[] RawRamDataBuffer { get; set; }

        /// <summary>
        /// Номер кадра из азу
        /// </summary>
        public int NFrameDownloadedFromRam { get; set; }

        /// <summary>
        /// Номер кадра из азу
        /// </summary>
        public int RemainingBytesInRam { get; set; }
        #endregion

        #region Params
        /// <summary>
        /// Структура общих параметров
        /// </summary>
        public GeneralParamsSet StatusGeneralParamsSet { get; set; }

        /// <summary>
        /// Структура параметров для коррекции
        /// </summary>
        public CorrectionParamsSet StatusCorrectionParamsSet { get; set; }

        /// <summary>
        /// Стректура параметров границ
        /// </summary>
        public BordersParamsSet StatusBordersParamsSet { get; set; }


        /// <summary>
        /// Структура параметров для матрицы
        /// </summary>
        public MatrixParamsSet StatusMatrixParamsSet { get; set; }
        #endregion
        /// <summary>
        /// Объект настроек профилометра
        /// </summary>
        public SensorSettings SensorSettings { get; set; }

        /// <summary>
        /// Объект настроек системы
        /// </summary>
        public SystemSettings SystemSettings { get; set; }

        /// <summary>
        /// Код последней ошибки
        /// </summary>
        public int LastErrorCode { get; set; }

        /// <summary>
        /// Код ошибок
        /// </summary>
        public ErrorCode GetLastErrorCode => (ErrorCode)LastErrorCode;

        /// <summary>
        /// Название файла библиотеки СенсорЕ
        /// </summary>
        public string FileName { get; set; } = "SensorE.dll";

        /// <summary>
        /// Режим синхронизации по стробу
        /// </summary>
        public int SyncFromStrobeMode { get; set; }

        /// <summary>
        /// Счетчик пришедших данных
        /// </summary>
        public int DataCounter { get; set; }
        #endregion

        /// <summary>
        /// Событие сразу после получения данных от датчика.
        /// Внимание. Слушатели не должны надолго захватывать управление, а лишь принять данные и положить их в некий буфер.
        /// Иначе внутренние буферы приема данных от датчика могут переполниться.    
        /// </summary>
        public event EventHandler<CallbackData> CallbackDataReceived;

        private CallbackData _lastCallback;
        public CallbackData LastCallBack
        {
            get { return _lastCallback; }
            set { SetProperty(ref _lastCallback, value); }
        }

        #region Constructors


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public SensorService(ILogger logger, IFooterRepository footerRepository, ICallbackPool callbackPool)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Logger = logger;
            FooterRepository = footerRepository;
            ProfileDelegate = CallbackProfile;
            VideoDelegate = CallbackVideo;
            CallbackPool = callbackPool;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Универсальный метод для вызова функций с записью в лог
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Метода не существует</exception>
        public int Call(Func<int> method, string methodName = null)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (methodName == null) methodName = method.Method.Name;

            sw.Restart();
            LastErrorCode = method.Invoke();
            sw.Stop();

            return Log(methodName, LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        ///     Считывает значение из регистра датчика по указанному адресу.
        /// ВНИМАНИЕ! Не рекомендуется использовать данный метод! Только для низкоуровневой отладки.
        /// [Безопасный вызов аналогичный вызову <see cref="Call"/>]
        /// </summary>
        /// <param name="nReg">Номер регистра датчика, значение которого необходимо считать.</param>
        /// <param name="value">Считанное значение из регистра датчика.</param>
        /// <returns>Успешное выполнение?</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetRegisterValue(byte nReg, out ushort value)
        {
            value = 0;
            ValidateObjectExist();

            sw.Restart();
            LastErrorCode = se.GetRegisterValue(SensorSettings.Handle, nReg, out value);
            sw.Stop();

            return Log(nameof(GetRegisterValue), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        ///     Устанавливает значение в регистр датчика по указанному адресу.
        /// ВНИМАНИЕ! Не рекомендуется использовать данный метод! Только для низкоуровневой отладки.
        /// [Безопасный вызов аналогичный вызову <see cref="Call"/>]
        /// </summary>
        /// <param name="nReg">Номер регистра датчика, в которой записать значение.</param>
        /// <param name="value">Записываемое значение в регистр датчика.</param>
        /// <returns>Успешное выполнение?</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetRegisterValue(byte nReg, ushort value)
        {
            const string methodName = nameof(SetRegisterValue);
            ValidateObjectExist();

            sw.Restart();
            LastErrorCode = se.SetRegisterValue(SensorSettings.Handle, nReg, value);
            sw.Stop();

            return Log(methodName, LastErrorCode, sw.ElapsedMilliseconds);
        }

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
        public int OnePointToMm(int x, float y, out float latitude, out float distance)
        {
            latitude = 0; distance = 0;

            ValidateObjectExist();

            sw.Restart();
            LastErrorCode = se.OnePointToMm(SensorSettings.Handle, x, y, out latitude, out distance);
            sw.Stop();

            return Log(nameof(OnePointToMm), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        ///     Считывает версию используемой библиотеки SensorE.dll в <see cref="SystemSettings.DllVersion"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        public int GetSensorEVersion()
        {
            const byte length = 22;
            var versionPtr = Marshal.AllocHGlobal(length);

            se.VersionV2(versionPtr);
            string version = Marshal.PtrToStringAnsi(versionPtr);
            Marshal.FreeHGlobal(versionPtr);

            string[] ver = version.Split('(');
            SystemSettings.DllVersion = ver[0].TrimEnd();

            return 0;
        }

        /// <summary>
        /// Считывает версию прошивки датчика в <see cref="SensorSettings.HWVersion"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetFirmwareVersion()
        {
            ValidateObjectExist();

            const byte length = 21;
            var versionPtr = Marshal.AllocHGlobal(length);

            LastErrorCode = Call(() => se.VersionHwV2(SensorSettings.Handle, versionPtr));

            if (LastErrorCode >= 0)
            {
                const int nDigits = 3;
                var digitalVersion = new int[nDigits];
                Marshal.Copy(versionPtr, digitalVersion, 0, nDigits);
                SensorSettings.HWVersion = $"{digitalVersion[0]}.{digitalVersion[1]}.{digitalVersion[2]}";//digitalVersion.Select(d => ToString()).Join(".");
            }
            Marshal.FreeHGlobal(versionPtr);

            return LastErrorCode;
        }

        /// <summary>
        /// Включает или выключает режим отладки в библиотеке SensorE.dll в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        public int SetDebugMode()
        {
            return Call(() => se.DebugMode(SystemSettings.DebugMode));
        }

        /// <summary>
        /// Включает или выключает режим отладки потока приема данных в библиотеке SensorE.dll в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        public int SetDebugModeListener()
        {
            return Call(() => se.DebugModeListener(SystemSettings.DebugModeListener));
        }

        /// <summary>
        /// Включает или выключает расширенный формат заголовка принимаемых данных из библиотеки SensorE.dll в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        public int SetEnhanceHeader()
        {
            return Call(() => se.EnhanceHeader(SystemSettings.EnhanceHeader));
        }

        /// <summary>
        /// Задает диапазон портов, который будет использоваться в запускаемом приложении для соединения с датчиками, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        public int SetPortRange()
        {
            return Call(() => se.SetPortRange(SystemSettings.FirstPort, SystemSettings.LastPort));
        }

        /// <summary>
        /// Создает объект датчика в библиотеке SensorE.dll в соответствие с настройками и возвращает его дескриптор.
        /// Помимо этого, выбирает порты для передачи данных с датчиков,
        /// устанавливает соединение с датчиком, выполняет команду останова <see cref="Stop"/>,
        /// и считывает необходимые характеристики (например, версия прошивки) датчиков для правильной работы с ним.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        public int CreateSensor()
        {
            lock (Locker)
            {
                var versionPtr = Marshal.AllocHGlobal(22);
                sw.Restart();
                LastErrorCode = se.CreateSensorV3(SensorSettings.Ip, out IntPtr Handle, SensorSettings.ManualPortMode,
                                                  SensorSettings.ManualDataPort, versionPtr);
                sw.Stop();

                if (LastErrorCode >= 0)
                {
                    SensorSettings.Handle = Handle;
                    var digitalVersion = new int[3];
                    Marshal.Copy(versionPtr, digitalVersion, 0, 3);
                    SensorSettings.HWVersion = digitalVersion.Select(d => d.ToString()).Join(".");
                }
                Marshal.FreeHGlobal(versionPtr);
            }

            return Log(nameof(CreateSensor), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Создает объект виртуального датчика в библиотеке SensorE.dll в соответствие с настройками и возвращает его дескриптор.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        public int CreateVirtualSensor()
        {
            lock (Locker)
            {
                sw.Restart();
                LastErrorCode = se.CreateVirtualSensor(SensorSettings.Ip, out IntPtr handle);
                sw.Stop();

                if (LastErrorCode >= 0) SensorSettings.Handle = handle;
            }

            return Log(nameof(CreateVirtualSensor), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Включает или выключает режим передачи в ПК неполных данных в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetUncompletedData()
        {
            ValidateObjectExist();

            return Call(() => se.UncompletedData(SensorSettings.Handle, SensorSettings.UncompletedData));
        }

        /// <summary>
        /// Запускает поток получения данных от датчика.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int StartListener()
        {
            ValidateObjectExist();

            return Call(() => se.StartListener(SensorSettings.Handle, VideoDelegate, ProfileDelegate, AutoExpDelegate));
        }

        /// <summary>
        /// Загружает файл калибровки в датчик, который необходим для пересчета пикселей в миллиметры, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int LoadCalibration()
        {
            ValidateObjectExist();

            var fullName = Path.Combine("Calibrations", SensorSettings.CalibrationFileName);
            if (!File.Exists(fullName))
                throw new FileNotFoundException($"Файл калибровки не найден: {fullName}.");

            return Call(() => se.LoadCalibrationV2(SensorSettings.Handle, fullName));
        }

        /// <summary>
        /// Включает или выключает пересчет выдаваемого профиля из пикселей в миллиметры, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int AllowCalibration()
        {
            ValidateObjectExist();

            return Call(() => se.CalibrationModeV2(SensorSettings.Handle, SensorSettings.InMm));
        }

        /// <summary>
        ///     Уничтожает объект датчика, который был создан методом <see cref="CreateSensor"/> или <see cref="CreateVirtualSensor"/>.
        /// Если объект датчика не был создан ранее, то ничего не произойдет.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        public int DisposeSensor()
        {
            sw.Restart();
            if (!SensorSettings.IsExist)
            {
                sw.Stop();
                return Log(nameof(DisposeSensor), (int)ErrorCode.Success, sw.ElapsedMilliseconds);
            }

            lock (Locker)
            {
                LastErrorCode = se.DisposeSensor(SensorSettings.Handle);
                SensorSettings.Initialized = false;
                SensorSettings.CalibrationIsLoaded = false;
                if (LastErrorCode >= 0) SensorSettings.Handle = (IntPtr)null;
            }
            sw.Stop();
            return Log(nameof(DisposeSensor), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Считывает количество кадров, хранящихся в ОЗУ датчика, в <see cref="RamFrameCount"/>.
        /// Режим сохранения данных в ОЗУ предназначен только для видеоданных.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetRamFramesCount()
        {
            ValidateObjectExist();

            sw.Restart();
            LastErrorCode = se.GetRamValue(SensorSettings.Handle, out var value, out var _);
            sw.Stop();

            if (LastErrorCode >= 0) RamFrameCount = value;
            return Log(nameof(GetRamFramesCount), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Считывает все видеоданные, которые имеются в ОЗУ,
        /// в <see cref="RawRamDataBuffer"/>, <see cref="NFrameDownloadedFromRam"/> и <see cref="RemainingBytesInRam"/>.
        /// В процессе передачи, новые данные в буфере не накапливаются.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetRawRamData()
        {
            ValidateObjectExist();

            sw.Restart();
            LastErrorCode = se.GetRawRamDataV2(SensorSettings.Handle, out var buffer, 0, out var nFrameDownloaded, out var remained);
            sw.Stop();

            if (LastErrorCode >= 0)
            {
                RawRamDataBuffer = buffer;
                NFrameDownloadedFromRam = nFrameDownloaded;
                RemainingBytesInRam = remained;
            }
            return Log(nameof(GetRawRamData), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Задает размер рабочего окна датчика в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetWindow()
        {
            ValidateObjectExist();

            return Call(() => se.SetWindow(SensorSettings.Handle, SensorSettings.LeftWindow, SensorSettings.TopWindow,
                                          SensorSettings.WidthWindow, SensorSettings.HeightWindow));
        }

        /// <summary>
        /// Включает или выключает срабатывание лазера в момент экспозиции кадра, в соответствие с настройками.
        /// Данная функция может быть аппаратно отключена при изготовлении профилометра по заказу,
        /// в этом случае работа лазера всегда будет разрешена.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetLaser()
        {
            ValidateObjectExist();

            return Call(() => se.Laser(SensorSettings.Handle, (byte)(SensorSettings.Laser ? 1 : 0)));
        }

        /// <summary>
        /// Включает или нет инверсию лазера, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetLaserInversion()
        {
            ValidateObjectExist();

            return Call(() => se.LaserInversion(SensorSettings.Handle, SensorSettings.LaserInversion));
        }

        /// <summary>
        /// Задает параметры обработки пятен на изображении датчиком, с целью формирования данных профиля, в соответствие с настройками.
        /// На режим передачи видео данный метод не оказывает влияния.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetParameters()
        {
            ValidateObjectExist();

            return Call(() => se.SetParameters(SensorSettings.Handle, SensorSettings.MaxSpotsCount, SensorSettings.Level,
                                              SensorSettings.MinWidth, SensorSettings.MaxWidth));
        }

        /// <summary>
        /// Включает или выключает режим сортировки координат по L в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetAscendingL()
        {
            ValidateObjectExist();

            return Call(() => se.AscendingL(SensorSettings.Handle, SensorSettings.AscendingL));
        }

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
        public int SetDebounce()
        {
            ValidateObjectExist();

            return Call(() => se.SetDebounce(SensorSettings.Handle, SensorSettings.DebounceTime));
        }

        /// <summary>
        /// Считывает значение цифрового усиления видеосигнала в настройки.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetDigitalGain()
        {
            ValidateObjectExist();

            sw.Restart();
            LastErrorCode = se.GetGain(SensorSettings.Handle, out var gain);
            sw.Stop();

            if (LastErrorCode >= 0) SensorSettings.Gain = gain;

            return Log(nameof(GetDigitalGain), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Задает значение цифрового усиления видеосигнала в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetDigitalGain()
        {
            ValidateObjectExist();

            return Call(() => se.SetGain(SensorSettings.Handle, SensorSettings.Gain));
        }

        /// <summary>
        /// Считывает значение аналогового усиления видеосигнала в настройки.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetAnalogGain()
        {
            ValidateObjectExist();

            sw.Restart();
            LastErrorCode = se.GetAnalogGain(SensorSettings.Handle, out byte gain);
            sw.Stop();

            if (LastErrorCode >= 0) SensorSettings.AnalogGain = (AnalogGain)gain;

            return Log(nameof(GetAnalogGain), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Задает значение аналогового усиления видеосигнала в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetAnalogGain()
        {
            ValidateObjectExist();

            return Call(() => se.SetAnalogGain(SensorSettings.Handle, (byte)SensorSettings.AnalogGain));
        }

        /// <summary>
        /// Считывает значение времени экспозиции в условных единицах в настройки.
        /// Одна условная единица приблизительно равна 4 мкс.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetExposition()
        {
            ValidateObjectExist();

            sw.Restart();
            LastErrorCode = se.GetExposition(SensorSettings.Handle, out var exposition);
            sw.Stop();

            if (LastErrorCode >= 0)
                SensorSettings.Exposition = exposition;

            return Log(nameof(GetExposition), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Задает значение времени экспозиции в условных единицах в соответствие с настройками.
        /// Одна условная единица приблизительно равна 4 мкс.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetExposition()
        {
            ValidateObjectExist();
            return Call(() => se.SetExposition(SensorSettings.Handle, SensorSettings.Exposition));
        }

        /// <summary>
        /// Задает количество кадров, получаемых на один строб.
        /// </summary>
        /// <returns>Код ошибки</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetNumFramesPerStrobe()
        {
            ValidateObjectExist();

            return Call(() => se.SetNumFrames(SensorSettings.Handle, SensorSettings.NumFramesPerStrobe));
        }

        /// <summary>
        /// Управляет включением конвейерного (pipeline) режима формирования кадров.
        /// </summary>
        /// <returns>Код ошибки</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetPipeline()
        {
            ValidateObjectExist();
            return Call(() => se.PipelineModeEn(SensorSettings.Handle, SensorSettings.Pipeline));
        }

        /// <summary>
        /// Включает или выключает коррекцию уровня черного в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int AllowBlackReference()
        {
            ValidateObjectExist();
            return Call(() => se.BlackReferenceEn(SensorSettings.Handle, SensorSettings.BlackReference));
        }

        /// <summary>
        /// Включает или выключает кусочную экспозицию в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int AllowPiecewiseLinear()
        {
            ValidateObjectExist();

            return Call(() => se.PiecewizeLinearModeEn(SensorSettings.Handle, SensorSettings.Piecewise));
        }

        /// <summary>
        /// Задает параметры кусочной экспозиции в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetPiecewiseLinearMode()
        {
            ValidateObjectExist();

            return Call(() => se.PiecewizeLinearMode(SensorSettings.Handle, SensorSettings.NSlopes, SensorSettings.XKp1,
                                                    SensorSettings.XKp2, SensorSettings.SlopeKp1, SensorSettings.SlopeKp2));
        }

        /// <summary>
        /// Задает коэффициент прореживания, который применяется к колонкам в процессе вычисления профиля, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetSubsamplingProfile()
        {
            ValidateObjectExist();
            return Call(() => se.SubsamplingProfile(SensorSettings.Handle, SensorSettings.SubsamplingProfile));
        }

        /// <summary>
        /// Задает коэффициент прореживания, который применяется к строкам матрицы, в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetSubsamplingMatrix()
        {
            ValidateObjectExist();

            return Call(() => se.SubsamplingMatrix(SensorSettings.Handle, SensorSettings.SubsamplingMatrix, SensorSettings.SubsamplingAdvMode));
        }

        /// <summary>
        /// Считывает значение температуры, измеренной датчиком, в <see cref="SensorSettings.Temperature"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetTemper()
        {
            ValidateObjectExist();

            sw.Restart();
            LastErrorCode = se.GetTemper(SensorSettings.Handle, out float value);
            sw.Stop();

            if (LastErrorCode >= 0) SensorSettings.Temperature = value;

            return Log(nameof(GetTemper), LastErrorCode, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Задает параметры коррекции профиля в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetCorrection()
        {
            ValidateObjectExist();

            return Call(() => se.Correction(SensorSettings.Handle, SensorSettings.AngleCorrection, SensorSettings.RatioLCorrection,
                                           SensorSettings.RatioDCorrection, SensorSettings.ShiftLCorrection,
                                           SensorSettings.ShiftDCorrection, SensorSettings.HFlipCorrection,
                                           SensorSettings.VFlipCorrection));
        }

        /// <summary>
        /// Включает или выключает коррекцию профиля в соответствие с настройками, не изменяя уже заданные значения.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int AllowCorrection()
        {
            ValidateObjectExist();

            return Call(() => se.CorrectionEn(SensorSettings.Handle, SensorSettings.Correction));
        }

        /// <summary>
        /// Задает границы рабочей зоны для профиля в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetBorders()
        {
            ValidateObjectExist();

            return Call(() => se.Borders(SensorSettings.Handle, SensorSettings.LeftBorder, SensorSettings.RightBorder,
                              SensorSettings.TopBorder, SensorSettings.BottomBorder));
        }

        /// <summary>
        /// Включает или выключает применение границ рабочей зоны для профиля в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int AllowBorders()
        {
            ValidateObjectExist();

            return Call(() => se.BordersEn(SensorSettings.Handle, SensorSettings.BordersL, SensorSettings.BordersD));
        }

        /// <summary>
        /// Задает параметры для режима слежения в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetTracking()
        {
            ValidateObjectExist();

            return Call(() => se.Tracking(SensorSettings.Handle, SensorSettings.MinPointsTracking, SensorSettings.TrackingMode));
        }

        /// <summary>
        /// Включает или выключает режим слежения в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int AllowTracking()
        {
            ValidateObjectExist();

            return Call(() => se.TrackingEn(SensorSettings.Handle, SensorSettings.Tracking));
        }

        /// <summary>
        /// Переводит датчик в режим измерения профиля, когда объектом обработки (колонкой) на изображении является строка.
        /// В процессе получения данных с профилометра не осуществляется контроль доставки пакетов (без подтверждения).
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int EnableProfileColumnsMode()
        {
            ValidateObjectExist();

            return Call(() => se.SpotsNoAckColumn(SensorSettings.Handle));
        }

        /// <summary>
        /// Переводит датчик в режиме измерения профиля, когда объектом обработки (колонкой) на изображении является столбец.
        /// В процессе получения данных с профилометра не осуществляется контроль доставки пакетов (без подтверждения).
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int EnableProfileRowsMode()
        {
            ValidateObjectExist();

            return Call(() => se.SpotsNoAckRow(SensorSettings.Handle));
        }

        /// <summary>
        /// Переключает профилометр в режим выдачи видеокадров и выделяет в памяти буфер,
        /// необходимый для временного хранения полученного кадра, в соответствии с размером матрицы + заголовок 64 байта.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int EnableVideoMode()
        {
            ValidateObjectExist();

            return Call(() => se.Video(SensorSettings.Handle));
        }

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
        public int EnableVideoToRamMode()
        {
            ValidateObjectExist();

            return Call(() => se.VideoToRam(SensorSettings.Handle));
        }

        /// <summary>
        ///     Переводит датчик в режим однократного срабатывания по команде. Одна команда - одно срабатывание.
        ///     Командой, запускающей срабатывание датчика, является функция SendSync.
        ///     Установка режима синхронизации по команде может использоваться также для останова работы датчика.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int EnableSyncCmdMode()
        {
            ValidateObjectExist();

            return Call(() => se.SyncCmd(SensorSettings.Handle));
        }

        /// <summary>
        /// Переводит датчик в режим срабатывания по внутреннему таймеру с частотой <see cref="SensorSettings.SyncNoneHz"/> Гц.
        /// При этом профилометр начинает выполнять измерения с указанной частотой, которая не является синхронной с другими профилометрами в системе.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int EnableSyncNoneMode()
        {
            ValidateObjectExist();

            return Call(() => se.SyncNone(SensorSettings.Handle, SensorSettings.SyncNoneHz));
        }

        /// <summary>
        /// Переводит датчик в особый режим синхронизации по внешнему импульсу.
        /// Данный тип синхронизации используется, если профилометр аппаратно настроен на передачу импульса запуска непосредственно на фотоматрицу,
        /// что обеспечивает максимально быструю реакцию.
        /// Это стандартный режим внешней синхронизации. Аппаратная настройка выполнятся в процессе изготовления профилометра.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int EnableSyncExtMode()
        {
            ValidateObjectExist();

            return Call(() => se.SyncExt(SensorSettings.Handle));
        }

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
        public int SyncFromStrobe()
        {
            ValidateObjectExist();

            return Call(() => se.SyncFromStrobe(SensorSettings.Handle, SyncFromStrobeMode));
        }

        /// <summary>
        /// Вызывает однократное срабатывание датчика, при условии, что тот находится в режиме «Синхронизация по команде».
        ///     Получив данные, библиотека вызывает соответствующую Callback-функцию.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SendSync()
        {
            ValidateObjectExist();

            return Call(() => se.SendSync(SensorSettings.Handle));
        }

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
        public int Stop()
        {
            ValidateObjectExist();

            SensorSettings.IsStopped = true;

            return Call(() => se.Stop(SensorSettings.Handle));
        }

        /// <summary>
        /// Считывает текущие параметры объекта датчика общего характера в <see cref="StatusGeneralParamsSet"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetStatusGeneralParamsSet()
        {
            ValidateObjectExist();

            sw.Restart();
            IntPtr ptr = se.StatusGen(SensorSettings.Handle);
            sw.Stop();

            StatusGeneralParamsSet = (GeneralParamsSet)Marshal.PtrToStructure(ptr, typeof(GeneralParamsSet));

            return Log(nameof(GetStatusGeneralParamsSet), 0, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Считывает текущие параметры коррекции профиля в <see cref="StatusCorrectionParamsSet"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetStatusCorrectionParamsSet()
        {
            ValidateObjectExist();

            sw.Restart();
            var ptr = se.StatusCorr(SensorSettings.Handle);
            sw.Stop();

            StatusCorrectionParamsSet = (CorrectionParamsSet)Marshal.PtrToStructure(ptr, typeof(CorrectionParamsSet));

            return Log(nameof(GetStatusCorrectionParamsSet), 0, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Считывает текущие границы рабочей области в <see cref="StatusBordersParamsSet"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetStatusBordersParamsSet()
        {
            ValidateObjectExist();

            sw.Restart();
            var ptr = se.StatusBord(SensorSettings.Handle);
            sw.Stop();

            StatusBordersParamsSet = (BordersParamsSet)Marshal.PtrToStructure(ptr, typeof(BordersParamsSet));

            return Log(nameof(GetStatusBordersParamsSet), 0, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Считывает текущие параметры видеоматрицы в <see cref="StatusMatrixParamsSet"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int GetStatusMatrixParamsSet()
        {
            ValidateObjectExist();

            sw.Restart();
            var ptr = se.StatusMatrix(SensorSettings.Handle);
            sw.Stop();

            StatusMatrixParamsSet = (MatrixParamsSet)Marshal.PtrToStructure(ptr, typeof(MatrixParamsSet));

            return Log(nameof(GetStatusMatrixParamsSet), 0, sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Включает или выключает удаление отдельных шумовых точек методом окна (аппаратно) в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int AllowNoiseFixedWindowRemover()
        {
            ValidateObjectExist();

            return Call(() => se.NoiseRemoverEn(SensorSettings.Handle, SensorSettings.NoiseRemover));
        }

        /// <summary>
        /// Включает или выключает удаление отдельных шумовых точек методом плавучего окна в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int AllowNoiseFloatWindowRemover()
        {
            ValidateObjectExist();

            return Call(() => se.NoiseFloatRemoverEn(SensorSettings.Handle, SensorSettings.NoiseFloatRemover));
        }

        /// <summary>
        /// Задает параметры удаления шумовых точек методом плавучего окна в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetNoiseFloatWindowRemoverMode()
        {
            ValidateObjectExist();

            return Call(() => se.NoiseFloatRemover(SensorSettings.Handle, SensorSettings.NoiseRemoverHeightWindow,
                                                  SensorSettings.NoiseRemoverWidthWindow,
                                                  SensorSettings.NoiseRemoverThreshold));
        }

        /// <summary>
        /// Задает параметры дискретизации в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetDiscretizeMode()
        {
            ValidateObjectExist();

            return Call(() => se.Discretize(SensorSettings.Handle, SensorSettings.DiscretizeStep, (byte)SensorSettings.DiscretizeMode,
                                            SensorSettings.DiscretizeNearValue));
        }

        /// <summary>
        /// Включает или выключает передискретизацию профиля в линейный массив в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int AllowDiscretize()
        {
            ValidateObjectExist();

            return Call(() => se.DiscretizeEn(SensorSettings.Handle, SensorSettings.Discretize));
        }

        /// <summary>
        /// Задает значение-заменитель для отсутствуюших значений в массиве дискретизированных данных в соответствие с настройками.
        /// Имеет смысл только в режиме включенной дискретизации.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetMissingDesignator()
        {
            ValidateObjectExist();

            return Call(() => se.SetMissingDesignator(SensorSettings.Handle, SensorSettings.MissingDesignatorValue));
        }

        /// <summary>
        /// Включает или выключает вычитание постоянной составляющей (измеренной медианой) в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ObjectDisposedException">Объект датчика не был создан.</exception>
        public int SetZeroDC()
        {
            ValidateObjectExist();

            return Call(() => se.ZeroDC(SensorSettings.Handle, SensorSettings.ZeroDC));
        }

        /// <summary>
        ///     Считывает данные, пришедшие от Callback из библиотеки SensorE.dll и пробрасывает эти данные слушателям.
        /// </summary>
        /// <param name="sender">Дескриптор объекта датчика.</param>
        /// <param name="data">Массив данных.</param>
        /// <param name="dataLength">Количество данных.</param>
        /// <param name="inVideo">Режим видео? (иначе профиль)</param>
        public void CallbackEvent(IntPtr sender, IntPtr data, int dataLength, bool inVideo)
        {
            if (SensorSettings.Handle != sender)
            {
                string msg = $"Пришли данные от неизвестного датчика: {SensorSettings.Handle} != {sender}." +
                    $"\nУказатель на данные = {data}; Длина данных = {dataLength}; Режим = {(inVideo ? "видео" : "профиль")}.";
                Logger?.Warn(msg, TypeObject);
                FooterRepository.Text = msg;
                return;
            }

            DataCounter++;
            SensorSettings.DataCounter++;

            var callback = new CallbackData(inVideo, data, dataLength);
            var handle = CallbackDataReceived;
            if (handle == null)
            {
                callback.Dispose();
            }
            else
            handle.Invoke(this, callback);
        }


        /// <summary>
        ///     Считывает данные по профилю, пришедшие от Callback из библиотеки SensorE.dll и пробрасывает эти данные слушателям.
        /// </summary>
        /// <param name="sender">Дескриптор объекта датчика.</param>
        /// <param name="mode">Режим работы датчика (внутреннее значение из SensorE).</param>
        /// <param name="data">Массив данных.</param>
        /// <param name="dataLength">Количество данных.</param>
        protected void CallbackProfile(IntPtr sender, int mode, IntPtr data, int dataLength) =>
            CallbackEvent(sender, data, dataLength, false);

        /// <summary>
        ///     Считывает данные по кадру, пришедшие от Callback из библиотеки SensorE.dll и пробрасывает эти данные слушателям.
        /// </summary>
        /// <param name="sender">Дескриптор объекта датчика.</param>
        /// <param name="mode">Режим работы датчика (внутреннее значение из SensorE).</param>
        /// <param name="data">Массив данных.</param>
        /// <param name="dataLength">Количество данных.</param>
        protected void CallbackVideo(IntPtr sender, int mode, IntPtr data, int dataLength) =>
            CallbackEvent(sender, data, dataLength, true);

        /// <summary>
        ///     Инициализирует уровень библиотеки SensorE.dll (версия, отладка, режим заголовка и диапазон портов при создании датчиков).
        /// [Безопасный вызов через <see cref="Call"/>]
        /// </summary>
        /// <returns>Успешное выполнение?</returns>
        public int InitDllStage()
        {
            int res = GetSensorEVersion();
            if (res < 0) return res;

            res = SetDebugMode();
            if (res < 0) return res;

            //сука, записывает в файл огромные массивы данных в режиме видео
            //если включен, то видеоколлбэк будет приходить раз в 1.5 секунды, блять!!!
            res = SetDebugModeListener();
            if (res < 0) return res;

            res = SetEnhanceHeader();
            if (res < 0) return res;

            res = SetPortRange();

            return res;
        }

        /// <summary>
        ///     Переводит датчик в какой-то из режимов измерения в соответствие с настройками.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Режим работы датчика в настройках имеет недопустимое значение.</exception>
        public int SetMode()
        {
            switch (SensorSettings.Mode)
            {
                case SensorMode.ProfileColumn:
                    LastErrorCode = EnableProfileColumnsMode();
                    break;
                case SensorMode.ProfileRow:
                    LastErrorCode = EnableProfileRowsMode();
                    break;
                case SensorMode.Video:
                    LastErrorCode = EnableVideoMode();
                    break;
                case SensorMode.VideoRam:
                    LastErrorCode = EnableVideoToRamMode();
                    break;
                default:
                    break;
            }
            return LastErrorCode;
        }

        /// <summary>
        ///     Переводит датчик в какой-то из режимов синхронизации в соответствие с <see cref="SyncMode"/>.
        /// </summary>
        /// <returns>Код ошибки.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Режим синхронизации датчика в поле данных имеет недопустимое значение.</exception>
        public int SetSyncMode()
        {
            switch (SystemSettings.SensorsSyncMode)
            {
                case SyncMode.SyncExt:
                    LastErrorCode = EnableSyncExtMode();
                    break;
                case SyncMode.SyncNone:
                    LastErrorCode = EnableSyncNoneMode();
                    break;
                case SyncMode.SyncCmd:
                    LastErrorCode = EnableSyncCmdMode();
                    break;
                case SyncMode.SyncFromStrobe:
                    LastErrorCode = SyncFromStrobe();
                    break;
            }
            return LastErrorCode;
        }

        /// <summary>
        ///     Инициализация датчика.
        /// Если идентификатор датчика существует, то будет произведена частичная инициализация, иначе полная.
        /// Частичная инициализация подразумевает под собой пропуск "тяжелых" функций, которые делаются один раз после включения питания.
        /// Таким образом, частичная инициализация - это в основном изменение настроек и режима работы.
        /// </summary>
        public async Task<int> Init()
        {
            SensorSettings.DataCounter = 0;
            var initSW = new Stopwatch();
            initSW.Start();
            int res = -1;

            if (!SensorSettings.IsExist && !SensorSettings.Initialized)
            {
                res = await Task.Run(() => CreateSensor());
                if (res < 0)
                {
                    return res;
                }

                //если не запустить прослушку, режимы синхры будут возвращать ошибку
                await Task.Run(() => StartListener());
                //if (res < 0) return res;
            }

            //останавливаем датчик (защита от случайного срабатывания по внешней синхре)
            res = await Task.Run(() => Stop());
            if (res < 0) return res;

            //переводим в режим по команде
            res = await Task.Run(() => EnableSyncCmdMode());
            if (res < 0) return res;

            //прописываем передачу неполных данных
            res = await Task.Run(() => SetUncompletedData());
            if (res < 0) return res;

            //задаем начальный режим по колонкам (чтобы в режиме видео был правильный пересчет в OnePointToMM())
            res = await Task.Run(() => EnableProfileColumnsMode());
            if (res < 0) return res;

            if (SensorSettings.ProfileMode)
            {
                if (SensorSettings.InMm && !SensorSettings.CalibrationIsLoaded)
                {
                    res = await Task.Run(() => LoadCalibration());
                    if (res < 0) return res;
                    SensorSettings.CalibrationIsLoaded = true;
                }

                res = await Task.Run(() => AllowCalibration());
                if (res < 0) return res;
            }

            //устанавливаем размеры окна
            res = await Task.Run(() => SetWindow());
            if (res < 0) return res;

            //разрешаем включение лазера
            res = await Task.Run(() => SetLaser());
            if (res < 0) return res;

            //устанавливаем инверсию лазера
            res = await Task.Run(() => SetLaserInversion());
            if (res < 0) return res;

            if (SensorSettings.ProfileMode)
            {
                //задаем порог по яркости, мин к-во пятен, макс к-во пятен, мин ширина пятна, макс ширина пятна
                res = await Task.Run(() => SetParameters());
                if (res < 0) return res;

                //включаем рижим сортировки точек по горизонтали
                res = await Task.Run(() => SetAscendingL());
                if (res < 0) return res;
            }

            //устанавливаем интервал антидребезга
            res = await Task.Run(() => SetDebounce());
            if (res < 0) return res;

            if (SensorSettings.ProfileMode)
            {
                //устанавливаем шаг для прореживания точек профиля
                res = await Task.Run(() => SetSubsamplingProfile());
                if (res < 0) return res;

                //устанавливем матрицу прореживания
                res = await Task.Run(() => SetSubsamplingMatrix());
                if (res < 0) return res;

                if (SensorSettings.Correction)
                {
                    //устанавливаем параметры коррекции
                    res = await Task.Run(() => SetCorrection());
                    if (res < 0) return res;
                }

                //включаем саму коррекцию
                res = await Task.Run(() => AllowCorrection());
                if (res < 0) return res;

                #region No need to set borders
                
                //    //устанавливаем параметры границ
                //    res = await Task.Run(() => SetBorders());
                //    if (res < 0) return res;

                ////включам границы
                //res = await Task.Run(() => AllowBorders());
                //if (res < 0) return res;
                #endregion

                //устанавливаем параметры следящего окна (окно подстроится под размеры найденного профиля)
                if (SensorSettings.Tracking)
                {
                    res = await Task.Run(() => SetTracking());
                    if (res < 0) return res;
                }

                //включаем режим следящего окна
                res = await Task.Run(() => AllowTracking());
                if (res < 0) return res;

                //success = success && Call(FrameStrobeCount);

                //success = success && Call(TrackingMasterSlave);

                //success = success && Call(NoiseRemover);

                //включаем режим шумоподавления фиксированным окном
                res = await Task.Run(() => AllowNoiseFixedWindowRemover());
                if (res < 0) return res;

                if (SensorSettings.NoiseFloatRemover)
                {
                    //задаем параметры плавающего окна для шумоподавления (NoiseRemoverHeightWindow, NoiseRemoverWidthWindow, NoiseRemoverThreshold)
                    res = await Task.Run(() => SetNoiseFloatWindowRemoverMode());
                    if (res < 0) return res;
                }

                //включаем режим шумоподавления плавающим окном
                res = await Task.Run(() => AllowNoiseFloatWindowRemover());
                if (res < 0) return res;

                #region No need to set Discretization
                //success = success && Call(ReferenceProfileEn);
                //if (SensorSettings.Discretize)
                //{
                //    //Устанавливаем параметры дискретизации (шаг, режим, околошовное значение)
                //    res = await Task.Run(() => SetDiscretizeMode());
                //    if (res < 0) return res;
                //}

                ////включаем режим дискретизации
                //res = await Task.Run(() => AllowDiscretize());
                //if (res < 0) return res;
                #endregion

                ////задаем значение для подстановки в пустое место
                //res = await Task.Run(() => Call(SetMissingDesignator, nameof(SetMissingDesignator)));
                //if (res < 0) return res;

                //разрешить вычитание постоянной составляющей (медианы) из профиля
                res = await Task.Run(() => SetZeroDC());
                if (res < 0) return res;

            }

            //переводим датчик в режим видео/профиль
            res = await Task.Run(() => SetMode());
            if (res < 0) return res;

            //включение конвейерного режима
            //Эта функция должна выполняться до установки экспозиции
            res = await Task.Run(() => SetPipeline());
            if (res < 0) return res;


            //задаем количество кадро на строб
            res = await Task.Run(() => SetNumFramesPerStrobe());
            if (res < 0) return res;

            //устанавливаем значение экспозиции
            res = await Task.Run(() => SetExposition());
            if (res < 0) return res;

            if (SensorSettings.Piecewise)
            {
                //задаем параметры кусочной экспозиции (NSlopes, XKp1, XKp2, SlopeKp1, SlopeKp2)
                res = await Task.Run(() => SetPiecewiseLinearMode());
                if (res < 0) return res;
            }

            //включить кусочную экспозицию
            res = await Task.Run(() => AllowPiecewiseLinear());
            if (res < 0) return res;

            //разрешить коррекцию уровня черного матрицы в соответствии с описанием производителя
            res = await Task.Run(() => AllowBlackReference());
            if (res < 0) return res;

            //success = success && Call(AutoExposition);

            //success = success && Call(AutoExpositionEn);

            //установить цифровое усиление
            res = await Task.Run(() => SetDigitalGain());
            if (res < 0) return res;

            //установить аналаговое усиление
            res = await Task.Run(() => SetAnalogGain());
            if (res < 0) return res;

            //замерить температуру
            res = await Task.Run(() => GetTemper());
            if (res < 0) return res;

            //установить режим синхронизации
            res = await Task.Run(() => SetSyncMode());
            if (res < 0) return res;


            initSW.Stop();

            SensorSettings.Initialized = res >= 0;
            SensorSettings.IsStopped = res > 0;

            return Log(nameof(Init), res, initSW.ElapsedMilliseconds);
        }

        /// <summary>
        /// Вызвать при удалении датчика из коллекции
        /// </summary>
        public override void Destroy()
        {
            Call(Stop);
            Call(DisposeSensor);
            base.Destroy();
        }

        private int Log(string name, int error, long ms)
        {
            string msg = $"Датчик (IP = {SensorSettings.Ip}): {name} = {error} ({GetLastErrorCode}). Время выполнения: {ms}";
            FooterRepository.Text = msg;

            if (LastErrorCode < 0)
            {
                Logger?.Error(msg, TypeObject);
            }

            SensorSettings.IsOk = error >= 0;
            return error;
        }

        /// <summary>
        /// Пишет в сообщение в лог интерфейса
        /// </summary>
        /// <param name="message">Сообщение</param>
        public void Log(string message)
        {
            FooterRepository.Text = message;
        }

        private void ValidateObjectExist()
        {
            if (!SensorSettings.IsExist)
            {
                string msg = $"Объект датчика не был создан в библиотеке \"" +
                   $"{FileName}\", необходимо вызвать \"{nameof(CreateSensor)}\".";
                Logger.Error(msg, TypeObject);
                //throw new ObjectDisposedException(nameof(SensorSettings.Handle), msg);
            }
        }
        #endregion
    }
}
