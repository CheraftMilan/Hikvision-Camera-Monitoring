using Camera_Monitoring_BaVPL.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Interfaces
{
    public interface ICameraConfigService
    {
        event EventHandler CamerasUpdated;
        List<Room> LoadRoomData();
        void SaveRoomData(List<Room> rooms);
        void AddRoom(Room room);
        void SaveCameraToRoom(string roomName, Camera camera, Camera existingCamera = null);
        void DeleteRoom(string roomName);
        void DeleteCameraFromRoom(string roomName, string cameraIpAddress);
        void ExportJson(string filePath);
        void ImportJson(string filePath);
    }
}
