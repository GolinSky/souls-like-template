using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class EnemyMultiHitActionTests
    {
        [Test]
        public void CharacterActionDefinition_GetHitDefinition_ReturnsFallbackWhenEmpty()
        {
            Type actionDefType = GetRequiredType("SoulsLike.Entities.Combat.CharacterActionDefinition");
            ScriptableObject action = ScriptableObject.CreateInstance(actionDefType);
            try
            {
                FieldInfo damageMultiplierField = actionDefType.GetField(
                    "damageMultiplier",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                damageMultiplierField.SetValue(action, 1.5f);

                MethodInfo getHitDefMethod = actionDefType.GetMethod("GetHitDefinition");
                object hitDef = getHitDefMethod.Invoke(action, new object[] { 0 });

                Type hitDefType = hitDef.GetType();
                PropertyInfo damageMultProp = hitDefType.GetProperty("DamageMultiplier");
                Assert.That((float)damageMultProp.GetValue(hitDef), Is.EqualTo(1.5f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(action);
            }
        }

        [Test]
        public void CharacterActionDefinition_GetHitDefinition_ReturnsIndexedDefinitionWhenPopulated()
        {
            Type actionDefType = GetRequiredType("SoulsLike.Entities.Combat.CharacterActionDefinition");
            Type hitDefType = GetRequiredType("SoulsLike.Entities.Combat.CharacterActionHitDefinition");
            ScriptableObject action = ScriptableObject.CreateInstance(actionDefType);
            try
            {
                Array hitDefsArray = Array.CreateInstance(hitDefType, 2);

                ConstructorInfo ctor = hitDefType.GetConstructor(new[]
                {
                    typeof(float), typeof(float), typeof(float), typeof(float),
                    GetRequiredType("SoulsLike.Entities.Combat.ImpactLevel"),
                    typeof(bool), typeof(bool)
                });

                Type impactType = GetRequiredType("SoulsLike.Entities.Combat.ImpactLevel");
                object lightImpact = Enum.Parse(impactType, "Light");
                object mediumImpact = Enum.Parse(impactType, "Medium");

                object hit0 = ctor.Invoke(new object[] { 0.6f, 10f, 12f, 8f, lightImpact, true, true });
                object hit1 = ctor.Invoke(new object[] { 1.4f, 30f, 35f, 20f, mediumImpact, true, true });

                hitDefsArray.SetValue(hit0, 0);
                hitDefsArray.SetValue(hit1, 1);

                FieldInfo hitDefsField = actionDefType.GetField(
                    "hitDefinitions",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                hitDefsField.SetValue(action, hitDefsArray);

                MethodInfo getHitDefMethod = actionDefType.GetMethod("GetHitDefinition");

                object res0 = getHitDefMethod.Invoke(action, new object[] { 0 });
                object res1 = getHitDefMethod.Invoke(action, new object[] { 1 });

                PropertyInfo dmgProp = hitDefType.GetProperty("DamageMultiplier");
                PropertyInfo impactProp = hitDefType.GetProperty("ImpactLevel");

                Assert.That((float)dmgProp.GetValue(res0), Is.EqualTo(0.6f));
                Assert.That((float)dmgProp.GetValue(res1), Is.EqualTo(1.4f));
                Assert.That(impactProp.GetValue(res0), Is.EqualTo(lightImpact));
                Assert.That(impactProp.GetValue(res1), Is.EqualTo(mediumImpact));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(action);
            }
        }

        [Test]
        public void Executor_ExposesIndexedReportActiveStarted()
        {
            Type executorType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyActionExecutor");
            Type actionIdType = GetRequiredType("SoulsLike.Entities.Combat.CharacterActionId");

            MethodInfo singleParamMethod = executorType.GetMethod(
                "ReportActiveStarted",
                new[] { actionIdType });
            Assert.That(singleParamMethod, Is.Not.Null, "Expected single-parameter ReportActiveStarted overload.");

            MethodInfo indexedMethod = executorType.GetMethod(
                "ReportActiveStarted",
                new[] { actionIdType, typeof(int) });
            Assert.That(indexedMethod, Is.Not.Null, "Expected indexed ReportActiveStarted(CharacterActionId, int) overload.");
        }

        [Test]
        public void MultiHitActionStateBehaviour_ExposesHitWindows()
        {
            Type multiHitType = GetRequiredType("SoulsLike.Entities.Enemy.EnemyMultiHitActionStateBehaviour");
            Assert.That(multiHitType, Is.Not.Null);

            Type hitWindowType = multiHitType.GetNestedType("HitWindow");
            Assert.That(hitWindowType, Is.Not.Null);

            Assert.That(hitWindowType.GetProperty("HitIndex"), Is.Not.Null);
            Assert.That(hitWindowType.GetProperty("ActiveStart"), Is.Not.Null);
            Assert.That(hitWindowType.GetProperty("ActiveEnd"), Is.Not.Null);
            Assert.That(hitWindowType.GetProperty("HasTrackingWindow"), Is.Not.Null);
            Assert.That(hitWindowType.GetProperty("TrackingStart"), Is.Not.Null);
            Assert.That(hitWindowType.GetProperty("TrackingEnd"), Is.Not.Null);
        }

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
