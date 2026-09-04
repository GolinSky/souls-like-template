using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Animations
{
    public class AnimatorStateMachine : StateMachineBehaviour
    {
        [SerializeField] private StateMachineName stateMachineName;
        [SerializeField] private bool isReportingProgress;
        [SerializeField, Range(0.0f, 1.0f)] private float progressNormalizedTime = 0.5f;
        [SerializeField] private bool reportsQueueCheck;
        [SerializeField, Range(0.0f, 1.0f)] private float queueCheckNormalizedTime = 0.55f;

        private IAnimatorStateMachineReceiver animatorStateMachineReceiver;
        
        private int _currentLoopIndex = 0;
        private bool _isProgressFired;
        private bool _isQueueCheckFired;

        private void ResetValues()
        {
            _currentLoopIndex = -1;
            _isProgressFired = false;
            _isQueueCheckFired = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (animator.IsInTransition(layerIndex))
            {
                return;
            }

            ReportProgress(stateInfo, layerIndex);
            ReportQueueCheck(stateInfo, layerIndex);
        }

        private void ReportProgress(AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!isReportingProgress
                || _isProgressFired
                || stateInfo.normalizedTime < progressNormalizedTime)
            {
                return;
            }

            _isProgressFired = true;
            animatorStateMachineReceiver?.OnProgress(stateInfo,
                layerIndex, stateMachineName);
        }

        private void ReportQueueCheck(AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!reportsQueueCheck || _isQueueCheckFired || stateInfo.normalizedTime < queueCheckNormalizedTime)
            {
                return;
            }

            _isQueueCheckFired = true;
            animatorStateMachineReceiver?.OnQueueCheck(stateInfo, layerIndex, stateMachineName);
        }

       
        
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            animatorStateMachineReceiver?.OnExit(stateInfo, layerIndex, stateMachineName);
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            animatorStateMachineReceiver?.OnEnter(stateInfo, layerIndex, stateMachineName);
            ResetValues();
        }

        public void Initialize(IAnimatorStateMachineReceiver animatorStateMachineReceiver)
        {
            this.animatorStateMachineReceiver = animatorStateMachineReceiver;
        }
    }
}
