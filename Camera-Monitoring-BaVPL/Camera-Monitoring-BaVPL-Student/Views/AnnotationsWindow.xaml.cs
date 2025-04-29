using Camera_Monitoring_BaVPL_Student.Core.Interfaces;
using Camera_Monitoring_BaVPL_Student.Core.Entities;
using System;
using System.Collections.Generic;
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
using Wpf.Ui.Controls;

namespace Camera_Monitoring_BaVPL_Student.Views
{
    /// <summary>
    /// Interaction logic for AnnotationsWindow.xaml
    /// </summary>
    public partial class AnnotationsWindow : Window
    {     

        private readonly IAnnotationService _annotationService;
        private static CheckBox _selectedCheckBox = null;
        private Simulation _selectedRecording = null;

        public AnnotationsWindow(IAnnotationService annotationService)
        {
            InitializeComponent();
            _annotationService = annotationService;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var simulations = _annotationService.GetAllSimulationsForAnnotations();
            lstSimulationsItemsControl.ItemsSource = simulations;
        }

        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private string GetDateTime(string timestamp)
        {
            DateTime dateTime = DateTime.Parse(timestamp);
            return dateTime.ToString("T");
        }
        private void GetAnnotationForRecording(string simulationName)
        {
            var simulation = _annotationService.GetSimulationForAnnotationsBySimulationName(simulationName);
            foreach (var annotation in simulation.Annotations)
            {
                System.Windows.Controls.Button annotationButton = new System.Windows.Controls.Button
                {
                    Height = 40,
                    Width = 160,
                    Margin = new Thickness(0, 10, 0, 0),
                    Tag = annotation
                };

                StackPanel buttonContent = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                SymbolIcon icon;

                if (annotation.AnnotationType == AnnotationType.Goed)
                {
                    annotationButton.Background = Brushes.Green;
                    icon = new SymbolIcon { Symbol = SymbolRegular.ThumbLike48 };
                }
                else if (annotation.AnnotationType == AnnotationType.Slecht)
                {
                    annotationButton.Background = Brushes.Red;
                    icon = new SymbolIcon { Symbol = SymbolRegular.ThumbDislike24 };
                }
                else 
                {
                    annotationButton.Background = (Brush)new BrushConverter().ConvertFromString("#FFA500");
                    icon = new SymbolIcon { Symbol = SymbolRegular.QuestionCircle48 };
                }

                buttonContent.Children.Add(icon);

                System.Windows.Controls.TextBlock timestampTextBlock = new System.Windows.Controls.TextBlock
                {
                    Text = GetDateTime(annotation.Timestamp), 
                    Margin = new Thickness(10, 0, 0, 0), 
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White 
                };

                buttonContent.Children.Add(timestampTextBlock);

                annotationButton.Content = buttonContent;

                annotationButton.Click += AnnotationButton_Click;

                stpButtons.Children.Add(annotationButton);
            }
        }

        private void AnnotationButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;

            var annotation = button?.Tag as Annotation;

            if (string.IsNullOrWhiteSpace(annotation.Comment) == false)
            {
                txtAnnotationTime.Text = GetDateTime(annotation.Timestamp);
                txbAnnotations.Text = annotation.Comment;
            }
            else
            {
                DateTime dateTime = DateTime.Parse(annotation.Timestamp);
                txtAnnotationTime.Text = dateTime.ToString("T");
                txbAnnotations.Text = "Geen feedback beschikbaar";
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_selectedCheckBox != null && _selectedCheckBox != sender)
            {
                _selectedCheckBox.IsChecked = false;
            }

            _selectedCheckBox = sender as CheckBox;
            _selectedRecording = _selectedCheckBox.Tag as Simulation;
            GetAnnotationForRecording(_selectedRecording.Name);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_selectedCheckBox == sender)
            {
                _selectedCheckBox = null;
                stpButtons.Children.OfType<System.Windows.Controls.Button>().ToList().ForEach(button => stpButtons.Children.Remove(button));
                _selectedRecording = null;
                txbAnnotations.Text = string.Empty;
                txtAnnotationTime.Text = string.Empty;
            }
        }

    }
}
