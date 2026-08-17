using System;
using UnityEngine;

namespace SoulsLike.Entities.Character.Runtime
{
    public readonly struct CharacterControlFrame
    {
        public Vector2 MoveInput { get; }
        public float CameraYaw { get; }
        public bool SprintHeld { get; }
        public bool CrouchHeld { get; }
        public bool GuardHeld { get; }
        public bool StrongAttackHeld { get; }

        public CharacterControlFrame(
            Vector2 moveInput,
            float cameraYaw,
            bool sprintHeld,
            bool crouchHeld,
            bool guardHeld,
            bool strongAttackHeld)
        {
            MoveInput = moveInput;
            CameraYaw = cameraYaw;
            SprintHeld = sprintHeld;
            CrouchHeld = crouchHeld;
            GuardHeld = guardHeld;
            StrongAttackHeld = strongAttackHeld;
        }
    }

    public readonly struct CharacterInputBatch
    {
        public CharacterControlFrame ControlFrame { get; }
        public ICharacterCommand FirstCommand { get; }
        public ICharacterCommand SecondCommand { get; }
        public int CommandCount { get; }

        public CharacterInputBatch(
            CharacterControlFrame controlFrame,
            ICharacterCommand firstCommand = null,
            ICharacterCommand secondCommand = null,
            int commandCount = 0)
        {
            ControlFrame = controlFrame;
            FirstCommand = firstCommand;
            SecondCommand = secondCommand;
            CommandCount = Mathf.Clamp(commandCount, 0, 2);
        }
    }

    public enum CharacterCommandKind
    {
        Attack,
        Roll,
        Jump,
        Equipment
    }

    public enum AttackIntent
    {
        Light,
        Heavy,
        Special
    }

    public enum CharacterCommandBufferPolicy
    {
        Never,
        RetainUntilQueueWindow,
        RetainUntilExpiry
    }

    public enum CharacterCommandExecutionStatus
    {
        Executed,
        TemporarilyBlocked,
        Invalid
    }

    public enum CharacterCommandDisposition
    {
        Executed,
        Buffered,
        Rejected,
        Ignored
    }

    public enum CharacterActionStateId
    {
        Neutral,
        Attack,
        Roll,
        EquipmentSwap
    }

    public enum CharacterAnimationSignalKind
    {
        Entered,
        Progressed,
        QueueWindowOpened,
        Exited
    }

    [Flags]
    public enum MovementGateReason
    {
        None = 0,
        Manual = 1 << 0,
        Animation = 1 << 1,
        EquipmentSwap = 1 << 2,
        Spawn = 1 << 3,
        Stagger = 1 << 4
    }

    public readonly struct CharacterCommandExecutionResult
    {
        public CharacterCommandExecutionStatus Status { get; }
        public CharacterActionStateId StartedState { get; }

        public CharacterCommandExecutionResult(
            CharacterCommandExecutionStatus status,
            CharacterActionStateId startedState = CharacterActionStateId.Neutral)
        {
            Status = status;
            StartedState = startedState;
        }
    }

    public readonly struct AttackRequest
    {
        public AttackIntent Intent { get; }
        public bool IsHeavy => Intent == AttackIntent.Heavy;
        public bool IsLeftHand { get; }
        public bool IsSprinting { get; }

        public AttackRequest(AttackIntent intent, bool isLeftHand, bool isSprinting)
        {
            Intent = intent;
            IsLeftHand = isLeftHand;
            IsSprinting = isSprinting;
        }
    }

    public readonly struct RollRequest
    {
        public Vector2 MoveInput { get; }
        public float CameraYaw { get; }
        public bool CanInterrupt { get; }

        public RollRequest(Vector2 moveInput, float cameraYaw, bool canInterrupt)
        {
            MoveInput = moveInput;
            CameraYaw = cameraYaw;
            CanInterrupt = canInterrupt;
        }
    }

    public readonly struct JumpRequest
    {
        public bool IsSprinting { get; }

        public JumpRequest(bool isSprinting)
        {
            IsSprinting = isSprinting;
        }
    }

    public readonly struct EquipmentActionRequest
    {
        public int ActionId { get; }

        public EquipmentActionRequest(int actionId)
        {
            ActionId = actionId;
        }
    }

    public interface ICharacterCommand
    {
        CharacterCommandKind Kind { get; }
        CharacterCommandBufferPolicy BufferPolicy { get; }
        CharacterCommandExecutionResult TryExecute();
    }

