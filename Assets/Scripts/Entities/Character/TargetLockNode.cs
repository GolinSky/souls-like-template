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

    //todo: move and refactor 
    public class TargetLockNode : MonoBehaviour
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
            if (TargetTransform == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(TargetTransform.position, 0.12f);
        }
    }
}
