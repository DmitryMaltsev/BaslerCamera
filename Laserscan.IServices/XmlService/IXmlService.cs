using System.Collections.Generic;

namespace Kogerent.Services.Interfaces
{
    /// <summary>
    /// Контракт на создание сервиса сериализации и десериализации
    /// </summary>
    public interface IXmlService
    {
        /// <summary>
        /// Десериализует xml-файл в объект и сохраняет путь
        /// </summary>
        /// <typeparam name="T">Тип объекта для десериализации</typeparam>
        /// <param name="filePath">Строка, куда сохранится путь к файлу</param>
        /// <param name="settingsModel">Объект для десериализации</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Десериализованный объект</returns>
        T ReadAs<T>(ref string filePath, T settingsModel, string filter);

        /// <summary>
        /// Десериализует xml-файл в объект
        /// </summary>
        /// <typeparam name="T">Тип объекта для десериализации</typeparam>
        /// <param name="filePath">Путь к файлу</param>
        /// <param name="settingsModel">Объект для десериализации</param>
        /// <returns>Десериализованный объект</returns>
        T Read<T>(string filePath, T settingsModel);

        /// <summary>
        /// Сериализует объект в xml-файл
        /// </summary>
        /// <typeparam name="T">Тип объекта для сериализации</typeparam>
        /// <param name="filePath">Куда сохранить</param>
        /// <param name="settingsModel">Объект для сериализации</param>
        void Write<T>(string filePath, T settingsModel);

        /// <summary>
        /// Сериализует объект в xml-файл
        /// </summary>
        /// <typeparam name="T">Тип объекта для сериализации</typeparam>
        /// <param name="settingsModel">Объект для сериализации</param>
        /// <param name="filter">Фильтр</param>
        void WriteAs<T>(T settingsModel, string filter);

        /// <summary>
        /// Сериализует объект в json-файл
        /// </summary>
        /// <typeparam name="T">Тип объекта для сериализации</typeparam>
        /// <param name="filePath">Куда сохранить</param>
        /// <param name="settingsModel">Объект для сериализации</param>
        void WriteJS<T>(string filePath, T settingsModel);
        void WriteText(byte[,] data, string path);
    }
}
