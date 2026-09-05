using System;
using System.Ui.Base;
using SoulsLike.Services.Settings;
using SoulsLike.Ui.Base;
using TMPro;
using UnityEngine;

namespace SoulsLike.Ui.Settings
{
    public sealed class SettingsUi : BaseUi
    {
        [SerializeField] private SettingsOptionUi[] options;
        [SerializeField] private CustomButton audioTabButton;
        [SerializeField] private CustomButton cameraTabButton;
        [SerializeField] private CustomButton graphicsTabButton;
        [SerializeField] private CustomButton controlsTabButton;
        [SerializeField] private CustomButton applyButton;
        [SerializeField] private CustomButton defaultsButton;
        [SerializeField] private CustomButton backButton;
        [SerializeField] private GameObject displayConfirmationPanel;
        [SerializeField] private TMP_Text displayConfirmationText;
        [SerializeField] private CustomButton keepDisplayButton;
        [SerializeField] private CustomButton revertDisplayButton;
        [SerializeField] private GameObject unsavedChangesPanel;
        [SerializeField] private CustomButton applyUnsavedButton;
        [SerializeField] private CustomButton discardUnsavedButton;
        [SerializeField] private CustomButton continueEditingButton;

        private ISettingsPresenter _presenter;

        public void AssignPresenter(ISettingsPresenter presenter)
        {
            _presenter = presenter;
        }

        public void Render(GameSettingsData settings, SettingsTab activeTab)
        {
            for (int index = 0; index < options.Length; index++)
            {
                SettingsOptionUi option = options[index];
                option.SetVisible(option.Tab == activeTab);
                RenderOption(option, settings);
            }
        }

        public void ShowDisplayConfirmation(int secondsRemaining)
        {
            displayConfirmationPanel.SetActive(true);
            displayConfirmationText.text = $"Keep these display settings? {secondsRemaining}";
        }

        public void HideDisplayConfirmation()
        {
            displayConfirmationPanel.SetActive(false);
        }

        public void ShowUnsavedChanges()
        {
            unsavedChangesPanel.SetActive(true);
        }

        public void HideUnsavedChanges()
        {
            unsavedChangesPanel.SetActive(false);
        }

        protected override void Awake()
        {
            base.Awake();
            if (options == null
                || audioTabButton == null
                || cameraTabButton == null
                || graphicsTabButton == null
                || controlsTabButton == null
                || applyButton == null
                || defaultsButton == null
                || backButton == null
                || displayConfirmationPanel == null
                || displayConfirmationText == null
                || keepDisplayButton == null
                || revertDisplayButton == null
                || unsavedChangesPanel == null
                || applyUnsavedButton == null
                || discardUnsavedButton == null
                || continueEditingButton == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(SettingsUi)} '{name}' has missing structural references.");
            }

            for (int index = 0; index < options.Length; index++)
            {
                SettingsOptionUi option = options[index];
                option.FloatValueChanged += HandleFloatValueChanged;
                option.BoolValueChanged += HandleBoolValueChanged;
                option.ActionRequested += HandleActionRequested;
            }

            audioTabButton.onClick.AddListener(HandleAudioTab);
            cameraTabButton.onClick.AddListener(HandleCameraTab);
            graphicsTabButton.onClick.AddListener(HandleGraphicsTab);
            controlsTabButton.onClick.AddListener(HandleControlsTab);
            applyButton.onClick.AddListener(HandleApply);
            defaultsButton.onClick.AddListener(HandleDefaults);
            backButton.onClick.AddListener(HandleBack);
            keepDisplayButton.onClick.AddListener(HandleKeepDisplay);
            revertDisplayButton.onClick.AddListener(HandleRevertDisplay);
            applyUnsavedButton.onClick.AddListener(HandleApplyUnsaved);
            discardUnsavedButton.onClick.AddListener(HandleDiscardUnsaved);
            continueEditingButton.onClick.AddListener(HandleContinueEditing);
            HideDisplayConfirmation();
            HideUnsavedChanges();
        }

