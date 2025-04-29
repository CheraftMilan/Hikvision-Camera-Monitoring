using Camera_Monitoring_BaVPL_Student.Core.Interfaces;
using Camera_Monitoring_BaVPL_Student.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Camera_Monitoring_BaVPL_Student
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Create the required service (or resolve it using a DI container)
            IAnnotationService annotationService = new AnnotationService(); // Or resolve from DI

            // Manually instantiate MainWindow with the required service
            var mainWindow = new MainWindow(annotationService);
            mainWindow.Show();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }
    }

}
