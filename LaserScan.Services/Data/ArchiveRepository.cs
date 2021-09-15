using Kogerent.Core;
using Kogerent.Core.Models;
using Kogerent.Services.Interfaces;
using Prism.Mvvm;
using System.Collections.Generic;

namespace Kogerent.Services.Implementation
{
    public class ArchiveRepository : BindableBase, IArchiveRepository
    {
        private List<DateTimePoint> _dateTimePointsCollection = new();
        public List<DateTimePoint> DateTimePointsCollection
        {
            get { return _dateTimePointsCollection; }
            set { SetProperty(ref _dateTimePointsCollection, value); }
        }

        private List<IntXFloatYPoint> _intXFloatYPointsCollection = new();
        public List<IntXFloatYPoint> IntXFloatYPointsCollection
        {
            get { return _intXFloatYPointsCollection; }
            set { SetProperty(ref _intXFloatYPointsCollection, value); }
        }
    }
}
