using UnityEngine;

namespace SoulsLike.Entities.Combat
{
    public sealed class PlayerMeleeAttackStateBehaviour : StateMachineBehaviour
    {
        [SerializeField] private CharacterActionId actionId;
        [SerializeField, Range(0f, 1f)] private float activeStart = 0.3f;
        [SerializeField, Range(0f, 1f)] private float activeEnd = 0.55f;

        private bool _opened;
        private bool _closed;

        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            _opened = false;
            _closed = false;
            ResolveRelay(animator).Begin(actionId);
        }

        public override void OnStateUpdate(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            float progress = Mathf.Clamp01(stateInfo.normalizedTime);
            PlayerMeleeCombatRelay relay = ResolveRelay(animator);
            if (!_opened && progress >= activeStart)
            {
                _opened = true;
                relay.Open();
            }

            if (!_closed && progress >= activeEnd)
            {
                _closed = true;
                relay.Close();
            }
        }

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveRelay(animator).Close();
        }

        private static PlayerMeleeCombatRelay ResolveRelay(Animator animator) =>
            animator.GetComponentInParent<PlayerMeleeCombatRelay>();
    }
}