        private void OnDestroy()
        {
            for (int index = 0; index < options.Length; index++)
            {
                SettingsOptionUi option = options[index];
                option.FloatValueChanged -= HandleFloatValueChanged;
                option.BoolValueChanged -= HandleBoolValueChanged;
                option.ActionRequested -= HandleActionRequested;
            }

            audioTabButton.onClick.RemoveListener(HandleAudioTab);
            cameraTabButton.onClick.RemoveListener(HandleCameraTab);
            graphicsTabButton.onClick.RemoveListener(HandleGraphicsTab);
            controlsTabButton.onClick.RemoveListener(HandleControlsTab);
            applyButton.onClick.RemoveListener(HandleApply);
            defaultsButton.onClick.RemoveListener(HandleDefaults);
            backButton.onClick.RemoveListener(HandleBack);
            keepDisplayButton.onClick.RemoveListener(HandleKeepDisplay);
            revertDisplayButton.onClick.RemoveListener(HandleRevertDisplay);
            applyUnsavedButton.onClick.RemoveListener(HandleApplyUnsaved);
            discardUnsavedButton.onClick.RemoveListener(HandleDiscardUnsaved);
            continueEditingButton.onClick.RemoveListener(HandleContinueEditing);
        }

        private static void RenderOption(SettingsOptionUi option, GameSettingsData settings)
        {
            switch (option.OptionId)
            {
                case SettingsOptionId.MasterVolume:
                    option.SetFloat(settings.Audio.MasterVolume, FormatVolume(settings.Audio.MasterVolume));
                    break;
                case SettingsOptionId.MusicVolume:
                    option.SetFloat(settings.Audio.MusicVolume, FormatVolume(settings.Audio.MusicVolume));
                    break;
                case SettingsOptionId.SfxVolume:
                    option.SetFloat(settings.Audio.SfxVolume, FormatVolume(settings.Audio.SfxVolume));
                    break;
                case SettingsOptionId.MuteAll:
                    option.SetToggle(settings.Audio.MuteAll, settings.Audio.MuteAll ? "On" : "Off");
                    break;
                case SettingsOptionId.CameraSensitivity:
                    option.SetFloat(settings.Camera.Sensitivity, FormatVolume(settings.Camera.Sensitivity));
                    break;
                case SettingsOptionId.InvertX:
                    option.SetToggle(settings.Camera.InvertX, settings.Camera.InvertX ? "On" : "Off");
                    break;
                case SettingsOptionId.InvertY:
                    option.SetToggle(settings.Camera.InvertY, settings.Camera.InvertY ? "On" : "Off");
                    break;
                case SettingsOptionId.WindowMode:
                    option.SetActionValue(settings.Graphics.WindowMode.ToString());
                    break;
                case SettingsOptionId.Resolution:
                    option.SetActionValue(FormatDisplayMode(settings.Graphics.DisplayMode));
                    break;
                case SettingsOptionId.Quality:
                    option.SetActionValue(settings.Graphics.QualityLevelName);
                    break;
                case SettingsOptionId.ResetBindings:
                    option.SetActionValue("Reset");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string FormatVolume(float value)
        {
            return Mathf.RoundToInt(value * 10f).ToString();
        }

        private static string FormatDisplayMode(DisplayModeData mode)
        {
            if (mode.RefreshRateDenominator == 0)
            {
                return $"{mode.Width} x {mode.Height}";
            }

            float refreshRate = (float)mode.RefreshRateNumerator / mode.RefreshRateDenominator;
            return $"{mode.Width} x {mode.Height} @ {refreshRate:0.#} Hz";
        }

        private void HandleFloatValueChanged(SettingsOptionId optionId, float value)
        {
            _presenter.OnOptionValueChanged(optionId, value);
        }

        private void HandleBoolValueChanged(SettingsOptionId optionId, bool value)
        {
            _presenter.OnOptionValueChanged(optionId, value);
        }

        private void HandleActionRequested(SettingsOptionId optionId)
        {
            _presenter.OnOptionAction(optionId);
        }

        private void HandleAudioTab() => _presenter.SelectTab(SettingsTab.Audio);
        private void HandleCameraTab() => _presenter.SelectTab(SettingsTab.Camera);
        private void HandleGraphicsTab() => _presenter.SelectTab(SettingsTab.Graphics);
        private void HandleControlsTab() => _presenter.SelectTab(SettingsTab.Controls);
        private void HandleApply() => _presenter.Apply();
        private void HandleDefaults() => _presenter.ResetCurrentSection();
        private void HandleBack() => _presenter.Back();
        private void HandleKeepDisplay() => _presenter.KeepDisplaySettings();
        private void HandleRevertDisplay() => _presenter.RevertDisplaySettings();
        private void HandleApplyUnsaved() => _presenter.ApplyUnsavedChanges();
        private void HandleDiscardUnsaved() => _presenter.DiscardUnsavedChanges();
        private void HandleContinueEditing() => _presenter.ContinueEditing();
    }
}
