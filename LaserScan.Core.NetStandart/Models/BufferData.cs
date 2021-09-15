using System;
using System.Collections.Generic;
using System.Text;

namespace LaserScan.Core.NetStandart.Models
{
    public class BufferData
    {
        public byte[] Data { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
