using System;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components
{
    [RequireComponent(typeof(Animator))]
    public sealed class AnimatorRootMotionRelay : MonoBehaviour
    {
        private const string ROOT_MOTION_TAG = "RootMotion";
        private const string MOVEMENT_BLOCKED_TAG = "MovementBlocked";

        private Animator _animator;
        private IComponentMediator _mediator;
        private bool _movementBlocked;
        private bool _usesRootMotion;

        public void Initialize(IComponentMediator mediator)
        {
            _mediator = mediator;
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                throw new InvalidOperationException($"{name} requires an Animator.");
            }
        }

        public void BeginRootMotionContract()
        {
            if (_mediator == null)
            {
                throw new InvalidOperationException($"{name} root motion relay is not initialized.");
            }

            SynchronizeMovementContract(true, true);
        }

        private void OnAnimatorMove()
        {
            if (_animator == null || _mediator == null)
            {
                throw new InvalidOperationException($"{name} root motion relay is not initialized.");
            }

            bool usesRootMotion = HasActiveStateTag(ROOT_MOTION_TAG);
            bool movementBlocked = usesRootMotion || HasActiveStateTag(MOVEMENT_BLOCKED_TAG);
            SynchronizeMovementContract(movementBlocked, usesRootMotion);
            _mediator.NotifyAnimationMovement(_animator.deltaPosition, _animator.deltaRotation);
        }

        private bool HasActiveStateTag(string tag)
        {
            for (int layer = 0; layer < _animator.layerCount; layer++)
            {
                if (_animator.GetCurrentAnimatorStateInfo(layer).IsTag(tag))
                {
                    return true;
                }

                if (_animator.IsInTransition(layer) && _animator.GetNextAnimatorStateInfo(layer).IsTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        private void SynchronizeMovementContract(bool movementBlocked, bool usesRootMotion)
        {
            if (_movementBlocked == movementBlocked && _usesRootMotion == usesRootMotion)
            {
                return;
            }

            _movementBlocked = movementBlocked;
            _usesRootMotion = usesRootMotion;
            _mediator.SetAnimationMovementContract(movementBlocked, usesRootMotion);
        }

        private void OnDisable()
        {
            if (_mediator != null && (_movementBlocked || _usesRootMotion))
            {
                _movementBlocked = false;
                _usesRootMotion = false;
                _mediator.SetAnimationMovementContract(false, false);
            }
        }
    }
}
