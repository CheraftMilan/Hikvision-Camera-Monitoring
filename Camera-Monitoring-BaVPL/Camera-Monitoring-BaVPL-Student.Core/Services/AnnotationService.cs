
using Camera_Monitoring_BaVPL_Student.Core.Entities;
using Camera_Monitoring_BaVPL_Student.Core.Interfaces;
using Camera_Monitoring_BaVPL_Student.Core.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Camera_Monitoring_BaVPL_Student.Core.Services
{
    public class AnnotationService : IAnnotationService
    {
        private readonly string _annotationPath = AppSettings.Annotations;

        public void CreateAnnotation(Annotation Annotation, string simulationName)
        {
            var simulations = GetAllSimulationsForAnnotations();
            var simulation = simulations.FirstOrDefault(r => r.Name == simulationName);
            simulation.Annotations.Add(Annotation);
            SaveSimulationData(simulations);
        }
        public void SaveSimulationData(List<Simulation> simulations)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(simulations, Formatting.Indented);
                File.WriteAllText(_annotationPath, jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while saving simulations: {ex.Message}");
            }
        }

        public void AddCommentToAnnotation(string simulationName, string comment, Annotation annotation)
        {
            var simulations = GetAllSimulationsForAnnotations();
            var simulationForComment = simulations.FirstOrDefault(r => r.Name == simulationName);
            var simulationAnnotation = simulationForComment.Annotations.FirstOrDefault(a => a.Timestamp == annotation.Timestamp);
            simulationAnnotation.Comment = comment;
            SaveSimulationData(simulations);
        }

        public List<Annotation> GetAnnotationsFromSimulation(Simulation simulation)
        {
            var annotations = simulation.Annotations;
            return annotations;
        }
        public bool CreateSimulationForAnnotations(string name, string description, string startTime)
        {
            var simulation = new Simulation
            {
                Name = name,
                Description = description,
                Timestamp = startTime,
                Annotations = new List<Annotation>()
            };
            var simulations = GetAllSimulationsForAnnotations();

            if (!simulations.Any(r => r.Name == simulation.Name))
            {
                simulations.Add(simulation);
                string jsonData = JsonConvert.SerializeObject(simulations, Formatting.Indented);
                File.WriteAllText(_annotationPath, jsonData);
                return true;
            }
            return false;
        }
        public List<Simulation> GetAllSimulationsForAnnotations()
        {
            if (!File.Exists(_annotationPath))
            {
                return new List<Simulation>();
            }
            string jsonData = File.ReadAllText(_annotationPath);
            var simulations = JsonConvert.DeserializeObject<List<Simulation>>(jsonData);

            
            simulations.Reverse();

            return simulations;
        }
        public Simulation GetSimulationForAnnotationsBySimulationName(string simulationName)
        {
            var simulations = GetAllSimulationsForAnnotations();
            var simulation = simulations.FirstOrDefault(r => r.Name == simulationName);
            if (simulation == null)
            {
                return null;
            }
            else
            {
                return simulation;
            }
        }

        public void DeleteSimulation(string simulationName)
        {
            var simulations = GetAllSimulationsForAnnotations();
            var simulationToDelete = GetSimulationForAnnotationsBySimulationName(simulationName);
            if (simulationToDelete != null)
            {
                simulations.RemoveAll(s => s.Name == simulationName);
                SaveSimulationData(simulations);
            }
        }
    }
}
