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

        public bool SetMaxFocus;
        public float MaxFocus;

        public bool SetCurrentFocus;
        public float CurrentFocus;

        public bool SetMaxStamina;
        public float MaxStamina;

        public bool SetCurrentStamina;
        public float CurrentStamina;
    }
}
