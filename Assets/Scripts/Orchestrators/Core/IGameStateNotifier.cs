namespace SoulsLike.Services
{
    public interface IGameStateNotifier
    {
        GameState CurrentGameState { get; }
        void RegisterObserver(IGameStateObserver observer);
        void UnregisterObserver(IGameStateObserver observer);
        void NotifyObservers();
    }
    
    
}
