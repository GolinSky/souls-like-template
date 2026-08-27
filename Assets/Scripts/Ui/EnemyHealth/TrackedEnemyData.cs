using SoulsLike.Entities.Enemy;

namespace SoulsLike.Ui.EnemyHealth
{
    public sealed class TrackedEnemyData
    {
        public IEnemyHealthUiSource Source { get; }
        public EnemyHealthBarUi Bar { get; }
        public bool IsVisible { get; set; }

        public TrackedEnemyData(
            IEnemyHealthUiSource source,
            EnemyHealthBarUi bar)
        {
            Source = source;
            Bar = bar;
        }
    }
}
