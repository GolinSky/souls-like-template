using NUnit.Framework;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Ladder;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class LadderAnimationTests
    {
        [Test]
        public void AnimatorComponent_Implements_ILadderAnimator()
        {
            Assert.That(typeof(ILadderAnimator).IsAssignableFrom(typeof(AnimatorComponent)), Is.True);
        }

        [Test]
        public void CharacterPrefab_LadderClimber_HasAnimatorComponentAssigned()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Models/Character/Character.prefab");
            Assert.That(prefab, Is.Not.Null);

            LadderClimber climber = prefab.GetComponent<LadderClimber>();
            Assert.That(climber, Is.Not.Null);

            var so = new SerializedObject(climber);
            SerializedProperty animCompProp = so.FindProperty("animatorComponent");
            Assert.That(animCompProp, Is.Not.Null);
            Assert.That(animCompProp.objectReferenceValue, Is.Not.Null);
            Assert.That(animCompProp.objectReferenceValue, Is.TypeOf<AnimatorComponent>());
        }

        [TestCase("Assets/Art/Animation/CharacterGreatSwordAnimator.controller")]
        [TestCase("Assets/Art/Animation/Enemy/ErikaGreatSwordEnemy.controller")]
        public void Controller_ContainsRequiredLadderParameters(string controllerPath)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            Assert.That(controller, Is.Not.Null);

            string[] expectedParams =
            {
                "IsOnLadder",
                "LadderClimbSpeed",
                "LadderSlide",
                "LadderIdle",
                "LadderEnterBottom",
                "LadderEnterTop",
                "LadderExitBottom",
                "LadderExitTop",
                "LadderPunch",
                "LadderKick",
                "LadderDrink",
                "LadderUnlock"
            };

            foreach (string paramName in expectedParams)
            {
                bool exists = System.Array.Exists(controller.parameters, p => p.name == paramName);
                Assert.That(exists, Is.True, $"Controller '{controllerPath}' is missing parameter '{paramName}'");
            }
        }

        [TestCase("Assets/Art/Animation/CharacterGreatSwordAnimator.controller")]
        [TestCase("Assets/Art/Animation/Enemy/ErikaGreatSwordEnemy.controller")]
        public void Controller_LadderLayer_ContainsTransitions(string controllerPath)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            Assert.That(controller, Is.Not.Null);

            AnimatorControllerLayer ladderLayer = System.Array.Find(controller.layers, l => l.name == "Ladder");
            Assert.That(ladderLayer, Is.Not.Null);

            AnimatorStateMachine rootSm = ladderLayer.stateMachine;
            Assert.That(rootSm.anyStateTransitions.Length, Is.GreaterThanOrEqualTo(10),
                $"Ladder root state machine in '{controllerPath}' should have at least 10 AnyState transitions");

            AnimatorStateMachine subSm = rootSm.stateMachines[0].stateMachine;
            Assert.That(subSm, Is.Not.Null);

            int subStateTransitionCount = 0;
            foreach (ChildAnimatorState childState in subSm.states)
            {
                subStateTransitionCount += childState.state.transitions.Length;
            }

            Assert.That(subStateTransitionCount, Is.GreaterThanOrEqualTo(12),
                $"Ladder sub-state machine in '{controllerPath}' should contain locomotion transitions between idle, climb, and slide");
        }
    }
}
