using System;
using System.Collections.Generic;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class ReadPostprocessinger<T>
    {
        #region Private Fields

        /// <summary>
        ///     Сам буфер чтения/записи
        /// </summary>
        private readonly T[] _buffer;

        /// <summary>
        ///     Индекс чтения
        /// </summary>
        private int _readIndex;

        /// <summary>
        ///     Индекс записи
        /// </summary>
        private int _writeIndex;

        #endregion

        #region Properties

        /// <summary>
        ///     Емкость буфера
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        ///     Режим с переполнением?
        /// </summary>
        public bool Overflow { get; }

        /// <summary>
        ///     Количество пропущенных данных (из-за переполнения)
        /// </summary>
        public int SkippedData { get; private set; }

        /// <summary>
        ///     Количество элементов в буфере
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///     Количество элементов в буфере
        /// </summary>
        public int Length => Count;

        #endregion

        #region Constructors

        /// <summary>
        ///     Конструктор буфера
        /// </summary>
        /// <param name="capacity">Емкость буфера</param>
        /// <param name="overflow">С переполнением? (исключение или же пропуск данных при записи)</param>
        public ReadPostprocessinger(int capacity, bool overflow = false)
        {
            Capacity = capacity;
            Overflow = overflow;
            _buffer = new T[capacity];
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Читает данные
        /// </summary>
        /// <param name="lengthData">Количество элементов</param>
        /// <param name="holdData">Удерживать данные?</param>
        /// <returns>Массив элементов</returns>
        public T[] Read(int lengthData, bool holdData = false)
        {
            //Если длина считываемых данных больше количества элементов в буфере
            if (lengthData > Count)
                throw new Exception("Недостаточно данных в буфере");

            var result = new T[lengthData];

            //Если с учетом данных индекс чтения будет больше окончания буфера
            if (_readIndex + lengthData >= _buffer.Length)
            {
                //Длина считываемых данных этого цикла
                var length = _buffer.Length - _readIndex;

                //Длина считываемых данных нового цикла
                var nextLength = lengthData - length;

                //Копируем данные из остаточного места этого цикла
                Array.Copy(_buffer, _readIndex, result, 0, length);

                //Копируем данные на места в новом цикле
                Array.Copy(_buffer, 0, result, length, nextLength);

                //Если данные не удерживать
                if (!holdData)
                {
                    //Смещаем индекс чтения
                    _readIndex = nextLength;

                    //Уменьшаем счетчик непрочитанных данных
                    Count -= lengthData;
                }

                return result;
            }

            //Копируем данные из буфера
            Array.Copy(_buffer, _readIndex, result, 0, lengthData);

            //Если данные не удерживать
            if (!holdData)
            {
                //Смещаем индекс чтения
                _readIndex += lengthData;

                //Уменьшаем счетчик непрочитанных данных
                Count -= lengthData;
            }

            return result;
        }

        /// <summary>
        ///     Считывает один элемент данных
        /// </summary>
        /// <param name="holdData">Удерживать данные?</param>
        /// <returns></returns>
        public T Read(bool holdData = false) => Read(1, holdData)[0];

        /// <summary>
        ///     Записывает массив данных в буфер
        /// </summary>
        /// <param name="data">Записываемый массив данных</param>
        public void Write(T[] data)
        {
            var lengthData = data.Length;

            //Если буфер переполняет от записываемых данных, то исключение
            if (Count + lengthData > _buffer.Length)
            {
                if (!Overflow)
                {
                    SkippedData++;
                    return;
                }

                throw new Exception("Переполнение буфера");
            }

            //Если с учетом данных индекс записи будет больше окончания буфера
            if (_writeIndex + lengthData >= _buffer.Length)
            {
                //Длина записываемых данных этого цикла
                var length = _buffer.Length - _writeIndex;

                //Длина записываемых данных нового цикла
                var nextLength = lengthData - length;

                //Копируем данные на остаточное место этого цикла
                Array.Copy(data, 0, _buffer, _writeIndex, length);

                //Копируем данные на место в новом цикле
                Array.Copy(data, length, _buffer, 0, nextLength);

                //Смещаем индекс записи
                _writeIndex = nextLength;

                //Увеличиваем счетчик непрочитанных данных
                Count += lengthData;

                return;
            }

            //Копируем данные в буфер
            Array.Copy(data, 0, _buffer, _writeIndex, lengthData);

            //Смещаем индекс записи
            _writeIndex += lengthData;

            //Увеличиваем счетчик непрочитанных данных
            Count += lengthData;
        }

        /// <summary>
        ///     Записывает данные в буфер
        /// </summary>
        /// <param name="data">Записываемый элемент данных</param>
        public void Write(T data) => Write(new[] { data });

        /// <summary>
        ///     Возвращает элемент в буфере
        /// </summary>
        /// <param name="index">Индекс элемента</param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                //Если индекс больше количества элементов в буфере, то исключение
                if (index >= Count)
                    throw new ArgumentOutOfRangeException();

                return _buffer[(_readIndex + index) % _buffer.Length];
            }
        }

        /// <summary>
        ///     Считывает все элементы буфера по одному (для foreach)
        /// </summary>
        public IEnumerable<T> GetData()
        {
            for (var i = 0; i < Count; i++)
                yield return _buffer[(_readIndex + i) % _buffer.Length];
        }

        /// <summary>
        ///     Считывает заданное число элементов буфера по одному (для foreach)
        /// </summary>
        /// <param name="count">Количество считываемых элементов</param>
        public IEnumerable<T> GetData(int count)
        {
            //Если количество считываемых элементов больше количества элементов в буфере
            if (count > Count)
                throw new Exception("Недостаточно данных в буфере");

            for (var i = 0; i < count; i++)
                yield return _buffer[(_readIndex + i) % _buffer.Length];
        }

        /// <summary>
        /// Производит очистку буфера
        /// </summary>
        public void Clear()
        {
            Count = 0;
            _readIndex = 0;
            _writeIndex = 0;
        }

        #endregion
    }
}
