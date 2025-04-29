using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Interfaces
{
    public interface ICameraMovementService
    {
        bool TiltUp(int userId);
        bool TiltDown(int userId);
        bool PanLeft(int userId);
        bool PanRight(int userId);
        bool ZoomIn(int userId);
        bool ZoomOut(int userId);
        bool StopZoomIn(int userId);
        bool StopZoomOut(int userId);
        bool StopTiltUp(int userId);
        bool StopTiltDown(int userId);
        bool StopPanLeft(int userId);
        bool StopPanRight(int userId);
    }
}
