using System.Drawing;
using System.Windows.Markup;
using Emgu.CV.Structure;
using Emgu.CV.Util;

[assembly: XmlnsDefinition("http://kogerent.org", "Kogerent.Core")]
namespace Kogerent.Core
{

    public class ContourData
    {
        public VectorOfPoint Contour { get; set; }
        public double Area { get; set; }
        public double Perimeter { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public RotatedRect RotRect { get; set; }
        public Rectangle Rectangle { get; set; }
    }
}
