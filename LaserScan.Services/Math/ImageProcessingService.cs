using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

using Kogerent.Core;
using Kogerent.Logger;
using Kogerent.Services.Interfaces;

namespace Kogerent.Services.Implementation
{
    /// <summary>
    /// Служба обработки изображений и поиска дефектов на них
    /// </summary>
    public class ImageProcessingService : IImageProcessingService
    {
        #region Private Fields
        private Pen _red = new Pen(Brushes.Red, 3f);
        private Pen _blue = new Pen(Brushes.Blue, 3f);

        private Bgr _redBgr = new Bgr(0, 0, 255);
        private Bgr _blueBgr = new Bgr(255, 0, 0);

        private Type _mathServiceType = typeof(ImageProcessingService);
        #endregion

        #region Properties
        public ILogger Logger { get; }

        public INonControlZonesRepository Zones { get; }
        #endregion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ImageProcessingService(ILogger logger, INonControlZonesRepository zones)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Logger = logger;
            Zones = zones;
        }


        #region Public Methods
        public Image<Gray, byte> FillImageUpThreshold(float up, List<List<PointF>> sourceList, int width, int height)
        {

            var img = new Image<Gray, byte>(width, height);
            if (sourceList == null) return img;
            if (sourceList.Count <= 0) return img;
            var mcData = img.Data;
            int workingHeight = Math.Min(height, sourceList.Count);
            for (int h = 0; h < workingHeight; h++)
            {
                if (sourceList[h] == null) continue;
                var row = sourceList[h];
                int cellCount = row.Count;
                for (int w = 0; w < cellCount; w++)
                {
                    if (row[w].Y > up) mcData[h, w, 0] = 255;
                }
            }
            img.Data = mcData;
            return img;
        }

        public Image<Gray, byte> FillImageUpThreshold(float up, List<List<PointF>> sourceList, int width, int height, INonControlZonesRepository zones)
        {
            var minXsO = zones.Obloys.Select(o => o.MinimumX).ToArray();
            var maxXsO = zones.Obloys.Select(o => o.MaximumX).ToArray();
            var minXsZ = zones.Zones.Select(o => o.MinimumX).ToArray();
            var maxXsZ = zones.Zones.Select(o => o.MaximumX).ToArray();
            var img = new Image<Gray, byte>(width, height);
            if (sourceList == null) return img;
            if (sourceList.Count <= 0) return img;
            var mcData = img.Data;
            int workingHeight = Math.Min(height, sourceList.Count);
            for (int h = 0; h < workingHeight; h++)
            {
                if (sourceList[h] == null) continue;
                var row = sourceList[h];
                int cellCount = row.Count;
                for (int w = 0; w < cellCount; w++)
                {
                    bool inZone = false;
                    for (int i = 0; i < minXsO.Length; i++)
                    {
                        if (w >= minXsO[i] && w < minXsO[i])
                        {
                            inZone = true;
                            break;
                        }
                    }
                    if (inZone) continue;
                    for (int i = 0; i < minXsZ.Length; i++)
                    {
                        if (w >= minXsZ[i] && w < minXsZ[i])
                        {
                            inZone = true;
                            break;
                        }
                    }
                    if (inZone) continue;

                    if (row[w].Y > up) mcData[h, w, 0] = 255;
                }
            }
            img.Data = mcData;
            return img;
        }

        public Image<Gray, byte> FillImageDnThreshold(float dn, List<List<PointF>> sourceList, int width, int height)
        {

            var img = new Image<Gray, byte>(width, height);
            if (sourceList == null) return img;
            if (sourceList.Count() <= 0) return img;
            var mcData = img.Data;
            int workingHeight = Math.Min(height, sourceList.Count());
            for (int h = 0; h < workingHeight; h++)
            {
                if (sourceList[h] == null) continue;
                var row = sourceList[h];
                int cellCount = row.Count;
                for (int w = 0; w < cellCount; w++)
                {
                    if (row[w].Y < dn) mcData[h, w, 0] = 255;
                }
            }
            img.Data = mcData;
            return img;
        }

        public Image<Gray, float> ProfilesToImageUp(float up, List<IEnumerable<PointF>> sourceList, int width, int height)
        {
            var img = new Image<Gray, float>(width, height);
            var mcData = img.Data;
            int workingHeight = Math.Min(height, sourceList.Count);
            for (int h = 0; h < workingHeight; h++)
            {
                var row = sourceList[h].ToList();
                int cellCount = row.Count;
                for (int w = 0; w < cellCount; w++)
                {
                    if (row[w].Y >= up)
                        mcData[h, w, 0] = row[w].Y;
                }
            }
            img.Data = mcData;
            return img;
        }

