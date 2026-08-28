namespace SoulsLike.Services
{
    public interface ICombatStateObserver
    {
        void OnCombatStateChanged(CombatState newState);
    }
}
