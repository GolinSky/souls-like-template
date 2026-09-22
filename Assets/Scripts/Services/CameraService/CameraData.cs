using System;
using UnityEngine;

namespace SoulsLike.Services.CameraService
{
    [CreateAssetMenu(fileName = "CameraData", menuName = "Data/CameraData")]
    public sealed class CameraData : ScriptableObject
    {
        [field: Header("Free Look - Cinemachine Camera")]
        [field: SerializeField] public CinemachineCameraSettings FreeLookCamera { get; private set; } = new()
        {
            FieldOfView = 48f,
            CameraSide = 0.5f,
            ThirdPersonFollowDamping = new Vector3(0.08f, 0.1f, 0.18f)
        };

        [field: Header("Lock On - Cinemachine Camera")]
        [field: SerializeField] public CinemachineCameraSettings LockOnCamera { get; private set; } = new()
        {
            FieldOfView = 48f,
            CameraSide = 0.5f,
            ThirdPersonFollowDamping = new Vector3(0.08f, 0.1f, 0.18f)
        };

        [field: SerializeField, Tooltip("Cinemachine Rotation Composer aim damping (horizontal, vertical).")]
        public Vector2 LockOnRotationComposerDamping { get; private set; } = new(0.5f, 0.5f);

        [Serializable]
        public struct CinemachineCameraSettings
        {
            public float FieldOfView;
            public float CameraSide;
            [Tooltip("Cinemachine Third Person Follow position damping (X, Y, Z).")]
            public Vector3 ThirdPersonFollowDamping;
        }
    }
}
