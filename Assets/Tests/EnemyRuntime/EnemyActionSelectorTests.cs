using System;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class EnemyActionSelectorTests
    {
        [Test]
        public void ChooseDoesNotCommitUntilTheMoveStarts()
        {
            object move = CreateMove("LightAttack1", cooldown: 1f);
            object selector = CreateSelector();

            object selected = Choose(selector, new[] { move }, null, false, 0f);

            Assert.That(selected, Is.SameAs(move));
            Assert.That(GetPreviousMove(selector), Is.Null);

            CommitStarted(selector, move, 0f);

            Assert.That(GetPreviousMove(selector), Is.SameAs(move));
            Assert.That(Choose(selector, new[] { move }, null, false, 0f), Is.Null);
        }

        [Test]
        public void UncommittedMultiCandidateSelectionDoesNotAdvanceTheSequence()
        {
            object firstMove = CreateMove("LightAttack1");
            object secondMove = CreateMove("HeavyAttack");
            object selector = CreateSelector();

            object selected = Choose(selector, new[] { firstMove, secondMove }, null, false, 0f);

            Assert.That(
                Choose(selector, new[] { firstMove, secondMove }, null, false, 0f),
                Is.SameAs(selected));
            Assert.That(GetPreviousMove(selector), Is.Null);
            Assert.That(GetCommittedSelectionCount(selector), Is.Zero);
        }

        [Test]
        public void DuplicateMoveEntriesRemainIndependentTransactionIdentities()
        {
            object action = CreateAction("LightAttack1", Array.Empty<object>());
            object firstMove = CreateMove(action, cooldown: 1f);
            object secondMove = CreateMove(action, cooldown: 1f);
            object selector = CreateSelector();

            CommitStarted(selector, firstMove, 0f);

            Assert.That(Choose(selector, new[] { secondMove }, null, false, 0f), Is.SameAs(secondMove));
        }

        [Test]
        public void FollowUpEligibilityUsesExactActionReferenceTopology()
        {
            object followUpAction = CreateAction("Combo1", Array.Empty<object>());
            object openerAction = CreateAction("LightAttack1", new[] { followUpAction });
            object openerMove = CreateMove(openerAction, usage: "Opener");
            object followUpMove = CreateMove(followUpAction, usage: "FollowUp");
            object selector = CreateSelector();

            Assert.That(
                Choose(selector, new[] { followUpMove }, openerMove, true, 0f),
                Is.SameAs(followUpMove));
        }

        [Test]
        public void CommittedStartAdvancesSelectionSequenceAndCooldown()
        {
            object move = CreateMove("LightAttack1", cooldown: 1f);
            object selector = CreateSelector();

            CommitStarted(selector, move, 0f);

            Assert.That(GetCommittedSelectionCount(selector), Is.EqualTo(1));
            Assert.That(Choose(selector, new[] { move }, null, false, 0f), Is.Null);
        }

        [Test]
        public void ExactMoveCanBeSelectedWithoutLineOfSightWhenAuthored()
        {
            object move = CreateMove("LightAttack1", requiresLineOfSight: false);
            object selector = CreateSelector();

            Assert.That(
                Choose(selector, new[] { move }, null, false, 0f, hasLineOfSight: false),
                Is.SameAs(move));
        }

        [Test]
        public void OpenerMoveCannotBeQueuedEvenWhenItsActionIsAnAuthoredFollowUp()
        {
            object followUpAction = CreateAction("Combo1", Array.Empty<object>());
            object currentAction = CreateAction("LightAttack1", new[] { followUpAction });
            object currentMove = CreateMove(currentAction, usage: "Any");
            object openerMove = CreateMove(followUpAction, usage: "Opener");
            Type controllerType = GetRequiredType(
                "SoulsLike.Entities.Enemy.EnemyActionExecutor");
            var gameObject = new GameObject("EnemyActionExecutor");
            try
            {
                Component controller = gameObject.AddComponent(controllerType);
                SetPrivateField(controllerType, controller, "<CurrentMove>k__BackingField", currentMove);
                SetPrivateField(
                    controllerType,
                    controller,
                    "<Mode>k__BackingField",
                    Enum.Parse(GetRequiredType("SoulsLike.Entities.Enemy.EnemyExecutionMode"), "Action"));
                SetPrivateField(controllerType, controller, "<ComboWindowOpen>k__BackingField", true);

                bool queued = (bool)controllerType.GetMethod("TryQueue").Invoke(
                    controller,
                    new[] { openerMove });

                Assert.That(queued, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [TestCase("Approach")]
        [TestCase("Hold")]
        [TestCase("CircleLeft")]
        [TestCase("CircleRight")]
        [TestCase("Retreat")]
        [TestCase("Guard")]
        [TestCase("Attack")]
        public void CombatMovementResultsAreExplicit(string movement)
        {
            Type movementType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyCombatMovement");

            Assert.That(Enum.IsDefined(movementType, movement), Is.True);
        }

        private static object CreateSelector()
        {
            Type profileType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyBehaviourProfile");
            object profile = ScriptableObject.CreateInstance(profileType);
            Type actorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActor");
            object actor = FormatterServices.GetUninitializedObject(actorType);
            SetPrivateField(actorType, actor, "<BehaviourProfile>k__BackingField", profile);
            SetPrivateField(actorType, actor, "<RandomSeedOffset>k__BackingField", 0);
            return Activator.CreateInstance(
                GetRequiredType("SoulsLike.Entities.Enemy.EnemyActionSelector"),
                actor);
        }

        private static object CreateMove(
            string actionId,
            float cooldown = 0f,
            string usage = "Any",
            bool requiresLineOfSight = true) =>
            CreateMove(
                CreateAction(actionId, Array.Empty<object>()),
                cooldown,
                usage,
                requiresLineOfSight);

        private static object CreateMove(
            object action,
            float cooldown = 0f,
            string usage = "Any",
            bool requiresLineOfSight = true)
        {
            Type moveType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyMove");
            object move = Activator.CreateInstance(moveType);
            SetPrivateField(moveType, move, "action", action);
            SetPrivateField(moveType, move, "cooldown", cooldown);
            SetPrivateField(moveType, move, "requiresLineOfSight", requiresLineOfSight);
            SetPrivateField(
                moveType,
                move,
                "usage",
                Enum.Parse(GetRequiredType("SoulsLike.Entities.Enemy.EnemyMove+Usage"), usage));
            return move;
        }

        private static object CreateAction(string actionId, object[] followUps)
        {
            Type actionType = GetRequiredType("SoulsLike.Entities.Combat.CharacterActionDefinition");
            object action = ScriptableObject.CreateInstance(actionType);
            SetPrivateField(
                actionType,
                action,
                "actionId",
                Enum.Parse(GetRequiredType("SoulsLike.Entities.Combat.CharacterActionId"), actionId));
            Array followUpArray = Array.CreateInstance(actionType, followUps.Length);
            for (int index = 0; index < followUps.Length; index++)
            {
                followUpArray.SetValue(followUps[index], index);
            }

            SetPrivateField(actionType, action, "followUps", followUpArray);
            return action;
        }

        private static object Choose(
            object selector,
            object[] moves,
            object currentMove,
            bool isFollowUp,
            float now,
            bool hasLineOfSight = true)
        {
            Type moveType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyMove");
            Array moveArray = Array.CreateInstance(moveType, moves.Length);
            for (int index = 0; index < moves.Length; index++)
            {
                moveArray.SetValue(moves[index], index);
            }

            return selector.GetType().GetMethod("Choose").Invoke(
                selector,
                new object[] { moveArray, 1f, 0f, hasLineOfSight, currentMove, isFollowUp, now });
        }

        private static void CommitStarted(object selector, object move, float now) =>
            selector.GetType().GetMethod("CommitStarted").Invoke(selector, new[] { move, (object)now });

        private static object GetPreviousMove(object selector) =>
            selector.GetType().GetProperty("PreviousMove").GetValue(selector);

        private static int GetCommittedSelectionCount(object selector) =>
            (int)selector.GetType().GetField(
                "_committedSelectionCount",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(selector);

        private static void SetPrivateField(Type type, object instance, string name, object value) =>
            type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value);

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
