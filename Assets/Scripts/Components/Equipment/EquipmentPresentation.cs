using System;
using SoulsLike.Items;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public sealed class EquipmentPresentation : MonoBehaviour
    {
        [SerializeField] private Transform _rightHandAnchor;
        [SerializeField] private Transform _leftHandAnchor;

        private GameObject _rightInstance;
        private GameObject _leftInstance;

        public WeaponRuntime ActiveRightWeaponRuntime { get; private set; }

        public void Configure(Transform rightHandAnchor, Transform leftHandAnchor)
        {
            _rightHandAnchor = rightHandAnchor
                ?? throw new ArgumentNullException(nameof(rightHandAnchor));
            _leftHandAnchor = leftHandAnchor
                ?? throw new ArgumentNullException(nameof(leftHandAnchor));
        }

        public void ApplyLoadout(EquipmentLoadout loadout)
        {
            ClearInstance(ref _rightInstance);
            ClearInstance(ref _leftInstance);
            ActiveRightWeaponRuntime = null;

            if (loadout.EffectiveRight != null)
            {
                _rightInstance = CreatePresentation(loadout.EffectiveRight, _rightHandAnchor);
                if (loadout.EffectiveRight.Definition is WeaponDefinition rightWeapon)
                {
                    ActiveRightWeaponRuntime = RequireWeaponRuntime(
                        _rightInstance,
                        loadout.EffectiveRight,
                        rightWeapon);
                }
            }

            if (loadout.EffectiveLeft != null)
            {
                _leftInstance = CreatePresentation(loadout.EffectiveLeft, _leftHandAnchor);
                if (loadout.EffectiveLeft.Definition is WeaponDefinition leftWeapon)
                {
                    RequireWeaponRuntime(_leftInstance, loadout.EffectiveLeft, leftWeapon);
                }
            }
        }

        private static GameObject CreatePresentation(
            EquippedItemContext context,
            Transform anchor)
        {
            GameObject prefab = context.Definition switch
            {
                WeaponDefinition weapon => weapon.EquippedPrefab,
                ShieldDefinition shield => shield.EquippedPrefab,
                _ => null
            };

            GameObject instance;
            if (prefab == null)
            {
                instance = new GameObject($"Runtime_{context.Definition.ItemId}");
                instance.transform.SetParent(anchor, false);
            }
            else
            {
                instance = Instantiate(prefab, anchor, false);
            }

            return instance;
        }

        private static WeaponRuntime RequireWeaponRuntime(
            GameObject instance,
            EquippedItemContext context,
            WeaponDefinition definition)
        {
            if (!instance.TryGetComponent(out WeaponRuntime weaponRuntime))
            {
                weaponRuntime = instance.AddComponent<WeaponRuntime>();
            }

            weaponRuntime.Initialize(context.Entry.EntryId, definition);
            return weaponRuntime;
        }

        private static void ClearInstance(ref GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            Destroy(instance);
            instance = null;
        }
    }
}
