namespace SoulsLike.Entities.Enemy
{
    public enum EnemyInterruptReason
    {
        Reaction,
        Death,
        CriticalVictim,
        ReactionComplete,
        CriticalComplete,
        LostTarget,
        AnimatorMismatch,
        AnimatorEntryTimeout,
        Disabled,
        Despawned
    }
}
