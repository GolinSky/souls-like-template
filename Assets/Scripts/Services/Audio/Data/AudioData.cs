using UnityEngine;

namespace SoulsLike.Services.Audio.Data
{
    [CreateAssetMenu(fileName = "AudioData", menuName = "Data/AudioData")]
    public class AudioData : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)] private float baseVolume = 0.15f;

        public float BaseVolume => baseVolume;
    }
}
