using System;
using System.Collections.Generic;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.Combat;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using VContainer;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyActor : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private MeleeHitboxController meleeHitbox;
        private Vector3[] _patrolPoints = { };
        private GameObject _lifetimeRoot;
        private bool _hasLifetimeRoot;
        private bool _isDespawned;
        
        
        [FormerlySerializedAs("<NavMeshAgent>k__BackingField")]
        [SerializeField] private NavMeshAgent navMeshAgent;

        public NavMeshAgent NavMeshAgent => navMeshAgent;
        public EnemyBehaviourProfile BehaviourProfile { get; private set; }
        public WeaponMovesetDefinition Moveset { get; private set; }
        public Entity Entity { get; private set; }
        public Vector3 HomePosition { get; private set; }
        public int RandomSeedOffset { get; private set; }
        public IReadOnlyList<Vector3> PatrolPoints => _patrolPoints;
        public bool HasPatrolPositions => _patrolPoints.Length > 0;
        public event Action<EnemyActor> Despawned;

        [Inject]
        public void Construct(
            Entity entity,
            EnemyBehaviourProfile behaviourProfile,
            WeaponMovesetDefinition moveset)
        {
            Entity = entity;
            BehaviourProfile = behaviourProfile;
            Moveset = moveset;
        }

        public void ConfigureSpawn(
            Vector3 homePosition,
            Vector3[] patrolPoints,
            int randomSeedOffset)
        {
            HomePosition = homePosition;
            _patrolPoints = patrolPoints;
            RandomSeedOffset = randomSeedOffset;
        }

        public void AttachLifetimeRoot(GameObject lifetimeRoot)
        {
            if (_hasLifetimeRoot)
            {
                throw new InvalidOperationException(
                    $"{nameof(EnemyActor)} lifetime root is already attached.");
            }

            _lifetimeRoot = lifetimeRoot;
            _hasLifetimeRoot = true;
        }

        public void Despawn()
        {
            if (_isDespawned)
            {
                return;
            }

            _isDespawned = true;
            try
            {
                Despawned?.Invoke(this);
            }
            finally
            {
                Destroy(_lifetimeRoot);
            }
        }
    }
}
