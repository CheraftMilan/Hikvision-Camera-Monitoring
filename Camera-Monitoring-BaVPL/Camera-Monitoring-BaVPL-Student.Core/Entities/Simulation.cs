using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL_Student.Core.Entities
{
    public class Simulation
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Timestamp { get; set; }
        public List<Annotation> Annotations { get; set; }
    }
}
