using System;
using Cysharp.Threading.Tasks;
using SoulsLike.Services;
using SoulsLike.Services.GenericPopupService;
using SoulsLike.Services.Scenes.Data;
using SoulsLike.Services.Travel;
using SoulsLike.Services.Travel.Data;
using VContainer.Unity;

namespace SoulsLike.Ui.Travel
{
    public sealed class TravelUiController : UiController,
        IInitializable,
        ITickable,
        ITravelRoute,
        ITravelUiPresenter
    {
        private readonly IInputService _inputService;
        private readonly TravelService _travelService;
        private readonly LocationData _locationData;
        private readonly IGenericPopupService _genericPopupService;

        private TravelUi _view;
        private LocationEntry _selectedLocation;
        private bool _isConfirmationPending;

        public event Action CloseRequested;

        public TravelUiController(
            IUiService uiService,
            IInputService inputService,
            TravelService travelService,
            LocationData locationData,
            IGenericPopupService genericPopupService)
            : base(uiService)
        {
            _inputService = inputService;
            _travelService = travelService;
            _locationData = locationData;
            _genericPopupService = genericPopupService;
        }

        public void Initialize()
        {
            _view = CreateUi<TravelUi>();
            _view.AssignPresenter(this);
            _view.ShowLocations(_locationData.Locations);
            _view.Hide();
        }

        public void Show()
        {
            _view.Show();
            OnLocationSelection(_locationData.Locations[0].Id);
        }

        public void Tick()
        {
            if (_view.IsHidden || _isConfirmationPending)
            {
                return;
            }

            if (_inputService.UIActions.Cancel.WasPressedThisFrame())
            {
                CloseRequested?.Invoke();
            }
        }

        public void Hide()
        {
            _view.Hide();
        }

        public void OnLocationSelection(SceneType locationId)
        {
            if (_isConfirmationPending)
            {
                return;
            }

            _selectedLocation = _locationData.GetLocation(locationId);
            _view.ShowGraces(
                _selectedLocation.Id,
                _selectedLocation.DisplayName,
                _selectedLocation.Graces);
        }

        public void OnGraceSelection(GraceId graceId)
        {
            if (_isConfirmationPending)
            {
                return;
            }

            GraceData grace = _selectedLocation.GetGrace(graceId);
            SceneType sceneType = _selectedLocation.Id;
            _isConfirmationPending = true;
            _genericPopupService.ShowAcceptPopup(
                "Travel",
                $"Travel to {grace.DisplayName}?",
                accepted =>
                {
                    _isConfirmationPending = false;
                    if (accepted)
                    {
                        _travelService.Travel(sceneType).Forget();
                    }
                });
        }
    }
}
