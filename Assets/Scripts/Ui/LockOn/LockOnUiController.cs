using System;
using SoulsLike.Entities.Character;
using SoulsLike.Services;
using SoulsLike.Services.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.LockOn
{
    public class LockOnUiController : UiController, IInitializable, ITickable, IDisposable
    {
        private readonly ITargetingService _targetingService;
        private LockOnUi _lockOnUi;
        private Camera _targetCamera;

        public LockOnUiController(IUiService uiService, ITargetingService targetingService)
            : base(uiService)
        {
            if (targetingService == null)
            {
                throw new ArgumentNullException(nameof(targetingService));
            }

            _targetingService = targetingService;
        }

        public void Initialize()
        {
            _lockOnUi = CreateUi<LockOnUi>();
            _lockOnUi.Hide();

            _targetCamera = Camera.main;
            if (_targetCamera == null)
            {
                throw new InvalidOperationException("LockOnUiController requires a camera tagged MainCamera.");
            }

            _targetingService.TargetChanged += OnTargetChanged;
        }

        public void Tick()
        {
            UpdateLockOnUi();
        }

        public void Dispose()
        {
            _targetingService.TargetChanged -= OnTargetChanged;

            if (_lockOnUi != null)
            {
                _lockOnUi.Hide();
            }
        }

        private void OnTargetChanged(TargetLockNode target)
        {
            if (target == null)
            {
                _lockOnUi.Hide();
                return;
            }

            UpdateLockOnUi();
        }

        private void UpdateLockOnUi()
        {
            if (_lockOnUi == null)
            {
                throw new InvalidOperationException("LockOnUiController must be initialized before it ticks.");
            }

            if (!_targetingService.IsLockedOn)
            {
                _lockOnUi.Hide();
                return;
            }

            TargetLockNode currentTarget = _targetingService.CurrentTarget;
            if (currentTarget == null || !currentTarget.isActiveAndEnabled)
            {
                _lockOnUi.Hide();
                return;
            }

            Transform targetTransform = currentTarget.TargetTransform;
            if (targetTransform == null)
            {
                _lockOnUi.Hide();
                return;
            }

            if (_targetCamera == null || !_targetCamera.isActiveAndEnabled)
            {
                _lockOnUi.Hide();
                return;
            }

            _lockOnUi.SetTargetPosition(targetTransform, _targetCamera);
        }
    }
}
