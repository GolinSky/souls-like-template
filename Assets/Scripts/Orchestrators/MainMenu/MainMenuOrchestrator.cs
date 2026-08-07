using SoulsLike.Services;
using SoulsLike.Services.Scenes.Data;
using VContainer.Unity;

namespace SoulsLike.Orchestrators.MainMenu
{
    public class MainMenuOrchestrator: IMainMenuOrchestrator, IInitializable
    {
        private readonly IGameOrchestrator _gameOrchestrator;

        public MainMenuOrchestrator(IGameOrchestrator gameOrchestrator)
        {
            _gameOrchestrator = gameOrchestrator;
        }
        
        public void Initialize()
        {
        }
        
        public void PlayGame()
        {
            _gameOrchestrator.LoadLevel(SceneType.DefaultLocation);
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