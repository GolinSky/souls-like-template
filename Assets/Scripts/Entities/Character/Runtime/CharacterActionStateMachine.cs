namespace SoulsLike.Entities.Character.Runtime
{
    public sealed class CharacterActionStateMachine
    {
        private readonly CharacterCommandBuffer _buffer;
        private readonly CharacterActionState[] _states;
        private CharacterActionState _active;
        private bool _inputBlocked;

        public CharacterActionStateId CurrentState => _active.Id;
        public bool IsInputBlocked => _inputBlocked;
        public bool CanGuardDuringAnimationBlock => _active.CanGuardDuringAnimationBlock;

        public CharacterActionStateMachine(
            IEquipmentCommandReceiver equipment,
            CharacterCommandBuffer buffer)
        {
            _buffer = buffer;
            _states = new CharacterActionState[]
            {
                new NeutralState(this),
                new AttackState(this),
                new RollState(this),
                new EquipmentSwapState(this, equipment)
            };
            _active = _states[0];
        }

        public void SetInputBlocked(bool blocked) => _inputBlocked = blocked;
        internal void Complete(CharacterActionStateId id)
        {
            if (_active.Id == id) Enter(CharacterActionStateId.Neutral);
        }

        public CharacterCommandDisposition Submit(ICharacterCommand command)
        {
            if (_inputBlocked)
            {
                return CharacterCommandDisposition.Ignored;
            }

            CharacterCommandExecutionResult result = _active.TryExecute(command);
            if (result.Status == CharacterCommandExecutionStatus.Executed)
            {
                if (_active.Id != CharacterActionStateId.EquipmentSwap
                    || result.StartedState != CharacterActionStateId.EquipmentSwap)
                {
                    Enter(result.StartedState);
                }
                return CharacterCommandDisposition.Executed;
            }

            if (result.Status == CharacterCommandExecutionStatus.TemporarilyBlocked
                && command.BufferPolicy != CharacterCommandBufferPolicy.Never)
            {
                _buffer.Store(command);
                return CharacterCommandDisposition.Buffered;
            }

            return result.Status == CharacterCommandExecutionStatus.Invalid
                ? CharacterCommandDisposition.Rejected
                : CharacterCommandDisposition.Ignored;
        }

        public void Tick(in CharacterInputBatch batch)
        {
            _active.Tick();
            if (batch.CommandCount > 0 && batch.FirstCommand != null) Submit(batch.FirstCommand);
            if (batch.CommandCount > 1 && batch.SecondCommand != null) Submit(batch.SecondCommand);

            if (_active.CanPruneExpiredCommand && _buffer.IsExpired()) _buffer.Clear();
            TryExecuteBufferedCommand();
        }

        public bool HandleAnimation(in CharacterAnimationSignal signal)
        {
            if (signal.ActionState != _active.Id)
            {
                return false;
            }

            switch (signal.Kind)
            {
                case CharacterAnimationSignalKind.Entered:
                    _active.HandleEntered(signal);
                    break;
                case CharacterAnimationSignalKind.QueueWindowOpened:
                    _active.OpenQueueWindow();
                    TryExecuteBufferedCommand();
                    break;
                case CharacterAnimationSignalKind.Exited:
                    if (_active.HandleExited(signal)) Enter(CharacterActionStateId.Neutral);
                    break;
            }

            return true;
        }

        private void Enter(CharacterActionStateId id)
        {
            bool chained = _active.Id == id && id != CharacterActionStateId.Neutral;
            _active = _states[(int)id];
            _active.Activate(chained);
        }

        private void TryExecuteBufferedCommand()
        {
            if (!_active.CanConsumeBufferedCommand
                || !_buffer.TryPeek(out ICharacterCommand command)) return;

            CharacterCommandExecutionResult result = _active.TryExecute(command);
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
    }

    public abstract class CharacterActionState
    {
        protected CharacterActionStateMachine Machine { get; }
        protected CharacterActionState(CharacterActionStateMachine machine) => Machine = machine;
        public abstract CharacterActionStateId Id { get; }
        public virtual bool CanConsumeBufferedCommand => Id == CharacterActionStateId.Neutral;
        public virtual bool CanPruneExpiredCommand => Id == CharacterActionStateId.Neutral;
        public virtual bool CanGuardDuringAnimationBlock => false;
        public virtual void Activate(bool chained) { }
        public virtual void Tick() { }
        public virtual void OpenQueueWindow() { }
        public virtual void HandleEntered(in CharacterAnimationSignal signal) { }
        public virtual bool HandleExited(in CharacterAnimationSignal signal) => true;
        public abstract CharacterCommandExecutionResult TryExecute(ICharacterCommand command);
    }

    public sealed class NeutralState : CharacterActionState
    {
        public NeutralState(CharacterActionStateMachine machine) : base(machine) { }
        public override CharacterActionStateId Id => CharacterActionStateId.Neutral;
        public override CharacterCommandExecutionResult TryExecute(ICharacterCommand command) => command.TryExecute();
    }

    public sealed class AttackState : CharacterActionState
    {
        private bool _queueWindowOpen;
        private bool _ignoreNextExit;
        public AttackState(CharacterActionStateMachine machine) : base(machine) { }
        public override CharacterActionStateId Id => CharacterActionStateId.Attack;
        public override bool CanConsumeBufferedCommand => _queueWindowOpen;
        public override bool CanPruneExpiredCommand => false;
        public override bool CanGuardDuringAnimationBlock => _queueWindowOpen;
        public override void Activate(bool chained) { _queueWindowOpen = false; _ignoreNextExit = chained; }
        public override void HandleEntered(in CharacterAnimationSignal signal) => _queueWindowOpen = false;
        public override void OpenQueueWindow() => _queueWindowOpen = true;
        public override bool HandleExited(in CharacterAnimationSignal signal)
        {
            _queueWindowOpen = false;
            if (!_ignoreNextExit) return true;
            _ignoreNextExit = false;
            return false;
        }
        public override CharacterCommandExecutionResult TryExecute(ICharacterCommand command) =>
            _queueWindowOpen && command.Kind != CharacterCommandKind.Equipment
                ? command.TryExecute()
                : new CharacterCommandExecutionResult(CharacterCommandExecutionStatus.TemporarilyBlocked);
    }

    public sealed class RollState : CharacterActionState
    {
        private bool _queueWindowOpen;
        public RollState(CharacterActionStateMachine machine) : base(machine) { }
        public override CharacterActionStateId Id => CharacterActionStateId.Roll;
        public override bool CanConsumeBufferedCommand => _queueWindowOpen;
        public override bool CanPruneExpiredCommand => false;
        public override void Activate(bool chained) => _queueWindowOpen = false;
        public override void OpenQueueWindow() => _queueWindowOpen = true;
        public override CharacterCommandExecutionResult TryExecute(ICharacterCommand command) =>
            _queueWindowOpen && command.Kind != CharacterCommandKind.Equipment
                ? command.TryExecute()
                : new CharacterCommandExecutionResult(CharacterCommandExecutionStatus.TemporarilyBlocked);
    }

    public sealed class EquipmentSwapState : CharacterActionState
    {
        private readonly IEquipmentCommandReceiver _receiver;
        private bool _acceptCompanionCommand;
        public EquipmentSwapState(CharacterActionStateMachine machine, IEquipmentCommandReceiver receiver) : base(machine) => _receiver = receiver;
        public override CharacterActionStateId Id => CharacterActionStateId.EquipmentSwap;
        public override bool CanPruneExpiredCommand => false;
        public override bool HandleExited(in CharacterAnimationSignal signal) => false;
        public override void Activate(bool chained) => _acceptCompanionCommand = true;
        public override void Tick()
        {
            _acceptCompanionCommand = false;
            if (_receiver.TryAdvanceEquipmentAction() == CharacterCommandExecutionStatus.Executed)
            {
                Machine.Complete(Id);
            }
        }
        public override CharacterCommandExecutionResult TryExecute(ICharacterCommand command)
        {
            if (!_acceptCompanionCommand || command.Kind != CharacterCommandKind.Equipment)
            {
                return new CharacterCommandExecutionResult(
                    CharacterCommandExecutionStatus.TemporarilyBlocked);
            }

            _acceptCompanionCommand = false;
            return command.TryExecute();
        }
    }
}
