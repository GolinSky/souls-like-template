using UnityEngine;

namespace SoulsLike.Services.PlayerSession
{
    /// <summary>Provides local camera and control state without exposing Character internals.</summary>
    public readonly struct PlayerSessionSnapshot
    {
        public Transform CameraTarget { get; }
        public bool IsGrounded { get; }
        public float VerticalVelocity { get; }
        public bool CanReadGameplayInput { get; }
        public bool IsPaused { get; }
        public bool ShouldUpdateCamera { get; }
        public bool CanRotateCamera { get; }

        public PlayerSessionSnapshot(
            Transform cameraTarget, bool isGrounded, float verticalVelocity,
            bool canReadGameplayInput, bool isPaused, bool shouldUpdateCamera, bool canRotateCamera)
        {
            CameraTarget = cameraTarget;
            IsGrounded = isGrounded;
            VerticalVelocity = verticalVelocity;
            CanReadGameplayInput = canReadGameplayInput;
            IsPaused = isPaused;
            ShouldUpdateCamera = shouldUpdateCamera;
            CanRotateCamera = canRotateCamera;
        }
    }
}
