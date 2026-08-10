using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Animations
{
    public class AnimatorStateMachine : StateMachineBehaviour
    {
        [SerializeField] private StateMachineName stateMachineName;


        [SerializeField] private bool isReportingProgress;

        private IAnimatorStateMachineReceiver animatorStateMachineReceiver;
        
        private int _currentLoopIndex = 0;
        private bool _isFinishFired;

        private void ResetValues()
        {
            _currentLoopIndex = -1;
            _isFinishFired = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            ReportProgress(stateInfo, layerIndex);
        }

        private void ReportProgress(AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!isReportingProgress)
            {
                return;
            }
            animatorStateMachineReceiver?.OnProgress(stateInfo,
                layerIndex, stateMachineName);
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
