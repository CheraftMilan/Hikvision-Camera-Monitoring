using Camera_Monitoring_BaVPL.Core.Entities;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using Camera_Monitoring_BaVPL.Core.Services;
using System;
using System.Windows;

namespace Camera_Monitoring_BaVPL.Views
{
    /// <summary>
    /// Interaction logic for AddCameraWindow.xaml
    /// </summary>
    public partial class AddCameraWindow : Window
    {
        private ICameraConfigService _cameraConfigService;
        private IEncryptionService _encryptionService;
        private string _roomName; 
        public bool IsEditing { get; private set; }
        public Camera NewCamera { get; private set; } 

        public AddCameraWindow(string roomName, ICameraConfigService cameraConfigService, IEncryptionService encryptionService, Camera cameraToEdit = null)
        {
            InitializeComponent();
            _cameraConfigService = cameraConfigService;
            _encryptionService = encryptionService;
            _roomName = roomName;

            foreach (var type in Enum.GetValues(typeof(CameraType)))
            {
                cmbCameraType.Items.Add(type);
            }

            if (cameraToEdit != null)
            {
                IsEditing = true;
                txtCameraName.Text = cameraToEdit.Name;
                txtIpAddress.Text = cameraToEdit.IpAddress;
                txtUsername.Text = _encryptionService.Decrypt(cameraToEdit.Username);
                txtPassword.Password = _encryptionService.Decrypt(cameraToEdit.Password);
                cmbCameraType.SelectedItem = cameraToEdit.CameraType;

                txtIpAddress.IsEnabled = false;
            }
            else
            {
                cmbCameraType.SelectedIndex = 0; 
            }
        }


        private void SaveCamera_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCameraName.Text) ||
                string.IsNullOrWhiteSpace(txtIpAddress.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Vul alstublieft alle velden in.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var camera = new Camera
            {
                Name = txtCameraName.Text,
                IpAddress = txtIpAddress.Text,
                Username = _encryptionService.Encrypt(txtUsername.Text),
                Password = _encryptionService.Encrypt(txtPassword.Password),
                CameraType = (CameraType)cmbCameraType.SelectedIndex
            };

            try
            {
                if (IsEditing)
                {
                    _cameraConfigService.SaveCameraToRoom(_roomName, camera, camera); 
                }
                else
                {
                    _cameraConfigService.SaveCameraToRoom(_roomName, camera);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
