using System;
using UnityEngine;

namespace SoulsLike.Entities.Combat
{
    //todo: use stats from shield/weapon -> parry equipment 
    public sealed class ParryWindowStateBehaviour : StateMachineBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float activeStart = 0.2f;
        [SerializeField, Range(0f, 1f)] private float activeEnd = 0.45f;

        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveDefense(animator).SetParryWindowActive(false);
        }

        public override void OnStateUpdate(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            float progress = Mathf.Clamp01(stateInfo.normalizedTime);
            ResolveDefense(animator).SetParryWindowActive(
                progress >= activeStart && progress <= activeEnd);
        }

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveDefense(animator).SetParryWindowActive(false);
        }

        private static CombatDefenseComponent ResolveDefense(Animator animator)
        {
            CombatDefenseComponent defense = animator.GetComponentInParent<CombatDefenseComponent>();
            if (defense == null)
            {
                throw new InvalidOperationException(
                    $"Animator '{animator.name}' requires a {nameof(CombatDefenseComponent)} in its parent hierarchy.");
            }

            return defense;
        }
    }
}
