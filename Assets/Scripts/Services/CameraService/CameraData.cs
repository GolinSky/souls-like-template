using System;
using DG.Tweening;
using UnityEngine;

namespace SoulsLike.Services.CameraService
{
    [CreateAssetMenu(fileName = "CameraData", menuName = "Data/CameraData")]
    public sealed class CameraData : ScriptableObject
    {
        [Header("Switch Angle")]
        [field: SerializeField] public float SwitchAngleDuration { get; private set; } = 0.4f;
        [field: SerializeField] public Ease SwitchAngleEase { get; private set; } = Ease.InOutQuad;

        [Header("Zoom")]
        [field: SerializeField] public float ZoomFov { get; private set; } = 30f;
        [field: SerializeField] public float ZoomDuration { get; private set; } = 0.3f;
        [field: SerializeField] public Ease ZoomEase { get; private set; } = Ease.OutSine;

        [Header("Vertical Follow")]
        [field: SerializeField, Min(0f)] public float AirborneRiseLag { get; private set; } = 0.65f;
        [field: SerializeField, Min(0f)] public float AirborneFallLag { get; private set; } = 0.40f;
        [field: SerializeField, Min(0.01f)] public float GroundedFollowSmoothTime { get; private set; } = 0.10f;
        [field: SerializeField, Min(0.01f)] public float JumpFollowSmoothTime { get; private set; } = 0.22f;
        [field: SerializeField, Min(0.01f)] public float FallFollowSmoothTime { get; private set; } = 0.15f;
        [field: SerializeField, Min(0.01f)] public float LongFallSmoothTime { get; private set; } = 0.08f;
        [field: SerializeField, Min(0f)] public float GroundedMaxFollowSpeed { get; private set; } = 5f;
        [field: SerializeField, Min(0f)] public float JumpMaxFollowSpeed { get; private set; } = 5f;
        [field: SerializeField, Min(0f)] public float FallMaxFollowSpeed { get; private set; } = 8f;
        [field: SerializeField, Min(0f)] public float LongFallMaxSpeed { get; private set; } = 18f;
        [field: SerializeField, Min(0f)] public float LongFallCatchupDistance { get; private set; } = 4f;

        [Header("Free Look")]
        [field: SerializeField, Min(0f)] public float MouseYawDegreesPerPixel { get; private set; } = 0.09f;
        [field: SerializeField, Min(0f)] public float MousePitchDegreesPerPixel { get; private set; } = 0.08f;
        [field: SerializeField, Min(0f)] public float StickYawDegreesPerSecond { get; private set; } = 220f;
        [field: SerializeField, Min(0f)] public float StickPitchDegreesPerSecond { get; private set; } = 150f;

