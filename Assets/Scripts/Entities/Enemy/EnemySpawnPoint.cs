using SoulsLike.Entities.Character.Components.Health;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    //todo: fully rework
    public sealed class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private EnemyActor enemyPrefab;
        [SerializeField] private EnemyBehaviourProfile behaviourProfile;
        [SerializeField] private WeaponMovesetDefinition moveset;
        [SerializeField] private HealthData healthData;
        [SerializeField] private Transform[] patrolPoints = { };
        [SerializeField] private int randomSeedOffset;

        public EnemyActor EnemyPrefab => enemyPrefab;
        public EnemyBehaviourProfile BehaviourProfile => behaviourProfile;
        public WeaponMovesetDefinition Moveset => moveset;
        public HealthData HealthData => healthData;
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
