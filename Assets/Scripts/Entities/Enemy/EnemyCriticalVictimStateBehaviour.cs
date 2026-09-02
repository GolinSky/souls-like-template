using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyCriticalVictimStateBehaviour : StateMachineBehaviour
    {
        [SerializeField] private bool isLethal;

        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveExecutor(animator).ReportCriticalVictimEntered(isLethal);
        }

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveExecutor(animator).ReportCriticalVictimExited(isLethal);
        }

        private static EnemyActionExecutor ResolveExecutor(Animator animator) =>
            animator.GetComponentInParent<EnemyActionExecutor>();
    }
}
