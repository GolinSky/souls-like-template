using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyGetUpStateBehaviour : StateMachineBehaviour
    {
        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveExecutor(animator).ReportGetUpEntered();
        }

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveExecutor(animator).ReportGetUpExited();
        }

        private static EnemyActionExecutor ResolveExecutor(Animator animator) =>
            animator.GetComponentInParent<EnemyActionExecutor>();
    }
}
