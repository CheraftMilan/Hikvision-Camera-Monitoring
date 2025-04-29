using Camera_Monitoring_BaVPL.Core.Entities;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using LibVLCSharp.Shared;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Vlc.DotNet.Core.Interops.Signatures;
using Wpf.Ui.Controls;

namespace Camera_Monitoring_BaVPL.Views
{
    /// <summary>
    /// Interaction logic for WatchRecordingWindow.xaml
    /// </summary>
    public partial class WatchRecordingWindow : Window
    {
        private LibVLC _libVLC;
        private LibVLCSharp.Shared.MediaPlayer _mediaPlayer;
        private readonly IAnnotationService _annotationService;
        private static CheckBox _selectedCheckBox = null;
        private Recording _selectedRecording = null;
        private bool _isDragging = false;
        public WatchRecordingWindow(IAnnotationService annotationService)
        {
            InitializeComponent();
            LibVLCSharp.Shared.Core.Initialize();
            _annotationService = annotationService;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Dispose();
            _libVLC.Dispose();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txbAnnotations.Visibility = Visibility.Hidden;
            var options = new[] { "--aout=directsound" };
            _libVLC = new LibVLC(options);
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            videoView.MediaPlayer = _mediaPlayer;
            var recordings = _annotationService.GetAllRecordingsForAnnotations();
            //check if recordings exsist in folder
            recordings = FilterRecordings(recordings);
            recordings.Reverse();
            lstRecordingsItemsControl.ItemsSource = recordings;
        }
        private List<Recording> FilterRecordings(List<Recording> recordings)
        {
            List<Recording> removedRecordings = new List<Recording>();
            recordings.RemoveAll(recording =>
            {
                bool exists = System.IO.File.Exists(System.IO.Path.Combine(recording.Path, recording.Name));
                if (!exists)
                {
                    removedRecordings.Add(recording);
                }
                return !exists;
            });
            _annotationService.RemoveRecordingsFromAnnotationFile(removedRecordings);
            return recordings;
        }
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GetAnnotationForRecording(Recording recording)
        {
            var annotations = _annotationService.GetAnnotationsFromRecording(recording);
            foreach (var annotation in annotations)
            {
                if (annotation.AnnotationType == Core.Entities.AnnotationType.Goed)
                {
                    System.Windows.Controls.Button goodButton = new System.Windows.Controls.Button
                    {
                        Background = Brushes.Green,
                        Height = 40,
                        Width = 160,
                        Margin = new Thickness(0, 10, 0, 0),
                        Tag = annotation
                    };
                    SymbolIcon thumbsUp = new SymbolIcon
                    {
                        Symbol = SymbolRegular.ThumbLike48
                    };
                    goodButton.Content = thumbsUp;
                    goodButton.Click += AnnotationButton_Click;
                    stpButtons.Children.Add(goodButton);
                }
                else if (annotation.AnnotationType == Core.Entities.AnnotationType.Slecht)
                {
                    System.Windows.Controls.Button badButton = new System.Windows.Controls.Button
                    {
                        Background = Brushes.Red,
                        Height = 40,
                        Width = 160,
                        Margin = new Thickness(0, 10, 0, 0),
                        Tag = annotation
                    };
                    SymbolIcon thumbsDown = new SymbolIcon
                    {
                        Symbol = SymbolRegular.ThumbDislike24
                    };
                    badButton.Content = thumbsDown;
                    badButton.Click += AnnotationButton_Click;
                    stpButtons.Children.Add(badButton);
                }
                else
                {
                    System.Windows.Controls.Button neutralButton = new System.Windows.Controls.Button
                    {
                        Background = (Brush)new BrushConverter().ConvertFromString("#FFA500"),
                        Height = 40,
                        Width = 160,
                        Margin = new Thickness(0, 10, 0, 0),
                        Tag = annotation
                    };
                    SymbolIcon thumbsNeutral = new SymbolIcon
                    {
                        Symbol = SymbolRegular.QuestionCircle48
                    };
                    neutralButton.Content = thumbsNeutral;
                    neutralButton.Click += AnnotationButton_Click;
                    stpButtons.Children.Add(neutralButton);
                }
            }
        }
        private void AnnotationButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;

            var annotation = button?.Tag as Annotation;

