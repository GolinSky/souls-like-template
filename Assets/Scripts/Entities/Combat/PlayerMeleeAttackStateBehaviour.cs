using UnityEngine;

namespace SoulsLike.Entities.Combat
{
    public sealed class PlayerMeleeAttackStateBehaviour : StateMachineBehaviour
    {
        private const float ATTACK_SFX_PROGRESS = 0.25f;

        [SerializeField] private CharacterActionId actionId;
        [SerializeField, Range(0f, 1f)] private float activeStart = 0.15f;
        [SerializeField, Range(0f, 1f)] private float activeEnd = 0.55f;

        private bool _opened;
        private bool _playedAttackSfx;
        private bool _closed;
        private int _attackSequence;

        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            _opened = false;
            _playedAttackSfx = false;
            _closed = false;
            _attackSequence = ResolveRelay(animator).Begin(actionId);
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
                relay.Open(_attackSequence);
            }

            if (!_playedAttackSfx && progress >= ATTACK_SFX_PROGRESS)
            {
                _playedAttackSfx = true;
                relay.PlayAttackSfx(_attackSequence);
            }

            if (!_closed && progress >= activeEnd)
            {
                _closed = true;
                relay.Close(_attackSequence);
            }
        }

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveRelay(animator).Close(_attackSequence);
        }

        private static PlayerMeleeCombatRelay ResolveRelay(Animator animator) =>
            animator.GetComponentInParent<PlayerMeleeCombatRelay>();
    }
}
