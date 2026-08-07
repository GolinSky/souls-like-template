namespace SoulsLike.Services
{
    public interface IGameStateObserver
    {
        void OnGameStateChanged(GameState newState);
    }
}
