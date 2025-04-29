using Camera_Monitoring_BaVPL.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QuickNV.HikvisionNetSDK.Defines;
using static QuickNV.HikvisionNetSDK.Methods;

namespace Camera_Monitoring_BaVPL.Core.Services
{
    public class CameraMovementService : ICameraMovementService
    {
        private const int MovementSpeed = 6;
        private const int Channel = 1;
        private const int StartMovement = 0;
        private const int StopMovement = 1;

        public bool TiltUp(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, TILT_UP, StartMovement, MovementSpeed);
        }

        public bool TiltDown(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, TILT_DOWN, StartMovement, MovementSpeed);
        }

        public bool PanLeft(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, PAN_LEFT, StartMovement, MovementSpeed);
        }

        public bool PanRight(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, PAN_RIGHT, StartMovement, MovementSpeed);
        }

        public bool StopTiltUp(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, TILT_UP, StopMovement, MovementSpeed);
        }

        public bool StopTiltDown(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, TILT_DOWN, StopMovement, MovementSpeed);
        }

        public bool StopPanLeft(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, PAN_LEFT, StopMovement, MovementSpeed);
        }

        public bool StopPanRight(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, PAN_RIGHT, StopMovement, MovementSpeed);
        }

        public bool ZoomIn(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, ZOOM_IN, StartMovement, MovementSpeed);
        }

        public bool ZoomOut(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, ZOOM_OUT, StartMovement, MovementSpeed);
        }

        public bool StopZoomIn(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, ZOOM_IN, StopMovement, MovementSpeed);
        }

        public bool StopZoomOut(int userId)
        {
            return NET_DVR_PTZControlWithSpeed_Other(userId, Channel, ZOOM_OUT, StopMovement, MovementSpeed);
        }
    }
}
