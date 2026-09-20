using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemySpawnGroup : MonoBehaviour
    {
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool respawnOnGrace = true;
        [SerializeField] private bool respawnOnGameEnded = true;
        [SerializeField, Min(1)] private int maxPressureSlots = 1;
        [SerializeField, Min(0.01f)] private float pressureSlotTimeoutSeconds = 3f;

        public bool SpawnOnStart => spawnOnStart;
        public bool RespawnOnGrace => respawnOnGrace;
        public bool RespawnOnGameEnded => respawnOnGameEnded;
        public int MaxPressureSlots => maxPressureSlots;
        public float PressureSlotTimeoutSeconds => pressureSlotTimeoutSeconds;

        public event System.Action<EnemySpawnGroup> Enabled;
        public event System.Action<EnemySpawnGroup> Disabled;

        private void OnEnable()
        {
            Enabled?.Invoke(this);
        }

        private void OnDisable()
        {
            Disabled?.Invoke(this);
        }
    }
}
