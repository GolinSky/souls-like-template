using System;
using SoulsLike.Entities.Enemy;

namespace SoulsLike.Ui.EnemyHealth
{
    public sealed class TrackedEnemyData
    {
        public IEnemyHealthUiSource Source { get; }
        public EnemyHealthBarUi Bar { get; }
        public Action<float, float> HealthChangedHandler { get; }

        public TrackedEnemyData(
            IEnemyHealthUiSource source,
            EnemyHealthBarUi bar,
            Action<float, float> healthChangedHandler)
        {
            Source = source;
            Bar = bar;
            HealthChangedHandler = healthChangedHandler;
        }
    }
}
