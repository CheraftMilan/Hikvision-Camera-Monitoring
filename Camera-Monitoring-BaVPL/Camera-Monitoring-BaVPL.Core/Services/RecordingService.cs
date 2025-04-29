using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Camera_Monitoring_BaVPL.Core.Settings;
using Camera_Monitoring_BaVPL.Core.Entities;
using System.Text;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Services
{
    public class RecordingService : IRecordingService
    {
        private readonly IAnnotationService _annotationService;
        private Process _ffmpegProcess;
        private Setting _accountSettings;

        public string _currentRecordingName;
        public string _currentRecordinTimeStamp;

        public event EventHandler RecordingStarted;
        public event EventHandler RecordingStopped;

        public RecordingService(IAnnotationService annotationService)
        {
            LoadAccountSettings();
            _annotationService = annotationService;
        }

        private void LoadAccountSettings()
        {
            if (File.Exists(AppSettings.AccountSettings))
            {
                var jsonContent = File.ReadAllText(AppSettings.AccountSettings);
                _accountSettings = JsonConvert.DeserializeObject<Setting>(jsonContent);
            }
            else
            {
                throw new FileNotFoundException("Het bestand met gebruikersinstellingen werd niet gevonden.");
            }
        }

        private string GetDownloadFolderPath()
        {
            string folderPath = _accountSettings.DownloadFolderPath;

            if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"De map '{folderPath}' bestaat niet.");
            }

            return folderPath;
        }

        private string GenerateUniqueFileName(string extension)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _currentRecordinTimeStamp = timestamp;
            var fileName = $"Opname_{timestamp}.{extension}";
            _currentRecordingName = fileName;
            return fileName;
        }

        public void StartRecording(string[] rtspUrls)
        {
            LoadAccountSettings();

            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                throw new InvalidOperationException("Er is al een opname bezig.");
            }

            if (rtspUrls.Length == 0)
            {
                throw new ArgumentException("Geen actieve camera's om op te nemen.");
            }

            string videoFileName = GenerateUniqueFileName("mkv");
            string folderPath = GetDownloadFolderPath();
            string videoOutputPath = Path.Combine(folderPath, videoFileName);

            StringBuilder ffmpegArgs = new StringBuilder();


            for (int i = 0; i < rtspUrls.Length; i++)
            {
                ffmpegArgs.Append($"-rtsp_transport tcp -i \"{rtspUrls[i]}\" ");
            }


            string filterComplex = GenerateFilterComplex(rtspUrls.Length);


            ffmpegArgs.Append($"-filter_complex \"{filterComplex}\" ");


            ffmpegArgs.Append("-fflags nobuffer -flags low_delay -strict experimental ");
            ffmpegArgs.Append("-buffer_size 100000k ");
            ffmpegArgs.Append("-b:v 4000k -crf 23 ");
            ffmpegArgs.Append("-err_detect ignore_err ");
            ffmpegArgs.Append("-c:v libx264 -preset ultrafast -tune zerolatency ");

 
            if (rtspUrls.Length > 1)
            {
                ffmpegArgs.Append($"-filter_complex \"amix=inputs={rtspUrls.Length}\" ");
            }
            else
            {
                ffmpegArgs.Append("-c:a aac -b:a 128k ");
            }

 
            ffmpegArgs.Append($"-f matroska \"{videoOutputPath}\"");

            var startInfo = new ProcessStartInfo
            {
                FileName = AppSettings.FFmpegPath,
                Arguments = ffmpegArgs.ToString(),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _ffmpegProcess = new Process { StartInfo = startInfo };
            _ffmpegProcess.OutputDataReceived += (sender, args) => Debug.WriteLine(args.Data);
            _ffmpegProcess.ErrorDataReceived += (sender, args) => Debug.WriteLine(args.Data);

            _ffmpegProcess.Start();
            _ffmpegProcess.BeginOutputReadLine();
            _ffmpegProcess.BeginErrorReadLine();

            CreateRecordingAnnotation();
            OnRecordingStarted();
        }


        private string GenerateFilterComplex(int numStreams)
        {
            if (numStreams == 1)
            {
                return "[0:v]scale=1920:1080"; 
            }
            else if (numStreams == 2)
            {
                return "[0:v]scale=960:540[v0];[1:v]scale=960:540[v1];[v0][v1]hstack=inputs=2";
            }
            else if (numStreams == 3)
            {
                return "[0:v][1:v][2:v]hstack=inputs=3";
            }
            else if (numStreams == 4)
            {
                return "[0:v]scale=960:540[v0];[1:v]scale=960:540[v1];[2:v]scale=960:540[v2];[3:v]scale=960:540[v3];[v0][v1]hstack=inputs=2[top];[v2][v3]hstack=inputs=2[bottom];[top][bottom]vstack=inputs=2";
            }

            return "";
        }


        public async void StopRecording()
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                Debug.WriteLine("Stopping recording gracefully by sending 'q'...");

                await _ffmpegProcess.StandardInput.WriteAsync("q");
                await _ffmpegProcess.WaitForExitAsync();

                _ffmpegProcess.Dispose();
                _ffmpegProcess = null;

                OnRecordingStopped();
            }
            else
            {
                throw new InvalidOperationException("Er is momenteel geen opname bezig.");
            }

            Debug.WriteLine("Recording stopped.");
        }

        private void CreateRecordingAnnotation()
        {
            var recording = new Recording()
            {
                Name = _currentRecordingName,
                Path = GetDownloadFolderPath(),
                StartTime = _currentRecordinTimeStamp,
            };
            _annotationService.CreateRecordingForAnnotations(recording);
        }

        public string GetCurrentRecordingName()
        {
            return _currentRecordingName;
        }

        protected virtual void OnRecordingStarted()
        {
            RecordingStarted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRecordingStopped()
        {
            RecordingStopped?.Invoke(this, EventArgs.Empty);
        }

    }
}
