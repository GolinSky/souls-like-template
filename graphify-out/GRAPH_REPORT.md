# Graph Report - SoulsLikeTemplate  (2026-08-21)

## Corpus Check
- 270 files · ~48,548 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2378 nodes · 4255 edges · 272 communities (120 shown, 152 thin omitted)
- Extraction: 91% EXTRACTED · 9% INFERRED · 0% AMBIGUOUS · INFERRED: 392 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `4b68a07f`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- ItemDefinition
- TargetingSnapshot
- AnimatorStateMachineReceiver
- PreviewRenderService
- SceneData
- PlayerHudUi
- EquipmentSlotId
- AnimatorComponent
- Character
- GroundItem
- bool
- EquipmentUiController
- EquipmentUi
- AudioService
- EquipmentComponent
- CharacterRuntime.cs
- InventorySlotUI
- .StartSwap
- InventoryEntry
- EnemyAnimationController
- CameraService
- PauseNavigationUiController
- SoulsLike
- OnGuiFpsCounter
- CharacterActions
- PauseMenuUiController
- AmbienceService
- Animator
- AttackComponent
- InventoryData.cs
- CustomButtonToggle
- EnemyBrain
- SpeedMultiplierKey
- ITimer
- HealthModel
- IInitializable
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
- SceneService
- .HandleLockOnInput
- EquipmentSlotUI
- InventoryUiController
- Vector2
- BaseUi
- MovementComponent
- IContainerBuilder
- CustomButtonMapping
- MonoBehaviour
- float
- CoroutineService
- IUniqueIdGenerator
- Quaternion
- MainMenuUiController
- SaveService
- ICustomButton
- InventoryItemSO
- EquipmentSlotHud
- .Tick
- InventoryViewStateController
- .TryStartAttack
- InventoryComponent
- SceneType
- CustomButtonEditor
- WeaponDefinition
- CharacterRuntime
- BasePopup
- .TryAdapt
- AnimatorRootMotionRelay
- .Select
- .Read
- StateMachineName
- SceneReferencePropertyDrawer
- InteractionController
- GameObject
- Tween
- ItemId
- List
- List
- CoreGameOrchestrator
- InteractionCommand
- .Submit
- Component
- EquipmentLoadout
- .ShowAcquisition
- ICharacterActionExecutor
- ConsumableDatabase
- IEntityLocator
- PlayerController.cs
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
- SoulsLike.Entities.Character.Components.Health
- bool
- float
- Vector2
- .Open
- Quaternion
- Vector2
- Vector3
- BufferedCharacterAction
- EquipmentSwapPhase
- string
- ICameraService
- IInputService
- float
- DamageRequest
- DamageResult
- InteractionUiController
- DeterministicEnemyActionSelector
- AnimatorStateMachineDto
- AttackRequest
- CharacterActionStateId
- CharacterInputBatch
- EquipmentPresentation
- CharacterAttributeStats
- Camera
- InputService
- Vector3
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
- EquipmentComponent
- .ApplyAnimationMovement
- int
- CharacterActionStateMachine
- AnimatorComponent
- int
- Inject
- Transform
- GroundItemCollectionCommand
- CharacterActionStateId
- CharacterInputBatch
- CharacterCommandFactory
- IContainerBuilder
- SoulsLike.Services
- EquipmentSlotGroup
- IAnimationStateSink
- Inject
- IRootMotionSink
- Character
- LandingType
- Transform
- Vector2
- Vector3
- ITimer
- StateMachineName
- PlayerMeleeAttackStateBehaviour
- Character
- AnimatorStateMachineDto
- AttackType
- .BeginRootMotionAction
- bool
- float
- HandMode
- RuntimeAnimatorController
- IInteractable
- bool
- EntityType
- AttackType
- EquipmentLoadout
- InventoryComponent
- EquipmentLoadout
- EquippedItemContext
- CharacterCommandExecutionStatus
- IInputService
- Color
- LandingType
- EnemySpawnPoint
- Action
- IReadOnlyCollection
- RectTransform
- MPImage
- InventoryChange
- Collider
- CombatProfile
- EquipmentSlotId
- IEntity
- HealthStats
- PlayerHudUiController
- InventoryComponent
- InventoryEntry
- InventoryEntryId
- IReadOnlyList
- ItemDatabase
- WeaponDefinition
- WeaponRuntime
- ShieldDatabase
- ICameraService
- .TryStartRoll
- IDisposable
- float
- Inject
- ITimer
- LandingType
- Transform
- Vector2
- Vector3
- MovementModel
- CharacterFactory
- GroundItemVfx
- SoulsLike.Entities.Character.Ports
- SoulsLike.Entities.Character.Components.Movement
- BaseComponent
- TargetingCommand
- EnemyEncounterSpawner
- DamageResult
- MovementModel
- ICameraService
- SoulsLike.Services.Scenes.Data
- EnemyBehaviourProfile
- EnemyContracts.cs
- .DisplayItemDetails
- .OnItemFocused
- HealthStats
- HealthStatUpdate
- SpeedMultiplierKey.cs
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
- Character
- DamageRequest
- DamageResult
- GameState
- HealthComponent
- HealthModel
- IGameStateNotifier
- ITargetingService
- LayerMask

