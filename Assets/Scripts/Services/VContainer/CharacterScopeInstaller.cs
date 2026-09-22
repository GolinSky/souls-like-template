using SoulsLike;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Character;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Character.Components.Movement;
using SoulsLike.Entities.Character.Components.Targeting;
using SoulsLike.Entities.Character.Input;
using SoulsLike.Entities.Character.Runtime;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Ladder;
using SoulsLike.Extensions;
using SoulsLike.Interactions;
using SoulsLike.Items;
using SoulsLike.Ui.Equipment;
using SoulsLike.Ui.Grace;
using SoulsLike.Ui.Interaction;
using SoulsLike.Ui.Inventory;
using SoulsLike.Ui.LevelUp;
using SoulsLike.Ui.LockOn;
using SoulsLike.Ui.PauseNavigation;
using SoulsLike.Ui.PlayerHud;
using SoulsLike.Ui.Status;
using SoulsLike.Services.PlayerSession;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Services.VContainer
{
    /// <summary>Composes reusable character runtime services and the local player binding.</summary>
    public sealed class CharacterScopeInstaller : EntityLifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPointExceptionHandler(exception => throw exception);

            RegisterActorServices(builder, transform);
            RegisterLocalPlayerServices(builder);
        }

        /// <summary>Registers reusable actor services under a supplied character hierarchy.</summary>
        public static void RegisterActorServices(IContainerBuilder builder, Transform actorRoot)
        {
            // Entity identity and scene components.
            builder.RegisterEntitySystemExt(EntityType.Player);
            builder.RegisterComponentInHierarchy<ViewEntity>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<TargetLockComponent>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<Character>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();

            // Entity commands.
            builder.Register<InteractionCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<GroundItemCollectionCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<ApplyDamageCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<ResolveMeleeHitCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<TargetingCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PlatformRideCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            // Character configuration, animation, and combat.
            builder.RegisterScriptableObject<CharacterData>();
            builder.Register<AnimatorModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponentInHierarchy<AnimatorComponent>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();
            builder.RegisterScriptableObject<CharacterAudioData>();
            builder.RegisterComponentInHierarchy<CharacterAudioComponent>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<AttackComponent>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<PlayerMeleeCombatRelay>().UnderTransform(actorRoot).AsSelf();
            builder.RegisterComponentInHierarchy<CriticalAttackController>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();

            // Movement and equipment.
            builder.Register<MovementModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterScriptableObject<MovementData>().As<IMovementData>();
            builder.RegisterComponentInHierarchy<MovementComponent>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();
            builder.Register<EquipmentModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponentInHierarchy<EquipmentComponent>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<EquipmentPresentation>().UnderTransform(actorRoot).AsSelf();

            // Inventory and health.
            builder.RegisterScriptableObject<InventoryData>();
            builder.RegisterScriptableObject<ItemDatabase>();
            builder.RegisterScriptableObject<WeaponDatabase>();
            builder.RegisterScriptableObject<ShieldDatabase>();
            builder.RegisterScriptableObject<ConsumableDatabase>();
            builder.Register<ItemCatalog>(Lifetime.Singleton).AsSelf();
            builder.Register<InventoryModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponentInHierarchy<InventoryComponent>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();
            builder.RegisterScriptableObject<HealthData>();
            builder.Register<CharacterHealthData>(Lifetime.Singleton).As<IHealthData>();
            builder.Register<HealthModel>(Lifetime.Singleton).AsSelf();
            builder.RegisterComponentInHierarchy<HealthComponent>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<CombatDefenseComponent>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();
            builder.RegisterComponentInHierarchy<LadderClimber>().UnderTransform(actorRoot).AsSelf().AsImplementedInterfaces();

            builder.Register<CharacterActionStateMachine>(Lifetime.Scoped).AsSelf();
            builder.Register<CharacterActionCoordinator>(Lifetime.Scoped).AsSelf();
            builder.Register<CharacterActorTick>(Lifetime.Scoped).AsSelf().AsImplementedInterfaces();
        }

        private void RegisterLocalPlayerServices(IContainerBuilder builder)
        {
            // Local-player UI, input, and orchestration.
            builder.Register<PlayerHudUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LockOnUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<InventoryUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<EquipmentUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<StatusUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<SystemUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PauseNavigationUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LevelUpUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<GraceUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PlayerInputReader>(Lifetime.Singleton).AsSelf();
            builder.Register<InteractionController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<InteractionUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PlayerSessionCoordinator>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PlayerController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
        }
    }
}
