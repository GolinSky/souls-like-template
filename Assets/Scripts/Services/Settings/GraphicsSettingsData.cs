using System;
using UnityEngine;

namespace SoulsLike.Services.Settings
{
    [Serializable]
    public sealed class GraphicsSettingsData
    {
        public FullScreenMode WindowMode = FullScreenMode.FullScreenWindow;
        public DisplayModeData DisplayMode = new();
        public string QualityLevelName = string.Empty;
    }
}
