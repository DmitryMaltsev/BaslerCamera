using Dapper;
using Kogerent.Core;
using Kogerent.Core.Models;
using Kogerent.Logger;
using Kogerent.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;

namespace Kogerent.Services.Implementation
{
    /// <summary>
    /// Служба по по работе с базой данных
    /// </summary>
    public class SqlDataService : IDataService
    {
        public ILogger Logger { get; }

        public SqlDataService(ILogger logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Получает строку для связи с базой по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Строка для связи</returns>
        public string GetConnectionString(string key)
        {
            return ConfigurationManager.ConnectionStrings[key].ConnectionString;
        }

        /// <summary>
        /// Вставляет ряд точек в соответствующую таблицу
        /// </summary>
        /// <param name="points">Точки</param>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="name">Ключ</param>
        public void InsertData(List<IntXFloatYPoint> points, string tableName, string name = "PirPoints")
        {
            string connectionString = GetConnectionString(name);
            StringBuilder colsBuilder = new();
            StringBuilder valsBuilder = new();

            for (int i = 0; i < points.Count; i++)
            {
                colsBuilder = colsBuilder.Append($"[{points[i].X}]");
                valsBuilder = valsBuilder.Append(points[i].Y.ToString("n4", CultureInfo.InvariantCulture));
                if (i < points.Count - 1)
                {
                    colsBuilder = colsBuilder.Append(", ");
                    valsBuilder = valsBuilder.Append(", ");
                }
            }

            string sql = $"INSERT INTO {tableName} ( [Date], {colsBuilder} ) VALUES ( '{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}', {valsBuilder} )";
            using (IDbConnection cnn = new SqlConnection(connectionString))
            {

                cnn.Execute(sql);
            }
        }

        /// <summary>
        /// Загружает данные из базы в диапазоне дат по длине
        /// </summary>
        /// <param name="firstDate">Начало диапазона</param>
        /// <param name="lastDate">Конец диапазона</param>
        /// <param name="tableName">Название таблицы</param>
        /// <param name="columnName">Координата икса</param>
        /// <param name="name">Ключ для подключения</param>
        /// <returns>Список точек дата-толщина</returns>
        public List<DateTimePoint> LoadData(DateTime firstDate, DateTime lastDate, string tableName, int columnName, string name = "PirPoints")
        {
            try
            {
                string connectionString = GetConnectionString(name);

                string sql = $"SELECT [Date] as X, [{columnName}] as Y FROM {tableName} WHERE [Date] >= '{firstDate:yyyy-MM-dd HH:mm:ss.fff}' AND [Date] <= '{lastDate:yyyy-MM-dd HH:mm:ss.fff}'";

                using (IDbConnection cnn = new SqlConnection(connectionString))
                {
                    var result = cnn.Query<DateTimePoint>(sql).ToList();
                    result.RemoveAll(p => p.Y <= 0);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex.Message);
                MessageBox.Show("Проверьте корректность введенных данных");
                return null;
            }
        }

        /// <summary>
        /// Загружает данные из базы в диапазоне дат по длине
        /// </summary>
        /// <param name="date">Начало диапазона</param>
        /// <param name="name">Ключ для подключения</param>
        /// <returns>Список точек дата-толщина</returns>
        public (DateTime, List<IntXFloatYPoint>) LoadWidthData(DateTime date, string name = "PirPoints")
        {
            try
            {
                string connectionString = GetConnectionString(name);
                List<IntXFloatYPoint> finalList = new();
                DateTime rowTime = new DateTime();
                using (IDbConnection cnn = new SqlConnection(connectionString))
                {
                    for (int index = 0; index < 4; index++)
                    {
                        string sql = $"SELECT TOP(1) * FROM PointsCollection{index} WHERE [Date] >= '{date:yyyy-MM-dd HH:mm:ss.fff}'";

                        IDictionary<string, object> result = (IDictionary<string, object>)cnn.QuerySingleOrDefault(sql);

                        if (result == null) continue;

                        object[] values = result.Values.ToArray();
                        string[] keys = result.Keys.ToArray();

                        if ((DateTime)values[1] >= rowTime)
                        {
                            rowTime = (DateTime)values[1];
                        }

                        for (int i = 2; i < values.Length; i++)
                        {
                            if (values[i] != null && keys[i] != null)
                            {
                                if (int.TryParse(keys[i], out int x) && float.TryParse(values[i].ToString(), out float y))
                                {
                                    finalList.Add(new IntXFloatYPoint(x, y));
                                }
                            }

                        }
                    }
                }

                return (rowTime, finalList);
            }

            catch (Exception ex)
            {
                Logger?.Error(ex.Message);
                MessageBox.Show("Проверьте корректность введенных данных");
                return (DateTime.Now, null);
            }
        }

        /// <summary>
        /// Загружает данные из базы в диапазоне дат по длине
        /// </summary>
        /// <param name="date">Начало диапазона</param>
        /// <param name="next"></param>
        /// <param name="name">Ключ для подключения</param>
        /// <returns>Список точек дата-толщина</returns>
        public (DateTime, List<IntXFloatYPoint>) LoadNextWidthData(DateTime date, bool next, string name = "PirPoints")
        {
            try
            {
                string connectionString = GetConnectionString(name);
                List<IntXFloatYPoint> finalList = new();
                DateTime rowTime = new DateTime();
                string sql = "";
                using (IDbConnection cnn = new SqlConnection(connectionString))
                {
                    for (int index = 0; index < 4; index++)
                    {
                        sql = next
                            ? @$"select top (1) * from dbo.PointsCollection{index}
                                 where [Date] = ( select Min([Date]) from dbo.PointsCollection{index} 
                                                  where [Date] > '{date:yyyy-MM-dd HH:mm:ss.fff}' )"
                            : @$"select top (1) * from dbo.PointsCollection{index}
                                 where [Date] = ( select Max([Date]) from dbo.PointsCollection{index} 
                                                  where [Date] < '{date:yyyy-MM-dd HH:mm:ss.fff}' )";

                        IDictionary<string, object> result = (IDictionary<string, object>)cnn.QuerySingleOrDefault(sql);

                        if (result == null) continue;

                        object[] values = result.Values.ToArray();
                        string[] keys = result.Keys.ToArray();

                        if ((DateTime)values[1] >= rowTime)
                        {
                            rowTime = (DateTime)values[1];
                        }

                        for (int i = 2; i < values.Length; i++)
                        {
                            if (values[i] != null && keys[i] != null)
                            {
                                if (int.TryParse(keys[i], out int x) && float.TryParse(values[i].ToString(), out float y))
                                {
                                    finalList.Add(new IntXFloatYPoint(x, y));
                                }
                            }

                        }
                    }
                }

                return (rowTime, finalList);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex.Message);
                MessageBox.Show("Проверьте корректность введенных данных");
                return (DateTime.Now, null);
            }
        }

        /// <summary>
        /// Сжимает таблицу
        /// </summary>
        /// <param name="tableNames">Имя таблицы</param>
        /// <param name="name">Ключ для строки поключения</param>
        public void Compress(string[] tableNames, string name = "PirPoints")
        {
            string connectionString = GetConnectionString(name);

            string sql = $"ALTER TABLE {tableNames[0]} REBUILD PARTITION = ALL WITH(DATA_COMPRESSION = page); ";
            string sql2 = $"ALTER TABLE {tableNames[1]} REBUILD PARTITION = ALL WITH(DATA_COMPRESSION = page); ";
            string sql3 = $"ALTER TABLE {tableNames[2]} REBUILD PARTITION = ALL WITH(DATA_COMPRESSION = page); ";
            string sql4 = $"ALTER TABLE {tableNames[3]} REBUILD PARTITION = ALL WITH(DATA_COMPRESSION = page); ";

            using (IDbConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Execute(sql);
                cnn.Execute(sql2);
                cnn.Execute(sql3);
                cnn.Execute(sql4);
            }
        }
    }
}
