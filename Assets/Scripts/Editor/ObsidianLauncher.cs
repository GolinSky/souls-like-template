using System.IO;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using Process = System.Diagnostics.Process;
using ProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace SoulsLike.EditorTools
{
    public static class ObsidianLauncher
    {
        private const string MCP_HOST = "127.0.0.1";
        private const int MCP_PORT = 27123;
        private const int MCP_CONNECT_TIMEOUT_MS = 250;
        private const double LAUNCH_COOLDOWN_SECONDS = 10d;
        private const string LAST_LAUNCH_TIME_KEY = "SoulsLike.ObsidianLauncher.LastLaunchTime";

        [MenuItem("Tools/SoulsLike/Obsidian/Start Obsidian")]
        public static void StartObsidian()
        {
            if (IsMcpPortReachable())
            {
                Debug.Log("[ObsidianLauncher] The Obsidian MCP port is already reachable.");
                return;
            }

            double currentTime = EditorApplication.timeSinceStartup;
            float lastLaunchTime = SessionState.GetFloat(LAST_LAUNCH_TIME_KEY, float.NegativeInfinity);
            if (currentTime - lastLaunchTime < LAUNCH_COOLDOWN_SECONDS)
            {
                Debug.Log("[ObsidianLauncher] An Obsidian launch was already requested. Waiting for the MCP port to open.");
                return;
            }

            string executablePath = ObsidianLauncherSettings.instance.ObsidianExecutablePath;
            if (!File.Exists(executablePath))
            {
                Debug.LogError($"[ObsidianLauncher] Obsidian executable was not found at '{executablePath}'. Configure it in Preferences/SoulsLike/Obsidian.");
                OpenSettings();
                return;
            }

            using (Process obsidianProcess = Process.Start(new ProcessStartInfo(executablePath)
            {
                UseShellExecute = true
            }))
            {
            }

            SessionState.SetFloat(LAST_LAUNCH_TIME_KEY, (float)currentTime);
        }

        [MenuItem("Tools/SoulsLike/Obsidian/Open Settings")]
        public static void OpenSettings()
        {
            SettingsService.OpenUserPreferences(ObsidianLauncherSettingsProvider.PREFERENCES_PATH);
        }

        public static bool IsMcpPortReachable()
        {
            using (var client = new TcpClient())
            {
                try
                {
                    var connectResult = client.BeginConnect(MCP_HOST, MCP_PORT, null, null);
                    using (var waitHandle = connectResult.AsyncWaitHandle)
                    {
                        if (!waitHandle.WaitOne(MCP_CONNECT_TIMEOUT_MS))
                        {
                            return false;
                        }
                    }

                    client.EndConnect(connectResult);
                    return client.Connected;
                }
                catch (SocketException)
                {
                    return false;
                }
            }
        }
    }
}
