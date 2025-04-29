using Camera_Monitoring_BaVPL.Core.Entities;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using Camera_Monitoring_BaVPL.Views;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static QuickNV.HikvisionNetSDK.Defines;
using static QuickNV.HikvisionNetSDK.Methods;


namespace Camera_Monitoring_BaVPL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ICameraViewService _cameraViewService;
        private readonly ICameraMovementService _cameraMovementService;
        private readonly ICameraConfigService _cameraConfigService;
        private readonly ISettingService _settingService;
        private readonly IEncryptionService _encryptionService;
        private readonly IAnnotationService _annotationService;
        private readonly IRecordingService _recordingService;
        private int userId;
        public Camera SelectedCamera;
        private List<Camera> DisplayedCameras;
        private CheckBox CheckBoxJustClicked = new();
        private LibVLC _libVLC;
        private bool isLogedIn = false;
        private Dictionary<string, LibVLCSharp.Shared.MediaPlayer> _mediaPlayers;
        private string _currentRecordingName;
        private Annotation _currentAnnotation;
        public bool IsRecording { get; private set; }
        public MainWindow(ICameraMovementService cameraMovementService, ICameraViewService cameraViewService, ICameraConfigService cameraConfigService, ISettingService settingService, IEncryptionService encryptionService, IAnnotationService annotationService, IRecordingService recordingService)
        {
            InitializeComponent();
            DataContext = this;
            _mediaPlayers = new Dictionary<string, LibVLCSharp.Shared.MediaPlayer>();
            _cameraViewService = cameraViewService;
            _cameraConfigService = cameraConfigService;
            var options = new[] { "--aout=directsound" };
            LibVLCSharp.Shared.Core.Initialize();
            _libVLC = new LibVLC(options);
            _cameraMovementService = cameraMovementService;
            cmbDisplayedCameras.ItemsSource = DisplayedCameras;
            DisplayedCameras = new List<Camera>();
            LoadCameras();
            NET_DVR_Init();
            _settingService = settingService;
            _settingService.FirstTimeLocationString();
            _encryptionService = encryptionService;
            HideAllControls();
            _settingService.EncryptAndSaveCredentials();
            _annotationService = annotationService;
            _recordingService = recordingService;
            _recordingService.RecordingStarted += OnRecordingStarted;
            _recordingService.RecordingStopped += OnRecordingStopped;
            _cameraConfigService.CamerasUpdated += OnCamerasUpdated;
            annotationsPanel.Visibility = Visibility.Collapsed;
        }
        private void OnRecordingStarted(object sender, EventArgs e)
        {
            IsRecording = true;
            annotationsPanel.Visibility = Visibility.Visible;
            _currentRecordingName = _recordingService.GetCurrentRecordingName();
        }

        private void OnCamerasUpdated(object sender, EventArgs e)
        {
            LoadCameras();
        }

        private void OnRecordingStopped(object sender, EventArgs e)
        {
            IsRecording = false;
           annotationsPanel.Visibility = Visibility.Collapsed;

        }
        private void BtnNeutral_Click(object sender, RoutedEventArgs e)
        {
            var annotation = new Annotation()
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                AnnotationType = AnnotationType.Neutraal
            };
            _annotationService.CreateAnnotation(annotation, _currentRecordingName);
            _currentAnnotation = annotation;
            CommentPopup.IsOpen = true;
        }
        private void BtnBad_Click(object sender, RoutedEventArgs e)
        {
            var annotation = new Annotation()
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                AnnotationType = AnnotationType.Slecht
            };
            _annotationService.CreateAnnotation(annotation, _currentRecordingName);
            _currentAnnotation = annotation;
            CommentPopup.IsOpen = true;
        }
        private void PopupOkButton_Click(object sender, RoutedEventArgs e)
        {
            var input = txtPopupComment.Text;
            if(string.IsNullOrEmpty(input))
            {
                input = null;
            }
            _annotationService.AddCommentToAnnotation(_currentRecordingName, input, _currentAnnotation);
            txtPopupComment.Text = "";
            CommentPopup.IsOpen = false;
        }

        private void BtnGood_Click(object sender, RoutedEventArgs e)
        {
            var annotation = new Annotation()
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                AnnotationType = AnnotationType.Goed
            };
            _annotationService.CreateAnnotation(annotation, _currentRecordingName);
            _currentAnnotation = annotation;
            CommentPopup.IsOpen = true;
        }
        private void DeleteFilesOverRetentionPeriod()
        {
            string locationFolder = _settingService.GetLocationAndDaysToKeepString()[0];
            MessageBoxResult result = MessageBox.Show($"Mogen we de opnames die over de bewaartijd zijn verwijderen? \n (in {locationFolder})", "Opgelet", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.OK)
            {
                try
                {
                    _settingService.DeleteFilesOverRetentionPeriod();
                    MessageBox.Show("De opnames zijn succesvol verwijderd.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show("Er is een onbekende fout opgetreden.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
                var mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
                var videoView = new VideoView
                {
                    MediaPlayer = mediaPlayer,
                    Width = 900,
                    Height = 900,
                    Background = Brushes.Transparent
                };
                grdCameraFootage.Children.Add(videoView);
                try
                {
                    var media = new Media(_libVLC, new Uri(rtspUrl));
                    media.AddOption(":network-caching=50");
                    media.AddOption(":clock-jitter=0");
                    media.AddOption(":clock-synchro=0");
                    mediaPlayer.Play(media);
                    mediaPlayer.Volume = 100;
                    _cameraViewService.MuteCamera(mediaPlayer);
                    _mediaPlayers[ip] = mediaPlayer;
                    if (selectCamera != null && !DisplayedCameras.Contains(selectCamera))
                    {
                        DisplayedCameras.Add(selectCamera);

                    }
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
                    _mediaPlayers.Remove(ip);
                }
                var camera = DisplayedCameras.FirstOrDefault(c => c.IpAddress == ip);
                if (camera != null && DisplayedCameras.Contains(camera))
                {
                    DisplayedCameras.Remove(camera);
                    UpdateComboBox();
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
                var ipAddress = checkBox.Tag.ToString();
                if (!SetUpCamera(ipAddress))
                {
                    MessageBox.Show("Er is iets foutgegaan tijdens het verbinden met de camera! Controleer of de camera actief is.", "Oops");
                    checkBox.IsChecked = false;
                }
            }
            UpdateComboBox();
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

        private void BtnSettingsWindow_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(_cameraConfigService,_encryptionService,_settingService);
            settingsWindow.Show();
        }

        private void BtnLiveFeedWindow_Click(object sender, RoutedEventArgs e)
        {
            LiveFeedWindow liveFeedWindow = new LiveFeedWindow(_cameraViewService, _encryptionService, _recordingService, _cameraConfigService);
            liveFeedWindow.Show();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isLogedIn == true)
            {
                if (cmbDisplayedCameras.SelectedItem != null)
                {

                    cmbDisplayedCameras.IsEnabled = false;
                    switch (e.Key)
                    {
                        case Key.Right:
                            btnRight.Focus();
                            HandleMovement(_cameraMovementService.PanRight(userId));
                            break;
                        case Key.Left:
                            btnLeft.Focus();
                            HandleMovement(_cameraMovementService.PanLeft(userId));
                            break;
                        case Key.Up:
                            btnUp.Focus();
                            HandleMovement(_cameraMovementService.TiltUp(userId));
                            break;
                        case Key.Down:
                            btnDown.Focus();
                            HandleMovement(_cameraMovementService.TiltDown(userId));
                            break;
                        case Key.Add:
                            btnZoom.Focus();
                            HandleMovement(_cameraMovementService.ZoomIn(userId));
                            break;
                        case Key.Subtract:
                            btnZoomOut.Focus();
                            HandleMovement(_cameraMovementService.ZoomOut(userId));
                            break;
                    }
                }
            }
            else
            {
                if (e.Key == Key.Enter)
                {
                    BtnLogin_Click(sender, e);
                }
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (isLogedIn == true)
            {
                if (cmbDisplayedCameras.SelectedItem != null)
                {
                    switch (e.Key)
                    {
                        case Key.Right:
                            btnRight.Focus();
                            HandleMovement(_cameraMovementService.StopPanRight(userId));
                            break;
                        case Key.Left:
                            btnLeft.Focus();
                            HandleMovement(_cameraMovementService.StopPanLeft(userId));
                            break;
                        case Key.Up:
                            btnUp.Focus();
                            HandleMovement(_cameraMovementService.StopTiltUp(userId));
                            break;
                        case Key.Down:
                            btnDown.Focus();
                            HandleMovement(_cameraMovementService.StopTiltDown(userId));
                            break;
                        case Key.Add:
                            btnZoom.Focus();
                            HandleMovement(_cameraMovementService.StopZoomIn(userId));
                            break;
                        case Key.Subtract:
                            btnZoomOut.Focus();
                            HandleMovement(_cameraMovementService.StopZoomOut(userId));
                            break;
                    }
                    cmbDisplayedCameras.IsEnabled = true;
                }
            }
        }

        private void BtnLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.PanLeft(userId));
        }

        private void BtnRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.PanRight(userId));
        }

        private void BtnUp_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.TiltUp(userId));
        }

        private void BtnDown_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.TiltDown(userId));
        }

        private void BtnLeft_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.StopPanLeft(userId));
        }

        private void BtnRight_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.StopPanRight(userId));
        }

        private void BtnUp_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.StopTiltUp(userId));
        }

        private void BtnDown_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.StopTiltDown(userId));
        }

        private void HandleMovement(bool isSuccess)
        {
            if (!isSuccess)
            {
                MessageBox.Show("Kon camera niet bewegen.");
            }
        }

        private void BtnZoom_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.ZoomIn(userId));
        }

        private void BtnZoom_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.StopZoomIn(userId));
        }

        private void BtnZoomOut_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.ZoomOut(userId));
        }

        private void BtnZoomOut_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleMovement(_cameraMovementService.StopZoomOut(userId));
        }

        private void UpdateComboBox()
        {
            SelectedCamera = null;
            cmbDisplayedCameras.ItemsSource = null;
            cmbDisplayedCameras.ItemsSource = DisplayedCameras;
        }

        private void CmbDisplayedCameras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HideAllControls();
            SelectedCamera = cmbDisplayedCameras.SelectedItem as Camera;
            if (SelectedCamera != null)
            {
                var cameraType = SelectedCamera.CameraType;
                bool isVisible = cameraType == CameraType.Dome || cameraType == CameraType.Portable;

                lblMovement.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                btnLeft.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                btnRight.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                btnUp.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                btnDown.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                btnCircle.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                btnZoom.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
                btnZoomOut.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

                bool isStatic = cameraType == CameraType.Static;
                if (isStatic)
                {
                    CheckIfCameraIsMuted();
                }
                //btnUnmute.Visibility = isStatic ? Visibility.Visible : Visibility.Collapsed;
                //lblSound.Visibility = isStatic ? Visibility.Visible : Visibility.Collapsed;


                NET_DVR_DEVICEINFO_V30 deviceInfo = new NET_DVR_DEVICEINFO_V30();
                try
                {
                    var username = _encryptionService.Decrypt(SelectedCamera.Username);
                    var password = _encryptionService.Decrypt(SelectedCamera.Password);
                    userId = Invoke(NET_DVR_Login_V30(SelectedCamera.IpAddress, 8000, username, password, ref deviceInfo));
                }
                catch
                {
                    MessageBox.Show("De gebruikersnaam/wachtwoord van deze camera is niet correct!", "Ongeldige gegevens");
                }
            }
            else
            {
                HideAllControls();
            }
        }
        private void HideAllControls()
        {
            btnLeft.Visibility = Visibility.Collapsed;
            btnRight.Visibility = Visibility.Collapsed;
            btnUp.Visibility = Visibility.Collapsed;
            btnDown.Visibility = Visibility.Collapsed;
            btnZoom.Visibility = Visibility.Collapsed;
            btnZoomOut.Visibility = Visibility.Collapsed;
            btnUnmute.Visibility = Visibility.Collapsed;
            btnMute.Visibility = Visibility.Collapsed;
            lblSound.Visibility = Visibility.Collapsed;
            lblMovement.Visibility = Visibility.Collapsed;
            btnCircle.Visibility = Visibility.Collapsed;
        }
        private void CheckIfCameraIsMuted()
        {
            if (SelectedCamera != null)
            {
                if (_mediaPlayers[SelectedCamera.IpAddress].Mute == true)
                {
                    btnMute.Visibility = Visibility.Collapsed;
                    btnUnmute.Visibility = Visibility.Visible;
                }
                else
                {
                    btnUnmute.Visibility = Visibility.Collapsed;
                    btnMute.Visibility = Visibility.Visible;
                }
            }
        }
        private void BtnMute_Click(object sender, RoutedEventArgs e)
        {
            _cameraViewService.MuteCamera(_mediaPlayers[SelectedCamera.IpAddress]);
            CheckIfCameraIsMuted();
            //btnMute.Visibility = Visibility.Collapsed;
            //btnUnmute.Visibility = Visibility.Visible;
        }

        private void BtnUnmute_Click(object sender, RoutedEventArgs e)
        {
            _cameraViewService.UnmuteCamera(_mediaPlayers[SelectedCamera.IpAddress]);
            CheckIfCameraIsMuted();
            //btnUnmute.Visibility = Visibility.Collapsed;
            //btnMute.Visibility = Visibility.Visible;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                MessageBox.Show("Vul een gebruikersnaam in");
                return;
            }
            if (string.IsNullOrEmpty(txtPassword.Password))
            {
                MessageBox.Show("Vul een wachtwoord in");
                return;
            }
            bool isSuccess = _settingService.Login(txtUsername.Text, txtPassword.Password);
            if (isSuccess)
            {
                grdLoginOverlay.Visibility = Visibility.Collapsed;
                isLogedIn = true;
                DeleteFilesOverRetentionPeriod();
            }
            else
            {
                MessageBox.Show("Inloggen mislukt");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnRecordingWindow_Click(object sender, RoutedEventArgs e)
        {
            WatchRecordingWindow watchRecordingWindow = new WatchRecordingWindow(_annotationService);
            watchRecordingWindow.Show();

        }
    }

}
