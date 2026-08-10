using Prospector.Utility.Timer;
using SoulsLike.Entities.Character.Components.Animations;
using VContainer.Unity;

namespace SoulsLike.Entities.Character.Components.Attack
{
    public sealed class AttackComponent : BaseComponent, IInitializable
    {
        private const float CHARGED_HEAVY_HOLD_THRESHOLD = 0.5f;
        private const float CONTEXTUAL_ATTACK_WINDOW = 1.0f;

        private IComponentMediator _mediator;
        private StateMachineName _activeState = StateMachineName.None;
        private StateMachineName _contextualState = StateMachineName.None;
        private ITimer _strongAttackHoldTimer;
        private ITimer _contextualAttackTimer;
        private bool _strongInputActive;
        private bool _suppressLightUntilRelease;

        public bool IsActionActive => _activeState != StateMachineName.None;

        public void Initialize()
        {
            _strongAttackHoldTimer = TimerFactory.ConstructTimer(CHARGED_HEAVY_HOLD_THRESHOLD);
            _contextualAttackTimer = TimerFactory.ConstructTimer(CONTEXTUAL_ATTACK_WINDOW);
        }

        public void SetMediator(IComponentMediator mediator)
        {
            _mediator = mediator;
        }

        public bool TryCaptureAction(
            ProjectInputActions.CharacterActions actions,
            bool isSprinting,
            bool canBufferAttack,
            bool canBufferSpecialAttack,
            out BufferedCharacterAction action)
        {
            action = default;

            if (!actions.Attack.IsPressed())
            {
                _suppressLightUntilRelease = false;
            }

            if (HandleStrongAttack(actions, canBufferAttack, out action))
            {
                return true;
            }

            if (actions.SpecialAbility.WasPressedThisFrame())
            {
                if (canBufferSpecialAttack)
                {
                    action = BufferedCharacterAction.Attack(CharacterActionType.SpecialAttack, false);
                    return true;
                }

                return false;
            }

            if (!canBufferAttack || _suppressLightUntilRelease || !actions.Attack.WasPressedThisFrame())
            {
                return false;
            }

            action = BufferedCharacterAction.Attack(CharacterActionType.LightAttack, isSprinting);
            return true;
        }

        public void ExecuteAction(BufferedCharacterAction action)
        {
            AttackType attackType = action.Type switch
            {
                CharacterActionType.LightAttack => ResolveLightAttack(action.IsSprinting),
                CharacterActionType.HeavyAttack => AttackType.HeavyAttack,
                CharacterActionType.ChargedHeavyAttack => AttackType.ChargedHeavyAttack,
                CharacterActionType.SpecialAttack => AttackType.SpecialAttack,
                _ => throw new System.ArgumentOutOfRangeException(nameof(action.Type), action.Type, null)
            };

            ClearContextualAttack();
            _mediator.NotifyAttack(attackType);
        }

        public void HandleAnimatorState(AnimatorStateMachineDto state)
        {
            if (state.State == StateMachineState.Enter)
            {
                _activeState = state.StateMachineName;

                if (state.StateMachineName == StateMachineName.Roll
                    || state.StateMachineName == StateMachineName.BackStep)
                {
                    _strongInputActive = false;
                    _strongAttackHoldTimer.Reset();
                    ClearContextualAttack();
                }

                return;
            }

            if (state.State != StateMachineState.Exit)
            {
                return;
            }

            if (state.StateMachineName == StateMachineName.Roll
                || state.StateMachineName == StateMachineName.BackStep)
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

        private bool HandleStrongAttack(
            ProjectInputActions.CharacterActions actions,
            bool canBufferAttack,
            out BufferedCharacterAction action)
        {
            action = default;

            if (actions.StrongAttack.WasPressedThisFrame())
            {
                _strongInputActive = canBufferAttack;
                _suppressLightUntilRelease = true;
                if (_strongInputActive)
                {
                    _strongAttackHoldTimer
                        .ChangeDuration(CHARGED_HEAVY_HOLD_THRESHOLD)
                        .Start();
                }
            }

            if (_strongInputActive
                && actions.StrongAttack.IsPressed()
                && _strongAttackHoldTimer.IsComplete)
            {
                _strongInputActive = false;
                _strongAttackHoldTimer.Reset();
                action = BufferedCharacterAction.Attack(CharacterActionType.ChargedHeavyAttack, false);
                return true;
            }

            if (_strongInputActive && actions.StrongAttack.WasReleasedThisFrame())
            {
                _strongInputActive = false;
                _strongAttackHoldTimer.Reset();
                action = BufferedCharacterAction.Attack(CharacterActionType.HeavyAttack, false);
                return true;
            }

            return false;
        }

        private AttackType ResolveLightAttack(bool isSprinting)
        {
            if (_activeState == StateMachineName.LightAttack)
            {
                return AttackType.LightAttackAlt;
            }

            if (_activeState == StateMachineName.LightAttackAlt)
            {
                return AttackType.LightAttack;
            }

            if (_activeState == StateMachineName.Roll)
            {
                return AttackType.RollingLightAttack;
            }

            if (_activeState == StateMachineName.BackStep)
            {
                return AttackType.BackStepAttack;
            }

            if (_contextualState != StateMachineName.None && _contextualAttackTimer.IsComplete)
            {
                ClearContextualAttack();
            }

            if (_contextualState == StateMachineName.Roll)
            {
                ClearContextualAttack();
                return AttackType.RollingLightAttack;
            }

            if (_contextualState == StateMachineName.BackStep)
            {
                ClearContextualAttack();
                return AttackType.BackStepAttack;
            }

            if (isSprinting)
            {
                return AttackType.SprintingAttack;
            }

            return AttackType.LightAttack;
        }

        private void ClearContextualAttack()
        {
            _contextualState = StateMachineName.None;
            _contextualAttackTimer.Reset();
        }
    }
}
