using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private EnemyId enemyId;
        [SerializeField] private Transform[] patrolPoints = { };
        [SerializeField] private int randomSeedOffset;

        public EnemyId EnemyId => enemyId;
        public bool HasPatrolPositions => patrolPoints is { Length: > 0 };
        public int RandomSeedOffset => randomSeedOffset;

        public Vector3[] BuildPatrolPositions()
        {
            if (!HasPatrolPositions)
            {
                return System.Array.Empty<Vector3>();
            }

            Vector3[] positions = new Vector3[patrolPoints.Length];
            for (int index = 0; index < patrolPoints.Length; index++)
            {
                positions[index] = patrolPoints[index].position;
            }

            return positions;
        }
    }
}
