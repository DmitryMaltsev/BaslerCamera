using Kogerent.Services.Interfaces;

using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogerent.Services.Repositories
{

    public class FilesRepository : BindableBase, IFilesRepository
    {
        private bool _isRecordingRawData=false;
        public bool IsRecordingRawData
        {
            get { return _isRecordingRawData; }
            set { SetProperty(ref _isRecordingRawData, value); }
        }

        private bool _isRecordStopped=true;
        public bool IsRecordStopped
        {
            get { return _isRecordStopped; }
            set { SetProperty(ref _isRecordStopped, value); }
        }

        private ObservableCollection<double> _filesRecordingTime;
        public ObservableCollection<double> FilesRecordingTime
        {
            get { return _filesRecordingTime; }
            set { SetProperty(ref _filesRecordingTime, value); }
        }

        private ObservableCollection<int> _filesRawsCount;
        public ObservableCollection<int> FilesRawCount
        {
            get { return _filesRawsCount; }
            set { SetProperty(ref _filesRawsCount, value); }
        }

        private bool _isRecording = false;
        public bool IsRecording
        {
            get { return _isRecording; }
            set { SetProperty(ref _isRecording, value); }
        }

        public FilesRepository()
        {
            FilesRecordingTime = new ObservableCollection<double>();
            FilesRawCount = new ObservableCollection<int>();
            for (int i = 0; i < 3; i++)
            {
                FilesRecordingTime.Add(0);
                FilesRawCount.Add(0);
            }

        }

    }
}
