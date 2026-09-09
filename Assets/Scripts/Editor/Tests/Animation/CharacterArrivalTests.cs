using System.Reflection;
using NUnit.Framework;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Animations;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Movement;
using SoulsLike.Entities.Combat;
using SoulsLike.Items;
using UnityEditor;
using UnityEngine;
using StateMachineName = SoulsLike.Entities.Character.Components.Animations.StateMachineName;

namespace SoulsLike.Editor.Tests.Animation
{
    public sealed class CharacterArrivalTests
    {
        private const string CHARACTER_PREFAB_PATH = "Assets/Prefabs/Models/Character/Character.prefab";
        private const string CHARACTER_DATA_PATH = "Assets/Settings/Data/CharacterData.asset";
        private const string HEALTH_DATA_PATH = "Assets/Settings/Data/HealthData.asset";
        private const string MOVEMENT_DATA_PATH = "Assets/Settings/Player/MovementData.asset";
        private const string ITEM_DATABASE_PATH = "Assets/Settings/Items/ItemDatabase.asset";
        private const string WEAPON_DATABASE_PATH = "Assets/Settings/Items/WeaponDatabase.asset";
        private const string SHIELD_DATABASE_PATH = "Assets/Settings/Items/ShieldDatabase.asset";
        private const string CONSUMABLE_DATABASE_PATH = "Assets/Settings/Items/ConsumableDatabase.asset";
        private const string ONE_HANDED_LAYER = "OneHandedLayer";

        private GameObject _instance;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_instance);
        }

        [Test]
        public void BeginArrival_WorldPosition_StaysBlockedUntilSpawnExit()
        {
            Character character = CreateInitializedCharacter(out _);

            Assert.That(character.IsInputBlocked, Is.True);

            character.BeginArrival(CharacterArrival.WorldPosition);

            Assert.That(character.IsInputBlocked, Is.True);

            character.OnAnimationStateChanged(new AnimatorStateMachineDto
            {
                StateMachineName = StateMachineName.Spawn,
                State = StateMachineState.Enter
            });
            Assert.That(character.IsInputBlocked, Is.True);

            character.OnAnimationStateChanged(new AnimatorStateMachineDto
            {
                StateMachineName = StateMachineName.Spawn,
                State = StateMachineState.Exit
            });
            Assert.That(character.IsInputBlocked, Is.False);
        }

        [Test]
        public void BeginArrival_GraceRest_EntersGraceWithoutSpawnAndKeepsProtection()
        {
            Character character = CreateInitializedCharacter(out Animator animator);

            Assert.That(character.IsInputBlocked, Is.True);

            character.BeginArrival(CharacterArrival.GraceRest);
            animator.Update(0.0f);

            Assert.That(IsStateActive(animator, Animator.StringToHash("GraceRestIdle")), Is.True);
            Assert.That(character.IsInputBlocked, Is.True);
            Assert.That(_instance.GetComponent<HealthComponent>().IsInvulnerable, Is.True);

            character.OnAnimationStateChanged(new AnimatorStateMachineDto
            {
                StateMachineName = StateMachineName.Spawn,
                State = StateMachineState.Exit
            });

            Assert.That(character.IsInputBlocked, Is.True);
            Assert.That(_instance.GetComponent<HealthComponent>().IsInvulnerable, Is.True);
        }

        private Character CreateInitializedCharacter(out Animator animator)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CHARACTER_PREFAB_PATH);
            Assert.That(prefab, Is.Not.Null, $"Missing character prefab at {CHARACTER_PREFAB_PATH}.");

            _instance = Object.Instantiate(prefab);
            Character character = _instance.GetComponent<Character>();
            AnimatorComponent animatorComponent = _instance.GetComponent<AnimatorComponent>();
            MovementComponent movement = _instance.GetComponent<MovementComponent>();
            HealthComponent health = _instance.GetComponent<HealthComponent>();
            EquipmentComponent equipment = _instance.GetComponent<EquipmentComponent>();
            EquipmentPresentation presentation = _instance.GetComponent<EquipmentPresentation>();
            AttackComponent attack = _instance.GetComponent<AttackComponent>();
            CombatDefenseComponent combatDefense = _instance.GetComponent<CombatDefenseComponent>();
            CriticalAttackController criticalAttack = _instance.GetComponent<CriticalAttackController>();
            animator = _instance.GetComponentInChildren<Animator>();
            Assert.That(character, Is.Not.Null);
            Assert.That(animator, Is.Not.Null);

            ItemCatalog itemCatalog = CreateItemCatalog();
            movement.Model = new MovementModel(AssetDatabase.LoadAssetAtPath<MovementData>(MOVEMENT_DATA_PATH));
            health.Model = new HealthModel(AssetDatabase.LoadAssetAtPath<HealthData>(HEALTH_DATA_PATH));
            equipment.Model = new EquipmentModel();
            attack.InjectDependencies(itemCatalog);
            presentation.InjectDependencies(itemCatalog);
            SetPrivateField(character, "_attackComponent", attack);
            SetPrivateField(character, "_itemCatalog", itemCatalog);
            SetPrivateField(character, "_characterData",
                AssetDatabase.LoadAssetAtPath<CharacterData>(CHARACTER_DATA_PATH));
            SetPrivateField(character, "_combatDefense", combatDefense);
            SetPrivateField(character, "_criticalAttackController", criticalAttack);

            animatorComponent.ConfigureCharacter(character, movement);
            character.Initialize();
            return character;
        }

        private static ItemCatalog CreateItemCatalog()
        {
            return new ItemCatalog(
                AssetDatabase.LoadAssetAtPath<ItemDatabase>(ITEM_DATABASE_PATH),
                AssetDatabase.LoadAssetAtPath<WeaponDatabase>(WEAPON_DATABASE_PATH),
                AssetDatabase.LoadAssetAtPath<ShieldDatabase>(SHIELD_DATABASE_PATH),
                AssetDatabase.LoadAssetAtPath<ConsumableDatabase>(CONSUMABLE_DATABASE_PATH));
        }

        private static bool IsStateActive(Animator animator, int stateHash)
        {
            int layerIndex = animator.GetLayerIndex(ONE_HANDED_LAYER);
            return animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == stateHash
                || animator.GetNextAnimatorStateInfo(layerIndex).shortNameHash == stateHash;
        }

        private static void SetPrivateField(Character character, string fieldName, object value)
        {
            FieldInfo field = typeof(Character).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing Character field '{fieldName}'.");
            field.SetValue(character, value);
        }
    }
}
