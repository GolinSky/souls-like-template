using SoulsLike.Entities.Combat;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public enum EnemyGoal
    {
        Dormant,
        Idle,
        Patrol,
        Investigate,
        Combat,
        Search,
        ReturnHome,
        Dead
    }

    public enum EnemyIntentKind
    {
        Move,
        Face,
        ExecuteAction,
        Wait
    }

    public enum EnemyActionStatus
    {
        Running,
        Completed,
        Interrupted,
        Failed
    }

    public enum EnemyActionPhase
    {
        None,
        Windup,
        Active,
        Recovery
    }

    public readonly struct EnemyIntent
    {
        public EnemyIntentKind Kind { get; }
        public Vector3 Position { get; }
        public CharacterActionDefinition Action { get; }

        public EnemyIntent(
            EnemyIntentKind kind,
            Vector3 position,
            CharacterActionDefinition action = null)
        {
            Kind = kind;
            Position = position;
            Action = action;
        }
    }

    public readonly struct EnemyMemory
    {
        public long EntityId { get; }
        public Vector3 LastKnownPosition { get; }
        public Vector3 LastKnownForward { get; }
        public float LastSeenTime { get; }
        public bool HadLineOfSight { get; }

        public EnemyMemory(
            long entityId,
            Vector3 position,
            Vector3 forward,
            float time,
            bool hadLineOfSight)
        {
            EntityId = entityId;
            LastKnownPosition = position;
            LastKnownForward = forward;
            LastSeenTime = time;
            HadLineOfSight = hadLineOfSight;
        }
    }
}
