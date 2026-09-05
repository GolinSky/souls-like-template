using System.IO;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.EditorTools
{
    public static class SaveTools
    {
        [MenuItem("Tools/SoulsLike/Saves/Wipe All Saves", false, 100)]
        [MenuItem("SoulsLike/Saves/Wipe All Saves", false, 200)]
        public static void WipeAllSaves()
        {
            string path = Application.persistentDataPath;
            int deletedCount = 0;
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    File.Delete(file);
                    deletedCount++;
                }
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log($"[SaveTools] Wiped {deletedCount} save file(s) and cleared PlayerPrefs at {path}.");
        }
    }
}
