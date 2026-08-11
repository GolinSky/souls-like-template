using System;
using SoulsLike.Services;
using SoulsLike.Services.Audio.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace SoulsLike.Services.Audio
{
    public class AmbienceManagerWrapper : IInitializable, IDisposable, IObserver<IAudioSettingsData>, IStartable
    {
        private const float AMBIENCE_MANAGER_VOLUME = 0.4f;
        private readonly IAudioService _audioService;
        private readonly IAmbienceSystem _ambienceSystem;
        private Component _ambienceManager;

        private float _settingsVolume = AMBIENCE_MANAGER_VOLUME;
        private float _volumeScale = 1f;

        public AmbienceManagerWrapper(IAudioService audioService, IAmbienceSystem ambienceSystem, Component ambienceManager = null)
        {
            _audioService = audioService;
            _ambienceSystem = ambienceSystem;
            _ambienceManager = ambienceManager;
        }

        public void Initialize()
        {
            _audioService?.AddObserver(this);
            if (_ambienceSystem != null)
            {
                _ambienceSystem.VolumeScaleChanged += OnVolumeScaleChanged;
            }
        }

        public void Start()
        {
            UndoDontDestroyOnLoad();
        }

        public void Dispose()
        {
            _audioService?.RemoveObserver(this);
            if (_ambienceSystem != null)
            {
                _ambienceSystem.VolumeScaleChanged -= OnVolumeScaleChanged;
            }
        }

        public void UpdateState(IAudioSettingsData arg)
        {
            if (arg == null || _audioService == null) return;
            _settingsVolume = arg.MuteAll
                ? 0f
                : AMBIENCE_MANAGER_VOLUME * _audioService.BaseVolume * arg.MasterVolume * arg.MusicVolume;
            ApplyVolume();
        }

        private void OnVolumeScaleChanged(float scale)
        {
            _volumeScale = scale;
            ApplyVolume();
        }

        private void ApplyVolume()
        {
            if (_ambienceManager == null) return;

            // Reflectively set m_volume if Component exists
            var field = _ambienceManager.GetType().GetField("m_volume");
            if (field != null)
            {
                field.SetValue(_ambienceManager, _settingsVolume * _volumeScale);
            }
        }

        private void UndoDontDestroyOnLoad()
        {
            if (_ambienceManager == null) return;

            var go = _ambienceManager.gameObject;
            if (go == null) return;

            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded) return;
            if (go.scene == activeScene) return;

            SceneManager.MoveGameObjectToScene(go, activeScene);
        }
    }
}
