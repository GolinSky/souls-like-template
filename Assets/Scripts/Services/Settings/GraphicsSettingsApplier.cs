using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoulsLike.Services.Settings
{
    public sealed class GraphicsSettingsApplier : IGraphicsSettingsApplier
    {
        public SettingsCapabilities Capabilities => new(
            supportsExclusiveFullscreen: Application.platform == RuntimePlatform.WindowsPlayer
                || Application.platform == RuntimePlatform.LinuxPlayer,
            supportsRefreshRateSelection: GetAvailableDisplayModes().Count > 1);

        public IReadOnlyList<DisplayModeData> GetAvailableDisplayModes()
        {
            Resolution[] resolutions = Screen.resolutions;
            var modes = new List<DisplayModeData>(resolutions.Length);
            for (int index = 0; index < resolutions.Length; index++)
            {
                Resolution resolution = resolutions[index];
                var mode = new DisplayModeData
                {
                    Width = resolution.width,
                    Height = resolution.height,
                    RefreshRateNumerator = resolution.refreshRateRatio.numerator,
                    RefreshRateDenominator = resolution.refreshRateRatio.denominator
                };

                if (!Contains(modes, mode))
                {
                    modes.Add(mode);
                }
            }

            if (modes.Count == 0)
            {
                modes.Add(ToDisplayMode(Screen.currentResolution));
            }

            return modes;
        }

        public GraphicsSettingsData GetCurrentSettings()
        {
            return new GraphicsSettingsData
            {
                WindowMode = Screen.fullScreenMode,
                DisplayMode = ToDisplayMode(Screen.currentResolution),
                QualityLevelName = QualitySettings.names[QualitySettings.GetQualityLevel()]
            };
        }

        public void Apply(GraphicsSettingsData settings)
        {
            int qualityLevel = GetQualityLevel(settings.QualityLevelName);
            if (qualityLevel >= 0)
            {
                QualitySettings.SetQualityLevel(qualityLevel, true);
            }

            DisplayModeData displayMode = settings.DisplayMode;
            if (displayMode.RefreshRateDenominator == 0)
            {
                Screen.SetResolution(displayMode.Width, displayMode.Height, settings.WindowMode);
                return;
            }

            Screen.SetResolution(
                displayMode.Width,
                displayMode.Height,
                settings.WindowMode,
                new RefreshRate
                {
                    numerator = displayMode.RefreshRateNumerator,
                    denominator = displayMode.RefreshRateDenominator
                });
        }

        private static DisplayModeData ToDisplayMode(Resolution resolution)
        {
            return new DisplayModeData
            {
                Width = resolution.width,
                Height = resolution.height,
                RefreshRateNumerator = resolution.refreshRateRatio.numerator,
                RefreshRateDenominator = resolution.refreshRateRatio.denominator
            };
        }

        private static bool Contains(IReadOnlyList<DisplayModeData> modes, DisplayModeData candidate)
        {
            for (int index = 0; index < modes.Count; index++)
            {
                if (SettingsDataUtility.AreEqual(modes[index], candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetQualityLevel(string qualityLevelName)
        {
            string[] qualityNames = QualitySettings.names;
            for (int index = 0; index < qualityNames.Length; index++)
            {
                if (qualityNames[index] == qualityLevelName)
                {
                    return index;
                }
            }

            return -1;
        }
    }
}
