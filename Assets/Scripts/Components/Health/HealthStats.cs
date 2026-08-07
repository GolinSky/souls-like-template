using System;

namespace SoulsLike.Entities.Character.Components.Health
{
    [Serializable]
    public struct HealthStats
    {
        public float CurrentHealth;
        public float MaxHealth;
        public bool IsAlive;
    }
}
