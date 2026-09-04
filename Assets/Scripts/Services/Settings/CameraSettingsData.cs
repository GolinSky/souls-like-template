using System;

namespace SoulsLike.Services.Settings
{
    [Serializable]
    public sealed class CameraSettingsData
    {
        public float Sensitivity = 0.5f;
        public bool InvertX;
        public bool InvertY;
    }
}
