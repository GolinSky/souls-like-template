using System;
using System.Collections.Generic;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using VContainer;
using VContainer.Unity;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Runtime;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public sealed class EquipmentComponent : BaseComponent<EquipmentModel>, IInitializable, IDisposable
    {
        private InventoryComponent _inventory;
        private ItemCatalog _itemCatalog;
        private Character _character;
        private AnimatorComponent _animator;
        private EquipmentPresentation _presentation;
        private SwapPhase _swapPhase;
        private EquipmentSlotGroup _swapSlotGroup;

        public event Action<EquipmentSlotChange> SlotChanged;
        public event Action<EquipmentLoadout> LoadoutChanged;

        //todo: avoid other component dependency (InventoryComponent)
        [Inject]
        public void InjectDependencies(InventoryComponent inventory, ItemCatalog itemCatalog, Character character, AnimatorComponent animator, EquipmentPresentation presentation)
        {
            _inventory = inventory;
            _itemCatalog = itemCatalog;
            _character = character;
            _animator = animator;
            _presentation = presentation;
        }

        public void Initialize()
        {
            _inventory.Model.Changed += HandleInventoryChanged;
        }

        public void Dispose()
        {
            if (_inventory != null)
            {
                _inventory.Model.Changed -= HandleInventoryChanged;
            }
        }

        public void Assign(EquipmentSlotId slotId, InventoryEntryId entryId)
        {
            InventoryEntry entry = _inventory.GetRequiredEntry(entryId);
            ItemDefinition definition = _itemCatalog.GetItem(entry.ItemId);
            EquipmentGroup requiredGroup = EquipmentSlotCatalog.GetCompatibilityGroup(slotId);
            if (!definition.CanEquipIn(requiredGroup))
            {
                throw new InvalidOperationException(
                    $"Item '{definition.DisplayName}' cannot be assigned to '{slotId}'.");
            }

            if (TryGetFirstAssignedSlot(entryId, out EquipmentSlotId existingSlot)
                && existingSlot != slotId)
            {
                throw new InvalidOperationException(
                    $"Inventory entry '{entryId}' is already assigned to '{existingSlot}'.");
            }

            InventoryEntryId? previous = Model.SetAssignment(slotId, entryId);
            PublishSlotChange(slotId, previous, entryId);
        }

        public void Unequip(EquipmentSlotId slotId)
        {
            InventoryEntryId? previous = Model.GetAssignedEntryId(slotId);
            if (!previous.HasValue)
            {
                return;
            }

            Model.SetAssignment(slotId, null);
            PublishSlotChange(slotId, previous, null);
        }

        public EquipmentSlotId SwitchActive(EquipmentSlotGroup group)
        {
            EquipmentSlotId previousActiveSlot = Model.GetActiveSlot(group);
            EquipmentSlotId activeSlot = Model.AdvanceActiveSlot(group);
            if (activeSlot != previousActiveSlot)
            {
                PublishLoadoutChanged();
            }

            return activeSlot;
        }

        public bool TrySwitchHandMode(out HandMode handMode)
        {
            EquippedItemContext right = ResolveActiveItem(EquipmentSlotGroup.RightHandArmament);
            if (right == null
                || _itemCatalog.GetItem(right.ItemId).ItemType != ItemType.Weapon
                || !_itemCatalog.GetWeapon(right.ItemId).CanTwoHand)
            {
                handMode = Model.ActiveHandMode;
                return false;
            }

            handMode = Model.ActiveHandMode switch
            {
                HandMode.OneHanded => HandMode.TwoHanded,
                HandMode.TwoHanded => HandMode.OneHanded,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(Model.ActiveHandMode),
                    Model.ActiveHandMode,
                    null)
            };
            Model.SetHandMode(handMode);
            PublishLoadoutChanged();
            return true;
        }

        public bool IsSwapInProgress => _swapPhase != SwapPhase.None;

        public CharacterAction.Result StartSwap(EquipmentSlotGroup slotGroup)
        {
            if (_swapPhase != SwapPhase.None) return CharacterAction.Result.TemporarilyBlocked;
            _swapSlotGroup = slotGroup;
            if (GetEquippedItem(BuildLoadout(), slotGroup) != null)
            {
                _swapPhase = SwapPhase.SwapOut;
                _animator.TriggerEquipmentSwapOut(slotGroup);
                return CharacterAction.Result.Executed;
            }
            SwapEquipment();
            return CharacterAction.Result.Executed;
        }

        public void HandleAnimationState(in AnimatorStateMachineDto state)
        {
            if (state.State == StateMachineState.Progress)
            {
                if (state.StateMachineName == StateMachineName.EquipmentSwapOut && _swapPhase == SwapPhase.SwapOut)
                {
                    _presentation.SetArmamentVisible(_swapSlotGroup, false);
                    _swapPhase = SwapPhase.SwapOutHidden;
                }
                else if (state.StateMachineName == StateMachineName.EquipmentSwapIn && _swapPhase == SwapPhase.SwapIn) _presentation.SetArmamentVisible(_swapSlotGroup, true);
                return;
            }
            if (state.State != StateMachineState.Exit) return;
            if (state.StateMachineName == StateMachineName.EquipmentSwapOut && _swapPhase == SwapPhase.SwapOutHidden)
            {
                _swapPhase = SwapPhase.None;
                SwapEquipment();
            }
            else if (state.StateMachineName == StateMachineName.EquipmentSwapIn && _swapPhase == SwapPhase.SwapIn) _swapPhase = SwapPhase.None;
        }

        public void CancelSwap()
        {
            if (_swapPhase == SwapPhase.None) return;
            _presentation.SetArmamentVisible(_swapSlotGroup, true);
            _swapPhase = SwapPhase.None;
        }

        public InventoryEntryId? GetAssignedEntryId(EquipmentSlotId slotId)
        {
            return Model.GetAssignedEntryId(slotId);
        }

        public bool IsEquipped(InventoryEntryId entryId)
        {
            foreach (EquipmentSlotId slotId in Enum.GetValues(typeof(EquipmentSlotId)))
            {
                if (Model.GetAssignedEntryId(slotId) == entryId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetFirstAssignedSlot(
            InventoryEntryId entryId,
            out EquipmentSlotId slotId)
        {
            foreach (EquipmentSlotId candidate in Enum.GetValues(typeof(EquipmentSlotId)))
            {
                if (Model.GetAssignedEntryId(candidate) == entryId)
                {
                    slotId = candidate;
                    return true;
                }
            }

            slotId = default;
            return false;
        }

        public EquipmentLoadout BuildLoadout()
        {
            return new EquipmentLoadout(
                ResolveActiveItem(EquipmentSlotGroup.RightHandArmament),
                ResolveActiveItem(EquipmentSlotGroup.LeftHandArmament),
                ResolveActiveItem(EquipmentSlotGroup.QuickItem),
                Model.ActiveHandMode);
        }

        public EquippedItemContext ResolveSlot(EquipmentSlotId slotId)
        {
            InventoryEntryId? entryId = Model.GetAssignedEntryId(slotId);
            if (!entryId.HasValue)
            {
                return null;
            }

            InventoryEntry entry = _inventory.GetRequiredEntry(entryId.Value);
            return new EquippedItemContext(slotId, entry);
        }

        public IReadOnlyList<InventoryEntry> GetCompatibleEntries(EquipmentSlotId slotId)
        {
            EquipmentGroup group = EquipmentSlotCatalog.GetCompatibilityGroup(slotId);
            var result = new List<InventoryEntry>();
            foreach (InventoryEntry entry in _inventory.Entries)
            {
                if (_itemCatalog.GetItem(entry.ItemId).CanEquipIn(group))
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        private EquippedItemContext ResolveActiveItem(EquipmentSlotGroup group)
        {
            return ResolveSlot(Model.GetActiveSlot(group));
        }

        private void PublishSlotChange(
            EquipmentSlotId slotId,
            InventoryEntryId? previous,
            InventoryEntryId? current)
        {
            SlotChanged?.Invoke(new EquipmentSlotChange(slotId, previous, current));
            PublishLoadoutChanged();
        }

        private void PublishLoadoutChanged()
        {
            NormalizeHandMode();
            EquipmentLoadout loadout = BuildLoadout();
            LoadoutChanged?.Invoke(loadout);
            _character.ApplyEquipmentLoadout(loadout);
        }

        private void NormalizeHandMode()
        {
            if (Model.ActiveHandMode != HandMode.TwoHanded)
            {
                return;
            }

            EquippedItemContext right = ResolveActiveItem(
                EquipmentSlotGroup.RightHandArmament);
            if (right != null
                && _itemCatalog.GetItem(right.ItemId).ItemType == ItemType.Weapon
                && _itemCatalog.GetWeapon(right.ItemId).CanTwoHand)
            {
                return;
            }

            Model.SetHandMode(HandMode.OneHanded);
        }

        private void HandleInventoryChanged(InventoryChange change)
        {
            if (change.Type != InventoryChangeType.Removed)
            {
                return;
            }

            var slotsToClear = new List<EquipmentSlotId>();
            foreach (EquipmentSlotId slotId in Enum.GetValues(typeof(EquipmentSlotId)))
            {
                if (Model.GetAssignedEntryId(slotId) == change.Entry.EntryId)
                {
                    slotsToClear.Add(slotId);
                }
            }

            foreach (EquipmentSlotId slotId in slotsToClear)
            {
                Unequip(slotId);
            }
        }

        private void SwapEquipment()
        {
            EquipmentSlotId previous = Model.GetActiveSlot(_swapSlotGroup);
            _presentation.SetArmamentVisible(_swapSlotGroup, false);
            EquipmentSlotId active = SwitchActive(_swapSlotGroup);
            EquippedItemContext equippedItem = GetEquippedItem(BuildLoadout(), _swapSlotGroup);
            if (active == previous || equippedItem == null)
            {
                _presentation.SetArmamentVisible(_swapSlotGroup, true);
                _swapPhase = SwapPhase.None;
                return;
            }
            _presentation.SetArmamentVisible(_swapSlotGroup, false);
            _swapPhase = SwapPhase.SwapIn;
            _animator.TriggerEquipmentSwapIn(_swapSlotGroup);
        }

        private static EquippedItemContext GetEquippedItem(in EquipmentLoadout loadout, EquipmentSlotGroup slotGroup) =>
            slotGroup == EquipmentSlotGroup.LeftHandArmament ? loadout.EffectiveLeft : loadout.EffectiveRight;

        private enum SwapPhase { None, SwapOut, SwapOutHidden, SwapIn }
    }
}
