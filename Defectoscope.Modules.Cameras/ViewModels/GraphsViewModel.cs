using Defectoscope.Modules.Cameras.Views;

using Kogerent.Core;
using Kogerent.Services.Interfaces;

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
        private readonly IBaslerRepository _baslerRepository;

        private ObservableCollection<Graph2> _graphs = new();
        public ObservableCollection<Graph2> Graphs
        {
            get { return _graphs; }
            set { SetProperty(ref _graphs, value); }
        }

        public GraphsViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IBaslerRepository baslerRepository) : base(regionManager)
        {
            _containerProvider = containerProvider;
            _baslerRepository = baslerRepository;

            Graph firstGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel fGraphViewModel = new GraphViewModel(_baslerRepository.BaslerCamerasCollection[0]);
            firstGraph.DataContext = fGraphViewModel;
            Graph2 fnamedGraph = new Graph2() { Graph = firstGraph, Header = _baslerRepository.BaslerCamerasCollection[0].ID};
            Graphs.Add(fnamedGraph);

            Graph secondGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel secondGraphViewModel = new GraphViewModel(_baslerRepository.BaslerCamerasCollection[1]);
            secondGraph.DataContext = secondGraphViewModel;
            Graph2 snamedGraph= new Graph2() { Graph = secondGraph, Header = _baslerRepository.BaslerCamerasCollection[1].ID };
            Graphs.Add(snamedGraph);
            Graph thirdGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel tGraphViewModel = new GraphViewModel(_baslerRepository.BaslerCamerasCollection[2]);
            thirdGraph.DataContext = tGraphViewModel;
            Graph2 tdnamedGraph = new Graph2() { Graph = thirdGraph, Header = _baslerRepository.BaslerCamerasCollection[2].ID };
            Graphs.Add(tdnamedGraph);
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



        public void OnDialogOpened(IDialogParameters parameters)
        {


        }

        public class Graph2 : BindableBase
        {
            private Graph _graph = new();
            public Graph Graph
            {
                get { return _graph; }
                set { SetProperty(ref _graph, value); }
            }

            private string _header;
            public string Header
            {
                get { return _header; }
                set { SetProperty(ref _header, value); }
            }
        }
    }
}
