using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Character.Components.Movement;
using SoulsLike.Extensions;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    public class CharacterScope : LifetimeScope
    {
        [SerializeField] private Character _character;
        [SerializeField] private AnimatorComponent _animatorComponent;
        [SerializeField] private MovementComponent _movementComponent;
        [SerializeField] private EquipmentComponent _equipmentComponent;
        [SerializeField] private InventoryComponent _inventoryComponent;
        [SerializeField] private HealthComponent _healthComponent;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_character).AsSelf().AsImplementedInterfaces();

            builder.Register<AnimatorModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponent(_animatorComponent).AsSelf().AsImplementedInterfaces();

            builder.Register<MovementModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterScriptableObject<MovementData>().As<IMovementData>();
            builder.RegisterComponent(_movementComponent).AsSelf().AsImplementedInterfaces();

            builder.Register<EquipmentModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponent(_equipmentComponent).AsSelf().AsImplementedInterfaces();

            builder.RegisterScriptableObject<InventoryData>();
            builder.RegisterComponent(_inventoryComponent).AsSelf();

            builder.RegisterScriptableObject<HealthData>().As<IHealthData>();
            builder.Register<HealthModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponent(_healthComponent).AsSelf().AsImplementedInterfaces();

            builder.Register<PlayerController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}
