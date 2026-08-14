using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Inventory;

namespace SoulsLike.Ui.Equipment
{
    public interface IEquipmentPresenter
    {
        void FocusSlot(EquipmentSlotId slotId);
        void SubmitSlot(EquipmentSlotId slotId);
        void FocusCandidate(InventoryEntryId entryId);
        void SubmitCandidate(InventoryEntryId entryId);
        void UnequipSelectedSlot();
        void CancelPicker();
        void CloseEquipment();
    }
}
