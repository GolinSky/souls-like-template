using System;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Items;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public sealed class WeaponRuntime : MonoBehaviour
    {
        private WeaponDefinition _definition;
        private float _temporaryLightningDamage;
        private float _infusionRemainingSeconds;

        public InventoryEntryId EntryId { get; private set; }
        public WeaponDefinition Definition => _definition;
        public float TemporaryLightningDamage => _temporaryLightningDamage;
        public bool HasTemporaryInfusion => _infusionRemainingSeconds > 0f;

        public void Initialize(InventoryEntryId entryId, WeaponDefinition definition)
        {
            EntryId = entryId;
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            ClearTemporaryInfusion();
        }

        //todo:rename to ApplyInfusion. No dependencies impl
        public void ApplyLightningInfusion(float damage, float durationSeconds)
        {
            if (_definition == null)
            {
                throw new InvalidOperationException("Weapon runtime must be initialized before applying infusion.");
            }

            if (damage <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), damage, null);
            }

            if (durationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds), durationSeconds, null);
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
