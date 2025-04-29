using Camera_Monitoring_BaVPL.Core.Interfaces;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Camera_Monitoring_BaVPL.Views
{
    public partial class SettingsUserControl : UserControl
    {
        private readonly ISettingService _settingService;
        private readonly ICameraConfigService _cameraConfigService;
        public SettingsUserControl()
        {
            InitializeComponent();
        }
        public SettingsUserControl(ISettingService settingService,ICameraConfigService cameraConfigService) : this()
        {
            _settingService = settingService;
            _cameraConfigService = cameraConfigService;
            LoadDownloadPath();
        }

        private void LoadDownloadPath()
        {
            List<string> settings = _settingService.GetLocationAndDaysToKeepString();
            txtFolderVid.Text = settings[0];
            txtRetentionPeriod.Text = settings[1];
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Folders|\n",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder",
            };
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string folderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                _settingService.SaveLocationString(folderPath);
                txtFolderVid.Text = folderPath;
            }
        }

        private void BtnSaveRetentionPeriod_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtRetentionPeriod.Text, out int retentionPeriod))
            {
                MessageBox.Show("Bewaartermijn opnames moet een nummer zijn", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (txtRetentionPeriod.Text == "0")
            {
                MessageBox.Show("Bewaartermijn opnames moet minstens 1 dag zijn", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            _settingService.SaveDaysToKeepString(txtRetentionPeriod.Text);
            MessageBox.Show("Bewaartermijn opnames is opgeslagen", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportCameras_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Select Location to Export Cameras"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _cameraConfigService.ExportJson(dialog.FileName);
                    MessageBox.Show("Cameraconfiguratie succesvol geëxporteerd.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout tijdens het exporteren: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnImportCameras_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Selecteer een bestand om camera's te importeren"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _cameraConfigService.ImportJson(dialog.FileName);
                    MessageBox.Show("Cameraconfiguratie succesvol geïmporteerd.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fout tijdens het importeren: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
