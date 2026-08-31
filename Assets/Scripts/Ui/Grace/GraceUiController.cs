using System;
using SoulsLike.Interactions;
using SoulsLike.Services;
using SoulsLike.Services.Fade;
using SoulsLike.Ui.Navigation;
using SoulsLike.Ui.Travel;
using VContainer.Unity;

namespace SoulsLike.Ui.Grace
{
    public sealed class GraceUiController : UiController,
        IInitializable,
        ITickable,
        IDisposable,
        IGameStateObserver,
        IGraceUiPresenter,
        IGraceRouteNavigation
    {
        private const float FADE_DURATION = 0.5f;
        private const float FADE_PAUSE_DURATION = 0.5f;

        private readonly GraceSystem _graceSystem;
        private readonly IGameStateNotifier _gameStateNotifier;
        private readonly IFadeService _fadeService;
        private readonly ITravelRoute _travelRoute;
        private readonly IInputService _inputService;

        private GraceUi _view;
        private UiRouteStack _routeStack;
        private bool _isOnGraceSit;
        private bool _isLeaving;

        public GraceUiController(
            IUiService uiService,
            GraceSystem graceSystem,
            IGameStateNotifier gameStateNotifier,
            IFadeService fadeService,
            ITravelRoute travelRoute,
            IInputService inputService)
            : base(uiService)
        {
            _graceSystem = graceSystem;
            _gameStateNotifier = gameStateNotifier;
            _fadeService = fadeService;
            _travelRoute = travelRoute;
            _inputService = inputService;
        }

        public void Initialize()
        {
            _view = CreateUi<GraceUi>();
            _view.AssignPresenter(this);
            _routeStack = new UiRouteStack(_view.Show, _view.Hide);
            _travelRoute.CloseRequested += HandleTravelCloseRequested;
            _gameStateNotifier.RegisterObserver(this);
            OnGameStateChanged(_gameStateNotifier.CurrentGameState);
        }

        public void Dispose()
        {
            _travelRoute.CloseRequested -= HandleTravelCloseRequested;
            _gameStateNotifier.UnregisterObserver(this);
        }

        public void OpenTravel()
        {
            OpenRoute(_travelRoute);
        }

        public void Tick()
        {
            if (!_isOnGraceSit || _isLeaving || !_inputService.UiBackAction.WasPressedThisFrame())
            {
                return;
            }

            _inputService.ConsumeUiBack();
            if (_routeStack.HasOpenRoutes)
            {
                _routeStack.CloseTop();
                return;
            }

            Leave();
        }

        public void Leave()
        {
            if (_isLeaving)
            {
                return;
            }

            _isLeaving = true;
            _view.Hide();
            _graceSystem.ExitGraceState();
        }

        public void OnGameStateChanged(GameState newState)
        {
            if (newState == GameState.OnGraceSit)
            {
                _isOnGraceSit = true;
                _isLeaving = false;
                _fadeService.FadeInOut(FADE_DURATION, FADE_PAUSE_DURATION);

                if (!_routeStack.HasOpenRoutes)
                {
                    _view.Show();
                }

                return;
            }

            if (_isOnGraceSit)
            {
                _isOnGraceSit = false;
                _fadeService.FadeInOut(FADE_DURATION, FADE_PAUSE_DURATION);
            }

            _isLeaving = false;

            _routeStack.CloseAll();
            _view.Hide();
        }

        private void OpenRoute(IGraceRoute route)
        {
            _routeStack.Open(route);
        }

        private void HandleTravelCloseRequested()
        {
            CloseRoute();
        }

        private void CloseRoute()
        {
            _routeStack.CloseTop();
        }
    }
}
