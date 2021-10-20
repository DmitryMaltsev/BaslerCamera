using Prism.Mvvm;

using System;
using System.Windows.Markup;
using System.Xml.Serialization;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class DefectProperties : BindableBase, IDisposable
    {
        private bool _disposed = false;

        [XmlIgnore]
        private DateTime _index;
        public DateTime Время
        {
            get { return _index; }
            set { SetProperty(ref _index, value); }
        }

        private string _type;
        public string Тип
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        private double _x;
        public double X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }
        private double _y;
        public double Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }
        private double _width;
        public double Ширина
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }
        private double _height;
        private bool _disposedValue;

        [XmlIgnore]
        public double Высота
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                _disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        // ~DefectProperties()
        // {
        //     // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
