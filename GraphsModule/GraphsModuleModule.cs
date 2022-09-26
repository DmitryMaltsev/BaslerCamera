using GraphsModule.ViewModels;
using GraphsModule.Views;

using Kogerent.Core;

using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace GraphsModule
{
    public class GraphsModuleModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<OneGraph, OneGraphViewModel>(RegionNames.Graph1Key);
            containerRegistry.RegisterForNavigation<OneGraph, OneGraphViewModel>(RegionNames.Graph2Key);
            containerRegistry.RegisterForNavigation<OneGraph, OneGraphViewModel>(RegionNames.Graph3Key);
        }
    }
}