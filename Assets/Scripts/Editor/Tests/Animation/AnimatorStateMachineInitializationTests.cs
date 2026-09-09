using NUnit.Framework;
using SoulsLike.Entities.Character.Components.Animations;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class AnimatorStateMachineInitializationTests
    {
        [Test]
        public void OnStateEnter_WithoutReceiver_FailsVisibly()
        {
            var behaviour = ScriptableObject.CreateInstance<AnimatorStateMachine>();

            try
            {
                Assert.That(
                    () => behaviour.OnStateEnter(null, default, 0),
                    Throws.TypeOf<System.NullReferenceException>());
            }
            finally
            {
                Object.DestroyImmediate(behaviour);
            }
        }
    }
}