## God Nodes (most connected - your core abstractions)
1. `Character` - 64 edges
2. `AnimatorComponent` - 59 edges
3. `MovementComponent` - 56 edges
4. `EnemyBrain` - 38 edges
5. `InventoryUiController` - 37 edges
6. `EquipmentUi` - 36 edges
7. `AmbienceService` - 36 edges
8. `EnemyAnimationController` - 35 edges
9. `EquipmentUiController` - 33 edges
10. `EquipmentSlotUI` - 33 edges

## Surprising Connections (you probably didn't know these)
- `EnemyAnimationController` --references--> `EnemyActionPhase`  [EXTRACTED]
  Assets/Scripts/Entities/Enemy/EnemyAnimationController.cs → Assets/Scripts/Entities/Enemy/EnemyContracts.cs
- `EnemyAnimationController` --references--> `EnemyActionStatus`  [EXTRACTED]
  Assets/Scripts/Entities/Enemy/EnemyAnimationController.cs → Assets/Scripts/Entities/Enemy/EnemyContracts.cs
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `HealthComponent` --references--> `HealthModel`  [EXTRACTED]
  Assets/Scripts/Components/Health/HealthComponent.cs → Assets/Scripts/Components/Health/HealthModel.cs
- `HealthComponent` --implements--> `IHealthComponent`  [EXTRACTED]
  Assets/Scripts/Components/Health/HealthComponent.cs → Assets/Scripts/Components/Health/IHealthComponent.cs

## Import Cycles
- None detected.

## Communities (272 total, 152 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.15
Nodes (11): AttackType, CharacterAttributeStats, SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character (+3 more)

### Community 1 - "SceneReference"
Cohesion: 0.14
Nodes (7): bool, string, SceneReference, IComparable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.24
Nodes (3): HealthStats, HealthStatUpdate, HealthComponent

### Community 3 - "ItemDefinition"
Cohesion: 0.14
Nodes (12): Dictionary, IReadOnlyList, List, ItemDatabase, float, int, IReadOnlyList, List (+4 more)

