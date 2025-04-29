using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Entities
{
    public class Camera
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public CameraType CameraType { get; set; }
    }
}