            if (!string.IsNullOrWhiteSpace(annotation.Comment))
            {
                DateTime timestamp = DateTime.ParseExact(annotation.Timestamp, "yyyy-MM-dd HH:mm:ss", null);

                string formattedTimestamp = timestamp.ToString("dd/MM/yyyy - HH:mm:ss");

                txbAnnotations.Text = $"{formattedTimestamp}\n\n{annotation.Comment}";
            }
            else
            {
                txbAnnotations.Text = "Geen feedback";
            }


            DateTime startTime = DateTime.ParseExact(_selectedRecording.StartTime, "yyyyMMdd_HHmmss", null);
            var timeStamp = DateTime.Parse(annotation.Timestamp) - startTime;
            _mediaPlayer.Time = (long)timeStamp.TotalMilliseconds;
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_selectedCheckBox != null && _selectedCheckBox != sender)
            {
                _selectedCheckBox.IsChecked = false;
            }
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            videoView.MediaPlayer = _mediaPlayer;
            _selectedCheckBox = sender as CheckBox;
            _selectedRecording = _selectedCheckBox.Tag as Recording;
            GetAnnotationForRecording(_selectedRecording);
            var recordingPath = System.IO.Path.Combine(_selectedRecording.Path, _selectedRecording.Name);
            var media = new Media(_libVLC, new Uri(recordingPath));
            media.AddOption(":input-repeat=9999");
            _mediaPlayer.Play(media);
            _mediaPlayer.Mute = false;
            _mediaPlayer.Volume = 100;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_selectedCheckBox == sender)
            {
                _selectedCheckBox = null;
                stpButtons.Children.OfType<System.Windows.Controls.Button>().ToList().ForEach(button => stpButtons.Children.Remove(button));
                _selectedRecording = null;
                _mediaPlayer.Stop();
                txbAnnotations.Text = string.Empty;
            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
            }
            else
            {
                _mediaPlayer.Play();
            }

        }

        private void BtnGoBack_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Time -= 5000;
        }

        private void BtnFastForward_Click(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Time += 5000;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    BtnPause_Click(null, null);
                    break;
                case Key.Left:
                    BtnGoBack_Click(null, null);
                    break;
                case Key.Right:
                    BtnFastForward_Click(null, null);
                    break;
            }
        }

        private void CkbRecording_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void BtnGoToTime_Click(object sender, RoutedEventArgs e)
        {
            if(_selectedRecording != null)
            {
                DateTime startTime = DateTime.ParseExact(_selectedRecording.StartTime, "yyyyMMdd_HHmmss", null);
                txtPopupTime.Text = startTime.ToString("HH:mm:ss");
                TimePopup.IsOpen = true;
            }
            else
            {
                System.Windows.MessageBox.Show("Selecteer een video om naar een specifieke tijd te gaan.");
            }
        }

        private void BtnCancelTime_Click(object sender, RoutedEventArgs e)
        {
            TimePopup.IsOpen = false;
        }

        private void BtnAcceptTime_Click(object sender, RoutedEventArgs e)
        {
            if(IsValidTime(txtPopupTime.Text))
            {
                DateTime startTime = DateTime.ParseExact(_selectedRecording.StartTime, "yyyyMMdd_HHmmss", null);
                DateTime time = DateTime.Parse(txtPopupTime.Text);
                var timeStamp = time - startTime;
                _mediaPlayer.Time = (long)timeStamp.TotalMilliseconds;
                TimePopup.IsOpen = false;
            }
            else
            {
                System.Windows.MessageBox.Show("Ongeldige tijd formaat. Gebruik HH:mm:ss");
            }
        }
        private bool IsValidTime(string input)
        {
            string timePattern = @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$";
            return Regex.IsMatch(input, timePattern);
        }

        private void BtnHideAnnotation_Click(object sender, RoutedEventArgs e)
        {
            if(txbAnnotations.Visibility == Visibility.Visible)
            {
                btnHideAnnotation.Content = "Toon annotaties";
                txbAnnotations.Visibility = Visibility.Hidden;
            }
            else
            {
                btnHideAnnotation.Content = "Verberg annotaties";
                txbAnnotations.Visibility = Visibility.Visible;
            }
        }
    }
}
