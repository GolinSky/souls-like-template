using NUnit.Framework;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Entities.Character.Input;
using CharacterRuntimeType = SoulsLike.Entities.Character.Runtime.CharacterRuntime;

namespace SoulsLike.Tests.CharacterRuntime
{
    public sealed class CharacterRuntimeTests
    {
        private sealed class FakeClock : ICharacterClock
        {
            public float Now { get; set; }
        }

        private sealed class AttackReceiver : IAttackCommandReceiver
        {
            public int Calls { get; private set; }
            public CharacterCommandExecutionStatus Result { get; set; } =
                CharacterCommandExecutionStatus.Executed;

            public CharacterCommandExecutionStatus TryStartAttack(
                in AttackRequest request)
            {
                Calls++;
                return Result;
            }

            public void SetStrongAttackHeld(bool held) { }
        }

        private sealed class MovementReceiver : IMovementCommandReceiver
        {
            public CharacterCommandExecutionStatus TryStartRoll(
                in RollRequest request) => CharacterCommandExecutionStatus.Executed;
            public CharacterCommandExecutionStatus TryStartJump(
                in JumpRequest request) => CharacterCommandExecutionStatus.Executed;
        }

        private sealed class EquipmentReceiver : IEquipmentCommandReceiver
        {
            public int StartCalls { get; private set; }
            public bool IsEquipmentActionInProgress { get; set; }
            public CharacterCommandExecutionStatus TryStartEquipmentAction(
                in EquipmentActionRequest request)
            {
                StartCalls++;
                return CharacterCommandExecutionStatus.Executed;
            }
            public CharacterCommandExecutionStatus TryAdvanceEquipmentAction() =>
                CharacterCommandExecutionStatus.Executed;
        }

        [Test]
        public void BufferUsesLatestCommandAndReportsExpiryAfterOneSecond()
        {
            FakeClock clock = new FakeClock();
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(clock);
            AttackReceiver receiver = new AttackReceiver();
            ICharacterCommand first = new AttackCommand(
                receiver,
                new AttackRequest(AttackIntent.Light, false, false));
            ICharacterCommand second = new AttackCommand(
                receiver,
                new AttackRequest(AttackIntent.Heavy, false, false));

            buffer.Store(first);
            buffer.Store(second);

            Assert.That(buffer.TryPeek(out ICharacterCommand actual), Is.True);
            Assert.That(actual, Is.SameAs(second));
            clock.Now = 1f;
            Assert.That(buffer.IsExpired(), Is.True);
        }

        [Test]
        public void TakingCommandConsumesItExactlyOnce()
        {
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(new FakeClock());
            ICharacterCommand command = new AttackCommand(
                new AttackReceiver(),
                new AttackRequest(AttackIntent.Light, false, false));
            buffer.Store(command);

            Assert.That(buffer.TryTake(out ICharacterCommand first), Is.True);
            Assert.That(first, Is.SameAs(command));
            Assert.That(buffer.TryTake(out _), Is.False);
        }

