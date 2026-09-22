#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SoulsLike.Entities.Enemy;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Configuration
{
    public sealed class EnemyCatalogTests
    {
        private const string CATALOG_PATH = "Assets/Settings/Enemy/EnemyCatalog.asset";

        [Test]
        public void SharedCatalog_PreservesExistingEnemyVariants()
        {
            EnemyCatalog catalog = AssetDatabase.LoadAssetAtPath<EnemyCatalog>(CATALOG_PATH);
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.GetValidationErrors(), Is.Empty);
            Assert.That(catalog.Definitions.Count, Is.EqualTo(3));

            EnemyCatalog.Definition main = catalog.GetDefinition(EnemyId.ErikaMelee);
            EnemyCatalog.Definition backstab = catalog.GetDefinition(EnemyId.BackstabDummy);
            EnemyCatalog.Definition riposte = catalog.GetDefinition(EnemyId.RiposteDummy);
            Assert.That(main.EnemyPrefab, Is.SameAs(backstab.EnemyPrefab));
            Assert.That(backstab.EnemyPrefab, Is.SameAs(riposte.EnemyPrefab));
            Assert.That(main.BehaviourProfile, Is.Not.SameAs(backstab.BehaviourProfile));
            Assert.That(backstab.BehaviourProfile, Is.Not.SameAs(riposte.BehaviourProfile));
            Assert.That(main.Moveset, Is.Not.SameAs(backstab.Moveset));
            Assert.That(backstab.Moveset, Is.Not.SameAs(riposte.Moveset));
            Assert.That(main.HealthData, Is.Not.SameAs(backstab.HealthData));
            Assert.That(backstab.HealthData, Is.SameAs(riposte.HealthData));
            Assert.Throws<InvalidOperationException>(() => catalog.GetDefinition((EnemyId)999));
        }

        [TestCase(EnemyId.Unassigned, "unassigned")]
        [TestCase(EnemyId.ErikaMelee, "more than once")]
        public void InvalidSerializedIds_ProduceAuthoringErrors(EnemyId id, string expectedError)
        {
            EnemyCatalog catalog = ScriptableObject.CreateInstance<EnemyCatalog>();
            try
            {
                var entries = new List<KeyValue<EnemyId, EnemyCatalog.Definition>>();
                int count = id == EnemyId.Unassigned ? 1 : 2;
                for (int index = 0; index < count; index++)
                {
                    var entry = new KeyValue<EnemyId, EnemyCatalog.Definition>();
                    entry.Assign(id, new EnemyCatalog.Definition());
                    entries.Add(entry);
                }

                var serialized = new SerializedDictionary<EnemyId, EnemyCatalog.Definition>();
                typeof(SerializedDictionary<EnemyId, EnemyCatalog.Definition>)
                    .GetField("keyValue", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(serialized, entries);
                typeof(EnemyCatalog)
                    .GetField("definitions", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(catalog, serialized);

                Assert.That(catalog.GetValidationErrors().Any(error => error.Contains(expectedError)), Is.True);
                Assert.Throws<InvalidOperationException>(() => catalog.GetDefinition(id));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(catalog);
            }
        }
    }
}
#endif
