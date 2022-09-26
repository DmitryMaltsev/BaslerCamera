using Defectoscope.Modules.Cameras.Views;

using Kogerent.Core;

using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Defectoscope.Modules.Cameras.ViewModels
{
    public class GraphsViewModel : RegionViewModelBase, IDialogAware
    {
        private readonly IContainerProvider _containerProvider;

        public GraphsViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
            _containerProvider = containerProvider;
            // Graphs = new();

            Graph firstGraph = _containerProvider.Resolve<Graph>();
            (firstGraph.DataContext as GraphViewModel).Text = "Первый";
            Graphs.Add(firstGraph);

            Graph secondGraph = _containerProvider.Resolve<Graph>();
            (secondGraph.DataContext as GraphViewModel).Text = "Второй";
            Graphs.Add(secondGraph);

            Graph thirdGraph = _containerProvider.Resolve<Graph>();
            (thirdGraph.DataContext as GraphViewModel).Text = "Третий";
            Graphs.Add(thirdGraph);
        }

        public string Title => "Графики сырых точек";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }
        private ObservableCollection<Graph> _graphs=new();
        public ObservableCollection<Graph> Graphs
        {
            get { return _graphs; }
            set { SetProperty(ref _graphs, value); }
        }


        public void OnDialogOpened(IDialogParameters parameters)
        {


        }
    }
}
