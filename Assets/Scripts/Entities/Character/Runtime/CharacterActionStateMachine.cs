using System;

namespace SoulsLike.Entities.Character.Runtime
{
    public sealed class CharacterActionStateMachine
    {
        private const float BUFFER_DURATION_SECONDS = 1f;
        private CharacterAction? _bufferedAction;
        private float _bufferExpiry;
        private CharacterAction.State _currentState = CharacterAction.State.Neutral;
        private bool _inputBlocked;
        private bool _queueWindowOpen;
        private bool _ignoreNextActionExit;
        private bool _sprintHeldDuringRoll;
        private bool _acceptEquipmentCompanion;
        private bool _rollSprintInterruptRequested;

        public CharacterAction.State CurrentState => _currentState;
        public bool IsInputBlocked => _inputBlocked;
        public bool HasBufferedAction => _bufferedAction.HasValue;
        public bool CanGuardDuringAnimationBlock => _currentState == CharacterAction.State.Attack && _queueWindowOpen;
        public void SetInputBlocked(bool blocked) => _inputBlocked = blocked;

        public bool TryConsumeRollSprintInterrupt()
        {
            if (!_rollSprintInterruptRequested) return false;
            _rollSprintInterruptRequested = false;
            return true;
        }

        public bool TryDispatch(in CharacterAction action, float now)
        {
            if (_inputBlocked) return false;
            if (CanExecute(action))
            {
                if (_currentState == CharacterAction.State.EquipmentSwap)
                {
                    _acceptEquipmentCompanion = false;
                }

                return true;
            }
            if (action.CanBuffer)
            {
                Buffer(action, now);
            }
            return false;
        }

        public void ReportExecution(
            in CharacterAction action,
            CharacterAction.Result result,
            CharacterAction.State startedState,
            float now)
        {
            if (result == CharacterAction.Result.Executed)
            {
                if (_currentState != CharacterAction.State.EquipmentSwap || startedState != CharacterAction.State.EquipmentSwap)
                {
                    Enter(startedState);
                }

                return;
            }

            if (result == CharacterAction.Result.TemporarilyBlocked && action.CanBuffer)
            {
                Buffer(action, now);
            }
        }

        public void Tick(bool sprintHeld, bool equipmentActionInProgress)
        {
            if (_currentState == CharacterAction.State.Roll)
            {
                _sprintHeldDuringRoll = sprintHeld;
                if (_queueWindowOpen && _sprintHeldDuringRoll) InterruptRollForSprint();
            }
            else if (_currentState == CharacterAction.State.EquipmentSwap)
            {
                _acceptEquipmentCompanion = false;
                if (!equipmentActionInProgress) Enter(CharacterAction.State.Neutral);
            }
        }

        public void PruneExpiredBuffer(float now)
        {
            if (_currentState == CharacterAction.State.Neutral
                && _bufferedAction.HasValue
                && now >= _bufferExpiry)
            {
                _bufferedAction = null;
            }
        }

        public bool HandleEntered(CharacterAction.State state)
        {
            if (state != _currentState) return false;
            if (_currentState == CharacterAction.State.Attack) _queueWindowOpen = false;
            return true;
        }

        public bool HandleQueueCheck(CharacterAction.State state)
        {
            if (state != _currentState) return false;
            HandleQueueWindow();
            return true;
        }

        public bool HandleExited(CharacterAction.State state)
        {
            if (state != _currentState) return false;
            if (HandleExit()) Enter(CharacterAction.State.Neutral);
            return true;
        }

        public bool TryGetBufferedAction(out CharacterAction action)
        {
            bool canConsume = _currentState == CharacterAction.State.Neutral || ((_currentState == CharacterAction.State.Attack || _currentState == CharacterAction.State.Roll) && _queueWindowOpen);
            if (!canConsume || !_bufferedAction.HasValue)
            {
                action = default;
                return false;
            }
            action = _bufferedAction.Value;
            return true;
        }

        public void ReportBufferedExecution(CharacterAction.Result result, CharacterAction.State startedState)
        {
            if (result == CharacterAction.Result.Executed)
            {
                _bufferedAction = null;
                Enter(startedState);
            }
            else if (result == CharacterAction.Result.Invalid) _bufferedAction = null;
        }

        public void EnterCritical()
        {
            _bufferedAction = null;
            Enter(CharacterAction.State.Critical);
        }

        public void CompleteCritical()
        {
            if (_currentState == CharacterAction.State.Critical)
            {
                Enter(CharacterAction.State.Neutral);
            }
        }

        private void Buffer(in CharacterAction action, float now)
        {
            _bufferedAction = action;
            _bufferExpiry = now + BUFFER_DURATION_SECONDS;
        }

        private bool CanExecute(in CharacterAction action) => _currentState switch
        {
            CharacterAction.State.Neutral => true,
            CharacterAction.State.Attack or CharacterAction.State.Roll => _queueWindowOpen && action.ActionKind != CharacterAction.Kind.Equipment,
            CharacterAction.State.EquipmentSwap => _acceptEquipmentCompanion && action.ActionKind == CharacterAction.Kind.Equipment,
            CharacterAction.State.Critical => false,
            _ => throw new ArgumentOutOfRangeException()
        };

        private void HandleQueueWindow()
        {
            if (_currentState != CharacterAction.State.Attack && _currentState != CharacterAction.State.Roll) return;
            _queueWindowOpen = true;
            if (_currentState == CharacterAction.State.Roll && _sprintHeldDuringRoll) InterruptRollForSprint();
        }

        private bool HandleExit()
        {
            if (_currentState == CharacterAction.State.EquipmentSwap) return false;
            _queueWindowOpen = false;
            if (!_ignoreNextActionExit) return true;
            _ignoreNextActionExit = false;
            return false;
        }

        private void Enter(CharacterAction.State state)
        {
            bool chained = _currentState == state && state != CharacterAction.State.Neutral;
            _currentState = state;
            switch (state)
            {
                case CharacterAction.State.Attack:
                    _queueWindowOpen = false;
                    _ignoreNextActionExit = chained;
                    break;
                case CharacterAction.State.Roll:
                    _queueWindowOpen = false;
                    _sprintHeldDuringRoll = false;
                    _ignoreNextActionExit = chained;
                    break;
                case CharacterAction.State.EquipmentSwap:
                    _acceptEquipmentCompanion = true;
                    break;
                case CharacterAction.State.Critical:
                case CharacterAction.State.Neutral:
                    _queueWindowOpen = false;
                    _ignoreNextActionExit = false;
                    break;
            }
        }

        private void InterruptRollForSprint()
        {
            _rollSprintInterruptRequested = true;
            if (_currentState == CharacterAction.State.Roll) Enter(CharacterAction.State.Neutral);
        }
    }
}
