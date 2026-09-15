using System;
using SoulsLike;
using SoulsLike.Entities.BaseEntity;
using SoulsLike.Entities.BaseEntity.EntityCommands;
using SoulsLike.Entities.Combat;
using SoulsLike.Entities.Character.Components;
using SoulsLike.Entities.Character.Components.Attack;
using SoulsLike.Entities.Character.Components.Equipment;
using SoulsLike.Entities.Character.Components.Health;
using SoulsLike.Entities.Character.Components.Inventory;
using SoulsLike.Entities.Character.Components.Movement;
using SoulsLike.Entities.Character.Input;
using SoulsLike.Extensions;
using SoulsLike.Factory;
using SoulsLike.Interactions;
using SoulsLike.Ui.LockOn;
using SoulsLike.Ui.PlayerHud;
using SoulsLike.Items;
using SoulsLike.Entities.Ladder;
using SoulsLike.Services.IdGeneration;
using SoulsLike.Ui.Inventory;
using SoulsLike.Ui.Equipment;
using SoulsLike.Ui.Interaction;
using SoulsLike.Ui.PauseNavigation;
using SoulsLike.Ui.Status;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SoulsLike.Entities.Character
{
    public class CharacterFactory : BaseFactory, IDisposable
    {
        private const string CHARACTER_PREFAB_KEY = nameof(Character);//todo: if character class will be renamed - const value will be changed too and prefab load will be fucked up 
        private readonly IUniqueIdGenerator _uniqueIdGenerator;

        private LifetimeScope _characterScope;

        public CharacterFactory(IObjectResolver resolver, IUniqueIdGenerator uniqueIdGenerator) : base(resolver)
        {
            _uniqueIdGenerator = uniqueIdGenerator;
        }

        public Character CreateCharacter(Vector3? spawnPosition = null)
        {
            GameObject prefab = AssetService.LoadPrefab(CHARACTER_PREFAB_KEY);
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Character prefab for Addressables key '{CHARACTER_PREFAB_KEY}' was not found.");
            }

            //todo: don't create go of character inside of this scope
            GameObject instance = UnityEngine.Object.Instantiate(prefab);
            instance.name = $"{nameof(Character)}_Instance";
            if (spawnPosition.HasValue)
            {
                instance.transform.position = spawnPosition.Value;
            }

            Character character = instance.GetComponent<Character>();
            
            //todo: add it dynamically in RootScope.CreateChild
            ViewEntity viewEntity = instance.GetComponent<ViewEntity>();
            if (viewEntity == null)
            {
                viewEntity = instance.AddComponent<ViewEntity>();
            }

            TargetLockNode targetLockNode = instance.GetComponentInChildren<TargetLockNode>(true);
            PlayerMeleeCombatRelay meleeCombatRelay =
                instance.GetComponent<PlayerMeleeCombatRelay>();
            CriticalAttackController criticalAttackController =
                instance.GetComponent<CriticalAttackController>();

            AnimatorComponent animatorComponent = instance.GetComponent<AnimatorComponent>();
            CharacterAudioComponent audioComponent = instance.GetComponentInChildren<CharacterAudioComponent>(true);
            AttackComponent attackComponent = instance.GetComponent<AttackComponent>();
            MovementComponent movementComponent = instance.GetComponent<MovementComponent>();
            EquipmentComponent equipmentComponent = instance.GetComponent<EquipmentComponent>();
            EquipmentPresentation equipmentPresentation =
                instance.GetComponent<EquipmentPresentation>();
            InventoryComponent inventoryComponent = instance.GetComponent<InventoryComponent>();
            HealthComponent healthComponent = instance.GetComponent<HealthComponent>();
            CombatDefenseComponent combatDefense = instance.GetComponent<CombatDefenseComponent>();
            LadderClimber ladderClimber = instance.GetComponent<LadderClimber>();
            animatorComponent.ConfigureCharacter(character, movementComponent);
            long entityId = _uniqueIdGenerator.GenerateUniqueId();

            _characterScope = RootScope.CreateChild(builder =>
            {
                builder.RegisterEntitySystemExt(EntityType.Player, entityId);
                builder.RegisterComponent(viewEntity).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(targetLockNode).AsSelf();
                builder.Register<InteractionCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<GroundItemCollectionCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<ApplyDamageCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<ResolveMeleeHitCommand>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();
                builder.Register<TargetingCommand>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<PlatformRideCommand>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces();

                builder.RegisterComponent(character).AsSelf().AsImplementedInterfaces();
                builder.RegisterScriptableObject<CharacterData>();

                builder.Register<AnimatorModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponent(animatorComponent).AsSelf().AsImplementedInterfaces();
                builder.RegisterScriptableObject<CharacterAudioData>();
                builder.RegisterComponent(audioComponent).AsSelf().AsImplementedInterfaces();

                builder.RegisterComponent(attackComponent).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(meleeCombatRelay).AsSelf();
                builder.RegisterComponent(criticalAttackController).AsSelf().AsImplementedInterfaces();

                builder.Register<MovementModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterScriptableObject<MovementData>().As<IMovementData>();
                builder.RegisterComponent(movementComponent).AsSelf().AsImplementedInterfaces();

                builder.Register<EquipmentModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponent(equipmentComponent).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(equipmentPresentation).AsSelf();

                builder.RegisterScriptableObject<InventoryData>();
                builder.RegisterScriptableObject<ItemDatabase>();
                builder.RegisterScriptableObject<WeaponDatabase>();
                builder.RegisterScriptableObject<ShieldDatabase>();
                builder.RegisterScriptableObject<ConsumableDatabase>();
                builder.Register<ItemCatalog>(Lifetime.Singleton).AsSelf();
                builder.Register<InventoryModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponent(inventoryComponent).AsSelf().AsImplementedInterfaces();

                builder.RegisterScriptableObject<HealthData>();
                builder.Register<CharacterHealthData>(Lifetime.Singleton).As<IHealthData>();
                builder.Register<HealthModel>(Lifetime.Singleton).AsSelf();
                builder.RegisterComponent(healthComponent).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(combatDefense).AsSelf().AsImplementedInterfaces();
                builder.RegisterComponent(ladderClimber).AsSelf().AsImplementedInterfaces();
                builder.Register<PlayerHudUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<LockOnUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<InventoryUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<EquipmentUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<StatusUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<SystemUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<PauseNavigationUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

                builder.Register<PlayerInputReader>(Lifetime.Singleton).AsSelf();
                builder.Register<InteractionController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<InteractionUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<PlayerController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            });

            instance.transform.SetParent(_characterScope.transform, true);

            return character;
        }

        public void Dispose()
        {
            _characterScope.Dispose();
        }

    }
}
