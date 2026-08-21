namespace SoulsLike.Entities.Combat.AI
{
    public readonly struct EnemyActionCandidate
    {
        public CharacterActionId ActionId { get; }
        public float MinimumDistance { get; }
        public float MaximumDistance { get; }
        public float MaximumAngle { get; }
        public bool RequiresLineOfSight { get; }
        public float BaseWeight { get; }
        public float Cooldown { get; }
        public float RepetitionPenalty { get; }
        public bool RequiresComboWindow { get; }
        public CharacterActionId RequiredPreviousAction { get; }

        public EnemyActionCandidate(
            CharacterActionId actionId,
            float minimumDistance,
            float maximumDistance,
            float maximumAngle,
            bool requiresLineOfSight,
            float baseWeight,
            float cooldown,
            float repetitionPenalty,
            bool requiresComboWindow = false,
            CharacterActionId requiredPreviousAction = CharacterActionId.None)
        {
            ActionId = actionId;
            MinimumDistance = minimumDistance;
            MaximumDistance = maximumDistance;
            MaximumAngle = maximumAngle;
            RequiresLineOfSight = requiresLineOfSight;
            BaseWeight = baseWeight;
            Cooldown = cooldown;
            RepetitionPenalty = repetitionPenalty;
            RequiresComboWindow = requiresComboWindow;
            RequiredPreviousAction = requiredPreviousAction;
        }
    }
}
