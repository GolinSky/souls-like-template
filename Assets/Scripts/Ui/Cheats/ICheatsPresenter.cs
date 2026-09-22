using System.Collections.Generic;
using SoulsLike.Entities.Enemy;

namespace SoulsLike.Ui.Cheats
{
    public interface ICheatsPresenter
    {
        bool IsPlayerInvincible { get; }
        IReadOnlyList<EnemyId> AvailableEnemyIds { get; }

        void HitPlayer();
        void KillPlayer();
        void TogglePlayerInvincibility();
        void ResetOpenGraces();
        void HitAllEnemies();
        void KillAllEnemies();
        void RespawnEnemies();
        void SpawnEnemy(EnemyId enemyId);
    }
}
