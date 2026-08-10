using System;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Animations;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components
{
    /// <summary>
    /// Character animator component
    /// </summary>
    public class AnimatorComponent: BaseComponent<AnimatorModel>, IInitializable,
        Animations.IObserver<AnimatorStateMachineDto>
    {
        private static readonly int _animIdHorizontal = Animator.StringToHash("Horizontal");
        private static readonly int _animIdVertical = Animator.StringToHash("Vertical");
        private static readonly int _animIdGrounded = Animator.StringToHash("Grounded");
        private static readonly int _animIdJump = Animator.StringToHash("Jump");
        private static readonly int _animIdRoll = Animator.StringToHash("Roll");
        private static readonly int _animIdBackStep = Animator.StringToHash("BackStep");
        private static readonly int _animIdRollHorizontal = Animator.StringToHash("RollHorizontal");
        private static readonly int _animIdRollVertical = Animator.StringToHash("RollVertical");
        private static readonly int _animIdCrouch = Animator.StringToHash("Crouch");
        private static readonly int _animIdTurn = Animator.StringToHash("Turn");
        private static readonly int _animIdMoving = Animator.StringToHash("Moving");
        private static readonly int _animIdSpeed = Animator.StringToHash("Speed");
        private static readonly int _animIdLockOn = Animator.StringToHash("LockOn");
        private static readonly int _lightAttackTrigger = Animator.StringToHash("LightAttack");
        private static readonly int _lightAttackAltTrigger = Animator.StringToHash("LightAttackAlt");
        private static readonly int _heavyAttackTrigger = Animator.StringToHash("HeavyAttack");
        private static readonly int _chargedHeavyAttackTrigger = Animator.StringToHash("ChargedHeavyAttack");
        private static readonly int _rollAttackTrigger = Animator.StringToHash("RollAttack");
        private static readonly int _backStepAttackTrigger = Animator.StringToHash("BackStepAttack");
        private static readonly int _runAttackTrigger = Animator.StringToHash("RunAttack");
        private static readonly int _specialAttackTrigger = Animator.StringToHash("SpecialAttack");

        public void SetLockOn(bool isLockedOn)
        {
            _animator.SetBool(_animIdLockOn, isLockedOn);
        }
        
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimatorStateMachineReceiver _stateMachineReceiver;
        [Header("Aim Target")]
        [SerializeField] private Transform _aimTarget;
        [Header("Aim Settings")]
        [SerializeField] private float _aimTrackingSpeed = 10f;
        [SerializeField] private float _aimVerticalOffset;
        [SerializeField] private float _turningLagAmount = 5f;
        [Header("Smooth Transitions")]
        [SerializeField] private float _layerTransitionSpeed = 10f;
        [SerializeField] private float _turnSmoothSpeed = 10f;
        [SerializeField] private float _locomotionSmoothSpeed = 10f;
        
        [field:SerializeField]public Transform RightHandAnchor { get; private set; }

    
        private float _currentTurnAmount;
        private float _targetSpeed;
        private float _currentSpeed;
        private Vector2 _targetLocomotion;
        private Vector2 _currentLocomotion;
        private Vector3 _targetAimPosition;
        private float _targetRiffleLayerWeight;
        private bool _aimTargetInitialized;
        private IComponentMediator _mediator;
        private AnimatorRootMotionRelay _rootMotionRelay;
        private bool _observingStateMachine;
        public Animator Animator => _animator;

        public void Initialize()
        {
            if (_animator == null)
            {
                throw new InvalidOperationException($"{name} requires an Animator.");
            }

            if (_stateMachineReceiver == null)
            {
                throw new InvalidOperationException($"{name} requires an AnimatorStateMachineReceiver.");
            }
        }

        public void SetMediator(IComponentMediator mediator)
        {
            _mediator = mediator;
            _rootMotionRelay = _animator.GetComponent<AnimatorRootMotionRelay>();
            if (_rootMotionRelay == null)
            {
                _rootMotionRelay = _animator.gameObject.AddComponent<AnimatorRootMotionRelay>();
            }

            _rootMotionRelay.Initialize(mediator);
            if (!_observingStateMachine)
            {
                _stateMachineReceiver.AddObserver(this);
                _observingStateMachine = true;
            }

            _animator.applyRootMotion = true;
        }

        public void SetGrounded(bool modelGrounded)
        {
            _animator.SetBool(_animIdGrounded, modelGrounded);
        }

        public void PlayAttack(AttackType attackType)
        {
            int triggerHash = attackType switch
            {
                AttackType.LightAttack => _lightAttackTrigger,
                AttackType.LightAttackAlt => _lightAttackAltTrigger,
                AttackType.RollingLightAttack => _rollAttackTrigger,
                AttackType.SprintingAttack => _runAttackTrigger,
                AttackType.HeavyAttack => _heavyAttackTrigger,
                AttackType.ChargedHeavyAttack => _chargedHeavyAttackTrigger,
                AttackType.SpecialAttack => _specialAttackTrigger,
                AttackType.BackStepAttack => _backStepAttackTrigger,
                _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
            };

            BeginRootMotionAction();
            _animator.SetTrigger(triggerHash);
        }

        public void UpdateState(AnimatorStateMachineDto state)
        {
            _mediator.NotifyAnimatorStateChanged(state);
        }


        public void SetJump()
        {
            _animator.SetTrigger(_animIdJump);
        }
        
        public void TriggerRoll(Vector2 direction)
        {
            _animator.SetFloat(_animIdRollHorizontal, direction.x);
            _animator.SetFloat(_animIdRollVertical, direction.y);
            BeginRootMotionAction();
            _animator.SetTrigger(_animIdRoll);
        }

        public void TriggerBackStep()
        {
            BeginRootMotionAction();
            _animator.SetTrigger(_animIdBackStep);
        }

        private void BeginRootMotionAction()
        {
            if (_rootMotionRelay == null)
            {
                throw new InvalidOperationException($"{name} requires an AnimatorRootMotionRelay.");
            }

            _rootMotionRelay.BeginRootMotionContract();
        }

        private void OnDestroy()
        {
            if (_observingStateMachine)
            {
                _stateMachineReceiver.RemoveObserver(this);
                _observingStateMachine = false;
            }
        }
        

        public void SetCrouch(bool isCrouching)
        {
            _animator.SetBool(_animIdCrouch, isCrouching);
        }
        
   

        public void SetAimTarget(Vector3 targetPosition)
        {
            _targetAimPosition = targetPosition + Vector3.up * _aimVerticalOffset;
            if (!_aimTargetInitialized && _aimTarget != null)
            {
                _aimTarget.position = _targetAimPosition;
                _aimTargetInitialized = true;
            }
        }

        public void SetLocomotion(float speed, Vector2 blendDirection)
        {
            _targetSpeed = speed;
            _targetLocomotion = blendDirection;
        }
        
        private void LateUpdate()
        {
            float dt = Time.deltaTime;

            // 1. Smooth Aim Target Tracking with Turning Lag
            if (_aimTarget != null && _aimTargetInitialized)
            {
                // Calculate turning lag based on rotation velocity from movement
                // We use transform.right to offset the target horizontally
                float turnLag = _targetTurnAmount * _turningLagAmount; 
                Vector3 laggedTarget = _targetAimPosition - transform.right * turnLag;
                
                _aimTarget.position = Vector3.Lerp(_aimTarget.position, laggedTarget, dt * _aimTrackingSpeed);
            }


            // 3. Smooth Animator Layer Weight
            int riffleLayerIndex = _animator.GetLayerIndex("Riffle");
            if (riffleLayerIndex != -1)
            {
                float currentWeight = _animator.GetLayerWeight(riffleLayerIndex);
                float nextWeight = Mathf.Lerp(currentWeight, _targetRiffleLayerWeight, dt * _layerTransitionSpeed);
                _animator.SetLayerWeight(riffleLayerIndex, nextWeight);
            }

            _currentTurnAmount = Mathf.Lerp(_currentTurnAmount, _targetTurnAmount, dt * _turnSmoothSpeed);
            _animator.SetFloat(_animIdTurn, _currentTurnAmount);

            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, dt * _locomotionSmoothSpeed);
            if (_currentSpeed < 0.01f) _currentSpeed = 0f;
            _animator.SetFloat(_animIdSpeed, _currentSpeed);

            _currentLocomotion = Vector2.Lerp(_currentLocomotion, _targetLocomotion, dt * _locomotionSmoothSpeed);
            _animator.SetFloat(_animIdHorizontal, _currentLocomotion.x);
            _animator.SetFloat(_animIdVertical, _currentLocomotion.y);

            bool isMoving = _targetSpeed > 0.01f || _targetLocomotion.sqrMagnitude > 0.0001f || _currentLocomotion.sqrMagnitude > 0.0001f;
            _animator.SetBool(_animIdMoving, isMoving);

        }
        
        private float _targetTurnAmount;
        public void SetTurn(float turnAmount)
        {
            _targetTurnAmount = turnAmount;
        }

        public void CopyStateFrom(AnimatorComponent source)
        {
            if (source == null) return;
            
            _currentTurnAmount = source._currentTurnAmount;
            _targetTurnAmount = source._targetTurnAmount;
            _targetSpeed = source._targetSpeed;
            _currentSpeed = source._currentSpeed;
            _targetLocomotion = source._targetLocomotion;
            _currentLocomotion = source._currentLocomotion;
            _targetAimPosition = source._targetAimPosition;
            _targetRiffleLayerWeight = source._targetRiffleLayerWeight;
            _aimTargetInitialized = source._aimTargetInitialized;
            
            if (source.Animator != null && this.Animator != null)
            {
                CopyAnimatorValues(source.Animator, this.Animator);
            }
        }

        private void CopyAnimatorValues(Animator source, Animator target)
        {
            int layerCount = Mathf.Min(source.layerCount, target.layerCount);
            for (int i = 0; i < layerCount; i++)
            {
                target.SetLayerWeight(i, source.GetLayerWeight(i));
                if (source.enabled && target.enabled)
                {
                    AnimatorStateInfo stateInfo = source.GetCurrentAnimatorStateInfo(i);
                    target.Play(stateInfo.fullPathHash, i, stateInfo.normalizedTime);
                }
            }

            foreach (AnimatorControllerParameter param in source.parameters)
            {
                if (source.IsParameterControlledByCurve(param.nameHash))
                    continue;

                switch (param.type)
                {
                    case AnimatorControllerParameterType.Float:
                        target.SetFloat(param.nameHash, source.GetFloat(param.nameHash));
                        break;
                    case AnimatorControllerParameterType.Int:
                        target.SetInteger(param.nameHash, source.GetInteger(param.nameHash));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        target.SetBool(param.nameHash, source.GetBool(param.nameHash));
                        break;
                }
            }
        }
        
        
        private void OnFootstep(AnimationEvent animationEvent)
        {
            // if (animationEvent.animatorClipInfo.weight > 0.5f)
            // {
            //     if (FootstepAudioClips.Length > 0)
            //     {
            //         var index = Random.Range(0, FootstepAudioClips.Length);
            //         AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            //     }
            // }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            // if (animationEvent.animatorClipInfo.weight > 0.5f)
            // {
            //     AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
            // }
        }
    }
}
