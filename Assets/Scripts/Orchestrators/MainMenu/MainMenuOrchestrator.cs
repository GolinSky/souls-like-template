using Cysharp.Threading.Tasks;
using SoulsLike.Services;
using SoulsLike.Services.Spawn;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Orchestrators.MainMenu
{
    public class MainMenuOrchestrator: IMainMenuOrchestrator, IInitializable
    {
        private readonly IGameOrchestrator _gameOrchestrator;
        private readonly CharacterSpawnService _characterSpawnService;

        public MainMenuOrchestrator(
            IGameOrchestrator gameOrchestrator,
            CharacterSpawnService characterSpawnService)
        {
            _gameOrchestrator = gameOrchestrator;
            _characterSpawnService = characterSpawnService;
        }
        
        public void Initialize()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void PlayGame()
        {
            _gameOrchestrator.LoadLevel(_characterSpawnService.PrepareResume()).Forget();
        }

        public void OpenOptions()
        {
            //todo: create route ui system
        }

        public void ExitGame()
        {
            _gameOrchestrator.ExitGame();
        }

 
    }
}
