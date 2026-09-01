using System;
using Prospector.Utility.Timer;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Items;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components.Attack
{
    public readonly struct AttackExecutionContext
    {
        public StateMachineName ActiveState { get; }
        public StateMachineName ContextualState { get; }

        public AttackExecutionContext(
            StateMachineName activeState,
            StateMachineName contextualState)
        {
            ActiveState = activeState;
            ContextualState = contextualState;
        }
    }

    public readonly struct AttackResolution
    {
        public AttackType AttackType { get; }
        public bool IsLeftHandAttack { get; }
        public float ChargedSpeed { get; }

        public AttackResolution(
            AttackType attackType,
            bool isLeftHandAttack,
            float chargedSpeed)
        {
            AttackType = attackType;
            IsLeftHandAttack = isLeftHandAttack;
            ChargedSpeed = chargedSpeed;
        }
    }

    public sealed class AttackComponent : BaseComponent, IInitializable
    {
        private const float CHARGED_HEAVY_SPEED = 0.25f;
        private const float NORMAL_ATTACK_SPEED = 1.0f;
        private const float CONTEXTUAL_ATTACK_WINDOW = 1.0f;

        private StateMachineName _activeState = StateMachineName.None;
        private StateMachineName _contextualState = StateMachineName.None;
        private ITimer _contextualAttackTimer;
        private bool _strongInputActive;
        private ItemId? _rightWeaponId;
        private ItemId? _leftWeaponId;
        private WeaponRuntime _rightWeaponRuntime;
        private WeaponRuntime _leftWeaponRuntime;
        private ItemCatalog _itemCatalog;

        public ItemId? ActiveWeaponId { get; private set; }
        public ItemId? RightWeaponId => _rightWeaponId;
        public CombatProfile ActiveCombatProfile { get; private set; }
        public WeaponRuntime ActiveWeaponRuntime { get; private set; }
        public HandMode ActiveHandMode { get; private set; } = HandMode.OneHanded;
        public AttackExecutionContext CurrentExecutionContext =>
            new AttackExecutionContext(_activeState, _contextualState);

        public void Initialize()
        {
            _contextualAttackTimer = TimerFactory.ConstructTimer(
                CONTEXTUAL_ATTACK_WINDOW);
        }

        [Inject]
        public void InjectDependencies(ItemCatalog itemCatalog)
        {
            _itemCatalog = itemCatalog;
        }

        public void SetActiveWeapons(
            ItemId? rightItemId,
            WeaponRuntime rightWeaponRuntime,
            ItemId? leftItemId,
            WeaponRuntime leftWeaponRuntime,
            HandMode handMode)
        {
            _rightWeaponId = rightItemId;
            _rightWeaponRuntime = rightWeaponRuntime;
            _leftWeaponId = leftItemId;
            _leftWeaponRuntime = leftWeaponRuntime;
            ActiveHandMode = handMode;
            SetActionWeapon(false);
        }

        public void SetStrongAttackHeld(bool held)
        {
            _strongInputActive = held;
        }

        public AttackResolution ResolveAttack(
            in CharacterAction action,
            in AttackExecutionContext context)
        {
            SetActionWeapon(action.IsLeftHand);
            AttackType attackType = action.IsLeftHand
                ? ResolveLeftHandAttack(context.ActiveState)
                : action.Intent switch
                {
                    CharacterAction.AttackIntent.Light => ResolveLightAttack(action.IsSprinting, context),
                    CharacterAction.AttackIntent.Heavy => ResolveHeavyAttack(context.ActiveState),
                    CharacterAction.AttackIntent.Special => AttackType.SpecialAttack,
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(action.Intent), action.Intent, null)
                };
            float chargedSpeed = !action.IsLeftHand
                && action.Intent == CharacterAction.AttackIntent.Heavy
                && _strongInputActive
                ? CHARGED_HEAVY_SPEED
                : NORMAL_ATTACK_SPEED;

            ClearContextualAttack();
            return new AttackResolution(
                attackType,
                action.IsLeftHand,
                chargedSpeed);
        }

        public void HandleAnimatorState(AnimatorStateMachineDto state)
        {
            if (state.State == StateMachineState.Enter)
            {
                _activeState = state.StateMachineName;
                if (state.StateMachineName is StateMachineName.Roll
                    or StateMachineName.BackStep)
                {
                    _strongInputActive = false;
                    ClearContextualAttack();
                }

                return;
            }

            if (state.State != StateMachineState.Exit)
            {
                return;
            }

            if (state.StateMachineName is StateMachineName.Roll
                or StateMachineName.BackStep)
            {
                _contextualState = state.StateMachineName;
                _contextualAttackTimer
                    .ChangeDuration(CONTEXTUAL_ATTACK_WINDOW)
                    .Start();
            }

            if (_activeState == state.StateMachineName)
            {
                _activeState = StateMachineName.None;
            }
        }

        private void SetActionWeapon(bool isLeftHandAttack)
        {
            ActiveWeaponId = isLeftHandAttack
                ? _leftWeaponId
                : _rightWeaponId;
            ActiveWeaponRuntime = isLeftHandAttack
                ? _leftWeaponRuntime
                : _rightWeaponRuntime;
            ActiveCombatProfile = !ActiveWeaponId.HasValue
                ? null
                : _itemCatalog.GetWeapon(ActiveWeaponId.Value).CombatProfile;
        }

        private static AttackType ResolveHeavyAttack(StateMachineName activeState) =>
            activeState == StateMachineName.HeavyAttack
                ? AttackType.HeavyAttackAlt
                : AttackType.HeavyAttack;

        private static AttackType ResolveLeftHandAttack(StateMachineName activeState) =>
            activeState == StateMachineName.LightAttack
                ? AttackType.LightAttackAlt
                : AttackType.LightAttack;

        private AttackType ResolveLightAttack(
            bool isSprinting,
            in AttackExecutionContext context)
        {
            StateMachineName contextualState = context.ContextualState;
            if (_contextualState != StateMachineName.None
                && _contextualAttackTimer.IsComplete)
            {
                ClearContextualAttack();
                contextualState = StateMachineName.None;
            }

            if (context.ActiveState == StateMachineName.LightAttack)
            {
                return AttackType.LightAttackAlt;
            }

            if (context.ActiveState == StateMachineName.LightAttackAlt)
            {
                return AttackType.LightAttack;
            }

            if (context.ActiveState == StateMachineName.Roll
                || contextualState == StateMachineName.Roll)
            {
                return AttackType.RollingLightAttack;
            }

            if (context.ActiveState == StateMachineName.BackStep
                || contextualState == StateMachineName.BackStep)
            {
                return AttackType.BackStepAttack;
            }

            return isSprinting
                ? AttackType.SprintingAttack
                : AttackType.LightAttack;
        }

        private void ClearContextualAttack()
        {
            _contextualState = StateMachineName.None;
            _contextualAttackTimer.Reset();
        }
    }
}
