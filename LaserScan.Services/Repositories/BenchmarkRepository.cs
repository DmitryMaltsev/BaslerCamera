
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

        private bool _rawIamage=false;
        public bool RawImage
        {
            get { return _rawIamage; }
            set { SetProperty(ref _rawIamage, value); }
        }

        private int _tempQueueCount;
        public int TempQueueCount
        {
            get { return _tempQueueCount; }
            set { SetProperty(ref _tempQueueCount, value); }
        }

        private double _defectsProcessingTimer;
        public double DefectsProcessingTimer
        {
            get { return _defectsProcessingTimer; }
            set { SetProperty(ref _defectsProcessingTimer, value); }
        }

        private int _leftStrobe;
        public int LeftStrobe
        {
            get { return _leftStrobe; }
            set { SetProperty(ref _leftStrobe, value); }
        }

        private int _centerStrobe;
        public int CenterStrobe
        {
            get { return _centerStrobe; }
            set { SetProperty(ref _centerStrobe, value); }
        }

        private int _rightStrobe;
        public int RightStrobe
        {
            get { return _rightStrobe; }
            set { SetProperty(ref _rightStrobe, value); }
        }
    }
}
