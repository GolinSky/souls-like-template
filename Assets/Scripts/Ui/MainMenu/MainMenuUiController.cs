using SoulsLike.Orchestrators.MainMenu;
using SoulsLike.Services;
using VContainer.Unity;

namespace SoulsLike.Ui.MainMenu
{
    public class MainMenuUiController: UiController, IInitializable, IMainMenuPresenter
    {
        private readonly IMainMenuOrchestrator _mainMenuOrchestrator;
        private MainMenuUi _mainMenuUi;

        public MainMenuUiController(IUiService uiService, IMainMenuOrchestrator mainMenuOrchestrator) : base(uiService)
        {
            _mainMenuOrchestrator = mainMenuOrchestrator;
        }


        public void Initialize()
        {
            _mainMenuUi = CreateUi<MainMenuUi>();
            _mainMenuUi.AssignPresenter(this);
            _mainMenuUi.Show();
        }

        public void PlayGame()
        {
            _mainMenuOrchestrator.PlayGame();
        }

        public void OpenOptions()
        {
            _mainMenuOrchestrator.OpenOptions();
        }

        public void ExitGame()
        {
            _mainMenuOrchestrator.ExitGame();
        }
    }
}