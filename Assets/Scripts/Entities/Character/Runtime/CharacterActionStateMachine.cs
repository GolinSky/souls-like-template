using System;

namespace SoulsLike.Entities.Character.Runtime
{
    public sealed class CharacterActionStateMachine
    {
        private readonly CharacterCommandBuffer _buffer;
        private CharacterActionStateId _currentState;
        private bool _inputBlocked;
        private bool _queueWindowOpen;
        private bool _ignoreNextActionExit;
        private bool _sprintHeldDuringRoll;
        private bool _acceptEquipmentCompanion;
        private bool _rollSprintInterruptRequested;

        public CharacterActionStateId CurrentState => _currentState;
        public bool IsInputBlocked => _inputBlocked;
        public bool CanGuardDuringAnimationBlock =>
            _currentState == CharacterActionStateId.Attack && _queueWindowOpen;

        public CharacterActionStateMachine(CharacterCommandBuffer buffer)
        {
            _buffer = buffer;
            _currentState = CharacterActionStateId.Neutral;
        }

        public void SetInputBlocked(bool blocked) => _inputBlocked = blocked;

        public bool TryConsumeRollSprintInterrupt()
        {
            if (!_rollSprintInterruptRequested) return false;

            _rollSprintInterruptRequested = false;
            return true;
        }

        public CharacterCommandDisposition Submit(
            CharacterCommand command,
            ICharacterActionExecutor executor)
        {
            if (_inputBlocked)
            {
                return CharacterCommandDisposition.Ignored;
            }

            CharacterCommandExecutionResult result = CanExecute(command)
                ? Execute(command, executor)
                : new CharacterCommandExecutionResult(
                    CharacterCommandExecutionStatus.TemporarilyBlocked);
            if (result.Status == CharacterCommandExecutionStatus.Executed)
            {
                if (_currentState != CharacterActionStateId.EquipmentSwap
                    || result.StartedState != CharacterActionStateId.EquipmentSwap)
                {
                    Enter(result.StartedState);
                }

                return CharacterCommandDisposition.Executed;
            }

            if (result.Status == CharacterCommandExecutionStatus.TemporarilyBlocked
                && command.CanBuffer)
            {
                _buffer.Store(command);
                return CharacterCommandDisposition.Buffered;
            }

            return result.Status == CharacterCommandExecutionStatus.Invalid
                ? CharacterCommandDisposition.Rejected
                : CharacterCommandDisposition.Ignored;
        }

        public void Tick(
            in CharacterInputBatch batch,
            ICharacterActionExecutor executor)
        {
            HandleTick(batch.ControlFrame, executor);
            if (batch.FirstCommand.HasValue)
            {
                Submit(batch.FirstCommand.Value, executor);
            }
            if (batch.SecondCommand.HasValue)
            {
                Submit(batch.SecondCommand.Value, executor);
            }

            if (_currentState == CharacterActionStateId.Neutral
                && _buffer.IsExpired())
            {
                _buffer.Clear();
            }

            TryExecuteBufferedCommand(executor);
        }

        public bool HandleAnimation(
            in CharacterAnimationSignal signal,
            ICharacterActionExecutor executor)
        {
            if (signal.ActionState != _currentState)
            {
                return false;
            }

            switch (signal.Kind)
            {
                case CharacterAnimationSignalKind.Entered:
                    if (_currentState == CharacterActionStateId.Attack)
                    {
                        _queueWindowOpen = false;
                    }
                    break;
                case CharacterAnimationSignalKind.QueueWindowOpened:
                    HandleQueueWindow();
                    TryExecuteBufferedCommand(executor);
                    break;
                case CharacterAnimationSignalKind.Exited:
                    if (HandleExit()) Enter(CharacterActionStateId.Neutral);
                    break;
            }

            return true;
        }

        private bool CanExecute(CharacterCommand command)
        {
            switch (_currentState)
            {
                case CharacterActionStateId.Neutral:
                    return true;
                case CharacterActionStateId.Attack:
                case CharacterActionStateId.Roll:
                    return _queueWindowOpen
                        && command.Kind != CharacterCommandKind.Equipment;
                case CharacterActionStateId.EquipmentSwap:
                    return _acceptEquipmentCompanion
                        && command.Kind == CharacterCommandKind.Equipment;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CharacterCommandExecutionResult Execute(
            CharacterCommand command,
            ICharacterActionExecutor executor)
        {
            if (_currentState == CharacterActionStateId.EquipmentSwap)
            {
                _acceptEquipmentCompanion = false;
            }

            return command.TryExecute(executor);
        }

        private void HandleTick(
            in CharacterControlFrame frame,
            ICharacterActionExecutor executor)
        {
            switch (_currentState)
            {
                case CharacterActionStateId.Roll:
                    _sprintHeldDuringRoll = frame.SprintHeld;
                    if (_queueWindowOpen && _sprintHeldDuringRoll)
                    {
                        InterruptRollForSprint();
                    }
                    break;
                case CharacterActionStateId.EquipmentSwap:
                    _acceptEquipmentCompanion = false;
                    if (executor.TryAdvanceEquipmentAction()
                        == CharacterCommandExecutionStatus.Executed)
                    {
                        Enter(CharacterActionStateId.Neutral);
                    }
                    break;
            }
        }

        private void HandleQueueWindow()
        {
            if (_currentState != CharacterActionStateId.Attack
                && _currentState != CharacterActionStateId.Roll)
            {
                return;
            }

            _queueWindowOpen = true;
            if (_currentState == CharacterActionStateId.Roll
                && _sprintHeldDuringRoll)
            {
                InterruptRollForSprint();
            }
        }

        private bool HandleExit()
        {
            if (_currentState == CharacterActionStateId.EquipmentSwap)
            {
                return false;
            }

            _queueWindowOpen = false;
            if (!_ignoreNextActionExit)
            {
                return true;
            }

            _ignoreNextActionExit = false;
            return false;
        }

        private void Enter(CharacterActionStateId state)
        {
            bool chained = _currentState == state
                && state != CharacterActionStateId.Neutral;
            _currentState = state;

            switch (state)
            {
                case CharacterActionStateId.Attack:
                    _queueWindowOpen = false;
                    _ignoreNextActionExit = chained;
                    break;
                case CharacterActionStateId.Roll:
                    _queueWindowOpen = false;
                    _sprintHeldDuringRoll = false;
                    _ignoreNextActionExit = chained;
                    break;
                case CharacterActionStateId.EquipmentSwap:
                    _acceptEquipmentCompanion = true;
                    break;
            }
        }

        private void TryExecuteBufferedCommand(ICharacterActionExecutor executor)
        {
            bool canConsume = _currentState == CharacterActionStateId.Neutral
                || ((_currentState == CharacterActionStateId.Attack
                    || _currentState == CharacterActionStateId.Roll)
                    && _queueWindowOpen);
            if (!canConsume || !_buffer.TryPeek(out CharacterCommand command))
            {
                return;
            }

            CharacterCommandExecutionResult result = Execute(command, executor);
            if (result.Status == CharacterCommandExecutionStatus.Executed)
            {
                _buffer.Clear();
                Enter(result.StartedState);
            }
            else if (result.Status == CharacterCommandExecutionStatus.Invalid)
            {
                _buffer.Clear();
            }
        }

        private void InterruptRollForSprint()
        {
            _rollSprintInterruptRequested = true;
            if (_currentState == CharacterActionStateId.Roll)
            {
                Enter(CharacterActionStateId.Neutral);
            }
        }
    }
}
