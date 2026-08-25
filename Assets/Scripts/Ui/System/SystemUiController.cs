using System;
using SoulsLike.Services;
using VContainer.Unity;

namespace SoulsLike
{
    public class SystemUiController : UiController, IInitializable, ITickable, ISystemPresenter, ISystemRoute
    {
        private readonly ICoreGameOrchestrator _coreGameOrchestrator;
        private readonly IInputService _inputService;
        private SystemUi _systemUi;

        public event Action CloseRequested;
        public event Action ResumeRequested;

        public SystemUiController(
            IUiService uiService,
            ICoreGameOrchestrator coreGameOrchestrator,
            IInputService inputService) : base(uiService)
        {
            _coreGameOrchestrator = coreGameOrchestrator;
            _inputService = inputService;
        }

        public void Initialize()
        {
            _systemUi = CreateUi<SystemUi>();
            _systemUi.Initialize(this);
            _systemUi.Hide();
        }

        public void Tick()
        {
            if (!_systemUi.IsHidden
                && _inputService.UIActions.Cancel.WasPressedThisFrame())
            {
                CloseRequested?.Invoke();
            }
        }

        public void ResumeGame()
        {
            ResumeRequested?.Invoke();
        }

        public void OpenOptions()
        {
        }

        public void QuitGame()
        {
            _coreGameOrchestrator.QuitGame();
        }

        public void Show()
        {
            _systemUi.Show();
        }

        public void Hide()
        {
            _systemUi.Hide();
        }
    }
}
