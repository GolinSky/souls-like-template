using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public readonly struct EnemySpawnData
    {
        public EnemySpawnData(
            Vector3 position,
            Quaternion rotation,
            Vector3[] patrolPoints,
            int randomSeedOffset)
        {
            Position = position;
            Rotation = rotation;
            PatrolPoints = (Vector3[])patrolPoints.Clone();
            RandomSeedOffset = randomSeedOffset;
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3[] PatrolPoints { get; }
        public int RandomSeedOffset { get; }
    }
}
