using System;
using SoulsLike.Items;
using UnityEngine;
using VContainer;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public sealed class EquipmentPresentation : MonoBehaviour
    {
        [SerializeField] private Transform rightHandAnchor;
        [SerializeField] private Transform leftHandAnchor;

        private static readonly Vector3 _leftHandSwordLocalPosition = new(0f, 0.11f, 0.039f);
        private static readonly Quaternion _leftHandSwordLocalRotation =
            new(-0.49997228f, -0.4999985f, -0.5000271f, 0.5000021f);

        private GameObject _rightInstance;
        private GameObject _leftInstance;
        private bool _isRightHandVisible = true;
        private bool _isLeftHandVisible = true;
        private ItemCatalog _itemCatalog;

        public WeaponRuntime ActiveRightWeaponRuntime { get; private set; }
        public WeaponRuntime ActiveLeftWeaponRuntime { get; private set; }

        [Inject]
        public void InjectDependencies(ItemCatalog itemCatalog)
        {
            _itemCatalog = itemCatalog;
        }

        private void Awake()
        {
            if (rightHandAnchor == null || leftHandAnchor == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EquipmentPresentation)} '{name}' requires right and left hand anchors.");
            }
        }

        public void ApplyLoadout(EquipmentLoadout loadout)
        {
            ClearInstance(ref _rightInstance);
            ClearInstance(ref _leftInstance);
            ActiveRightWeaponRuntime = null;
            ActiveLeftWeaponRuntime = null;

            if (loadout.EffectiveRight != null)
            {
                _rightInstance = CreatePresentation(loadout.EffectiveRight, rightHandAnchor, false);
                _rightInstance.SetActive(_isRightHandVisible);
                if (_itemCatalog.GetItem(loadout.EffectiveRight.ItemId).ItemType == ItemType.Weapon)
                {
                    ActiveRightWeaponRuntime = RequireWeaponRuntime(
                        _rightInstance,
                        loadout.EffectiveRight);
                }
            }

            if (loadout.EffectiveLeft != null)
            {
                _leftInstance = CreatePresentation(loadout.EffectiveLeft, leftHandAnchor, true);
                _leftInstance.SetActive(_isLeftHandVisible);
                if (_itemCatalog.GetItem(loadout.EffectiveLeft.ItemId).ItemType == ItemType.Weapon)
                {
                    ActiveLeftWeaponRuntime = RequireWeaponRuntime(
                        _leftInstance,
                        loadout.EffectiveLeft);
                }
            }
        }

        public void SetArmamentVisible(EquipmentSlotGroup slotGroup, bool isVisible)
        {
            switch (slotGroup)
            {
                case EquipmentSlotGroup.RightHandArmament:
                    _isRightHandVisible = isVisible;
                    if (_rightInstance != null)
                    {
                        _rightInstance.SetActive(isVisible);
                    }
                    break;
                case EquipmentSlotGroup.LeftHandArmament:
                    _isLeftHandVisible = isVisible;
                    if (_leftInstance != null)
                    {
                        _leftInstance.SetActive(isVisible);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slotGroup), slotGroup, null);
            }
        }

        private GameObject CreatePresentation(
            EquippedItemContext context,
            Transform anchor,
            bool isLeftHand)
        {
            ItemType itemType = _itemCatalog.GetItem(context.ItemId).ItemType;
            GameObject prefab = itemType switch
            {
                ItemType.Weapon => _itemCatalog.GetWeapon(context.ItemId).EquippedPrefab,
                ItemType.Shield => _itemCatalog.GetShield(context.ItemId).EquippedPrefab,
                _ => null
            };

            GameObject instance;
            if (prefab == null)
            {
                instance = new GameObject($"Runtime_{context.ItemId}");
                instance.transform.SetParent(anchor, false);
            }
            else
            {
                instance = Instantiate(prefab, anchor, false);
            }

            if (isLeftHand && itemType == ItemType.Weapon)
            {
                instance.transform.localPosition = _leftHandSwordLocalPosition;
                instance.transform.localRotation = _leftHandSwordLocalRotation;
            }

            return instance;
        }

        private static WeaponRuntime RequireWeaponRuntime(
            GameObject instance,
            EquippedItemContext context)
        {
            if (!instance.TryGetComponent(out WeaponRuntime weaponRuntime))
            {
                weaponRuntime = instance.AddComponent<WeaponRuntime>();
            }

            weaponRuntime.Initialize(context.Entry.EntryId, context.ItemId);
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
