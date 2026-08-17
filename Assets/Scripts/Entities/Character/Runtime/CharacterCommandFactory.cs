using UnityEngine;

namespace SoulsLike.Entities.Character.Runtime
{
    public sealed class CharacterCommandFactory
    {
        private readonly IAttackCommandReceiver _attack;
        private readonly IMovementCommandReceiver _movement;
        private readonly IEquipmentCommandReceiver _equipment;

        public CharacterCommandFactory(IAttackCommandReceiver attack, IMovementCommandReceiver movement,
            IEquipmentCommandReceiver equipment)
        { _attack = attack; _movement = movement; _equipment = equipment; }

        public ICharacterCommand CreateAttack(AttackIntent intent, bool leftHand, bool sprinting) => new AttackCommand(_attack, new AttackRequest(intent, leftHand, sprinting));
        public ICharacterCommand CreateRoll(Vector2 moveInput, float cameraYaw, bool canInterrupt) => new RollCommand(_movement, new RollRequest(moveInput, cameraYaw, canInterrupt));
        public ICharacterCommand CreateJump(bool sprinting) => new JumpCommand(_movement, new JumpRequest(sprinting));
        public ICharacterCommand CreateEquipmentAction(int actionId) => new EquipmentCommand(_equipment, new EquipmentActionRequest(actionId));
    }
}
