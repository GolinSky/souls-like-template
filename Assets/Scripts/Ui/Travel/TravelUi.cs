using System.Collections.Generic;
using SoulsLike.Services.Scenes.Data;
using SoulsLike.Services.Travel.Data;
using SoulsLike.Ui.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Travel
{
    public sealed class TravelUi : BaseUi
    {
        [SerializeField] private TMP_Text locationNameText;
        [SerializeField] private RectTransform locationContainer;
        [SerializeField] private RectTransform graceContainer;
        [SerializeField] private ToggleGroup locationToggleGroup;
        [SerializeField] private TravelLocationView locationViewPrefab;
        [SerializeField] private TravelGraceView graceViewPrefab;

        private readonly List<TravelLocationView> _locationViews = new();
        private readonly List<TravelGraceView> _graceViews = new();
        private ITravelUiPresenter _presenter;

        public void AssignPresenter(ITravelUiPresenter presenter)
        {
            _presenter = presenter;
        }

        public void ShowLocations(IReadOnlyList<LocationEntry> locations)
        {
            ClearLocations();
            ClearGraces();

            foreach (LocationEntry location in locations)
            {
                TravelLocationView locationView = Instantiate(locationViewPrefab, locationContainer);
                locationView.Bind(location.Id, location.DisplayName, locationToggleGroup);
                locationView.Selected += HandleLocationSelected;
                _locationViews.Add(locationView);
            }
        }

        public void ShowGraces(
            SceneType selectedLocationId,
            string locationDisplayName,
            IReadOnlyList<GraceData> graces)
        {
            locationNameText.text = locationDisplayName;
            SetSelectedLocation(selectedLocationId);
            ClearGraces();

            foreach (GraceData grace in graces)
            {
                TravelGraceView graceView = Instantiate(graceViewPrefab, graceContainer);
                graceView.Bind(grace.Id, grace.DisplayName);
                graceView.Selected += HandleGraceSelected;
                _graceViews.Add(graceView);
            }
        }

        private void OnDestroy()
        {
            ClearLocations();
            ClearGraces();
        }

        private void SetSelectedLocation(SceneType selectedLocationId)
        {
            foreach (TravelLocationView locationView in _locationViews)
            {
                locationView.SetSelected(locationView.LocationId == selectedLocationId);
            }
        }

        private void HandleLocationSelected(SceneType locationId)
        {
            _presenter.OnLocationSelection(locationId);
        }

        private void HandleGraceSelected(GraceId graceId)
        {
            _presenter.OnGraceSelection(graceId);
        }

        private void ClearLocations()
        {
            foreach (TravelLocationView locationView in _locationViews)
            {
                locationView.Selected -= HandleLocationSelected;
                Destroy(locationView.gameObject);
            }

            _locationViews.Clear();
        }

        private void ClearGraces()
        {
            foreach (TravelGraceView graceView in _graceViews)
            {
                graceView.Selected -= HandleGraceSelected;
                Destroy(graceView.gameObject);
            }

            _graceViews.Clear();
        }
    }
}
