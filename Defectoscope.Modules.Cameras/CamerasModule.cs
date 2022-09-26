using System;
using System.IO;

using Defectoscope.Modules.Cameras.ViewModels;
using Defectoscope.Modules.Cameras.Views;

using Kogerent.Core;
using Kogerent.Services.Interfaces;

using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Defectoscope.Modules.Cameras
{
    public class CamerasModule : IModule
    {
       
        public CamerasModule(IRegionManager regionManager, IXmlService xmlService, IBaslerRepository baslerRepository)
        {
            RegionManager = regionManager;
            XmlService = xmlService;
            BaslerRepository = baslerRepository;
        }

        public IRegionManager RegionManager { get; }
        public IXmlService XmlService { get; }
        public IBaslerRepository BaslerRepository { get; }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            RegionManager.RequestNavigate(RegionNames.RibbonRegion, RegionNames.CamerasRibbonKey);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CamerasRibbon, CamerasRibbonViewModel>(RegionNames.CamerasRibbonKey);
            containerRegistry.RegisterForNavigation<CamerasContent, CamerasContentViewModel>(RegionNames.CamerasContentKey);
        }
    }
}