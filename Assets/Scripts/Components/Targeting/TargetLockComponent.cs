using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Targeting
{
    public class TargetLockComponent : BaseComponent
    {
        [SerializeField] private TargetLockAnchorType anchorType = TargetLockAnchorType.Torso;
        [SerializeField] private Transform customTargetPoint;

        public Transform TargetTransform
        {
            get
            {
                if (anchorType == TargetLockAnchorType.Custom && customTargetPoint != null)
                {
                    return customTargetPoint;
                }

                return transform;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(TargetTransform.position, 0.12f);
        }
    }
}
