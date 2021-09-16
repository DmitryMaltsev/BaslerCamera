
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

    }
}
