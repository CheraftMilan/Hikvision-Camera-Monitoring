using Camera_Monitoring_BaVPL_Student.Core.Interfaces;
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

namespace Camera_Monitoring_BaVPL_Student.Views
{
    /// <summary>
    /// Interaction logic for DeleteSimulationsWindow.xaml
    /// </summary>
    public partial class DeleteSimulationsWindow : Window
    {
        private readonly IAnnotationService _annotaionService;
        public DeleteSimulationsWindow(IAnnotationService annotationService)
        {
            InitializeComponent();
            _annotaionService = annotationService;
            LoadSimulations();
        }
        private void LoadSimulations()
        {
            var simulations = _annotaionService.GetAllSimulationsForAnnotations();
            lstSimulations.ItemsSource = null;
            if (simulations.Count > 0)
            {
                lstSimulations.ItemsSource = simulations;
            }
            else
            {
                lblNoSimulations.Content = "Er zijn geen simulaties om weer te geven.";
            }
        }
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void DeleteSimulation_Click(object sender, RoutedEventArgs e)
        {
            string simulationName = (string)(sender as Button).Tag;

            MessageBoxResult result = MessageBox.Show($"Weet u zeker dat u '{simulationName}' en alle bijbehorende annotaties wilt verwijderen?",
                                                      "Bevestig verwijdering",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _annotaionService.DeleteSimulation(simulationName);
                    LoadSimulations();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Er is een fout opgetreden bij het verwijderen van de simulatie: {ex.Message}",
                                    "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
