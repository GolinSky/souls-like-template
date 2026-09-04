namespace SoulsLike.Ui.Settings
{
    public interface ISettingsPresenter
    {
        void SelectTab(SettingsTab tab);
        void OnOptionValueChanged(SettingsOptionId optionId, float value);
        void OnOptionValueChanged(SettingsOptionId optionId, bool value);
        void OnOptionAction(SettingsOptionId optionId);
        void Apply();
        void ResetCurrentSection();
        void Back();
        void KeepDisplaySettings();
        void RevertDisplaySettings();
        void ApplyUnsavedChanges();
        void DiscardUnsavedChanges();
        void ContinueEditing();
    }
}
