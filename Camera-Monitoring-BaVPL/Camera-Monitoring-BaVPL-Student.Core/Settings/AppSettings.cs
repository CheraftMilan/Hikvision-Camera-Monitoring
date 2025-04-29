using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL_Student.Core.Settings
{
    public static class AppSettings
    {
        public static readonly string Annotations = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Camera-Monitoring-BaVPL-Student.Core\Settings\simulationAnnotations.json");

    }
}
