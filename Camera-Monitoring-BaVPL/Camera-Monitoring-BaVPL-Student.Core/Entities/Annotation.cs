using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL_Student.Core.Entities
{
    public class Annotation
    {
        public string Timestamp { get; set; }
        public string Comment { get; set; }
        public AnnotationType AnnotationType { get; set; }
    }
}
