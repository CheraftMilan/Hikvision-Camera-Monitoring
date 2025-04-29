using Camera_Monitoring_BaVPL.Core.Entities;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Interfaces
{
    public interface ICameraViewService
    {
        Camera GetCameraByIpAddress(string ipAddress);
        IEnumerable<Room> GetRooms();
        void MuteCamera(MediaPlayer selectedMediaPlayer);
        void UnmuteCamera(MediaPlayer selectedMediaPlayer);
    }
}
