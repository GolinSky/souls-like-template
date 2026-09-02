namespace SoulsLike.Ui.Cheats
{
    public interface ICheatsPresenter
    {
        bool IsPlayerInvincible { get; }

        void HitPlayer();
        void KillPlayer();
        void TogglePlayerInvincibility();
        void ResetOpenGraces();
        void HitAllEnemies();
        void KillAllEnemies();
        void RespawnEnemies();
    }
}
