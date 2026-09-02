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
        [Header("Tracking")]
        [SerializeField] private bool hasTrackingWindow = true;
        [SerializeField, Range(0f, 1f)] private float trackingEnd = 0.6f;
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
        private bool _trackingEnded;

        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResetState();
            ResolveExecutor(animator).ReportStateEntered(actionId);
            ResolveExecutor(animator).ReportTrackingWindow(actionId, hasTrackingWindow);
        }

        public override void OnStateUpdate(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            EnemyActionExecutor executor = ResolveExecutor(animator);
            float progress = Mathf.Clamp01(stateInfo.normalizedTime);

            if (hasHitboxWindow && !_activeStarted && progress >= activeStart)
            {
                _activeStarted = true;
                executor.ReportActiveStarted(actionId);
            }

            if (hasHitboxWindow && !_activeEnded && progress >= activeEnd)
            {
                _activeEnded = true;
                executor.ReportActiveEnded(actionId);
            }

            if (hasComboWindow && !_comboStarted && progress >= comboStart)
            {
                _comboStarted = true;
                executor.ReportComboWindow(actionId, true);
            }

            if (hasComboWindow && !_comboEnded && progress >= comboEnd)
            {
                _comboEnded = true;
                executor.ReportComboWindow(actionId, false);
            }

            if (!_recoveryStarted && progress >= recoveryStart)
            {
                _recoveryStarted = true;
                executor.ReportRecoveryStarted(actionId);
            }

            if (hasTrackingWindow && !_trackingEnded && progress >= trackingEnd)
            {
                _trackingEnded = true;
                executor.ReportTrackingWindow(actionId, false);
            }

            executor.ReportHyperArmor(
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
            EnemyActionExecutor executor = ResolveExecutor(animator);
            executor.ReportActiveEnded(actionId);
            executor.ReportComboWindow(actionId, false);
            executor.ReportTrackingWindow(actionId, false);
            executor.ReportStateExited(actionId);
            executor.ReportHyperArmor(false, 0f, false);
        }

        private static EnemyActionExecutor ResolveExecutor(Animator animator) =>
            animator.GetComponentInParent<EnemyActionExecutor>();

        private void ResetState()
        {
            _activeStarted = false;
            _activeEnded = false;
            _comboStarted = false;
            _comboEnded = false;
            _recoveryStarted = false;
            _trackingEnded = false;
        }
    }
}
