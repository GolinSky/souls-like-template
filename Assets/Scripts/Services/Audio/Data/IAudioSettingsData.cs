namespace SoulsLike.Services.Audio.Data
{
    public interface IAudioSettingsData
    {
        float MasterVolume { get; }
        float MusicVolume { get; }
        float SfxVolume { get; }
        bool MuteAll { get; }
    }
}
