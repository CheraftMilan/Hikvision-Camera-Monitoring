using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Settings
{
    public static class AppSettings
    {
        public static readonly string CameraConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,@"..\..\..\..\Camera-Monitoring-BaVPL.Core\Settings\config.json");
        public static readonly string AccountSettings = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Camera-Monitoring-BaVPL.Core\Settings\accountSettings.json");
        public static readonly string Annotations = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Camera-Monitoring-BaVPL.Core\Settings\recordingAnnotations.json");
        public static readonly string FFmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\Camera-Monitoring-BaVPL.Core\Settings\ffmpeg.exe");
    }
}
