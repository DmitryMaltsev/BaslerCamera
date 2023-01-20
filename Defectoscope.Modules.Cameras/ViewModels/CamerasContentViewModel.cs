using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

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
        public INonControlZonesRepository NonControlZonesRepository { get; }
        #endregion

        public CamerasContentViewModel(IRegionManager regionManager, IApplicationCommands applicationCommands, IBaslerService baslerService,
            IBaslerRepository baslerRepository, IContainerProvider containerProvider, IDefectRepository defectRepository, IXmlService xmlService,
            INonControlZonesRepository nonControlZonesRepository) : base(regionManager)
        {
            ApplicationCommands = applicationCommands;
            BaslerService = baslerService;
            BaslerRepository = baslerRepository;
            ContainerProvider = containerProvider;
            DefectRepository = defectRepository;
            XmlService = xmlService;
            NonControlZonesRepository = nonControlZonesRepository;
            ApplicationCommands.Destroy.RegisterCommand(DestroyCommand);
            //ApplicationCommands.InitAllSensors.RegisterCommand(InitCameras);
            //ApplicationCommands.StartAllSensors.RegisterCommand(StartAllCameras);
            //ApplicationCommands.StopAllSensors.RegisterCommand(StopAllCameras);
            DefectRepository.DefectsCollection = new();
        }

        public override void Destroy()
        {
            ApplicationCommands.Destroy.UnregisterCommand(DestroyCommand);
            //ApplicationCommands.InitAllSensors.UnregisterCommand(InitCameras);
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
            List<BaslerCameraModel> cameras = new List<BaslerCameraModel>();
            BaslerRepository.BaslerCamerasCollection = new(XmlService.Read(path, cameras));
            BaslerRepository.LeftBorder = BaslerRepository.BaslerCamerasCollection[0].LeftBorder;
            BaslerRepository.RightBorder = BaslerRepository.BaslerCamerasCollection[0].RightBorder;
            //string materialPath = Path.Combine(SettingsDir, "MaterialSettings.xml");
            //List<MaterialModel> materials = new();
            //BaslerRepository.MaterialModelCollection = new(XmlService.Read(materialPath, materials));
            //if (BaslerRepository.CurrentCamera.Deltas != null && BaslerRepository.CurrentMaterial.CameraDeltaList.Count > 0)
            //{
            //    for (int i = 0; i < BaslerRepository.CurrentMaterial.CameraDeltaList.Count; i++)
            //    {
            //        for (int j = 0; j < BaslerRepository.BaslerCamerasCollection.Count; j++)
            //        {
            //            if (BaslerRepository.CurrentMaterial.CameraDeltaList[i].CameraId == BaslerRepository.BaslerCamerasCollection[j].ID)
            //            {
            //                BaslerRepository.BaslerCamerasCollection[j].DownThreshold = BaslerRepository.CurrentMaterial.CameraDeltaList[i].DownThreshhold;
            //                BaslerRepository.BaslerCamerasCollection[j].UpThreshold = BaslerRepository.CurrentMaterial.CameraDeltaList[i].UpThreshhold;
            //                BaslerRepository.BaslerCamerasCollection[j].Deltas = BaslerRepository.CurrentMaterial.CameraDeltaList[i].Deltas;
            //                BaslerRepository.BaslerCamerasCollection[j].MultipleDeltas = BaslerRepository.CurrentMaterial.CameraDeltaList[i].MultipleDeltas;
                           
            //                break;
            //            }
            //        }
            //    }
            //}

            NonControlZonesRepository.AddZones(BaslerRepository);
            float shift = 0;
            OneCameraContent Camera1V = ContainerProvider.Resolve<OneCameraContent>();
            OneCameraContentViewModel Camera1VM = ContainerProvider.Resolve<OneCameraContentViewModel>();
            Camera1V.DataContext = Camera1VM;
            if (Camera1VM != null)
            {
                Camera1VM.CurrentCamera = BaslerRepository.BaslerCamerasCollection[0];
                IRegion currentRegion = RegionManager.Regions[RegionNames.Camera1Region];
                Camera1VM.Shift = shift;
                currentRegion.Add(Camera1V);
                currentRegion.Activate(Camera1V);
            }

            OneCameraContent Camera2V = ContainerProvider.Resolve<OneCameraContent>();
            OneCameraContentViewModel Camera2VM = ContainerProvider.Resolve<OneCameraContentViewModel>();
            Camera2V.DataContext = Camera2VM;
            if (Camera2VM != null)
            {
                shift += 6144 * BaslerRepository.BaslerCamerasCollection[0].WidthDescrete;
                Camera2VM.CurrentCamera = BaslerRepository.BaslerCamerasCollection[1];
                IRegion currentRegion = RegionManager.Regions[RegionNames.Camera2Region];
                Camera2VM.Shift = shift;
                currentRegion.Add(Camera2V);
                currentRegion.Activate(Camera2V);
            }

            OneCameraContent Camera3V = ContainerProvider.Resolve<OneCameraContent>();
            OneCameraContentViewModel Camera3VM = ContainerProvider.Resolve<OneCameraContentViewModel>();
            Camera3V.DataContext = Camera3VM;
            if (Camera3V != null)
            {
                shift += 6144 * BaslerRepository.BaslerCamerasCollection[1].WidthDescrete;
                Camera3VM.CurrentCamera = BaslerRepository.BaslerCamerasCollection[2];
                IRegion currentRegion = RegionManager.Regions[RegionNames.Camera3Region];
                Camera3VM.Shift = shift;
                currentRegion.Add(Camera3V);
                currentRegion.Activate(Camera3V);
            }
        }
    }
}
