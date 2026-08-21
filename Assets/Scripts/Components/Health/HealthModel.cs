using System;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Health
{
    public class HealthModel : Model.Model, IHealthData
    {
        public event Action<HealthStats> OnStatsChanged;
        public event Action<DamageResult> OnDamageApplied;
        public event Action<long> OnDied;
        private long _lastDamageSourceEntityId;

        public float MaxHealth { get; }
        public float StartingHealth { get; }
        public float MaxFocus { get; }
        public float StartingFocus { get; }
        public float MaxStamina { get; }
        public float StartingStamina { get; }
        public bool CanDie { get; }
        public float InvulnerableOnSpawnSeconds { get; }
        public HealthStats Stats { get; private set; }

        public HealthModel(IHealthData healthData)
        {
            MaxHealth = Mathf.Max(1f, healthData.MaxHealth);
            StartingHealth = Mathf.Clamp(healthData.StartingHealth, 0f, MaxHealth);
            MaxFocus = Mathf.Max(1f, healthData.MaxFocus);
            StartingFocus = Mathf.Clamp(healthData.StartingFocus, 0f, MaxFocus);
            MaxStamina = Mathf.Max(1f, healthData.MaxStamina);
            StartingStamina = Mathf.Clamp(healthData.StartingStamina, 0f, MaxStamina);
            CanDie = healthData.CanDie;
            InvulnerableOnSpawnSeconds = Mathf.Max(0f, healthData.InvulnerableOnSpawnSeconds);

            Stats = new HealthStats
            {
                CurrentHealth = StartingHealth,
                MaxHealth = MaxHealth,
                CurrentFocus = StartingFocus,
                MaxFocus = MaxFocus,
                CurrentStamina = StartingStamina,
                MaxStamina = MaxStamina,
                IsAlive = StartingHealth > 0f
            };
        }

        public void ApplyStats(HealthStats stats)
        {
            Stats = stats;
            OnStatsChanged?.Invoke(Stats);
        }

        public void NotifyDamageApplied(DamageResult result)
        {
            _lastDamageSourceEntityId = result.SourceEntityId;
            OnDamageApplied?.Invoke(result);
        }

        public void SetDamageSource(long sourceEntityId)
        {
            _lastDamageSourceEntityId = sourceEntityId;
        }

        public void NotifyDeath()
        {
            OnDied?.Invoke(_lastDamageSourceEntityId);
        }
    }
}
