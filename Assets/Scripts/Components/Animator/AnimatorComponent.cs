using System;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Equipment;
using UnityEngine;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components
{
    /// <summary>
    /// Character animator component
    /// </summary>
    public class AnimatorComponent : BaseComponent<AnimatorModel>,
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
        private static readonly int SpawnTrigger = Animator.StringToHash("Spawn");
        private static readonly int LightAttackTrigger = Animator.StringToHash("LightAttack");
        private static readonly int LightAttackAltTrigger = Animator.StringToHash("LightAttackAlt");
        private static readonly int HeavyAttackTrigger = Animator.StringToHash("HeavyAttack");
        private static readonly int HeavyAttackAltTrigger = Animator.StringToHash("ChargedHeavyAttack");
        private static readonly int ChargedSpeedParameter = Animator.StringToHash("ChargedSpeed");
        private static readonly int RollAttackTrigger = Animator.StringToHash("RollAttack");
        private static readonly int BackStepAttackTrigger = Animator.StringToHash("BackStepAttack");
        private static readonly int RunAttackTrigger = Animator.StringToHash("RunAttack");
        private static readonly int SpecialAttackTrigger = Animator.StringToHash("SpecialAttack");
        private static readonly int ChangeModeTrigger = Animator.StringToHash("ChangeMode");
        private static readonly int HandModeTargetParameter = Animator.StringToHash("HandModeTarget");
        private static readonly int UseUpperBodyHandModeSwitchParameter =
            Animator.StringToHash("UseUpperBodyHandModeSwitch");

        private const string ONE_HANDED_LAYER = "OneHandedLayer";
        private const string TWO_HANDED_LAYER = "TwoHandedLayer";
        private const string UPPER_BODY_LAYER = "UpperBody";
        private const string FULL_BODY_LAYER = "FullBody";
        
        [SerializeField] private Animator animator;
        [SerializeField] private AnimatorStateMachineReceiver stateMachineReceiver;
     
        [Header("Smooth Transitions")]
        [SerializeField] private float layerTransitionSpeed = 10f;
        [SerializeField] private float turnSmoothSpeed = 10f;
        [SerializeField] private float locomotionSmoothSpeed = 10f;
        [SerializeField] private AnimatorRootMotionRelay rootMotionRelay;
        
        [field:SerializeField] public Transform RightHandAnchor { get; private set; }

        
        private IComponentMediator _mediator;
        private Vector2 _targetLocomotion;
        private Vector2 _currentLocomotion;
        private Vector3 _targetAimPosition;
        private float _currentTurnAmount;
        private float _targetSpeed;
        private float _currentSpeed;
        private float _targetTurnAmount;
        private float _targetRiffleLayerWeight;
        private bool _aimTargetInitialized;
        private bool _observingStateMachine;

        private Animator Animator => animator;
        
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
        
        public void SetLockOn(bool isLockedOn)
        {
            animator.SetBool(AnimIdLockOn, isLockedOn);
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
                AttackType.HeavyAttackAlt => HeavyAttackAltTrigger,
                AttackType.SpecialAttack => SpecialAttackTrigger,
                AttackType.BackStepAttack => BackStepAttackTrigger,
                _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
            };

            BeginRootMotionAction();
            animator.SetTrigger(triggerHash);
        }

        public void SetChargedAttackSpeed(float speed)
        {
            if (speed <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(speed), speed, "Attack speed must be greater than zero.");
            }

            if (HasParameter(animator, ChargedSpeedParameter, AnimatorControllerParameterType.Float))
            {
                animator.SetFloat(ChargedSpeedParameter, speed);
            }
        }

        public void UpdateState(AnimatorStateMachineDto state)
        {
            _mediator.NotifyAnimatorStateChanged(state);
        }

        public void SetJump()
        {
            animator.SetTrigger(AnimIdJump);
        }

        public void TriggerSpawn()
        {
            animator.SetTrigger(SpawnTrigger);
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

        public void TriggerHandModeSwitch(HandMode handMode, bool isMoving)
        {
            switch (handMode)
            {
                case HandMode.OneHanded:
                case HandMode.TwoHanded:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(handMode), handMode, null);
            }

            animator.SetLayerWeight(GetRequiredLayerIndex(FULL_BODY_LAYER), isMoving ? 0.0f : 1.0f);
            animator.SetLayerWeight(GetRequiredLayerIndex(UPPER_BODY_LAYER), isMoving ? 1.0f : 0.0f);
            animator.SetBool(UseUpperBodyHandModeSwitchParameter, isMoving);
            animator.SetInteger(HandModeTargetParameter, (int)handMode);
            animator.SetTrigger(ChangeModeTrigger);
        }

        public void SetHandMode(HandMode handMode)
        {
            int oneHandedLayerIndex = GetRequiredLayerIndex(ONE_HANDED_LAYER);
            int twoHandedLayerIndex = GetRequiredLayerIndex(TWO_HANDED_LAYER);

            switch (handMode)
            {
                case HandMode.OneHanded:
                    animator.SetLayerWeight(oneHandedLayerIndex, 1.0f);
                    animator.SetLayerWeight(twoHandedLayerIndex, 0.0f);
                    break;
                case HandMode.TwoHanded:
                    animator.SetLayerWeight(oneHandedLayerIndex, 0.0f);
                    animator.SetLayerWeight(twoHandedLayerIndex, 1.0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(handMode), handMode, null);
            }
        }

        public bool IsHandModeSwitchLayer(int layerIndex)
        {
            return layerIndex == GetRequiredLayerIndex(FULL_BODY_LAYER)
                || layerIndex == GetRequiredLayerIndex(UPPER_BODY_LAYER);
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
        

        public void SetLocomotion(float speed, Vector2 blendDirection)
        {
            _targetSpeed = speed;
            _targetLocomotion = blendDirection;
        }
        
        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            

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
        
        public void SetTurn(float turnAmount)
        {
            _targetTurnAmount = turnAmount;
        }

        public void CopyStateFrom(AnimatorComponent source)
        {
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

                if (!HasParameter(target, param.nameHash, param.type))
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

        private static bool HasParameter(
            Animator target,
            int parameterHash,
            AnimatorControllerParameterType parameterType)
        {
            foreach (AnimatorControllerParameter parameter in target.parameters)
            {
                if (parameter.nameHash == parameterHash && parameter.type == parameterType)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetRequiredLayerIndex(string layerName)
        {
            int layerIndex = animator.GetLayerIndex(layerName);
            if (layerIndex < 0)
            {
                throw new InvalidOperationException(
                    $"Animator '{animator.name}' requires the '{layerName}' layer.");
            }

            return layerIndex;
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
        }

        private void OnLand(AnimationEvent animationEvent)
        {
        }
    }
}
