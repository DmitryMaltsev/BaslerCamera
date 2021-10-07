using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Text;

namespace LaserScan.Core.NetStandart.Models
{
    public class MaterialModel : BindableBase
    {
        public DateTime SupplyTime { get; set; }
        public List<CameraDelta> CameraDeltaList { get; set; }
        private string _materialName;
        public string MaterialName
        {
            get { return _materialName; }
            set { SetProperty(ref _materialName, value); }
        }
        private byte _upThreshhold;
        public byte UpThreshhold
        {
            get { return _upThreshhold; }
            set { SetProperty(ref _upThreshhold, value); }
        }
        private byte _dnThreshhold;
        public byte DnThreshhold
        {
            get { return _dnThreshhold; }
            set { SetProperty(ref _dnThreshhold, value); }
        }

    }

    public class CameraDelta
    {
        public string CameraId { get; set; }
        public sbyte[] Delta { get; set; }
    }
}
