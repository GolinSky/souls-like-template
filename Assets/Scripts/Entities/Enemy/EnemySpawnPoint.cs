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

        public EnemyActor EnemyPrefab => enemyPrefab;
        public EnemyBehaviourProfile BehaviourProfile => behaviourProfile;
        public WeaponMovesetDefinition Moveset => moveset;
        public HealthData HealthData => healthData;

        public Vector3[] BuildPatrolPositions()
        {
            Vector3[] positions = new Vector3[patrolPoints.Length];
            for (int index = 0; index < patrolPoints.Length; index++)
            {
                positions[index] = patrolPoints[index].position;
            }

            return positions;
        }
    }
}
