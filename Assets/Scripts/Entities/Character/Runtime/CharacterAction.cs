using UnityEngine;

namespace SoulsLike.Entities.Character.Runtime
{
    public readonly struct CharacterAction
    {
        public enum Kind { Attack, Roll, Jump, Equipment }
        public enum AttackIntent { Light, Heavy, Special }
        public enum EquipmentKind { SwitchRightWeapon, SwitchLeftWeapon, SwitchQuickItem, UseQuickItem, ToggleHandMode }
        public enum Result { Executed, TemporarilyBlocked, Invalid }
        public enum State { Neutral, Attack, Roll, EquipmentSwap, Critical, ItemUse, BlockHit }

        public Kind ActionKind { get; }
        public AttackIntent Intent { get; }
        public EquipmentKind EquipmentAction { get; }
        public bool IsLeftHand { get; }
        public bool IsSprinting { get; }
        public Vector2 MoveInput { get; }
        public float CameraYaw { get; }
        public bool CanBuffer => ActionKind != Kind.Equipment || EquipmentAction == EquipmentKind.UseQuickItem;

        private CharacterAction(Kind actionKind, AttackIntent intent, EquipmentKind equipmentAction, bool isLeftHand, bool isSprinting, Vector2 moveInput, float cameraYaw)
        {
            ActionKind = actionKind;
            Intent = intent;
            EquipmentAction = equipmentAction;
            IsLeftHand = isLeftHand;
            IsSprinting = isSprinting;
            MoveInput = moveInput;
            CameraYaw = cameraYaw;
        }

        public static CharacterAction Attack(AttackIntent intent, bool isLeftHand, bool isSprinting, Vector2 moveInput, float cameraYaw) =>
            new CharacterAction(Kind.Attack, intent, default, isLeftHand, isSprinting, moveInput, cameraYaw);
        public static CharacterAction Roll(Vector2 moveInput, float cameraYaw) =>
            new CharacterAction(Kind.Roll, default, default, false, false, moveInput, cameraYaw);
        public static CharacterAction Jump(bool isSprinting) =>
            new CharacterAction(Kind.Jump, default, default, false, isSprinting, default, 0f);
        public static CharacterAction Equipment(EquipmentKind equipmentAction) =>
            new CharacterAction(Kind.Equipment, default, equipmentAction, false, false, default, 0f);
    }
}
