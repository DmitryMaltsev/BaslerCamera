using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Text;

namespace LaserScan.Core.NetStandart.Models
{
    public class CameraStatisticsData : BindableBase
    {

        private long _totalBufferCount;
        public long TotalBufferCount
        {
            get { return _totalBufferCount; }
            set
            {
                SetProperty(ref _totalBufferCount, value);
                BufferCountWithHeight = _totalBufferCount * StrobesHeight;
            }

        }

        private long _failedBufferCount;
        public long FailedBufferCount
        {
            get { return _failedBufferCount; }
            set { SetProperty(ref _failedBufferCount, value); }
        }

        private long _bufferCountWithHeight;
        public long BufferCountWithHeight
        {
            get { return _bufferCountWithHeight; }
            set
            {
                SetProperty(ref _bufferCountWithHeight, value);
            }
        }

        private int _strobesHeight = 5;
        public int StrobesHeight
        {
            get { return _strobesHeight; }
            set { SetProperty(ref _strobesHeight, value); }
        }

        private long _strobesCount;
        public long StrobesCount
        {
            get { return _strobesCount; }
            set { SetProperty(ref _strobesCount, value); }
        }
    }
}
