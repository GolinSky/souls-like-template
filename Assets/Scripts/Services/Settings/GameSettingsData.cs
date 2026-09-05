using System;
using SoulsLike.Services.Audio.Data;

namespace SoulsLike.Services.Settings
{
    [Serializable]
    public sealed class GameSettingsData
    {
        public int SchemaVersion = SettingsSchema.CURRENT_VERSION;
        public AudioSettingsData Audio = new();
        public CameraSettingsData Camera = new();
        public GraphicsSettingsData Graphics = new();
        public ControlsSettingsData Controls = new();
    }
}
