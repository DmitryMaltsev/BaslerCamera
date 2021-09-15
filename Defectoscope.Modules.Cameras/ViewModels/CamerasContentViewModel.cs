using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Defectoscope.Modules.Cameras.Views;

using Kogerent.Core;
using Kogerent.Services.Interfaces;

using LaserScan.Core.NetStandart.Models;
using LaserScan.Core.NetStandart.Services;

using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class CamerasContentViewModel : RegionViewModelBase
    {
        public string SettingsDir => Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Settings").FullName;

        #region Rising properties
        private DelegateCommand _destroyCommand;
        public DelegateCommand DestroyCommand =>
            _destroyCommand ?? (_destroyCommand = new DelegateCommand(ExecuteDestroy));

        private DelegateCommand _initCameras;
        public DelegateCommand InitCameras =>
            _initCameras ?? (_initCameras = new DelegateCommand(ExecuteInitCameras));

        private DelegateCommand _startAllCameras;
        public DelegateCommand StartAllCameras => _startAllCameras ??= new DelegateCommand(ExecuteStart);

        private DelegateCommand _stopAllCameras;
        public DelegateCommand StopAllCameras => _stopAllCameras ??= new DelegateCommand(ExecuteStop);
        #endregion

        #region Свойства
        public IApplicationCommands ApplicationCommands { get; }
        public IBaslerService BaslerService { get; }
        public IBaslerRepository BaslerRepository { get; }
        public IContainerProvider ContainerProvider { get; }
        public IDefectRepository DefectRepository { get; }
        public IXmlService XmlService { get; }
        #endregion

        public CamerasContentViewModel(IRegionManager regionManager, IApplicationCommands applicationCommands, IBaslerService baslerService,
            IBaslerRepository baslerRepository, IContainerProvider containerProvider, IDefectRepository defectRepository, IXmlService xmlService) : base(regionManager)
        {
            ApplicationCommands = applicationCommands;
            BaslerService = baslerService;
            BaslerRepository = baslerRepository;
            ContainerProvider = containerProvider;
            DefectRepository = defectRepository;
            XmlService = xmlService;
            ApplicationCommands.Destroy.RegisterCommand(DestroyCommand);
            ApplicationCommands.InitAllSensors.RegisterCommand(InitCameras);
            //ApplicationCommands.StartAllSensors.RegisterCommand(StartAllCameras);
            ApplicationCommands.StopAllSensors.RegisterCommand(StopAllCameras);
            DefectRepository.DefectsCollection = new();
            //for (int i = 0; i < 20; i++)
            //{
            //    DefectRepository.DefectsCollection.Add(new DefectProperties
            //    {
            //        Время = DateTime.Now.AddMinutes(i),
            //        X = i,
            //        Y = i,
            //        Высота = i * i,
            //        Ширина = i - i,
            //        Тип = i % 2 == 0 ? "вдав" : "выпуклость"
            //    }); ;
            //}
        }

        public override void Destroy()
        {
            ApplicationCommands.Destroy.UnregisterCommand(DestroyCommand);
            ApplicationCommands.InitAllSensors.UnregisterCommand(InitCameras);
            ApplicationCommands.StartAllSensors.UnregisterCommand(StartAllCameras);
            ApplicationCommands.StopAllSensors.UnregisterCommand(StopAllCameras);
            base.Destroy();
        }

        #region Execute methods
        private async void ExecuteInitCameras()
        {
            List<Task> tasks = new();
            foreach (BaslerCameraModel camera in BaslerRepository.BaslerCamerasCollection)
            {
                tasks.Add(Task.Run(() => camera.CameraInit()));
            }
            Task result = Task.WhenAll(tasks);
            await result;
            BaslerRepository.AllCamerasInitialized = BaslerRepository.BaslerCamerasCollection.All(c => c.Initialized);
        }

        private void ExecuteDestroy()
        {

            ExecuteStop();
            Destroy();
        }

        private async void ExecuteStart()
        {
            List<Task> tasks = new();
            foreach (BaslerCameraModel camera in BaslerRepository.BaslerCamerasCollection)
            {
                tasks.Add(Task.Run(() => camera.Start()));
            }
            Task result = Task.WhenAll(tasks);
            await result;
        }

        private async void ExecuteStop()
        {
            List<Task> tasks = new();
            foreach (BaslerCameraModel camera in BaslerRepository.BaslerCamerasCollection)
            {
                tasks.Add(Task.Run(() => camera.StopAndKill()));
            }
            Task result = Task.WhenAll(tasks);
            await result;
        }
        #endregion

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            string path = Path.Combine(SettingsDir, "BaslerSettings.xml");
            var cameras = new List<BaslerCameraModel>();
            BaslerRepository.BaslerCamerasCollection = new(XmlService.Read(path, cameras));

            //TODO: Если файл с настройками не существует - создать настройки по умолчанию и записать их в файл.

            //string[] ips = { "0.0.0.0" };
            //List<BaslerCameraModel> cameras = BaslerService.CreateCameras(ips);
            //if (cameras.Count == 0)
            //{
            //    cameras.Add(new BaslerCameraModel { Ip = "0.0.0.0", ID = "Левая камера" });
            //    cameras.Add(new BaslerCameraModel { Ip = "1.1.1.1", ID = "Центральная камера" });
            //    cameras.Add(new BaslerCameraModel { Ip = "2.2.2.2", ID = "Правая камера" });
            //}


            //XmlService.Write(path, cameras);

            OneCameraContent Camera1V = ContainerProvider.Resolve<OneCameraContent>();
            OneCameraContentViewModel Camera1VM = ContainerProvider.Resolve<OneCameraContentViewModel>();
            Camera1V.DataContext = Camera1VM;
            if (Camera1VM != null)
            {
                Camera1VM.CurrentCamera = BaslerRepository.BaslerCamerasCollection[0];
                IRegion currentRegion = RegionManager.Regions[RegionNames.Camera1Region];
                Camera1VM.Shift = 0;
                currentRegion.Add(Camera1V);
                currentRegion.Activate(Camera1V);
            }

            OneCameraContent Camera2V = ContainerProvider.Resolve<OneCameraContent>();
            OneCameraContentViewModel Camera2VM = ContainerProvider.Resolve<OneCameraContentViewModel>();
            Camera2V.DataContext = Camera2VM;
            if (Camera2VM != null)
            {
                Camera2VM.CurrentCamera = BaslerRepository.BaslerCamerasCollection[1];
                IRegion currentRegion = RegionManager.Regions[RegionNames.Camera2Region];
                Camera2VM.Shift=6144;
                currentRegion.Add(Camera2V);
                currentRegion.Activate(Camera2V);
            }

            OneCameraContent Camera3V = ContainerProvider.Resolve<OneCameraContent>();
            OneCameraContentViewModel Camera3VM = ContainerProvider.Resolve<OneCameraContentViewModel>();
            Camera3V.DataContext = Camera3VM;
            if (Camera3V != null)
            {
                Camera3VM.CurrentCamera = BaslerRepository.BaslerCamerasCollection[2];
                IRegion currentRegion = RegionManager.Regions[RegionNames.Camera3Region];
                Camera3VM.Shift = 6144*2;
                currentRegion.Add(Camera3V);
                currentRegion.Activate(Camera3V);
            }

            

        }
    }
}
