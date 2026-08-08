using System;

namespace SoulsLike.Entities.Character.Components.Health
{
    [Serializable]
    public struct HealthStats
    {
        public float CurrentHealth;
        public float MaxHealth;
        public float CurrentFocus;
        public float MaxFocus;
        public float CurrentStamina;
        public float MaxStamina;
        public bool IsAlive;
    }
}
