using System.Windows;

using Defectoscope.Modules.Cameras;
using Defectoscope.Modules.Cameras.ViewModels;
using Defectoscope.Modules.Cameras.Views;

using Kogerent.Core;
using Kogerent.LaserScan.Views;
using Kogerent.Logger;
using Kogerent.Services.Implementation;
using Kogerent.Services.Interfaces;

using LaserScan.Core.NetStandart.Services;
using LaserScan.Views;

using Prism.Ioc;
using Prism.Modularity;

namespace LaserScan
{
    /// <summary>
    /// Логика взаимодействия с разметкой App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// Создает окно из контейнера
        /// </summary>
        /// <returns>Возвращает окно</returns>
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        /// <summary>
        /// Регистрация сервисов на уровне всего приложения
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            _ = containerRegistry.RegisterSingleton<ILogger, OFFLogger>();
            _ = containerRegistry.RegisterSingleton<ISensorRepository, SensorRepository>();
            _ = containerRegistry.RegisterSingleton<IFooterRepository, FooterRepository>();
            _ = containerRegistry.Register<ISensorService, SensorService>();
            _ = containerRegistry.RegisterSingleton<IXmlService, XmlService>();
            // _ = containerRegistry.RegisterSingleton<ILaserscanUIService, LaserscanUIService>();
            _ = containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
            _ = containerRegistry.RegisterSingleton<IMathService, MathService>();
            _ = containerRegistry.RegisterSingleton<IDefectRepository, DefectRepository>();
            _ = containerRegistry.RegisterSingleton<ICallbackPool, CallbackPool>();
            _ = containerRegistry.RegisterSingleton<IProcessingService, ProcessingService>();
            _ = containerRegistry.RegisterSingleton<INonControlZonesRepository, NonControlZonesRepository>();
            _ = containerRegistry.RegisterSingleton<IDiscretizeService, DiscretizeService>();
            _ = containerRegistry.RegisterSingleton<IPointService, PointService>();
            _ = containerRegistry.RegisterSingleton<IImageProcessingService, ImageProcessingService>();
            _ = containerRegistry.RegisterSingleton<INetworkService, NetworkService>();
            _ = containerRegistry.RegisterSingleton<INumericService, NumericService>();
            _ = containerRegistry.RegisterSingleton<IDataService, SqliteDataService>();
            _ = containerRegistry.RegisterSingleton<IArchiveRepository, ArchiveRepository>();
            _ = containerRegistry.RegisterSingleton<ISynchronizer, Synchronizer>();
            _ = containerRegistry.RegisterSingleton<IBaslerRepository, BaslerRepository>();
            _ = containerRegistry.RegisterSingleton<IBaslerService, BaslerService>();
            _ = containerRegistry.RegisterSingleton<IBenchmarkRepository, BenchmarkRepository>();
            _ = containerRegistry.RegisterSingleton<ICalibrateService, CalibrateService>();
            containerRegistry.RegisterDialog<AddMaterialContext, AddMaterialContextViewModel>();
            containerRegistry.RegisterDialog<DeleteMaterialContext, DeleteMaterialContextViewModel>();
            containerRegistry.RegisterDialog<Graphs, GraphsViewModel>();
        }

        /// <summary>
        /// Добавляет необходимые модули в приложение
        /// </summary>
        /// <param name="moduleCatalog">Каталог, куда будут добавлены модули</param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<CamerasModule>();
        }

        /// <summary>
        /// Загружает форму с логин-паролем для входа
        /// </summary>
        protected override void OnInitialized()
        {
            var login = Container.Resolve<LoginWindow>();
            var result = login.ShowDialog();
            if (result.Value == true)
                base.OnInitialized();
            else
                Current.Shutdown();
        }
    }
}
