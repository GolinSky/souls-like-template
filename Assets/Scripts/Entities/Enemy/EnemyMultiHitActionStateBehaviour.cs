using System;
using SoulsLike.Entities.Combat;
using UnityEngine;

namespace SoulsLike.Entities.Enemy
{
    public sealed class EnemyMultiHitActionStateBehaviour : StateMachineBehaviour
    {
        [Serializable]
        public struct HitWindow
        {
            [SerializeField] private int hitIndex;
            [SerializeField, Range(0f, 1f)] private float activeStart;
            [SerializeField, Range(0f, 1f)] private float activeEnd;
            [SerializeField] private bool hasTrackingWindow;
            [SerializeField, Range(0f, 1f)] private float trackingStart;
            [SerializeField, Range(0f, 1f)] private float trackingEnd;

            public int HitIndex => hitIndex;
            public float ActiveStart => activeStart;
            public float ActiveEnd => activeEnd;
            public bool HasTrackingWindow => hasTrackingWindow;
            public float TrackingStart => trackingStart;
            public float TrackingEnd => trackingEnd;

            public HitWindow(
                int hitIndex,
                float activeStart,
                float activeEnd,
                bool hasTrackingWindow = false,
                float trackingStart = 0f,
                float trackingEnd = 0f)
            {
                this.hitIndex = hitIndex;
                this.activeStart = activeStart;
                this.activeEnd = activeEnd;
                this.hasTrackingWindow = hasTrackingWindow;
                this.trackingStart = trackingStart;
                this.trackingEnd = trackingEnd;
            }
        }

        [SerializeField] private CharacterActionId actionId;
        [SerializeField] private HitWindow[] hitWindows = Array.Empty<HitWindow>();
        [SerializeField] private bool hasComboWindow;
        [SerializeField, Range(0f, 1f)] private float comboStart = 0.7f;
        [SerializeField, Range(0f, 1f)] private float comboEnd = 0.9f;
        [SerializeField, Range(0f, 1f)] private float recoveryStart = 0.75f;
        [Header("Hyper Armor")]
        [SerializeField] private bool hasHyperArmorWindow;
        [SerializeField, Range(0f, 1f)] private float hyperArmorStart;
        [SerializeField, Range(0f, 1f)] private float hyperArmorEnd = 1f;
        [SerializeField, Min(0f)] private float hyperArmorPoiseBonus;
        [SerializeField] private bool canBeInterruptedDuringHyperArmor;

        private bool[] _hitStarted;
        private bool[] _hitEnded;
        private bool[] _trackingStarted;
        private bool[] _trackingEnded;
        private bool _comboStarted;
        private bool _comboEnded;
        private bool _recoveryStarted;

        public CharacterActionId ActionId => actionId;
        public HitWindow[] HitWindows => hitWindows;

        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            ResetState();
            EnemyActionExecutor executor = ResolveExecutor(animator);
            executor.ReportStateEntered(actionId);
        }

        public override void OnStateUpdate(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            EnemyActionExecutor executor = ResolveExecutor(animator);
            float progress = Mathf.Clamp01(stateInfo.normalizedTime);

            if (hitWindows != null)
            {
                for (int i = 0; i < hitWindows.Length; i++)
                {
                    ref readonly HitWindow window = ref hitWindows[i];

                    if (window.HasTrackingWindow && !_trackingStarted[i] && progress >= window.TrackingStart)
                    {
                        _trackingStarted[i] = true;
                        executor.ReportTrackingWindow(actionId, true);
                    }

                    if (!_hitStarted[i] && progress >= window.ActiveStart)
                    {
                        _hitStarted[i] = true;
                        executor.ReportActiveStarted(actionId, window.HitIndex);
                    }

                    if (!_hitEnded[i] && progress >= window.ActiveEnd)
                    {
                        _hitEnded[i] = true;
                        executor.ReportActiveEnded(actionId);
                    }

                    if (window.HasTrackingWindow && !_trackingEnded[i] && progress >= window.TrackingEnd)
                    {
                        _trackingEnded[i] = true;
                        executor.ReportTrackingWindow(actionId, false);
                    }
                }
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
            int count = hitWindows?.Length ?? 0;
            if (_hitStarted == null || _hitStarted.Length != count)
            {
                _hitStarted = new bool[count];
                _hitEnded = new bool[count];
                _trackingStarted = new bool[count];
                _trackingEnded = new bool[count];
            }
            else
            {
                Array.Clear(_hitStarted, 0, count);
                Array.Clear(_hitEnded, 0, count);
                Array.Clear(_trackingStarted, 0, count);
                Array.Clear(_trackingEnded, 0, count);
            }

            _comboStarted = false;
            _comboEnded = false;
            _recoveryStarted = false;
        }
    }
}
