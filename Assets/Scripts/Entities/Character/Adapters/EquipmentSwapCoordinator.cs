using System;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Items;

namespace SoulsLike.Entities.Character.Adapters
{
    public sealed class EquipmentSwapCoordinator
    {
        private enum SwapPhase
        {
            None,
            SwapOut,
            SwapOutCompleted,
            SwapIn
        }

        private SwapPhase _phase;

        public bool IsActive => _phase != SwapPhase.None;

        public CharacterCommandExecutionStatus StartRightHandSwap(
            EquipmentComponent equipment,
            AnimatorComponent animator)
        {
            if (_phase != SwapPhase.None)
            {
                return CharacterCommandExecutionStatus.TemporarilyBlocked;
            }

            EquipmentSlotId previous = equipment.Model.GetActiveSlot(
                EquipmentSlotGroup.RightHandArmament);
            if (!animator.IsNoWeaponMode)
            {
                _phase = SwapPhase.SwapOut;
                animator.TriggerEquipmentSwapOut();
                return CharacterCommandExecutionStatus.Executed;
            }

            _phase = SwapPhase.SwapIn;
            EquipmentSlotId active = equipment.SwitchActive(
                EquipmentSlotGroup.RightHandArmament);
            EquipmentLoadout loadout = equipment.BuildLoadout();
            bool hasWeapon = loadout.EffectiveRight?.Definition is WeaponDefinition
                || loadout.EffectiveLeft?.Definition is WeaponDefinition;
            if (active == previous || !hasWeapon)
            {
                _phase = SwapPhase.None;
                return CharacterCommandExecutionStatus.Executed;
            }

            animator.TriggerEquipmentSwapIn();
            return CharacterCommandExecutionStatus.Executed;
        }

        public CharacterCommandExecutionStatus TryAdvance(
            EquipmentComponent equipment,
            AnimatorComponent animator)
        {
            if (_phase == SwapPhase.None)
            {
                return CharacterCommandExecutionStatus.Executed;
            }

            if (_phase != SwapPhase.SwapOutCompleted)
            {
                return CharacterCommandExecutionStatus.TemporarilyBlocked;
            }

            _phase = SwapPhase.SwapIn;
            equipment.SwitchActive(EquipmentSlotGroup.RightHandArmament);
            if (animator.IsNoWeaponMode)
            {
                _phase = SwapPhase.None;
                return CharacterCommandExecutionStatus.Executed;
            }

            animator.TriggerEquipmentSwapIn();
            return CharacterCommandExecutionStatus.TemporarilyBlocked;
        }

        public void HandleAnimationState(in AnimatorStateMachineDto state)
        {
            if (state.State != StateMachineState.Exit)
            {
                return;
            }

            if (state.StateMachineName == StateMachineName.EquipmentSwapOut)
            {
                if (_phase != SwapPhase.SwapOut)
                {
                    throw new InvalidOperationException(
                        $"Equipment swap-out exited during phase '{_phase}'.");
                }

                _phase = SwapPhase.SwapOutCompleted;
            }
            else if (state.StateMachineName == StateMachineName.EquipmentSwapIn)
            {
                if (_phase != SwapPhase.SwapIn)
                {
                    throw new InvalidOperationException(
                        $"Equipment swap-in exited during phase '{_phase}'.");
                }

                _phase = SwapPhase.None;
            }
        }
    }
}
