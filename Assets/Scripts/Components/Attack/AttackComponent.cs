using SoulsLike.Entities.Character.Components.Animations;
using UnityEngine;

namespace SoulsLike.Entities.Character.Components.Attack
{
    public sealed class AttackComponent : BaseComponent
    {
        private const float CHARGED_HEAVY_HOLD_THRESHOLD = 0.5f;
        private const float CONTEXTUAL_ATTACK_WINDOW = 1.0f;

        private IComponentMediator _mediator;
        private StateMachineName _activeState = StateMachineName.None;
        private StateMachineName _contextualState = StateMachineName.None;
        private float _strongAttackPressedAt;
        private float _contextualAttackExpiresAt;
        private bool _strongInputActive;
        private bool _suppressLightUntilRelease;
        private bool _comboQueued;

        public void SetMediator(IComponentMediator mediator)
        {
            _mediator = mediator;
        }

        public void HandleInput(
            ProjectInputActions.CharacterActions actions,
            bool isSprinting,
            bool canStartAttack,
            bool canUseSpecialAttack)
        {
            if (!actions.Attack.IsPressed())
            {
                _suppressLightUntilRelease = false;
            }

            if (_activeState != StateMachineName.None)
            {
                HandleActiveAttackInput(actions);
                return;
            }

            if (!canStartAttack)
            {
                return;
            }

            if (HandleStrongAttack(actions))
            {
                return;
            }

            if (actions.SpecialAbility.WasPressedThisFrame())
            {
                if (canUseSpecialAttack)
                {
                    ClearContextualAttack();
                    _mediator.NotifyAttack(AttackType.SpecialAttack);
                }

                return;
            }

            if (_suppressLightUntilRelease || !actions.Attack.WasPressedThisFrame())
            {
                return;
            }

            AttackType attackType = ResolveLightAttack(isSprinting);
            _mediator.NotifyAttack(attackType);
        }

        public void HandleAnimatorState(AnimatorStateMachineDto state)
        {
            if (state.State == StateMachineState.Enter)
            {
                _activeState = state.StateMachineName;
                _comboQueued = false;

                if (state.StateMachineName == StateMachineName.Roll
                    || state.StateMachineName == StateMachineName.BackStep)
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

            if (state.StateMachineName == StateMachineName.Roll
                || state.StateMachineName == StateMachineName.BackStep)
            {
                _contextualState = state.StateMachineName;
                _contextualAttackExpiresAt = Time.time + CONTEXTUAL_ATTACK_WINDOW;
            }

            if (_activeState == state.StateMachineName)
            {
                _activeState = StateMachineName.None;
                _comboQueued = false;
            }
        }

        private void HandleActiveAttackInput(ProjectInputActions.CharacterActions actions)
        {
            if (_comboQueued
                || actions.StrongAttack.IsPressed()
                || !actions.Attack.WasPressedThisFrame())
            {
                return;
            }

            if (_activeState == StateMachineName.LightAttack)
            {
                _comboQueued = true;
                _mediator.NotifyAttack(AttackType.LightAttackAlt);
            }
            else if (_activeState == StateMachineName.LightAttackAlt)
            {
                _comboQueued = true;
                _mediator.NotifyAttack(AttackType.LightAttack);
            }
        }

        private bool HandleStrongAttack(ProjectInputActions.CharacterActions actions)
        {
            if (actions.StrongAttack.WasPressedThisFrame())
            {
                _strongAttackPressedAt = Time.time;
                _strongInputActive = true;
                _suppressLightUntilRelease = true;
            }

            if (_strongInputActive
                && actions.StrongAttack.IsPressed()
                && Time.time - _strongAttackPressedAt >= CHARGED_HEAVY_HOLD_THRESHOLD)
            {
                _strongInputActive = false;
                ClearContextualAttack();
                _mediator.NotifyAttack(AttackType.ChargedHeavyAttack);
                return true;
            }

            if (_strongInputActive && actions.StrongAttack.WasReleasedThisFrame())
            {
                _strongInputActive = false;
                ClearContextualAttack();
                _mediator.NotifyAttack(AttackType.HeavyAttack);
                return true;
            }

            return _strongInputActive || actions.StrongAttack.IsPressed();
        }

        private AttackType ResolveLightAttack(bool isSprinting)
        {
            if (Time.time > _contextualAttackExpiresAt)
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
            _contextualAttackExpiresAt = 0.0f;
        }
    }
}
