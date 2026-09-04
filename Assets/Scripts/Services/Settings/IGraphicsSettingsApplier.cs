using System.Collections.Generic;

namespace SoulsLike.Services.Settings
{
    public interface IGraphicsSettingsApplier
    {
        SettingsCapabilities Capabilities { get; }
        IReadOnlyList<DisplayModeData> GetAvailableDisplayModes();
        GraphicsSettingsData GetCurrentSettings();
        void Apply(GraphicsSettingsData settings);
    }
}
