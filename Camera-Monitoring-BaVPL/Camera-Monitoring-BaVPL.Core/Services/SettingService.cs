using Camera_Monitoring_BaVPL.Core.Entities;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using Camera_Monitoring_BaVPL.Core.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Services
{
    public class SettingService : ISettingService
    {
        IEncryptionService _encryptionService;
        private Setting _settings;
        
        public SettingService(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
            _settings = LoadSettings();
        }
        private Setting LoadSettings()
        {
            var jsonData = File.ReadAllText(AppSettings.AccountSettings);
            return JsonConvert.DeserializeObject<Setting>(jsonData) ?? new Setting();
        }
        private void SaveSettings()
        {
            var jsonData = JsonConvert.SerializeObject(_settings);
            File.WriteAllText(AppSettings.AccountSettings, jsonData);
        }
        public List<string> GetLocationAndDaysToKeepString()
        {

            return new List<string>
            {
                _settings.DownloadFolderPath,
                _settings.DaysToKeep.ToString()
            };
        }
        public void EncryptAndSaveCredentials()
        {
            if (!IsEncrypted(_settings.Username))
            {
                _settings.Username = _encryptionService.Encrypt(_settings.Username);
            }
            if (!IsEncrypted(_settings.Password))
            {
                _settings.Password = _encryptionService.Encrypt(_settings.Password);
            }
            SaveSettings();
        }
        public bool Login(string username,string password)
        {
    
            var decryptedUsername = _encryptionService.Decrypt(_settings.Username);
            var decryptedPassword = _encryptionService.Decrypt(_settings.Password);
            return username == decryptedUsername && password == decryptedPassword;
        }
        private bool IsEncrypted(string input)
        {
            try
            {
                _encryptionService.Decrypt(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public void SaveLocationString(string location)
        {
            _settings.DownloadFolderPath = location;
            SaveSettings();
        }
        public void FirstTimeLocationString()
        {
            if (string.IsNullOrWhiteSpace(_settings.DownloadFolderPath))
            {
                GetMyVideosPath();
            }
        }
        public void GetMyVideosPath()
        {
            _settings.DownloadFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            SaveSettings();
        }
        public void SaveDaysToKeepString(string daysToKeep)
        {
            if (int.TryParse(daysToKeep, out int days))
            {
                _settings.DaysToKeep = days;
                SaveSettings();
            }
        }
        public void DeleteFilesOverRetentionPeriod()
        {
            var retentionPeriod = _settings.DaysToKeep;
            var downloadFolderPath = _settings.DownloadFolderPath;

            try
            {
                if (Directory.Exists(downloadFolderPath))
                {
                    var files = Directory.GetFiles(downloadFolderPath, "*.mkv");

                    foreach (var file in files)
                    {
                        var creationTime = File.GetCreationTime(file);

                        if (DateTime.Now - creationTime > TimeSpan.FromDays(retentionPeriod))
                        {
                            File.Delete(file);
                        }
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException($"De map '{downloadFolderPath}' bestaat niet.");
                }
            }
            catch (Exception)
            {
                if (!Directory.Exists(downloadFolderPath))
                {
                    GetMyVideosPath();
                    throw new InvalidOperationException($"De map bestaat niet is hij aangepast naar {Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)}");
                }
                throw new InvalidOperationException($"Er ging iets mis, er werd niks verwijderd");
            }
        }

    }
}
