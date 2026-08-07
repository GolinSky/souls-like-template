using System;

namespace SoulsLike.Entities.Character.Components.Health
{
    [Serializable]
    public struct HealthStatUpdate
    {
        public bool SetMaxHealth;
        public float MaxHealth;

        public bool SetCurrentHealth;
        public float CurrentHealth;
    }
}
