using System;
using System.Collections.Generic;
using SoulsLike.Services;
using SoulsLike.Services.Audio.Data;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Services.Audio
{
    public class AudioService : IAudioService, IInitializable, IDisposable
    {
        private readonly AudioData _audioData;
        private readonly List<IObserver<IAudioSettingsData>> _observers = new();
        private AudioSettingsData _settingsData = new();

        public AudioService(AudioData audioData)
        {
            _audioData = audioData;
        }

        public float BaseVolume => _audioData != null ? _audioData.BaseVolume : 1f;
        public IAudioSettingsData CurrentSettings => _settingsData;

        public void Initialize()
        {
            NotifyObservers();
        }

        public void Dispose()
        {
            _observers.Clear();
        }

        public void AddObserver(IObserver<IAudioSettingsData> observer)
        {
            if (_observers.Contains(observer))
            {
                Debug.LogError("[AudioService] Observer is already added to audio observer list");
                return;
            }
            _observers.Add(observer);
            observer.UpdateState(_settingsData);
        }

        public void RemoveObserver(IObserver<IAudioSettingsData> observer)
        {
            _observers.Remove(observer);
        }

        public void UpdateSettings(IAudioSettingsData newSettings)
        {
            if (newSettings == null) return;
            _settingsData.MasterVolume = newSettings.MasterVolume;
            _settingsData.MusicVolume = newSettings.MusicVolume;
            _settingsData.SfxVolume = newSettings.SfxVolume;
            _settingsData.MuteAll = newSettings.MuteAll;
            NotifyObservers();
        }

        private void NotifyObservers()
        {
            for (var i = 0; i < _observers.Count; i++)
            {
                _observers[i].UpdateState(_settingsData);
            }
        }
    }
}
