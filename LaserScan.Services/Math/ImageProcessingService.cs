using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Kogerent.Core;
using Kogerent.Logger;
using Kogerent.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

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
        public IBaslerRepository BaslerRepository { get; }
        #endregion

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public ImageProcessingService(ILogger logger, INonControlZonesRepository zones, IBaslerRepository baslerRepository)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Logger = logger;
            Zones = zones;
            BaslerRepository = baslerRepository;
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

        private VectorOfVectorOfPoint GetContours(Image<Gray, byte> img, out List<ContourData> result)
        {
            List<ContourData> contours = new();
            VectorOfVectorOfPoint c = new();
            CvInvoke.FindContours(img, c, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
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
                        if (rotatedRectangle.Center.Y > 1000)
                        {
                            rotatedRectangle.Center.Y = 0;
                        }
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
            return c;
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
            //  GetContours(imgUp, out List<ContourData> upContours);
            //  GetContours(imgDn, out List<ContourData> dnContours);
            var imgWidth = Math.Max(imgDn.Width, imgUp.Width);
            var imgHeight = Math.Max(imgDn.Height, imgUp.Height);
            var tempBmp = new Bitmap(imgWidth, imgHeight);

            //if (upContours != null && upContours.Count > 0)
            //{
            //    defects.AddRange(DrawDefectProperties(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete, imgWidth, imgHeight, strobe,
            //                        upContours, tempBmp, true));
            //}

            //if (dnContours != null && dnContours.Count > 0)
            //{
            //    defects.AddRange(DrawDefectProperties(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete, imgWidth, imgHeight, strobe,
            //                         dnContours, tempBmp, false));
            // }

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
                                                                                  float heightDiscrete, int strobe, float Shift)
        {
            List<DefectProperties> defects = new();
            VectorOfVectorOfPoint upCnt = GetContours(imgUp, out List<ContourData> upContours);
            VectorOfVectorOfPoint dnCnt = GetContours(imgDn, out List<ContourData> dnContours);

            int imgWidth = Math.Max(imgUp.Width, imgUp.Width);
            int imgHeight = Math.Max(imgDn.Height, imgUp.Height);

            Image<Bgr, byte> tempBmp = new Image<Bgr, byte>(imgWidth, imgHeight);
            //Image<Bgr, byte> tempUpBmp = new Image<Bgr, byte>(upContours.G, imgHeight);
            //Image<Bgr, byte> tempDownBmp = new Image<Bgr, byte>(imgWidth, imgHeight);


            if (upContours != null && upContours.Count > 0)
            {
                //foreach (ContourData upContour in upContours)
                //{
                defects.AddRange(DrawDefectPropertiesEmgu(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete,
                                                         imgWidth, imgHeight, strobe, upContours, tempBmp, true, Shift, upCnt));
                //}
            }
            if (dnContours != null && dnContours.Count > 0)
            {
                //foreach (var dnContour in dnContours)
                //{
                defects.AddRange(DrawDefectPropertiesEmgu(widthThreshold, heightThreshold, widthDiscrete, heightDiscrete,
                                                         imgWidth, imgHeight, strobe, dnContours, tempBmp, false, Shift, dnCnt));
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
                                                            Image<Bgr, byte> tempBmp, bool up, float Shift, VectorOfVectorOfPoint contours)
        {
            List<DefectProperties> defects = new();
            Bgr rectColor = up ? _blueBgr : _redBgr;
            MCvScalar currentColor;
            if (up == true)
                currentColor = new MCvScalar(255, 0, 0);
            else
                currentColor = new MCvScalar(0, 0, 255);

            List<ContourData> largeContours = dnContours.Where(c => c.Width * widthDiscrete >= widthThreshold && c.Height * heightDiscrete >= heightThreshold).ToList();
            VectorOfVectorOfPoint resultDefects = new();
            List<float> minObloysXs = Zones.Obloys.Select(o => o.MinimumX).ToList();
            List<float> maxObloysXs = Zones.Obloys.Select(o => o.MaximumX).ToList();
            List<float> minZonesXs = Zones.Zones.Select(z => z.MinimumX).ToList();
            List<float> maxZonesXs = Zones.Zones.Select(z => z.MaximumX).ToList();
            foreach (ContourData c in largeContours)
            {
                Point center = new((int)c.RotRect.Center.X, (int)c.RotRect.Center.Y);
                Rectangle rectangle = c.Rectangle;
                var imageCount = strobe / (uint)imgHeight;

                DefectProperties defect = new DefectProperties
                {
                    X = Math.Round(center.X * widthDiscrete + Shift, 1),
                    Y = Math.Round(((uint)center.Y + imageCount * (uint)imgHeight) * heightDiscrete, 1),
                    Ширина = Math.Round(rectangle.Width * widthDiscrete, 1),
                    Высота = Math.Round(rectangle.Height * heightDiscrete, 1),
                    Тип = up ? "выпуклость" : "вдав",
                    Время = DateTime.Now
                };

                // int res = defects.RemoveAll(d => d.X >= min && d.X < max);
                //float bufferMin = minXs.Find(p => p >= defect.X);
                //float bufferMax = maxXs.Find(p => p <= defect.X);
                if (defect != null && (maxObloysXs[0] < defect.X) && (minObloysXs[1] > defect.X))
                {
                    #region Комментарии
                    //bool defectNotInZone = true;
                    //for (int i = 0; i < minZonesXs.Count; i++)
                    //{
                    //    if (minZonesXs[i] <= defect.X && defect.X <= maxZonesXs[i])
                    //    {
                    //        defectNotInZone = false;
                    //        continue;
                    //    }
                    //}
                    //if (defectNotInZone)
                    // {
                    //if (BaslerRepository.BaslerCamerasCollection[1].LeftBorder < defect.X && defect.X < BaslerRepository.BaslerCamerasCollection[1].RightBorder)
                    //{
                    //    defect.X -= BaslerRepository.BaslerCamerasCollection[1].LeftBoundWidth;
                    //}
                    //else
                    //if (BaslerRepository.BaslerCamerasCollection[1].RightBorder < defect.X)
                    //{
                    //    defect.X -= BaslerRepository.BaslerCamerasCollection[1].LeftBoundWidth + BaslerRepository.BaslerCamerasCollection[1].RightBoundWidth;
                    //}

                    #endregion
                    defects.Add(defect);
                    Size size = new(rectangle.Width, rectangle.Height);
                    // Rectangle rectF = new Rectangle(rectangle.Location, size);
                    CvInvoke.Polylines(tempBmp, c.Contour, true, currentColor, 20);
                    //  }
                    // tempBmp.Draw(rectangle, rectColor, 20);
                }
            }
            //if (up == true)
            //{
            //    CvInvoke.DrawContours(tempBmp, contours, -1, new MCvScalar(255, 0, 0), 20, LineType.Filled);
            //}
            //else
            //{
            //    CvInvoke.DrawContours(tempBmp, contours, -1, new MCvScalar(0, 0, 255), 20, LineType.Filled);
            //}
            return defects;
        }
        #endregion

        public void DrawDefects(Image<Bgr, byte> tempBmp, string name)
        {
            string currentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images", DateTime.Now.ToString("dd.MM.yyyy"));
            if (Directory.Exists(currentDirectory))
            {
                string fileName = DateTime.Now.ToString("HH.mm.ss.ffffff") + "_" + name + ".bmp";
                string imagesPath = Path.Combine(currentDirectory, fileName);
                using (Bitmap bmpIm = tempBmp.ToBitmap())
                {
                    bmpIm.Save(imagesPath, ImageFormat.Bmp);
                }
            }
            else
            {
                Directory.CreateDirectory(currentDirectory);
            }
        }


        public void DrawBoundsWhereDefectsCanDefined(int leftBorderStart, int rightBorderStart,
                                                                            Image<Bgr, byte> tempBmp, string cameraId)
        {
            MCvScalar currentColor = new MCvScalar(0, 215, 255);
            if (leftBorderStart > 6144 && cameraId == "Центральная камера")
            {
                leftBorderStart -= 6144;
                Point p1 = new() { X = leftBorderStart, Y = 0 }; Point p2 = new() { X = leftBorderStart, Y = 500 };
                CvInvoke.Line(tempBmp, p1, p2, currentColor, 50, LineType.Filled, 0);
                //   float bound = baslerRepository.BaslerCamerasCollection[1].WidthDescrete * leftBound;
            }
            else
                if (leftBorderStart < 6144 && cameraId == "Левая камера")
            {
                Point p1 = new() { X = leftBorderStart, Y = 0 }; Point p2 = new() { X = leftBorderStart, Y = 500 };
                CvInvoke.Line(tempBmp, p1, p2, currentColor, 50, LineType.EightConnected, 0);
            }
            if (rightBorderStart > 6144 * 2 && cameraId == "Правая камера")
            {
                rightBorderStart -= 6144 * 2;
                Point p1 = new() { X = rightBorderStart, Y = 0 }; Point p2 = new() { X = rightBorderStart, Y = 500 };
                CvInvoke.Line(tempBmp, p1, p2, currentColor, 50, LineType.Filled, 0);
                //   float bound = baslerRepository.BaslerCamerasCollection[1].WidthDescrete * leftBound;
            }
            else
                if (rightBorderStart < 6144 * 2 && rightBorderStart > (6144 + 6144 / 2) && cameraId == "Центральная камера")
            {
                rightBorderStart -= 6144;
                Point p1 = new() { X = rightBorderStart, Y = 0 }; Point p2 = new() { X = rightBorderStart, Y = 500 };
                CvInvoke.Line(tempBmp, p1, p2, currentColor, 50, LineType.EightConnected, 0);
            }
        }
    }
}
