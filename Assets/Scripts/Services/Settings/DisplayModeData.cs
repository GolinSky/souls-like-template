using System;

namespace SoulsLike.Services.Settings
{
    [Serializable]
    public sealed class DisplayModeData
    {
        public int Width;
        public int Height;
        public uint RefreshRateNumerator;
        public uint RefreshRateDenominator;
    }
}
