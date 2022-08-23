using Basler.Pylon;

using Prism.Mvvm;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace LaserScan.Core.NetStandart.Models
{
    public class BaslerCameraModel : BindableBase
    {
        public bool CalibrationMode { get; set; } = false;
        public float HeightDescrete { get; set; }
        public float WidthDescrete { get; set; }
        public int Hz { get; set; }
        public string SerialNumber { get; set; }
        public float LeftBorder { get; set; }
        public float RightBorder { get; set; }
        public float CanvasWidth { get; set; }
        public double[] P { get; set; } = new double[4];
        public sbyte[] Deltas { get; set; }
        public double[] MultipleDeltas { get; set; }
        public int StartPixelPoint { get; set; }
        public int AllCamerasWidth { get; set; }
        [XmlIgnore]
        //Начало определения дефектов для текущей камеры
        public int LeftBoundIndex { get; set; } = 0;
        [XmlIgnore]
        //Конец определения дефектов для текущей камеры
        public int RightBoundIndex { get; set; } = 6144;
        public bool DefectsFound { get; set; } = false;
        //XmlIgnore
        public long ExposureTimeCurrent { get; set; }

        #region Raising properties
        private string _ip;
        public string Ip
        {
            get { return _ip; }
            set { SetProperty(ref _ip, value); }
        }

        private string id;
        public string ID
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private bool initialized = false; // инициализация
        public bool Initialized
        {
            get { return initialized; }
            set { SetProperty(ref initialized, value); }
        }
        private byte upThreshold = 60; // верхняя граница
        public byte UpThreshold
        {
            get { return upThreshold; }
            set { SetProperty(ref upThreshold, value); }
        }

        private byte downThreshold = 30; // нижняя граница
        public byte DownThreshold
        {
            get { return downThreshold; }
            set { SetProperty(ref downThreshold, value); }
        }

        private float _widthThreshold;
        public float WidthThreshold
        {
            get { return _widthThreshold; }
            set { SetProperty(ref _widthThreshold, value); }
        }

        private float _heightThreshold;
        public float HeightThreshold
        {
            get { return _heightThreshold; }
            set { SetProperty(ref _heightThreshold, value); }
        }

        private int _strobeNum;
        public int StrobeNum
        {
            get { return _strobeNum; }
            set { SetProperty(ref _strobeNum, value); }
        }

        private long _exposureTime = 1300;
        public long ExposureTime
        {
            get { return _exposureTime; }
            set { SetProperty(ref _exposureTime, value); }
        }

        private double _currentAverage;
        [XmlIgnore]
        public double CurrentAverage
        {
            get { return _currentAverage; }
            set { SetProperty(ref _currentAverage, value); }
        }

        private byte _addBrightness = 0;
        public byte Addbrightness
        {
            get { return _addBrightness; }
            set { SetProperty(ref _addBrightness, value); }
        }
        #endregion

        public event EventHandler<BufferData> CameraImageEvent;// событие, имеющее делегат CameraImage

        //public delegate void CameraImage(BufferData bmp); // делегат CameraImage, представляющий метод с возвращ. значением BufferData
        [XmlIgnore]
        public Camera Camera { get; set; }
        // Управляем процессом получения изображений камерой
        public bool GrabOver = false;

        // Инициализация камеры
        public void CameraInit()
        {
            List<ICameraInfo> allCameras = CameraFinder.Enumerate();

            ICameraInfo currentInfo = allCameras.FirstOrDefault(ci => ci["SerialNumber"] == SerialNumber);

            if (currentInfo == default)
            {
                return;
            }

            if (Camera != null)
            {
                StopAndKill();
                return;
            }

            Camera = new Camera(currentInfo);

            // режим сбора данных на свободный непрерывный сбор данных
            Camera.CameraOpened += Configuration.AcquireContinuous;

            // Событие потери соединения
            Camera.ConnectionLost += Camera_ConnectionLost;

            // Собитие старта захвата
            Camera.StreamGrabber.GrabStarted += StreamGrabber_GrabStarted;

            // Событие захвата изображения
            Camera.StreamGrabber.ImageGrabbed += StreamGrabber_ImageGrabbed;

            // Событие остановки захвата 
            Camera.StreamGrabber.GrabStopped += StreamGrabber_GrabStopped;

            //Открываем камеру
            Camera.Open();

            Camera.Parameters[PLCamera.Height].SetValue(5);
            Camera.Parameters[PLCamera.BlackLevelRaw].SetValue(0);
            Camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
            Camera.Parameters[PLCamera.AcquisitionLineRateAbs].SetValue(1000);
            ExposureTime = 1300;
            Camera.Parameters[PLCamera.ExposureTimeRaw].SetValue(ExposureTime);
            Camera.Parameters[PLCamera.GainRaw].SetValue(1000);
            Camera.Parameters[PLCamera.TriggerSource].SetValue("Line1");
            Camera.Parameters[PLCamera.TriggerSelector].SetValue("FrameStart");
            Camera.Parameters[PLCamera.TriggerMode].SetValue("On");

            //if (ID == "Центральная камера" || ID == "Правая камера")
            //{
            //    Camera.Parameters[PLCamera.ReverseX].SetValue(true);
            //}
            if (ID == "Левая камера")
            {
                Camera.Parameters[PLCamera.ReverseX].SetValue(false);
                //Camera.Parameters[PLCamera.ExposureTimeRaw].SetValue(2500);
                //ExposureTime = Camera.Parameters[PLCamera.ExposureTimeRaw].GetValue();
            }
            if (ID == "Центральная камера")
            {
                Camera.Parameters[PLCamera.ReverseX].SetValue(true);
                //Camera.Parameters[PLCamera.ExposureTimeRaw].SetValue(2200);
                //ExposureTime = Camera.Parameters[PLCamera.ExposureTimeRaw].GetValue();
            }
            if (ID == "Правая камера")
            {
                //     Camera.Parameters[PLCamera.ExposureTimeRaw].SetValue(1900);
                Camera.Parameters[PLCamera.ReverseX].SetValue(true);
            }
            Initialized = true; // успешная 
                                //   ExposureTime = Camera.Parameters[PLCamera.ExposureTimeRaw].GetValue();
        }

        public long ChangeExposureTime(int value)
        {
            long val = ExposureTime;
            if (ExposureTime < 4000 && ExposureTime > 60 && IsGrabbing())
            {
                val += value;
                Camera.Parameters[PLCamera.ExposureTimeRaw].SetValue(val);
                //  ExposureTime = Camera.Parameters[PLCamera.ExposureTimeRaw].GetValue();

            }
            return val;
        }



        private void StreamGrabber_GrabStarted(object sender, EventArgs e)
        {
            GrabOver = true;
        }

        private void StreamGrabber_ImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            IGrabResult grabResult = e.GrabResult;
            // Проверяем, можно ли отобразить изображение
            if (grabResult.IsValid)
            {
                if (GrabOver)
                {
                    // Если событие произошло, тогда выполняем метод через делегат
                    CameraImageEvent?.Invoke(this, GrabResult2Bmp(grabResult));

                }
            }
        }

        // Остановка захвата картинок 
        private void StreamGrabber_GrabStopped(object sender, GrabStopEventArgs e)
        {
            GrabOver = false;
        }

        // Удаляем камеру
        public void Camera_ConnectionLost(object sender, EventArgs e)
        {
            // Останавливаем поток 
            Camera.StreamGrabber.Stop();
            DestroyCamera();
        }

        // Одиночный захват
        public void OneShot()
        {
            if (Camera != null)
            {
                // Одиночный захват
                Camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                Camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
        }

        // Одиночный захват
        public void OneShotForCalibration()
        {
            if (Camera != null)
            {
                // Одиночный захват
                Camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                Camera.StreamGrabber.Start(1, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
        }

        // Многократный захват
        public void Start()
        {
            if (Camera != null)
            {

                // Непрерывный сбор 
                Camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                Camera.StreamGrabber.Start(GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
            }
        }

        public bool IsGrabbing()
        {
            return Camera.StreamGrabber.IsGrabbing;
        }

        // Останов захвата 
        public void StopGrabber()
        {
            if (Camera != null)
            {
                Camera.StreamGrabber.Stop();
            }
        }

        // Останов захвата и удаление камеры 
        public void StopAndKill()
        {
            if (Camera != null) // если камера существует
            {
                Camera.StreamGrabber.Stop(); // останавливаем захват 
                DestroyCamera();             // закрываем и удалем камеру
                //Camera.Dispose();
            }
        }
        /// <summary>
        /// Преобразуем изображение, захваченное камерой, в растровое изображение Bitmap
        /// </summary>
        /// <param name="grabResult"> изображение, захваченное камерой</param>
        /// <returns></returns>

        private BufferData GrabResult2Bmp(IGrabResult grabResult)
        {
            return new BufferData
            {
                Data = grabResult.PixelData as byte[],
                Height = grabResult.Height,
                Width = grabResult.Width
            };
        }

        // Убиваем камеру
        public void DestroyCamera()
        {
            if (Camera != null)
            {
                Camera.Close();
                Camera.Dispose();
                Camera = null;
            }
        }

        //public void SetCameraExposureTime(int exposureTime)
        //{
        //    CurrentExposureTimeRaw = exposureTime;
        //    Camera.Parameters[PLCamera.ExposureTimeRaw].SetValue(CurrentExposureTimeRaw);
        //}
    }
}
