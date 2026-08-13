using System;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components.Equipment
{
    public class EquipmentComponent : BaseComponent<EquipmentModel>, IInitializable
    {
        [SerializeField] private Transform _equipmentParent;
        [SerializeField] private Transform _weaponAnchor;

        private IComponentMediator _componentMediator;
        private int _activeSlotIndex = -1;

        public void Initialize()
        {
            if (_equipmentParent == null)
            {
                throw new InvalidOperationException($"{name} requires an equipment parent.");
            }
        }

        public HandMode SwitchHandMode()
        {
            HandMode handMode = Model.ActiveHandMode switch
            {
                HandMode.OneHanded => HandMode.TwoHanded,
                HandMode.TwoHanded => HandMode.OneHanded,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(Model.ActiveHandMode),
                    Model.ActiveHandMode,
                    null)
            };

            Model.SetHandMode(handMode);
            return handMode;
        }

        public void SetMediator(IComponentMediator componentMediator)
        {
            _componentMediator = componentMediator;
        }
    }
}
