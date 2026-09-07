using NUnit.Framework;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Equipment;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class GraceAnimationTests
    {
        private const string CONTROLLER_PATH = "Assets/Art/Animation/CharacterGreatSwordAnimator.controller";
        private const string ONE_HANDED_LAYER = "OneHandedLayer";
        private const string TWO_HANDED_LAYER = "TwoHandedLayer";
        private static readonly int GraceRestIdleState = Animator.StringToHash("GraceRestIdle");
        private static readonly int GraceRestEndState = Animator.StringToHash("GraceRestEnd");
        private static readonly int FreeLocomotionState = Animator.StringToHash("FreeLocomotion");

        [TestCase(false, HandMode.OneHanded, ONE_HANDED_LAYER)]
        [TestCase(false, HandMode.TwoHanded, TWO_HANDED_LAYER)]
        [TestCase(true, HandMode.OneHanded, ONE_HANDED_LAYER)]
        [TestCase(true, HandMode.TwoHanded, TWO_HANDED_LAYER)]
        public void AnimatorComponent_GraceRest_EntersIdleThenReturnsToLocomotion(
            bool hasPendingSpawnTrigger,
            HandMode handMode,
            string layerName)
        {
            AnimatorComponent component = CreateAnimatorComponent(out GameObject gameObject, out Animator animator);

            try
            {
                int layerIndex = animator.GetLayerIndex(layerName);
                component.SetHandMode(handMode);
                if (hasPendingSpawnTrigger)
                {
                    component.TriggerSpawn();
                }

                component.EnterGraceRestIdle();
                animator.Update(0.0f);

                Assert.That(animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash,
                    Is.EqualTo(GraceRestIdleState));

                animator.Update(0.1f);
                animator.Update(0.1f);

                Assert.That(animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash,
                    Is.EqualTo(GraceRestIdleState));

                component.TriggerGraceRestEnd();
                animator.Update(0.1f);

                Assert.That(animator.GetNextAnimatorStateInfo(layerIndex).shortNameHash,
                    Is.EqualTo(GraceRestEndState));

                animator.Update(0.1f);

                Assert.That(animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash,
                    Is.EqualTo(GraceRestEndState));

                for (int i = 0; i < 100; i++)
                {
                    animator.Update(0.1f);
                }

                Assert.That(animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash,
                    Is.EqualTo(FreeLocomotionState));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        private static AnimatorComponent CreateAnimatorComponent(
            out GameObject gameObject,
            out Animator animator)
        {
            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(CONTROLLER_PATH);
            Assert.That(controller, Is.Not.Null);

            gameObject = new GameObject("Grace Animation Test");
            animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            AnimatorComponent component = gameObject.AddComponent<AnimatorComponent>();
            var serializedComponent = new SerializedObject(component);
            serializedComponent.FindProperty("animator").objectReferenceValue = animator;
            serializedComponent.ApplyModifiedPropertiesWithoutUndo();
            return component;
        }
    }
}
