using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using AnimatorStateMachineBehaviour = SoulsLike.Entities.Character.Components.Animations.AnimatorStateMachine;
using StateMachineName = SoulsLike.Entities.Character.Components.Animations.StateMachineName;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class CharacterArrivalAnimationContractTests
    {
        private const string CONTROLLER_PATH = "Assets/Art/Animation/CharacterGreatSwordAnimator.controller";
        private const string ONE_HANDED_LAYER = "OneHandedLayer";
        private const string TWO_HANDED_LAYER = "TwoHandedLayer";

        [Test]
        public void CharacterGreatSwordAnimator_ArrivalStatesHaveRequiredCallbacksAndExitPaths()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);

            Assert.That(controller, Is.Not.Null, $"Missing animator controller at {CONTROLLER_PATH}.");
            Assert.That(controller.parameters.Any(parameter => parameter.name == "Spawn"
                && parameter.type == UnityEngine.AnimatorControllerParameterType.Trigger), Is.True,
                "The arrival contract requires a Spawn trigger.");

            int oneHandedLayerIndex = Array.FindIndex(
                controller.layers,
                layer => layer.name == ONE_HANDED_LAYER);
            int twoHandedLayerIndex = Array.FindIndex(
                controller.layers,
                layer => layer.name == TWO_HANDED_LAYER);
            Assert.That(oneHandedLayerIndex, Is.GreaterThanOrEqualTo(0),
                $"Missing required layer '{ONE_HANDED_LAYER}'.");
            Assert.That(twoHandedLayerIndex, Is.GreaterThanOrEqualTo(0),
                $"Missing required layer '{TWO_HANDED_LAYER}'.");

            AnimatorControllerLayer oneHandedLayer = controller.layers[oneHandedLayerIndex];
            AnimatorControllerLayer twoHandedLayer = controller.layers[twoHandedLayerIndex];
            Assert.That(twoHandedLayer.syncedLayerIndex, Is.EqualTo(oneHandedLayerIndex),
                "TwoHandedLayer must remain synchronized to the arrival callback owner.");

            AnimatorState spawnState = FindStateWithCallback(
                oneHandedLayer.stateMachine,
                StateMachineName.Spawn);
            Assert.That(spawnState, Is.Not.Null,
                "The arrival contract requires a state that reports Spawn callbacks.");
            AssertAuthoredContinuation(spawnState);

            Assert.That(FindState(oneHandedLayer.stateMachine, "GraceRestIdle"), Is.Not.Null,
                "The direct grace arrival route requires a GraceRestIdle state.");
        }

        private static void AssertAuthoredContinuation(AnimatorState state)
        {
            Assert.That(state.transitions.Any(transition => transition.isExit
                || transition.destinationState != null
                || transition.destinationStateMachine != null), Is.True,
                $"State '{state.name}' must have an authored continuation. "
                + "Runtime trace coverage validates its Exit callback.");
        }

        private static bool HasStateMachineName(
            AnimatorStateMachineBehaviour behaviour,
            StateMachineName expectedStateMachineName)
        {
            var serializedBehaviour = new SerializedObject(behaviour);
            return serializedBehaviour.FindProperty("stateMachineName").intValue
                == (int)expectedStateMachineName;
        }

        private static AnimatorState FindState(
            UnityEditor.Animations.AnimatorStateMachine stateMachine,
            string stateName)
        {
            AnimatorState directState = stateMachine.states
                .Select(childState => childState.state)
                .FirstOrDefault(state => state.name == stateName);
            if (directState != null)
            {
                return directState;
            }

            foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
            {
                AnimatorState nestedState = FindState(childStateMachine.stateMachine, stateName);
                if (nestedState != null)
                {
                    return nestedState;
                }
            }

            return null;
        }

        private static AnimatorState FindStateWithCallback(
            UnityEditor.Animations.AnimatorStateMachine stateMachine,
            StateMachineName stateMachineName)
        {
            AnimatorState directState = stateMachine.states
                .Select(childState => childState.state)
                .FirstOrDefault(state => state.behaviours.OfType<AnimatorStateMachineBehaviour>()
                        .Any(behaviour => HasStateMachineName(behaviour, stateMachineName)));
            if (directState != null)
            {
                return directState;
            }

            foreach (ChildAnimatorStateMachine childStateMachine in stateMachine.stateMachines)
            {
                AnimatorState nestedState = FindStateWithCallback(
                    childStateMachine.stateMachine,
                    stateMachineName);
                if (nestedState != null)
                {
                    return nestedState;
                }
            }

            return null;
        }
    }
}
