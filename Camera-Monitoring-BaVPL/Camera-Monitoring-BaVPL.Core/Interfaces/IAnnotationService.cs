using Camera_Monitoring_BaVPL.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Interfaces
{
    public interface IAnnotationService
    {

        void AddCommentToAnnotation(string recordingName, string comment, Annotation annotation);
        void CreateAnnotation(Annotation Annotation, string recordingName);
        void CreateRecordingForAnnotations(Recording recording);
        List<Recording> GetAllRecordingsForAnnotations();
        List<Annotation> GetAnnotationsFromRecording(Recording recording);
        Recording GetRecordingForAnnotationsByRecordingName(string recordingName);
        void SaveRecordingData(List<Recording> recordings);
        void RemoveRecordingsFromAnnotationFile(List<Recording> recordings);
    }
}
