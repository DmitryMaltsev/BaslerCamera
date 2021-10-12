using Kogerent.Core;
using Kogerent.Services.Interfaces;
using Prism.Commands;
using Prism.Regions;
using LaserScan.Core.NetStandart.Models;
using System;
using System.IO;
using Prism.Services.Dialogs;
using Defectoscope.Modules.Cameras.Views;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class CamerasRibbonViewModel : RegionViewModelBase
    {
        public string SettingsDir => Directory.CreateDirectory($"{Environment.CurrentDirectory}\\Settings").FullName;

        #region Rising properties

        #endregion

        #region Delegate commands
        private DelegateCommand _destroyCommand;
        public DelegateCommand DestroyCommand => _destroyCommand ??= new DelegateCommand(ExecuteDestroyCommand);
        private DelegateCommand _saveCamersSettings;
        public DelegateCommand SaveCamerasSettings =>
            _saveCamersSettings ?? (_saveCamersSettings = new DelegateCommand(ExecuteSaveCamerasSettings));
        private DelegateCommand _checkCamerasOverLay;
        public DelegateCommand CheckCamerasOverLay =>
            _checkCamerasOverLay ?? (_checkCamerasOverLay = new DelegateCommand(ExecuteCheckCamerasOverlay));
        private DelegateCommand _addNewMaterialCommand;
        public DelegateCommand AddNewMaterialCommand =>
            _addNewMaterialCommand ?? (_addNewMaterialCommand = new DelegateCommand(ExecuteAddNewMaterialCommand));
        private DelegateCommand _deleteMaterialCommand;
        public DelegateCommand DeleteMaterialCommand =>
            _deleteMaterialCommand ?? (_deleteMaterialCommand = new DelegateCommand(ExecuteDeleteMaterialCommand));

        #endregion
        public IRegionManager RegionManager { get; }
        public IFooterRepository FooterRepository { get; }
        public IApplicationCommands ApplicationCommands { get; }
        public IBaslerRepository BaslerRepository { get; }
        public IXmlService XmlService { get; }
        public IBenchmarkRepository BenchmarkRepository { get; }
        public IDefectRepository DefectRepository { get; }
        public IDialogService DialogService { get; }

        public CamerasRibbonViewModel(IRegionManager regionManager, IFooterRepository footerRepository,
            IApplicationCommands applicationCommands, IBaslerRepository baslerRepository,
            IXmlService xmlService, IBenchmarkRepository benchmarkRepository, IDefectRepository defectRepository,
            IDialogService dialogService) : base(regionManager)
        {
            RegionManager = regionManager;
            FooterRepository = footerRepository;
            ApplicationCommands = applicationCommands;
            BaslerRepository = baslerRepository;
            XmlService = xmlService;
            BenchmarkRepository = benchmarkRepository;
            DefectRepository = defectRepository;
            DialogService = dialogService;
            ApplicationCommands.Destroy.RegisterCommand(DestroyCommand);
        }

        #region Execute methods delegates
        private void ExecuteSaveCamerasSettings()
        {
            string path = Path.Combine(SettingsDir, "BaslerSettings.xml");
            foreach (BaslerCameraModel cameraModel in BaslerRepository.BaslerCamerasCollection)
            {
                cameraModel.CanvasWidth = BaslerRepository.CanvasWidth;
            }
            XmlService.Write(path, BaslerRepository.BaslerCamerasCollection);
            string materialPath = Path.Combine(SettingsDir, "MaterialSettings.xml");
            if (BaslerRepository.CurrentMaterial.CameraDeltaList.Count > 0)
                BaslerRepository.CurrentMaterial.CameraDeltaList.Clear();
            foreach (var camera in BaslerRepository.BaslerCamerasCollection)
            {
                BaslerRepository.CurrentMaterial.CameraDeltaList.Add(new CameraDelta
                {
                    CameraId = camera.ID,
                    Deltas = camera.Deltas
                });
            }
            XmlService.Write(materialPath, BaslerRepository.MaterialModelCollection);
        }

        private void ExecuteDestroyCommand()
        {
            Destroy();
        }


        void ExecuteAddNewMaterialCommand()
        {
            DialogService.ShowDialog("AddMaterialContext");
        }

        void ExecuteDeleteMaterialCommand()
        {
            if (BaslerRepository.MaterialModelCollection.Count > 1)
            {
                DialogParameters p = new DialogParameters();
                p.Add("MaterialName", BaslerRepository.CurrentMaterial.MaterialName);
                DialogService.ShowDialog("DeleteMaterialContext", p, result => { });
            }
        }

        #endregion

        public override void Destroy()
        {
            ApplicationCommands.Destroy.UnregisterCommand(DestroyCommand);
            base.Destroy();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            FooterRepository.Title = "Module1";
            RegionManager.RequestNavigate(RegionNames.ContentRegion, RegionNames.CamerasContentKey);
        }
    }
}