        [Header("Cinemachine")]
        [field: SerializeField, Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp { get; private set; } = 70.0f;

        [field: SerializeField, Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp { get; private set; } = -30.0f;

        [field: SerializeField, Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride { get; private set; }

        [field: SerializeField, Tooltip("For locking the camera position on all axes")]
        public bool LockCameraPosition { get; private set; }

        [Header("Lock Acquisition")]
        [field: SerializeField, Min(0f)] public float LockBlendDuration { get; private set; } = 0.3f;
        [field: SerializeField] public Ease LockBlendEase { get; private set; } = Ease.InOutSine;
        [field: SerializeField, Min(0f)] public float LockInitialFocusMinDistance { get; private set; } = 1.50f;

        [Header("Lock Aim")]
        [field: SerializeField, Min(0.01f)] public float LockAimSmoothTime { get; private set; } = 0.08f;
        [field: SerializeField, Min(0f)] public float LockAimMaxSpeed { get; private set; } = 40f;
        [field: SerializeField] public float LockMinFocusHeight { get; private set; } = -0.75f;
        [field: SerializeField] public float LockMaxFocusHeight { get; private set; } = 1.25f;

        [Header("Lock Orbit Yaw")]
        [field: SerializeField, Min(0f)] public float LockOrbitYawEnterAngle { get; private set; } = 7f;
        [field: SerializeField, Min(0f)] public float LockOrbitYawReleaseAngle { get; private set; } = 3.5f;
        [field: SerializeField, Min(0.01f)] public float LockOrbitYawSmoothTime { get; private set; } = 0.32f;
        [field: SerializeField, Min(0f)] public float LockOrbitYawMaxSpeed { get; private set; } = 110f;
        [field: SerializeField, Min(0f)] public float LockYawHalfTurnTolerance { get; private set; } = 2f;
        [field: SerializeField, Min(0f)] public float LockHeadingHoldDistance { get; private set; } = 0.55f;
        [field: SerializeField, Min(0f)] public float LockHeadingReleaseDistance { get; private set; } = 0.90f;

        [Header("Lock Orbit Fast Yaw")]
        [field: SerializeField, Min(0.01f)] public float LockYawFastSmoothTime { get; private set; } = 0.05f;
        [field: SerializeField, Min(0f)] public float LockYawFastMaxSpeed { get; private set; } = 360f;
        [field: SerializeField, Min(0f)] public float LockYawFastDeadZoneDegrees { get; private set; } = 0.75f;
        [field: SerializeField, Min(0f)] public float LockFastFollowStartRate { get; private set; } = 45f;
        [field: SerializeField, Min(0f)] public float LockFastFollowFullRate { get; private set; } = 135f;
        [field: SerializeField, Min(0f)] public float LockFastFollowStartError { get; private set; } = 5f;
        [field: SerializeField, Min(0f)] public float LockFastFollowFullError { get; private set; } = 18f;
        [field: SerializeField, Min(0.01f)] public float LockYawRateFilterTime { get; private set; } = 0.08f;
        [field: SerializeField, Min(0.01f)] public float LockYawUrgencySmoothTime { get; private set; } = 0.08f;
        [field: SerializeField, Min(0f)] public float LockYawLeadTime { get; private set; } = 0.045f;
        [field: SerializeField, Min(0f)] public float LockYawMaxLeadDegrees { get; private set; } = 5f;

        [Header("Lock Orbit Pitch")]
        [field: SerializeField] public float LockBasePitch { get; private set; } = 1f;
        [field: SerializeField, Min(0f)] public float LockOrbitPitchEnterAngle { get; private set; } = 10f;
        [field: SerializeField, Min(0f)] public float LockOrbitPitchReleaseAngle { get; private set; } = 5f;
        [field: SerializeField, Min(0.01f)] public float LockOrbitPitchSmoothTime { get; private set; } = 0.35f;
        [field: SerializeField, Min(0f)] public float LockOrbitPitchMaxSpeed { get; private set; } = 60f;
        [field: SerializeField, Obsolete("Replaced by neutral body pitch in humanoid lock.")] public float LockMinPitchDistance { get; private set; } = 1.50f;
        [field: SerializeField, Obsolete("Replaced by neutral body pitch in humanoid lock.")] public float LockVerticalCloseDistance { get; private set; } = 1.25f;
        [field: SerializeField, Obsolete("Replaced by neutral body pitch in humanoid lock.")] public float LockVerticalFarDistance { get; private set; } = 4f;
        [field: SerializeField, Obsolete("Replaced by neutral body pitch in humanoid lock.")] public float LockCloseVerticalInfluence { get; private set; } = 0.25f;
        [field: SerializeField, Obsolete("Replaced by neutral body pitch in humanoid lock.")] public float LockFarVerticalInfluence { get; private set; } = 0.65f;

        [Header("Lock Rig Profile")]
        [field: SerializeField] public CameraRigProfile HumanoidLockProfile { get; private set; } = new CameraRigProfile
        {
            ShoulderOffset = new Vector3(0f, 0.48f, 0f),
            VerticalArmLength = -0.31f,
            CameraDistance = 3.30f,
            CameraSide = 0.5f,
            FieldOfView = 48f,
            MinPitch = -10f,
            MaxPitch = 16f,
            Damping = new Vector3(0.08f, 0.10f, 0.18f)
        };

        [Serializable]
        public struct CameraRigProfile
        {
            public Vector3 ShoulderOffset;
            public float VerticalArmLength;
            public float CameraDistance;
            public float CameraSide;
            public float FieldOfView;
            public float MinPitch;
            public float MaxPitch;
            public Vector3 Damping;
        }
    }
}
