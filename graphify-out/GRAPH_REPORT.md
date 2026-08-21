# Graph Report - SoulsLikeTemplate  (2026-08-21)

## Corpus Check
- 285 files · ~51,066 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2463 nodes · 4360 edges · 272 communities (130 shown, 142 thin omitted)
- Extraction: 91% EXTRACTED · 9% INFERRED · 0% AMBIGUOUS · INFERRED: 410 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c8d41435`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneType
- HealthComponent
- ItemDefinition
- TargetingSnapshot
- AnimatorStateMachineReceiver
- PreviewRenderService
- EnemyActionStateBehaviour
- PlayerHudUi
- EquipmentSlotId
- AnimatorComponent
- Character
- GroundItem
- CharacterAudioComponent
- EquipmentUiController
- EquipmentUi
- SoulsLike.Services.Audio.Data
- EquipmentComponent
- CharacterCommand
- InventorySlotUI
- .BuildLoadout
- InventoryEntry
- EnemyAnimationController
- CameraService
- PauseNavigationUiController
- SoulsLike
- OnGuiFpsCounter
- CharacterActions
- IPauseMenuPresenter
- AmbienceService
- IEntityLocator
- AttackComponent
- InventoryData.cs
- CustomButtonToggle
- EnemyBrain
- SpeedMultiplierKey
- ITimer
- HealthModel
- MainMenuOrchestrator
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntryId
- IHealthComponent
- InventoryItemViewData
- AudioClip
- LockOnUiController
- PlayerController
- System.Ui.Base
- .CreateButton
- ItemAcquisitionPanel
- InventoryUi
- Color
- PauseMenuUiController
- AmbienceData
- EquipmentSlotUI
- InventoryUiController
- Vector2
- BaseUi
- MovementComponent
- IContainerBuilder
- CustomButtonMapping
- TargetLockNode
- float
- CoroutineService
- IUniqueIdGenerator
- PlayerMeleeCombatRelay
- MainMenuUiController
- SaveService
- ICustomButton
- InventoryItemSO
- EquipmentSlotHud
- .Tick
- InventoryViewStateController
- .TryStartAttack
- IInitializable
- BaseFactory
- CustomButtonEditor
- WeaponDefinition
- CharacterRuntime.cs
- BasePopup
- .TryAdapt
- AnimatorRootMotionRelay
- SharedSceneScope
- .Read
- StateMachineName
- UiService
- .DisplaySlot
- GameObject
- Tween
- SceneReference
- List
- List
- IGameStateNotifier
- CanvasGroupExt.cs
- .Submit
- Component
- EnemyHealthUiController
- .ShowAcquisition
- ICharacterActionExecutor
- ItemId
- MeleeHitboxController
- SoulsLike.Entities.BaseEntity.EntityCommands
- IComponentMediator
- DamageRequest
- IComponentMediator
- IComponentMediator
- IComponentMediator
- .SetAirborneMotion
- IComponentMediator
- IComponentMediator
- Dictionary
- IComponentMediator
- bool
- CharacterActions
- SoulsLike.Services
- bool
- float
- Vector2
- .Open
- Quaternion
- Vector2
- Vector3
- BufferedCharacterAction
- EquipmentSwapPhase
- EnemySpawnPoint
- ICameraService
- IInputService
- float
- DamageRequest
- IAudioSettingsData
- InteractionUiController
- DeterministicEnemyActionSelector
- SoulsLike.Ui.Base
- AttackRequest
- CharacterActionStateId
- CharacterInputBatch
- EquipmentPresentation
- CharacterAttributeStats
- AttackType.cs
- InputService
- Vector3Converter
- ICharacterCommand
- IEquipmentCommandReceiver
- AddressableAssetService
- EnemyNavigationMotor
- .Move
- IContainerBuilder
- AnimatorControllerParameterType
- ScriptableObject
- EnemyActor
- EntityLocator
- AnimatorComponent
- Tween
- MonoBehaviour
- Vector3
- ItemType
- CharacterActionStateMachine
- AnimatorComponent
- int
- Inject
- Transform
- SoulsLike.Ui.PlayerHud
- CharacterActionStateId
- CharacterInputBatch
- CharacterCommandFactory
- IContainerBuilder
- MainMenuOrchestrator.cs
- EquipmentSlotGroup
- AnimationEvent
- Animator
- AnimatorComponent
- Character
- AnimatorStateMachineDto
- EquipmentComponent
- EquipmentLoadout
- InventoryComponent
- ITimer
- StateMachineName
- PlayerMeleeAttackStateBehaviour
- Character
- .UpdateState
- AttackType
- .BeginRootMotionAction
- ItemCatalog
- LayerMask
- HandMode
- RuntimeAnimatorController
- InteractionController
- bool
- Collider
- AttackType
- EquipmentLoadout
- InventoryComponent
- EquipmentLoadout
- EquippedItemContext
- CharacterCommandExecutionStatus
- IInputService
- Color
- Inject
- SoulsLike.Entities.Combat
- Action
- IReadOnlyCollection
- RectTransform
- MPImage
- InventoryChange
- IMovementPresentationSink
- CombatProfile
- EquipmentSlotId
- IEntity
- HealthStats
- PlayerHudUiController
- LandingType
- InventoryEntry
- InventoryEntryId
- IReadOnlyList
- ItemDatabase
- WeaponDefinition
- WeaponRuntime
- ShieldDatabase
- ICameraService
- .ResolveMovementCollisions
- Entity
- MovementComponent
- Quaternion
- ITimer
- IDisposable
- Vector2
- EnemyHealthUi
- .Interrupt
- SceneReferencePropertyDrawer
- GameObject
- GroundItemVfx
- IEquipmentLoadoutSink.cs
- SoulsLike.Entities.Character.Components.Movement
- IMovementData
- TargetingCommand
- EnemyFactory
- DamageResult
- SceneService
- PauseNavigationUi
- SoulsLike.Services.Scenes.Data
- EnemyBehaviourProfile
- EnemyContracts.cs
- .DisplayItemDetails
- LockOnUi
- .OnItemFocused
- SoulsLike.Entities.Character.Components.Health
- CharacterCommandExecutionStatus
- GameOrchestrator
- DamageRequest
- DamageResult
- DamageRequest
- DamageResult
- Character
- IInputService
- Transform
- RectTransform
- Transform
- InteractionPrompt
- Sprite
- AttackComponent
- Camera
- VContainerExt
- DamageRequest
- DamageResult
- GameState
- InteractionPrompt
- HealthModel
- CoreGameOrchestrator
- ITargetingService
- string

## God Nodes (most connected - your core abstractions)
1. `Character` - 69 edges
2. `AnimatorComponent` - 59 edges
3. `MovementComponent` - 57 edges
4. `EnemyBrain` - 38 edges
5. `InventoryUiController` - 37 edges
6. `EquipmentUi` - 36 edges
7. `EnemyAnimationController` - 36 edges
8. `AmbienceService` - 36 edges
9. `EquipmentUiController` - 33 edges
10. `EquipmentSlotUI` - 33 edges

## Surprising Connections (you probably didn't know these)
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `ConsumableDefinition` --references--> `ItemUseType`  [EXTRACTED]
  Assets/Scripts/Items/ConsumableDefinition.cs → Assets/Scripts/Items/ItemTypes.cs
- `SceneService` --references--> `SceneModel`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/SceneService.cs → Assets/Scripts/Services/Scenes/Data/SceneModel.cs
- `SceneService` --references--> `SceneType`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/SceneService.cs → Assets/Scripts/Services/Scenes/Data/SceneType.cs
- `GameOrchestrator` --references--> `ISceneService`  [EXTRACTED]
  Assets/Scripts/Orchestrators/Game/GameOrchestrator.cs → Assets/Scripts/Services/Scenes/ISceneService.cs

## Import Cycles
- None detected.

## Communities (272 total, 142 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.16
Nodes (10): CharacterAttributeStats, SpeedMultiplierKey, SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character (+2 more)

### Community 1 - "SceneType"
Cohesion: 0.15
Nodes (9): UniTask, Scene, SerializedDictionary, SceneData, Scene, SceneModel, SceneType, UniTask (+1 more)

### Community 2 - "HealthComponent"
Cohesion: 0.24
Nodes (3): HealthStats, HealthStatUpdate, HealthComponent

### Community 3 - "ItemDefinition"
Cohesion: 0.14
Nodes (12): Dictionary, IReadOnlyList, List, ItemDatabase, float, int, IReadOnlyList, List (+4 more)

### Community 4 - "TargetingSnapshot"
Cohesion: 0.14
Nodes (11): EntityType, Vector3, TargetingSnapshot, Collider, RaycastHit, List, Vector3, EnemyPerception (+3 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (18): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+10 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.06
Nodes (23): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerMask, ILayerService (+15 more)

### Community 7 - "EnemyActionStateBehaviour"
Cohesion: 0.23
Nodes (6): Animator, AnimatorStateInfo, bool, float, EnemyActionStateBehaviour, CharacterActionId

### Community 8 - "PlayerHudUi"
Cohesion: 0.18
Nodes (8): bool, float, HealthStats, PlayerHudUi, StatBar, EquipmentSlotHud, MPImage, StatBar

### Community 9 - "EquipmentSlotId"
Cohesion: 0.18
Nodes (8): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode, EquipmentSlotId

### Community 10 - "AnimatorComponent"
Cohesion: 0.10
Nodes (17): AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, Animator, bool, float, IAnimationStateSink, Inject (+9 more)

### Community 11 - "Character"
Cohesion: 0.07
Nodes (26): InventoryComponent, ItemCatalog, Transform, GroundItemCollectionCommand, AttackComponent, float, HealthStats, IEntityLocator (+18 more)

### Community 12 - "GroundItem"
Cohesion: 0.17
Nodes (12): CancellationToken, Collider, IEntity, int, InteractionPrompt, ItemId, string, Transform (+4 more)

### Community 13 - "CharacterAudioComponent"
Cohesion: 0.12
Nodes (12): bool, float, Inject, CharacterAudioComponent, CharacterAudioData, DamageResult, AudioResource, AudioSource (+4 more)

### Community 14 - "EquipmentUiController"
Cohesion: 0.14
Nodes (8): EquipmentSlotId, IInputService, InventoryChange, InventoryEntry, InventoryEntryId, EquipmentUiController, IEquipmentPresenter, IEquipmentRoute

### Community 15 - "EquipmentUi"
Cohesion: 0.13
Nodes (7): Dictionary, GameObject, IEquipmentPresenter, Image, int, Transform, EquipmentUi

### Community 16 - "SoulsLike.Services.Audio.Data"
Cohesion: 0.15
Nodes (5): MusicType, SfxType, IAmbienceSystem, SoulsLike.Services.Audio.Data, SoulsLike.Services.Audio

### Community 17 - "EquipmentComponent"
Cohesion: 0.17
Nodes (9): EquipmentSlotId, IEquipmentLoadoutSink, Inject, InventoryChange, InventoryEntry, InventoryEntryId, IReadOnlyList, EquipmentComponent (+1 more)

### Community 18 - "CharacterCommand"
Cohesion: 0.17
Nodes (11): float, Vector2, AttackIntent, AttackRequest, CharacterCommand, CharacterCommandBuffer, CharacterCommandKind, EquipmentActionKind (+3 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.12
Nodes (12): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+4 more)

### Community 20 - ".BuildLoadout"
Cohesion: 0.13
Nodes (13): EquipmentSlotGroup, HandMode, EquipmentSlotGroup, AnimatorStateMachineDto, CharacterCommandExecutionStatus, EquipmentComponent, EquipmentLoadout, EquipmentSlotGroup (+5 more)

### Community 21 - "InventoryEntry"
Cohesion: 0.18
Nodes (8): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "EnemyAnimationController"
Cohesion: 0.14
Nodes (14): Animator, EnemyActor, EnemyNavigationMotor, Entity, float, IEntityLocator, Inject, int (+6 more)

### Community 23 - "CameraService"
Cohesion: 0.11
Nodes (10): bool, Camera, float, long, Vector3, CameraService, CinemachineCamera, CinemachineThirdPersonFollow (+2 more)

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.17
Nodes (4): IEquipmentRoute, IPauseNavigationRoute, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 29 - "AmbienceService"
Cohesion: 0.13
Nodes (10): AudioClip, float, IAudioService, IAudioSettingsData, MusicType, SceneType, AmbienceService, IAmbienceSystem (+2 more)

### Community 30 - "IEntityLocator"
Cohesion: 0.18
Nodes (5): EntityType, IEntity, List, IEntityLocator, Inject

### Community 31 - "AttackComponent"
Cohesion: 0.15
Nodes (12): AnimatorStateMachineDto, AttackRequest, bool, CombatProfile, float, HandMode, Inject, AttackComponent (+4 more)

### Community 32 - "InventoryData.cs"
Cohesion: 0.21
Nodes (9): HealthData, IHealthData, IReadOnlyList, List, InitialInventoryEntry, InventoryData, Data, Model (+1 more)

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "EnemyBrain"
Cohesion: 0.12
Nodes (16): bool, EnemyActor, EnemyNavigationMotor, float, GameState, HealthModel, IGameStateNotifier, int (+8 more)

### Community 36 - "ITimer"
Cohesion: 0.12
Nodes (6): ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 37 - "HealthModel"
Cohesion: 0.17
Nodes (7): HealthStats, long, HealthModel, float, CharacterHealthData, HealthData, IHealthData

### Community 38 - "MainMenuOrchestrator"
Cohesion: 0.18
Nodes (4): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator

### Community 39 - "StorageRegistry"
Cohesion: 0.20
Nodes (6): Enum, IStorageRegistry, Enum, string, StorageRegistry, SoulsLike.Services.Storage

### Community 40 - "IMovementComponent"
Cohesion: 0.15
Nodes (6): Quaternion, SpeedMultiplierKey, Transform, Vector2, Vector3, IMovementComponent

### Community 41 - "CustomButton"
Cohesion: 0.13
Nodes (8): bool, ColorBlock, Image, SelectionState, Sprite, TMP_Text, CustomButton, Button

### Community 42 - "InventoryEntryId"
Cohesion: 0.18
Nodes (3): string, InventoryEntryId, IEquipmentPresenter

### Community 43 - "IHealthComponent"
Cohesion: 0.23
Nodes (3): HealthStats, HealthStatUpdate, IHealthComponent

### Community 44 - "InventoryItemViewData"
Cohesion: 0.29
Nodes (5): IReadOnlyList, InventoryEntryId, InventoryPrimaryCategory, Sprite, InventoryItemViewData

### Community 46 - "LockOnUiController"
Cohesion: 0.22
Nodes (4): bool, Camera, LockOnUiController, IPostLateTickable

### Community 47 - "PlayerController"
Cohesion: 0.08
Nodes (13): GameState, IGameStateNotifier, InteractionController, PlayerController, Transform, Vector2, ICameraService, Vector3 (+5 more)

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 50 - "ItemAcquisitionPanel"
Cohesion: 0.18
Nodes (6): float, Sprite, ItemAcquisitionPanel, CanvasGroup, Image, TextMeshProUGUI

### Community 51 - "InventoryUi"
Cohesion: 0.14
Nodes (10): Color, IInventoryPresenter, Image, int, IReadOnlyList, List, Transform, InventoryUi (+2 more)

### Community 52 - "Color"
Cohesion: 0.24
Nodes (7): ColorConverter, JsonReader, JsonSerializer, JsonWriter, Type, Color, JsonConverter

### Community 54 - "AmbienceData"
Cohesion: 0.16
Nodes (13): SfxType, AudioClip, float, MusicType, SceneType, SfxType, AmbienceData, MusicEntry (+5 more)

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Color, EquipmentSlotId, GameObject, Image, MPImage, PointerEventData (+3 more)

### Community 56 - "InventoryUiController"
Cohesion: 0.13
Nodes (11): EquipmentSlotId, InventoryEntryId, EquipmentSlotChange, bool, IInputService, InventoryChange, InventoryPrimaryCategory, InventoryUiController (+3 more)

### Community 58 - "BaseUi"
Cohesion: 0.20
Nodes (6): bool, CanvasGroup, Transform, BaseUi, IBaseUi, SoulsLike.Extensions

### Community 59 - "MovementComponent"
Cohesion: 0.11
Nodes (14): bool, float, int, LandingType, Transform, MovementComponent, Dictionary, IMovementComponent (+6 more)

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 62 - "TargetLockNode"
Cohesion: 0.50
Nodes (3): Transform, TargetLockAnchorType, TargetLockNode

### Community 64 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 65 - "IUniqueIdGenerator"
Cohesion: 0.33
Nodes (3): IUniqueIdGenerator, UniqueIdGenerator, SoulsLike.Services.IdGeneration

### Community 66 - "PlayerMeleeCombatRelay"
Cohesion: 0.24
Nodes (8): AttackComponent, CharacterActionId, Entity, float, IEntityLocator, Inject, WeaponDatabase, PlayerMeleeCombatRelay

### Community 67 - "MainMenuUiController"
Cohesion: 0.15
Nodes (5): IMainMenuOrchestrator, IMainMenuPresenter, MainMenuUi, MainMenuUiController, IStartable

### Community 68 - "SaveService"
Cohesion: 0.08
Nodes (11): string, FileService, SaveKeys, string, ISaveService, SaveService, string, SaveStore (+3 more)

### Community 70 - "InventoryItemSO"
Cohesion: 0.12
Nodes (10): InventoryPrimaryCategory, InventorySubCategory, bool, float, int, Sprite, string, InventoryItemSO (+2 more)

### Community 71 - "EquipmentSlotHud"
Cohesion: 0.22
Nodes (8): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud

### Community 73 - "InventoryViewStateController"
Cohesion: 0.23
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 74 - ".TryStartAttack"
Cohesion: 0.20
Nodes (4): AnimatorStateMachineDto, ItemId, AttackRequest, EquippedItemContext

### Community 75 - "IInitializable"
Cohesion: 0.14
Nodes (9): Inject, InventoryEntry, InventoryEntryId, IReadOnlyList, InventoryComponent, BaseComponent, IInitializable, InventoryData (+1 more)

### Community 76 - "BaseFactory"
Cohesion: 0.22
Nodes (7): LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory, SoulsLike.Factory, IObjectResolver

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "WeaponDefinition"
Cohesion: 0.10
Nodes (21): AnimationProfile, float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot, ItemUseType (+13 more)

### Community 79 - "CharacterRuntime.cs"
Cohesion: 0.16
Nodes (12): bool, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind, CharacterCommandExecutionResult, CharacterControlFrame, CharacterInputBatch, CharacterRuntime (+4 more)

### Community 80 - "BasePopup"
Cohesion: 0.11
Nodes (12): Action, GenericPopupService, IGenericPopupService, AcceptPopup, Button, AlertPopup, Action, Button (+4 more)

### Community 81 - ".TryAdapt"
Cohesion: 0.20
Nodes (6): AnimatorStateMachineDto, CharacterAnimationAdapter, UnityCharacterClock, ICharacterClock, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Runtime

### Community 82 - "AnimatorRootMotionRelay"
Cohesion: 0.17
Nodes (7): Animator, bool, string, AnimatorRootMotionRelay, Quaternion, Vector3, IRootMotionSink

### Community 83 - "SharedSceneScope"
Cohesion: 0.18
Nodes (8): CoreScope, ProjectScope, SharedSceneScope, CameraService, IContainerBuilder, LifetimeScope, OnGuiFpsCounter, PreviewRenderService

### Community 84 - ".Read"
Cohesion: 0.12
Nodes (11): PlayerCharacterInputAdapter, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, SoulsLike.Entities.Character.Input, float, HeavyAttackGestureResolver (+3 more)

### Community 86 - "UiService"
Cohesion: 0.20
Nodes (8): IUiService, UiService, BaseUi, Canvas, Inject, List, Transform, UiFactory

### Community 87 - ".DisplaySlot"
Cohesion: 0.20
Nodes (5): CharacterAttributeStats, EquipmentSlotId, List, TMP_Text, IReadOnlyDictionary

### Community 90 - "SceneReference"
Cohesion: 0.14
Nodes (7): bool, string, SceneReference, IComparable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 93 - "IGameStateNotifier"
Cohesion: 0.18
Nodes (3): GameState, IGameStateNotifier, IGameStateObserver

### Community 97 - "EnemyHealthUiController"
Cohesion: 0.19
Nodes (9): Action, Camera, EnemyActor, HealthModel, HealthStats, List, EnemyHealthUiController, TrackedEnemy (+1 more)

### Community 100 - "ItemId"
Cohesion: 0.15
Nodes (11): Dictionary, IReadOnlyList, List, ConsumableDatabase, float, ConsumableDefinition, Sprite, ItemCatalog (+3 more)

### Community 101 - "MeleeHitboxController"
Cohesion: 0.19
Nodes (9): CharacterActionId, float, IEntityLocator, int, ItemId, long, MeleeHitboxController, Collider (+1 more)

### Community 102 - "SoulsLike.Entities.BaseEntity.EntityCommands"
Cohesion: 0.33
Nodes (3): SoulsLike.Services.Targeting, SoulsLike.Services.CameraService, SoulsLike.Entities.BaseEntity.EntityCommands

### Community 104 - "DamageRequest"
Cohesion: 0.22
Nodes (7): float, int, long, Vector3, DamageRequest, ItemId, ApplyDamageRequest

### Community 115 - "SoulsLike.Services"
Cohesion: 0.31
Nodes (3): SoulsLike.Services, SoulsLike.Entities.Enemy, SoulsLike.Ui.EnemyHealth

### Community 125 - "EnemySpawnPoint"
Cohesion: 0.33
Nodes (4): HealthData, Transform, Vector3, EnemySpawnPoint

### Community 130 - "IAudioSettingsData"
Cohesion: 0.19
Nodes (6): bool, float, AudioSettingsData, IAudioSettingsData, IAudioService, IObserver

### Community 131 - "InteractionUiController"
Cohesion: 0.17
Nodes (9): IInteractionPresenter, string, InteractionUi, InteractionUiController, SoulsLike.Ui.Interaction, InteractionController, InteractionPrompt, TMP_Text (+1 more)

### Community 132 - "DeterministicEnemyActionSelector"
Cohesion: 0.13
Nodes (13): CharacterActionId, Dictionary, IReadOnlyList, List, DeterministicEnemyActionSelector, CharacterActionId, EnemyActionCandidate, EnemyActionSelectionContext (+5 more)

### Community 137 - "EquipmentPresentation"
Cohesion: 0.13
Nodes (14): HandMode, InventoryEntry, EquipmentLoadout, EquippedItemContext, bool, GameObject, Inject, Quaternion (+6 more)

### Community 140 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 141 - "Vector3Converter"
Cohesion: 0.31
Nodes (6): JsonReader, JsonSerializer, JsonWriter, Type, Vector3Converter, Vector3

### Community 144 - "AddressableAssetService"
Cohesion: 0.20
Nodes (4): GameObject, AddressableAssetService, IAssetService, SoulsLike.Services.Repository

### Community 145 - "EnemyNavigationMotor"
Cohesion: 0.13
Nodes (8): Vector3, bool, float, Quaternion, Vector3, EnemyNavigationMotor, CharacterController, NavMeshAgent

### Community 146 - ".Move"
Cohesion: 0.13
Nodes (4): Inject, LandingType, Vector2, IMovementPresentationSink

### Community 149 - "ScriptableObject"
Cohesion: 0.22
Nodes (6): RuntimeAnimatorController, AnimationProfile, CombatProfile, float, AudioData, ScriptableObject

### Community 150 - "EnemyActor"
Cohesion: 0.25
Nodes (8): Animator, Entity, HealthData, Inject, IReadOnlyList, LifetimeScope, Vector3, EnemyActor

### Community 151 - "EntityLocator"
Cohesion: 0.23
Nodes (7): Collider, Dictionary, EntityType, IEntity, List, RaycastHit, EntityLocator

### Community 154 - "MonoBehaviour"
Cohesion: 0.83
Nodes (3): BaseComponent, IComponent, MonoBehaviour

### Community 155 - "Vector3"
Cohesion: 0.22
Nodes (3): Quaternion, Vector2, Vector3

### Community 156 - "ItemType"
Cohesion: 0.14
Nodes (7): IReadOnlyList, ItemType, Action, IReadOnlyCollection, IInventoryRoute, IPauseNavigationRouteNavigation, IReadOnlyCollection

### Community 157 - "CharacterActionStateMachine"
Cohesion: 0.21
Nodes (8): CharacterActionStateMachine, CharacterActionStateId, CharacterAnimationSignal, CharacterCommand, CharacterCommandBuffer, CharacterCommandExecutionResult, CharacterControlFrame, ICharacterActionExecutor

### Community 167 - "MainMenuOrchestrator.cs"
Cohesion: 0.20
Nodes (4): IContainerBuilder, MainMenuScope, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu

### Community 179 - "PlayerMeleeAttackStateBehaviour"
Cohesion: 0.26
Nodes (6): Animator, AnimatorStateInfo, bool, float, PlayerMeleeAttackStateBehaviour, StateMachineBehaviour

### Community 181 - ".UpdateState"
Cohesion: 0.33
Nodes (3): AnimatorStateMachineDto, AnimatorStateMachineDto, IAnimationStateSink

### Community 188 - "InteractionController"
Cohesion: 0.07
Nodes (24): CancellationToken, InteractionPrompt, UniTask, InteractionCommand, CancellationToken, IEntity, InteractionPrompt, Transform (+16 more)

### Community 200 - "SoulsLike.Entities.Combat"
Cohesion: 0.18
Nodes (9): AnimationClip, CharacterActionId, bool, float, CharacterActionDefinition, ItemId, WeaponMovesetDefinition, SoulsLike.Entities.Combat (+1 more)

### Community 211 - "PlayerHudUiController"
Cohesion: 0.13
Nodes (12): IPlayerHudPresenter, EquipmentComponent, EquipmentLoadout, HealthStats, InteractionController, InventoryComponent, ItemCatalog, PlayerHudUiController (+4 more)

### Community 219 - "ShieldDatabase"
Cohesion: 0.21
Nodes (7): Dictionary, IReadOnlyList, List, ShieldDatabase, float, GameObject, ShieldDefinition

### Community 222 - "Entity"
Cohesion: 0.11
Nodes (12): List, Entity, IEntity, EntityCommand, IContainerBuilder, EntityRegistrationExt, EntityType, IEntityComponent (+4 more)

### Community 226 - "IDisposable"
Cohesion: 0.23
Nodes (6): IAudioSettingsData, IObserver, AudioService, AudioData, AudioSettingsData, IDisposable

### Community 228 - "EnemyHealthUi"
Cohesion: 0.29
Nodes (7): RectTransform, EnemyHealthBarUi, Camera, int, RectTransform, Vector3, EnemyHealthUi

### Community 230 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 232 - "GroundItemVfx"
Cohesion: 0.20
Nodes (9): CancellationToken, float, int, Transform, UniTask, GroundItemVfx, MaterialPropertyBlock, ParticleSystem (+1 more)

### Community 234 - "SoulsLike.Entities.Character.Components.Movement"
Cohesion: 0.18
Nodes (6): LandingType, LocomotionState, MovementMode, SoulsLike.Entities.Character.Ports, SoulsLike.Entities.Character.Components.Movement, SoulsLike.Entities.Character.Components

### Community 235 - "IMovementData"
Cohesion: 0.21
Nodes (9): AnimatorModel, AnimationCurve, LayerMask, IMovementData, MovementData, AnimationCurve, LayerMask, MovementModel (+1 more)

### Community 236 - "TargetingCommand"
Cohesion: 0.33
Nodes (5): Entity, Transform, TargetingCommand, TargetLockNode, ViewEntity

### Community 237 - "EnemyFactory"
Cohesion: 0.20
Nodes (7): bool, EnemySpawnPoint, EnemyEncounterSpawner, EnemyActor, EnemySpawnPoint, GameObject, EnemyFactory

### Community 238 - "DamageResult"
Cohesion: 0.18
Nodes (8): bool, float, HealthStats, long, DamageResult, Entity, IEntityLocator, ApplyDamageCommand

### Community 239 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 241 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.32
Nodes (3): SceneDependency, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 242 - "EnemyBehaviourProfile"
Cohesion: 0.43
Nodes (6): bool, float, int, LayerMask, AiActionRule, EnemyBehaviourProfile

### Community 243 - "EnemyContracts.cs"
Cohesion: 0.32
Nodes (7): Vector3, EnemyActionPhase, EnemyActionStatus, EnemyGoal, EnemyIntent, EnemyIntentKind, EnemyMemory

### Community 244 - ".DisplayItemDetails"
Cohesion: 0.33
Nodes (3): CharacterAttributeStats, ScalingGrade, TMP_Text

### Community 245 - "LockOnUi"
Cohesion: 0.36
Nodes (5): Camera, Vector3, LockOnUi, SoulsLike.Ui.LockOn, RectTransform

### Community 247 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.17
Nodes (7): bool, float, HealthStats, bool, float, HealthStatUpdate, SoulsLike.Entities.Character.Components.Health

### Community 248 - "CharacterCommandExecutionStatus"
Cohesion: 0.33
Nodes (3): CharacterCommandExecutionStatus, JumpRequest, RollRequest

### Community 249 - "GameOrchestrator"
Cohesion: 0.33
Nodes (3): UniTaskVoid, GameOrchestrator, ITickable

### Community 263 - "VContainerExt"
Cohesion: 0.47
Nodes (4): AssetMappingData, IContainerBuilder, VContainerExt, RegistrationBuilder

### Community 269 - "CoreGameOrchestrator"
Cohesion: 0.11
Nodes (11): CharacterFactory, GameState, IGameStateObserver, List, CoreGameOrchestrator, BaseFactory, Character, GameObject (+3 more)

## Knowledge Gaps
- **16 isolated node(s):** `CharacterAttributeStats`, `AttackType`, `IPlayerHudPresenter`, `SwapPhase`, `LandingType` (+11 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **142 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `PlayerController` (3× useful, score=2.930100614)
- `EntityLocator` (3× useful, score=2.929447566)
- `CharacterAudioComponent` (2× useful, score=1.998403944)
- `EnemyAnimationController` (2× useful, score=1.998134458)
- `CoreScope` (2× useful, score=1.930995604)
- `AmbienceData` (2× useful, score=1.867321583)
- `CharacterCommandFactory` (2× useful, score=1.855618286)
- `ICharacterCommand` (2× useful, score=1.85498036)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `HealthComponent`, `TargetingSnapshot`, `AnimatorComponent`, `CharacterAudioComponent`, `EquipmentUiController`, `EquipmentUi`, `.Move`, `.BuildLoadout`, `MonoBehaviour`, `CharacterActionStateMachine`, `HealthModel`, `PlayerController`, `.BeginRootMotionAction`, `InventoryUiController`, `MovementComponent`, `InteractionController`, `.Tick`, `.TryStartAttack`, `IInitializable`, `.Submit`, `IDisposable`, `SoulsLike.Entities.Character.Components.Movement`, `.SetAirborneMotion`, `CharacterCommandExecutionStatus`?**
  _High betweenness centrality (0.226) - this node is a cross-community bridge._
- **Why does `EnemyBrain` connect `EnemyBrain` to `IDisposable`, `.Interrupt`, `IInitializable`, `PlayerController`, `EnemyNavigationMotor`, `SoulsLike.Services`, `EnemyAnimationController`, `GameOrchestrator`, `IGameStateNotifier`?**
  _High betweenness centrality (0.054) - this node is a cross-community bridge._
- **Why does `ItemId` connect `ItemId` to `InventoryData.cs`, `ItemDefinition`, `EquipmentPresentation`, `IInitializable`, `InventoryItemViewData`, `WeaponDefinition`, `InventoryEntry`, `ShieldDatabase`, `AttackComponent`?**
  _High betweenness centrality (0.052) - this node is a cross-community bridge._
- **What connects `CharacterAttributeStats`, `AttackType`, `IPlayerHudPresenter` to the rest of the system?**
  _16 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ItemDefinition` be split into smaller, more focused modules?**
  _Cohesion score 0.1368421052631579 - nodes in this community are weakly interconnected._
- **Should `TargetingSnapshot` be split into smaller, more focused modules?**
  _Cohesion score 0.1396011396011396 - nodes in this community are weakly interconnected._
- **Should `AnimatorStateMachineReceiver` be split into smaller, more focused modules?**
  _Cohesion score 0.08067375886524823 - nodes in this community are weakly interconnected._