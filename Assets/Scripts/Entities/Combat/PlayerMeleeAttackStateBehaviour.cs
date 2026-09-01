using UnityEngine;

namespace SoulsLike.Entities.Combat
{
    public sealed class PlayerMeleeAttackStateBehaviour : StateMachineBehaviour
    {
        private const float ATTACK_SFX_PROGRESS = 0.25f;

        [SerializeField] private CharacterActionId actionId;
        [SerializeField, Range(0f, 1f)] private float activeStart = 0.15f;
        [SerializeField, Range(0f, 1f)] private float activeEnd = 0.55f;
        [Header("Hyper Armor")]
        [SerializeField] private bool hasHyperArmorWindow;
        [SerializeField, Range(0f, 1f)] private float hyperArmorStart;
        [SerializeField, Range(0f, 1f)] private float hyperArmorEnd = 1f;
        [SerializeField, Min(0f)] private float hyperArmorPoiseBonus;
        [SerializeField] private bool canBeInterruptedDuringHyperArmor;

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

            ResolveDefense(animator).SetHyperArmor(
                hasHyperArmorWindow
                && progress >= hyperArmorStart
                && progress <= hyperArmorEnd,
                hyperArmorPoiseBonus,
                canBeInterruptedDuringHyperArmor);
        }

        public override void OnStateExit(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResolveRelay(animator).Close(_attackSequence);
            ResolveDefense(animator).SetHyperArmor(false);
        }

        private static PlayerMeleeCombatRelay ResolveRelay(Animator animator) =>
            animator.GetComponentInParent<PlayerMeleeCombatRelay>();

        private static CombatDefenseComponent ResolveDefense(Animator animator) =>
            animator.GetComponentInParent<CombatDefenseComponent>();
    }
}
