using System;

namespace SoulsLike.Entities.Character.Components.Health
{
    //todo: make getters 
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

        public float DisplayCurrentStamina =>
            CurrentStamina <= 0f
                ? 0f
                : CurrentStamina > MaxStamina
                    ? MaxStamina
                    : CurrentStamina;
    }
}
