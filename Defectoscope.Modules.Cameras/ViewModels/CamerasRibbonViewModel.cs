using Kogerent.Core;
using Kogerent.Services.Interfaces;
using Prism.Commands;
using Prism.Regions;
using LaserScan.Core.NetStandart.Models;
using System;
using System.IO;

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


        #endregion

        public IFooterRepository FooterRepository { get; }
        public IApplicationCommands ApplicationCommands { get; }
        public IBaslerRepository BaslerRepository { get; }
        public IXmlService XmlService { get; }
        public IBenchmarkRepository BenchmarkRepository { get; }
        public IDefectRepository DefectRepository { get; }

        public CamerasRibbonViewModel(IRegionManager regionManager, IFooterRepository footerRepository,
            IApplicationCommands applicationCommands, IBaslerRepository baslerRepository,
            IXmlService xmlService, IBenchmarkRepository benchmarkRepository, IDefectRepository defectRepository) : base(regionManager)
        {
            FooterRepository = footerRepository;
            ApplicationCommands = applicationCommands;
            BaslerRepository = baslerRepository;
            XmlService = xmlService;
            BenchmarkRepository = benchmarkRepository;
            DefectRepository = defectRepository;
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
        }

        private void ExecuteDestroyCommand()
        {
            Destroy();
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
