using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SoulsLike.Tests.EnemyRuntime
{
    public sealed class CombatDefenseRecoveryTests
    {
        [Test]
        public void TickRecovery_PoiseDelayBlocksPoiseButNotStance()
        {
            Component defense = CreateDefense(out GameObject gameObject, out ScriptableObject healthData);
            try
            {
                Type defenseType = defense.GetType();
                SetPrivateField(defenseType, defense, "_currentPoise", 60f);
                SetPrivateField(defenseType, defense, "_currentStance", 70f);
                SetPrivateField(defenseType, defense, "_poiseRecoveryDelayRemaining", 1f);

                defenseType.GetMethod("TickRecovery").Invoke(defense, new object[] { 0.5f });

                Assert.That(GetFloatProperty(defenseType, defense, "CurrentPoise"), Is.EqualTo(60f));
                Assert.That(GetFloatProperty(defenseType, defense, "CurrentStance"), Is.EqualTo(75f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
                UnityEngine.Object.DestroyImmediate(healthData);
            }
        }

        [Test]
        public void TickRecovery_PoiseRecoveryStartsOnTheTickAfterDelayExpires()
        {
            Component defense = CreateDefense(out GameObject gameObject, out ScriptableObject healthData);
            try
            {
                Type defenseType = defense.GetType();
                SetPrivateField(defenseType, defense, "_currentPoise", 60f);
                SetPrivateField(defenseType, defense, "_poiseRecoveryDelayRemaining", 0.5f);

                defenseType.GetMethod("TickRecovery").Invoke(defense, new object[] { 0.5f });
                Assert.That(GetFloatProperty(defenseType, defense, "CurrentPoise"), Is.EqualTo(60f));

                defenseType.GetMethod("TickRecovery").Invoke(defense, new object[] { 0.5f });
                Assert.That(GetFloatProperty(defenseType, defense, "CurrentPoise"), Is.EqualTo(72.5f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
                UnityEngine.Object.DestroyImmediate(healthData);
            }
        }

        [Test]
        public void TickRecovery_CriticalOpportunitySuppressesStanceRecovery()
        {
            Component defense = CreateDefense(out GameObject gameObject, out ScriptableObject healthData);
            try
            {
                Type defenseType = defense.GetType();
                SetPrivateField(defenseType, defense, "_currentStance", 70f);
                defenseType.GetMethod("SetCriticalOpportunity").Invoke(defense, new object[] { true });

                defenseType.GetMethod("TickRecovery").Invoke(defense, new object[] { 0.5f });

                Assert.That(GetFloatProperty(defenseType, defense, "CurrentStance"), Is.EqualTo(70f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
                UnityEngine.Object.DestroyImmediate(healthData);
            }
        }

        private static Component CreateDefense(out GameObject gameObject, out ScriptableObject healthData)
        {
            Type defenseType = GetRequiredType("SoulsLike.Entities.Combat.CombatDefenseComponent");
            Type healthType = GetRequiredType("SoulsLike.Entities.Character.Components.Health.HealthComponent");
            Type healthDataType = GetRequiredType("SoulsLike.Entities.Character.Components.Health.HealthData");
            Type healthModelType = GetRequiredType("SoulsLike.Entities.Character.Components.Health.HealthModel");

            gameObject = new GameObject("CombatDefenseRecoveryTest");
            Component health = gameObject.AddComponent(healthType);
            healthData = ScriptableObject.CreateInstance(healthDataType);
            object healthModel = Activator.CreateInstance(healthModelType, new object[] { healthData });
            healthType.GetProperty("Model").SetValue(health, healthModel);

            Component defense = gameObject.AddComponent(defenseType);
            SetPrivateField(defenseType, defense, "_health", health);
            return defense;
        }

        private static float GetFloatProperty(Type type, object instance, string name) =>
            (float)type.GetProperty(name).GetValue(instance);

        private static void SetPrivateField(Type type, object instance, string name, object value) =>
            type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value);

        private static Type GetRequiredType(string typeName) =>
            Type.GetType($"{typeName}, Assembly-CSharp")
            ?? throw new InvalidOperationException($"Type '{typeName}' was not loaded.");
    }
}
