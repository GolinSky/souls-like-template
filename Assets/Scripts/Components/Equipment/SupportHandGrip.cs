using System;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    [Serializable]
    public sealed class SupportHandGrip
    {
        [Tooltip("Opt in to support-hand grip metadata. This does not enable runtime rigging.")]
        [SerializeField] private bool isEnabled;
        [Tooltip("Support-hand position in WeaponRuntime local coordinates.")]
        [SerializeField] private Vector3 localPosition;
        [Tooltip("Support-hand rotation in WeaponRuntime local coordinates.")]
        [SerializeField] private Vector3 localEulerAngles;

        public bool IsEnabled => isEnabled;
        public Vector3 LocalPosition => localPosition;
        public Vector3 LocalEulerAngles => localEulerAngles;
        public Quaternion LocalRotation => Quaternion.Euler(localEulerAngles);
        public Pose LocalPose => new(localPosition, LocalRotation);
    }
}
