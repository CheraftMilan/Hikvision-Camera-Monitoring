using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using Camera_Monitoring_BaVPL.Core.Entities;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using System.Net;
using System.Linq;
using Camera_Monitoring_BaVPL.Core.Settings;

namespace Camera_Monitoring_BaVPL.Core.Services
{
    public class CameraConfigService : ICameraConfigService
    {
        private readonly string configFilePath = AppSettings.CameraConfig;
        public event EventHandler CamerasUpdated;
        public List<Room> LoadRoomData()
        {
            if (!File.Exists(configFilePath))
            {
                return new List<Room>();
            }

            string jsonData = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<List<Room>>(jsonData);
        }
        protected virtual void OnCamerasUpdated()
        {
            CamerasUpdated?.Invoke(this, EventArgs.Empty);
        }
        public void SaveRoomData(List<Room> rooms)
        {
            string jsonData = JsonConvert.SerializeObject(rooms, Formatting.Indented);
            File.WriteAllText(configFilePath, jsonData);
            OnCamerasUpdated();
        }

        public void AddRoom(Room room)
        {
            var rooms = LoadRoomData();
            ValidateRoomName(room.Name, rooms);
            rooms.Add(room);
            SaveRoomData(rooms);
            
        }

        public void SaveCameraToRoom(string roomName, Camera camera, Camera existingCamera = null)
        {
            var rooms = LoadRoomData();
            var room = GetRoomByName(roomName, rooms);

            if (room == null)
            {
                throw new Exception("Kamer niet gevonden.");
            }

            ValidateCamera(camera, existingCamera, rooms);

            if (existingCamera != null)
            {
                UpdateExistingCamera(room, existingCamera, camera);
            }
            else
            {
                room.Cameras.Add(camera);
            }

            SaveRoomData(rooms);
        }

        public void DeleteRoom(string roomName)
        {
            var rooms = LoadRoomData();
            var roomToDelete = GetRoomByName(roomName, rooms);

            if (roomToDelete != null)
            {
                rooms.Remove(roomToDelete);
                SaveRoomData(rooms);
            }
        }

        public void DeleteCameraFromRoom(string roomName, string cameraIpAddress)
        {
            var rooms = LoadRoomData();
            var room = GetRoomByName(roomName, rooms);

            if (room != null)
            {
                var cameraToDelete = room.Cameras.FirstOrDefault(c => c.IpAddress == cameraIpAddress);
                if (cameraToDelete != null)
                {
                    room.Cameras.Remove(cameraToDelete);
                    SaveRoomData(rooms);
                }
            }
        }

        private Room GetRoomByName(string roomName, List<Room> rooms)
        {
            return rooms.FirstOrDefault(r => string.Equals(r.Name, roomName, System.StringComparison.OrdinalIgnoreCase));
        }

        private void ValidateRoomName(string roomName, List<Room> rooms)
        {
            if (rooms.Any(r => string.Equals(r.Name.Trim(), roomName.Trim(), System.StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Kamernaam is reeds in gebruik.");
            }
        }

        private void ValidateCamera(Camera camera, Camera existingCamera, List<Room> rooms)
        {
            if (!ValidateIpAddress(camera.IpAddress))
            {
                throw new Exception("Ongeldig IP-adres formaat.");
            }

            if (existingCamera == null || existingCamera.IpAddress != camera.IpAddress)
            {
                if (rooms.Any(r => r.Cameras.Any(c => c.IpAddress == camera.IpAddress)))
                {
                    throw new Exception("Dit IP-adres wordt al gebruikt door een andere camera.");
                }
            }
        }

        private void UpdateExistingCamera(Room room, Camera existingCamera, Camera camera)
        {
            var cameraInRoom = room.Cameras.FirstOrDefault(c => c.IpAddress == existingCamera.IpAddress);

            if (cameraInRoom != null)
            {
                cameraInRoom.Name = camera.Name;
                cameraInRoom.IpAddress = camera.IpAddress;
                cameraInRoom.Username = camera.Username;
                cameraInRoom.Password = camera.Password;
                cameraInRoom.CameraType = camera.CameraType;
            }
        }

        private bool ValidateIpAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }

        public void ExportJson(string filePath)
        {
            File.Copy(configFilePath, filePath, overwrite: true);
        }

        public void ImportJson(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Bestand niet gevonden.");
            }

            var importedRooms = JsonConvert.DeserializeObject<List<Room>>(File.ReadAllText(filePath));
            ValidateImportedData(importedRooms);
            SaveRoomData(importedRooms);
        }

        private void ValidateImportedData(List<Room> importedRooms)
        {
            var roomNames = new HashSet<string>();
            var importedCameraIps = new HashSet<string>();

            foreach (var importedRoom in importedRooms)
            {
                if (!roomNames.Add(importedRoom.Name))
                {
                    throw new Exception($"De kamer met naam '{importedRoom.Name}' bestaat meerdere keren in het importbestand.");
                }

                foreach (var camera in importedRoom.Cameras)
                {
                    if (!importedCameraIps.Add(camera.IpAddress))
                    {
                        throw new Exception($"Camera met IP '{camera.IpAddress}' bestaat meerdere keren in het importbestand.");
                    }

                    if (!ValidateIpAddress(camera.IpAddress))
                    {
                        throw new Exception($"Ongeldig IP-adres '{camera.IpAddress}' in kamer '{importedRoom.Name}'.");
                    }
                }
            }
        }
    }
}
