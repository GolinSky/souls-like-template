using UnityEngine;
using UnityEngine.Serialization;

namespace SoulsLike.Entities.Enemy
{
    [CreateAssetMenu(fileName = "EnemyBehaviourProfile", menuName = "Enemy/Behaviour Profile")]
    public sealed class EnemyBehaviourProfile : ScriptableObject
    {
        [Header("Awareness")]
        [SerializeField, Min(0f)] private float perceptionRange = 12f;
        [SerializeField, Range(0f, 360f)] private float fieldOfView = 120f;
        [SerializeField, Range(0f, 360f)] private float verticalFieldOfView = 180f;
        [SerializeField, Min(0f)] private float closeAwarenessRange = 2.6f;
        [SerializeField, Min(0f)] private float eyeHeight = 1.5f;
        [SerializeField] private LayerMask lineOfSightMask = ~0;
        [SerializeField, Min(0f)] private float sightConfirmationSeconds;
        [FormerlySerializedAs("targetMemorySeconds")]
        [SerializeField, Min(0f)] private float sightForgetSeconds = 4f;
        [SerializeField, Min(0f)] private float soundForgetSeconds = 3f;
        [SerializeField, Min(0f)] private float damageForgetSeconds = 6f;
        [SerializeField, Min(0f)] private float allyForgetSeconds = 4f;
        [SerializeField] private bool sharesAllyAlerts;
        [SerializeField, Min(0f)] private float reactionDelayMin = 0.15f;
        [SerializeField, Min(0f)] private float reactionDelayMax = 0.35f;

        [Header("Home and Patrol")]
        [SerializeField] private EnemyActivationMode activationMode;
        [SerializeField, Min(0f)] private float softLeashDistance = 16f;
        [FormerlySerializedAs("leashDistance")]
        [SerializeField, Min(0f)] private float hardLeashDistance = 20f;
        [SerializeField, Min(0f)] private float returnHysteresis = 2f;
        [SerializeField, Min(0f)] private float patrolWaitSeconds = 1.5f;
        [SerializeField, Min(0f)] private float searchSeconds = 4f;
        [SerializeField, Min(0f)] private float searchTurnSpeed = 90f;
        [SerializeField, Range(0, 2)] private int searchPointCount = 2;
        [SerializeField, Min(0f)] private float searchPointRadius = 2f;
        [SerializeField, Min(0f)] private float searchPauseSeconds = 0.75f;
        [SerializeField, Min(0f)] private float arrivalDistance = 0.35f;

        [Header("Combat Decisions")]
        [SerializeField, Min(0.01f)] private float decisionInterval = 0.15f;
        [SerializeField, Min(0f)] private float preferredRangeMin = 1.4f;
        [SerializeField, Min(0f)] private float preferredRangeMax = 2.6f;
        [SerializeField, Min(0f)] private float strafeDistance = 1.5f;
        [FormerlySerializedAs("waitSeconds")]
        [SerializeField, Min(0f)] private float postActionDecisionDelaySeconds = 0.35f;
        [SerializeField, Min(0f)] private float decisionJitterSeconds;
        [SerializeField, Min(0f)] private float firstAttackHesitationMin;
        [SerializeField, Min(0f)] private float firstAttackHesitationMax;
        [SerializeField] private bool usesPressureSlot;
        [SerializeField] private bool remainsStationary;
        [SerializeField] private bool locksFacing;
        [SerializeField, Min(0)] private int maximumAttackCount;
        [SerializeField] private int randomSeed = 1;

        [Header("Traversal")]
        [SerializeField] private bool canUseLadders;

        public float PerceptionRange => perceptionRange;
        public float FieldOfView => fieldOfView;
        public float VerticalFieldOfView => verticalFieldOfView;
        public float CloseAwarenessRange => closeAwarenessRange;
        public float EyeHeight => eyeHeight;
        public LayerMask LineOfSightMask => lineOfSightMask;
        public float SightConfirmationSeconds => sightConfirmationSeconds;
        public float SightForgetSeconds => sightForgetSeconds;
        public float SoundForgetSeconds => soundForgetSeconds;
        public float DamageForgetSeconds => damageForgetSeconds;
        public float AllyForgetSeconds => allyForgetSeconds;
        public bool SharesAllyAlerts => sharesAllyAlerts;
        public float ReactionDelayMin => reactionDelayMin;
        public float ReactionDelayMax => Mathf.Max(reactionDelayMin, reactionDelayMax);
        public EnemyActivationMode ActivationMode => activationMode;
        public float HardLeashDistance => Mathf.Max(softLeashDistance, hardLeashDistance);
        public float SoftLeashDistance => Mathf.Min(softLeashDistance, HardLeashDistance);
        public float ReturnHysteresis => returnHysteresis;
        public float ReturnHomeDistance => Mathf.Max(0f, SoftLeashDistance - returnHysteresis);
        public float PatrolWaitSeconds => patrolWaitSeconds;
        public float SearchSeconds => searchSeconds;
        public float SearchTurnSpeed => searchTurnSpeed;
        public int SearchPointCount => Mathf.Clamp(searchPointCount, 0, 2);
        public float SearchPointRadius => searchPointRadius;
        public float SearchPauseSeconds => searchPauseSeconds;
        public float ArrivalDistance => arrivalDistance;
        public float DecisionInterval => decisionInterval;
        public float PreferredRangeMin => preferredRangeMin;
        public float PreferredRangeMax => Mathf.Max(preferredRangeMin, preferredRangeMax);
        public float StrafeDistance => strafeDistance;
        public float PostActionDecisionDelaySeconds => postActionDecisionDelaySeconds;
        public float WaitSeconds => postActionDecisionDelaySeconds;
        public float DecisionJitterSeconds => decisionJitterSeconds;
        public float FirstAttackHesitationMin => firstAttackHesitationMin;
        public float FirstAttackHesitationMax => Mathf.Max(
            firstAttackHesitationMin,
            firstAttackHesitationMax);
        public bool UsesPressureSlot => usesPressureSlot;
        public bool RemainsStationary => remainsStationary;
        public bool LocksFacing => locksFacing;
        public int MaximumAttackCount => maximumAttackCount;
        public int RandomSeed => randomSeed;
        public bool CanUseLadders => canUseLadders;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (lineOfSightMask.value == 0)
            {
                Debug.LogError($"[{nameof(EnemyBehaviourProfile)}] LineOfSightMask cannot be zero.", this);
            }
        }
#endif
    }
}
