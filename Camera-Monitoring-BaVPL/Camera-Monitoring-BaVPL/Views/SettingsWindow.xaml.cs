using Camera_Monitoring_BaVPL.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Camera_Monitoring_BaVPL.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private readonly ICameraConfigService _cameraConfigService;
        private readonly IEncryptionService _encryptionService;
        private readonly ISettingService _settingService;

        private double OriginalHeight;
        private double OriginalWidth;

        public SettingsWindow(ICameraConfigService cameraConfigService, IEncryptionService encryptionService, ISettingService settingService)
        {
            InitializeComponent();
            _cameraConfigService = cameraConfigService;
            _encryptionService = encryptionService;
            _settingService = settingService;
            var cameraConfig = new CameraConfigUserControl(_cameraConfigService, _encryptionService);
            var settings = new SettingsUserControl(_settingService, _cameraConfigService);

            OriginalWidth = Width;
            OriginalHeight = Height;


            MainTabControl.Items.Add(new TabItem { Header = "Instellingen", Content = settings });
            MainTabControl.Items.Add(new TabItem { Header = "Camera Configuratie", Content = cameraConfig });
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedIndex == 1) 
            {
                WindowState = WindowState.Normal;

                var cameraConfigControl = MainTabControl.SelectedItem as TabItem; 

                if (cameraConfigControl?.Content is CameraConfigUserControl cameraConfig) 
                {
                    cameraConfig.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    var desiredSize = cameraConfig.DesiredSize;

                    Height = desiredSize.Height + 50; 
                }
            }
            else if (MainTabControl.SelectedIndex == 0) 
            {
                Width = OriginalWidth;
                Height = OriginalHeight;
                WindowState = WindowState.Normal;
            }
        }
    }
}
