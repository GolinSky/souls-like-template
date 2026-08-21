using SoulsLike.Services;
using SoulsLike.Services.Audio;
using SoulsLike.Services.Audio.Data;
using UnityEngine;
using VContainer;

namespace SoulsLike.Entities.Character.Components
{
    public sealed class CharacterAudioComponent : BaseComponent, IObserver<IAudioSettingsData>
    {
        [SerializeField] private AudioSource footstepSource;
        [SerializeField] private AudioSource landingSource;
        [SerializeField] private AudioSource hitSource;
        [SerializeField] private AudioSource swordClashSource;
        [SerializeField, Min(0.01f)] private float footstepFrequency = 2f;

        private IAudioService _audioService;
        private CharacterAudioData _data;
        private float _footstepTimer;
        private bool _isObserving;

        [Inject]
        public void Configure(IAudioService audioService, CharacterAudioData data)
        {
            _audioService = audioService;
            _data = data;
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
        }

        public void Tick(bool isMoving)
        {
            if (!isMoving)
            {
                _footstepTimer = 0f;
                return;
            }

            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer > 0f)
            {
                return;
            }

            footstepSource.Play();
            _footstepTimer = 1f / footstepFrequency;
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
    }
}
