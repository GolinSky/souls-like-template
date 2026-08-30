using System;
using SoulsLike.Items;
using UnityEngine;
using VContainer;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public class EquipmentPresentation : MonoBehaviour
    {
        [SerializeField] private Transform rightHandAnchor;
        [SerializeField] private Transform leftHandAnchor;
        [SerializeField] private WeaponRuntime rightFistRuntime;

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
            if (rightHandAnchor == null
                || leftHandAnchor == null
                || rightFistRuntime == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(EquipmentPresentation)} '{name}' requires hand anchors and a fist runtime.");
            }

            rightFistRuntime.Initialize(default, ItemId.Fist);
        }

        public void ApplyLoadout(EquipmentLoadout loadout)
        {
            ClearInstance(ref _rightInstance);
            ClearInstance(ref _leftInstance);
            ActiveRightWeaponRuntime = null;
            ActiveLeftWeaponRuntime = null;

            if (loadout.EffectiveRight != null)
            {
                if (_itemCatalog.GetItem(loadout.EffectiveRight.ItemId).ItemType == ItemType.Weapon)
                {
                    ActiveRightWeaponRuntime = CreatePresentation(
                        loadout.EffectiveRight,
                        rightHandAnchor,
                        false);
                    _rightInstance = ActiveRightWeaponRuntime.gameObject;
                }
                else
                {
                    _rightInstance = CreateShieldPresentation(
                        loadout.EffectiveRight,
                        rightHandAnchor);
                }

                _rightInstance.SetActive(_isRightHandVisible);
            }

            if (loadout.EffectiveLeft != null)
            {
                if (_itemCatalog.GetItem(loadout.EffectiveLeft.ItemId).ItemType == ItemType.Weapon)
                {
                    ActiveLeftWeaponRuntime = CreatePresentation(
                        loadout.EffectiveLeft,
                        leftHandAnchor,
                        true);
                    _leftInstance = ActiveLeftWeaponRuntime.gameObject;
                }
                else
                {
                    _leftInstance = CreateShieldPresentation(
                        loadout.EffectiveLeft,
                        leftHandAnchor);
                }

                _leftInstance.SetActive(_isLeftHandVisible);
            }

            if (loadout.EffectiveRight == null
                && ActiveLeftWeaponRuntime == null)
            {
                ActiveRightWeaponRuntime = rightFistRuntime;
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

        private WeaponRuntime CreatePresentation(
            EquippedItemContext context,
            Transform anchor,
            bool isLeftHand)
        {
            WeaponRuntime prefab = _itemCatalog.GetWeapon(context.ItemId).EquippedPrefab;
            WeaponRuntime instance = Instantiate<WeaponRuntime>(prefab, anchor, false);

            if (isLeftHand)
            {
                instance.transform.localPosition = _leftHandSwordLocalPosition;
                instance.transform.localRotation = _leftHandSwordLocalRotation;
            }

            instance.Initialize(context.Entry.EntryId, context.ItemId);
            return instance;
        }

        private GameObject CreateShieldPresentation(
            EquippedItemContext context,
            Transform anchor)
        {
            GameObject prefab = _itemCatalog.GetShield(context.ItemId).EquippedPrefab;
            if (prefab != null)
            {
                return Instantiate(prefab, anchor, false);
            }

            GameObject instance = new($"Runtime_{context.ItemId}");
            instance.transform.SetParent(anchor, false);
            return instance;
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
