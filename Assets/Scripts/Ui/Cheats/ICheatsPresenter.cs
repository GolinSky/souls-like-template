namespace SoulsLike.Ui.Cheats
{
    public interface ICheatsPresenter
    {
        void HitPlayer();
        void KillPlayer();
        void ResetOpenGraces();
        void HitAllEnemies();
        void KillAllEnemies();
        void RespawnEnemies();
    }
}
