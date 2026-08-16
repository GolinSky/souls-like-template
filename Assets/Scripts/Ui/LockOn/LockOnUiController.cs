using System;
using SoulsLike.Entities.Character;
using SoulsLike.Services;
using SoulsLike.Services.CameraService;
using SoulsLike.Services.Targeting;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Ui.LockOn
{
    public class LockOnUiController : UiController, IInitializable, IPostLateTickable, IDisposable
    {
        private readonly ITargetingService _targetingService;
        private readonly ICameraService _cameraService;
        private LockOnUi _lockOnUi;
        private Camera _targetCamera;

        public LockOnUiController(IUiService uiService, ITargetingService targetingService, ICameraService cameraService)
            : base(uiService)
        {
            _targetingService = targetingService;
            _cameraService = cameraService;
        }

        public void Initialize()
        {
            _lockOnUi = CreateUi<LockOnUi>();
            _lockOnUi.Hide();
            _targetCamera = _cameraService.GetMainCamera();
        }

        public void Dispose()
        {
            _lockOnUi.Hide();
        }

        public void PostLateTick()
        {
            UpdateLockOnUi();
        }

        private void UpdateLockOnUi()
        {
            TargetLockNode currentTarget = _targetingService.CurrentTarget;
            if (currentTarget == null || !currentTarget.isActiveAndEnabled)
            {
                _lockOnUi.Hide();
                return;
            }

            bool isVisible = _lockOnUi.TrySetTargetPosition(currentTarget.TargetTransform, _targetCamera);
            if (isVisible)
            {
                _lockOnUi.Show();
                return;
            }

            _lockOnUi.Hide();
        }
    }
}
