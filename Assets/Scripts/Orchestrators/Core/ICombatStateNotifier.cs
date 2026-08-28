namespace SoulsLike.Services
{
    public interface ICombatStateNotifier
    {
        CombatState CurrentCombatState { get; }
        void RegisterObserver(ICombatStateObserver observer);
        void UnregisterObserver(ICombatStateObserver observer);
        void ReportEnemyAggroStarted(long enemyEntityId);
        void ReportEnemyAggroEnded(long enemyEntityId);
    }
}
