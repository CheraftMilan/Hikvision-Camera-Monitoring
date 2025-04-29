using Camera_Monitoring_BaVPL.Core.Entities;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using Camera_Monitoring_BaVPL.Core.Settings;
using LibVLCSharp.Shared;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Camera_Monitoring_BaVPL.Core.Services
{
    public class CameraViewService : ICameraViewService
    {
        private List<Room> _rooms;

        public CameraViewService()
        {
            var jsonData = File.ReadAllText(AppSettings.CameraConfig);
            _rooms = JsonConvert.DeserializeObject<List<Room>>(jsonData);
        }
        
        public IEnumerable<Room> GetRooms()
        {
            var jsonData = File.ReadAllText(AppSettings.CameraConfig);
            _rooms = JsonConvert.DeserializeObject<List<Room>>(jsonData);
            return _rooms;
        }
        public Camera GetCameraByIpAddress(string ipAddress)
        {
            return _rooms.SelectMany(r => r.Cameras).FirstOrDefault(c => c.IpAddress == ipAddress);
        }
        public void MuteCamera(MediaPlayer selectedMediaPlayer)
        {
            selectedMediaPlayer.Mute = true;
        }
        public void UnmuteCamera(MediaPlayer selectedMediaPlayer)
        {
            selectedMediaPlayer.Mute = false;
        }
    }
}
