using Camera_Monitoring_BaVPL.Core.Entities;
using Camera_Monitoring_BaVPL.Core.Interfaces;
using Camera_Monitoring_BaVPL.Core.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera_Monitoring_BaVPL.Core.Services
{
    public class AnnotationService : IAnnotationService
    {
        private readonly string annotationFilePath = AppSettings.Annotations;
        public void CreateAnnotation(Annotation Annotation, string recordingName)
        {
            var recordings = GetAllRecordingsForAnnotations();
            var record = recordings.FirstOrDefault(r => r.Name == recordingName);
            record.Annotations.Add(Annotation);
            SaveRecordingData(recordings);
        }
        public void SaveRecordingData(List<Recording> recordings)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(recordings, Formatting.Indented);
                File.WriteAllText(annotationFilePath, jsonData);
            }
            catch
            {
                throw new FileNotFoundException("Bestand werd niet gevonden!");
            }
           
        }
        public void AddCommentToAnnotation(string recordingName,string comment, Annotation annotation)
        {
            var recordings = GetAllRecordingsForAnnotations();
            var record = recordings.FirstOrDefault(r => r.Name == recordingName);
            var recordAnnotation = record.Annotations.FirstOrDefault(a => a.Timestamp == annotation.Timestamp);
            recordAnnotation.Comment = comment;
            SaveRecordingData(recordings);

        }
       
        public List<Annotation> GetAnnotationsFromRecording(Recording recording)
        {
            var annotations = recording.Annotations;
            return annotations;
        }
        public void CreateRecordingForAnnotations(Recording recording)
        {
            recording.Annotations = new List<Annotation>();
            var recordings = GetAllRecordingsForAnnotations();
            if(!recordings.Any(r => r.Name == recording.Name))
            {
                recordings.Add(recording);
                SaveRecordingData(recordings);
            }
           
        }
        public List<Recording> GetAllRecordingsForAnnotations()
        {
            if (!File.Exists(annotationFilePath))
            {
                return new List<Recording>();
            }
            string jsonData = File.ReadAllText(annotationFilePath);
            return JsonConvert.DeserializeObject<List<Recording>>(jsonData);
        }
        public Recording GetRecordingForAnnotationsByRecordingName(string recordingName)
        {
            var recordings = GetAllRecordingsForAnnotations();
            var recording = recordings.FirstOrDefault(r => r.Name == recordingName );
            if(recording == null)
            {
                return null;
            }
            else
            {
                return recording;
            }
        }
        public void RemoveRecordingsFromAnnotationFile(List<Recording> recordings)
        {
            List<Recording> allRecordings;
            var json = File.ReadAllText(annotationFilePath);
            allRecordings = JsonConvert.DeserializeObject<List<Recording>>(json);

            foreach (var recording in recordings)
            {
                allRecordings.RemoveAll(r => r.Name == recording.Name);
            }

            var updatedJson = JsonConvert.SerializeObject(allRecordings, Formatting.Indented);

            File.WriteAllText(annotationFilePath, updatedJson);
        }
    }
}
