using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace SoulsLike.Services.Storage
{
    public class StorageRegistry : IStorageRegistry
    {
        private readonly string _basePath;

        public StorageRegistry()
        {
            _basePath = Application.persistentDataPath;
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public void SaveData<T>(string key, T data)
        {
            try
            {
                string filePath = GetFilePath(key);
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StorageRegistry] Failed to save data for key '{key}': {ex.Message}");
            }
        }

        public void SaveData<T>(Enum key, T data) => SaveData(key.ToString(), data);

        public T GetData<T>(string key, T defaultValue = default)
        {
            try
            {
                string filePath = GetFilePath(key);
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StorageRegistry] Failed to load data for key '{key}': {ex.Message}");
            }
            
            return defaultValue;
        }

        public T GetData<T>(Enum key, T defaultValue = default) => GetData<T>(key.ToString(), defaultValue);

        public bool HasData(string key)
        {
            return File.Exists(GetFilePath(key));
        }

        public bool HasData(Enum key) => HasData(key.ToString());

        public void DeleteData(string key)
        {
            try
            {
                string filePath = GetFilePath(key);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[StorageRegistry] Failed to delete data for key '{key}': {ex.Message}");
            }
        }

        public void DeleteData(Enum key) => DeleteData(key.ToString());

        private string GetFilePath(string key)
        {
            // Sanitize key to prevent invalid characters in file name if needed
            string safeKey = string.Join("_", key.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_basePath, $"{safeKey}.json");
        }
    }
}
