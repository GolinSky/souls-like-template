using System;
using UnityEngine;

namespace SoulsLike.Entities.Character
{
    [Serializable]
    public struct CharacterAttributeStats
    {
        [field: SerializeField, Min(1)] public int Vigor { get; private set; }
        [field: SerializeField, Min(1)] public int Mind { get; private set; }
        [field: SerializeField, Min(1)] public int Endurance { get; private set; }
        [field: SerializeField, Min(1)] public int Strength { get; private set; }
        [field: SerializeField, Min(1)] public int Dexterity { get; private set; }
        [field: SerializeField, Min(1)] public int Intelligence { get; private set; }
        [field: SerializeField, Min(1)] public int Faith { get; private set; }
        [field: SerializeField, Min(1)] public int Arcane { get; private set; }
    }
}
