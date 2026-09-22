using NUnit.Framework;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Equipment;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class DeathAnimationTests
    {
        private const string CHARACTER_PREFAB_PATH = "Assets/Prefabs/Models/Character/Character.prefab";
        private const string ONE_HANDED_LAYER = "OneHandedLayer";
        private const string TWO_HANDED_LAYER = "TwoHandedLayer";
        private static readonly int DeathIdleState = Animator.StringToHash("DeathIdle");
        private static readonly int FreeLocomotionState = Animator.StringToHash("FreeLocomotion");
        private static readonly int DeathTrigger = Animator.StringToHash("Death");

        [TestCase(HandMode.OneHanded, ONE_HANDED_LAYER)]
        [TestCase(HandMode.TwoHanded, TWO_HANDED_LAYER)]
        public void AnimatorComponent_CompleteDeathAnimation_WhenInDeathIdle_TransitionsToFreeLocomotion(
            HandMode handMode,
            string layerName)
        {
            AnimatorComponent component = CreateAnimatorComponent(out GameObject gameObject, out Animator animator);

            try
            {
                int targetLayerIndex = animator.GetLayerIndex(layerName);
                int masterLayerIndex = animator.GetLayerIndex(ONE_HANDED_LAYER);
                component.SetHandMode(handMode);
                animator.Play(DeathIdleState, masterLayerIndex, 0f);
                animator.Update(0.1f);

                Assert.That(animator.GetCurrentAnimatorStateInfo(targetLayerIndex).shortNameHash,
                    Is.EqualTo(DeathIdleState),
                    $"Expected animator to be in DeathIdle state on {layerName}.");

                component.CompleteDeathAnimation();
                animator.Update(0.0f);

                Assert.That(animator.GetCurrentAnimatorStateInfo(targetLayerIndex).shortNameHash,
                    Is.EqualTo(FreeLocomotionState),
                    $"Expected animator to transition to FreeLocomotion on {layerName} after CompleteDeathAnimation.");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void AnimatorComponent_CompleteDeathAnimation_ResetsDeathTrigger()
        {
            AnimatorComponent component = CreateAnimatorComponent(out GameObject gameObject, out Animator animator);

            try
            {
                animator.SetTrigger(DeathTrigger);
                Assert.That(animator.GetBool(DeathTrigger), Is.True, "Death trigger should be set.");

                component.CompleteDeathAnimation();

                Assert.That(animator.GetBool(DeathTrigger), Is.False, "CompleteDeathAnimation should reset Death trigger.");
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
