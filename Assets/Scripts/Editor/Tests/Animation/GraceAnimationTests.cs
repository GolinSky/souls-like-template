using NUnit.Framework;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Equipment;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class GraceAnimationTests
    {
        private const string CHARACTER_PREFAB_PATH = "Assets/Prefabs/Models/Character/Character.prefab";
        private const string ONE_HANDED_LAYER = "OneHandedLayer";
        private const string TWO_HANDED_LAYER = "TwoHandedLayer";
        private static readonly int GraceRestIdleState = Animator.StringToHash("GraceRestIdle");
        private static readonly int GraceRestEndState = Animator.StringToHash("GraceRestEnd");
        private static readonly int FreeLocomotionState = Animator.StringToHash("FreeLocomotion");

        [TestCase(HandMode.OneHanded, ONE_HANDED_LAYER)]
        [TestCase(HandMode.TwoHanded, TWO_HANDED_LAYER)]
        public void AnimatorComponent_GraceRest_DirectlyEntersIdleThenReturnsToLocomotion(
            HandMode handMode,
            string layerName)
        {
            AnimatorComponent component = CreateAnimatorComponent(out GameObject gameObject, out Animator animator);

            try
            {
                int layerIndex = animator.GetLayerIndex(layerName);
                component.SetHandMode(handMode);
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
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                CHARACTER_PREFAB_PATH);

            Assert.That(prefab, Is.Not.Null, $"Missing character prefab at {CHARACTER_PREFAB_PATH}.");
            gameObject = Object.Instantiate(prefab);
            AnimatorComponent component = gameObject.GetComponent<AnimatorComponent>();
            animator = gameObject.GetComponentInChildren<Animator>(true);
            AnimatorStateMachineReceiver receiver = gameObject.GetComponentInChildren<AnimatorStateMachineReceiver>(true);
            Assert.That(component, Is.Not.Null);
            Assert.That(animator, Is.Not.Null);
            Assert.That(receiver, Is.Not.Null);
            receiver.InitializeStateMachines();
            return component;
        }
    }
}
