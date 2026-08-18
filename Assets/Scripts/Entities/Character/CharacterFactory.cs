using System;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Character.Components.Movement;
using SoulsLike.Entities.Character.Adapters;
using SoulsLike.Entities.Character.Input;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Extensions;
using SoulsLike.Factory;
using SoulsLike.Ui.LockOn;
using SoulsLike.Ui.PlayerHud;
using SoulsLike.Items;
using SoulsLike.Ui.Inventory;
using SoulsLike.Ui.Equipment;
using SoulsLike.Ui.PauseNavigation;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    public class CharacterFactory : BaseFactory
    {
        private const string CHARACTER_PREFAB_KEY = nameof(Character);

        public CharacterFactory(IObjectResolver resolver) : base(resolver)
        {
        }

        public Character CreateCharacter()
        {
            GameObject prefab = AssetService.LoadPrefab(CHARACTER_PREFAB_KEY);
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Character prefab for Addressables key '{CHARACTER_PREFAB_KEY}' was not found.");
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            instance.name = $"{nameof(Character)}_Instance";

            Character character = GetRequiredComponent<Character>(instance);
            AnimatorComponent animatorComponent = GetRequiredComponent<AnimatorComponent>(instance);
            AttackComponent attackComponent = GetRequiredComponent<AttackComponent>(instance);
            MovementComponent movementComponent = GetRequiredComponent<MovementComponent>(instance);
            EquipmentComponent equipmentComponent = GetRequiredComponent<EquipmentComponent>(instance);
            EquipmentPresentation equipmentPresentation =
                GetRequiredComponent<EquipmentPresentation>(instance);
            InventoryComponent inventoryComponent = GetRequiredComponent<InventoryComponent>(instance);
            HealthComponent healthComponent = GetRequiredComponent<HealthComponent>(instance);

            LifetimeScope characterScope = RootScope.CreateChild(builder =>
            {
                builder.RegisterComponent(character).AsSelf().AsImplementedInterfaces();

                builder.Register<AnimatorModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponent(animatorComponent).AsSelf().AsImplementedInterfaces();

                builder.RegisterComponent(attackComponent).AsSelf().AsImplementedInterfaces();

                builder.Register<MovementModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterScriptableObject<MovementData>().As<IMovementData>();
                builder.RegisterComponent(movementComponent).AsSelf().AsImplementedInterfaces();

                builder.Register<EquipmentModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponent(equipmentComponent).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(equipmentPresentation).AsSelf();

                builder.RegisterScriptableObject<InventoryData>();
                builder.RegisterScriptableObject<ItemDatabase>();
                builder.Register<InventoryModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponent(inventoryComponent).AsSelf().AsImplementedInterfaces();

                builder.RegisterScriptableObject<HealthData>().As<IHealthData>();
                builder.Register<HealthModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponent(healthComponent).AsSelf().AsImplementedInterfaces();
                builder.Register<PlayerHudUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<LockOnUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<InventoryUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<EquipmentUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<PauseNavigationUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

                builder.Register<UnityCharacterClock>(Lifetime.Singleton).As<ICharacterClock>();
                builder.Register<MovementGate>(Lifetime.Singleton).AsSelf();
                builder.Register<CharacterCommandBuffer>(Lifetime.Singleton).AsSelf();
                builder.Register<CharacterActionStateMachine>(Lifetime.Singleton).AsSelf();
                builder.Register<CharacterRuntime>(Lifetime.Singleton).AsSelf();
                builder.Register<CharacterAnimationAdapter>(Lifetime.Singleton).AsSelf();
                builder.Register<EquipmentSwapCoordinator>(Lifetime.Singleton).AsSelf();
                builder.Register<SprintRollGestureResolver>(Lifetime.Singleton).AsSelf();
                builder.Register<HeavyAttackGestureResolver>(Lifetime.Singleton).AsSelf();
                builder.Register<PlayerCharacterInputAdapter>(Lifetime.Singleton).AsSelf();
                builder.Register<PlayerController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            });

            instance.transform.SetParent(characterScope.transform, true);
            return character;
        }

        private static TComponent GetRequiredComponent<TComponent>(GameObject instance)
            where TComponent : Component
        {
            TComponent component = instance.GetComponent<TComponent>();
            if (component == null)
            {
                throw new InvalidOperationException(
                    $"Character prefab requires a {typeof(TComponent).Name} component.");
            }

            return component;
        }
    }
}
