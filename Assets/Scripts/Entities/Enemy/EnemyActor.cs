using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Combat;
using UnityEngine;
using UnityEngine.AI;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyActor : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private MeleeHitboxController meleeHitbox;
        private Vector3[] _patrolPoints = { };
        private LifetimeScope _lifetimeScope;
        
        
        [field:SerializeField] public NavMeshAgent NavMeshAgent { get; private set; }

        public Animator Animator => animator;
        public EnemyBehaviourProfile BehaviourProfile { get; private set; }
        public WeaponMovesetDefinition Moveset { get; private set; }
        public HealthData HealthData { get; private set; }
        public MeleeHitboxController MeleeHitbox => meleeHitbox;
        public Entity Entity { get; private set; }
        public Vector3 HomePosition { get; private set; }
        public IReadOnlyList<Vector3> PatrolPoints => _patrolPoints;
        public bool HasPatrolPositions => _patrolPoints.Length > 0;

        [Inject]
        public void Construct(
            Entity entity,
            EnemyBehaviourProfile behaviourProfile,
            WeaponMovesetDefinition moveset,
            HealthData healthData)
        {
            Entity = entity;
            BehaviourProfile = behaviourProfile;
            Moveset = moveset;
            HealthData = healthData;
        }

        public void ConfigureSpawn(Vector3 homePosition, Vector3[] patrolPoints)
        {
            HomePosition = homePosition;
            _patrolPoints = patrolPoints;
        }

        public void AttachLifetime(LifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public void Despawn()
        {
            Destroy(_lifetimeScope.gameObject);
        }
    }
}
