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
        public CharacterCommand? FirstCommand { get; }
        public CharacterCommand? SecondCommand { get; }

        public CharacterInputBatch(
            CharacterControlFrame controlFrame,
            CharacterCommand? firstCommand = null,
            CharacterCommand? secondCommand = null)
        {
            ControlFrame = controlFrame;
            FirstCommand = firstCommand;
            SecondCommand = secondCommand;
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
        Spawn = 1 << 3,
        Stagger = 1 << 4,
        Parry = 1 << 5
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
        public Vector2 MoveInput { get; }
        public float CameraYaw { get; }

        public AttackRequest(AttackIntent intent, bool isLeftHand, bool isSprinting)
            : this(intent, isLeftHand, isSprinting, Vector2.zero, 0.0f)
        {
        }

        public AttackRequest(
            AttackIntent intent,
            bool isLeftHand,
            bool isSprinting,
            Vector2 moveInput,
            float cameraYaw)
        {
            Intent = intent;
            IsLeftHand = isLeftHand;
            IsSprinting = isSprinting;
            MoveInput = moveInput;
            CameraYaw = cameraYaw;
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
        public EquipmentActionKind Kind { get; }

        public EquipmentActionRequest(EquipmentActionKind kind)
        {
            Kind = kind;
        }
    }

    public enum EquipmentActionKind
    {
        SwitchRightWeapon,
        SwitchLeftWeapon,
        SwitchQuickItem,
        UseQuickItem,
        ToggleHandMode
    }

    public interface ICharacterActionExecutor
    {
        bool IsEquipmentActionInProgress { get; }
        CharacterCommandExecutionStatus TryStartAttack(in AttackRequest request);
        CharacterCommandExecutionStatus TryStartRoll(in RollRequest request);
        CharacterCommandExecutionStatus TryStartJump(in JumpRequest request);
        CharacterCommandExecutionStatus TryStartEquipmentAction(
            in EquipmentActionRequest request);
        CharacterCommandExecutionStatus TryAdvanceEquipmentAction();
    }

    public readonly struct CharacterCommand
    {
        private readonly AttackRequest _attackRequest;
        private readonly RollRequest _rollRequest;
        private readonly JumpRequest _jumpRequest;
        private readonly EquipmentActionRequest _equipmentRequest;

        public CharacterCommandKind Kind { get; }
        public bool CanBuffer => Kind != CharacterCommandKind.Equipment;

        private CharacterCommand(
            CharacterCommandKind kind,
            AttackRequest attackRequest = default,
            RollRequest rollRequest = default,
            JumpRequest jumpRequest = default,
            EquipmentActionRequest equipmentRequest = default)
        {
            Kind = kind;
            _attackRequest = attackRequest;
            _rollRequest = rollRequest;
            _jumpRequest = jumpRequest;
            _equipmentRequest = equipmentRequest;
        }

        public static CharacterCommand Attack(
            AttackIntent intent,
            bool isLeftHand,
            bool isSprinting)
        {
            return Attack(
                intent,
                isLeftHand,
                isSprinting,
                Vector2.zero,
                0.0f);
        }

        public static CharacterCommand Attack(
            AttackIntent intent,
            bool isLeftHand,
            bool isSprinting,
            Vector2 moveInput,
            float cameraYaw)
        {
            AttackRequest request = new AttackRequest(
                intent,
                isLeftHand,
                isSprinting,
                moveInput,
                cameraYaw);
            return new CharacterCommand(
                CharacterCommandKind.Attack,
                attackRequest: request);
        }

        public static CharacterCommand Roll(
            Vector2 moveInput,
            float cameraYaw,
            bool canInterrupt)
        {
            RollRequest request = new RollRequest(
                moveInput,
                cameraYaw,
                canInterrupt);
            return new CharacterCommand(
                CharacterCommandKind.Roll,
                rollRequest: request);
        }

        public static CharacterCommand Jump(bool isSprinting)
        {
            JumpRequest request = new JumpRequest(isSprinting);
            return new CharacterCommand(
                CharacterCommandKind.Jump,
                jumpRequest: request);
        }

        public static CharacterCommand Equipment(EquipmentActionKind kind)
        {
            EquipmentActionRequest request = new EquipmentActionRequest(kind);
            return new CharacterCommand(
                CharacterCommandKind.Equipment,
                equipmentRequest: request);
        }

        public CharacterCommandExecutionResult TryExecute(
            ICharacterActionExecutor executor)
        {
            switch (Kind)
            {
                case CharacterCommandKind.Attack:
                    return new CharacterCommandExecutionResult(
                        executor.TryStartAttack(in _attackRequest),
                        CharacterActionStateId.Attack);
                case CharacterCommandKind.Roll:
                    return new CharacterCommandExecutionResult(
                        executor.TryStartRoll(in _rollRequest),
                        CharacterActionStateId.Roll);
                case CharacterCommandKind.Jump:
                    return new CharacterCommandExecutionResult(
                        executor.TryStartJump(in _jumpRequest),
                        CharacterActionStateId.Neutral);
                case CharacterCommandKind.Equipment:
                    CharacterCommandExecutionStatus status =
                        executor.TryStartEquipmentAction(in _equipmentRequest);
                    CharacterActionStateId startedState =
                        status == CharacterCommandExecutionStatus.Executed
                        && executor.IsEquipmentActionInProgress
                            ? CharacterActionStateId.EquipmentSwap
                            : CharacterActionStateId.Neutral;
                    return new CharacterCommandExecutionResult(status, startedState);
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
        private CharacterCommand? _command;
        private float _expiresAt;

        public bool HasCommand => _command.HasValue;

        public CharacterCommandBuffer(ICharacterClock clock)
        {
            _clock = clock;
        }

        public void Store(CharacterCommand command)
        {
            _command = command;
            _expiresAt = _clock.Now + BUFFER_DURATION_SECONDS;
        }

        public bool TryPeek(out CharacterCommand command)
        {
            command = _command.GetValueOrDefault();
            return _command.HasValue;
        }

        public bool IsExpired() => _command.HasValue && _clock.Now >= _expiresAt;

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

        public CharacterCommandDisposition Submit(
            CharacterCommand command,
            ICharacterActionExecutor executor) =>
            _stateMachine.Submit(command, executor);

        public void Tick(
            in CharacterInputBatch batch,
            ICharacterActionExecutor executor)
        {
            _stateMachine.Tick(in batch, executor);
        }

        public bool HandleAnimation(
            in CharacterAnimationSignal signal,
            ICharacterActionExecutor executor) =>
            _stateMachine.HandleAnimation(in signal, executor);

        public bool TryConsumeRollSprintInterrupt() =>
            _stateMachine.TryConsumeRollSprintInterrupt();

        public void SetInputBlocked(bool blocked)
        {
            _stateMachine.SetInputBlocked(blocked);
            MovementGate.Set(MovementGateReason.Spawn, blocked);
        }

        public void SetParryLocked(bool locked)
        {
            _stateMachine.SetInputBlocked(locked);
            MovementGate.Set(MovementGateReason.Parry, locked);
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

        public MovementPolicy ResolveMovementPolicy(bool useRootMotion) =>
            MovementGate.Resolve(
                useRootMotion,
                _stateMachine.CanGuardDuringAnimationBlock);
    }
}
