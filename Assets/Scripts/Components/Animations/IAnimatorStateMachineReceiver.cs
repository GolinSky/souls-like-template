using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Animations
{
    public interface IAnimatorStateMachineReceiver
    {
        void OnEnter(AnimatorStateInfo stateInfo, int layerIndex, StateMachineName stateMachineName);
        void OnExit(AnimatorStateInfo stateInfo, int layerIndex, StateMachineName stateMachineName);
        void OnLoop(int loopIndex, AnimatorStateInfo stateInfo, int layerIndex, StateMachineName stateMachineName);
        void OnFinished(AnimatorStateInfo stateInfo, int layerIndex, StateMachineName stateMachineName);
        void OnProgress(AnimatorStateInfo stateInfo, int layerIndex, StateMachineName stateMachineName);
    }
}
