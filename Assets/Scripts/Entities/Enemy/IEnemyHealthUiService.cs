namespace SoulsLike.Entities.Enemy
{
    public interface IEnemyHealthUiService
    {
        void Track(IEnemyHealthUiSource source);
        void Release(IEnemyHealthUiSource source);
        void NotifyHealthChanged(
            IEnemyHealthUiSource source,
            float currentHealth,
            float maxHealth);
        void NotifyVisibilityChanged(IEnemyHealthUiSource source, bool isVisible);
    }
}
