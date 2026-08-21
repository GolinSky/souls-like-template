using System;
using UnityEngine;

namespace Jorjouto.AnimComposerSystem
{
    public class AnimDispatcher : MonoBehaviour
    {
        public event Action<Animator, AnimatorStateInfo, int> StateEntered;

        void OnAnimatorMove() {}
        
        public void NotifyStateEntered(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            StateEntered?.Invoke(animator, stateInfo, layerIndex);
        }
    }
}
