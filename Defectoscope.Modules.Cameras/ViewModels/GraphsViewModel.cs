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

        public GraphsViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IBaslerRepository baslerRepository) : base(regionManager)
        {
            _containerProvider = containerProvider;
            _baslerRepository = baslerRepository;
            // Graphs = new();

            Graph firstGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel fGraphViewModel = new GraphViewModel(_baslerRepository.BaslerCamerasCollection[0]);
            firstGraph.DataContext = fGraphViewModel;
            Graphs.Add(firstGraph);

            Graph secondGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel sGraphViewModel = new GraphViewModel(_baslerRepository.BaslerCamerasCollection[1]);
            secondGraph.DataContext = sGraphViewModel;
            Graphs.Add(secondGraph);
            Graph thirdGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel tGraphViewModel = new GraphViewModel(_baslerRepository.BaslerCamerasCollection[2]);
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
