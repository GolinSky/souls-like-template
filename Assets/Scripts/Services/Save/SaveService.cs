using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using Steamworks;
#endif

namespace SoulsLike.Services.Save
{
    /// <summary>
    /// Persists JSON to local disk. When Steam is initialised and Cloud (UFS) is
    /// enabled for the app, also mirrors writes to Steam Remote Storage and
    /// prefers the cloud copy on load. If Steam isn't running, AppID is wrong,
    /// or UFS is off in the Steamworks portal, the Steam path is silently skipped
    /// and only the local file is used — no errors, no crashes.
    /// </summary>
    public interface ISaveService
    {
        bool Exists(string fileName);
        void Save<T>(string fileName, T data);
        T Load<T>(string fileName);
        void Delete(string fileName);
        void DeleteAll();
    }

    public class SaveService : ISaveService
    {
        private const string FILE_EXTENSION = ".json";

        // Master switch for Steam Cloud (Remote Storage / UFS). Set to false to
        // disable all cloud reads/writes and use only the local save file.
        private const bool ENABLE_STEAM_CLOUD = false;

        private readonly string _saveFolderPath;
        private readonly JsonSerializerSettings _jsonSettings;

        public SaveService()
        {
            _saveFolderPath = Application.persistentDataPath;

            _jsonSettings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            };
            _jsonSettings.Converters.Add(new Vector3Converter());
            _jsonSettings.Converters.Add(new ColorConverter());

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
            if (CloudFileExists(fileName))
                return true;

            return File.Exists(GetFilePath(fileName));
        }

        public void Save<T>(string fileName, T data)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented, _jsonSettings);

            // Always write local — disk is the source of truth when offline.
            if (!TryWriteLocal(fileName, json))
                return;

            // Mirror to Steam Cloud when available. Failure here is non-fatal.
            TryCloudWrite(fileName, json);
        }

        private bool TryWriteLocal(string fileName, string json)
        {
            string path = GetFilePath(fileName);
            try
            {
                // Creates the file if it doesn't exist, overwrites it if it does.
                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to write '{fileName}': {e.GetType().Name}: {e.Message}");
                return false;
            }
        }

        public T Load<T>(string fileName)
        {
            // Prefer cloud copy if present (cross-device authoritative).
            if (TryCloudRead(fileName, out string cloudJson))
                return Deserialize<T>(cloudJson, fileName);

            string filePath = GetFilePath(fileName);
            if (!File.Exists(filePath))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[SaveService] File not found: {filePath}");
#endif
                return default;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                return Deserialize<T>(json, fileName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to read '{fileName}': {e.Message}");
                return default;
            }
        }

        private T Deserialize<T>(string json, string fileName)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
            }
            catch (Exception e)
            {
                // Corrupt / partial JSON — don't crash the load path.
                Debug.LogError($"[SaveService] Corrupt save '{fileName}', returning default: {e.Message}");
                return default;
            }
        }

        public void Delete(string fileName)
        {
            try
            {
                string path = GetFilePath(fileName);
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to delete '{fileName}': {e.Message}");
            }

            TryCloudDelete(fileName);
        }

        public void DeleteAll()
        {
            try
            {
                if (Directory.Exists(_saveFolderPath))
                {
                    string[] files = Directory.GetFiles(_saveFolderPath, "*" + FILE_EXTENSION);
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Failed to delete all saves: {e.Message}");
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(_saveFolderPath, fileName + FILE_EXTENSION);
        }

        private static string GetCloudFileName(string fileName)
        {
            return fileName + FILE_EXTENSION;
        }

        // --- Steam Cloud (Remote Storage / UFS) -------------------------------
        // All helpers below are best-effort. They return false / do nothing if
        // Steam isn't initialised or the app doesn't have UFS configured.

        private static bool IsCloudAvailable()
        {
#if !DISABLESTEAMWORKS && STEAMWORKS_NET
            try
            {
                if (!ENABLE_STEAM_CLOUD || !SteamAPI.IsSteamRunning())
                    return false;

                // App-level toggle is set in the Steamworks portal (Cloud config).
                // User-level toggle is set by the player in Steam settings.
                return SteamRemoteStorage.IsCloudEnabledForApp()
                    && SteamRemoteStorage.IsCloudEnabledForAccount();
            }
            catch (Exception)
            {
                // Steamworks may throw if the native API isn't ready yet.
                return false;
            }
#else
            return false;
#endif
        }

        private static bool CloudFileExists(string fileName)
        {
#if !DISABLESTEAMWORKS && STEAMWORKS_NET
            if (!IsCloudAvailable())
                return false;

            try
            {
                return SteamRemoteStorage.FileExists(GetCloudFileName(fileName));
            }
            catch (Exception)
            {
                return false;
            }
#else
            return false;
#endif
        }

        private static void TryCloudWrite(string fileName, string json)
        {
#if !DISABLESTEAMWORKS && STEAMWORKS_NET
            if (!IsCloudAvailable())
                return;

            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                bool ok = SteamRemoteStorage.FileWrite(GetCloudFileName(fileName), bytes, bytes.Length);
#if UNITY_EDITOR
                if (!ok) Debug.LogWarning($"[SaveService] Steam Cloud write failed for '{fileName}'.");
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveService] Steam Cloud write threw: {e.Message}");
            }
#endif
        }

        private static void TryCloudDelete(string fileName)
        {
#if !DISABLESTEAMWORKS && STEAMWORKS_NET
            if (!IsCloudAvailable())
                return;

            try
            {
                string cloudName = GetCloudFileName(fileName);
                if (SteamRemoteStorage.FileExists(cloudName))
                    SteamRemoteStorage.FileDelete(cloudName);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveService] Steam Cloud delete threw: {e.Message}");
            }
#endif
        }

        private static bool TryCloudRead(string fileName, out string json)
        {
            json = null;
#if !DISABLESTEAMWORKS && STEAMWORKS_NET
            if (!IsCloudAvailable())
                return false;

            try
            {
                string cloudName = GetCloudFileName(fileName);
                if (!SteamRemoteStorage.FileExists(cloudName))
                    return false;

                int size = SteamRemoteStorage.GetFileSize(cloudName);
                if (size <= 0)
                    return false;

                byte[] buffer = new byte[size];
                int read = SteamRemoteStorage.FileRead(cloudName, buffer, size);
                if (read != size)
                    return false;

                json = Encoding.UTF8.GetString(buffer);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveService] Steam Cloud read threw: {e.Message}");
                return false;
            }
#else
            return false;
#endif
        }
    }
}
