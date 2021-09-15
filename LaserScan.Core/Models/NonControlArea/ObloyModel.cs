using Prism.Mvvm;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{
    public class ObloyModel : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private float _minimumX = 0;
        public float MinimumX
        {
            get { return _minimumX; }
            set { SetProperty(ref _minimumX, value); }
        }

        private float _maximumX = 0;
        public float MaximumX
        {
            get { return _maximumX; }
            set { SetProperty(ref _maximumX, value); }
        }

        private float _minimumY = 0;
        public float MinimumY
        {
            get { return _minimumY; }
            set { SetProperty(ref _minimumY, value); }
        }

        private float _MaximumY = 0;
        public float MaximumY
        {
            get { return _MaximumY; }
            set { SetProperty(ref _MaximumY, value); }
        }
    }
}
