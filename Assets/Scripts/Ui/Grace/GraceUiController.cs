using System;
using SoulsLike.Interactions;
using SoulsLike.Services;
using VContainer.Unity;

namespace SoulsLike.Ui.Grace
{
    public sealed class GraceUiController : UiController,
        IInitializable,
        IDisposable,
        IGameStateObserver,
        IGraceUiPresenter
    {
        private readonly GraceSystem _graceSystem;
        private readonly IGameStateNotifier _gameStateNotifier;

        private GraceUi _view;

        public GraceUiController(
            IUiService uiService,
            GraceSystem graceSystem,
            IGameStateNotifier gameStateNotifier)
            : base(uiService)
        {
            _graceSystem = graceSystem;
            _gameStateNotifier = gameStateNotifier;
        }

        public void Initialize()
        {
            _view = CreateUi<GraceUi>();
            _view.AssignPresenter(this);
            _gameStateNotifier.RegisterObserver(this);
            OnGameStateChanged(_gameStateNotifier.CurrentGameState);
        }

        public void Dispose()
        {
            _gameStateNotifier.UnregisterObserver(this);
        }

        public void Leave()
        {
            _view.Hide();
            _graceSystem.ExitGraceState();
        }

        public void OnGameStateChanged(GameState newState)
        {
            if (newState == GameState.OnGraceSit)
            {
                _view.Show();
                return;
            }

            _view.Hide();
        }
    }
}
