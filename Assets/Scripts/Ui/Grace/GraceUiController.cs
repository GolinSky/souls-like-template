using System;
using System.Collections.Generic;
using SoulsLike.Interactions;
using SoulsLike.Services;
using SoulsLike.Ui.Travel;
using VContainer.Unity;

namespace SoulsLike.Ui.Grace
{
    public sealed class GraceUiController : UiController,
        IInitializable,
        IDisposable,
        IGameStateObserver,
        IGraceUiPresenter,
        IGraceRouteNavigation
    {
        private readonly GraceSystem _graceSystem;
        private readonly IGameStateNotifier _gameStateNotifier;
        private readonly ITravelRoute _travelRoute;
        private readonly Stack<IGraceRoute> _routeStack = new();

        private GraceUi _view;

        public GraceUiController(
            IUiService uiService,
            GraceSystem graceSystem,
            IGameStateNotifier gameStateNotifier,
            ITravelRoute travelRoute)
            : base(uiService)
        {
            _graceSystem = graceSystem;
            _gameStateNotifier = gameStateNotifier;
            _travelRoute = travelRoute;
        }

        public void Initialize()
        {
            _view = CreateUi<GraceUi>();
            _view.AssignPresenter(this);
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

        public void Leave()
        {
            _view.Hide();
            _graceSystem.ExitGraceState();
        }

        public void OnGameStateChanged(GameState newState)
        {
            if (newState == GameState.OnGraceSit)
            {
                if (_routeStack.Count == 0)
                {
                    _view.Show();
                }

                return;
            }

            CloseAllRoutes();
            _view.Hide();
        }

        private void OpenRoute(IGraceRoute route)
        {
            if (_routeStack.Count > 0)
            {
                _routeStack.Peek().Hide();
            }
            else
            {
                _view.Hide();
            }

            _routeStack.Push(route);
            route.Show();
        }

        private void HandleTravelCloseRequested()
        {
            CloseRoute();
        }

        private void CloseRoute()
        {
            _routeStack.Pop().Hide();
            if (_routeStack.Count > 0)
            {
                _routeStack.Peek().Show();
            }
            else
            {
                _view.Show();
            }
        }

        private void CloseAllRoutes()
        {
            while (_routeStack.Count > 0)
            {
                _routeStack.Pop().Hide();
            }
        }
    }
}
