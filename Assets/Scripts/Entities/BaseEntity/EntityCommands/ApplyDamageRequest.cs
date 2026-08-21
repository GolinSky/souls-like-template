using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Combat;
using SoulsLike.Items;

namespace SoulsLike.Entities.BaseEntity.EntityCommands
{
    public readonly struct ApplyDamageRequest
    {
        public long SourceEntityId { get; }
        public ItemId WeaponId { get; }
        public CharacterActionId ActionId { get; }
        public DamageRequest Damage { get; }

        public ApplyDamageRequest(long sourceEntityId, ItemId weaponId, CharacterActionId actionId, DamageRequest damage)
        {
            SourceEntityId = sourceEntityId;
            WeaponId = weaponId;
            ActionId = actionId;
            Damage = damage;
        }
    }
}
