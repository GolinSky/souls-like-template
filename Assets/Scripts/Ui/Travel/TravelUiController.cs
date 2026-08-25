using System;
using SoulsLike.Services;
using SoulsLike.Services.Travel;
using VContainer.Unity;

namespace SoulsLike.Ui.Travel
{
    public sealed class TravelUiController : UiController,
        IInitializable,
        ITickable,
        ITravelRoute
    {
        private readonly IInputService _inputService;
        private readonly TravelService _travelService;

        private TravelUi _view;

        public event Action CloseRequested;

        public TravelUiController(
            IUiService uiService,
            IInputService inputService,
            TravelService travelService)
            : base(uiService)
        {
            _inputService = inputService;
            _travelService = travelService;
        }

        public void Initialize()
        {
            _view = CreateUi<TravelUi>();
            _view.Hide();
        }

        public void Tick()
        {
            if (_view.IsHidden)
            {
                return;
            }

            if (_inputService.UIActions.Cancel.WasPressedThisFrame())
            {
                CloseRequested?.Invoke();
            }
        }

        public void Show()
        {
            _view.Show();
        }

        public void Hide()
        {
            _view.Hide();
        }
    }
}
