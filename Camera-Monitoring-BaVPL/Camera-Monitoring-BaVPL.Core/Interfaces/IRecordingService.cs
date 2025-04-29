using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Interfaces
{
    public interface IRecordingService
    {
        event EventHandler RecordingStarted;
        event EventHandler RecordingStopped;

        string GetCurrentRecordingName();
        void StartRecording(string[] rtspUrls);
        void StopRecording();
    }
}
