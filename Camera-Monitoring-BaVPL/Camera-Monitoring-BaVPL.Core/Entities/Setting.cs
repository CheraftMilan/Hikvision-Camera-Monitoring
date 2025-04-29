using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Entities
{
    public class Setting
    {
        public string DownloadFolderPath { get; set; }
        public int DaysToKeep { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
