using SoulsLike.Items;
using UnityEngine;

namespace SoulsLike.Entities.Combat
{
    public readonly struct MeleeHitRequest
    {
        public long AttackerEntityId { get; }
        public ItemId WeaponId { get; }
        public int AttackInstanceId { get; }
        public Vector3 AttackerPosition { get; }
        public Vector3 ContactPoint { get; }
        public int HitZone { get; }
        public MeleeAttackData Attack { get; }

        public MeleeHitRequest(
            long attackerEntityId,
            ItemId weaponId,
            int attackInstanceId,
            Vector3 attackerPosition,
            Vector3 contactPoint,
            int hitZone,
            in MeleeAttackData attack)
        {
            AttackerEntityId = attackerEntityId;
            WeaponId = weaponId;
            AttackInstanceId = attackInstanceId;
            AttackerPosition = attackerPosition;
            ContactPoint = contactPoint;
            HitZone = hitZone;
            Attack = attack;
        }
    }
}
