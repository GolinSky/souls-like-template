using SoulsLike.Model;
using UnityEngine;

namespace SoulsLike.Entities.Character
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "Data/CharacterData")]
    public sealed class CharacterData : Data
    {
        [SerializeField] private CharacterAttributeStats attributes;
        [SerializeField, Min(0)] private int startingCurrency;

        public CharacterAttributeStats Attributes => attributes;
        public int StartingCurrency => startingCurrency;
    }
}