    public interface IAttackCommandReceiver
    {
        CharacterCommandExecutionStatus TryStartAttack(in AttackRequest request);
        void SetStrongAttackHeld(bool held);
    }

    public interface IMovementCommandReceiver
    {
        CharacterCommandExecutionStatus TryStartRoll(in RollRequest request);
        CharacterCommandExecutionStatus TryStartJump(in JumpRequest request);
    }

    public interface IEquipmentCommandReceiver
    {
        bool IsEquipmentActionInProgress { get; }
        CharacterCommandExecutionStatus TryStartEquipmentAction(
            in EquipmentActionRequest request);
        CharacterCommandExecutionStatus TryAdvanceEquipmentAction();
    }

    public sealed class AttackCommand : ICharacterCommand
    {
        private readonly IAttackCommandReceiver _receiver;
        private readonly AttackRequest _request;

        public CharacterCommandKind Kind => CharacterCommandKind.Attack;
        public CharacterCommandBufferPolicy BufferPolicy { get; }

        public AttackCommand(
            IAttackCommandReceiver receiver,
            in AttackRequest request,
            CharacterCommandBufferPolicy policy =
                CharacterCommandBufferPolicy.RetainUntilQueueWindow)
        {
            _receiver = receiver;
            _request = request;
            BufferPolicy = policy;
        }

        public CharacterCommandExecutionResult TryExecute() =>
            new CharacterCommandExecutionResult(
                _receiver.TryStartAttack(in _request),
                CharacterActionStateId.Attack);
    }

    public sealed class RollCommand : ICharacterCommand
    {
        private readonly IMovementCommandReceiver _receiver;
        private readonly RollRequest _request;

        public CharacterCommandKind Kind => CharacterCommandKind.Roll;
        public CharacterCommandBufferPolicy BufferPolicy { get; }

        public RollCommand(
            IMovementCommandReceiver receiver,
            in RollRequest request,
            CharacterCommandBufferPolicy policy =
                CharacterCommandBufferPolicy.RetainUntilExpiry)
        {
            _receiver = receiver;
            _request = request;
            BufferPolicy = policy;
        }

        public CharacterCommandExecutionResult TryExecute() =>
            new CharacterCommandExecutionResult(
                _receiver.TryStartRoll(in _request),
                CharacterActionStateId.Roll);
    }

    public sealed class JumpCommand : ICharacterCommand
    {
        private readonly IMovementCommandReceiver _receiver;
        private readonly JumpRequest _request;

        public CharacterCommandKind Kind => CharacterCommandKind.Jump;
        public CharacterCommandBufferPolicy BufferPolicy { get; }

        public JumpCommand(
            IMovementCommandReceiver receiver,
            in JumpRequest request,
            CharacterCommandBufferPolicy policy =
                CharacterCommandBufferPolicy.RetainUntilExpiry)
        {
            _receiver = receiver;
            _request = request;
            BufferPolicy = policy;
        }

        public CharacterCommandExecutionResult TryExecute() =>
            new CharacterCommandExecutionResult(
                _receiver.TryStartJump(in _request),
                CharacterActionStateId.Neutral);
    }

    public sealed class EquipmentCommand : ICharacterCommand
    {
        private readonly IEquipmentCommandReceiver _receiver;
        private readonly EquipmentActionRequest _request;

        public CharacterCommandKind Kind => CharacterCommandKind.Equipment;
        public CharacterCommandBufferPolicy BufferPolicy =>
            CharacterCommandBufferPolicy.Never;

        public EquipmentCommand(
            IEquipmentCommandReceiver receiver,
            in EquipmentActionRequest request)
        {
            _receiver = receiver;
            _request = request;
        }

        public CharacterCommandExecutionResult TryExecute()
        {
            CharacterCommandExecutionStatus status =
                _receiver.TryStartEquipmentAction(in _request);
            CharacterActionStateId startedState =
                status == CharacterCommandExecutionStatus.Executed
                && _receiver.IsEquipmentActionInProgress
                    ? CharacterActionStateId.EquipmentSwap
                    : CharacterActionStateId.Neutral;
            return new CharacterCommandExecutionResult(status, startedState);
        }
    }

    public interface ICharacterClock
    {
        float Now { get; }
    }

    public sealed class CharacterCommandBuffer
    {
        private const float BUFFER_DURATION_SECONDS = 1f;

        private readonly ICharacterClock _clock;
        private ICharacterCommand _command;
        private float _expiresAt;

