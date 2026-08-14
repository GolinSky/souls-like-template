using System;
using System.Collections.Generic;
using SoulsLike.Items;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public enum EquipmentSlotGroup
    {
        RightHandArmament = 0,
        LeftHandArmament = 1,
        Arrow = 2,
        Bolt = 3,
        Armor = 4,
        Talisman = 5,
        QuickItem = 6
    }

    public enum EquipmentSlotId
    {
        RightHand1 = 0,
        RightHand2 = 1,
        RightHand3 = 2,
        LeftHand1 = 3,
        LeftHand2 = 4,
        LeftHand3 = 5,
        Arrow1 = 6,
        Arrow2 = 7,
        Bolt1 = 8,
        Bolt2 = 9,
        Head = 10,
        Chest = 11,
        Arms = 12,
        Legs = 13,
        Talisman1 = 14,
        Talisman2 = 15,
        Talisman3 = 16,
        Talisman4 = 17,
        QuickItem1 = 18,
        QuickItem2 = 19,
        QuickItem3 = 20,
        QuickItem4 = 21,
        QuickItem5 = 22,
        QuickItem6 = 23,
        QuickItem7 = 24,
        QuickItem8 = 25,
        QuickItem9 = 26,
        QuickItem10 = 27
    }

    public static class EquipmentSlotCatalog
    {
        private static readonly EquipmentSlotId[] RightHandSlots =
        {
            EquipmentSlotId.RightHand1,
            EquipmentSlotId.RightHand2,
            EquipmentSlotId.RightHand3
        };

        private static readonly EquipmentSlotId[] LeftHandSlots =
        {
            EquipmentSlotId.LeftHand1,
            EquipmentSlotId.LeftHand2,
            EquipmentSlotId.LeftHand3
        };

        private static readonly EquipmentSlotId[] ArrowSlots =
        {
            EquipmentSlotId.Arrow1,
            EquipmentSlotId.Arrow2
        };

        private static readonly EquipmentSlotId[] BoltSlots =
        {
            EquipmentSlotId.Bolt1,
            EquipmentSlotId.Bolt2
        };

        private static readonly EquipmentSlotId[] ArmorSlots =
        {
            EquipmentSlotId.Head,
            EquipmentSlotId.Chest,
            EquipmentSlotId.Arms,
            EquipmentSlotId.Legs
        };

        private static readonly EquipmentSlotId[] TalismanSlots =
        {
            EquipmentSlotId.Talisman1,
            EquipmentSlotId.Talisman2,
            EquipmentSlotId.Talisman3,
            EquipmentSlotId.Talisman4
        };

        private static readonly EquipmentSlotId[] QuickItemSlots =
        {
            EquipmentSlotId.QuickItem1,
            EquipmentSlotId.QuickItem2,
            EquipmentSlotId.QuickItem3,
            EquipmentSlotId.QuickItem4,
            EquipmentSlotId.QuickItem5,
            EquipmentSlotId.QuickItem6,
            EquipmentSlotId.QuickItem7,
            EquipmentSlotId.QuickItem8,
            EquipmentSlotId.QuickItem9,
            EquipmentSlotId.QuickItem10
        };

        public static IReadOnlyList<EquipmentSlotId> GetSlots(EquipmentSlotGroup group)
        {
            return group switch
            {
                EquipmentSlotGroup.RightHandArmament => RightHandSlots,
                EquipmentSlotGroup.LeftHandArmament => LeftHandSlots,
                EquipmentSlotGroup.Arrow => ArrowSlots,
                EquipmentSlotGroup.Bolt => BoltSlots,
                EquipmentSlotGroup.Armor => ArmorSlots,
                EquipmentSlotGroup.Talisman => TalismanSlots,
                EquipmentSlotGroup.QuickItem => QuickItemSlots,
                _ => throw new ArgumentOutOfRangeException(nameof(group), group, null)
            };
        }

        public static EquipmentSlotGroup GetGroup(EquipmentSlotId slotId)
        {
            return slotId switch
            {
                >= EquipmentSlotId.RightHand1 and <= EquipmentSlotId.RightHand3
                    => EquipmentSlotGroup.RightHandArmament,
                >= EquipmentSlotId.LeftHand1 and <= EquipmentSlotId.LeftHand3
                    => EquipmentSlotGroup.LeftHandArmament,
                EquipmentSlotId.Arrow1 or EquipmentSlotId.Arrow2
                    => EquipmentSlotGroup.Arrow,
                EquipmentSlotId.Bolt1 or EquipmentSlotId.Bolt2
                    => EquipmentSlotGroup.Bolt,
                >= EquipmentSlotId.Head and <= EquipmentSlotId.Legs
                    => EquipmentSlotGroup.Armor,
                >= EquipmentSlotId.Talisman1 and <= EquipmentSlotId.Talisman4
                    => EquipmentSlotGroup.Talisman,
                >= EquipmentSlotId.QuickItem1 and <= EquipmentSlotId.QuickItem10
                    => EquipmentSlotGroup.QuickItem,
                _ => throw new ArgumentOutOfRangeException(nameof(slotId), slotId, null)
            };
        }

        public static EquipmentGroup GetCompatibilityGroup(EquipmentSlotId slotId)
        {
            return slotId switch
            {
                >= EquipmentSlotId.RightHand1 and <= EquipmentSlotId.LeftHand3
                    => EquipmentGroup.Armament,
                EquipmentSlotId.Arrow1 or EquipmentSlotId.Arrow2
                    => EquipmentGroup.Arrow,
                EquipmentSlotId.Bolt1 or EquipmentSlotId.Bolt2
                    => EquipmentGroup.Bolt,
                EquipmentSlotId.Head => EquipmentGroup.HeadArmor,
                EquipmentSlotId.Chest => EquipmentGroup.ChestArmor,
                EquipmentSlotId.Arms => EquipmentGroup.ArmArmor,
                EquipmentSlotId.Legs => EquipmentGroup.LegArmor,
                >= EquipmentSlotId.Talisman1 and <= EquipmentSlotId.Talisman4
                    => EquipmentGroup.Talisman,
                >= EquipmentSlotId.QuickItem1 and <= EquipmentSlotId.QuickItem10
                    => EquipmentGroup.QuickItem,
                _ => throw new ArgumentOutOfRangeException(nameof(slotId), slotId, null)
            };
        }

        public static bool IsCyclable(EquipmentSlotGroup group)
        {
            return group is EquipmentSlotGroup.RightHandArmament
                or EquipmentSlotGroup.LeftHandArmament
                or EquipmentSlotGroup.QuickItem;
        }

        public static string GetDisplayName(EquipmentSlotId slotId)
        {
            return slotId switch
            {
                EquipmentSlotId.RightHand1 => "Right Armament 1",
                EquipmentSlotId.RightHand2 => "Right Armament 2",
                EquipmentSlotId.RightHand3 => "Right Armament 3",
                EquipmentSlotId.LeftHand1 => "Left Armament 1",
                EquipmentSlotId.LeftHand2 => "Left Armament 2",
                EquipmentSlotId.LeftHand3 => "Left Armament 3",
                EquipmentSlotId.Arrow1 => "Arrow 1",
                EquipmentSlotId.Arrow2 => "Arrow 2",
                EquipmentSlotId.Bolt1 => "Bolt 1",
                EquipmentSlotId.Bolt2 => "Bolt 2",
                EquipmentSlotId.Head => "Head",
                EquipmentSlotId.Chest => "Chest",
                EquipmentSlotId.Arms => "Arms",
                EquipmentSlotId.Legs => "Legs",
                >= EquipmentSlotId.Talisman1 and <= EquipmentSlotId.Talisman4
                    => $"Talisman {(int)slotId - (int)EquipmentSlotId.Talisman1 + 1}",
                >= EquipmentSlotId.QuickItem1 and <= EquipmentSlotId.QuickItem10
                    => $"Quick Item {(int)slotId - (int)EquipmentSlotId.QuickItem1 + 1}",
                _ => throw new ArgumentOutOfRangeException(nameof(slotId), slotId, null)
            };
        }
    }
}
