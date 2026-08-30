using Cysharp.Threading.Tasks;
using SoulsLike.Services.Scenes.Data;

namespace SoulsLike.Services.Travel
{
    public sealed class TravelService
    {
        private readonly IGameOrchestrator _gameOrchestrator;

        public TravelService(IGameOrchestrator gameOrchestrator)
        {
            _gameOrchestrator = gameOrchestrator;
        }

        public UniTask Travel(SceneType sceneType) => _gameOrchestrator.LoadLevel(sceneType);
    }
}
