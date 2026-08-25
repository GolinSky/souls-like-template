using Prospector.Utility.Timer;
using SoulsLike.Services;
using SoulsLike.Services.Audio;
using SoulsLike.Services.Audio.Data;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace SoulsLike.Entities.Character.Components
{
    public sealed class CharacterAudioComponent : BaseComponent, IObserver<IAudioSettingsData>
    {
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioSource landingSource;
        [SerializeField] private AudioSource hitSource;
        [SerializeField] private AudioSource swordClashSource;
        [SerializeField] private AudioSource attackSource;
        [SerializeField, Min(0.01f)] private float footstepFrequency = 2f;
        [SerializeField, Min(0.01f)] private float sprintFootstepFrequency = 3f;

        private IAudioService _audioService;
        private CharacterAudioData _data;
        private ITimer _footstepTimer;
        private bool _isObserving;

        [Inject]
        public void Configure(IAudioService audioService, CharacterAudioData data)
        {
            _audioService = audioService;
            _data = data;
            _footstepTimer = TimerFactory.ConstructTimer();
            footstepSource.resource = _data.Footstep;
            landingSource.resource = _data.Landing;
            hitSource.resource = _data.Hit;
            swordClashSource.resource = _data.SwordClash;
            _audioService.AddObserver(this);
            _isObserving = true;
        }

        private void OnDestroy()
        {
            if (!_isObserving)
            {
                return;
            }

            _audioService.RemoveObserver(this);
            _isObserving = false;
        }

        public void UpdateState(IAudioSettingsData settings)
        {
            float volume = settings.MuteAll
                ? 0f
                : _audioService.BaseVolume * settings.MasterVolume * settings.SfxVolume;

            footstepSource.volume = volume;
            landingSource.volume = volume;
            hitSource.volume = volume;
            swordClashSource.volume = volume;
            attackSource.volume = volume;
        }

        public void Tick(bool isMoving, bool isSprinting)
        {
            if (!isMoving)
            {
                _footstepTimer.Reset();
                return;
            }

            if (_footstepTimer.IsRunning && !_footstepTimer.IsComplete)
            {
                return;
            }

            footstepSource.Play();
            float frequency = isSprinting ? sprintFootstepFrequency : footstepFrequency;
            _footstepTimer
                .ChangeDuration(1f / frequency)
                .Start();
        }

        public void NotifyLand() => landingSource.Play();

        public void NotifyHit()
        {
            if (_data.Hit == null)
            {
                Debug.LogError($"[{nameof(CharacterAudioComponent)}] Hit audio resource is missing.");
                return;
            }

            hitSource.Play();
        }

        public void NotifySwordClash()
        {
            if (_data.SwordClash == null)
            {
                Debug.LogError($"[{nameof(CharacterAudioComponent)}] Sword clash audio resource is missing.");
                return;
            }

            swordClashSource.Play();
        }

        public void NotifyAttack(AudioResource attackSfx)
        {
            if (attackSfx == null)
            {
                Debug.LogError($"[{nameof(CharacterAudioComponent)}] Attack audio resource is missing.");
                return;
            }

            attackSource.resource = attackSfx;
            attackSource.Play();
        }
    }
}
