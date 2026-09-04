using SoulsLike.Services.Audio.Data;
using UnityEngine;

namespace SoulsLike.Services.Settings
{
    public static class SettingsDataUtility
    {
        public static GameSettingsData Copy(GameSettingsData source)
        {
            return new GameSettingsData
            {
                SchemaVersion = source.SchemaVersion,
                Audio = Copy(source.Audio),
                Camera = Copy(source.Camera),
                Graphics = Copy(source.Graphics),
                Controls = Copy(source.Controls)
            };
        }

        public static AudioSettingsData Copy(AudioSettingsData source)
        {
            return new AudioSettingsData
            {
                MasterVolume = source.MasterVolume,
                MusicVolume = source.MusicVolume,
                SfxVolume = source.SfxVolume,
                MuteAll = source.MuteAll
            };
        }

        public static CameraSettingsData Copy(CameraSettingsData source)
        {
            return new CameraSettingsData
            {
                Sensitivity = source.Sensitivity,
                InvertX = source.InvertX,
                InvertY = source.InvertY
            };
        }

        public static GraphicsSettingsData Copy(GraphicsSettingsData source)
        {
            return new GraphicsSettingsData
            {
                WindowMode = source.WindowMode,
                DisplayMode = Copy(source.DisplayMode),
                QualityLevelName = source.QualityLevelName
            };
        }

        public static DisplayModeData Copy(DisplayModeData source)
        {
            return new DisplayModeData
            {
                Width = source.Width,
                Height = source.Height,
                RefreshRateNumerator = source.RefreshRateNumerator,
                RefreshRateDenominator = source.RefreshRateDenominator
            };
        }

        public static ControlsSettingsData Copy(ControlsSettingsData source)
        {
            return new ControlsSettingsData
            {
                BindingOverridesJson = source.BindingOverridesJson
            };
        }

        public static bool AreEqual(GameSettingsData left, GameSettingsData right)
        {
            return left.SchemaVersion == right.SchemaVersion
                && AreEqual(left.Audio, right.Audio)
                && AreEqual(left.Camera, right.Camera)
                && AreEqual(left.Graphics, right.Graphics)
                && AreEqual(left.Controls, right.Controls);
        }

        public static bool AreEqual(AudioSettingsData left, AudioSettingsData right)
        {
            return Mathf.Approximately(left.MasterVolume, right.MasterVolume)
                && Mathf.Approximately(left.MusicVolume, right.MusicVolume)
                && Mathf.Approximately(left.SfxVolume, right.SfxVolume)
                && left.MuteAll == right.MuteAll;
        }

        public static bool AreEqual(CameraSettingsData left, CameraSettingsData right)
        {
            return Mathf.Approximately(left.Sensitivity, right.Sensitivity)
                && left.InvertX == right.InvertX
                && left.InvertY == right.InvertY;
        }

        public static bool AreEqual(GraphicsSettingsData left, GraphicsSettingsData right)
        {
            return left.WindowMode == right.WindowMode
                && AreEqual(left.DisplayMode, right.DisplayMode)
                && left.QualityLevelName == right.QualityLevelName;
        }

        public static bool AreEqual(DisplayModeData left, DisplayModeData right)
        {
            return left.Width == right.Width
                && left.Height == right.Height
                && left.RefreshRateNumerator == right.RefreshRateNumerator
                && left.RefreshRateDenominator == right.RefreshRateDenominator;
        }

        public static bool AreEqual(ControlsSettingsData left, ControlsSettingsData right)
        {
            return left.BindingOverridesJson == right.BindingOverridesJson;
        }
    }
}
