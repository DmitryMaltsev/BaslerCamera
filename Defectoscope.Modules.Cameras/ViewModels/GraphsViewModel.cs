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
using System.Windows.Threading;

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

        private ObservableCollection<CameraStatisticsData> _camerasStaticsData;
        public ObservableCollection<CameraStatisticsData> CamerasStatisticsData
        {
            get { return _camerasStaticsData; }
            set { SetProperty(ref _camerasStaticsData, value); }
        }
        private DispatcherTimer _timerForStats;

        public GraphsViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IBaslerRepository baslerRepository) : base(regionManager)
        {
            _containerProvider = containerProvider;
            _baslerRepository = baslerRepository;

            Graph firstGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel fGraphViewModel = new GraphViewModel(_baslerRepository.BaslerCamerasCollection[0]);
            firstGraph.DataContext = fGraphViewModel;
            Graph2 fnamedGraph = new Graph2() { Graph = firstGraph, Header = _baslerRepository.BaslerCamerasCollection[0].ID };
            Graphs.Add(fnamedGraph);

            Graph secondGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel secondGraphViewModel = new GraphViewModel(_baslerRepository.BaslerCamerasCollection[1]);
            secondGraph.DataContext = secondGraphViewModel;
            Graph2 snamedGraph = new() { Graph = secondGraph, Header = _baslerRepository.BaslerCamerasCollection[1].ID };
            Graphs.Add(snamedGraph);
            Graph thirdGraph = _containerProvider.Resolve<Graph>();
            GraphViewModel tGraphViewModel = new GraphViewModel(_baslerRepository.BaslerCamerasCollection[2]);
            thirdGraph.DataContext = tGraphViewModel;
            Graph2 tdnamedGraph = new Graph2() { Graph = thirdGraph, Header = _baslerRepository.BaslerCamerasCollection[2].ID };
            Graphs.Add(tdnamedGraph);


            CamerasStatisticsData = new();
            for (int i = 0; i < _baslerRepository.BaslerCamerasCollection.Count; i++)
            {
                CamerasStatisticsData.Add(new CameraStatisticsData()
                {
                    CameraName = _baslerRepository.BaslerCamerasCollection[i].ID,
                    TotalBufferCount = "total started",
                    FailedBufferCount = "failed started"
                });
            }
            _timerForStats = new() { Interval = TimeSpan.FromSeconds(1) };
            _timerForStats.Tick += Timer_Tick;
            _timerForStats.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {

            for (int i = 0; i < _baslerRepository.BaslerCamerasCollection.Count; i++)
            {
                CamerasStatisticsData[i].TotalBufferCount = _baslerRepository.BaslerCamerasCollection[i].GetTotalBufferCount();
                CamerasStatisticsData[i].FailedBufferCount = _baslerRepository.BaslerCamerasCollection[i].GetFailedBufferCount();
            }
        }

        #region Prism methods
        public void OnDialogClosed()
        {
            foreach (Graph2 graph in Graphs)
            {
                if (graph.Graph.DataContext as GraphViewModel != null)
                    (graph.Graph.DataContext as GraphViewModel).Dispose();
            }
            _timerForStats.Stop();
        }



        public void OnDialogOpened(IDialogParameters parameters)
        {


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
        #endregion


        public class CameraStatisticsData : BindableBase
        {
            private string _cameraName;
            public string CameraName
            {
                get { return _cameraName; }
                set { SetProperty(ref _cameraName, value); }
            }

            private string _totalBufferCount;
            public string TotalBufferCount
            {
                get { return _totalBufferCount; }
                set { SetProperty(ref _totalBufferCount, value); }
            }

            private string _failedBufferCount;
            public string FailedBufferCount
            {
                get { return _failedBufferCount; }
                set { SetProperty(ref _failedBufferCount, value); }
            }
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
