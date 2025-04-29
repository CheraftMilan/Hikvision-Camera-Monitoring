using Camera_Monitoring_BaVPL.Core.Entities;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using Camera_Monitoring_BaVPL.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Camera_Monitoring_BaVPL.Views
{
    public partial class CameraConfigUserControl : UserControl
    {
        private ICameraConfigService _cameraConfigService;
        private IEncryptionService _encryptionService;
        private List<Room> _rooms;
        public CameraConfigUserControl()
        {
            InitializeComponent();
        }
        public CameraConfigUserControl(ICameraConfigService cameraConfigService,IEncryptionService encryptionService) : this()
        {
            _cameraConfigService = cameraConfigService;
            _encryptionService = encryptionService;
            LoadRooms();
        }

        private void LoadRooms()
        {
            _rooms = _cameraConfigService.LoadRoomData();
            RoomsList.ItemsSource = _rooms;
        }

        private void DeleteCamera_Click(object sender, RoutedEventArgs e)
        {
            Camera selectedCamera = (Camera)(sender as Button).Tag;

            string roomName = _rooms.Find(r => r.Cameras.Contains(selectedCamera))?.Name;

            if (!string.IsNullOrEmpty(roomName))
            {
                MessageBoxResult result = MessageBox.Show($"Weet u zeker dat u de camera '{selectedCamera.Name}' wilt verwijderen uit '{roomName}'?",
                                                          "Bevestig verwijdering",
                                                          MessageBoxButton.YesNo,
                                                          MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _cameraConfigService.DeleteCameraFromRoom(roomName, selectedCamera.IpAddress);
                        LoadRooms();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Er is een fout opgetreden bij het verwijderen van de camera: {ex.Message}",
                                        "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private void AddRoom_Click(object sender, RoutedEventArgs e)
        {
            var addRoomWindow = new AddRoomWindow(_cameraConfigService);

            if (addRoomWindow.ShowDialog() == true)
            {
                var newRoom = new Room
                {
                    Name = addRoomWindow.RoomName,
                    Cameras = new List<Camera>()
                };

                _rooms.Add(newRoom);
                _cameraConfigService.SaveRoomData(_rooms);
                LoadRooms();
            }
        }

        private void EditRoom_Click(object sender, RoutedEventArgs e)
        {
            Room selectedRoom = (Room)(sender as Button).Tag;

   
            var editRoomWindow = new AddRoomWindow(_cameraConfigService)
            {
                Title = "Lokaal wijzigen",
                txtRoomName = { Text = selectedRoom.Name },
            };

            if (editRoomWindow.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(editRoomWindow.RoomName))
                {
                    MessageBox.Show("Kamernaam mag niet leeg zijn.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    selectedRoom.Name = editRoomWindow.RoomName;

                    try
                    {
                        _cameraConfigService.SaveRoomData(_rooms);
                        LoadRooms(); 
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Fout bij het opslaan van de kamer.", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }




        private void AddCamera_Click(object sender, RoutedEventArgs e)
        {
            string roomName = (sender as Button).Tag.ToString();
            var addCameraWindow = new AddCameraWindow(roomName, _cameraConfigService, _encryptionService);

            if (addCameraWindow.ShowDialog() == true)
            {
                LoadRooms();
            }
        }


        private void EditCamera_Click(object sender, RoutedEventArgs e)
        {
            Camera selectedCamera = (Camera)(sender as Button).Tag;

            string roomName = _rooms.FirstOrDefault(r => r.Cameras.Contains(selectedCamera))?.Name;


            var editCameraWindow = new AddCameraWindow(roomName, _cameraConfigService, _encryptionService, selectedCamera) { Title = "Camera wijzigen" };

            if (editCameraWindow.ShowDialog() == true)
            {
                LoadRooms();
            }
        }

        private void DeleteRoom_Click(object sender, RoutedEventArgs e)
        {
            string roomName = (string)(sender as Button).Tag;

            MessageBoxResult result = MessageBox.Show($"Weet u zeker dat u '{roomName}' en alle bijbehorende camera's wilt verwijderen?",
                                                      "Bevestig verwijdering",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _cameraConfigService.DeleteRoom(roomName);
                    LoadRooms();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Er is een fout opgetreden bij het verwijderen van de kamer: {ex.Message}",
                                    "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}