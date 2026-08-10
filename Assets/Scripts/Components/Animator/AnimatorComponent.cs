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
    public class AnimatorComponent : BaseComponent<AnimatorModel>, IInitializable,
        Animations.IObserver<AnimatorStateMachineDto>
    {
        private static readonly int AnimIdHorizontal = Animator.StringToHash("Horizontal");
        private static readonly int AnimIdVertical = Animator.StringToHash("Vertical");
        private static readonly int AnimIdGrounded = Animator.StringToHash("Grounded");
        private static readonly int AnimIdJump = Animator.StringToHash("Jump");
        private static readonly int AnimIdRoll = Animator.StringToHash("Roll");
        private static readonly int AnimIdBackStep = Animator.StringToHash("BackStep");
        private static readonly int AnimIdRollHorizontal = Animator.StringToHash("RollHorizontal");
        private static readonly int AnimIdRollVertical = Animator.StringToHash("RollVertical");
        private static readonly int AnimIdCrouch = Animator.StringToHash("Crouch");
        private static readonly int AnimIdTurn = Animator.StringToHash("Turn");
        private static readonly int AnimIdMoving = Animator.StringToHash("Moving");
        private static readonly int AnimIdSpeed = Animator.StringToHash("Speed");
        private static readonly int AnimIdLockOn = Animator.StringToHash("LockOn");
        private static readonly int LightAttackTrigger = Animator.StringToHash("LightAttack");
        private static readonly int LightAttackAltTrigger = Animator.StringToHash("LightAttackAlt");
        private static readonly int HeavyAttackTrigger = Animator.StringToHash("HeavyAttack");
        private static readonly int ChargedHeavyAttackTrigger = Animator.StringToHash("ChargedHeavyAttack");
        private static readonly int RollAttackTrigger = Animator.StringToHash("RollAttack");
        private static readonly int BackStepAttackTrigger = Animator.StringToHash("BackStepAttack");
        private static readonly int RunAttackTrigger = Animator.StringToHash("RunAttack");
        private static readonly int SpecialAttackTrigger = Animator.StringToHash("SpecialAttack");

        public void SetLockOn(bool isLockedOn)
        {
            animator.SetBool(AnimIdLockOn, isLockedOn);
        }
        
        [SerializeField] private Animator animator;
        [SerializeField] private AnimatorStateMachineReceiver stateMachineReceiver;
        [Header("Aim Target")]
        [SerializeField] private Transform aimTarget;
        [Header("Aim Settings")]
        [SerializeField] private float aimTrackingSpeed = 10f;
        [SerializeField] private float aimVerticalOffset;
        [SerializeField] private float turningLagAmount = 5f;
        [Header("Smooth Transitions")]
        [SerializeField] private float layerTransitionSpeed = 10f;
        [SerializeField] private float turnSmoothSpeed = 10f;
        [SerializeField] private float locomotionSmoothSpeed = 10f;
        [SerializeField] private AnimatorRootMotionRelay rootMotionRelay;
        
        [field:SerializeField] public Transform RightHandAnchor { get; private set; }

        private float _currentTurnAmount;
        private float _targetSpeed;
        private float _currentSpeed;
        private Vector2 _targetLocomotion;
        private Vector2 _currentLocomotion;
        private Vector3 _targetAimPosition;
        private float _targetRiffleLayerWeight;
        private bool _aimTargetInitialized;
        private IComponentMediator _mediator;
        private bool _observingStateMachine;
        public Animator Animator => animator;

        public void Initialize()
        {
        }

        public void SetMediator(IComponentMediator mediator)
        {
            _mediator = mediator;
            rootMotionRelay.Initialize(mediator);
            if (!_observingStateMachine)
            {
                stateMachineReceiver.AddObserver(this);
                _observingStateMachine = true;
            }

            animator.applyRootMotion = true;
        }

        public void SetGrounded(bool modelGrounded)
        {
            animator.SetBool(AnimIdGrounded, modelGrounded);
        }

        public void PlayAttack(AttackType attackType)
        {
            int triggerHash = attackType switch
            {
                AttackType.LightAttack => LightAttackTrigger,
                AttackType.LightAttackAlt => LightAttackAltTrigger,
                AttackType.RollingLightAttack => RollAttackTrigger,
                AttackType.SprintingAttack => RunAttackTrigger,
                AttackType.HeavyAttack => HeavyAttackTrigger,
                AttackType.ChargedHeavyAttack => ChargedHeavyAttackTrigger,
                AttackType.SpecialAttack => SpecialAttackTrigger,
                AttackType.BackStepAttack => BackStepAttackTrigger,
                _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
            };

            BeginRootMotionAction();
            animator.SetTrigger(triggerHash);
        }

        public void UpdateState(AnimatorStateMachineDto state)
        {
            _mediator.NotifyAnimatorStateChanged(state);
        }

        public void SetJump()
        {
            animator.SetTrigger(AnimIdJump);
        }
        
        public void TriggerRoll(Vector2 direction)
        {
            animator.SetFloat(AnimIdRollHorizontal, direction.x);
            animator.SetFloat(AnimIdRollVertical, direction.y);
            BeginRootMotionAction();
            animator.SetTrigger(AnimIdRoll);
        }

        public void TriggerBackStep()
        {
            BeginRootMotionAction();
            animator.SetTrigger(AnimIdBackStep);
        }

        private void BeginRootMotionAction()
        {
            rootMotionRelay.BeginRootMotionContract();
        }

        private void OnDestroy()
        {
            if (_observingStateMachine)
            {
                stateMachineReceiver.RemoveObserver(this);
                _observingStateMachine = false;
            }
        }

        public void SetCrouch(bool isCrouching)
        {
            animator.SetBool(AnimIdCrouch, isCrouching);
        }

        public void SetAimTarget(Vector3 targetPosition)
        {
            _targetAimPosition = targetPosition + Vector3.up * aimVerticalOffset;
            if (!_aimTargetInitialized && aimTarget != null)
            {
                aimTarget.position = _targetAimPosition;
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
            if (aimTarget != null && _aimTargetInitialized)
            {
                // Calculate turning lag based on rotation velocity from movement
                // We use transform.right to offset the target horizontally
                float turnLag = _targetTurnAmount * turningLagAmount; 
                Vector3 laggedTarget = _targetAimPosition - transform.right * turnLag;
                
                aimTarget.position = Vector3.Lerp(aimTarget.position, laggedTarget, dt * aimTrackingSpeed);
            }

            // 3. Smooth Animator Layer Weight
            int riffleLayerIndex = animator.GetLayerIndex("Riffle");
            if (riffleLayerIndex != -1)
            {
                float currentWeight = animator.GetLayerWeight(riffleLayerIndex);
                float nextWeight = Mathf.Lerp(currentWeight, _targetRiffleLayerWeight, dt * layerTransitionSpeed);
                animator.SetLayerWeight(riffleLayerIndex, nextWeight);
            }

            _currentTurnAmount = Mathf.Lerp(_currentTurnAmount, _targetTurnAmount, dt * turnSmoothSpeed);
            animator.SetFloat(AnimIdTurn, _currentTurnAmount);

            _currentSpeed = Mathf.Lerp(_currentSpeed, _targetSpeed, dt * locomotionSmoothSpeed);
            if (_currentSpeed < 0.01f) _currentSpeed = 0f;
            animator.SetFloat(AnimIdSpeed, _currentSpeed);

            _currentLocomotion = Vector2.Lerp(_currentLocomotion, _targetLocomotion, dt * locomotionSmoothSpeed);
            animator.SetFloat(AnimIdHorizontal, _currentLocomotion.x);
            animator.SetFloat(AnimIdVertical, _currentLocomotion.y);

            bool isMoving = _targetSpeed > 0.01f || _targetLocomotion.sqrMagnitude > 0.0001f || _currentLocomotion.sqrMagnitude > 0.0001f;
            animator.SetBool(AnimIdMoving, isMoving);
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
        }

        private void OnLand(AnimationEvent animationEvent)
        {
        }
    }
}
