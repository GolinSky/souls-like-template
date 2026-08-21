using SoulsLike.Model;
using UnityEngine;
using UnityEngine.Audio;

namespace SoulsLike.Entities.Character.Components
{
    [CreateAssetMenu(fileName = "CharacterAudioData", menuName = "Data/CharacterAudioData")]
    public sealed class CharacterAudioData : Data
    {
        [SerializeField] private AudioResource footstep;
        [SerializeField] private AudioResource landing;
        [SerializeField] private AudioResource hit;
        [SerializeField] private AudioResource swordClash;

        public AudioResource Footstep => footstep;
        public AudioResource Landing => landing;
        public AudioResource Hit => hit;
        public AudioResource SwordClash => swordClash;
    }
}
