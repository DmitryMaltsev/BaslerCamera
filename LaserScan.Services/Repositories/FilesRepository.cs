using Kogerent.Services.Interfaces;

using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kogerent.Services.Repositories
{
    public class FilesRepository : BindableBase, IFilesRepository
    {
        private float[] _filesRecordingTime;
        public float[] FilesRecordingTime
        {
            get { return _filesRecordingTime; }
            set { SetProperty(ref _filesRecordingTime, value); }
        }

        private int[] _filesRawsCount;
        public int[] FilesRawCount
        {
            get { return _filesRawsCount; }
            set { SetProperty(ref _filesRawsCount, value); }
        }



    }
}
