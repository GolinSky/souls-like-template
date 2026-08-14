using System;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Items;
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
        private static readonly int LeftLightAttackTrigger = Animator.StringToHash("LeftLightAttack");
        private static readonly int LeftLightAttackAltTrigger = Animator.StringToHash("LeftLightAttackAlt");
        private static readonly int LeftHeavyAttackTrigger = Animator.StringToHash("LeftHeavyAttack");
        private static readonly int LeftHeavyAttackAltTrigger = Animator.StringToHash("LeftChargedHeavyAttack");
        private static readonly int LeftRollAttackTrigger = Animator.StringToHash("LeftRollAttack");
        private static readonly int LeftBackStepAttackTrigger = Animator.StringToHash("LeftBackStepAttack");
        private static readonly int LeftRunAttackTrigger = Animator.StringToHash("LeftRunAttack");
        private static readonly int LeftSpecialAttackTrigger = Animator.StringToHash("LeftSpecialAttack");
        private static readonly int WeaponBlockParameter = Animator.StringToHash("WeaponBlock");
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
        [SerializeField, Min(0.0f)] private float _chargedAttackSpeedSmoothTime = 0.15f;
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
        private HandMode _targetHandMode;
        private float _targetRiffleLayerWeight;
        private float _targetChargedAttackSpeed = 1.0f;
        private bool _aimTargetInitialized;
        private bool _hasChargedAttackSpeedParameter;
        private bool _observingStateMachine;
        private RuntimeAnimatorController _defaultController;
        private bool _supportsLeftHandAttacks;

        private Animator Animator => animator;
        
        public void SetMediator(IComponentMediator mediator)
        {
            _mediator = mediator;
            _defaultController = animator.runtimeAnimatorController;
            rootMotionRelay.Initialize(mediator);
            _hasChargedAttackSpeedParameter = HasParameter(
                animator,
                ChargedSpeedParameter,
                AnimatorControllerParameterType.Float);

            if (_hasChargedAttackSpeedParameter)
            {
                _targetChargedAttackSpeed = animator.GetFloat(ChargedSpeedParameter);
            }

            if (!_observingStateMachine)
            {
                stateMachineReceiver.AddObserver(this);
                _observingStateMachine = true;
            }

            animator.applyRootMotion = true;
        }

        public void ApplyAnimationProfile(
            AnimationProfile animationProfile,
            bool hasRightWeapon,
            bool hasLeftWeapon)
        {
            RuntimeAnimatorController targetController = animationProfile.GetController(
                hasRightWeapon,
                hasLeftWeapon);
            _supportsLeftHandAttacks = hasLeftWeapon;

            if (animator.runtimeAnimatorController == targetController)
            {
                return;
            }

            animator.runtimeAnimatorController = targetController;
            _hasChargedAttackSpeedParameter = HasParameter(
                animator,
                ChargedSpeedParameter,
                AnimatorControllerParameterType.Float);
            SetHandMode(_targetHandMode);
        }

        public void ResetAnimationProfile()
        {
            _supportsLeftHandAttacks = false;
            if (_defaultController == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(AnimatorComponent)} requires a default animator controller.");
            }

            if (animator.runtimeAnimatorController == _defaultController)
            {
                return;
            }

            animator.runtimeAnimatorController = _defaultController;
            _hasChargedAttackSpeedParameter = HasParameter(
                animator,
                ChargedSpeedParameter,
                AnimatorControllerParameterType.Float);
            SetHandMode(_targetHandMode);
        }
        
        public void SetLockOn(bool isLockedOn)
        {
            animator.SetBool(AnimIdLockOn, isLockedOn);
        }
        
        public void SetGrounded(bool modelGrounded)
        {
            animator.SetBool(AnimIdGrounded, modelGrounded);
        }

        public void PlayAttack(AttackType attackType, bool isLeftHandAttack)
        {
            if (isLeftHandAttack && !_supportsLeftHandAttacks)
            {
                throw new InvalidOperationException(
                    "The active animator controller does not support left-hand attacks.");
            }

            int triggerHash = isLeftHandAttack
                ? GetLeftAttackTrigger(attackType)
                : GetRightAttackTrigger(attackType);

            BeginRootMotionAction();
            animator.SetTrigger(triggerHash);
        }

        private static int GetRightAttackTrigger(AttackType attackType)
        {
            return attackType switch
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
        }

        private static int GetLeftAttackTrigger(AttackType attackType)
        {
            return attackType switch
            {
                AttackType.LightAttack => LeftLightAttackTrigger,
                AttackType.LightAttackAlt => LeftLightAttackAltTrigger,
                AttackType.RollingLightAttack => LeftRollAttackTrigger,
                AttackType.SprintingAttack => LeftRunAttackTrigger,
                AttackType.HeavyAttack => LeftHeavyAttackTrigger,
                AttackType.HeavyAttackAlt => LeftHeavyAttackAltTrigger,
                AttackType.SpecialAttack => LeftSpecialAttackTrigger,
                AttackType.BackStepAttack => LeftBackStepAttackTrigger,
                _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
            };
        }

        public void SetChargedAttackSpeed(float speed)
        {
            if (speed <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(speed), speed, "Attack speed must be greater than zero.");
            }

            if (!_hasChargedAttackSpeedParameter)
            {
                return;
            }

            _targetChargedAttackSpeed = speed;
            if (speed < animator.GetFloat(ChargedSpeedParameter))
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

        public void SetWeaponBlock(bool isBlocking)
        {
            animator.SetBool(WeaponBlockParameter, isBlocking);
        }

        public void TransitionHandMode(HandMode handMode)
        {
            SetHandModeTarget(handMode);
        }

        //todo: we had smooth 1hand/2hand layer switching - find this method
        public void SetHandMode(HandMode handMode)
        {
            SetHandModeTarget(handMode);
            int oneHandedLayerIndex = GetRequiredLayerIndex(ONE_HANDED_LAYER);
            int twoHandedLayerIndex = GetRequiredLayerIndex(TWO_HANDED_LAYER);

            animator.SetLayerWeight(
                oneHandedLayerIndex,
                handMode == HandMode.OneHanded ? 1.0f : 0.0f);
            animator.SetLayerWeight(
                twoHandedLayerIndex,
                handMode == HandMode.TwoHanded ? 1.0f : 0.0f);
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

            if (_hasChargedAttackSpeedParameter)
            {
                animator.SetFloat(
                    ChargedSpeedParameter,
                    _targetChargedAttackSpeed,
                    _chargedAttackSpeedSmoothTime,
                    dt);
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
            UpdateHandModeLayerWeights(dt);
            UpdateHandModeSwitchLayerWeights(isMoving, dt);
        }

        private void SetHandModeTarget(HandMode handMode)
        {
            switch (handMode)
            {
                case HandMode.OneHanded:
                case HandMode.TwoHanded:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(handMode), handMode, null);
            }

            _targetHandMode = handMode;
        }

        private void UpdateHandModeLayerWeights(float deltaTime)
        {
            int oneHandedLayerIndex = GetRequiredLayerIndex(ONE_HANDED_LAYER);
            int twoHandedLayerIndex = GetRequiredLayerIndex(TWO_HANDED_LAYER);
            float maxDelta = deltaTime * layerTransitionSpeed;
            float targetTwoHandedLayerWeight = _targetHandMode == HandMode.TwoHanded ? 1.0f : 0.0f;
            float twoHandedLayerWeight = Mathf.MoveTowards(
                animator.GetLayerWeight(twoHandedLayerIndex),
                targetTwoHandedLayerWeight,
                maxDelta);

            // OneHandedLayer is below TwoHandedLayer. Keeping it fully weighted during the
            // crossfade prevents the base pose from leaking in and shifting the hips vertically.
            animator.SetLayerWeight(oneHandedLayerIndex, 1.0f);
            animator.SetLayerWeight(twoHandedLayerIndex, twoHandedLayerWeight);

            if (_targetHandMode == HandMode.TwoHanded && twoHandedLayerWeight >= 1.0f)
            {
                animator.SetLayerWeight(oneHandedLayerIndex, 0.0f);
            }
        }

        private void UpdateHandModeSwitchLayerWeights(bool isMoving, float deltaTime)
        {
            int fullBodyLayerIndex = GetRequiredLayerIndex(FULL_BODY_LAYER);
            int upperBodyLayerIndex = GetRequiredLayerIndex(UPPER_BODY_LAYER);
            float maxDelta = deltaTime * layerTransitionSpeed;

            animator.SetLayerWeight(
                fullBodyLayerIndex,
                Mathf.MoveTowards(
                    animator.GetLayerWeight(fullBodyLayerIndex),
                    isMoving ? 0.0f : 1.0f,
                    maxDelta));
            animator.SetLayerWeight(
                upperBodyLayerIndex,
                Mathf.MoveTowards(
                    animator.GetLayerWeight(upperBodyLayerIndex),
                    isMoving ? 1.0f : 0.0f,
                    maxDelta));
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
