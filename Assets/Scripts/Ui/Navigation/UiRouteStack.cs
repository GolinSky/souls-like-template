using System;
using System.Collections.Generic;

namespace SoulsLike.Ui.Navigation
{
    public sealed class UiRouteStack
    {
        private readonly Stack<IUiRoute> _routes = new();
        private readonly Action _showRoot;
        private readonly Action _hideRoot;

        public bool HasOpenRoutes => _routes.Count > 0;

        public UiRouteStack(Action showRoot, Action hideRoot)
        {
            _showRoot = showRoot;
            _hideRoot = hideRoot;
        }

        public void Open(IUiRoute route)
        {
            HideCurrentRouteOrRoot();
            _routes.Push(route);
            route.Show();
        }

        public void Open(IUiRoute route, Action showRoute)
        {
            HideCurrentRouteOrRoot();
            _routes.Push(route);
            showRoute();
        }

        public void CloseTop()
        {
            _routes.Pop().Hide();
            if (_routes.Count > 0)
            {
                _routes.Peek().Show();
            }
            else
            {
                _showRoot();
            }
        }

        public void CloseAll()
        {
            while (_routes.Count > 0)
            {
                _routes.Pop().Hide();
            }
        }

        private void HideCurrentRouteOrRoot()
        {
            if (_routes.Count > 0)
            {
                _routes.Peek().Hide();
            }
            else
            {
                _hideRoot();
            }
        }
    }
}