        public bool HasCommand => _command != null;

        public CharacterCommandBuffer(ICharacterClock clock)
        {
            _clock = clock;
        }

        public void Store(ICharacterCommand command)
        {
            _command = command;
            _expiresAt = _clock.Now + BUFFER_DURATION_SECONDS;
        }

        public bool TryPeek(out ICharacterCommand command)
        {
            command = _command;
            return command != null;
        }

        public bool TryTake(out ICharacterCommand command)
        {
            command = _command;
            _command = null;
            return command != null;
        }

        public bool IsExpired() => _command != null && _clock.Now >= _expiresAt;

        public void Clear()
        {
            _command = null;
        }
    }

    public readonly struct MovementPolicy
    {
        public bool MovementBlocked { get; }
        public bool RotationBlocked { get; }
        public bool GuardAllowed { get; }
        public bool UseRootMotion { get; }

        public MovementPolicy(
            bool movementBlocked,
            bool rotationBlocked,
            bool guardAllowed,
            bool useRootMotion)
        {
            MovementBlocked = movementBlocked;
            RotationBlocked = rotationBlocked;
            GuardAllowed = guardAllowed;
            UseRootMotion = useRootMotion;
        }
    }

    public sealed class MovementGate
    {
        private MovementGateReason _reasons;

        public MovementGateReason Reasons => _reasons;
        public bool IsBlocked => _reasons != MovementGateReason.None;

        public void Set(MovementGateReason reason, bool value)
        {
            if (value)
            {
                _reasons |= reason;
            }
            else
            {
                _reasons &= ~reason;
            }
        }

        public bool IsSet(MovementGateReason reason) => (_reasons & reason) != 0;

        public MovementPolicy Resolve(
            bool useRootMotion,
            bool canGuardDuringAnimationBlock)
        {
            bool guardAllowed = !IsBlocked
                || (canGuardDuringAnimationBlock
                    && _reasons == MovementGateReason.Animation);
            return new MovementPolicy(
                IsBlocked,
                IsBlocked,
                guardAllowed,
                useRootMotion);
        }
    }

    public readonly struct CharacterAnimationSignal
    {
        public CharacterAnimationSignalKind Kind { get; }
        public CharacterActionStateId ActionState { get; }

        public CharacterAnimationSignal(
            CharacterAnimationSignalKind kind,
            CharacterActionStateId actionState)
        {
            Kind = kind;
            ActionState = actionState;
        }
    }

    public sealed class CharacterRuntime
    {
        private readonly CharacterActionStateMachine _stateMachine;
        private bool _animationRootMotionEnabled;

        public MovementGate MovementGate { get; }
        public CharacterActionStateId ActionState => _stateMachine.CurrentState;
        public bool IsInputBlocked => _stateMachine.IsInputBlocked;
        public bool CanApplyRootMotion => _animationRootMotionEnabled;

        public CharacterRuntime(
            CharacterActionStateMachine stateMachine,
            MovementGate movementGate)
        {
            _stateMachine = stateMachine;
            MovementGate = movementGate;
        }

        public CharacterCommandDisposition Submit(ICharacterCommand command) =>
            _stateMachine.Submit(command);

        public void Tick(in CharacterInputBatch batch)
        {
            _stateMachine.Tick(in batch);
        }

        public bool HandleAnimation(in CharacterAnimationSignal signal) =>
            _stateMachine.HandleAnimation(in signal);

        public bool TryConsumeRollSprintInterrupt() =>
            _stateMachine.TryConsumeRollSprintInterrupt();

        public void SetInputBlocked(bool blocked)
        {
            _stateMachine.SetInputBlocked(blocked);
            MovementGate.Set(MovementGateReason.Spawn, blocked);
        }

        public void SetMovementBlocked(bool blocked) =>
            MovementGate.Set(MovementGateReason.Manual, blocked);

        public void SetAnimationMotionContract(
            bool movementBlocked,
            bool useRootMotion)
        {
            MovementGate.Set(MovementGateReason.Animation, movementBlocked);
            _animationRootMotionEnabled = useRootMotion;
        }

        public void SetEquipmentSwapActive(bool active) =>
            MovementGate.Set(MovementGateReason.EquipmentSwap, active);

        public MovementPolicy ResolveMovementPolicy(bool useRootMotion) =>
            MovementGate.Resolve(
                useRootMotion,
                _stateMachine.CanGuardDuringAnimationBlock);
    }
}
