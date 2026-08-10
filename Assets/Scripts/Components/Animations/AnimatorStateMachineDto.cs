using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Animations
{
    public struct AnimatorStateMachineDto
    {
        public AnimatorStateInfo StateInfo { get; set; }
        public int LayerIndex { get; set; }
        public StateMachineName StateMachineName { get; set; }
        public StateMachineState State { get; set; }
        public readonly int LoopIndex => Mathf.FloorToInt(StateInfo.normalizedTime);
    }
}
