using NUnit.Framework;
using SoulsLike.Entities.Character.Input;
using SoulsLike.Entities.Character.Runtime;
using CharacterRuntimeType = SoulsLike.Entities.Character.Runtime.CharacterRuntime;

namespace SoulsLike.Tests.CharacterRuntime
{
    public sealed class CharacterRuntimeTests
    {
        private sealed class FakeClock : ICharacterClock
        {
            public float Now { get; set; }
        }

        private sealed class ActionExecutor : ICharacterActionExecutor
        {
            public int AttackCalls { get; private set; }
            public int RollCalls { get; private set; }
            public int JumpCalls { get; private set; }
            public int EquipmentStartCalls { get; private set; }
            public AttackIntent LastAttackIntent { get; private set; }
            public CharacterCommandExecutionStatus AttackResult { get; set; } =
                CharacterCommandExecutionStatus.Executed;
            public CharacterCommandExecutionStatus RollResult { get; set; } =
                CharacterCommandExecutionStatus.Executed;
            public CharacterCommandExecutionStatus JumpResult { get; set; } =
                CharacterCommandExecutionStatus.Executed;
            public CharacterCommandExecutionStatus EquipmentStartResult { get; set; } =
                CharacterCommandExecutionStatus.Executed;
            public CharacterCommandExecutionStatus EquipmentAdvanceResult { get; set; } =
                CharacterCommandExecutionStatus.Executed;
            public bool IsEquipmentActionInProgress { get; set; }

            public CharacterCommandExecutionStatus TryStartAttack(
                in AttackRequest request)
            {
                AttackCalls++;
                LastAttackIntent = request.Intent;
                return AttackResult;
            }

            public CharacterCommandExecutionStatus TryStartRoll(in RollRequest request)
            {
                RollCalls++;
                return RollResult;
            }

            public CharacterCommandExecutionStatus TryStartJump(in JumpRequest request)
            {
                JumpCalls++;
                return JumpResult;
            }

            public CharacterCommandExecutionStatus TryStartEquipmentAction(
                in EquipmentActionRequest request)
            {
                EquipmentStartCalls++;
                return EquipmentStartResult;
            }

            public CharacterCommandExecutionStatus TryAdvanceEquipmentAction() =>
                EquipmentAdvanceResult;
        }

        [Test]
        public void BufferUsesLatestCommandAndReportsExpiryAfterOneSecond()
        {
            FakeClock clock = new FakeClock();
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(clock);
            ActionExecutor executor = new ActionExecutor();
            CharacterCommand first = CharacterCommand.Attack(
                AttackIntent.Light,
                false,
                false);
            CharacterCommand second = CharacterCommand.Attack(
                AttackIntent.Heavy,
                false,
                false);

            buffer.Store(first);
            buffer.Store(second);

            Assert.That(buffer.TryPeek(out CharacterCommand actual), Is.True);
            actual.TryExecute(executor);
            Assert.That(executor.LastAttackIntent, Is.EqualTo(AttackIntent.Heavy));
            clock.Now = 1f;
            Assert.That(buffer.IsExpired(), Is.True);
        }

