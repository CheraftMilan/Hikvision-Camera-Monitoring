using Camera_Monitoring_BaVPL_Student.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL_Student.Core.Interfaces
{
    public interface IAnnotationService
    {
        void AddCommentToAnnotation(string simulationName, string comment, Annotation annotation);
        void CreateAnnotation(Annotation Annotation, string simulationName);
        bool CreateSimulationForAnnotations(string name, string description, string startTime);
        void DeleteSimulation(string simulationName);
        List<Simulation> GetAllSimulationsForAnnotations();
        List<Annotation> GetAnnotationsFromSimulation(Simulation simulation);
        Simulation GetSimulationForAnnotationsBySimulationName(string simulationName);
        void SaveSimulationData(List<Simulation> simulations);
    }
}
