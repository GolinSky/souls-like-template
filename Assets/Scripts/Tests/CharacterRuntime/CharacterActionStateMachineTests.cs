using NUnit.Framework;
using System.Numerics;
using SoulsLike.Entities.Character.Runtime;

namespace SoulsLike.Tests.CharacterRuntime
{
    public sealed class CharacterActionStateMachineTests
    {
        [Test]
        public void StateBlockedActionBuffersUntilQueueCheck()
        {
            var machine = new CharacterActionStateMachine();
            CharacterAction attack = Attack();
            CharacterAction roll = Roll();
            Start(machine, attack, CharacterAction.State.Attack);

            Assert.That(machine.TryDispatch(roll, 1f), Is.False);
            Assert.That(machine.HandleQueueCheck(CharacterAction.State.Attack), Is.True);
            Assert.That(machine.TryGetBufferedAction(out CharacterAction buffered), Is.True);
            Assert.That(buffered.ActionKind, Is.EqualTo(CharacterAction.Kind.Roll));
        }

        [Test]
        public void TransientExecutionResultBuffersBufferableAction()
        {
            var machine = new CharacterActionStateMachine();
            CharacterAction attack = Attack();

            Assert.That(machine.TryDispatch(attack, 1f), Is.True);
            machine.ReportExecution(attack, CharacterAction.Result.TemporarilyBlocked, CharacterAction.State.Attack, 1f);

            Assert.That(machine.TryGetBufferedAction(out CharacterAction buffered), Is.True);
            Assert.That(buffered.ActionKind, Is.EqualTo(CharacterAction.Kind.Attack));
        }

        [Test]
        public void ExpiredNeutralBufferSurvivesSameFrameActionThatEntersAttack()
        {
            var machine = new CharacterActionStateMachine();
            CharacterAction attack = Attack();
            CharacterAction roll = Roll();
            Start(machine, attack, CharacterAction.State.Attack);
            machine.TryDispatch(roll, 0f);
            machine.HandleExited(CharacterAction.State.Attack);

            Assert.That(machine.TryDispatch(attack, 2f), Is.True);
            machine.ReportExecution(attack, CharacterAction.Result.Executed, CharacterAction.State.Attack, 2f);
            machine.PruneExpiredBuffer(2f);
            machine.HandleQueueCheck(CharacterAction.State.Attack);

            Assert.That(machine.TryGetBufferedAction(out CharacterAction buffered), Is.True);
            Assert.That(buffered.ActionKind, Is.EqualTo(CharacterAction.Kind.Roll));
        }

        [Test]
        public void ExpiredBufferRemainsAvailableAtQueueCheckButNotAfterNeutralPrune()
        {
            var machine = new CharacterActionStateMachine();
            CharacterAction attack = Attack();
            CharacterAction roll = Roll();
            Start(machine, attack, CharacterAction.State.Attack);
            machine.TryDispatch(roll, 0f);

            machine.HandleQueueCheck(CharacterAction.State.Attack);
            Assert.That(machine.TryGetBufferedAction(out _), Is.True);

            machine.HandleExited(CharacterAction.State.Attack);
            machine.PruneExpiredBuffer(2f);
            Assert.That(machine.TryGetBufferedAction(out _), Is.False);
        }

        [Test]
        public void InputBlockIgnoresActionWithoutBuffering()
        {
            var machine = new CharacterActionStateMachine();
            machine.SetInputBlocked(true);

            Assert.That(machine.TryDispatch(Attack(), 1f), Is.False);
            Assert.That(machine.TryGetBufferedAction(out _), Is.False);
        }

