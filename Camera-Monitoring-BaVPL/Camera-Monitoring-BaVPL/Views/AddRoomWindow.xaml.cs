using Camera_Monitoring_BaVPL.Core.Entities;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using Camera_Monitoring_BaVPL.Core.Services;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Camera_Monitoring_BaVPL.Views
{
    /// <summary>
    /// Interaction logic for AddRoomWindow.xaml
    /// </summary>
    public partial class AddRoomWindow : Window
    {
        private ICameraConfigService _cameraConfigService;
        public bool IsEditing { get; set; }
        public string RoomName { get; private set; }

        public AddRoomWindow(ICameraConfigService cameraConfigService)
        {
            InitializeComponent();
            _cameraConfigService = cameraConfigService;
        }

        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            RoomName = txtRoomName.Text;

            if (string.IsNullOrWhiteSpace(RoomName))
            {
                MessageBox.Show("Kamernaam mag niet leeg zijn.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsEditing) 
            {
                try
                {
                    var newRoom = new Room { Name = RoomName, Cameras = new List<Camera>() };
                    _cameraConfigService.AddRoom(newRoom);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                DialogResult = true;
                Close();
            }
        }


    }
}
