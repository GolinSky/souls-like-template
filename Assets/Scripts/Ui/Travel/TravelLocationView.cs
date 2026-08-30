using System;
using SoulsLike.Services.Scenes.Data;
using UI.Base;
using UnityEngine;
using UnityEngine.UI;

namespace SoulsLike.Ui.Travel
{
    public sealed class TravelLocationView : MonoBehaviour
    {
        [SerializeField] private CustomButtonToggle button;

        private SceneType _locationId;

        public SceneType LocationId => _locationId;
        public event Action<SceneType> Selected;

        public void Bind(SceneType locationId, string displayName, ToggleGroup toggleGroup)
        {
            _locationId = locationId;
            button.group = toggleGroup;
            button.SetText(displayName);
            button.onValueChanged.RemoveListener(HandleValueChanged);
            button.onValueChanged.AddListener(HandleValueChanged);
        }

        public void SetSelected(bool isSelected) => button.SetIsOnWithoutNotify(isSelected);

        private void OnDestroy()
        {
            button.onValueChanged.RemoveListener(HandleValueChanged);
        }

        private void HandleValueChanged(bool isSelected)
        {
            if (isSelected)
            {
                Selected?.Invoke(_locationId);
            }
        }
    }
}
