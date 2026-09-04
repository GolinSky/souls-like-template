using System;
using System.Collections.Generic;
using SoulsLike.Services;
using SoulsLike.Services.Settings;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.Settings
{
    public sealed class SettingsUiController : UiController,
        IInitializable,
        ITickable,
        ISettingsPresenter,
        ISettingsRoute,
        IDisposable
    {
        private const float DISPLAY_CONFIRMATION_DURATION = 15f;

        private readonly ISettingsService _settingsService;
        private SettingsUi _settingsUi;
        private SettingsTab _activeTab = SettingsTab.Audio;
        private float _displayConfirmationRemaining;
        private bool _isDisplayConfirmationActive;

        public event Action CloseRequested;

        public SettingsUiController(IUiService uiService, ISettingsService settingsService)
            : base(uiService)
        {
            _settingsService = settingsService;
        }

        public void Initialize()
        {
            _settingsUi = CreateUi<SettingsUi>();
            _settingsUi.AssignPresenter(this);
            _settingsUi.Hide();
        }

        public void Dispose()
        {
            if (_settingsService.IsEditing)
            {
                _settingsService.CancelEdit();
            }

            _isDisplayConfirmationActive = false;
        }

        public void Tick()
        {
            if (!_isDisplayConfirmationActive)
            {
                return;
            }

            _displayConfirmationRemaining -= Time.unscaledDeltaTime;
            if (_displayConfirmationRemaining <= 0f)
            {
                RevertDisplaySettings();
                return;
            }

            _settingsUi.ShowDisplayConfirmation(Mathf.CeilToInt(_displayConfirmationRemaining));
        }

        public void Show()
        {
            _settingsService.BeginEdit();
            _settingsUi.Show();
            _settingsUi.HideDisplayConfirmation();
            _settingsUi.HideUnsavedChanges();
            RenderDraft();
        }

        public void Hide()
        {
            if (_settingsService.IsEditing)
            {
                _settingsService.CancelEdit();
            }

            _isDisplayConfirmationActive = false;
            _settingsUi.HideDisplayConfirmation();
            _settingsUi.HideUnsavedChanges();
            _settingsUi.Hide();
        }

        public void OnOptionValueChanged(SettingsOptionId optionId, float value)
        {
            switch (optionId)
            {
                case SettingsOptionId.MasterVolume:
                    _settingsService.Draft.Audio.MasterVolume = value;
                    _settingsService.Preview(SettingsSection.Audio);
                    break;
                case SettingsOptionId.MusicVolume:
                    _settingsService.Draft.Audio.MusicVolume = value;
                    _settingsService.Preview(SettingsSection.Audio);
                    break;
                case SettingsOptionId.SfxVolume:
                    _settingsService.Draft.Audio.SfxVolume = value;
                    _settingsService.Preview(SettingsSection.Audio);
                    break;
                case SettingsOptionId.CameraSensitivity:
                    _settingsService.Draft.Camera.Sensitivity = value;
                    _settingsService.Preview(SettingsSection.Camera);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(optionId), optionId, null);
            }

            RenderDraft();
        }

        public void OnOptionValueChanged(SettingsOptionId optionId, bool value)
        {
            switch (optionId)
            {
                case SettingsOptionId.MuteAll:
                    _settingsService.Draft.Audio.MuteAll = value;
                    _settingsService.Preview(SettingsSection.Audio);
                    break;
                case SettingsOptionId.InvertX:
                    _settingsService.Draft.Camera.InvertX = value;
                    _settingsService.Preview(SettingsSection.Camera);
                    break;
                case SettingsOptionId.InvertY:
                    _settingsService.Draft.Camera.InvertY = value;
                    _settingsService.Preview(SettingsSection.Camera);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(optionId), optionId, null);
            }

            RenderDraft();
        }

        public void OnOptionAction(SettingsOptionId optionId)
        {
            switch (optionId)
            {
                case SettingsOptionId.WindowMode:
                    CycleWindowMode();
                    break;
                case SettingsOptionId.Resolution:
                    CycleResolution();
                    break;
                case SettingsOptionId.Quality:
                    CycleQualityLevel();
                    break;
                case SettingsOptionId.ResetBindings:
                    _settingsService.Draft.Controls.BindingOverridesJson = string.Empty;
                    _settingsService.Preview(SettingsSection.Controls);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(optionId), optionId, null);
            }

            RenderDraft();
        }

        public void Apply()
        {
            SettingsApplyResult result = _settingsService.Apply();
            if (result == SettingsApplyResult.RequiresDisplayConfirmation)
            {
                _isDisplayConfirmationActive = true;
                _displayConfirmationRemaining = DISPLAY_CONFIRMATION_DURATION;
                _settingsUi.ShowDisplayConfirmation(Mathf.CeilToInt(_displayConfirmationRemaining));
                return;
            }

            Close();
        }

        public void ResetCurrentSection()
        {
            SettingsSection section = ToSettingsSection(_activeTab);
            _settingsService.ResetSection(section);
            _settingsService.Preview(section);
            RenderDraft();
        }

        public void Back()
        {
            if (_settingsService.HasUnsavedChanges)
            {
                _settingsUi.ShowUnsavedChanges();
                return;
            }

            Close();
        }

        public void KeepDisplaySettings()
        {
            _settingsService.ConfirmPendingDisplayChange();
            _isDisplayConfirmationActive = false;
            _settingsUi.HideDisplayConfirmation();
            Close();
        }

        public void RevertDisplaySettings()
        {
            _settingsService.RevertPendingDisplayChange();
            _isDisplayConfirmationActive = false;
            _settingsUi.HideDisplayConfirmation();
            Close();
        }

        public void ApplyUnsavedChanges()
        {
            _settingsUi.HideUnsavedChanges();
            Apply();
        }

        public void DiscardUnsavedChanges()
        {
            _settingsUi.HideUnsavedChanges();
            _settingsService.CancelEdit();
            Close();
        }

        public void ContinueEditing()
        {
            _settingsUi.HideUnsavedChanges();
        }

        public void SelectTab(SettingsTab tab)
        {
            _activeTab = tab;
            RenderDraft();
        }

        private void Close()
        {
            if (_settingsService.IsEditing)
            {
                _settingsService.CancelEdit();
            }

            _isDisplayConfirmationActive = false;
            _settingsUi.HideDisplayConfirmation();
            _settingsUi.HideUnsavedChanges();
            _settingsUi.Hide();
            CloseRequested?.Invoke();
        }

        private void RenderDraft()
        {
            _settingsUi.Render(_settingsService.Draft, _activeTab);
        }

        private void CycleWindowMode()
        {
            FullScreenMode currentMode = _settingsService.Draft.Graphics.WindowMode;
            _settingsService.Draft.Graphics.WindowMode = currentMode switch
            {
                FullScreenMode.Windowed => FullScreenMode.FullScreenWindow,
                FullScreenMode.FullScreenWindow when _settingsService.Capabilities.SupportsExclusiveFullscreen
                    => FullScreenMode.ExclusiveFullScreen,
                _ => FullScreenMode.Windowed
            };
        }

        private void CycleResolution()
        {
            IReadOnlyList<DisplayModeData> modes = _settingsService.AvailableDisplayModes;
            DisplayModeData current = _settingsService.Draft.Graphics.DisplayMode;
            int currentIndex = 0;
            for (int index = 0; index < modes.Count; index++)
            {
                if (SettingsDataUtility.AreEqual(modes[index], current))
                {
                    currentIndex = index;
                    break;
                }
            }

            int nextIndex = (currentIndex + 1) % modes.Count;
            _settingsService.Draft.Graphics.DisplayMode = SettingsDataUtility.Copy(modes[nextIndex]);
        }

        private void CycleQualityLevel()
        {
            string[] names = QualitySettings.names;
            int currentIndex = Array.IndexOf(names, _settingsService.Draft.Graphics.QualityLevelName);
            int nextIndex = (currentIndex + 1) % names.Length;
            _settingsService.Draft.Graphics.QualityLevelName = names[nextIndex];
        }

        private static SettingsSection ToSettingsSection(SettingsTab tab)
        {
            return tab switch
            {
                SettingsTab.Audio => SettingsSection.Audio,
                SettingsTab.Camera => SettingsSection.Camera,
                SettingsTab.Graphics => SettingsSection.Graphics,
                SettingsTab.Controls => SettingsSection.Controls,
                _ => throw new ArgumentOutOfRangeException(nameof(tab), tab, null)
            };
        }
    }
}
