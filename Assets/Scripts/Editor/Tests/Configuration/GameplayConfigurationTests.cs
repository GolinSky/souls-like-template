#if UNITY_EDITOR
using System;
using NUnit.Framework;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Character.Components.Movement;
using SoulsLike.Entities.Character.Components.Targeting;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Enemy;
using SoulsLike.Entities.Ladder;
using UnityEditor;
using UnityEngine;

namespace SoulsLike.Editor.Tests.Configuration
{
    public sealed class GameplayConfigurationTests
    {
        [Test]
        public void MovementDataAssets_HaveGroundProbeMasks()
        {
            string[] guids = AssetDatabase.FindAssets("t:MovementData", new[] { "Assets" });
            Assert.That(guids, Is.Not.Empty);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<MovementData>(path);
                Assert.That(data.GroundProbeMask.value, Is.Not.Zero, path);
            }
        }

        [Test]
        public void EnemyBehaviourProfiles_HaveLineOfSightMasks()
        {
            string[] guids = AssetDatabase.FindAssets("t:EnemyBehaviourProfile", new[] { "Assets" });
            Assert.That(guids, Is.Not.Empty);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var profile = AssetDatabase.LoadAssetAtPath<EnemyBehaviourProfile>(path);
                Assert.That(profile.LineOfSightMask.value, Is.Not.Zero, path);
            }
        }

        [TestCase(typeof(EquipmentPresentation))]
        [TestCase(typeof(WeaponRuntime))]
        [TestCase(typeof(AnimatorRootMotionRelay))]
        [TestCase(typeof(EnemyActor))]
        public void GameplayPrefabs_HaveRequiredConfiguration(Type componentType)
        {
            int checkedCount = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Models" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                foreach (Component component in prefab.GetComponentsInChildren(componentType, true))
                {
                    string context = $"{path}: '{component.name}'";
                    switch (component)
                    {
                        case EquipmentPresentation presentation:
                            using (var serialized = new SerializedObject(presentation))
                            {
                                foreach (string property in new[] { "rightHandAnchor", "leftHandAnchor", "rightFistRuntime" })
                                {
                                    Assert.That(serialized.FindProperty(property).objectReferenceValue, Is.Not.Null,
                                        $"{context} requires '{property}'.");
                                }
                            }
                            break;
                        case WeaponRuntime weapon:
                            Assert.That(weapon.MeleeHitbox, Is.Not.Null, $"{context} requires a melee hitbox.");
                            break;
                        case AnimatorRootMotionRelay relay:
                            Assert.That(relay.GetComponent<Animator>(), Is.Not.Null, $"{context} requires an Animator.");
                            break;
                        case EnemyActor enemy:
                            Assert.That(enemy.GetComponentsInChildren<EnemyActivationTrigger>(true).Length,
                                Is.LessThanOrEqualTo(1), $"{context} may have at most one activation trigger.");
                            break;
                    }
                    checkedCount++;
                }
            }
            Assert.That(checkedCount, Is.GreaterThan(0), $"No {componentType.Name} prefab components were checked.");
        }

        [Test]
        public void CharacterPrefab_HasFactoryComponents()
        {
            const string PATH = "Assets/Prefabs/Models/Character/Character.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PATH);
            Assert.That(prefab, Is.Not.Null, PATH);
            Type[] rootTypes =
            {
                typeof(Character), typeof(PlayerMeleeCombatRelay), typeof(CriticalAttackController),
                typeof(AnimatorComponent), typeof(AttackComponent), typeof(MovementComponent),
                typeof(EquipmentComponent), typeof(EquipmentPresentation), typeof(InventoryComponent),
                typeof(HealthComponent), typeof(CombatDefenseComponent), typeof(LadderClimber)
            };
            foreach (Type type in rootTypes)
            {
                Assert.That(prefab.GetComponent(type), Is.Not.Null, $"{PATH}: requires {type.Name} on the root.");
            }
            Assert.That(prefab.GetComponentInChildren<TargetLockComponent>(true), Is.Not.Null, PATH);
            Assert.That(prefab.GetComponentInChildren<CharacterAudioComponent>(true), Is.Not.Null, PATH);
        }
    }
}
#endif
