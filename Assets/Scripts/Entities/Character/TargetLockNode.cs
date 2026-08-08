using UnityEngine;

namespace SoulsLike.Entities.Character
{
    public enum TargetLockAnchorType
    {
        Base = 0,
        Torso = 1,
        Head = 2,
        Custom = 3
    }

    public class TargetLockNode : MonoBehaviour
    {
        [SerializeField] private TargetLockAnchorType _anchorType = TargetLockAnchorType.Torso;
        [SerializeField] private Transform _customTargetPoint;

        public Transform TargetTransform
        {
            get
            {
                if (_anchorType == TargetLockAnchorType.Custom && _customTargetPoint != null)
                {
                    return _customTargetPoint;
                }

                return transform;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (TargetTransform == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(TargetTransform.position, 0.12f);
        }
    }
}
