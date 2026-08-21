using System;
using SoulsLike.Entities.BaseEntity.EntityCommands;
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
        private bool _isDisposed;

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
            _isDisposed = true;

            if (_lockOnUi != null)
            {
                _lockOnUi.Hide();
            }
        }

        public void PostLateTick()
        {
            if (_isDisposed || _lockOnUi == null) return;

            UpdateLockOnUi();
        }

        private void UpdateLockOnUi()
        {
            if (!_targetingService.TryGetCurrentTarget(out TargetingSnapshot snapshot) || !snapshot.IsAlive)
            {
                _lockOnUi.Hide();
                return;
            }

            bool isVisible = _lockOnUi.TrySetTargetPosition(snapshot.LockPoint, _targetCamera);
            if (isVisible)
            {
                _lockOnUi.Show();
                return;
            }

            _lockOnUi.Hide();
        }
    }
}