        public Image<Gray, float> ProfilesToImageDn(float up, List<IEnumerable<PointF>> sourceList, int width, int height)
        {
            var img = new Image<Gray, float>(width, height);
            var mcData = img.Data;
            int workingHeight = Math.Min(height, sourceList.Count);
            for (int h = 0; h < workingHeight; h++)
            {
                var row = sourceList[h].ToList();
                int cellCount = row.Count;
                for (int w = 0; w < cellCount; w++)
                {
                    if (row[w].Y <= up)
                        mcData[h, w, 0] = row[w].Y;
                }
            }
            img.Data = mcData;
            return img;
        }

        public List<PointF> Get2DChart(List<List<PointF>> sourceList)
        {
            List<PointF> result = new List<PointF>();
            int height = sourceList.Count;
            foreach (var list in sourceList)
            {
                int y = sourceList.IndexOf(list);
                result.AddRange(list.Select(p => new PointF { X = p.X, Y = y }));
            }
            return result;
        }

        public BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                ms.Position = 0;
                BitmapImage bmpImage = new BitmapImage();
                bmpImage.BeginInit();
                bmpImage.StreamSource = ms;
                bmpImage.CacheOption = BitmapCacheOption.OnLoad;
                bmpImage.EndInit();

                //bmpImage.Freeze();
                return bmpImage;
            }
        }

        public void GetContours(Image<Gray, byte> img, out List<ContourData> result)
        {
            List<ContourData> contours = new();
            VectorOfVectorOfPoint c = new();
            CvInvoke.FindContours(img, c, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            if (c.Size > 0)
            {
                for (int i = 0; i < c.Size; i++)
                {
                    double perimeter = CvInvoke.ArcLength(c[i], true);
                    if (perimeter > 0)
                    {
                        Rectangle rectangle = CvInvoke.BoundingRectangle(c[i]);
                        double area = CvInvoke.ContourArea(c[i]);
                        RotatedRect rotatedRectangle = CvInvoke.MinAreaRect(c[i]);
                        contours.Add(new ContourData()
                        {
                            Contour = c[i],
                            Area = area,
                            RotRect = rotatedRectangle,
                            Rectangle = rectangle,
                            Perimeter = perimeter,
                            Width = rectangle.Width,
                            Height = rectangle.Height
                        });

                    }
                }
                result = contours;
            }
            else
            {
                result = null;
            }
        }

        public void GetContours(Image<Gray, float> img, out List<ContourData> result)
        {
            List<ContourData> contours = new();
            VectorOfVectorOfPoint c = new();
            CvInvoke.FindContours(img, c, null, RetrType.External, ChainApproxMethod.ChainApproxNone);
            if (c.Size > 0)
            {
                for (int i = 0; i < c.Size; i++)
                {
                    double perimeter = CvInvoke.ArcLength(c[i], true);
                    if (perimeter > 0)
                    {
                        Rectangle rectangle = CvInvoke.BoundingRectangle(c[i]);
                        double area = CvInvoke.ContourArea(c[i]);
                        RotatedRect rotatedRectangle = CvInvoke.MinAreaRect(c[i]);
                        contours.Add(new ContourData()
                        {
                            Contour = c[i],
                            Area = area,
                            RotRect = rotatedRectangle,
                            Rectangle = rectangle,
                            Perimeter = perimeter,
                            Width = rectangle.Width,
                            Height = rectangle.Height
                        });

                    }
                }
                result = contours;
            }
            else
            {
                result = null;
            }
        }

        //public Task<Bitmap> AnalyzeDefectsAsync(Image<Gray, float> imgUp, Image<Gray, float> imgDn, float widthThreshold,
        //                                        float heightThreshold, float widthDiscrete, float heightDiscrete,
        //                                        int strobe)
        //{
        //    return Task.Run(() =>
        //    {
        //        GetContours(imgUp, out List<ContourData> upContours);
        //        GetContours(imgDn, out List<ContourData> dnContours);
        //        var imgWidth = Math.Max(imgDn.Width, imgUp.Width);
        //        var imgHeight = Math.Max(imgDn.Height, imgUp.Height);
        //        var tempBmp = new Bitmap(imgWidth, imgHeight);

        //        if (upContours != null && upContours.Count > 0)
        //        {
        //            DrawDefectProperties(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete, imgWidth, imgHeight, strobe,
        //                                upContours, tempBmp, true);
        //        }

        //        if (dnContours != null && dnContours.Count > 0)
        //        {
        //            DrawDefectProperties(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete, imgWidth, imgHeight, strobe,
        //                                 dnContours, tempBmp, false);
        //        }
        //        return tempBmp;
        //    });
        //}
        public (Bitmap, IOrderedEnumerable<DefectProperties>) AnalyzeDefectsAsync(Image<Gray, byte> imgUp,
                                                                                  Image<Gray, byte> imgDn,
                                                                                  float widthThreshold,
                                                                                  float heightThreshold,
                                                                                  float widthDiscrete,
                                                                                  float heightDiscrete, int strobe)
        {
            List<DefectProperties> defects = new();
            var defectCollection = new ConcurrentBag<DefectProperties>();
            GetContours(imgUp, out List<ContourData> upContours);
            GetContours(imgDn, out List<ContourData> dnContours);
            var imgWidth = Math.Max(imgDn.Width, imgUp.Width);
            var imgHeight = Math.Max(imgDn.Height, imgUp.Height);
            var tempBmp = new Bitmap(imgWidth, imgHeight);

            if (upContours != null && upContours.Count > 0)
            {
                defects.AddRange(DrawDefectProperties(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete, imgWidth, imgHeight, strobe,
                                    upContours, tempBmp, true));
            }

            if (dnContours != null && dnContours.Count > 0)
            {
                defects.AddRange(DrawDefectProperties(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete, imgWidth, imgHeight, strobe,
                                     dnContours, tempBmp, false));
            }

            return (tempBmp, defects.OrderBy(d => d.Время));
        }

        /// <summary>
        /// Удаляет дефекты, которые попадают в зоны неконтроля
        /// </summary>
        /// <param name="defects">Коллекция дефектов</param>
        public List<DefectProperties> FilterDefects(List<DefectProperties> defects)
        {
            List<float> minXs = Zones.Obloys.Select(o => o.MinimumX).ToList();
            //minXs.AddRange(Zones.Zones.Select(z => z.MinimumX));

            List<float> maxXs = Zones.Obloys.Select(o => o.MaximumX).ToList();
            // maxXs.AddRange(Zones.Obloys.Select(z => z.MaximumX));
            if (defects.Count > 0)
            {
                for (int i = 0; i < minXs.Count; i++)
                {
                    float min = minXs[i];
                    float max = maxXs[i];
                    int res = defects.RemoveAll(d => d.X >= min && d.X < max);
                }
            }
            return defects;
        }

        public (Image<Bgr, byte>, IOrderedEnumerable<DefectProperties>) AnalyzeDefects(Image<Gray, byte> imgUp,
                                                                                  Image<Gray, byte> imgDn,
                                                                                  float widthThreshold,
                                                                                  float heightThreshold,
                                                                                  float widthDiscrete,
                                                                                  float heightDiscrete, int strobe)
        {
            List<DefectProperties> defects = new();
            GetContours(imgUp, out List<ContourData> upContours);
            GetContours(imgDn, out List<ContourData> dnContours);

            int imgWidth = Math.Max(imgDn.Width, imgUp.Width);
            int imgHeight = Math.Max(imgDn.Height, imgUp.Height);

            Image<Bgr, byte> tempBmp = new Image<Bgr, byte>(imgWidth, imgHeight);
            //Image<Bgr, byte> tempUpBmp = new Image<Bgr, byte>(upContours.G, imgHeight);
            //Image<Bgr, byte> tempDownBmp = new Image<Bgr, byte>(imgWidth, imgHeight);
            if (upContours != null && upContours.Count > 0)
            {
                //foreach (ContourData upContour in upContours)
                //{
                defects.AddRange(DrawDefectPropertiesEmgu(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete,
                                                         imgWidth, imgHeight, strobe, upContours, tempBmp, true));
                //}
            }
            if (dnContours != null && dnContours.Count > 0)
            {
                //foreach (var dnContour in dnContours)
                //{
                defects.AddRange(DrawDefectPropertiesEmgu(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete,
                                                         imgWidth, imgHeight, strobe, dnContours, tempBmp, false));
                //}
            }

            return (tempBmp, defects.OrderBy(d => d.Время));
        }
        #endregion

        #region Private Methods
        private List<DefectProperties> DrawDefectProperties(float widthThreshold, float heightThreshold,
                                                            float widthDiscrete, float heightDiscrete, int imgWidth,
                                                            int imgHeight, int strobe, List<ContourData> dnContours,
                                                            Bitmap tempBmp, bool up)
        {

            List<DefectProperties> defects = new();
            try
            {
                var rectColor = up ? _blue : _red;
                var largeContours = dnContours.Where(c => c.Width * widthDiscrete >= widthThreshold && c.Height * heightDiscrete >= heightThreshold).ToList();

                foreach (ContourData c in largeContours)
                {
                    Point center = new((int)c.RotRect.Center.X, (int)c.RotRect.Center.Y);
                    Size size = new(imgWidth / 2, imgHeight);
                    Rectangle rectangle = c.Rectangle;
                    long imageCount = strobe / (uint)imgHeight;

                    DefectProperties defect = new()
                    {
                        X = Math.Round(center.X * widthDiscrete, 1),
                        Y = Math.Round(((uint)center.Y + imageCount * (uint)imgHeight) * heightDiscrete, 1),
                        Ширина = Math.Round(rectangle.Width * widthDiscrete, 1),
                        Высота = Math.Round(rectangle.Height * heightDiscrete, 1),
                        Тип = up ? "выпуклость" : "вдав",
                        Время = DateTime.Now
                    };
                    defects.Add(defect);

                    var rectF = new Rectangle(rectangle.Location, size);


                    using (Graphics g = Graphics.FromImage(tempBmp))
                    {

                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawRectangle(rectColor, rectangle);

                    }

                }

            }
            catch (Exception ex)
            {
                Logger?.Error($"{ex.Message}", _mathServiceType);
            }
            return defects;
        }

        private List<DefectProperties> DrawDefectPropertiesEmgu(float widthThreshold, float heightThreshold,
                                                            float widthDiscrete, float heightDiscrete, int imgWidth,
                                                            int imgHeight, int strobe, List<ContourData> dnContours,
                                                            Image<Bgr, byte> tempBmp, bool up)
        {
            List<DefectProperties> defects = new();
            Bgr rectColor = up ? _blueBgr : _redBgr;
            List<ContourData> largeContours = dnContours.Where(c => c.Width * widthDiscrete >= widthThreshold && c.Height * heightDiscrete >= heightThreshold).ToList();
            List<float> minXs = Zones.Obloys.Select(o => o.MinimumX).ToList();
            //minXs.AddRange(Zones.Zones.Select(z => z.MinimumX));

            List<float> maxXs = Zones.Obloys.Select(o => o.MaximumX).ToList();
            // maxXs.AddRange(Zones.Obloys.Select(z => z.MaximumX));   
            foreach (ContourData c in largeContours)
            {
                Point center = new((int)c.RotRect.Center.X, (int)c.RotRect.Center.Y);
                Rectangle rectangle = c.Rectangle;
                var imageCount = strobe / (uint)imgHeight;

                DefectProperties defect = new DefectProperties
                {
                    X = Math.Round(center.X * widthDiscrete, 1),
                    Y = Math.Round(((uint)center.Y + imageCount * (uint)imgHeight) * heightDiscrete, 1),
                    Ширина = Math.Round(rectangle.Width * widthDiscrete, 1),
                    Высота = Math.Round(rectangle.Height * heightDiscrete, 1),
                    Тип = up ? "выпуклость" : "вдав",
                    Время = DateTime.Now
                };
                if (defect != null)
                {
                    for (int i = 0; i < minXs.Count; i++)
                    {
                        float min = minXs[i];
                        float max = maxXs[i];
                        // int res = defects.RemoveAll(d => d.X >= min && d.X < max);
                        if (!(defect.X >= min && defect.X < max))
                        {
                            defect = null;
                            continue;
                        }
                    }
                }
                   
                {
                    defects.Add(defect);
                    Size size = new(rectangle.Width, rectangle.Height);
                    Rectangle rectF = new Rectangle(rectangle.Location, size);
                    tempBmp.Draw(rectF, rectColor, 40);
                }
            }
            return defects;
        }
        #endregion
    }
}
