using SoulsLike.Entities.Combat;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyActionStateBehaviour : StateMachineBehaviour
    {
        [SerializeField] private CharacterActionId actionId;
        [SerializeField] private bool hasHitboxWindow = true;
        [SerializeField, Range(0f, 1f)] private float activeStart = 0.3f;
        [SerializeField, Range(0f, 1f)] private float activeEnd = 0.55f;
        [SerializeField] private bool hasComboWindow;
        [SerializeField, Range(0f, 1f)] private float comboStart = 0.45f;
        [SerializeField, Range(0f, 1f)] private float comboEnd = 0.7f;
        [SerializeField, Range(0f, 1f)] private float recoveryStart = 0.6f;
        [Header("Hyper Armor")]
        [SerializeField] private bool hasHyperArmorWindow;
        [SerializeField, Range(0f, 1f)] private float hyperArmorStart;
        [SerializeField, Range(0f, 1f)] private float hyperArmorEnd = 1f;
        [SerializeField, Min(0f)] private float hyperArmorPoiseBonus;
        [SerializeField] private bool canBeInterruptedDuringHyperArmor;

        private bool _activeStarted;
        private bool _activeEnded;
        private bool _comboStarted;
        private bool _comboEnded;
        private bool _recoveryStarted;

        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResetState();
            ResolveController(animator).ReportStateEntered(actionId);
        }

        public override void OnStateUpdate(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            EnemyAnimationController controller = ResolveController(animator);
            float progress = Mathf.Clamp01(stateInfo.normalizedTime);

            if (hasHitboxWindow && !_activeStarted && progress >= activeStart)
            {
                _activeStarted = true;
                controller.ReportActiveStarted(actionId);
            }

            if (hasHitboxWindow && !_activeEnded && progress >= activeEnd)
            {
                _activeEnded = true;
                controller.ReportActiveEnded(actionId);
            }

            if (hasComboWindow && !_comboStarted && progress >= comboStart)
            {
                _comboStarted = true;
                controller.ReportComboWindow(actionId, true);
            }

            if (hasComboWindow && !_comboEnded && progress >= comboEnd)
            {
                _comboEnded = true;
                controller.ReportComboWindow(actionId, false);
            }

            if (!_recoveryStarted && progress >= recoveryStart)
            {
                _recoveryStarted = true;
                controller.ReportRecoveryStarted(actionId);
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
            EnemyAnimationController controller = ResolveController(animator);
            controller.ReportActiveEnded(actionId);
            controller.ReportComboWindow(actionId, false);
            controller.ReportStateExited(actionId);
            ResolveDefense(animator).SetHyperArmor(false);
        }

        private static EnemyAnimationController ResolveController(Animator animator) =>
            animator.GetComponentInParent<EnemyAnimationController>();

        private static CombatDefenseComponent ResolveDefense(Animator animator) =>
            animator.GetComponentInParent<CombatDefenseComponent>();

        private void ResetState()
        {
            _activeStarted = false;
            _activeEnded = false;
            _comboStarted = false;
            _comboEnded = false;
            _recoveryStarted = false;
        }
    }
}
