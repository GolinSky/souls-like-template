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
        private const float DEFAULT_CLIP_FADE_DURATION = 0.5f;

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
        private Tween _musicTransitionTween;
        private Tween _ambienceTransitionTween;
        private float _musicTransitionScale = 1f;
        private float _ambienceTransitionScale = 1f;

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
            if (_sceneAmbienceClips == null || _sceneAmbienceClips.Length <= 1 || IsAmbienceTransitioning())
            {
                return;
            }

            if (!_ambienceSource.isPlaying)
            {
                PlayNextSceneAmbience();
                return;
            }

            if (_ambienceSource.time >= _ambienceSource.clip.length - DEFAULT_CLIP_FADE_DURATION)
            {
                PlayNextSceneAmbience(GetAmbienceOutgoingFadeDuration());
            }
        }

        public void Dispose()
        {
            _fadeTween?.Kill();
            _fadeTween = null;
            _musicTransitionTween?.Kill();
            _musicTransitionTween = null;
            _ambienceTransitionTween?.Kill();
            _ambienceTransitionTween = null;
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
            TransitionMusicTo(clip);
        }

        public void StopMusic()
        {
            _currentMusic = MusicType.None;
            TransitionMusicTo(null);
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
            TransitionAmbienceTo(clip);
        }

        public void StopAmbience()
        {
            _currentAmbience = MusicType.None;
            _combatAmbienceActive = false;
            ClearSceneAmbiencePlaylist();
            TransitionAmbienceTo(null);
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
                TransitionAmbienceTo(null);
                return;
            }

            PlayAmbiencePlaylist(clips);
        }

        private void PlayAmbiencePlaylist(AudioClip[] clips)
        {
            _currentAmbience = MusicType.None;
            _sceneAmbienceClips = (AudioClip[])clips.Clone();
            _sceneAmbienceIndex = 0;
            _ambienceSource.loop = _sceneAmbienceClips.Length == 1;
            var outgoingFadeDuration = GetAmbienceOutgoingFadeDuration();
            ShuffleSceneAmbience();
            PlayNextSceneAmbience(outgoingFadeDuration);
        }

        private void PlayNextSceneAmbience(float outgoingFadeDuration = DEFAULT_CLIP_FADE_DURATION)
        {
            if (_sceneAmbienceClips == null || _sceneAmbienceClips.Length == 0)
            {
                return;
            }

            if (_sceneAmbienceIndex >= _sceneAmbienceClips.Length)
            {
                ShuffleSceneAmbience();
                _sceneAmbienceIndex = 0;
            }

            TransitionAmbienceTo(_sceneAmbienceClips[_sceneAmbienceIndex++], true, outgoingFadeDuration);
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
            _musicSource.volume = clipVolume * _musicSettingsVolume * _audioScale * _musicTransitionScale;
        }

        private void ApplyAmbienceVolume()
        {
            if (_ambienceSource == null) return;
            var clipVolume = _data != null ? _data.MusicClipVolume : 1f;
            _ambienceSource.volume = clipVolume * _musicSettingsVolume * _audioScale * _ambienceTransitionScale;
        }

        private void TransitionMusicTo(AudioClip clip)
        {
            _musicTransitionTween?.Kill();
            _musicTransitionTween = TransitionTo(_musicSource, clip, false, DEFAULT_CLIP_FADE_DURATION);
        }

        private void TransitionAmbienceTo(AudioClip clip, bool forceRestart = false, float outgoingFadeDuration = DEFAULT_CLIP_FADE_DURATION)
        {
            _ambienceTransitionTween?.Kill();
            _ambienceTransitionTween = TransitionTo(_ambienceSource, clip, forceRestart, outgoingFadeDuration);
        }

        private Tween TransitionTo(AudioSource source, AudioClip clip, bool forceRestart, float outgoingFadeDuration)
        {
            if (source == null)
            {
                return null;
            }

            if (!source.isPlaying)
            {
                if (clip == null)
                {
                    source.Stop();
                    source.clip = null;
                    SetTransitionScale(source, 1f);
                    return null;
                }

                source.clip = clip;
                SetTransitionScale(source, 0f);
                source.Play();
                return CreateFadeTween(source, 1f);
            }

            if (!forceRestart && source.clip == clip)
            {
                return CreateFadeTween(source, 1f);
            }

            var transition = DOTween.Sequence();
            transition.Append(CreateFadeTween(source, GetTransitionScale(source), 0f, outgoingFadeDuration));
            transition.AppendCallback(() =>
            {
                source.Stop();
                source.clip = clip;
                if (clip != null)
                {
                    source.Play();
                }
            });

            if (clip != null)
            {
                transition.Append(CreateFadeTween(source, 0f, 1f, DEFAULT_CLIP_FADE_DURATION));
            }
            else
            {
                transition.AppendCallback(() => SetTransitionScale(source, 1f));
            }

            return transition;
        }

        private Tween CreateFadeTween(AudioSource source, float target)
        {
            return CreateFadeTween(source, GetTransitionScale(source), target, DEFAULT_CLIP_FADE_DURATION);
        }

        private Tween CreateFadeTween(AudioSource source, float start, float target, float duration)
        {
            return DOVirtual.Float(start, target, duration, value => SetTransitionScale(source, value))
                .SetEase(Ease.InOutSine);
        }

        private float GetTransitionScale(AudioSource source)
        {
            return source == _musicSource ? _musicTransitionScale : _ambienceTransitionScale;
        }

        private void SetTransitionScale(AudioSource source, float value)
        {
            if (source == _musicSource)
            {
                _musicTransitionScale = value;
                ApplyMusicVolume();
                return;
            }

            _ambienceTransitionScale = value;
            ApplyAmbienceVolume();
        }

        private bool IsAmbienceTransitioning()
        {
            return _ambienceTransitionTween != null && _ambienceTransitionTween.IsActive() && _ambienceTransitionTween.IsPlaying();
        }

        private float GetAmbienceOutgoingFadeDuration()
        {
            if (!_ambienceSource.isPlaying || _ambienceSource.loop)
            {
                return DEFAULT_CLIP_FADE_DURATION;
            }

            var remainingPlaybackTime = Mathf.Max(0f, _ambienceSource.clip.length - _ambienceSource.time);
            return Mathf.Min(DEFAULT_CLIP_FADE_DURATION, remainingPlaybackTime);
        }
    }
}
