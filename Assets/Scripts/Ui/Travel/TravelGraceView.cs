using System;
using SoulsLike.Services.Travel.Data;
using System.Ui.Base;
using UnityEngine;

namespace SoulsLike.Ui.Travel
{
    public sealed class TravelGraceView : MonoBehaviour
    {
        [SerializeField] private CustomButton button;

        private GraceId _graceId;

        public event Action<GraceId> Selected;

        public void Bind(GraceId graceId, string displayName)
        {
            _graceId = graceId;
            button.SetText(displayName);
            button.onClick.RemoveListener(HandleSelected);
            button.onClick.AddListener(HandleSelected);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleSelected);
        }

        private void HandleSelected() => Selected?.Invoke(_graceId);
    }
}
