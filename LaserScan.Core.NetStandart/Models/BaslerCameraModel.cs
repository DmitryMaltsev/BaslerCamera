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
        public int LeftBorder { get; set; }
        public int RightBorder { get; set; }
        public double[] P { get; set; } = new double[4];


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

        public event EventHandler<BufferData> CameraImageEvent;// событие, имеющее делегат CameraImage

        //public delegate void CameraImage(BufferData bmp); // делегат CameraImage, представляющий метод с возвращ. значением BufferData
        [XmlIgnore]
        public Camera Camera { get; set; }

        // Управляем процессом получения изображений камерой
        bool GrabOver = false;

        // Инициализация камеры
        public void CameraInit()
        {

            try
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

                // Параметр MaxNumBuffer можно использовать для управления количеством буферов, выделенных для захвата. 
                // Значение по умолчанию для этого параметра - 10.
                Camera.Parameters[PLCameraInstance.MaxNumBuffer].SetValue(700);
                Camera.Parameters[PLCamera.Height].SetValue(5);
                //Camera.Parameters[PLCamera.AcquisitionFrameRate].SetValue(10000);
                Camera.Parameters[PLCamera.ExposureTimeAbs].SetValue(94.5);

                //Camera.Parameters.Load("Settings\\left_settings.pfs", ParameterPath.CameraDevice);------

                //Camera.Parameters[PLCamera.TriggerSelector].SetValue(PLCamera.TriggerSelector.FrameStart);
                //Camera.Parameters[PLCamera.TriggerMode].SetValue(PLCamera.TriggerMode.On);
                //Camera.Parameters[PLCamera.TriggerSource].SetValue(PLCamera.TriggerSource.Line1);
                ////Camera.Parameters[PLCamera.LineTermination].SetValue(true); 
                //Camera.Parameters[PLCamera.TriggerActivation].SetValue(PLCamera.TriggerActivation.RisingEdge);
                //Camera.Parameters[PLCamera.AcquisitionFrameCount].SetValue(1000);

                //Camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);-------

                //// Select the acquisition start trigger
                //Camera.TriggerSelector.SetValue(TriggerSelector_AcquisitionStart);
                //// Set the mode for the selected trigger
                //Camera.TriggerMode.SetValue(TriggerMode_On);
                //// Set the source for the selected trigger
                //Camera.TriggerSource.SetValue(TriggerSource_Software);
                //// Set the acquisition frame count
                //Camera.AcquisitionFrameCount.SetValue(5);

                Initialized = true; // успешная инициализация
            }
            catch (Exception ex)
            {
                StopAndKill();
            }
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
    }
}
