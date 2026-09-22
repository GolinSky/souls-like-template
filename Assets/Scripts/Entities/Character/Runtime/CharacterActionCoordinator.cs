namespace SoulsLike.Entities.Character.Runtime
{
    /// <summary>Arbitrates input and coordinates action admission with outer execution adapters.</summary>
    public sealed class CharacterActionCoordinator
    {
        private readonly CharacterActionStateMachine _stateMachine;
        private bool _suppressLightUntilRelease;

        public CharacterActionCoordinator(CharacterActionStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public CharacterInput BuildInput(in CharacterInputFrame frame)
        {
            CharacterAction.State currentState = _stateMachine.CurrentState;
            bool hasMovement = frame.MoveInput.LengthSquared() > 0.0001f;
            bool isItemUseActive = currentState == CharacterAction.State.ItemUse;
            bool isRollActive = currentState == CharacterAction.State.Roll;
            bool sprinting = frame.SprintHeld && hasMovement && !isItemUseActive;
            bool strongAttackPressed = !isRollActive && TryResolveHeavyAttack(
                frame.StrongAttackPressed,
                frame.AttackHeld);

            CharacterAction? first = ResolveEquipmentAction(frame);
            CharacterAction? second = null;
            if (frame.TwoHandedPressed)
            {
                CharacterAction handMode = CharacterAction.Equipment(
                    CharacterAction.EquipmentKind.ToggleHandMode);
                if (first.HasValue) second = handMode;
                else first = handMode;
            }

            if (!first.HasValue)
            {
                first = ResolveCombatAction(
                    frame,
                    sprinting,
                    isRollActive,
                    strongAttackPressed);
            }

            return new CharacterInput(
                frame.MoveInput,
                frame.CameraYaw,
                sprinting,
                !isItemUseActive && frame.CrouchHeld,
                frame.GuardHeld,
                frame.StrongAttackHeld && !isRollActive,
                first,
                second);
        }

        public CharacterInput BuildMovementOnly(in CharacterInputFrame frame)
        {
            return new CharacterInput(
                frame.MoveInput,
                frame.CameraYaw,
                false,
                false,
                false,
                false);
        }

        public void Clear()
        {
            _suppressLightUntilRelease = false;
        }

        public void Process(
            in CharacterInput input,
            float now,
            ICharacterActionExecutor executor)
        {
            Submit(input.FirstAction, now, executor);
            Submit(input.SecondAction, now, executor);
            _stateMachine.PruneExpiredBuffer(now);
            TryExecuteBufferedAction(executor);
        }

        public void TryExecuteBufferedAction(ICharacterActionExecutor executor)
        {
            if (!_stateMachine.TryGetBufferedAction(out CharacterAction action)) return;
            CharacterActionExecution execution = executor.Execute(action);
            _stateMachine.ReportBufferedExecution(execution.Result, execution.StartedState);
        }

        private void Submit(
            CharacterAction? action,
            float now,
            ICharacterActionExecutor executor)
        {
            if (!action.HasValue || !_stateMachine.TryDispatch(action.Value, now)) return;
            CharacterActionExecution execution = executor.Execute(action.Value);
            _stateMachine.ReportExecution(
                action.Value,
                execution.Result,
                execution.StartedState,
                now);
        }

        private static CharacterAction? ResolveEquipmentAction(in CharacterInputFrame frame)
        {
            if (frame.SwitchWeaponPressed)
            {
                return CharacterAction.Equipment(CharacterAction.EquipmentKind.SwitchRightWeapon);
            }
            if (frame.SwitchShieldPressed)
            {
                return CharacterAction.Equipment(CharacterAction.EquipmentKind.SwitchLeftWeapon);
            }
            if (frame.SwitchFlaskPressed)
            {
                return CharacterAction.Equipment(CharacterAction.EquipmentKind.SwitchQuickItem);
            }
            return frame.UseItemPressed
                ? CharacterAction.Equipment(CharacterAction.EquipmentKind.UseQuickItem)
                : null;
        }

        private CharacterAction? ResolveCombatAction(
            in CharacterInputFrame frame,
            bool sprinting,
            bool isRollActive,
            bool strongAttackPressed)
        {
            if (strongAttackPressed)
            {
                return CharacterAction.Attack(CharacterAction.AttackIntent.Heavy, false, sprinting, frame.MoveInput, frame.CameraYaw);
            }
            if (!isRollActive && frame.SpecialAttackPressed)
            {
                return CharacterAction.Attack(CharacterAction.AttackIntent.Special, false, false, frame.MoveInput, frame.CameraYaw);
            }
            if (!ShouldSuppressLightAttack(frame.AttackPressed))
            {
                return CharacterAction.Attack(CharacterAction.AttackIntent.Light, false, sprinting, frame.MoveInput, frame.CameraYaw);
            }
            if (frame.GuardPressed)
            {
                return CharacterAction.Attack(CharacterAction.AttackIntent.Light, true, false, frame.MoveInput, frame.CameraYaw);
            }
            if (frame.RollRequested)
            {
                return CharacterAction.Roll(frame.MoveInput, frame.CameraYaw);
            }
            return frame.JumpPressed ? CharacterAction.Jump(sprinting) : null;
        }

        private bool TryResolveHeavyAttack(bool pressedThisFrame, bool lightIsPressed)
        {
            if (!lightIsPressed) _suppressLightUntilRelease = false;
            if (!pressedThisFrame) return false;
            _suppressLightUntilRelease = true;
            return true;
        }

        private bool ShouldSuppressLightAttack(bool lightPressedThisFrame) =>
            _suppressLightUntilRelease || !lightPressedThisFrame;
    }
}
