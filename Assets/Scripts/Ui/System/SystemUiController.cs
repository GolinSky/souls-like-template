using System;
using SoulsLike.Services;
using VContainer.Unity;

namespace SoulsLike
{
    public class SystemUiController : UiController, IInitializable, ISystemPresenter, ISystemRoute
    {
        private readonly ICoreGameOrchestrator _coreGameOrchestrator;
        private SystemUi _systemUi;

        public event Action CloseRequested;
        public event Action ResumeRequested;

        public SystemUiController(
            IUiService uiService,
            ICoreGameOrchestrator coreGameOrchestrator) : base(uiService)
        {
            _coreGameOrchestrator = coreGameOrchestrator;
        }

        public void Initialize()
        {
            _systemUi = CreateUi<SystemUi>();
            _systemUi.Initialize(this);
            _systemUi.Hide();
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
