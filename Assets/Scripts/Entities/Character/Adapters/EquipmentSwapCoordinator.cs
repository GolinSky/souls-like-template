using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Runtime;

namespace SoulsLike.Entities.Character.Adapters
{
    public sealed class EquipmentSwapCoordinator
    {
        private enum SwapPhase
        {
            None,
            SwapOut,
            SwapIn
        }

        private SwapPhase _phase;
        private EquipmentSlotGroup _slotGroup;

        public bool IsActive => _phase != SwapPhase.None;

        public CharacterCommandExecutionStatus StartSwap(
            EquipmentSlotGroup slotGroup,
            EquipmentComponent equipment,
            AnimatorComponent animator)
        {
            if (_phase != SwapPhase.None)
            {
                return CharacterCommandExecutionStatus.TemporarilyBlocked;
            }

            _slotGroup = slotGroup;
            if (!animator.IsNoWeaponMode)
            {
                _phase = SwapPhase.SwapOut;
                animator.TriggerEquipmentSwapOut(slotGroup);
                return CharacterCommandExecutionStatus.Executed;
            }

            SwapEquipment(equipment, animator);
            return CharacterCommandExecutionStatus.Executed;
        }

        public void HandleAnimationState(
            in AnimatorStateMachineDto state,
            EquipmentComponent equipment,
            AnimatorComponent animator)
        {
            if (state.State != StateMachineState.Exit)
            {
                return;
            }

            if (state.StateMachineName == StateMachineName.EquipmentSwapOut
                && _phase == SwapPhase.SwapOut)
            {
                SwapEquipment(equipment, animator);
            }
            else if (state.StateMachineName == StateMachineName.EquipmentSwapIn
                && _phase == SwapPhase.SwapIn)
            {
                _phase = SwapPhase.None;
            }
        }

        private void SwapEquipment(
            EquipmentComponent equipment,
            AnimatorComponent animator)
        {
            EquipmentSlotId previous = equipment.Model.GetActiveSlot(_slotGroup);
            EquipmentSlotId active = equipment.SwitchActive(_slotGroup);
            EquipmentLoadout loadout = equipment.BuildLoadout();
            EquippedItemContext equippedItem =
                _slotGroup == EquipmentSlotGroup.LeftHandArmament
                    ? loadout.EffectiveLeft
                    : loadout.EffectiveRight;
            if (active == previous || equippedItem == null)
            {
                _phase = SwapPhase.None;
                return;
            }

            _phase = SwapPhase.SwapIn;
            animator.TriggerEquipmentSwapIn(_slotGroup);
        }
    }
}
