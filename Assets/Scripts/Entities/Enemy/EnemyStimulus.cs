using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public readonly struct EnemyStimulus
    {
        public EnemyStimulusType Type { get; }
        public Vector3 Position { get; }
        public long? SourceEntityId { get; }
        public float Strength { get; }
        public int Priority { get; }
        public float Time { get; }
        public float ForgetTime { get; }

        public EnemyStimulus(
            EnemyStimulusType type,
            Vector3 position,
            long? sourceEntityId,
            float strength,
            int priority,
            float time,
            float forgetTime)
        {
            Type = type;
            Position = position;
            SourceEntityId = sourceEntityId;
            Strength = strength;
            Priority = priority;
            Time = time;
            ForgetTime = forgetTime;
        }
    }
}
