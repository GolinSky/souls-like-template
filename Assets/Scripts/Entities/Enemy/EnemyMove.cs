using System;
using SoulsLike.Entities.Combat;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    [Serializable]
    public sealed class EnemyMove
    {
        public enum Usage
        {
            Opener,
            FollowUp,
            Any
        }

        [SerializeField] private CharacterActionDefinition action;
        [SerializeField] private Usage usage = Usage.Any;
        [SerializeField, Min(0f)] private float minimumDistance;
        [SerializeField, Min(0f)] private float maximumDistance = 4f;
        [SerializeField, Range(0f, 180f)] private float maximumAngle = 180f;
        [SerializeField] private bool requiresLineOfSight = true;
        [SerializeField, Min(0f)] private float weight = 1f;
        [SerializeField, Min(0f)] private float cooldown;
        [SerializeField, Range(0f, 1f)] private float repetitionPenalty = 0.5f;

        public CharacterActionDefinition Action => action;
        public Usage MoveUsage => usage;
        public float MinimumDistance => minimumDistance;
        public float MaximumDistance => maximumDistance;
        public float MaximumAngle => maximumAngle;
        public bool RequiresLineOfSight => requiresLineOfSight;
        public float Weight => weight;
        public float Cooldown => cooldown;
        public float RepetitionPenalty => repetitionPenalty;
    }
}
