using Cysharp.Threading.Tasks;
using SoulsLike.Services;
using SoulsLike.Services.Spawn;
using SoulsLike.Ui.Settings;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Orchestrators.MainMenu
{
    public class MainMenuOrchestrator: IMainMenuOrchestrator, IInitializable
    {
        private readonly IGameOrchestrator _gameOrchestrator;
        private readonly CharacterSpawnService _characterSpawnService;
        private readonly SettingsUiController _settingsUiController;

        public MainMenuOrchestrator(
            IGameOrchestrator gameOrchestrator,
            CharacterSpawnService characterSpawnService,
            SettingsUiController settingsUiController)
        {
            _gameOrchestrator = gameOrchestrator;
            _characterSpawnService = characterSpawnService;
            _settingsUiController = settingsUiController;
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
            _settingsUiController.Show();
        }

        public void ExitGame()
        {
            _gameOrchestrator.ExitGame();
        }

 
    }
}
