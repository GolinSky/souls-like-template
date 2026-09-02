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
            ResolveExecutor(animator).ReportHitEntered();
        }

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveExecutor(animator).ReportHitExited();
        }

        private static EnemyActionExecutor ResolveExecutor(Animator animator) =>
            animator.GetComponentInParent<EnemyActionExecutor>();
    }
}
