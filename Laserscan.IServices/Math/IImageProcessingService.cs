using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;

using Emgu.CV;
using Emgu.CV.Structure;

using Kogerent.Core;

namespace Kogerent.Services.Interfaces
{
    public interface IImageProcessingService
    {
        (Image<Bgr, byte>, IOrderedEnumerable<DefectProperties>) AnalyzeDefects(Image<Gray, byte> imgUp, Image<Gray, byte> imgDn, float widthThreshold, float heightThreshold, float widthDiscrete, float heightDiscrete, int strobe, float Shift);
        (Bitmap, IOrderedEnumerable<DefectProperties>) AnalyzeDefectsAsync(Image<Gray, byte> imgUp, Image<Gray, byte> imgDn, float widthThreshold, float heightThreshold, float widthDiscrete, float heightDiscrete, int strobe);
        BitmapImage BitmapToImageSource(Bitmap bitmap);
        void DrawBoundsWhereDefectsCanDefined(int leftBorderStart, int rightBorderStart, Image<Bgr, byte> tempBmp, string cameraId);
        void SaveDefectsPictures(Image<Bgr, byte> tempBmp, string name);
        Image<Gray, byte> FillImageDnThreshold(float dn, List<List<PointF>> sourceList, int width, int height);
        public  Image<Gray, byte> FillImageUpThreshold(float up, List<List<PointF>> sourceList, int width, int height);
        Image<Gray, byte> FillImageUpThreshold(float up, List<List<PointF>> sourceList, int width, int height, INonControlZonesRepository zones);
        
        List<DefectProperties> FilterDefects(List<DefectProperties> defects);
        List<PointF> Get2DChart(List<List<PointF>> sourceList);
        void GetContours(Image<Gray, float> img, out List<ContourData> result);
        Image<Gray, float> ProfilesToImageDn(float up, List<IEnumerable<PointF>> sourceList, int width, int height);
        Image<Gray, float> ProfilesToImageUp(float up, List<IEnumerable<PointF>> sourceList, int width, int height);
    }
}