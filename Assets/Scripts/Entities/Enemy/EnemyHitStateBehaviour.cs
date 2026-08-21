using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyHitStateBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveController(animator).ReportHitEntered();
        }

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveController(animator).ReportHitExited();
        }

        private static EnemyAnimationController ResolveController(Animator animator) =>
            animator.GetComponentInParent<EnemyAnimationController>();
    }
}
