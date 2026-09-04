using System.Collections.Generic;
using SoulsLike.Services.CameraService;

namespace SoulsLike.Services.Settings
{
    public interface ISettingsService
    {
        GameSettingsData Current { get; }
        GameSettingsData Draft { get; }
        bool IsEditing { get; }
        bool HasUnsavedChanges { get; }
        SettingsCapabilities Capabilities { get; }
        IReadOnlyList<DisplayModeData> AvailableDisplayModes { get; }

        void BeginEdit();
        void Preview(SettingsSection section);
        void ResetSection(SettingsSection section);
        SettingsApplyResult Apply();
        void ConfirmPendingDisplayChange();
        void RevertPendingDisplayChange();
        void CancelEdit();
        void RegisterCameraService(ICameraService cameraService);
        void UnregisterCameraService(ICameraService cameraService);
    }
}
