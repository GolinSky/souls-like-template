#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using NUnit.Framework;
using SoulsLike.Entities.Enemy;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Ui
{
    public sealed class CheatsEnemySpawnTests
    {
        private const string CATALOG_PATH = "Assets/Settings/Enemy/EnemyCatalog.asset";

        [Test]
        public void AvailableEnemyIds_FromCatalog_ExcludesUnassignedAndContainsConfiguredIds()
        {
            EnemyCatalog catalog = AssetDatabase.LoadAssetAtPath<EnemyCatalog>(CATALOG_PATH);
            Assert.That(catalog, Is.Not.Null);

            List<EnemyId> ids = new();
            foreach (var entry in catalog.Definitions)
            {
                if (entry != null && entry.Key != EnemyId.Unassigned && !ids.Contains(entry.Key))
                {
                    ids.Add(entry.Key);
                }
            }

            Assert.That(ids, Contains.Item(EnemyId.ErikaMelee));
            Assert.That(ids, Contains.Item(EnemyId.BackstabDummy));
            Assert.That(ids, Contains.Item(EnemyId.RiposteDummy));
            Assert.That(ids, Has.No.Member(EnemyId.Unassigned));
        }

        [TestCase("ErikaMelee", EnemyId.ErikaMelee)]
        [TestCase("erikamelee", EnemyId.ErikaMelee)]
        [TestCase("BackstabDummy", EnemyId.BackstabDummy)]
        [TestCase("RiposteDummy", EnemyId.RiposteDummy)]
        [TestCase("1", EnemyId.ErikaMelee)]
        [TestCase("2", EnemyId.BackstabDummy)]
        [TestCase("3", EnemyId.RiposteDummy)]
        public void EnemyTypeParsing_ResolvesExpectedEnemyId(string input, EnemyId expectedId)
        {
            bool parsed = Enum.TryParse(input, true, out EnemyId result);
            if (!parsed && int.TryParse(input, out int rawId) && Enum.IsDefined(typeof(EnemyId), (EnemyId)rawId))
            {
                result = (EnemyId)rawId;
                parsed = true;
            }

            Assert.That(parsed, Is.True);
            Assert.That(result, Is.EqualTo(expectedId));
        }

        [Test]
        public void EnemySpawnCalculation_PlacesEnemyInFrontFacingPlayer()
        {
            Vector3 playerPosition = new Vector3(10f, 0f, 10f);
            Vector3 playerForward = Vector3.forward;
            float spawnDistance = 3.0f;

            Vector3 forward = playerForward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 spawnPosition = playerPosition + forward * spawnDistance;
            Quaternion spawnRotation = Quaternion.LookRotation(-forward, Vector3.up);

            Assert.That(spawnPosition, Is.EqualTo(new Vector3(10f, 0f, 13f)));
            // Enemy facing backward toward the player
            Vector3 enemyFacing = spawnRotation * Vector3.forward;
            Assert.That(Vector3.Dot(enemyFacing, Vector3.back), Is.EqualTo(1.0f).Within(0.0001f));
        }
    }
}
#endif
