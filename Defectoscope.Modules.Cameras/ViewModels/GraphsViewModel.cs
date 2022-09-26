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
            GraphViewModel fGraphViewModel = new GraphViewModel("Первый");
            firstGraph.DataContext = fGraphViewModel;
            Graphs.Add(firstGraph);

            Graph secondGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel sGraphViewModel = new GraphViewModel("Второй");
            secondGraph.DataContext = sGraphViewModel;
            Graphs.Add(secondGraph);
            Graph thirdGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel tGraphViewModel = new GraphViewModel("Третий");
            thirdGraph.DataContext = tGraphViewModel;
            Graphs.Add(thirdGraph);
        }

        private object GraphViewModel(GraphViewModel graphViewModel)
        {
            throw new NotImplementedException();
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
        private ObservableCollection<Graph> _graphs = new();
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
