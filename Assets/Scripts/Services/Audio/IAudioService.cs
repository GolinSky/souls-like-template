using SoulsLike.Services;
using SoulsLike.Services.Audio.Data;

namespace SoulsLike.Services.Audio
{
    public interface IAudioService
    {
        float BaseVolume { get; }
        IAudioSettingsData CurrentSettings { get; }
        void AddObserver(IObserver<IAudioSettingsData> observer);
        void RemoveObserver(IObserver<IAudioSettingsData> observer);
        void UpdateSettings(IAudioSettingsData newSettings);
        void ApplySettings(AudioSettingsData settings);
    }
}
