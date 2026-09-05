namespace SoulsLike.Services.Settings
{
    public readonly struct SettingsCapabilities
    {
        public readonly bool SupportsExclusiveFullscreen;
        public readonly bool SupportsRefreshRateSelection;

        public SettingsCapabilities(
            bool supportsExclusiveFullscreen,
            bool supportsRefreshRateSelection)
        {
            SupportsExclusiveFullscreen = supportsExclusiveFullscreen;
            SupportsRefreshRateSelection = supportsRefreshRateSelection;
        }
    }
}