### Community 4 - "TargetingSnapshot"
Cohesion: 0.10
Nodes (14): EntityType, Vector3, TargetingSnapshot, Collider, EntityType, IEntity, List, RaycastHit (+6 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (18): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+10 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.07
Nodes (18): GameObject, LayerMask, ILayerService, LayerName, GameObject, LayerMask, LayerService, IPreviewRenderService (+10 more)

### Community 7 - "SceneData"
Cohesion: 0.22
Nodes (5): Scene, SerializedDictionary, SceneData, Scene, SceneModel

### Community 8 - "PlayerHudUi"
Cohesion: 0.16
Nodes (10): bool, float, HealthStats, IPlayerHudPresenter, PlayerHudUi, StatBar, EquipmentSlotHud, MPImage (+2 more)

### Community 9 - "EquipmentSlotId"
Cohesion: 0.16
Nodes (8): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode, EquipmentSlotId

### Community 10 - "AnimatorComponent"
Cohesion: 0.14
Nodes (11): AnimationEvent, Animator, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, AnimatorComponent, EquipmentLoadout, HandMode (+3 more)

### Community 11 - "Character"
Cohesion: 0.08
Nodes (21): AnimatorComponent, AttackComponent, EquipmentComponent, float, HealthStats, Inject, InventoryComponent, ItemCatalog (+13 more)

### Community 12 - "GroundItem"
Cohesion: 0.17
Nodes (12): CancellationToken, Collider, IEntity, int, InteractionPrompt, ItemId, string, Transform (+4 more)

### Community 14 - "EquipmentUiController"
Cohesion: 0.09
Nodes (15): EquipmentSlotId, HandMode, InventoryEntry, InventoryEntryId, EquipmentLoadout, EquipmentSlotChange, EquippedItemContext, EquipmentSlotId (+7 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.12
Nodes (12): CharacterAttributeStats, Dictionary, EquipmentSlotId, GameObject, IEquipmentPresenter, Image, int, List (+4 more)

### Community 16 - "AudioService"
Cohesion: 0.06
Nodes (17): IAudioSettingsData, IObserver, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, MusicType (+9 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.12
Nodes (12): EquipmentSlotGroup, EquipmentSlotId, HandMode, IEquipmentLoadoutSink, Inject, InventoryChange, InventoryEntry, InventoryEntryId (+4 more)

### Community 18 - "CharacterRuntime.cs"
Cohesion: 0.15
Nodes (17): float, Vector2, AttackIntent, AttackRequest, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind, CharacterCommand (+9 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (12): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+4 more)

### Community 20 - ".StartSwap"
Cohesion: 0.18
Nodes (11): EquipmentSlotGroup, AnimatorStateMachineDto, CharacterCommandExecutionStatus, EquipmentComponent, EquipmentLoadout, EquipmentSlotGroup, EquippedItemContext, EquipmentSwapCoordinator (+3 more)

### Community 21 - "InventoryEntry"
Cohesion: 0.18
Nodes (8): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "EnemyAnimationController"
Cohesion: 0.10
Nodes (18): CharacterActionId, bool, float, CharacterActionDefinition, Animator, AnimatorStateInfo, bool, float (+10 more)

### Community 23 - "CameraService"
Cohesion: 0.11
Nodes (10): bool, float, Inject, long, Vector3, CameraService, CinemachineCamera, CinemachineThirdPersonFollow (+2 more)

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.09
Nodes (9): IEquipmentRoute, Action, IReadOnlyCollection, IInventoryRoute, IPauseNavigationRoute, IPauseNavigationRouteNavigation, IReadOnlyCollection, PauseNavigationUiController (+1 more)

### Community 25 - "SoulsLike"
Cohesion: 0.08
Nodes (17): AssetMappingData, CoreScope, ProjectScope, SharedSceneScope, IKeyValue, KeyValue, Dictionary, List (+9 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 28 - "PauseMenuUiController"
Cohesion: 0.12
Nodes (4): UiController, IPauseMenuPresenter, PauseMenuUi, PauseMenuUiController

### Community 29 - "AmbienceService"
Cohesion: 0.07
Nodes (26): AudioClip, float, IAudioService, IAudioSettingsData, MusicType, SceneType, SfxType, AmbienceService (+18 more)

### Community 31 - "AttackComponent"
Cohesion: 0.14
Nodes (14): AnimatorStateMachineDto, AttackRequest, bool, CombatProfile, float, HandMode, Inject, AttackComponent (+6 more)

### Community 32 - "InventoryData.cs"
Cohesion: 0.21
Nodes (9): HealthData, IHealthData, IReadOnlyList, List, InitialInventoryEntry, InventoryData, Data, Model (+1 more)

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "EnemyBrain"
Cohesion: 0.13
Nodes (12): IReadOnlyList, List, EnemyActionSelector, bool, float, GameState, IGameStateNotifier, int (+4 more)

### Community 36 - "ITimer"
Cohesion: 0.11
Nodes (7): LandingType, ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 37 - "HealthModel"
Cohesion: 0.25
Nodes (4): HealthStats, long, HealthModel, IHealthData

### Community 38 - "IInitializable"
Cohesion: 0.16
Nodes (5): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator, IInitializable

### Community 39 - "StorageRegistry"
Cohesion: 0.20
Nodes (6): Enum, IStorageRegistry, Enum, string, StorageRegistry, SoulsLike.Services.Storage

### Community 40 - "IMovementComponent"
Cohesion: 0.16
Nodes (6): Quaternion, SpeedMultiplierKey, Transform, Vector2, Vector3, IMovementComponent

### Community 41 - "CustomButton"
Cohesion: 0.13
Nodes (8): bool, ColorBlock, Image, SelectionState, Sprite, TMP_Text, CustomButton, Button

### Community 42 - "InventoryEntryId"
Cohesion: 0.14
Nodes (5): string, InventoryEntryId, InteractionPrompt, IEquipmentPresenter, IEquatable

### Community 43 - "IHealthComponent"
Cohesion: 0.21
Nodes (3): HealthStats, HealthStatUpdate, IHealthComponent

### Community 44 - "InventoryItemViewData"
Cohesion: 0.29
Nodes (5): IReadOnlyList, InventoryEntryId, InventoryPrimaryCategory, Sprite, InventoryItemViewData

### Community 46 - "LockOnUiController"
Cohesion: 0.22
Nodes (7): Camera, Vector3, LockOnUi, bool, Camera, LockOnUiController, IPostLateTickable

### Community 47 - "PlayerController"
Cohesion: 0.20
Nodes (8): GameState, IGameStateNotifier, InteractionController, PlayerController, Vector2, IGameStateObserver, ILateTickable, PlayerCharacterInputAdapter

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 50 - "ItemAcquisitionPanel"
Cohesion: 0.17
Nodes (7): float, Sprite, ItemAcquisitionPanel, CanvasGroup, Image, InteractionPrompt, TextMeshProUGUI

### Community 51 - "InventoryUi"
Cohesion: 0.15
Nodes (10): Color, IInventoryPresenter, Image, int, IReadOnlyList, List, Transform, InventoryUi (+2 more)

### Community 52 - "Color"
Cohesion: 0.24
Nodes (7): ColorConverter, JsonReader, JsonSerializer, JsonWriter, Type, Color, JsonConverter

### Community 53 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.15
Nodes (10): BaseEventData, Color, EquipmentSlotId, GameObject, Image, MPImage, PointerEventData, TMP_Text (+2 more)

### Community 56 - "InventoryUiController"
Cohesion: 0.17
Nodes (8): bool, IInputService, InventoryChange, InventoryPrimaryCategory, InventoryUiController, IInventoryPresenter, IInventoryRoute, InventorySubCategory

### Community 58 - "BaseUi"
Cohesion: 0.11
Nodes (8): bool, CanvasGroup, Transform, BaseUi, IBaseUi, IPauseNavigationPresenter, PauseNavigationUi, SoulsLike.Ui.Base

### Community 59 - "MovementComponent"
Cohesion: 0.15
Nodes (8): MovementComponent, Dictionary, IMovementComponent, IMovementPresentationSink, LocomotionState, MovementMode, RaycastHit, SpeedMultiplierKey

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 62 - "MonoBehaviour"
Cohesion: 0.40
Nodes (4): Transform, TargetLockAnchorType, TargetLockNode, MonoBehaviour

### Community 64 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 65 - "IUniqueIdGenerator"
Cohesion: 0.33
Nodes (3): IUniqueIdGenerator, UniqueIdGenerator, SoulsLike.Services.IdGeneration

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
Cohesion: 0.20
Nodes (8): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud

### Community 73 - "InventoryViewStateController"
Cohesion: 0.39
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 74 - ".TryStartAttack"
Cohesion: 0.15
Nodes (5): AnimatorStateMachineDto, ItemId, AnimatorStateMachineDto, AttackRequest, EquippedItemContext

### Community 75 - "InventoryComponent"
Cohesion: 0.17
Nodes (7): Inject, InventoryEntry, InventoryEntryId, IReadOnlyList, InventoryComponent, InventoryData, InventoryModel

### Community 76 - "SceneType"
Cohesion: 0.18
Nodes (7): UniTask, UniTaskVoid, GameOrchestrator, SceneType, UniTask, ISceneService, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "WeaponDefinition"
Cohesion: 0.10
Nodes (20): AnimationProfile, float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot, ScalingGrade (+12 more)

### Community 79 - "CharacterRuntime"
Cohesion: 0.21
Nodes (6): bool, CharacterRuntime, MovementGate, MovementGateReason, MovementPolicy, CharacterActionStateMachine

### Community 80 - "BasePopup"
Cohesion: 0.07
Nodes (20): Action, GenericPopupService, IGenericPopupService, IUiService, UiService, AcceptPopup, Button, AlertPopup (+12 more)

### Community 81 - ".TryAdapt"
Cohesion: 0.20
Nodes (6): AnimatorStateMachineDto, CharacterAnimationAdapter, UnityCharacterClock, ICharacterClock, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Runtime

### Community 82 - "AnimatorRootMotionRelay"
Cohesion: 0.17
Nodes (7): Animator, bool, string, AnimatorRootMotionRelay, Quaternion, Vector3, IRootMotionSink

### Community 84 - ".Read"
Cohesion: 0.11
Nodes (11): PlayerCharacterInputAdapter, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, SoulsLike.Entities.Character.Input, float, HeavyAttackGestureResolver (+3 more)

### Community 86 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 87 - "InteractionController"
Cohesion: 0.15
Nodes (9): bool, Collider, float, int, InteractionPrompt, InteractionController, CancellationTokenSource, IEntityLocator (+1 more)

### Community 90 - "ItemId"
Cohesion: 0.24
Nodes (7): IReadOnlyList, Sprite, ItemCatalog, ItemId, ItemType, CharacterAttributeStats, InventoryEntry

### Community 93 - "CoreGameOrchestrator"
Cohesion: 0.11
Nodes (6): List, CoreGameOrchestrator, ICoreGameOrchestrator, GameState, IGameStateNotifier, IGameStateObserver

### Community 94 - "InteractionCommand"
Cohesion: 0.29
Nodes (3): InteractionCommand, SoulsLike.Interactions, Entity

### Community 100 - "ConsumableDatabase"
Cohesion: 0.18
Nodes (7): Dictionary, IReadOnlyList, List, ConsumableDatabase, float, ConsumableDefinition, ItemUseType

### Community 101 - "IEntityLocator"
Cohesion: 0.13
Nodes (14): IEntityLocator, Collider, float, int, ItemId, long, MeleeHitboxController, AttackComponent (+6 more)

### Community 102 - "PlayerController.cs"
Cohesion: 0.29
Nodes (3): SoulsLike.Services.Targeting, SoulsLike.Services.CameraService, SoulsLike.Ui.LockOn

### Community 104 - "DamageRequest"
Cohesion: 0.22
Nodes (7): float, int, long, Vector3, DamageRequest, ItemId, ApplyDamageRequest

### Community 115 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.22
Nodes (5): SoulsLike.Entities.Character.Components.Health, SoulsLike.Entities.Enemy, SoulsLike.Entities.BaseEntity.EntityCommands, SoulsLike.Entities.BaseEntity, SoulsLike.Entities.Combat

### Community 131 - "InteractionUiController"
Cohesion: 0.18
Nodes (8): IInteractionPresenter, string, InteractionUi, InteractionUiController, SoulsLike.Ui.Interaction, InteractionController, TMP_Text, UiController

### Community 132 - "DeterministicEnemyActionSelector"
Cohesion: 0.17
Nodes (10): CharacterActionId, Dictionary, IReadOnlyList, List, DeterministicEnemyActionSelector, CharacterActionId, EnemyActionCandidate, EnemyActionSelectionContext (+2 more)

### Community 137 - "EquipmentPresentation"
Cohesion: 0.17
Nodes (10): bool, GameObject, Inject, Quaternion, Transform, Vector3, EquipmentPresentation, float (+2 more)

### Community 140 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 141 - "Vector3"
Cohesion: 0.21
Nodes (6): JsonReader, JsonSerializer, JsonWriter, Type, Vector3Converter, Vector3

### Community 144 - "AddressableAssetService"
Cohesion: 0.07
Nodes (18): GameObject, AddressableAssetService, IAssetService, LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory (+10 more)

### Community 145 - "EnemyNavigationMotor"
Cohesion: 0.17
Nodes (7): bool, float, Quaternion, Vector3, EnemyNavigationMotor, CharacterController, NavMeshAgent

### Community 149 - "ScriptableObject"
Cohesion: 0.22
Nodes (6): RuntimeAnimatorController, AnimationProfile, CombatProfile, float, AudioData, ScriptableObject

### Community 150 - "EnemyActor"
Cohesion: 0.21
Nodes (8): Animator, Entity, HealthData, Inject, IReadOnlyList, LifetimeScope, Vector3, EnemyActor

### Community 151 - "EntityLocator"
Cohesion: 0.21
Nodes (7): Collider, Dictionary, EntityType, IEntity, List, RaycastHit, EntityLocator

### Community 155 - ".ApplyAnimationMovement"
Cohesion: 0.29
Nodes (3): Quaternion, Vector3, Quaternion

### Community 157 - "CharacterActionStateMachine"
Cohesion: 0.21
Nodes (8): CharacterActionStateMachine, CharacterActionStateId, CharacterAnimationSignal, CharacterCommand, CharacterCommandBuffer, CharacterCommandExecutionResult, CharacterControlFrame, ICharacterActionExecutor

### Community 162 - "GroundItemCollectionCommand"
Cohesion: 0.18
Nodes (6): InventoryComponent, ItemCatalog, Transform, GroundItemCollectionCommand, IPlayerHudPresenter, SoulsLike.Ui.PlayerHud

### Community 167 - "SoulsLike.Services"
Cohesion: 0.18
Nodes (5): IContainerBuilder, MainMenuScope, SoulsLike.Services, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu

### Community 179 - "PlayerMeleeAttackStateBehaviour"
Cohesion: 0.33
Nodes (6): Animator, AnimatorStateInfo, bool, float, PlayerMeleeAttackStateBehaviour, StateMachineBehaviour

### Community 188 - "IInteractable"
Cohesion: 0.16
Nodes (11): CancellationToken, InteractionPrompt, UniTask, CancellationToken, IEntity, InteractionPrompt, Transform, UniTask (+3 more)

### Community 190 - "EntityType"
Cohesion: 0.24
Nodes (6): IContainerBuilder, EntityRegistrationExt, EntityType, Inject, IViewEntity, ViewEntity

### Community 200 - "EnemySpawnPoint"
Cohesion: 0.20
Nodes (8): AnimationClip, HealthData, Transform, Vector3, EnemySpawnPoint, ItemId, WeaponMovesetDefinition, RuntimeAnimatorController

### Community 211 - "PlayerHudUiController"
Cohesion: 0.14
Nodes (11): EquipmentComponent, EquipmentLoadout, HealthStats, InteractionController, InventoryComponent, ItemCatalog, PlayerHudUiController, EquipmentSlotChange (+3 more)

### Community 219 - "ShieldDatabase"
Cohesion: 0.13
Nodes (13): Dictionary, IReadOnlyList, List, ShieldDatabase, float, GameObject, ShieldDefinition, Dictionary (+5 more)

### Community 222 - "IDisposable"
Cohesion: 0.17
Nodes (6): List, Entity, IEntity, EntityCommand, IEntityComponent, IDisposable

### Community 231 - "CharacterFactory"
Cohesion: 0.33
Nodes (4): GameObject, string, CharacterFactory, BaseFactory

### Community 232 - "GroundItemVfx"
Cohesion: 0.20
Nodes (9): CancellationToken, float, int, Transform, UniTask, GroundItemVfx, MaterialPropertyBlock, ParticleSystem (+1 more)

### Community 233 - "SoulsLike.Entities.Character.Ports"
Cohesion: 0.25
Nodes (4): IAnimationStateSink, EquipmentLoadout, IEquipmentLoadoutSink, SoulsLike.Entities.Character.Ports

### Community 234 - "SoulsLike.Entities.Character.Components.Movement"
Cohesion: 0.25
Nodes (4): LandingType, LocomotionState, MovementMode, SoulsLike.Entities.Character.Components.Movement

### Community 235 - "BaseComponent"
Cohesion: 0.28
Nodes (5): AnimatorModel, BaseComponent, IComponent, SoulsLike.Entities.Character.Components, Model

### Community 236 - "TargetingCommand"
Cohesion: 0.22
Nodes (9): Entity, IEntityLocator, ApplyDamageCommand, Entity, Transform, TargetingCommand, EntityCommand, TargetLockNode (+1 more)

### Community 237 - "EnemyEncounterSpawner"
Cohesion: 0.28
Nodes (5): bool, Inject, EnemyEncounterSpawner, GameObject, EnemyFactory

### Community 238 - "DamageResult"
Cohesion: 0.25
Nodes (5): bool, float, HealthStats, long, DamageResult

### Community 239 - "MovementModel"
Cohesion: 0.31
Nodes (7): AnimationCurve, LayerMask, IMovementData, MovementData, AnimationCurve, LayerMask, MovementModel

### Community 240 - "ICameraService"
Cohesion: 0.20
Nodes (3): Transform, ICameraService, Ray

### Community 241 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.32
Nodes (3): SceneDependency, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 242 - "EnemyBehaviourProfile"
Cohesion: 0.43
Nodes (6): bool, float, int, LayerMask, AiActionRule, EnemyBehaviourProfile

### Community 243 - "EnemyContracts.cs"
Cohesion: 0.38
Nodes (6): Vector3, EnemyActionPhase, EnemyActionStatus, EnemyIntent, EnemyIntentKind, EnemyMemory

### Community 244 - ".DisplayItemDetails"
Cohesion: 0.33
Nodes (3): CharacterAttributeStats, ScalingGrade, TMP_Text

### Community 247 - "HealthStats"
Cohesion: 0.50
Nodes (3): bool, float, HealthStats

### Community 248 - "HealthStatUpdate"
Cohesion: 0.50
Nodes (3): bool, float, HealthStatUpdate

## Knowledge Gaps
- **13 isolated node(s):** `CharacterAttributeStats`, `SpeedMultiplierKey`, `AttackType`, `IPlayerHudPresenter`, `SwapPhase` (+8 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **152 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `EntityLocator` (3× useful, score=2.937309801)
- `PlayerController` (2× useful, score=1.935386642)
- `AmbienceData` (2× useful, score=1.872333218)
- `CharacterCommandFactory` (2× useful, score=1.860598511)
- `ICharacterCommand` (2× useful, score=1.859958872)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `HealthComponent`, `AnimatorComponent`, `EquipmentUiController`, `EquipmentComponent`, `.ApplyAnimationMovement`, `CharacterActionStateMachine`, `GroundItemCollectionCommand`, `IInitializable`, `PlayerController`, `.HandleLockOnInput`, `.BeginRootMotionAction`, `InventoryUiController`, `MovementComponent`, `MonoBehaviour`, `.Tick`, `.TryStartAttack`, `InventoryComponent`, `InteractionController`, `.Submit`, `IEntityLocator`, `CharacterFactory`, `.SetAirborneMotion`, `SoulsLike.Entities.Character.Components.Health`?**
  _High betweenness centrality (0.151) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `ITimer`, `SaveService`, `IInitializable`, `.Tick`, `SoulsLike.Entities.Character.Components.Movement`, `.SetAirborneMotion`, `Vector3`, `MovementModel`, `BasePopup`, `EnemyNavigationMotor`, `.Move`, `AmbienceService`, `.Read`, `.HandleLockOnInput`, `.ApplyAnimationMovement`, `.TryStartRoll`, `AttackComponent`?**
  _High betweenness centrality (0.081) - this node is a cross-community bridge._
- **Why does `AmbienceService` connect `AmbienceService` to `IInitializable`, `SceneType`, `AudioService`, `CameraService`, `IDisposable`?**
  _High betweenness centrality (0.058) - this node is a cross-community bridge._
- **What connects `CharacterAttributeStats`, `SpeedMultiplierKey`, `AttackType` to the rest of the system?**
  _13 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.1437908496732026 - nodes in this community are weakly interconnected._
- **Should `ItemDefinition` be split into smaller, more focused modules?**
  _Cohesion score 0.1368421052631579 - nodes in this community are weakly interconnected._
- **Should `TargetingSnapshot` be split into smaller, more focused modules?**
  _Cohesion score 0.10084033613445378 - nodes in this community are weakly interconnected._