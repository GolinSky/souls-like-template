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
    public class AmbienceSystem : IAmbienceSystem, IInitializable, ITickable, IDisposable, IObserver<IAudioSettingsData>, ICombatStateObserver
    {
        private readonly AmbienceData _data;
        private readonly IAudioService _audioService;
        private readonly ISceneService _sceneService;
        private readonly ICombatStateNotifier _combatStateNotifier;

        private GameObject _hostGo;
        private AudioSource _musicSource;
        private AudioSource _ambienceSource;
        private AudioSource _sfxSource;

        private MusicType _currentMusic = MusicType.None;
        private MusicType _currentAmbience = MusicType.None;
        private AudioClip[] _sceneAmbienceClips;
        private int _sceneAmbienceIndex;
        private bool _combatAmbienceActive;
        private float _musicSettingsVolume = 1f;
        private float _sfxSettingsVolume = 1f;

        private float _audioScale = 1f;
        private Tween _fadeTween;

        public event Action<float> VolumeScaleChanged;

        public AmbienceSystem(
            AmbienceData data,
            IAudioService audioService,
            ISceneService sceneService,
            ICombatStateNotifier combatStateNotifier)
        {
            _data = data;
            _audioService = audioService;
            _sceneService = sceneService;
            _combatStateNotifier = combatStateNotifier;
        }

        public void Initialize()
        {
            _hostGo = new GameObject(nameof(AmbienceSystem));
            UnityEngine.Object.DontDestroyOnLoad(_hostGo);

            _musicSource = _hostGo.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.spatialBlend = 0f;

            _ambienceSource = _hostGo.AddComponent<AudioSource>();
            _ambienceSource.playOnAwake = false;
            _ambienceSource.loop = false;
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
            _combatStateNotifier.RegisterObserver(this);
            OnCombatStateChanged(_combatStateNotifier.CurrentCombatState);
        }

        public void Tick()
        {
            if (_sceneAmbienceClips == null || _sceneAmbienceClips.Length <= 1 || _ambienceSource.isPlaying)
            {
                return;
            }

            PlayNextSceneAmbience();
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
            _combatStateNotifier.UnregisterObserver(this);

            if (_hostGo != null)
            {
                UnityEngine.Object.Destroy(_hostGo);
                _hostGo = null;
                _musicSource = null;
                _ambienceSource = null;
                _sfxSource = null;
            }

            _sceneAmbienceClips = null;
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

        public void OnCombatStateChanged(CombatState newState)
        {
            if (newState == CombatState.Combat)
            {
                PlayCombatAmbience();
                return;
            }

            if (!_combatAmbienceActive)
            {
                return;
            }

            _combatAmbienceActive = false;
            if (_sceneService != null)
            {
                PlaySceneAmbience(_sceneService.CurrentScene);
                return;
            }

            StopAmbience();
        }

        public void PlayMusic(MusicType type)
        {
            var clip = _data.GetMusicClip(type);
            if (clip == null)
            {
                Debug.LogError($"{nameof(AmbienceSystem)}.PlayMusic({type}): no clip mapped in {nameof(AmbienceData)}.musicClips.");
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
            if (type == MusicType.None)
            {
                StopAmbience();
                return;
            }

            var clip = _data.GetMusicClip(type);
            if (clip == null)
            {
                Debug.LogError($"{nameof(AmbienceSystem)}.PlayAmbience({type}): no clip mapped in {nameof(AmbienceData)}.musicClips.");
                StopAmbience();
                return;
            }

            _combatAmbienceActive = false;
            ClearSceneAmbiencePlaylist();
            _ambienceSource.loop = true;

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
            _combatAmbienceActive = false;
            ClearSceneAmbiencePlaylist();
            if (_ambienceSource == null) return;
            _ambienceSource.Stop();
            _ambienceSource.clip = null;
        }

        public void PlaySfx(SfxType type)
        {
            var clip = _data.GetSfxClip(type);
            if (clip == null)
            {
                Debug.LogError($"{nameof(AmbienceSystem)}.PlaySfx({type}): no clip mapped in {nameof(AmbienceData)}.sfxClips.");
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
            if (_combatAmbienceActive)
            {
                return;
            }

            PlaySceneAmbience(sceneType);
        }

        private void PlaySceneAmbience(SceneType sceneType)
        {
            var clips = _data.GetAmbienceClipsForScene(sceneType);
            if (clips == null || clips.Length == 0)
            {
                StopAmbience();
                return;
            }

            PlayAmbiencePlaylist(clips);
        }

        private void PlayCombatAmbience()
        {
            _combatAmbienceActive = true;
            var clips = _data.GetCombatAmbienceClips();
            if (clips == null || clips.Length == 0)
            {
                Debug.LogError($"{nameof(AmbienceSystem)}.PlayCombatAmbience(): no clips mapped in {nameof(AmbienceData)}.combatAmbienceClips.");
                ClearSceneAmbiencePlaylist();
                _ambienceSource.Stop();
                _ambienceSource.clip = null;
                return;
            }

            PlayAmbiencePlaylist(clips);
        }

        private void PlayAmbiencePlaylist(AudioClip[] clips)
        {
            _currentAmbience = MusicType.None;
            _sceneAmbienceClips = (AudioClip[])clips.Clone();
            _sceneAmbienceIndex = 0;
            _ambienceSource.Stop();
            _ambienceSource.loop = _sceneAmbienceClips.Length == 1;
            ShuffleSceneAmbience();
            PlayNextSceneAmbience();
        }

        private void PlayNextSceneAmbience()
        {
            if (_sceneAmbienceIndex >= _sceneAmbienceClips.Length)
            {
                ShuffleSceneAmbience();
                _sceneAmbienceIndex = 0;
            }

            _ambienceSource.clip = _sceneAmbienceClips[_sceneAmbienceIndex++];
            ApplyAmbienceVolume();
            _ambienceSource.Play();
        }

        private void ShuffleSceneAmbience()
        {
            for (var i = _sceneAmbienceClips.Length - 1; i > 0; i--)
            {
                var swapIndex = UnityEngine.Random.Range(0, i + 1);
                (_sceneAmbienceClips[i], _sceneAmbienceClips[swapIndex]) =
                    (_sceneAmbienceClips[swapIndex], _sceneAmbienceClips[i]);
            }
        }

        private void ClearSceneAmbiencePlaylist()
        {
            _sceneAmbienceClips = null;
            _sceneAmbienceIndex = 0;
            if (_ambienceSource != null)
            {
                _ambienceSource.loop = false;
            }
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
