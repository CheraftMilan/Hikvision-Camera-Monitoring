using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Entities
{
    public class Room
    {
        public string Name { get; set; }
        public List<Camera> Cameras { get; set; }
    }
}
