using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace MultiPlayerTemplate.Services.Save
{
    public class FileService 
    {
        private const string FILE_EXTENSION = ".json";
        
        private readonly string _saveFolderPath;

        public FileService()
        {
            _saveFolderPath = Application.persistentDataPath;

            if (!Directory.Exists(_saveFolderPath))
            {
                Directory.CreateDirectory(_saveFolderPath);
#if UNITY_EDITOR
                Debug.Log($"[SaveService] Created folder at {_saveFolderPath}");
#endif
            }
        }

        public bool Exists(string fileName)
        {
            string filePath = GetFilePath(fileName);
            return File.Exists(filePath);
        }

        public void Save<T>(string fileName, T data)
        {
            string filePath = GetFilePath(fileName);
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
#if UNITY_EDITOR
            Debug.Log($"[SaveService] Saved data to {filePath}");
#endif
        }

        public T Load<T>(string fileName)
        {
            string filePath = GetFilePath(fileName);
            if (!File.Exists(filePath))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[SaveService] File not found: {filePath}");
#endif
                return default;
            }

            var settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            };
            
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(_saveFolderPath, fileName + FILE_EXTENSION);
        }
    }
}