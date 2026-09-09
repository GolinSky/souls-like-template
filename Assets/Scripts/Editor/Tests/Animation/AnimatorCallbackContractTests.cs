using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SoulsLike.Items;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorStateMachineBehaviour = SoulsLike.Entities.Character.Components.Animations.AnimatorStateMachine;
using StateMachineName = SoulsLike.Entities.Character.Components.Animations.StateMachineName;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class AnimatorCallbackContractTests
    {
        private const string CHARACTER_PREFAB_PATH = "Assets/Prefabs/Models/Character/Character.prefab";
        private const string CONTROLLER_PATH = "Assets/Art/Animation/CharacterGreatSwordAnimator.controller";

        [Test]
        public void SupportedPlayerControllerRoutes_UseCharacterGreatSwordAnimatorAndRightHandProfilesOnly()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
            GameObject characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CHARACTER_PREFAB_PATH);
            Animator characterAnimator = characterPrefab.GetComponentInChildren<Animator>(true);

            Assert.That(controller, Is.Not.Null, $"Missing controller at {CONTROLLER_PATH}.");
            Assert.That(characterAnimator, Is.Not.Null,
                $"Character prefab at {CHARACTER_PREFAB_PATH} requires an Animator.");
            var defaultOverride = characterAnimator.runtimeAnimatorController as AnimatorOverrideController;
            Assert.That(defaultOverride, Is.Not.Null,
                "Character default controller must remain an AnimatorOverrideController.");
            Assert.That(defaultOverride.runtimeAnimatorController, Is.EqualTo(controller),
                "Character default override must use CharacterGreatSwordAnimator as its base.");

            string[] profileGuids = AssetDatabase.FindAssets("t:AnimationProfile", new[] { "Assets" });
            Assert.That(profileGuids, Is.Not.Empty, "At least one weapon animation profile is required.");
            foreach (string profileGuid in profileGuids)
            {
                string profilePath = AssetDatabase.GUIDToAssetPath(profileGuid);
                AnimationProfile profile = AssetDatabase.LoadAssetAtPath<AnimationProfile>(profilePath);
                Assert.That(profile.Controller, Is.Not.Null,
                    $"Animation profile at {profilePath} requires a right-hand controller.");
                Assert.That(profile.Controller, Is.EqualTo(controller),
                    $"Animation profile at {profilePath} must use the supported right-hand controller.");
                Assert.That(new SerializedObject(profile).FindProperty("<LeftHandController>k__BackingField"),
                    Is.Null, $"Animation profile at {profilePath} must not serialize a left-hand controller route.");
            }
        }

        [TestCase(StateMachineName.Spawn, false, 0f, false, 0f)]
        [TestCase(StateMachineName.Death, false, 0f, false, 0f)]
        [TestCase(StateMachineName.GraceUnblock, false, 0f, false, 0f)]
        [TestCase(StateMachineName.GraceRestStart, false, 0f, false, 0f)]
        [TestCase(StateMachineName.GraceRestIdle, false, 0f, false, 0f)]
        [TestCase(StateMachineName.GraceRestEnd, false, 0f, false, 0f)]
        [TestCase(StateMachineName.LightAttack, false, 0f, true, 0.5f)]
        [TestCase(StateMachineName.LightAttackAlt, false, 0f, true, 0.5f)]
        [TestCase(StateMachineName.HeavyAttack, true, 0.15f, true, 0.5f)]
        [TestCase(StateMachineName.HeavyAttackAlt, true, 0.15f, true, 0.5f)]
        [TestCase(StateMachineName.RollAttack, false, 0f, true, 0.5f)]
        [TestCase(StateMachineName.BackStepAttack, false, 0f, true, 0.5f)]
        [TestCase(StateMachineName.RunAttack, false, 0f, true, 0.5f)]
        [TestCase(StateMachineName.SpecialAttack, false, 0f, true, 0.5f)]
        [TestCase(StateMachineName.Roll, false, 0f, true, 0.5f)]
        [TestCase(StateMachineName.BackStep, false, 0f, true, 0.6f)]
        [TestCase(StateMachineName.EquipmentSwapOut, true, 0.5f, false, 0f)]
        [TestCase(StateMachineName.EquipmentSwapIn, true, 0.5f, false, 0f)]
        [TestCase(StateMachineName.ItemDrink, true, 0.48f, true, 0.68f)]
        [TestCase(StateMachineName.ItemDrinkEmpty, false, 0f, true, 0.78f)]
        [TestCase(StateMachineName.BlockHit, false, 0f, true, 0.5f)]
        [TestCase(StateMachineName.Parry, false, 0f, true, 0.55f)]
        [TestCase(StateMachineName.ShieldBlock, false, 0f, false, 0f)]
        [TestCase(StateMachineName.HitReaction, false, 0f, false, 0f)]
        [TestCase(StateMachineName.ParryStun, false, 0f, false, 0f)]
        public void CharacterGreatSwordAnimator_GameplayCallbacksHaveRequiredFlagsAndExitPaths(
            StateMachineName stateMachineName,
            bool reportsProgress,
            float progressThreshold,
            bool reportsQueueCheck,
            float queueCheckThreshold)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
            List<AnimatorState> states = FindStatesWithCallback(controller, stateMachineName);

            Assert.That(states, Is.Not.Empty,
                $"Required StateMachineName '{stateMachineName}' is not authored in {CONTROLLER_PATH}.");
            foreach (AnimatorState state in states)
            {
                AnimatorStateMachineBehaviour behaviour = state.behaviours
                    .OfType<AnimatorStateMachineBehaviour>()
                    .Single(candidate => GetStateMachineName(candidate) == stateMachineName);
                var serialized = new SerializedObject(behaviour);
                Assert.That(serialized.FindProperty("isReportingProgress").boolValue,
                    Is.EqualTo(reportsProgress), $"Unexpected Progress flag on '{state.name}'.");
                Assert.That(serialized.FindProperty("reportsQueueCheck").boolValue,
                    Is.EqualTo(reportsQueueCheck), $"Unexpected QueueCheck flag on '{state.name}'.");
                if (reportsProgress)
                {
                    Assert.That(serialized.FindProperty("progressNormalizedTime").floatValue,
                        Is.EqualTo(progressThreshold).Within(0.001f),
                        $"Unexpected Progress threshold on '{state.name}'.");
                }

                if (reportsQueueCheck)
                {
                    Assert.That(serialized.FindProperty("queueCheckNormalizedTime").floatValue,
                        Is.EqualTo(queueCheckThreshold).Within(0.001f),
                        $"Unexpected QueueCheck threshold on '{state.name}'.");
                }

                Assert.That(state.transitions.Any(transition => transition.isExit
                    || transition.destinationState != null
                    || transition.destinationStateMachine != null), Is.True,
                    $"State '{state.name}' must have an authored continuation; "
                    + "runtime trace coverage validates reachable Exit callbacks.");
            }
        }

        [Test]
        public void CharacterGreatSwordAnimator_CriticalAttackHasAuthoredExitPath()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
            AnimatorState criticalAttack = FindStates(controller.layers
                    .Select(layer => layer.stateMachine), "CriticalAttack")
                .Single();

            Assert.That(criticalAttack.transitions.Any(transition => transition.isExit
                || transition.destinationState != null
                || transition.destinationStateMachine != null), Is.True,
                "CriticalAttack must have an authored completion path.");
        }

        private static StateMachineName GetStateMachineName(AnimatorStateMachineBehaviour behaviour)
        {
            return (StateMachineName)new SerializedObject(behaviour)
                .FindProperty("stateMachineName").intValue;
        }

        private static List<AnimatorState> FindStatesWithCallback(
            AnimatorController controller,
            StateMachineName stateMachineName)
        {
            return FindStates(controller.layers.Select(layer => layer.stateMachine), null)
                .Where(state => state.behaviours.OfType<AnimatorStateMachineBehaviour>()
                    .Any(behaviour => GetStateMachineName(behaviour) == stateMachineName))
                .ToList();
        }

        private static IEnumerable<AnimatorState> FindStates(
            IEnumerable<UnityEditor.Animations.AnimatorStateMachine> stateMachines,
            string stateName)
        {
            foreach (UnityEditor.Animations.AnimatorStateMachine stateMachine in stateMachines)
            {
                foreach (ChildAnimatorState childState in stateMachine.states)
                {
                    if (stateName == null || childState.state.name == stateName)
                    {
                        yield return childState.state;
                    }
                }

                foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
                {
                    foreach (AnimatorState nestedState in FindStates(
                                 new[] { childStateMachine.stateMachine }, stateName))
                    {
                        yield return nestedState;
                    }
                }
            }
        }
    }
}
