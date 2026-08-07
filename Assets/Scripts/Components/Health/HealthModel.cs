using System;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Health
{
    public class HealthModel : Model.Model, IHealthData
    {
        public float MaxHealth { get; }
        public float StartingHealth { get; }
        public bool CanDie { get; }
        public float InvulnerableOnSpawnSeconds { get; }

        public HealthModel(IHealthData healthData)
        {
            if (healthData == null)
            {
                throw new ArgumentNullException(nameof(healthData));
            }

            MaxHealth = Mathf.Max(1f, healthData.MaxHealth);
            StartingHealth = Mathf.Clamp(healthData.StartingHealth, 0f, MaxHealth);
            CanDie = healthData.CanDie;
            InvulnerableOnSpawnSeconds = Mathf.Max(0f, healthData.InvulnerableOnSpawnSeconds);
        }
    }
}
