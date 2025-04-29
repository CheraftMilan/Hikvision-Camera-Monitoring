using Camera_Monitoring_BaVPL.Core.Interfaces;
using Camera_Monitoring_BaVPL.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Camera_Monitoring_BaVPL
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<ICameraViewService, CameraViewService>();
            services.AddSingleton<ICameraMovementService, CameraMovementService>();
            services.AddSingleton<ISettingService, SettingService>();
            services.AddSingleton<ICameraConfigService, CameraConfigService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IAnnotationService, AnnotationService>();
            services.AddSingleton<IRecordingService, RecordingService>();

            // Register the MainWindow
            services.AddSingleton<MainWindow>();
        }
    }

}