        [Test]
        public void AttackStateRetainsExpiredCommandUntilQueueWindow()
        {
            FakeClock clock = new FakeClock();
            ActionExecutor executor = new ActionExecutor();
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(clock);
            CharacterActionStateMachine machine = CreateMachine(buffer);

            Assert.That(machine.Submit(CharacterCommand.Attack(
                AttackIntent.Light, false, false), executor), Is.EqualTo(
                CharacterCommandDisposition.Executed));
            Assert.That(machine.Submit(CharacterCommand.Attack(
                AttackIntent.Heavy, false, false), executor), Is.EqualTo(
                CharacterCommandDisposition.Buffered));

            clock.Now = 2f;
            machine.Tick(default, executor);
            Assert.That(buffer.HasCommand, Is.True);

            machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.QueueWindowOpened,
                CharacterActionStateId.Attack), executor);
            Assert.That(buffer.HasCommand, Is.False);
            Assert.That(executor.AttackCalls, Is.EqualTo(2));
        }

        [Test]
        public void FakeAiCanSubmitAttackDataWithoutInputSystem()
        {
            ActionExecutor executor = new ActionExecutor();
            MovementGate gate = new MovementGate();
            CharacterActionStateMachine machine = CreateMachine(
                new CharacterCommandBuffer(new FakeClock()));
            CharacterRuntimeType runtime = new CharacterRuntimeType(machine, gate);

            CharacterCommandDisposition disposition = runtime.Submit(
                CharacterCommand.Attack(AttackIntent.Light, false, false),
                executor);

            Assert.That(disposition, Is.EqualTo(CharacterCommandDisposition.Executed));
            Assert.That(executor.AttackCalls, Is.EqualTo(1));
        }

        [Test]
        public void InputBlockRejectsNewCommandsWithoutBufferingThem()
        {
            ActionExecutor executor = new ActionExecutor();
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(new FakeClock());
            CharacterActionStateMachine machine = CreateMachine(buffer);
            machine.SetInputBlocked(true);

            CharacterCommandDisposition disposition = machine.Submit(
                CharacterCommand.Attack(AttackIntent.Light, false, false),
                executor);

            Assert.That(disposition, Is.EqualTo(CharacterCommandDisposition.Ignored));
            Assert.That(executor.AttackCalls, Is.Zero);
            Assert.That(buffer.HasCommand, Is.False);
        }

        [Test]
        public void NeutralStatePrunesExpiredTemporarilyBlockedCommand()
        {
            FakeClock clock = new FakeClock();
            ActionExecutor executor = new ActionExecutor
            {
                AttackResult = CharacterCommandExecutionStatus.TemporarilyBlocked
            };
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(clock);
            CharacterActionStateMachine machine = CreateMachine(buffer);

            Assert.That(machine.Submit(CharacterCommand.Attack(
                AttackIntent.Light, false, false), executor),
                Is.EqualTo(CharacterCommandDisposition.Buffered));

            clock.Now = 1f;
            machine.Tick(default, executor);

            Assert.That(buffer.HasCommand, Is.False);
        }

        [Test]
        public void AttackQueueAllowsGuardThroughAnimationBlockOnly()
        {
            ActionExecutor executor = new ActionExecutor();
            MovementGate gate = new MovementGate();
            CharacterActionStateMachine machine = CreateMachine(
                new CharacterCommandBuffer(new FakeClock()));
            CharacterRuntimeType runtime = new CharacterRuntimeType(machine, gate);

            runtime.Submit(CharacterCommand.Attack(
                AttackIntent.Light, false, false), executor);
            runtime.SetAnimationMotionContract(true, false);
            Assert.That(runtime.ResolveMovementPolicy(false).GuardAllowed, Is.False);

            runtime.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.QueueWindowOpened,
                CharacterActionStateId.Attack), executor);
            Assert.That(runtime.ResolveMovementPolicy(false).GuardAllowed, Is.True);

            runtime.SetMovementBlocked(true);
            Assert.That(runtime.ResolveMovementPolicy(false).GuardAllowed, Is.False);
        }

        [Test]
        public void ContradictoryAnimationSignalDoesNotAlterCurrentState()
        {
            ActionExecutor executor = new ActionExecutor();
            CharacterActionStateMachine machine = CreateMachine(
                new CharacterCommandBuffer(new FakeClock()));

            bool handled = machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.Exited,
                CharacterActionStateId.Roll), executor);

            Assert.That(handled, Is.False);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterActionStateId.Neutral));
        }

        [Test]
        public void AttackQueueDoesNotExecuteOrBufferEquipmentCommand()
        {
            ActionExecutor executor = new ActionExecutor();
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(new FakeClock());
            CharacterActionStateMachine machine = CreateMachine(buffer);
            machine.Submit(CharacterCommand.Attack(
                AttackIntent.Light, false, false), executor);
            machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.QueueWindowOpened,
                CharacterActionStateId.Attack), executor);

            CharacterCommandDisposition disposition = machine.Submit(
                CharacterCommand.Equipment(EquipmentActionKind.SwitchRightWeapon),
                executor);

            Assert.That(disposition, Is.EqualTo(CharacterCommandDisposition.Ignored));
            Assert.That(executor.EquipmentStartCalls, Is.Zero);
            Assert.That(buffer.HasCommand, Is.False);
        }

        [Test]
        public void EquipmentSwapAcceptsExactlyOneSameFrameCompanionCommand()
        {
            ActionExecutor executor = new ActionExecutor
            {
                IsEquipmentActionInProgress = true
            };
            CharacterActionStateMachine machine = CreateMachine(
                new CharacterCommandBuffer(new FakeClock()));

            CharacterInputBatch batch = new CharacterInputBatch(
                default,
                CharacterCommand.Equipment(EquipmentActionKind.SwitchRightWeapon),
                CharacterCommand.Equipment(EquipmentActionKind.ToggleHandMode));
            machine.Tick(batch, executor);
            CharacterCommandDisposition thirdDisposition = machine.Submit(
                CharacterCommand.Equipment(EquipmentActionKind.SwitchQuickItem),
                executor);

            Assert.That(machine.CurrentState, Is.EqualTo(
                CharacterActionStateId.EquipmentSwap));
            Assert.That(executor.EquipmentStartCalls, Is.EqualTo(2));
            Assert.That(thirdDisposition, Is.EqualTo(CharacterCommandDisposition.Ignored));
        }

        [Test]
        public void ChainedAttackIgnoresPreviousAnimationsExitCallback()
        {
            ActionExecutor executor = new ActionExecutor();
            CharacterActionStateMachine machine = CreateMachine(
                new CharacterCommandBuffer(new FakeClock()));
            machine.Submit(CharacterCommand.Attack(
                AttackIntent.Light, false, false), executor);
            machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.QueueWindowOpened,
                CharacterActionStateId.Attack), executor);
            machine.Submit(CharacterCommand.Attack(
                AttackIntent.Heavy, false, false), executor);

            machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.Exited,
                CharacterActionStateId.Attack), executor);

            Assert.That(machine.CurrentState, Is.EqualTo(CharacterActionStateId.Attack));

            machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.Exited,
                CharacterActionStateId.Attack), executor);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterActionStateId.Neutral));
        }

        [Test]
        public void JumpStartsWithoutChangingActionStateFromNeutral()
        {
            ActionExecutor executor = new ActionExecutor();
            CharacterActionStateMachine machine = CreateMachine(
                new CharacterCommandBuffer(new FakeClock()));

            CharacterCommandDisposition disposition = machine.Submit(
                CharacterCommand.Jump(false), executor);

            Assert.That(disposition, Is.EqualTo(CharacterCommandDisposition.Executed));
            Assert.That(executor.JumpCalls, Is.EqualTo(1));
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterActionStateId.Neutral));
        }

        [Test]
        public void MovementGateKeepsIndependentBlockReasons()
        {
            MovementGate gate = new MovementGate();
            gate.Set(MovementGateReason.Manual, true);
            gate.Set(MovementGateReason.Animation, true);
            gate.Set(MovementGateReason.Manual, false);

            Assert.That(gate.IsBlocked, Is.True);
            Assert.That(gate.IsSet(MovementGateReason.Animation), Is.True);
        }

        [Test]
        public void SprintTapRequestsRollButQualifiedHoldDoesNot()
        {
            SprintRollGestureResolver gesture = new SprintRollGestureResolver();
            gesture.Update(true, true, false, 0f);
            gesture.Update(false, false, true, 0.1f);
            Assert.That(gesture.ShouldRoll(true), Is.True);

            gesture.Update(true, true, false, 0f);
            gesture.Update(false, true, false, 0.31f);
            Assert.That(gesture.IsSprinting, Is.True);
            gesture.Update(false, false, true, 0f);
            Assert.That(gesture.ShouldRoll(true), Is.False);
        }

        [Test]
        public void SprintHeldDuringRollInterruptsOnlyWhenQueueWindowOpens()
        {
            SprintRollGestureResolver gesture = new SprintRollGestureResolver();
            gesture.Update(true, true, false, 0f);
            gesture.Update(false, true, false, 0.31f);
            Assert.That(gesture.IsSprinting, Is.True);
            ActionExecutor executor = new ActionExecutor();
            CharacterActionStateMachine machine = CreateMachine(
                new CharacterCommandBuffer(new FakeClock()));
            machine.Submit(CharacterCommand.Roll(default, 0f, true), executor);
            CharacterControlFrame sprintFrame = new CharacterControlFrame(
                default, 0f, gesture.IsSprinting, false, false, false);

            machine.Tick(new CharacterInputBatch(sprintFrame), executor);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterActionStateId.Roll));
            Assert.That(machine.TryConsumeRollSprintInterrupt(), Is.False);

            machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.QueueWindowOpened,
                CharacterActionStateId.Roll), executor);

            Assert.That(machine.CurrentState, Is.EqualTo(CharacterActionStateId.Neutral));
            Assert.That(machine.TryConsumeRollSprintInterrupt(), Is.True);
            Assert.That(machine.TryConsumeRollSprintInterrupt(), Is.False);
            Assert.That(executor.RollCalls, Is.EqualTo(1));
            gesture.Update(false, false, true, 0f);
            Assert.That(gesture.ShouldRoll(true), Is.False);
        }

        [Test]
        public void HeavyPressSuppressesLightUntilLightRelease()
        {
            HeavyAttackGestureResolver gesture = new HeavyAttackGestureResolver();
            Assert.That(gesture.TryResolve(true, false, true, true), Is.True);
            Assert.That(gesture.ShouldSuppressLightAttack(true), Is.True);
            gesture.TryResolve(false, true, false, true);
            Assert.That(gesture.ShouldSuppressLightAttack(false), Is.True);
            gesture.TryResolve(false, false, false, true);
            Assert.That(gesture.ShouldSuppressLightAttack(true), Is.False);
        }

        private static CharacterActionStateMachine CreateMachine(
            CharacterCommandBuffer buffer)
        {
            return new CharacterActionStateMachine(buffer);
        }
    }
}
