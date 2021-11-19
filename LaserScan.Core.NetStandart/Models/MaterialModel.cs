using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Text;

namespace LaserScan.Core.NetStandart.Models
{
    public class MaterialModel : BindableBase
    {
        public DateTime SupplyTime { get; set; }
        private List<CameraDelta> _cameraDeltaList;
        public List<CameraDelta> CameraDeltaList
        {
            get { return _cameraDeltaList; }
            set { SetProperty(ref _cameraDeltaList, value); }
        }
        private string _materialName;
        public string MaterialName
        {
            get { return _materialName; }
            set { SetProperty(ref _materialName, value); }
        }
       

    }

    public class CameraDelta:BindableBase
    {
        public string CameraId { get; set; }
        public sbyte[] Deltas { get; set; }
        //public double[] Deltas { get; set; }

        private byte _upThreshhold;
        public byte UpThreshhold
        {
            get { return _upThreshhold; }
            set { SetProperty(ref _upThreshhold, value); }
        }
        private byte _dnThreshhold;
        public byte DownThreshhold
        {
            get { return _dnThreshhold; }
            set { SetProperty(ref _dnThreshhold, value); }
        }
    }
}
