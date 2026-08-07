using SoulsLike.Model;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Health
{
    public interface IHealthData
    {
        float MaxHealth { get; }
        float StartingHealth { get; }
        bool CanDie { get; }
        float InvulnerableOnSpawnSeconds { get; }
    }

    [CreateAssetMenu(fileName = "HealthData", menuName = "Data/Health/HealthData")]
    public class HealthData : Data, IHealthData
    {
        [Header("Health")]
        [Min(1f)]
        [field: SerializeField]
        public float MaxHealth { get; private set; } = 100f;

        [Min(0f)]
        [field: SerializeField]
        public float StartingHealth { get; private set; } = 100f;

        [field: SerializeField]
        public bool CanDie { get; private set; } = true;

        [Min(0f)]
        [field: SerializeField]
        public float InvulnerableOnSpawnSeconds { get; private set; }
    }
}
