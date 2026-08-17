using SoulsLike.Entities.Character.Components.Equipment;

namespace SoulsLike.Entities.Character.Ports
{
    public interface IEquipmentLoadoutSink
    {
        void ApplyEquipmentLoadout(EquipmentLoadout loadout);
    }
}
