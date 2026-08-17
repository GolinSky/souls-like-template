using SoulsLike.Entities.Character.Components.Animations;

namespace SoulsLike.Entities.Character.Ports
{
    public interface IAnimationStateSink
    {
        void OnAnimationStateChanged(AnimatorStateMachineDto state);
    }
}
