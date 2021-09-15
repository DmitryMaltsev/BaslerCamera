using Kogerent.Core;
using Kogerent.Services.Interfaces;

using Prism.Commands;
using Prism.Regions;

namespace LaserScan.ViewModels
{
    public class MainWindowViewModel : RegionViewModelBase
    {
        private string _title = "Дефектоскоп";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private DelegateCommand<string> _navigate;
        public DelegateCommand<string> Navigate => _navigate ??= new DelegateCommand<string>(ExecuteNavigate);

        public IFooterRepository FooterRepository { get; }
        public IApplicationCommands ApplicationCommands { get; }
        public IProcessingService ProcessingService { get; }

        public MainWindowViewModel(IFooterRepository footerRepository, IRegionManager regionManager,
                                   IApplicationCommands applicationCommands) : base(regionManager)
        {
            FooterRepository = footerRepository;
            ApplicationCommands = applicationCommands;
            FooterRepository.HeaderText = "Program started!";
            FooterRepository.Text = "Log is here...";
            FooterRepository.Title = Title;
        }

        #region Methods

        void ExecuteNavigate(string parameter)
        {
            if (parameter is null) return;
            RegionManager.RequestNavigate(RegionNames.RibbonRegion, parameter);
        }
        #endregion
    }
}
