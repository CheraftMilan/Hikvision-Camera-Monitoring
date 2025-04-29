using Camera_Monitoring_BaVPL_Student.Core.Entities;
using Camera_Monitoring_BaVPL_Student.Core.Interfaces;
using Camera_Monitoring_BaVPL_Student.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Camera_Monitoring_BaVPL_Student
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IAnnotationService _annotationService;

        private string _currentSimulationName;
        private Annotation _currentAnnotation;

        public MainWindow(IAnnotationService annotationService)
        {
            InitializeComponent();
            _annotationService = annotationService;
            btnStopClass.Visibility = Visibility.Collapsed;
            btnGood.Visibility = Visibility.Collapsed;
            btnNeutral.Visibility = Visibility.Collapsed;
            btnBad.Visibility = Visibility.Collapsed;
            txtFeedback.Visibility = Visibility.Collapsed;  
        }
        private void BtnAnnotationsWindow_Click(object sender, RoutedEventArgs e)
        {
            AnnotationsWindow annotationsWindow = new AnnotationsWindow(_annotationService);
            annotationsWindow.Show();
        }
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
           
            if (string.IsNullOrWhiteSpace(txtRecordingName.Text))
            {
                MessageBox.Show("Gelieve een naam in te vullen");
            }
            else if (string.IsNullOrWhiteSpace(txtRecordingDescription.Text))
            {
                MessageBox.Show("Gelieve de beschrijving in te vullen");
            }
            else
            {

                var simulation = new Simulation()
                {
                    Name = txtRecordingName.Text,
                    Description = txtRecordingDescription.Text,
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Annotations = new List<Annotation>()
                };
                var result = _annotationService.CreateSimulationForAnnotations(simulation.Name, simulation.Description, simulation.Timestamp);
                if (result)
                {
                    _currentSimulationName = simulation.Name;
                    btnStopClass.Visibility = Visibility.Visible;
                    btnStartClass.Visibility = Visibility.Collapsed;
                    txtRecordingDescription.IsEnabled = false;
                    txtRecordingName.IsEnabled = false;
                    btnGood.Visibility = Visibility.Visible;
                    btnNeutral.Visibility = Visibility.Visible;
                    btnBad.Visibility = Visibility.Visible;
                    txtFeedback.Visibility = Visibility.Visible;
                    btnDeleteSimulationsWindow.IsEnabled = false;
                    btnAnnotationsWindow.IsEnabled = false;
                }
                else
                {
                    MessageBox.Show("Er bestaat al een simulatie met deze naam");
                    return;
                }
            }
        }
        private void BtnDeleteSimulationsWindow_Click(object sender, RoutedEventArgs e)
        {
            DeleteSimulationsWindow deleteSimulationsWindow = new DeleteSimulationsWindow(_annotationService);
            deleteSimulationsWindow.Show();
        }
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _currentAnnotation = null;
            btnStartClass.Visibility = Visibility.Visible;
            btnStopClass.Visibility = Visibility.Collapsed;
            btnGood.Visibility = Visibility.Collapsed;
            btnNeutral.Visibility = Visibility.Collapsed;
            btnBad.Visibility = Visibility.Collapsed;
            txtFeedback.Visibility = Visibility.Collapsed;
            txtRecordingName.IsEnabled = true;
            txtRecordingDescription.IsEnabled = true;
            btnDeleteSimulationsWindow.IsEnabled = true;
            btnAnnotationsWindow.IsEnabled = true;
        }
        private void BtnNeutral_Click(object sender, RoutedEventArgs e)
        {
            var annotation = new Annotation()
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                AnnotationType = AnnotationType.Neutraal
            };
            _annotationService.CreateAnnotation(annotation, _currentSimulationName);
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
            _annotationService.CreateAnnotation(annotation, _currentSimulationName);
            _currentAnnotation = annotation;
            CommentPopup.IsOpen = true;
        }
        private void PopupOkButton_Click(object sender, RoutedEventArgs e)
        {
            var input = txtPopupComment.Text;
            if (string.IsNullOrEmpty(input))
            {
                input = null;
            }
            _annotationService.AddCommentToAnnotation(_currentSimulationName, input, _currentAnnotation);
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
            _annotationService.CreateAnnotation(annotation, _currentSimulationName);
            _currentAnnotation = annotation;
            CommentPopup.IsOpen = true;
        }
    }
}