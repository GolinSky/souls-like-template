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

        [Header("Lock On")]
        [field: SerializeField, Min(0f)] public float LockHeadingHoldDistance { get; private set; } = 0.55f;
        [field: SerializeField, Min(0f)] public float LockHeadingReleaseDistance { get; private set; } = 0.90f;
        [field: SerializeField, Min(0f)] public float LockYawDeadZoneDegrees { get; private set; } = 4f;
        [field: SerializeField, Min(0.01f)] public float LockYawSmoothTime { get; private set; } = 0.12f;
        [field: SerializeField, Min(0f)] public float LockYawMaxSpeed { get; private set; } = 150f;
        [field: SerializeField] public float LockBasePitch { get; private set; } = 6f;
        [field: SerializeField, Min(0f)] public float LockMinPitchDistance { get; private set; } = 1.50f;
        [field: SerializeField, Min(0f)] public float LockVerticalCloseDistance { get; private set; } = 1.25f;
        [field: SerializeField, Min(0f)] public float LockVerticalFarDistance { get; private set; } = 4f;
        [field: SerializeField, Range(0f, 1f)] public float LockCloseVerticalInfluence { get; private set; } = 0.25f;
        [field: SerializeField, Range(0f, 1f)] public float LockFarVerticalInfluence { get; private set; } = 0.65f;
        [field: SerializeField, Min(0.01f)] public float LockPitchSmoothTime { get; private set; } = 0.20f;
        [field: SerializeField, Min(0f)] public float LockPitchMaxSpeed { get; private set; } = 100f;
        [field: SerializeField, Min(0f)] public float LockMinFocusDistance { get; private set; } = 1.50f;
        [field: SerializeField] public float LockMinFocusHeight { get; private set; } = -0.75f;
        [field: SerializeField] public float LockMaxFocusHeight { get; private set; } = 1.25f;
        [field: SerializeField, Min(0.01f)] public float LockTargetSmoothTime { get; private set; } = 0.10f;
        [field: SerializeField, Min(0f)] public float LockYawHalfTurnTolerance { get; private set; } = 2f;
        [field: SerializeField] public CameraRigProfile HumanoidLockProfile { get; private set; } = new CameraRigProfile
        {
            ShoulderOffset = new Vector3(1f, 0.48f, 0f),
            VerticalArmLength = 0f,
            CameraDistance = 3.30f,
            CameraSide = 0.5f,
            FieldOfView = 48f,
            MinPitch = -10f,
            MaxPitch = 16f
        };
        [field: SerializeField, Min(0f)] public float LockRigBlendDuration { get; private set; } = 0.2f;
        [field: SerializeField] public Ease LockRigBlendEase { get; private set; } = Ease.OutSine;

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
        }
    }
}
