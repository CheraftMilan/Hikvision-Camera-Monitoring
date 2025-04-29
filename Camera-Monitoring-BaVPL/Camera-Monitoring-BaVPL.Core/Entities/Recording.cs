using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Entities
{
    public class Recording
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public List<Annotation> Annotations { get; set; }
        public string FormattedName
        {
            get
            {
                var date = DateTime.ParseExact(StartTime, "yyyyMMdd_HHmmss", null);
                return $"Les {date.ToShortDateString()} {date.Hour:D2}:{date.Minute:D2}";
            }
        }
    }
}
