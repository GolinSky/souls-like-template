using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public sealed class WeaponRuntime : MonoBehaviour
    {
        [SerializeField] private MeleeHitboxController meleeHitbox;

        private float _temporaryLightningDamage;
        private float _infusionRemainingSeconds;

        public InventoryEntryId EntryId { get; private set; }
        public ItemId ItemId { get; private set; }
        public MeleeHitboxController MeleeHitbox => meleeHitbox;
        public float TemporaryLightningDamage => _temporaryLightningDamage;
        public bool HasTemporaryInfusion => _infusionRemainingSeconds > 0f;

        public void Initialize(InventoryEntryId entryId, ItemId itemId)
        {
            EntryId = entryId;
            ItemId = itemId;
            ClearTemporaryInfusion();
        }

        //todo:rename to ApplyInfusion. No dependencies impl
        public void ApplyLightningInfusion(float damage, float durationSeconds)
        {
            if (damage <= 0f)
            {
                throw new System.ArgumentOutOfRangeException(nameof(damage), damage, null);
            }

            if (durationSeconds <= 0f)
            {
                throw new System.ArgumentOutOfRangeException(nameof(durationSeconds), durationSeconds, null);
            }

            _temporaryLightningDamage = damage;
            _infusionRemainingSeconds = durationSeconds;
        }

        private void Update()
        {
            if (_infusionRemainingSeconds <= 0f)
            {
                return;
            }

            _infusionRemainingSeconds = Mathf.Max(0f, _infusionRemainingSeconds - Time.deltaTime);
            if (_infusionRemainingSeconds <= 0f)
            {
                ClearTemporaryInfusion();
            }
        }

        private void ClearTemporaryInfusion()
        {
            _temporaryLightningDamage = 0f;
            _infusionRemainingSeconds = 0f;
        }
    }
}
