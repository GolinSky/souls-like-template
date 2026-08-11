using System;
using SoulsLike.Services.Audio.Data;

namespace SoulsLike.Services.Audio
{
    public interface IAmbienceSystem
    {
        /// <summary>
        /// Raised whenever the global audio fade level changes (every step of a
        /// <see cref="DisableAllAudio"/> / <see cref="EnableAllAudio"/> fade, and on instant changes).
        /// The argument is the current scale in the 0..1 range (0 = fully faded out, 1 = full volume).
        /// External audio owners can multiply their own volume by it to fade in lockstep.
        /// </summary>
        event Action<float> VolumeScaleChanged;

        void PlayMusic(MusicType type);
        void StopMusic();
        void PlayAmbience(MusicType type);
        void StopAmbience();
        void PlaySfx(SfxType type);

        /// <summary>
        /// Smoothly fades all audio (music, ambience and newly played SFX) down to silence over
        /// <paramref name="fadeDuration"/> seconds. Nothing is stopped — playback keeps running at
        /// zero volume so <see cref="EnableAllAudio"/> can fade it straight back in.
        /// A duration of 0 (or less) applies the change instantly.
        /// </summary>
        void DisableAllAudio(float fadeDuration);

        /// <summary>
        /// Smoothly fades all audio back up to full volume over <paramref name="fadeDuration"/> seconds.
        /// A duration of 0 (or less) applies the change instantly.
        /// </summary>
        void EnableAllAudio(float fadeDuration);
    }
}
