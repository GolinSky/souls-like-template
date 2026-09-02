using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public readonly struct EnemyMemory
    {
        public long? EntityId { get; }
        public Vector3 LastKnownPosition { get; }
        public Vector3 LastKnownLockPoint { get; }
        public float LastConfirmedTime { get; }
        public EnemyStimulusType StimulusType { get; }
        public float ForgetTime { get; }

        public EnemyMemory(TargetObservation observation, EnemyStimulus stimulus)
        {
            EntityId = observation.EntityId;
            LastKnownPosition = observation.Position;
            LastKnownLockPoint = observation.LockPoint;
            LastConfirmedTime = stimulus.Time;
            StimulusType = stimulus.Type;
            ForgetTime = stimulus.ForgetTime;
        }

        public EnemyMemory(EnemyStimulus stimulus)
        {
            EntityId = stimulus.SourceEntityId;
            LastKnownPosition = stimulus.Position;
            LastKnownLockPoint = stimulus.Position;
            LastConfirmedTime = stimulus.Time;
            StimulusType = stimulus.Type;
            ForgetTime = stimulus.ForgetTime;
        }
    }
}
