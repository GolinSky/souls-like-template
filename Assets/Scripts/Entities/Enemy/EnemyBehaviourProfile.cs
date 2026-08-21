using System;
using SoulsLike.Entities.Combat;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    [CreateAssetMenu(fileName = "EnemyBehaviourProfile", menuName = "Enemy/Behaviour Profile")]
    public sealed class EnemyBehaviourProfile : ScriptableObject
    {
        [Header("Awareness")]
        [SerializeField, Min(0f)] private float perceptionRange = 12f;
        [SerializeField, Range(0f, 360f)] private float fieldOfView = 120f;
        [SerializeField, Min(0f)] private float eyeHeight = 1.5f;
        [SerializeField] private LayerMask lineOfSightMask = ~0;
        [SerializeField, Min(0f)] private float targetMemorySeconds = 4f;
        [SerializeField, Min(0f)] private float reactionDelayMin = 0.15f;
        [SerializeField, Min(0f)] private float reactionDelayMax = 0.35f;

        [Header("Home and Patrol")]
        [SerializeField] private bool startsDormant;
        [SerializeField, Min(0f)] private float leashDistance = 20f;
        [SerializeField, Min(0f)] private float patrolWaitSeconds = 1.5f;
        [SerializeField, Min(0f)] private float searchSeconds = 4f;
        [SerializeField, Min(0f)] private float searchTurnSpeed = 90f;
        [SerializeField, Min(0f)] private float arrivalDistance = 0.35f;

        [Header("Combat Decisions")]
        [SerializeField, Min(0.01f)] private float decisionInterval = 0.15f;
        [SerializeField, Min(0f)] private float preferredRangeMin = 1.4f;
        [SerializeField, Min(0f)] private float preferredRangeMax = 2.6f;
        [SerializeField, Min(0f)] private float strafeDistance = 1.5f;
        [SerializeField, Min(0f)] private float waitSeconds = 0.35f;
        [SerializeField] private int randomSeed = 1;
        [SerializeField] private AiActionRule[] actionRules = { };

        public float PerceptionRange => perceptionRange;
        public float FieldOfView => fieldOfView;
        public float EyeHeight => eyeHeight;
        public LayerMask LineOfSightMask => lineOfSightMask;
        public float TargetMemorySeconds => targetMemorySeconds;
        public float ReactionDelayMin => reactionDelayMin;
        public float ReactionDelayMax => Mathf.Max(reactionDelayMin, reactionDelayMax);
        public bool StartsDormant => startsDormant;
        public float LeashDistance => leashDistance;
        public float PatrolWaitSeconds => patrolWaitSeconds;
        public float SearchSeconds => searchSeconds;
        public float SearchTurnSpeed => searchTurnSpeed;
        public float ArrivalDistance => arrivalDistance;
        public float DecisionInterval => decisionInterval;
        public float PreferredRangeMin => preferredRangeMin;
        public float PreferredRangeMax => Mathf.Max(preferredRangeMin, preferredRangeMax);
        public float StrafeDistance => strafeDistance;
        public float WaitSeconds => waitSeconds;
        public int RandomSeed => randomSeed;
        public AiActionRule[] ActionRules => actionRules;
    }

    [Serializable]
    public sealed class AiActionRule
    {
        [SerializeField] private CharacterActionDefinition action;
        [SerializeField, Min(0f)] private float minimumDistance;
        [SerializeField, Min(0f)] private float maximumDistance = 4f;
        [SerializeField, Range(0f, 180f)] private float maximumAngle = 180f;
        [SerializeField] private bool requiresLineOfSight = true;
        [SerializeField, Min(0f)] private float baseWeight = 1f;
        [SerializeField, Min(0f)] private float cooldown;
        [SerializeField, Range(0f, 1f)] private float repetitionPenalty = 0.5f;
        [SerializeField] private bool requiresComboWindow;
        [SerializeField] private CharacterActionId requiredPreviousAction;

        public CharacterActionDefinition Action => action;
        public float MinimumDistance => minimumDistance;
        public float MaximumDistance => maximumDistance;
        public float MaximumAngle => maximumAngle;
        public bool RequiresLineOfSight => requiresLineOfSight;
        public float BaseWeight => baseWeight;
        public float Cooldown => cooldown;
        public float RepetitionPenalty => repetitionPenalty;
        public bool RequiresComboWindow => requiresComboWindow;
        public CharacterActionId RequiredPreviousAction => requiredPreviousAction;
    }
}
