using System;
using System.Collections.Generic;
using SoulsLike.Services.Audio;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.Save;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Services.Settings
{
    public sealed class SettingsService : ISettingsService, IInitializable
    {
        private const string SETTINGS_SAVE_KEY = "settings";

        private readonly ISaveService _saveService;
        private readonly SettingsDefaultsData _defaultsData;
        private readonly IAudioService _audioService;
        private readonly IInputService _inputService;
        private readonly IGraphicsSettingsApplier _graphicsSettingsApplier;

        private ICameraService _cameraService;
        private GameSettingsData _current;
        private GameSettingsData _baseline;
        private GameSettingsData _draft;
        private GameSettingsData _pendingCandidate;
        private GameSettingsData _pendingBaseline;

        public GameSettingsData Current => _current;
        public GameSettingsData Draft => _draft;
        public bool IsEditing => _draft != null;
        public bool HasUnsavedChanges => IsEditing && !SettingsDataUtility.AreEqual(_draft, _current);
        public SettingsCapabilities Capabilities => _graphicsSettingsApplier.Capabilities;
        public IReadOnlyList<DisplayModeData> AvailableDisplayModes => _graphicsSettingsApplier.GetAvailableDisplayModes();

        public SettingsService(
            ISaveService saveService,
            SettingsDefaultsData defaultsData,
            IAudioService audioService,
            IInputService inputService,
            IGraphicsSettingsApplier graphicsSettingsApplier)
        {
            _saveService = saveService;
            _defaultsData = defaultsData;
            _audioService = audioService;
            _inputService = inputService;
            _graphicsSettingsApplier = graphicsSettingsApplier;
        }

        public void Initialize()
        {
            bool exists = _saveService.Exists(SETTINGS_SAVE_KEY);
            GameSettingsData loaded = exists ? _saveService.Load<GameSettingsData>(SETTINGS_SAVE_KEY) : null;
            bool saveDefaults = !exists;

            if (loaded == null)
            {
                if (exists)
                {
                    Debug.LogError("[SettingsService] Settings data could not be loaded. Using defaults for this run.");
                }

                loaded = _defaultsData.CreateCopy();
            }
            else if (loaded.SchemaVersion > SettingsSchema.CURRENT_VERSION)
            {
                Debug.LogError(
                    $"[SettingsService] Settings schema {loaded.SchemaVersion} is newer than "
                    + $"supported schema {SettingsSchema.CURRENT_VERSION}. Using defaults without overwriting the save.");
                loaded = _defaultsData.CreateCopy();
                saveDefaults = false;
            }

            Validate(loaded);
            _current = SettingsDataUtility.Copy(loaded);
            ApplyAll(_current);

            if (saveDefaults)
            {
                _saveService.Save(SETTINGS_SAVE_KEY, _current);
            }
        }

        public void BeginEdit()
        {
            if (IsEditing)
            {
                throw new InvalidOperationException("A settings edit session is already active.");
            }

            _baseline = SettingsDataUtility.Copy(_current);
            _draft = SettingsDataUtility.Copy(_current);
        }

        public void Preview(SettingsSection section)
        {
            EnsureEditing();
            ValidateSection(_draft, section);

            switch (section)
            {
                case SettingsSection.Audio:
                    _audioService.ApplySettings(SettingsDataUtility.Copy(_draft.Audio));
                    break;
                case SettingsSection.Camera:
                    ApplyCamera(_draft.Camera);
                    break;
                case SettingsSection.Controls:
                    ApplyControls(_draft.Controls);
                    break;
                case SettingsSection.Graphics:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(section), section, null);
            }
        }

        public void ResetSection(SettingsSection section)
        {
            EnsureEditing();
            GameSettingsData defaults = _defaultsData.CreateCopy();
            Validate(defaults);

            switch (section)
            {
                case SettingsSection.Audio:
                    _draft.Audio = SettingsDataUtility.Copy(defaults.Audio);
                    break;
                case SettingsSection.Camera:
                    _draft.Camera = SettingsDataUtility.Copy(defaults.Camera);
                    break;
                case SettingsSection.Graphics:
                    _draft.Graphics = SettingsDataUtility.Copy(defaults.Graphics);
                    break;
                case SettingsSection.Controls:
                    _draft.Controls = SettingsDataUtility.Copy(defaults.Controls);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(section), section, null);
            }
        }

        public SettingsApplyResult Apply()
        {
            EnsureEditing();
            var candidate = SettingsDataUtility.Copy(_draft);
            Validate(candidate);
            ApplySafeSections(candidate);
            _graphicsSettingsApplier.Apply(candidate.Graphics);

            if (HasDisplayModeChanged(candidate.Graphics, _baseline.Graphics))
            {
                _pendingCandidate = candidate;
                _pendingBaseline = SettingsDataUtility.Copy(_baseline);
                return SettingsApplyResult.RequiresDisplayConfirmation;
            }

            Commit(candidate);
            return SettingsApplyResult.Applied;
        }

        public void ConfirmPendingDisplayChange()
        {
            if (_pendingCandidate == null)
            {
                throw new InvalidOperationException("There is no pending display change to confirm.");
            }

            Commit(_pendingCandidate);
        }

        public void RevertPendingDisplayChange()
        {
            if (_pendingCandidate == null)
            {
                throw new InvalidOperationException("There is no pending display change to revert.");
            }

            _pendingCandidate.Graphics.WindowMode = _pendingBaseline.Graphics.WindowMode;
            _pendingCandidate.Graphics.DisplayMode = SettingsDataUtility.Copy(_pendingBaseline.Graphics.DisplayMode);
            _graphicsSettingsApplier.Apply(_pendingCandidate.Graphics);
            Commit(_pendingCandidate);
        }

        public void CancelEdit()
        {
            EnsureEditing();

            if (_pendingCandidate != null)
            {
                _graphicsSettingsApplier.Apply(_pendingBaseline.Graphics);
            }

            ApplySafeSections(_baseline);
            ClearEditState();
        }

        public void RegisterCameraService(ICameraService cameraService)
        {
            _cameraService = cameraService;
            if (_current != null)
            {
                ApplyCamera(_current.Camera);
            }
        }

        public void UnregisterCameraService(ICameraService cameraService)
        {
            if (ReferenceEquals(_cameraService, cameraService))
            {
                _cameraService = null;
            }
        }

        private void ApplyAll(GameSettingsData settings)
        {
            ApplySafeSections(settings);
            _graphicsSettingsApplier.Apply(settings.Graphics);
        }

        private void ApplySafeSections(GameSettingsData settings)
        {
            _audioService.ApplySettings(SettingsDataUtility.Copy(settings.Audio));
            ApplyCamera(settings.Camera);
            ApplyControls(settings.Controls);
        }

        private void ApplyCamera(CameraSettingsData settings)
        {
            if (_cameraService != null)
            {
                _cameraService.ApplySettings(SettingsDataUtility.Copy(settings));
            }
        }

        private void ApplyControls(ControlsSettingsData settings)
        {
            try
            {
                _inputService.LoadBindingOverrides(settings.BindingOverridesJson);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[SettingsService] Binding overrides are invalid and were reset. {exception.Message}");
                settings.BindingOverridesJson = string.Empty;
                _inputService.ClearBindingOverrides();
            }
        }

        private void Commit(GameSettingsData settings)
        {
            _current = SettingsDataUtility.Copy(settings);
            _saveService.Save(SETTINGS_SAVE_KEY, _current);
            ClearEditState();
        }

        private void ClearEditState()
        {
            _baseline = null;
            _draft = null;
            _pendingCandidate = null;
            _pendingBaseline = null;
        }

        private void Validate(GameSettingsData settings)
        {
            settings.SchemaVersion = SettingsSchema.CURRENT_VERSION;
            settings.Audio ??= new SoulsLike.Services.Audio.Data.AudioSettingsData();
            settings.Camera ??= new CameraSettingsData();
            settings.Graphics ??= new GraphicsSettingsData();
            settings.Controls ??= new ControlsSettingsData();

            settings.Audio.MasterVolume = settings.Audio.MasterVolume;
            settings.Audio.MusicVolume = settings.Audio.MusicVolume;
            settings.Audio.SfxVolume = settings.Audio.SfxVolume;
            settings.Camera.Sensitivity = Mathf.Clamp01(settings.Camera.Sensitivity);
            settings.Controls.BindingOverridesJson ??= string.Empty;
            ValidateGraphics(settings.Graphics);
        }

        private void ValidateSection(GameSettingsData settings, SettingsSection section)
        {
            switch (section)
            {
                case SettingsSection.Audio:
                    settings.Audio ??= new SoulsLike.Services.Audio.Data.AudioSettingsData();
                    settings.Audio.MasterVolume = settings.Audio.MasterVolume;
                    settings.Audio.MusicVolume = settings.Audio.MusicVolume;
                    settings.Audio.SfxVolume = settings.Audio.SfxVolume;
                    break;
                case SettingsSection.Camera:
                    settings.Camera ??= new CameraSettingsData();
                    settings.Camera.Sensitivity = Mathf.Clamp01(settings.Camera.Sensitivity);
                    break;
                case SettingsSection.Graphics:
                    settings.Graphics ??= new GraphicsSettingsData();
                    ValidateGraphics(settings.Graphics);
                    break;
                case SettingsSection.Controls:
                    settings.Controls ??= new ControlsSettingsData();
                    settings.Controls.BindingOverridesJson ??= string.Empty;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(section), section, null);
            }
        }

        private void ValidateGraphics(GraphicsSettingsData graphics)
        {
            if (graphics.WindowMode != FullScreenMode.Windowed
                && graphics.WindowMode != FullScreenMode.FullScreenWindow
                && graphics.WindowMode != FullScreenMode.ExclusiveFullScreen)
            {
                graphics.WindowMode = FullScreenMode.FullScreenWindow;
            }

            if (graphics.WindowMode == FullScreenMode.ExclusiveFullScreen
                && !Capabilities.SupportsExclusiveFullscreen)
            {
                graphics.WindowMode = FullScreenMode.FullScreenWindow;
            }

            IReadOnlyList<DisplayModeData> modes = AvailableDisplayModes;
            if (graphics.DisplayMode == null || graphics.DisplayMode.Width <= 0 || graphics.DisplayMode.Height <= 0)
            {
                graphics.DisplayMode = SettingsDataUtility.Copy(modes[0]);
            }
            else if (!ContainsDisplayMode(modes, graphics.DisplayMode))
            {
                graphics.DisplayMode = SettingsDataUtility.Copy(FindClosestDisplayMode(modes, graphics.DisplayMode));
            }

            string[] qualityNames = QualitySettings.names;
            if (Array.IndexOf(qualityNames, graphics.QualityLevelName) < 0)
            {
                graphics.QualityLevelName = qualityNames[QualitySettings.GetQualityLevel()];
            }
        }

        private static bool HasDisplayModeChanged(GraphicsSettingsData candidate, GraphicsSettingsData baseline)
        {
            return candidate.WindowMode != baseline.WindowMode
                || !SettingsDataUtility.AreEqual(candidate.DisplayMode, baseline.DisplayMode);
        }

        private static bool ContainsDisplayMode(IReadOnlyList<DisplayModeData> modes, DisplayModeData candidate)
        {
            for (int index = 0; index < modes.Count; index++)
            {
                if (SettingsDataUtility.AreEqual(modes[index], candidate))
                {
                    return true;
                }
            }

            return false;
        }

        private static DisplayModeData FindClosestDisplayMode(
            IReadOnlyList<DisplayModeData> modes,
            DisplayModeData candidate)
        {
            DisplayModeData closest = modes[0];
            long bestDistance = long.MaxValue;
            for (int index = 0; index < modes.Count; index++)
            {
                DisplayModeData mode = modes[index];
                long widthDistance = mode.Width - candidate.Width;
                long heightDistance = mode.Height - candidate.Height;
                long distance = widthDistance * widthDistance + heightDistance * heightDistance;
                if (distance < bestDistance)
                {
                    closest = mode;
                    bestDistance = distance;
                }
            }

            return closest;
        }

        private void EnsureEditing()
        {
            if (!IsEditing)
            {
                throw new InvalidOperationException("No settings edit session is active.");
            }
        }
    }
}
