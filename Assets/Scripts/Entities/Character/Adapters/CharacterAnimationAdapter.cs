using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Runtime;

namespace SoulsLike.Entities.Character.Adapters
{
    public sealed class CharacterAnimationAdapter
    {
        public bool TryAdapt(
            in AnimatorStateMachineDto state,
            out CharacterAnimationSignal signal)
        {
            if (!TryResolveActionState(state.StateMachineName, out CharacterActionStateId actionState))
            {
                signal = default;
                return false;
            }

            CharacterAnimationSignalKind kind = state.State switch
            {
                StateMachineState.Enter => CharacterAnimationSignalKind.Entered,
                StateMachineState.Progress => CharacterAnimationSignalKind.Progressed,
                StateMachineState.QueueCheck => CharacterAnimationSignalKind.QueueWindowOpened,
                StateMachineState.Exit => CharacterAnimationSignalKind.Exited,
                StateMachineState.Loop => CharacterAnimationSignalKind.Progressed,
                _ => throw new System.ArgumentOutOfRangeException(
                    nameof(state.State), state.State, null)
            };
            signal = new CharacterAnimationSignal(kind, actionState);
            return true;
        }

        private static bool TryResolveActionState(
            StateMachineName stateMachine,
            out CharacterActionStateId actionState)
        {
            switch (stateMachine)
            {
                case StateMachineName.LightAttack:
                case StateMachineName.LightAttackAlt:
                case StateMachineName.HeavyAttack:
                case StateMachineName.HeavyAttackAlt:
                case StateMachineName.RollAttack:
                case StateMachineName.BackStepAttack:
                case StateMachineName.RunAttack:
                case StateMachineName.SpecialAttack:
                case StateMachineName.Parry:
                    actionState = CharacterActionStateId.Attack;
                    return true;
                case StateMachineName.Roll:
                case StateMachineName.BackStep:
                    actionState = CharacterActionStateId.Roll;
                    return true;
                case StateMachineName.EquipmentSwapOut:
                case StateMachineName.EquipmentSwapIn:
                    actionState = CharacterActionStateId.EquipmentSwap;
                    return true;
                default:
                    actionState = default;
                    return false;
            }
        }
    }
}