        [Test]
        public void ContradictoryAnimationSignalIsIgnored()
        {
            var machine = new CharacterActionStateMachine();
            Start(machine, Attack(), CharacterAction.State.Attack);

            Assert.That(machine.HandleQueueCheck(CharacterAction.State.Roll), Is.False);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.Attack));
        }

        [Test]
        public void EquipmentDoesNotBufferAndAllowsOnlyOneCompanionDispatch()
        {
            var machine = new CharacterActionStateMachine();
            CharacterAction swap = CharacterAction.Equipment(CharacterAction.EquipmentKind.SwitchRightWeapon);
            CharacterAction handMode = CharacterAction.Equipment(CharacterAction.EquipmentKind.ToggleHandMode);
            Start(machine, swap, CharacterAction.State.EquipmentSwap);

            Assert.That(machine.TryDispatch(handMode, 1f), Is.True);
            Assert.That(machine.TryDispatch(swap, 1f), Is.False);
            Assert.That(machine.TryGetBufferedAction(out _), Is.False);
        }

        [Test]
        public void ChainedActionIgnoresItsFirstExit()
        {
            var machine = new CharacterActionStateMachine();
            CharacterAction attack = Attack();
            Start(machine, attack, CharacterAction.State.Attack);
            machine.HandleQueueCheck(CharacterAction.State.Attack);
            machine.ReportExecution(attack, CharacterAction.Result.Executed, CharacterAction.State.Attack, 1f);

            Assert.That(machine.HandleExited(CharacterAction.State.Attack), Is.True);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.Attack));
            machine.HandleExited(CharacterAction.State.Attack);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.Neutral));
        }

        [Test]
        public void MultipleChainedRollsIgnoreExitsUntilFinalRollCompletes()
        {
            var machine = new CharacterActionStateMachine();
            CharacterAction roll = Roll();
            Start(machine, roll, CharacterAction.State.Roll);

            machine.HandleQueueCheck(CharacterAction.State.Roll);
            machine.ReportExecution(roll, CharacterAction.Result.Executed, CharacterAction.State.Roll, 1f);

            machine.HandleQueueCheck(CharacterAction.State.Roll);
            machine.ReportExecution(roll, CharacterAction.Result.Executed, CharacterAction.State.Roll, 2f);

            Assert.That(machine.HandleExited(CharacterAction.State.Roll), Is.True);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.Roll));

            Assert.That(machine.HandleExited(CharacterAction.State.Roll), Is.True);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.Roll));

            Assert.That(machine.HandleExited(CharacterAction.State.Roll), Is.True);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.Neutral));
        }

        [Test]
        public void SprintInterruptsRollOnlyWhenQueueCheckOpens()
        {
            var machine = new CharacterActionStateMachine();
            Start(machine, Roll(), CharacterAction.State.Roll);

            machine.Tick(true, false);
            Assert.That(machine.TryConsumeRollSprintInterrupt(), Is.False);
            machine.HandleQueueCheck(CharacterAction.State.Roll);

            Assert.That(machine.TryConsumeRollSprintInterrupt(), Is.True);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.Neutral));
        }

        [Test]
        public void JumpExecutionLeavesActionStateNeutral()
        {
            var machine = new CharacterActionStateMachine();
            CharacterAction jump = CharacterAction.Jump(false);

            Assert.That(machine.TryDispatch(jump, 1f), Is.True);
            machine.ReportExecution(jump, CharacterAction.Result.Executed, CharacterAction.State.Neutral, 1f);

            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.Neutral));
        }

        [Test]
        public void BlockHitActionBuffersUntilQueueCheck()
        {
            var machine = new CharacterActionStateMachine();
            Assert.That(machine.HandleEntered(CharacterAction.State.BlockHit), Is.True);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.BlockHit));

            Assert.That(machine.TryDispatch(Roll(), 1f), Is.False);
            Assert.That(machine.HandleQueueCheck(CharacterAction.State.BlockHit), Is.True);
            Assert.That(machine.TryGetBufferedAction(out CharacterAction buffered), Is.True);
            Assert.That(buffered.ActionKind, Is.EqualTo(CharacterAction.Kind.Roll));
        }

        [Test]
        public void BlockHitAllowsAttackAndRollAtQueueCheck()
        {
            var machine = new CharacterActionStateMachine();
            machine.HandleEntered(CharacterAction.State.BlockHit);

            Assert.That(machine.TryDispatch(Attack(), 1f), Is.False);
            machine.HandleQueueCheck(CharacterAction.State.BlockHit);
            Assert.That(machine.TryDispatch(Attack(), 1f), Is.True);
        }

        [Test]
        public void BlockHitExitedReturnsToNeutral()
        {
            var machine = new CharacterActionStateMachine();
            machine.HandleEntered(CharacterAction.State.BlockHit);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.BlockHit));

            machine.HandleExited(CharacterAction.State.BlockHit);
            Assert.That(machine.CurrentState, Is.EqualTo(CharacterAction.State.Neutral));
        }

        [Test]
        public void ToggleHandModeBeatsCombatInput()
        {
            var coordinator = new CharacterActionCoordinator(new CharacterActionStateMachine());

            CharacterInput input = coordinator.BuildInput(
                Frame(attackPressed: true, twoHandedPressed: true));

            Assert.That(input.FirstAction.Value.EquipmentAction,
                Is.EqualTo(CharacterAction.EquipmentKind.ToggleHandMode));
            Assert.That(input.SecondAction, Is.Null);
        }

        [Test]
        public void EquipmentAndToggleHandModeRemainTheOnlySameFramePair()
        {
            var coordinator = new CharacterActionCoordinator(new CharacterActionStateMachine());

            CharacterInput input = coordinator.BuildInput(
                Frame(switchWeaponPressed: true, twoHandedPressed: true));

            Assert.That(input.FirstAction.Value.EquipmentAction,
                Is.EqualTo(CharacterAction.EquipmentKind.SwitchRightWeapon));
            Assert.That(input.SecondAction.Value.EquipmentAction,
                Is.EqualTo(CharacterAction.EquipmentKind.ToggleHandMode));
        }

        [Test]
        public void HeavyPressDuringRollDoesNotSuppressLaterLightAttack()
        {
            var machine = new CharacterActionStateMachine();
            var coordinator = new CharacterActionCoordinator(machine);
            machine.ReportExecution(
                Roll(), CharacterAction.Result.Executed, CharacterAction.State.Roll, 0f);
            coordinator.BuildInput(Frame(strongAttackPressed: true, attackHeld: true));

            machine.HandleExited(CharacterAction.State.Roll);
            CharacterInput input = coordinator.BuildInput(
                Frame(attackPressed: true, attackHeld: true));

            Assert.That(input.FirstAction.Value.Intent,
                Is.EqualTo(CharacterAction.AttackIntent.Light));
        }

        [Test]
        public void StaminaAdmissionPreservesThresholdAndZeroCostRules()
        {
            Assert.That(CharacterStaminaPolicy.CanStart(10f, 100f, 0f, 50f), Is.True);
            Assert.That(CharacterStaminaPolicy.CanStart(50f, 100f, 10f, 50f), Is.False);
            Assert.That(CharacterStaminaPolicy.CanStart(51f, 100f, 10f, 50f), Is.True);
            Assert.That(CharacterStaminaPolicy.CanStart(-100f, 100f, 10f, -500f), Is.False);
        }

        [Test]
        public void AttackStaminaUsesHeavyValuesForHeavyAndSpecialActions()
        {
            Assert.That(
                CharacterStaminaPolicy.CalculateAttackCost(false, 10f, 20f, 1.5f),
                Is.EqualTo(15f));
            Assert.That(
                CharacterStaminaPolicy.CalculateAttackCost(true, 10f, 20f, 1.5f),
                Is.EqualTo(30f));
            Assert.That(
                CharacterStaminaPolicy.GetAttackStartThreshold(true, 5f, 15f),
                Is.EqualTo(15f));
        }

        private static CharacterAction Attack() => CharacterAction.Attack(
            CharacterAction.AttackIntent.Light, false, false, default, 0f);
        private static CharacterAction Roll() => CharacterAction.Roll(default, 0f);
        private static CharacterInputFrame Frame(
            bool attackPressed = false,
            bool attackHeld = false,
            bool strongAttackPressed = false,
            bool switchWeaponPressed = false,
            bool twoHandedPressed = false)
        {
            return new CharacterInputFrame(
                Vector2.Zero, 0f, false, false,
                false, false, false, attackPressed, attackHeld,
                strongAttackPressed, false, false, switchWeaponPressed,
                false, false, false, twoHandedPressed, false);
        }
        private static void Start(CharacterActionStateMachine machine, CharacterAction action, CharacterAction.State state) =>
            machine.ReportExecution(action, CharacterAction.Result.Executed, state, 0f);
    }
}
