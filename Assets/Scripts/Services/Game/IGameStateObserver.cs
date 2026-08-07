namespace MultiPlayerTemplate.Services
{
    public interface IGameStateObserver
    {
        void OnGameStateChanged(GameState newState);
    }
}
