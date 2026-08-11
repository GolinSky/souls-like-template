using System;
using DG.Tweening;
using SoulsLike.Services;
using SoulsLike.Services.Audio.Data;
using SoulsLike.Services.Scenes;
using SoulsLike.Services.Scenes.Data;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Services.Audio
{
    public class AmbienceService : IAmbienceSystem, IInitializable, IDisposable, IObserver<IAudioSettingsData>
    {
        private readonly AmbienceData _data;
        private readonly IAudioService _audioService;
        private readonly ISceneService _sceneService;

        private GameObject _hostGo;
        private AudioSource _musicSource;
        private AudioSource _ambienceSource;
        private AudioSource _sfxSource;

        private MusicType _currentMusic = MusicType.None;
        private MusicType _currentAmbience = MusicType.None;
        private float _musicSettingsVolume = 1f;
        private float _sfxSettingsVolume = 1f;

        private float _audioScale = 1f;
        private Tween _fadeTween;

        public event Action<float> VolumeScaleChanged;

        public AmbienceService(AmbienceData data, IAudioService audioService, ISceneService sceneService)
        {
            _data = data;
            _audioService = audioService;
            _sceneService = sceneService;
        }

        public void Initialize()
        {
            _hostGo = new GameObject(nameof(AmbienceService));
            UnityEngine.Object.DontDestroyOnLoad(_hostGo);

            _musicSource = _hostGo.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;

            _ambienceSource = _hostGo.AddComponent<AudioSource>();
            _ambienceSource.playOnAwake = false;
            _ambienceSource.loop = true;
            _ambienceSource.spatialBlend = 0f;

            _sfxSource = _hostGo.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;
            _sfxSource.loop = false;
            _sfxSource.spatialBlend = 0f;

            if (_sceneService != null)
            {
                _sceneService.OnSceneChanged += OnSceneChanged;
                OnSceneChanged(_sceneService.CurrentScene);
            }

            _audioService?.AddObserver(this);
        }

        public void Dispose()
        {
            _fadeTween?.Kill();
            _fadeTween = null;
            if (_sceneService != null)
            {
                _sceneService.OnSceneChanged -= OnSceneChanged;
            }
            _audioService?.RemoveObserver(this);

            if (_hostGo != null)
            {
                UnityEngine.Object.Destroy(_hostGo);
                _hostGo = null;
                _musicSource = null;
                _ambienceSource = null;
                _sfxSource = null;
            }
        }

        public void UpdateState(IAudioSettingsData arg)
        {
            if (arg == null) return;

            if (arg.MuteAll)
            {
                _musicSettingsVolume = 0f;
                _sfxSettingsVolume = 0f;
            }
            else
            {
                var baseTimesMaster = (_audioService != null ? _audioService.BaseVolume : 1f) * arg.MasterVolume;
                _musicSettingsVolume = baseTimesMaster * arg.MusicVolume;
                _sfxSettingsVolume = baseTimesMaster * arg.SfxVolume;
            }
            ApplyMusicVolume();
            ApplyAmbienceVolume();
        }

        public void PlayMusic(MusicType type)
        {
            if (_musicSource == null)
            {
                Debug.LogError($"{nameof(AmbienceService)}.PlayMusic({type}) called before Initialize.");
                return;
            }

            if (type == MusicType.None)
            {
                return;
            }

            if (_data == null)
            {
                Debug.LogError($"{nameof(AmbienceService)}.PlayMusic({type}): {nameof(AmbienceData)} is null.");
                return;
            }

            var clip = _data.GetMusicClip(type);
            if (clip == null)
            {
                Debug.LogError($"{nameof(AmbienceService)}.PlayMusic({type}): no clip mapped in {nameof(AmbienceData)}.musicClips.");
                return;
            }

            if (_currentMusic == type && _musicSource.clip == clip && _musicSource.isPlaying)
            {
                ApplyMusicVolume();
                return;
            }

            _currentMusic = type;
            _musicSource.clip = clip;
            ApplyMusicVolume();
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _currentMusic = MusicType.None;
            if (_musicSource == null) return;
            _musicSource.Stop();
            _musicSource.clip = null;
        }

        public void PlayAmbience(MusicType type)
        {
            if (_ambienceSource == null)
            {
                Debug.LogError($"{nameof(AmbienceService)}.PlayAmbience({type}) called before Initialize.");
                return;
            }

            if (type == MusicType.None)
            {
                StopAmbience();
                return;
            }

            if (_data == null)
            {
                Debug.LogError($"{nameof(AmbienceService)}.PlayAmbience({type}): {nameof(AmbienceData)} is null.");
                return;
            }

            var clip = _data.GetMusicClip(type);
            if (clip == null)
            {
                Debug.LogError($"{nameof(AmbienceService)}.PlayAmbience({type}): no clip mapped in {nameof(AmbienceData)}.musicClips.");
                StopAmbience();
                return;
            }

            if (_currentAmbience == type && _ambienceSource.clip == clip && _ambienceSource.isPlaying)
            {
                ApplyAmbienceVolume();
                return;
            }

            _currentAmbience = type;
            _ambienceSource.clip = clip;
            ApplyAmbienceVolume();
            _ambienceSource.Play();
        }

        public void StopAmbience()
        {
            _currentAmbience = MusicType.None;
            if (_ambienceSource == null) return;
            _ambienceSource.Stop();
            _ambienceSource.clip = null;
        }

        public void PlaySfx(SfxType type)
        {
            if (_sfxSource == null)
            {
                Debug.LogError($"{nameof(AmbienceService)}.PlaySfx({type}) called before Initialize.");
                return;
            }
            if (_data == null)
            {
                Debug.LogError($"{nameof(AmbienceService)}.PlaySfx({type}): {nameof(AmbienceData)} is null.");
                return;
            }
            var clip = _data.GetSfxClip(type);
            if (clip == null)
            {
                Debug.LogError($"{nameof(AmbienceService)}.PlaySfx({type}): no clip mapped in {nameof(AmbienceData)}.sfxClips.");
                return;
            }
            _sfxSource.PlayOneShot(clip, _data.SfxClipVolume * _sfxSettingsVolume * _audioScale);
        }

        public void DisableAllAudio(float fadeDuration) => FadeAudioScale(0f, fadeDuration);

        public void EnableAllAudio(float fadeDuration) => FadeAudioScale(1f, fadeDuration);

        private void FadeAudioScale(float target, float duration)
        {
            _fadeTween?.Kill();

            if (duration <= 0f)
            {
                SetAudioScale(target);
                return;
            }

            _fadeTween = DOVirtual.Float(_audioScale, target, duration, SetAudioScale);
        }

        private void SetAudioScale(float value)
        {
            _audioScale = value;
            ApplyMusicVolume();
            ApplyAmbienceVolume();
            VolumeScaleChanged?.Invoke(value);
        }

        private void OnSceneChanged(SceneType sceneType)
        {
            var ambience = _data != null ? _data.GetMusicForScene(sceneType) : MusicType.None;
            if (ambience == MusicType.None)
            {
                StopAmbience();
                return;
            }
            PlayAmbience(ambience);
        }

        private void ApplyMusicVolume()
        {
            if (_musicSource == null) return;
            var clipVolume = _data != null ? _data.MusicClipVolume : 1f;
            _musicSource.volume = clipVolume * _musicSettingsVolume * _audioScale;
        }

        private void ApplyAmbienceVolume()
        {
            if (_ambienceSource == null) return;
            var clipVolume = _data != null ? _data.MusicClipVolume : 1f;
            _ambienceSource.volume = clipVolume * _musicSettingsVolume * _audioScale;
        }
    }
}
