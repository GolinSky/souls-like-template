using System;
using UnityEngine;

namespace SoulsLike.Entities.Combat
{
    public sealed class CriticalDamageStateBehaviour : StateMachineBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float impactNormalizedTime = 0.22f;

        private bool _applied;

        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            _applied = false;
        }

        public override void OnStateUpdate(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            if (_applied || stateInfo.normalizedTime < impactNormalizedTime)
            {
                return;
            }

            _applied = true;
            ResolveController(animator).ApplyCachedDamage();
        }

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveController(animator).Complete();
        }

        private static CriticalAttackController ResolveController(Animator animator)
        {
            CriticalAttackController controller = animator.GetComponentInParent<CriticalAttackController>();
            if (controller == null)
            {
                throw new InvalidOperationException(
                    $"Animator '{animator.name}' requires a {nameof(CriticalAttackController)} in its parent hierarchy.");
            }

            return controller;
        }
    }
}
