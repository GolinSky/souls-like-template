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
        private HandMode _pendingHandMode;
        private bool _isHandModeSwitchInProgress;

        public bool IsHandModeSwitchInProgress => _isHandModeSwitchInProgress;

        public HandMode PendingHandMode
        {
            get
            {
                if (!_isHandModeSwitchInProgress)
                {
                    throw new InvalidOperationException(
                        "Cannot read a pending hand mode when no switch is in progress.");
                }

                return _pendingHandMode;
            }
        }

        public void Initialize()
        {
            if (_equipmentParent == null)
            {
                throw new InvalidOperationException($"{name} requires an equipment parent.");
            }
        }

        public bool TryBeginHandModeSwitch()
        {
            if (_isHandModeSwitchInProgress)
            {
                return false;
            }

            _pendingHandMode = Model.ActiveHandMode switch
            {
                HandMode.OneHanded => HandMode.TwoHanded,
                HandMode.TwoHanded => HandMode.OneHanded,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(Model.ActiveHandMode),
                    Model.ActiveHandMode,
                    null)
            };
            _isHandModeSwitchInProgress = true;
            return true;
        }

        public void CompleteHandModeSwitch()
        {
            if (!_isHandModeSwitchInProgress)
            {
                throw new InvalidOperationException("Cannot complete a hand mode switch that is not in progress.");
            }

            Model.SetHandMode(_pendingHandMode);
            _isHandModeSwitchInProgress = false;
        }

        public void SetMediator(IComponentMediator componentMediator)
        {
            _componentMediator = componentMediator;
        }
    }
}
