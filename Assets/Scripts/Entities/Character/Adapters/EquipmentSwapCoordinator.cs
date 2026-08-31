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
            SwapOutHidden,
            SwapIn
        }

        private SwapPhase _phase;
        private EquipmentSlotGroup _slotGroup;

        public bool IsActive => _phase != SwapPhase.None;

        public void Cancel(EquipmentPresentation presentation)
        {
            if (!IsActive)
            {
                return;
            }

            presentation.SetArmamentVisible(_slotGroup, true);
            _phase = SwapPhase.None;
        }

        public CharacterCommandExecutionStatus StartSwap(
            EquipmentSlotGroup slotGroup,
            EquipmentComponent equipment,
            AnimatorComponent animator,
            EquipmentPresentation presentation)
        {
            if (_phase != SwapPhase.None)
            {
                return CharacterCommandExecutionStatus.TemporarilyBlocked;
            }

            _slotGroup = slotGroup;
            EquippedItemContext equippedItem = GetEquippedItem(
                equipment.BuildLoadout(),
                slotGroup);
            if (equippedItem != null)
            {
                _phase = SwapPhase.SwapOut;
                animator.TriggerEquipmentSwapOut(slotGroup);
                return CharacterCommandExecutionStatus.Executed;
            }

            SwapEquipment(equipment, animator, presentation);
            return CharacterCommandExecutionStatus.Executed;
        }

        public void HandleAnimationState(
            in AnimatorStateMachineDto state,
            EquipmentComponent equipment,
            AnimatorComponent animator,
            EquipmentPresentation presentation)
        {
            if (state.State == StateMachineState.Progress)
            {
                if (state.StateMachineName == StateMachineName.EquipmentSwapOut
                    && _phase == SwapPhase.SwapOut)
                {
                    presentation.SetArmamentVisible(_slotGroup, false);
                    _phase = SwapPhase.SwapOutHidden;
                }
                else if (state.StateMachineName == StateMachineName.EquipmentSwapIn
                    && _phase == SwapPhase.SwapIn)
                {
                    presentation.SetArmamentVisible(_slotGroup, true);
                }

                return;
            }

            if (state.State != StateMachineState.Exit)
            {
                return;
            }

            if (state.StateMachineName == StateMachineName.EquipmentSwapOut
                && _phase == SwapPhase.SwapOutHidden)
            {
                _phase = SwapPhase.None;
                SwapEquipment(equipment, animator, presentation);
            }
            else if (state.StateMachineName == StateMachineName.EquipmentSwapIn
                && _phase == SwapPhase.SwapIn)
            {
                _phase = SwapPhase.None;
            }
        }

        private void SwapEquipment(
            EquipmentComponent equipment,
            AnimatorComponent animator,
            EquipmentPresentation presentation)
        {
            EquipmentSlotId previous = equipment.Model.GetActiveSlot(_slotGroup);
            presentation.SetArmamentVisible(_slotGroup, false);
            EquipmentSlotId active = equipment.SwitchActive(_slotGroup);
            EquipmentLoadout loadout = equipment.BuildLoadout();
            EquippedItemContext equippedItem = GetEquippedItem(loadout, _slotGroup);
            if (active == previous)
            {
                presentation.SetArmamentVisible(_slotGroup, true);
                _phase = SwapPhase.None;
                return;
            }

            if (equippedItem == null)
            {
                presentation.SetArmamentVisible(_slotGroup, true);
                _phase = SwapPhase.None;
                return;
            }

            presentation.SetArmamentVisible(_slotGroup, false);
            _phase = SwapPhase.SwapIn;
            animator.TriggerEquipmentSwapIn(_slotGroup);
        }

        private static EquippedItemContext GetEquippedItem(
            in EquipmentLoadout loadout,
            EquipmentSlotGroup slotGroup)
        {
            return slotGroup == EquipmentSlotGroup.LeftHandArmament
                ? loadout.EffectiveLeft
                : loadout.EffectiveRight;
        }
    }
}
