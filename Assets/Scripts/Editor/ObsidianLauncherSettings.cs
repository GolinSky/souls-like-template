using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.EditorTools
{
    [FilePath("UserSettings/ObsidianLauncherSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class ObsidianLauncherSettings : ScriptableSingleton<ObsidianLauncherSettings>
    {
        [SerializeField] private string obsidianExecutablePath;

        public string ObsidianExecutablePath
        {
            get => string.IsNullOrWhiteSpace(obsidianExecutablePath)
                ? GetDefaultObsidianExecutablePath()
                : obsidianExecutablePath;
            set
            {
                obsidianExecutablePath = value;
                Save(true);
            }
        }

        private static string GetDefaultObsidianExecutablePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Obsidian",
                "Obsidian.exe");
        }
    }
}
