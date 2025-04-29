using Camera_Monitoring_BaVPL.Core.Entities;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using Camera_Monitoring_BaVPL.Core.Services;
using Camera_Monitoring_BaVPL.Core.Settings;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Wpf.Ui.Controls;
using static QuickNV.HikvisionNetSDK.Defines;
using static QuickNV.HikvisionNetSDK.Methods;


namespace Camera_Monitoring_BaVPL.Views
{
    /// <summary>
    /// Interaction logic for LiveFeedWindow.xaml
    /// </summary>
    public partial class LiveFeedWindow : Window
    {
        private readonly ICameraViewService _cameraViewService;
        private readonly IEncryptionService _encryptionService;
        private readonly IRecordingService _recordingService;
        private readonly ICameraConfigService _cameraConfigService;
        private LibVLC _libVLC;
        private Dictionary<string, LibVLCSharp.Shared.MediaPlayer> _mediaPlayers = new Dictionary<string, LibVLCSharp.Shared.MediaPlayer>();
        private bool _isRecording;
        private List<CheckBox> _checkBoxes;
        private DispatcherTimer _timer;
        private TimeSpan _elapsedTime;


        public LiveFeedWindow(ICameraViewService cameraViewService, IEncryptionService encryptionService, IRecordingService recordingService, ICameraConfigService cameraConfigService)
        {
            InitializeComponent();
            DataContext = this;
            _cameraViewService = cameraViewService;
            _encryptionService = encryptionService;
            _cameraConfigService = cameraConfigService;
            LibVLCSharp.Shared.Core.Initialize();
            _libVLC = new LibVLC();
            LoadCameras();
            _recordingService = recordingService;
            btnStopRecording.IsEnabled = false;
            btnStartRecording.IsEnabled = true;
            _checkBoxes = new List<CheckBox>();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _cameraConfigService.CamerasUpdated += OnCamerasUpdated;
        }
        private void OnCamerasUpdated(object sender, EventArgs e)
        {
            LoadCameras();
        }
        private void LoadCameras()
        {
            var rooms = _cameraViewService.GetRooms();
            lstCameraItemsControl.ItemsSource = rooms;
            if (rooms != null)
            {
                foreach (var room in rooms)
                {
                    foreach (var camera in room.Cameras)
                    {
                        if (!_mediaPlayers.ContainsKey(camera.IpAddress))
                        {
                            _mediaPlayers[camera.IpAddress] = null;
                        }
                    }
                }
            }
        }
        private bool SetUpCamera(string ip)
        {
            var mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            var videoView = new VideoView
            {
                MediaPlayer = mediaPlayer,
                Width = 900,
                Height = 900,
                Background = System.Windows.Media.Brushes.Transparent
            };


            var selectCamera = _cameraViewService.GetCameraByIpAddress(ip);
            if (selectCamera == null)
            {
                return false;
            }
            var username = _encryptionService.Decrypt(selectCamera.Username);
            var password = _encryptionService.Decrypt(selectCamera.Password);
            var rtspUrl = $"rtsp://{username}:{password}@{selectCamera.IpAddress}:554/stream";
            if (!IsCameraActive(selectCamera.IpAddress, username, password))
            {
                return false;
            }
            else
            {
                grdCameraFootage.Children.Add(videoView);
                try
                {
                    var media = new Media(_libVLC, new Uri(rtspUrl));
                    media.AddOption(":network-caching=50");
                    media.AddOption(":clock-jitter=0");
                    media.AddOption(":clock-synchro=0");
                    mediaPlayer.Play(media);
                    _cameraViewService.MuteCamera(mediaPlayer);
                    _mediaPlayers[ip] = mediaPlayer;
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        private bool IsCameraActive(string ipAddress, string username, string password)
        {
            NET_DVR_DEVICEINFO_V30 deviceInfo = new NET_DVR_DEVICEINFO_V30();
            try
            {
                int userId = Invoke(NET_DVR_Login_V30(ipAddress, 8000, username, password, ref deviceInfo));

                if (userId >= 0)
                {
                    Invoke(NET_DVR_Logout(userId));
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
        private void RemoveCamera(string ip)
        {
            if (_mediaPlayers.TryGetValue(ip, out var mediaPlayer))
            {
                var videoView = FindVideoView(mediaPlayer);
                if (videoView != null)
                {
                    grdCameraFootage.Children.Remove(videoView);
                }
                if (mediaPlayer != null)
                {
                    mediaPlayer.Stop();
                    mediaPlayer.Dispose();
                    _mediaPlayers[ip] = null;
                }

            }
        }

        private VideoView FindVideoView(LibVLCSharp.Shared.MediaPlayer mediaPlayer)
        {
            foreach (var child in grdCameraFootage.Children)
            {
                if (child is VideoView view && view.MediaPlayer == mediaPlayer)
                {
                    return view;
                }
            }
            return null;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;

            if (checkBox != null)
            {
                int activeCameras = _mediaPlayers.Values.Count(mp => mp != null);

                if (activeCameras >= 4)
                {
                    System.Windows.MessageBox.Show("Je kunt niet meer dan 4 camera's tegelijkertijd selecteren op de live feed.", "Limiet Bereikt", System.Windows.MessageBoxButton.OK, MessageBoxImage.Warning);
                    checkBox.IsChecked = false;
                    return;
                }

                var ipAddress = checkBox.Tag.ToString();
                if (!SetUpCamera(ipAddress))
                {
                    System.Windows.MessageBox.Show("Er is iets foutgegaan tijdens het verbinden met de camera! Controleer of de camera actief is.", "Oops");
                    checkBox.IsChecked = false;
                }
            }
            if (!_checkBoxes.Contains(checkBox))
            {
                _checkBoxes.Add(checkBox);
            }
            UpdateGridLayout();
        }


        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                var ipAddress = checkBox.Tag.ToString();
                RemoveCamera(ipAddress);
            }
            UpdateGridLayout();
        }

        private void UpdateGridLayout()
        {
            int numberOfCameras = _mediaPlayers.Values.Count(mp => mp != null);

            int columns = (int)Math.Ceiling(Math.Sqrt(numberOfCameras));
            int rows = (int)Math.Ceiling((double)numberOfCameras / columns);
            if (rows <= 0) rows = 1;
            if (columns <= 0) columns = 1;

            grdCameraFootage.Rows = rows;
            grdCameraFootage.Columns = columns;

            foreach (var child in grdCameraFootage.Children)
            {
                if (child is VideoView view)
                {
                    view.Width = grdCameraFootage.ActualWidth / columns;
                    view.Height = grdCameraFootage.ActualHeight / rows;
                }
            }
        }

        private List<string> GetActiveCameraStreams()
        {
            List<string> rtspUrls = new List<string>();
            foreach (var mediaPlayer in _mediaPlayers.Values)
            {
                if (mediaPlayer != null && mediaPlayer.Media != null)
                {
                    rtspUrls.Add(mediaPlayer.Media.Mrl);
                }
            }
            return rtspUrls;
        }

        private void StartScreenRecording()
        {
            try
            {
                var rtspUrls = GetActiveCameraStreams();
                if (rtspUrls.Count == 0)
                {
                    System.Windows.MessageBox.Show("Er zijn geen actieve cameras om op te nemen.", "Waarschuwing", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                _recordingService.StartRecording(rtspUrls.ToArray());
                _isRecording = true;
                UpdateCameraTextColor();
                btnStartRecording.IsEnabled = false;
                btnStopRecording.IsEnabled = true;
                _elapsedTime = TimeSpan.Zero;
                _timer.Start();
                System.Windows.MessageBox.Show("Opname succesvol gestart.", "Opname", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                
            }
            catch (InvalidOperationException ex)
            {
                System.Windows.MessageBox.Show($"Kon opname niet starten: {ex.Message}", "Fout", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Er is een fout opgetreden tijdens het starten van de opname: {ex.Message}", "Fout", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }


        private void StopScreenRecording()
        {
            try
            {
                _recordingService.StopRecording();
                btnStartRecording.IsEnabled = true;
                btnStopRecording.IsEnabled = false;
                _isRecording = false;
                UpdateCameraTextColor();
                _timer.Stop();
                System.Windows.MessageBox.Show("Opname succesvol gestopt.", "Opname", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                btnStartRecording.Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
            {
                new SymbolIcon { Symbol = SymbolRegular.Record24 },
                new System.Windows.Controls.TextBlock { Text = "Start opname", Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center }
            }
                };
            }
            catch (InvalidOperationException ex)
            {
                System.Windows.MessageBox.Show($"Kon opname niet stoppen: {ex.Message}", "Fout", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Er is een fout opgetreden tijdens het stoppen van de opname: {ex.Message}", "Fout", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }


        private void StartRecording_Click(object sender, RoutedEventArgs e)
        {
            StartScreenRecording();
            
        }

        private void StopRecording_Click(object sender, RoutedEventArgs e)
        {
            StopScreenRecording();
            
        }
        private void UpdateCameraTextColor()
        {
            foreach (var checkbox in _checkBoxes)
            {
                if (checkbox.Content is System.Windows.Controls.TextBlock textBlock)
                {
                    if (_isRecording)
                    {
                        if (checkbox.IsChecked == true)
                        {
                            textBlock.Foreground = System.Windows.Media.Brushes.Red;
                        }
                    }
                    else
                    {
                        textBlock.Foreground = System.Windows.Media.Brushes.Black; 
                    }
                }
                else if (checkbox.Content is string)
                {
                    var textBlock2 = new System.Windows.Controls.TextBlock { Text = checkbox.Content.ToString() };
                    if (_isRecording)
                    {
                        if (checkbox.IsChecked == true)
                        {
                            textBlock2.Foreground = System.Windows.Media.Brushes.Red;
                        }
                    }
                    else
                    {
                        textBlock2.Foreground = System.Windows.Media.Brushes.Black;
                    }
                   
                    checkbox.Content = textBlock2;  
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            btnStartRecording.Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new SymbolIcon { Symbol = SymbolRegular.Record24 },
                    new System.Windows.Controls.TextBlock { Text = $"{_elapsedTime:mm\\:ss}", Margin = new Thickness(10, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center }
                }
            };
        }
    }
}
