using Kogerent.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class CallbackData:IDisposable
    {
        #region Fields

        private SubWindow[] _subWindows;
        private bool disposedValue;
        #endregion

        #region Constructors

        /// <summary>
        ///     Создает данные, приходящие от Callback из библиотеки SensorE.dll.
        /// </summary>
        /// <param name="fromVideoCallback">Данные были получены из библиотеки SensorE.dll через Callback видео (иначе профиля)?</param>
        public CallbackData(bool fromVideoCallback)
        {
            SubWindows = null;

            FromVideoCallback = fromVideoCallback;
        }

        /// <summary>
        ///     Считывает данные, пришедшие от Callback из библиотеки SensorE.dll, в новый объект.
        /// </summary>
        /// <param name="fromVideoCallback">Данные были получены из библиотеки SensorE.dll через Callback видео (иначе профиля)?</param>
        /// <param name="data">Указатель на массив данных.</param>
        /// <param name="dataLength">Длина данных.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public CallbackData(bool fromVideoCallback, IntPtr data, int dataLength) :
            this(fromVideoCallback) => ReadData(data, dataLength);

        #endregion

        #region Properties

        /// <summary>
        /// Данные были получены из библиотеки SensorE.dll через Callback видео (иначе профиля)?
        /// </summary>
        public bool FromVideoCallback { get; set; }

        /// <summary>
        ///     Общее количество удаленных из профиля точек различными фильтрами. Используется в режиме профиля.
        /// </summary>
        public uint NRemovedPoints { get; protected set; }

        /// <summary>
        ///     Средняя ширина пятна. Используется в режиме профиля.
        /// </summary>
        public float AverageWidth { get; protected set; }

        /// <summary>
        ///     Номер кадра, полученный от ЦПС. Используется в режимах видео и профиля.
        /// </summary>
        /// <returns></returns>
        public int NData { get; protected set; }

        /// <summary>
        ///     Состояние дискретных входов (от 0 до 0x0F). Используется в режиме профиля.
        /// </summary>
        /// <returns></returns>
        public int IO { get; protected set; }

        /// <summary>
        ///     Количество точек, полученных профилометром в результате обнаружения фотоматрицы (может отличаться от объема данных,
        ///     получаемых callback'ом, которые могут подвергаться постобработке). Используется в режиме профиля.
        /// </summary>
        /// <returns></returns>
        public int NPoints { get; protected set; }

        /// <summary>
        ///     Номер строба. Используется в режиме профиля.
        /// </summary>
        /// <returns></returns>
        public uint NStrobe { get; protected set; }

        /// <summary>
        ///     Текущая экспозиция. Используется в режиме профиля.
        /// </summary>
        /// <returns></returns>
        public uint CurrentExposition { get; protected set; }

        /// <summary>
        ///     Ошибка переполнения входного буфера? Используется в режимах видео и профиля.
        /// </summary>
        public bool OverflowsBuffer { get; protected set; }

        ///// <summary>
        /////     Таймаут получения данных?
        ///// </summary>
        //public bool Timeout { get; protected set; }

        /// <summary>
        ///     Ошибка получения полных данных? Используется в режимах видео и профиля.
        /// </summary>
        public bool UncompletedData { get; protected set; }

        /// <summary>
        ///     Обнаружен пропуск строба? Используется в режиме профиля.
        /// </summary>
        public bool SkipStrobe { get; protected set; }

        ///// <summary>
        /////     Достигнута предельная экспозиция?
        ///// </summary>
        //public bool LimitExposition { get; protected set; }

        /// <summary>
        ///     Режим работы видео (иначе профиль)?
        /// </summary>
        public bool InVideo { get; protected set; }

        /// <summary>
        ///     Переход в состояние останова (иначе работа)? Используется в режимах видео и профиля.
        /// </summary>
        public bool Stopped { get; protected set; }

        /// <summary>
        ///     Обработка матрицы по строкам (иначе по столбцам)? Используется в режимах видео и профиля.
        /// </summary>
        public bool RowProcessingDirection { get; protected set; }

        /// <summary>
        ///     Передача данных с подтверждением (иначе без подтверждения)? Используется в режимах видео и профиля.
        /// </summary>
        public bool ConfirmDataTransmission { get; protected set; }

        /// <summary>
        ///     Запись данных в ОЗУ включена (иначе отключена)? Используется в режимах видео и профиля.
        /// </summary>
        public bool WritingInRAM { get; protected set; }

        /// <summary>
        ///     Данные в миллиметрах (иначе в пикселях)? Используется в режимах видео и профиля.
        /// </summary>
        public bool InMm { get; protected set; }

        /// <summary>
        ///     Коррекция включена (иначе отключена)? Используется в режимах видео и профиля.
        /// </summary>
        public bool Correction { get; protected set; }

        /// <summary>
        ///     Границы по вертикали или горизонтали включены (иначе отключены)? Используется в режимах видео и профиля.
        /// </summary>
        public bool Borders { get; protected set; }

        /// <summary>
        ///     Режим слежения включен (иначе отключен)? Используется в режимах видео и профиля.
        /// </summary>
        public bool Tracking { get; protected set; }

        /// <summary>
        ///     Источник синхронизации. Используется в режимах видео и профиля.
        /// </summary>
        public SyncSource SyncSource { get; protected set; }

        /// <summary>
        ///     Прореживание по строкам включено (иначе отключено)? Используется в режимах видео и профиля.
        /// </summary>
        public bool SubsamplingMatrix { get; protected set; }

        /// <summary>
        ///     Прореживание по столбцам включено (иначе отключено)? Используется в режимах видео и профиля.
        /// </summary>
        public bool SubsamplingProfile { get; protected set; }

        /// <summary>
        ///     Размер заголовка. Стандартный заголовок имеет фиксированный размер, равный 64 байтам.
        /// </summary>
        public int HeaderLength { get; protected set; }

        /// <summary>
        ///     Вложенные окна. Используется в режиме профиля.
        /// </summary>
        public SubWindow[] SubWindows
        {
            get => _subWindows;
            protected set => _subWindows = value ?? new SubWindow[0];
        }

        /// <summary>
        ///     Данные в передискретизированном формате. Иначе в формате xyzw. Используется в режиме профиля.
        /// </summary>
        public bool Discretize { get; protected set; }

        ///// <summary>
        ///// Указатель на данные из библиотеки SensorE.dll.
        ///// </summary>
        //public IntPtr DataPtr { get; protected set; }

        /// <summary>
        ///     Данные кадра. Если <see cref="InVideo" /> = false, то данное поле будет равно null.
        /// </summary>
        public byte[] FrameData { get; protected set; }

        /// <summary>
        ///     Данные профиля. Если <see cref="InVideo" /> = true, то данное поле будет равно null.
        /// </summary>
        public float[] ProfileData { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Считывает данные, пришедшие от Callback из библиотеки SensorE.dll.
        /// </summary>
        /// <param name="data">Указатель на массив данных.</param>
        /// <param name="dataLength">Количество данных.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ReadData(IntPtr data, int dataLength)
        {
            //Анализируем длину данных в зависимости от того, по какому каналу пришли данные
            var fromVideoCallback = FromVideoCallback;

            //Стандартный заголовок равен 64 байтам
            const int baseHeaderLengthInBytes = 64;

            //Длина заголовка в количестве данных
            var baseHeaderLength = fromVideoCallback ? baseHeaderLengthInBytes : baseHeaderLengthInBytes / 4;

            //Если количество данных меньше чем стандартный заголовок, то это ошибка
            if (dataLength < baseHeaderLength)
            {
                throw new ArgumentOutOfRangeException(nameof(dataLength), dataLength,
                    $@"Длина считанных данных меньше длины стандартного заголовка данных ({baseHeaderLength}).");
            }

            //Флаги, режим, общая длина заголовка
            var ptr = IntPtr.Add(data, 16);
            var bHeader = new byte[12];
            Marshal.Copy(ptr, bHeader, 0, 12);

            var flags = bHeader[0] | bHeader[1] << 8 | bHeader[2] << 16 | bHeader[3] << 24;
            OverflowsBuffer = KUtils.IsBitSet(flags, 0);
            //Timeout = KUtils.IsBitSet(flags, 1);
            UncompletedData = KUtils.IsBitSet(flags, 2);
            SkipStrobe = KUtils.IsBitSet(flags, 3);
            //LimitExposition = KUtils.IsBitSet(flags, 4);

            var mode = bHeader[4] | bHeader[5] << 8 | bHeader[6] << 16 | bHeader[7] << 24;

            var inVideo = KUtils.IsBitSet(mode, 0);
            InVideo = inVideo;

            Stopped = KUtils.IsBitSet(mode, 1);
            RowProcessingDirection = KUtils.IsBitSet(mode, 2);
            ConfirmDataTransmission = KUtils.IsBitSet(mode, 3);
            WritingInRAM = KUtils.IsBitSet(mode, 4);
            InMm = KUtils.IsBitSet(mode, 5);
            Correction = KUtils.IsBitSet(mode, 6);
            Borders = KUtils.IsBitSet(mode, 7);
            Tracking = KUtils.IsBitSet(mode, 8);

            var k9 = KUtils.IsBitSet(mode, 9);
            var k10 = KUtils.IsBitSet(mode, 10);
            var tSource = (k9 ? 1 << 0 : 0) + (k10 ? 1 << 1 : 0);
            SyncSource = (SyncSource)tSource;

            SubsamplingMatrix = KUtils.IsBitSet(mode, 11);
            SubsamplingProfile = KUtils.IsBitSet(mode, 12);
            Discretize = KUtils.IsBitSet(mode, 13);



            //Стандартный участок заголовка
            if (inVideo)
            {
                var header = new int[4];
                Marshal.Copy(data, header, 0, 4);

                NData = header[0];
                IO = header[1];
                NPoints = header[2];
                //NStrobe = 0; // header[3];
            }
            else
            {
                var headerFloat = new float[9];
                Marshal.Copy(data, headerFloat, 0, 9);

                NData = (int)headerFloat[0];
                IO = (int)headerFloat[1];
                NPoints = (int)headerFloat[2];
                AverageWidth = headerFloat[8];

                //Смещение задается в байтах
                ptr = IntPtr.Add(data, 12);
                var header = new int[7];
                Marshal.Copy(ptr, header, 0, 7);

                NStrobe = (uint)header[0];

                CurrentExposition = (uint)header[4];

                NRemovedPoints = (uint)header[6];
            }

            //Длина расширенного заголовка полностью
            var headerLength = bHeader[8] | bHeader[9] << 8 | bHeader[10] << 16 | bHeader[11] << 24;
            HeaderLength = headerLength;

            //Смещаемся на после базового заголовка
            var offsetData = baseHeaderLengthInBytes;

            //Если используется режим расширенного заголовка
            if (headerLength > offsetData)
            {
                ptr = IntPtr.Add(data, offsetData);

                //Количество байт подзаголовка
                const int subHeaderLength = 4;
                bHeader = new byte[subHeaderLength];
                Marshal.Copy(ptr, bHeader, 0, subHeaderLength);

                var fieldId = (ushort)(bHeader[1] << 8 | bHeader[0]);

                //Количество байт в поле с учетом подзаголовка
                var fieldLength = (ushort)(bHeader[3] << 8 | bHeader[2]);

                //Без учета заголовка
                var fieldDataLength = fieldLength - subHeaderLength;

                offsetData = headerLength + fieldLength;

                //Идентификатор поля рамок в режиме слежения
                if (fieldId == 0x0001)
                {
                    bHeader = new byte[fieldDataLength];
                    ptr = IntPtr.Add(data, offsetData + subHeaderLength);
                    Marshal.Copy(ptr, bHeader, 0, fieldDataLength);

                    var nSubWindows = fieldDataLength / 8;
                    var subWindows = new SubWindow[nSubWindows];

                    for (var i = 0; i < nSubWindows; i++)
                    {
                        var elementIndex = i * 8;

                        subWindows[i].X1 = bHeader[elementIndex + 1] << 8 | bHeader[elementIndex];
                        subWindows[i].Y1 = bHeader[elementIndex + 3] << 8 | bHeader[elementIndex + 2];
                        subWindows[i].X2 = bHeader[elementIndex + 5] << 8 | bHeader[elementIndex + 4];
                        subWindows[i].Y2 = bHeader[elementIndex + 7] << 8 | bHeader[elementIndex + 6];
                    }

                    SubWindows = subWindows;
                }
            }

            //Если в режиме видео, то вычитаем из длины байты, иначе вычитаем float'ы
            dataLength -= inVideo ? offsetData : offsetData / 4;

            //Производим смещение указателя непосредственно к массивам данных
            ptr = IntPtr.Add(data, offsetData);
            //data += offsetData;

            //Если данные существуют
            if (dataLength > 0)
            {
                if (inVideo)
                {
                    //Создаем массив кадра и заполняем его имеющимися данными
                    Span<byte> frame;

                    unsafe { frame = new Span<byte>((byte*)ptr, dataLength); }

                    //Фиксируем массив кадра
                    FrameData = frame.ToArray();
                }
                else
                {
                    //Создаем массив профиля и заполняем его имеющимися данными
                    Span<float> profile;

                    unsafe { profile = new Span<float>((float*)ptr, dataLength); }
                    //var profile = new float[dataLength];

                    //Фиксируем массив профиля
                    ProfileData = profile.ToArray();
                }
            }
            ////Считываем сами данные
            //ReadData(data + offsetData, dataLength);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer

                // TODO: set large fields to null
                FrameData = null;
                ProfileData = null;
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CallbackData()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
