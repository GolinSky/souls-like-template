using System;
using SoulsLike.Services.Scenes.Data;
using UnityEngine;

namespace SoulsLike.Services.Audio.Data
{
    [CreateAssetMenu(fileName = "AmbienceData", menuName = "Data/AmbienceData")]
    public class AmbienceData : ScriptableObject
    {
        [Serializable]
        public class SceneMusicEntry
        {
            public SceneType sceneType = SceneType.Undefined;
            public MusicType musicType = MusicType.None;
        }

        [Serializable]
        public class MusicEntry
        {
            public MusicType musicType = MusicType.None;
            public AudioClip clip;
        }

        [Serializable]
        public class SfxEntry
        {
            public SfxType sfxType = SfxType.None;
            public AudioClip clip;
        }

        [Header("Scene → Music")]
        [SerializeField] private SceneMusicEntry[] sceneMusic;

        [Header("Music clips")]
        [SerializeField] private MusicEntry[] musicClips;
        [SerializeField, Range(0f, 1f)] private float musicClipVolume = 1f;

        [Header("Sfx clips")]
        [SerializeField] private SfxEntry[] sfxClips;
        [SerializeField, Range(0f, 1f)] private float sfxClipVolume = 1f;

        public float MusicClipVolume => musicClipVolume;
        public float SfxClipVolume => sfxClipVolume;

        public MusicType GetMusicForScene(SceneType sceneType)
        {
            if (sceneMusic == null) return MusicType.None;
            for (var i = 0; i < sceneMusic.Length; i++)
            {
                if (sceneMusic[i] != null && sceneMusic[i].sceneType == sceneType)
                    return sceneMusic[i].musicType;
            }
            return MusicType.None;
        }

        public AudioClip GetMusicClip(MusicType musicType)
        {
            if (musicType == MusicType.None || musicClips == null) return null;
            for (var i = 0; i < musicClips.Length; i++)
            {
                if (musicClips[i] != null && musicClips[i].musicType == musicType)
                    return musicClips[i].clip;
            }
            return null;
        }

        public AudioClip GetSfxClip(SfxType sfxType)
        {
            if (sfxType == SfxType.None || sfxClips == null) return null;
            for (var i = 0; i < sfxClips.Length; i++)
            {
                if (sfxClips[i] != null && sfxClips[i].sfxType == sfxType)
                    return sfxClips[i].clip;
            }
            return null;
        }
    }
}
