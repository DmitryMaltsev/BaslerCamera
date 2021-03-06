using System.IO;
using System.Xml.Serialization;
using Kogerent.Services.Interfaces;
using Microsoft.Win32;

using Newtonsoft.Json;

namespace Kogerent.Services.Implementation
{
    /// <summary>
    /// Сервис сериализации и десериализации
    /// </summary>
    public class XmlService : IXmlService
    {
        /// <summary>
        /// Десериализует xml-файл в объект и сохраняет путь
        /// </summary>
        /// <typeparam name="T">Тип объекта для десериализации</typeparam>
        /// <param name="filePath">Строка, куда сохранится путь к файлу</param>
        /// <param name="settingsModel">Объект для десериализации</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Десериализованный объект</returns>
        public T ReadAs<T>(ref string filePath, T settingsModel, string filter)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Открыть файл настроек",
                Filter = filter,
                RestoreDirectory = true
            };
            if (dlg.ShowDialog() != true) return settingsModel;
            filePath = dlg.FileName;
            if (string.IsNullOrEmpty(filePath)) return settingsModel;
            XmlSerializer formatter = new(typeof(T));
            using FileStream fs = new(filePath, FileMode.OpenOrCreate);
            settingsModel = (T)formatter.Deserialize(fs);
            return settingsModel;
        }

        /// <summary>
        /// Десериализует xml-файл в объект
        /// </summary>
        /// <typeparam name="T">Тип объекта для десериализации</typeparam>
        /// <param name="filePath">Путь к файлу</param>
        /// <param name="settingsModel">Объект для десериализации</param>
        /// <returns>Десериализованный объект</returns>
        public T Read<T>(string filePath, T settingsModel)
        {
            if (string.IsNullOrEmpty(filePath)) return settingsModel;
            XmlSerializer formatter = new(typeof(T));
            using FileStream fs = new(filePath, FileMode.OpenOrCreate);
            settingsModel = (T)formatter.Deserialize(fs);
            return settingsModel;
        }

        /// <summary>
        /// Сериализует объект в xml-файл
        /// </summary>
        /// <typeparam name="T">Тип объекта для сериализации</typeparam>
        /// <param name="filePath">Куда сохранить</param>
        /// <param name="settingsModel">Объект для сериализации</param>
        public void Write<T>(string filePath, T settingsModel)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            XmlSerializer formatter = new(typeof(T));
            if (File.Exists(filePath)) File.Delete(filePath);
            using FileStream fs = new(filePath, FileMode.CreateNew);
            formatter.Serialize(fs, settingsModel);
        }

        /// <summary>
        /// Сериализует объект в json-файл
        /// </summary>
        /// <typeparam name="T">Тип объекта для сериализации</typeparam>
        /// <param name="filePath">Куда сохранить</param>
        /// <param name="settingsModel">Объект для сериализации</param>
        public void WriteJS<T>(string filePath, T settingsModel)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            if (File.Exists(filePath)) File.Delete(filePath);
            string js = JsonConvert.SerializeObject(settingsModel, Formatting.Indented);
            using StreamWriter stream = new(filePath);
            stream.Write(js);
        }

        /// <summary>
        /// Сериализует объект в xml-файл
        /// </summary>
        /// <typeparam name="T">Тип объекта для сериализации</typeparam>
        /// <param name="settingsModel">Объект для сериализации</param>
        /// <param name="filter">Фильтр</param>
        public void WriteAs<T>(T settingsModel, string filter)
        {
            var dlg = new SaveFileDialog
            {
                Title = "Открыть файл настроек",
                Filter = filter,
                RestoreDirectory = true
            };
            if (dlg.ShowDialog() != true) return;
            string filePath = dlg.FileName;
            XmlSerializer formatter = new(typeof(T));
            if (File.Exists(filePath)) File.Delete(filePath);
            using FileStream fs = new(filePath, FileMode.CreateNew);
            formatter.Serialize(fs, settingsModel);
        }
    }
}
