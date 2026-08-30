using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.EditorTools
{
    public static class ObsidianLauncherSettingsProvider
    {
        public const string PREFERENCES_PATH = "Preferences/SoulsLike/Obsidian";

        private static bool? _isMcpPortReachable;

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider(PREFERENCES_PATH, SettingsScope.User)
            {
                label = "Obsidian",
                guiHandler = DrawSettings,
                keywords = new HashSet<string>
                {
                    "Obsidian",
                    "MCP",
                    "Executable"
                }
            };
        }

        private static void DrawSettings(string searchContext)
        {
            ObsidianLauncherSettings settings = ObsidianLauncherSettings.instance;
            string executablePath = EditorGUILayout.TextField("Obsidian Executable", settings.ObsidianExecutablePath);
            if (executablePath != settings.ObsidianExecutablePath)
            {
                settings.ObsidianExecutablePath = executablePath;
            }

            if (GUILayout.Button("Browse"))
            {
                string selectedPath = EditorUtility.OpenFilePanel(
                    "Select Obsidian Executable",
                    Path.GetDirectoryName(settings.ObsidianExecutablePath),
                    "exe");

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    settings.ObsidianExecutablePath = selectedPath;
                }
            }

            string mcpPortStatus = _isMcpPortReachable switch
            {
                true => "Reachable",
                false => "Not reachable",
                _ => "Not checked"
            };
            EditorGUILayout.LabelField("MCP Port Status", mcpPortStatus);

            if (GUILayout.Button("Refresh MCP Status"))
            {
                _isMcpPortReachable = ObsidianLauncher.IsMcpPortReachable();
            }

            if (GUILayout.Button("Start Obsidian"))
            {
                ObsidianLauncher.StartObsidian();
                _isMcpPortReachable = null;
            }
        }
    }
}
