using Cysharp.Threading.Tasks;
using SoulsLike.Services.Spawn;
using SoulsLike.Services.Travel.Data;

namespace SoulsLike.Services.Travel
{
    public sealed class TravelService
    {
        private readonly IGameOrchestrator _gameOrchestrator;
        private readonly CharacterSpawnService _characterSpawnService;

        public TravelService(
            IGameOrchestrator gameOrchestrator,
            CharacterSpawnService characterSpawnService)
        {
            _gameOrchestrator = gameOrchestrator;
            _characterSpawnService = characterSpawnService;
        }

        public UniTask Travel(GraceId graceId) =>
            _gameOrchestrator.LoadLevel(_characterSpawnService.PrepareGraceSpawn(graceId));
    }
}
