using Kogerent.Core;
using Kogerent.Core.Models;
using System;
using System.Collections.Generic;

namespace Kogerent.Services.Interfaces
{
    /// <summary>
    /// Описание службы по работе с базой данных
    /// </summary>
    public interface IDataService
    {
        void Compress(string[] tableNames, string name = "PirPoints");

        /// <summary>
        /// Получает строку для связи с базой по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Строка для связи</returns>
        string GetConnectionString(string key);

        /// <summary>
        /// Вставляет ряд точек в соответствующую таблицу
        /// </summary>
        /// <param name="points">Точки</param>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="name">Ключ</param>
        void InsertData(List<IntXFloatYPoint> points, string tableName, string name = "PirPoints");

        
        List<DateTimePoint> LoadData(DateTime firstDate, DateTime lastDate, string tableName, int columnName, string name = "PirPoints");

        (DateTime, List<IntXFloatYPoint>) LoadNextWidthData(DateTime date, bool next, string name = "PirPoints");

        (DateTime, List<IntXFloatYPoint>) LoadWidthData(DateTime date,string name = "PirPoints");
    }
}
