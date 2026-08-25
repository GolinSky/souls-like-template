using SoulsLike.Services;
using VContainer.Unity;

namespace SoulsLike
{
    public class PauseMenuUiController : UiController, IInitializable, ITickable, IPauseMenuPresenter
    {
        private readonly ICoreGameOrchestrator _coreGameOrchestrator;
        private readonly IInputService _inputService;
        private PauseMenuUi _pauseMenuUi;

        public PauseMenuUiController(
            IUiService uiService,
            ICoreGameOrchestrator coreGameOrchestrator,
            IInputService inputService) : base(uiService)
        {
            _coreGameOrchestrator = coreGameOrchestrator;
            _inputService = inputService;
        }

        public void Initialize()
        {
            _pauseMenuUi = CreateUi<PauseMenuUi>();
            _pauseMenuUi.Initialize(this);
        }

        public void Tick()
        {
            if (!_inputService.CharacterActions.Pause.WasPressedThisFrame()) return;

            if (_coreGameOrchestrator.CurrentGameState == GameState.Idle)
            {
                PauseGame();
            }
            else if (_coreGameOrchestrator.CurrentGameState == GameState.Paused
                && !_pauseMenuUi.IsHidden)
            {
                ResumeGame();
            }
        }

        public void ResumeGame()
        {
            _pauseMenuUi.Hide();
            _coreGameOrchestrator.ResumeGame();
        }

        public void OpenOptions()
        {
        }

        public void QuitGame()
        {
            _coreGameOrchestrator.QuitGame();
        }

        private void PauseGame()
        {
            _pauseMenuUi.Show();
            _coreGameOrchestrator.PauseGame();
        }
    }
}
