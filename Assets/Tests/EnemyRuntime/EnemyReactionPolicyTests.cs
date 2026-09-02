using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class EnemyReactionPolicyTests
    {
        [TestCase("Hit", "None")]
        [TestCase("HitFromBack", "None")]
        [TestCase("Blocked", "Authored")]
        [TestCase("PoiseStaggered", "Forced")]
        [TestCase("StanceBroken", "Forced")]
        [TestCase("GuardBroken", "Forced")]
        [TestCase("Parried", "None")]
        [TestCase("Killed", "None")]
        public void DefenderResultsUseTheExpectedReactionAuthority(
            string resultTypeName,
            string expectedReaction)
        {
            Type controllerType = GetRequiredType(
                "SoulsLike.Entities.Enemy.EnemyActionExecutor");
            Type resultType = GetRequiredType(
                "SoulsLike.Entities.Combat.MeleeHitResultType");
            MethodInfo method = controllerType.GetMethod(
                "ResolveDefenderReaction",
                BindingFlags.Static | BindingFlags.NonPublic);

            object parsedResultType = Enum.Parse(resultType, resultTypeName);
            object reaction = method.Invoke(null, new[] { parsedResultType });

            Assert.That(reaction.ToString(), Is.EqualTo(expectedReaction));
        }

        [Test]
        public void UninterruptibleHyperArmorRejectsPoiseStagger()
        {
            Type defenseType = GetRequiredType(
                "SoulsLike.Entities.Combat.CombatDefenseComponent");
            var gameObject = new GameObject("Defense");
            try
            {
                Component defense = gameObject.AddComponent(defenseType);
                defenseType.GetMethod("SetHyperArmor").Invoke(
                    defense,
                    new object[] { true, 0f, false });

                bool staggered = (bool)defenseType.GetMethod("ApplyPoiseDamage").Invoke(
                    defense,
                    new object[] { float.MaxValue });

                Assert.That(staggered, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void AttackerParryRemainsAForcedResponse()
        {
            Type controllerType = GetRequiredType(
                "SoulsLike.Entities.Enemy.EnemyActionExecutor");
            Type resultType = GetRequiredType(
                "SoulsLike.Entities.Combat.MeleeHitResultType");
            MethodInfo method = controllerType.GetMethod(
                "IsAttackerParried",
                BindingFlags.Static | BindingFlags.NonPublic);

            bool isParried = (bool)method.Invoke(
                null,
                new[] { Enum.Parse(resultType, "Parried") });

            Assert.That(isParried, Is.True);
        }

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
