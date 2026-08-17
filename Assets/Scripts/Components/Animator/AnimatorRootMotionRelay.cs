using System;
using UnityEngine;
using SoulsLike.Entities.Character.Ports;

namespace SoulsLike.Entities.Character.Components
{
    [RequireComponent(typeof(Animator))]
    public sealed class AnimatorRootMotionRelay : MonoBehaviour
    {
        private const string ROOT_MOTION_TAG = "RootMotion";
        private const string MOVEMENT_BLOCKED_TAG = "MovementBlocked";

        private Animator _animator;
        private IRootMotionSink _rootMotionSink;
        private bool _movementBlocked;
        private bool _usesRootMotion;
        private bool _initialized;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator == null)
            {
                throw new InvalidOperationException($"{name} requires an Animator.");
            }
        }

        public void Initialize(IRootMotionSink rootMotionSink)
        {
            _rootMotionSink = rootMotionSink;
            _initialized = true;
        }

        public void BeginRootMotionContract()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException($"{name} root motion relay is not initialized.");
            }

            SynchronizeMovementContract(true, true);
        }

        private void OnAnimatorMove()
        {
            if (!_initialized)
            {
                return;
            }

            if (_animator == null)
            {
                throw new InvalidOperationException($"{name} root motion relay is not initialized.");
            }

            bool usesRootMotion = HasActiveStateTag(ROOT_MOTION_TAG);
            bool movementBlocked = usesRootMotion || HasActiveStateTag(MOVEMENT_BLOCKED_TAG);
            SynchronizeMovementContract(movementBlocked, usesRootMotion);
            _rootMotionSink.ApplyRootMotion(_animator.deltaPosition, _animator.deltaRotation);
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
            _rootMotionSink.SetAnimationMotionContract(movementBlocked, usesRootMotion);
        }

        private void OnDisable()
        {
            if (_initialized && (_movementBlocked || _usesRootMotion))
            {
                _movementBlocked = false;
                _usesRootMotion = false;
                _rootMotionSink.SetAnimationMotionContract(false, false);
            }
        }
    }
}