        [Test]
        public void AttackStateRetainsExpiredCommandUntilQueueWindow()
        {
            FakeClock clock = new FakeClock();
            AttackReceiver attack = new AttackReceiver();
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(clock);
            CharacterActionStateMachine machine = CreateMachine(buffer);
            CharacterCommandFactory factory = new CharacterCommandFactory(
                attack,
                new MovementReceiver(),
                new EquipmentReceiver());

            Assert.That(machine.Submit(factory.CreateAttack(
                AttackIntent.Light, false, false)), Is.EqualTo(
                CharacterCommandDisposition.Executed));
            Assert.That(machine.Submit(factory.CreateAttack(
                AttackIntent.Heavy, false, false)), Is.EqualTo(
                CharacterCommandDisposition.Buffered));

            clock.Now = 2f;
            machine.Tick(default);
            Assert.That(buffer.HasCommand, Is.True);

            machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.QueueWindowOpened,
                CharacterActionStateId.Attack));
            Assert.That(buffer.HasCommand, Is.False);
            Assert.That(attack.Calls, Is.EqualTo(2));
        }

        [Test]
        public void FakeAiCanSubmitAttackWithoutInputSystem()
        {
            AttackReceiver attack = new AttackReceiver();
            MovementGate gate = new MovementGate();
            CharacterActionStateMachine machine = new CharacterActionStateMachine(
                new EquipmentReceiver(),
                new CharacterCommandBuffer(new FakeClock()));
            CharacterRuntimeType runtime = new CharacterRuntimeType(machine, gate);

            CharacterCommandDisposition disposition = runtime.Submit(
                new AttackCommand(
                    attack,
                    new AttackRequest(AttackIntent.Light, false, false)));

            Assert.That(disposition, Is.EqualTo(CharacterCommandDisposition.Executed));
            Assert.That(attack.Calls, Is.EqualTo(1));
        }

        [Test]
        public void InputBlockRejectsNewCommandsWithoutBufferingThem()
        {
            AttackReceiver attack = new AttackReceiver();
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(new FakeClock());
            CharacterActionStateMachine machine = CreateMachine(buffer);
            machine.SetInputBlocked(true);

            CharacterCommandDisposition disposition = machine.Submit(
                new AttackCommand(
                    attack,
                    new AttackRequest(AttackIntent.Light, false, false)));

            Assert.That(disposition, Is.EqualTo(CharacterCommandDisposition.Ignored));
            Assert.That(attack.Calls, Is.Zero);
            Assert.That(buffer.HasCommand, Is.False);
        }

        [Test]
        public void NeutralStatePrunesExpiredTemporarilyBlockedCommand()
        {
            FakeClock clock = new FakeClock();
            AttackReceiver attack = new AttackReceiver
            {
                Result = CharacterCommandExecutionStatus.TemporarilyBlocked
            };
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(clock);
            CharacterActionStateMachine machine = CreateMachine(buffer);

            Assert.That(machine.Submit(new AttackCommand(
                attack,
                new AttackRequest(AttackIntent.Light, false, false))),
                Is.EqualTo(CharacterCommandDisposition.Buffered));

            clock.Now = 1f;
            machine.Tick(default);

            Assert.That(buffer.HasCommand, Is.False);
        }

        [Test]
        public void AttackQueueAllowsGuardThroughAnimationBlockOnly()
        {
            AttackReceiver attack = new AttackReceiver();
            MovementGate gate = new MovementGate();
            CharacterActionStateMachine machine = new CharacterActionStateMachine(
                new EquipmentReceiver(),
                new CharacterCommandBuffer(new FakeClock()));
            CharacterRuntimeType runtime = new CharacterRuntimeType(machine, gate);

            runtime.Submit(new AttackCommand(
                attack,
                new AttackRequest(AttackIntent.Light, false, false)));
            runtime.SetAnimationMotionContract(true, false);
            Assert.That(runtime.ResolveMovementPolicy(false).GuardAllowed, Is.False);

            runtime.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.QueueWindowOpened,
                CharacterActionStateId.Attack));
            Assert.That(runtime.ResolveMovementPolicy(false).GuardAllowed, Is.True);

            runtime.SetMovementBlocked(true);
            Assert.That(runtime.ResolveMovementPolicy(false).GuardAllowed, Is.False);
        }

        [Test]
        public void ContradictoryAnimationSignalIsReported()
        {
            CharacterActionStateMachine machine = CreateMachine(
                new CharacterCommandBuffer(new FakeClock()));

            bool handled = machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.Exited,
                CharacterActionStateId.Roll));

            Assert.That(handled, Is.False);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterActionStateId.Neutral));
        }

        [Test]
        public void AttackQueueDoesNotExecuteEquipmentCommand()
        {
            AttackReceiver attack = new AttackReceiver();
            EquipmentReceiver equipment = new EquipmentReceiver();
            CharacterActionStateMachine machine = new CharacterActionStateMachine(
                equipment,
                new CharacterCommandBuffer(new FakeClock()));
            machine.Submit(new AttackCommand(
                attack,
                new AttackRequest(AttackIntent.Light, false, false)));
            machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.QueueWindowOpened,
                CharacterActionStateId.Attack));

            CharacterCommandDisposition disposition = machine.Submit(
                new EquipmentCommand(equipment, new EquipmentActionRequest(0)));

            Assert.That(disposition, Is.EqualTo(CharacterCommandDisposition.Ignored));
            Assert.That(equipment.StartCalls, Is.Zero);
        }

        [Test]
        public void EquipmentSwapAcceptsOneSameFrameCompanionCommand()
        {
            EquipmentReceiver equipment = new EquipmentReceiver
            {
                IsEquipmentActionInProgress = true
            };
            CharacterActionStateMachine machine = new CharacterActionStateMachine(
                equipment,
                new CharacterCommandBuffer(new FakeClock()));

            CharacterInputBatch batch = new CharacterInputBatch(
                default,
                new EquipmentCommand(equipment, new EquipmentActionRequest(0)),
                new EquipmentCommand(equipment, new EquipmentActionRequest(4)),
                2);
            machine.Tick(batch);
            CharacterCommandDisposition thirdDisposition = machine.Submit(
                new EquipmentCommand(equipment, new EquipmentActionRequest(2)));

            Assert.That(machine.CurrentState, Is.EqualTo(CharacterActionStateId.EquipmentSwap));
            Assert.That(equipment.StartCalls, Is.EqualTo(2));
            Assert.That(thirdDisposition, Is.EqualTo(CharacterCommandDisposition.Ignored));
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
        public void SprintHeldDuringRollCompletesAtQueueWindowWithoutRequestingAnotherRoll()
        {
            SprintRollGestureResolver gesture = new SprintRollGestureResolver();
            gesture.Update(true, true, false, 0f);
            gesture.Update(false, true, false, 0.31f);
            Assert.That(gesture.IsSprinting, Is.True);
            CharacterCommandBuffer buffer = new CharacterCommandBuffer(new FakeClock());
            CharacterActionStateMachine machine = CreateMachine(buffer);
            CharacterCommandFactory factory = new CharacterCommandFactory(
                new AttackReceiver(),
                new MovementReceiver(),
                new EquipmentReceiver());
            machine.Submit(factory.CreateRoll(default, 0f, true));
            CharacterControlFrame sprintFrame = new CharacterControlFrame(
                default, 0f, gesture.IsSprinting, false, false, false);

            machine.Tick(new CharacterInputBatch(sprintFrame));
            machine.HandleAnimation(new CharacterAnimationSignal(
                CharacterAnimationSignalKind.QueueWindowOpened,
                CharacterActionStateId.Roll));

            Assert.That(machine.CurrentState, Is.EqualTo(CharacterActionStateId.Neutral));
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
            return new CharacterActionStateMachine(
                new EquipmentReceiver(),
                buffer);
        }
    }
}
