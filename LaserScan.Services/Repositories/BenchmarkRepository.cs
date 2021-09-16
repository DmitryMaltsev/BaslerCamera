
using Kogerent.Services.Interfaces;

using Prism.Mvvm;

namespace Kogerent.Services.Implementation
{
    public class BenchmarkRepository :BindableBase, IBenchmarkRepository
    {
        private double _imageProcessingSpeedCounter;
        public double ImageProcessingSpeedCounter
        {
            get { return _imageProcessingSpeedCounter; }
            set { SetProperty(ref _imageProcessingSpeedCounter, value); }
        }

        private bool _rawIamage;
        public bool RawImage
        {
            get { return _rawIamage; }
            set { SetProperty(ref _rawIamage, value); }
        }

    }
}
