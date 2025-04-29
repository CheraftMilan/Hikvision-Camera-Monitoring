using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Interfaces
{
    public interface ISettingService
    {
        List<string> GetLocationAndDaysToKeepString();
        void SaveLocationString(string location);
        void SaveDaysToKeepString(string daysToKeep);
        void DeleteFilesOverRetentionPeriod();
        void EncryptAndSaveCredentials();
        bool Login(string username, string password);
        void FirstTimeLocationString();
    }
}
