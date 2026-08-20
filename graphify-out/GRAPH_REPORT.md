# Graph Report - SoulsLikeTemplate  (2026-08-20)

## Corpus Check
- 238 files · ~42,573 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 2101 nodes · 3650 edges · 230 communities (94 shown, 136 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 303 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `ec67bb39`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- ItemDefinition
- IDisposable
- AnimatorStateMachineReceiver
- PreviewRenderService
- SceneType
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
- MonoBehaviour
- InventoryEntry
- UiService
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
- EquipmentSlotChange
- SpeedMultiplierKey
- ITimer
- SoulsLike.Entities.Character.Components.Health
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
- ColorConverter
- SceneService
- ICameraService
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
- GameOrchestrator
- CustomButtonEditor
- WeaponDefinition
- MovementGate
- BasePopup
- SoulsLike.Entities.Character.Runtime
- AnimatorRootMotionRelay
- .ShowPicker
- .Read
- StateMachineName
- SoulsLike.Services.Scenes.Data
- .DisplayItemDetails
- GameObject
- Tween
- ItemId
- List
- List
- CoreGameOrchestrator
- InteractionPrompt
- .Submit
- Component
- EquipmentLoadout
- .ShowAcquisition
- ICharacterActionExecutor
- ConsumableDatabase
- CharacterRuntime
- CoreScope.cs
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
- LayerMask
- bool
- float
- Vector2
- .OnItemFocused
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
- SpeedMultiplierKey.cs
- bool
- AnimatorStateMachineDto
- AttackRequest
- CharacterActionStateId
- CharacterInputBatch
- WeaponDatabase
- CharacterAttributeStats
- Camera
- SoulsLike.Services
- AttackComponent.cs
- ICharacterCommand
- IEquipmentCommandReceiver
- AddressableAssetService
- SceneReferencePropertyDrawer
- float
- SharedSceneScope
- AnimatorControllerParameterType
- ScriptableObject
- Inject
- Transform
- AnimatorComponent
- Tween
- EquipmentComponent
- Quaternion
- int
- CharacterActionStateMachine
- AnimatorComponent
- int
- Inject
- Transform
- SoulsLike.Interactions
- CharacterActionStateId
- CharacterInputBatch
- CharacterCommandFactory
- IContainerBuilder
- Vector3
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
- Vector2
- Character
- AnimatorStateMachineDto
- AttackType
- AttackType
- bool
- float
- HandMode
- RuntimeAnimatorController
- InteractionController
- bool
- HealthStatUpdate
- AttackType
- EquipmentLoadout
- InventoryComponent
- EquipmentLoadout
- EquippedItemContext
- CharacterCommandExecutionStatus
- IInputService
- Color
- LandingType
- Vector2
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
- ICameraService
- GameObject
- float
- Inject
- ITimer
- LandingType
- Transform
- Vector2
- Vector3
- MovementModel
- Character

## God Nodes (most connected - your core abstractions)
1. `Character` - 63 edges
2. `AnimatorComponent` - 59 edges
3. `MovementComponent` - 56 edges
4. `InventoryUiController` - 37 edges
5. `EquipmentUi` - 36 edges
6. `AmbienceService` - 36 edges
7. `SoulsLike.Items` - 35 edges
8. `EquipmentUiController` - 33 edges
9. `EquipmentSlotUI` - 33 edges
10. `InventorySlotUI` - 31 edges

## Surprising Connections (you probably didn't know these)
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `GroundItemCollectionCommand` --references--> `Character`  [EXTRACTED]
  Assets/Scripts/Entities/BaseEntity/EntityCommands/GroundItemCollectionCommand.cs → Assets/Scripts/Entities/Character/Character.cs
- `GroundItemCollectionCommand` --references--> `PlayerHudUiController`  [EXTRACTED]
  Assets/Scripts/Entities/BaseEntity/EntityCommands/GroundItemCollectionCommand.cs → Assets/Scripts/Ui/PlayerHud/PlayerHudUiController.cs
- `InteractionController` --references--> `Character`  [EXTRACTED]
  Assets/Scripts/Interactions/InteractionController.cs → Assets/Scripts/Entities/Character/Character.cs
- `EquipmentUiController` --references--> `Character`  [EXTRACTED]
  Assets/Scripts/Ui/Equipment/EquipmentUiController.cs → Assets/Scripts/Entities/Character/Character.cs

## Import Cycles
- None detected.

## Communities (230 total, 136 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.17
Nodes (9): CharacterAttributeStats, SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character, SoulsLike.Ui.Inventory (+1 more)

### Community 1 - "SceneReference"
Cohesion: 0.16
Nodes (7): bool, string, SceneReference, IComparable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.16
Nodes (7): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult

### Community 3 - "ItemDefinition"
Cohesion: 0.15
Nodes (11): Dictionary, IReadOnlyList, List, ItemDatabase, float, int, IReadOnlyList, List (+3 more)

### Community 4 - "IDisposable"
Cohesion: 0.07
Nodes (20): List, Entity, IEntity, EntityCommand, Collider, Dictionary, RaycastHit, EntityLocator (+12 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (19): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+11 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.07
Nodes (18): GameObject, LayerMask, ILayerService, LayerName, GameObject, LayerMask, LayerService, IPreviewRenderService (+10 more)

### Community 7 - "SceneType"
Cohesion: 0.21
Nodes (6): Scene, SerializedDictionary, SceneData, Scene, SceneModel, SceneType

### Community 8 - "PlayerHudUi"
Cohesion: 0.17
Nodes (10): bool, float, HealthStats, PlayerHudUi, StatBar, Color, EquipmentSlotHud, MPImage (+2 more)

### Community 9 - "EquipmentSlotId"
Cohesion: 0.20
Nodes (7): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode

### Community 10 - "AnimatorComponent"
Cohesion: 0.11
Nodes (15): AnimationEvent, Animator, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, AnimatorComponent, EquipmentLoadout, BaseComponent (+7 more)

### Community 11 - "Character"
Cohesion: 0.10
Nodes (17): AnimatorComponent, EquipmentComponent, float, HealthStats, InventoryComponent, ItemCatalog, Character, AttackComponent (+9 more)

### Community 12 - "GroundItem"
Cohesion: 0.07
Nodes (26): InventoryComponent, ItemCatalog, Transform, GroundItemCollectionCommand, CancellationToken, Collider, IEntity, int (+18 more)

### Community 14 - "EquipmentUiController"
Cohesion: 0.10
Nodes (12): HandMode, InventoryEntry, EquipmentLoadout, EquippedItemContext, EquipmentSlotId, IInputService, InventoryChange, InventoryEntry (+4 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.13
Nodes (9): Dictionary, GameObject, IEquipmentPresenter, Image, int, List, TMP_Text, Transform (+1 more)

### Community 16 - "AudioService"
Cohesion: 0.06
Nodes (18): IAudioSettingsData, IObserver, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, MusicType (+10 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.11
Nodes (13): EquipmentSlotGroup, EquipmentSlotId, HandMode, IEquipmentLoadoutSink, Inject, InventoryChange, InventoryEntry, InventoryEntryId (+5 more)

### Community 18 - "CharacterRuntime.cs"
Cohesion: 0.18
Nodes (13): float, Vector2, AttackIntent, AttackRequest, CharacterCommand, CharacterCommandBuffer, CharacterCommandKind, CharacterControlFrame (+5 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.12
Nodes (12): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+4 more)

### Community 20 - "MonoBehaviour"
Cohesion: 0.07
Nodes (25): BaseComponent, IComponent, bool, EquipmentSlotGroup, GameObject, Inject, Quaternion, Transform (+17 more)

### Community 21 - "InventoryEntry"
Cohesion: 0.18
Nodes (8): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "UiService"
Cohesion: 0.20
Nodes (7): IUiService, UiService, BaseUi, Canvas, Inject, Transform, UiFactory

### Community 23 - "CameraService"
Cohesion: 0.13
Nodes (6): CameraService, Camera, CinemachineCamera, CinemachineThirdPersonFollow, Ease, Tween

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.10
Nodes (9): IEquipmentRoute, Action, IReadOnlyCollection, IInventoryRoute, IPauseNavigationRoute, IPauseNavigationRouteNavigation, IReadOnlyCollection, PauseNavigationUiController (+1 more)

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 28 - "PauseMenuUiController"
Cohesion: 0.13
Nodes (4): ICoreGameOrchestrator, IPauseMenuPresenter, PauseMenuUi, PauseMenuUiController

### Community 29 - "AmbienceService"
Cohesion: 0.08
Nodes (23): AudioClip, float, IAudioService, IAudioSettingsData, MusicType, SceneType, SfxType, AmbienceService (+15 more)

### Community 31 - "AttackComponent"
Cohesion: 0.17
Nodes (11): AnimatorStateMachineDto, AttackRequest, bool, CombatProfile, float, HandMode, Inject, AttackComponent (+3 more)

### Community 32 - "InventoryData.cs"
Cohesion: 0.21
Nodes (9): HealthData, IHealthData, IReadOnlyList, List, InitialInventoryEntry, InventoryData, Data, Model (+1 more)

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "EquipmentSlotChange"
Cohesion: 0.50
Nodes (3): EquipmentSlotId, InventoryEntryId, EquipmentSlotChange

### Community 36 - "ITimer"
Cohesion: 0.12
Nodes (6): ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 37 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.16
Nodes (8): bool, float, DamageResult, HealthModel, bool, float, HealthStats, SoulsLike.Entities.Character.Components.Health

### Community 38 - "IInitializable"
Cohesion: 0.16
Nodes (5): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator, IInitializable

### Community 39 - "StorageRegistry"
Cohesion: 0.20
Nodes (6): Enum, IStorageRegistry, Enum, string, StorageRegistry, SoulsLike.Services.Storage

### Community 40 - "IMovementComponent"
Cohesion: 0.07
Nodes (19): AnimatorModel, Quaternion, SpeedMultiplierKey, Transform, Vector2, Vector3, IMovementComponent, LandingType (+11 more)

### Community 41 - "CustomButton"
Cohesion: 0.13
Nodes (8): bool, ColorBlock, Image, SelectionState, Sprite, TMP_Text, CustomButton, Button

### Community 42 - "InventoryEntryId"
Cohesion: 0.18
Nodes (3): string, InventoryEntryId, IEquipmentPresenter

### Community 43 - "IHealthComponent"
Cohesion: 0.18
Nodes (5): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, IHealthComponent

### Community 44 - "InventoryItemViewData"
Cohesion: 0.15
Nodes (9): CharacterAttributeStats, EquipmentSlotId, EquipmentSlotId, InventoryEntryId, InventoryPrimaryCategory, Sprite, InventoryItemViewData, InventoryEntry (+1 more)

### Community 46 - "LockOnUiController"
Cohesion: 0.15
Nodes (10): Camera, RectTransform, Transform, Vector3, LockOnUi, bool, Camera, LockOnUiController (+2 more)

### Community 47 - "PlayerController"
Cohesion: 0.14
Nodes (9): Character, IInputService, PlayerController, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable, ITargetingService (+1 more)

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 50 - "ItemAcquisitionPanel"
Cohesion: 0.17
Nodes (7): float, Sprite, ItemAcquisitionPanel, InteractionPrompt, CanvasGroup, Image, TextMeshProUGUI

### Community 51 - "InventoryUi"
Cohesion: 0.13
Nodes (10): Color, IInventoryPresenter, Image, int, IReadOnlyList, List, Transform, InventoryUi (+2 more)

### Community 52 - "ColorConverter"
Cohesion: 0.25
Nodes (6): ColorConverter, JsonReader, JsonSerializer, JsonWriter, Type, JsonConverter

### Community 53 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 54 - "ICameraService"
Cohesion: 0.14
Nodes (3): Transform, ICameraService, Ray

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Color, EquipmentSlotId, GameObject, Image, MPImage, PointerEventData (+3 more)

### Community 56 - "InventoryUiController"
Cohesion: 0.18
Nodes (8): bool, IInputService, InventoryChange, InventoryPrimaryCategory, InventoryUiController, IInventoryPresenter, IInventoryRoute, InventorySubCategory

### Community 58 - "BaseUi"
Cohesion: 0.09
Nodes (9): UiController, bool, CanvasGroup, Transform, BaseUi, IBaseUi, IPauseNavigationPresenter, PauseNavigationUi (+1 more)

### Community 59 - "MovementComponent"
Cohesion: 0.06
Nodes (21): MovementComponent, LandingType, Vector2, IMovementPresentationSink, JsonReader, JsonSerializer, JsonWriter, Type (+13 more)

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 62 - "TargetLockNode"
Cohesion: 0.18
Nodes (8): Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService, TargetingService, SoulsLike.Services.Targeting

### Community 64 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 65 - "IUniqueIdGenerator"
Cohesion: 0.33
Nodes (3): IUniqueIdGenerator, UniqueIdGenerator, SoulsLike.Services.IdGeneration

### Community 67 - "MainMenuUiController"
Cohesion: 0.14
Nodes (6): IMainMenuOrchestrator, IMainMenuPresenter, MainMenuUi, MainMenuUiController, IStartable, UiController

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
Cohesion: 0.20
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 74 - ".TryStartAttack"
Cohesion: 0.15
Nodes (6): ItemId, AttackRequest, CharacterCommandExecutionStatus, EquippedItemContext, JumpRequest, RollRequest

### Community 75 - "InventoryComponent"
Cohesion: 0.26
Nodes (7): Inject, InventoryEntry, InventoryEntryId, IReadOnlyList, InventoryComponent, InventoryData, InventoryModel

### Community 76 - "GameOrchestrator"
Cohesion: 0.22
Nodes (5): UniTask, UniTaskVoid, GameOrchestrator, UniTask, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "WeaponDefinition"
Cohesion: 0.14
Nodes (16): AnimationProfile, float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot, ScalingGrade (+8 more)

### Community 79 - "MovementGate"
Cohesion: 0.43
Nodes (3): MovementGate, MovementGateReason, MovementPolicy

### Community 80 - "BasePopup"
Cohesion: 0.11
Nodes (12): Action, GenericPopupService, IGenericPopupService, AcceptPopup, Button, AlertPopup, Action, Button (+4 more)

### Community 81 - "SoulsLike.Entities.Character.Runtime"
Cohesion: 0.33
Nodes (4): UnityCharacterClock, ICharacterClock, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Runtime

### Community 82 - "AnimatorRootMotionRelay"
Cohesion: 0.09
Nodes (13): AnimatorStateMachineDto, Animator, bool, string, AnimatorRootMotionRelay, AnimatorStateMachineDto, IAnimationStateSink, EquipmentLoadout (+5 more)

### Community 84 - ".Read"
Cohesion: 0.11
Nodes (11): PlayerCharacterInputAdapter, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, SoulsLike.Entities.Character.Input, float, HeavyAttackGestureResolver (+3 more)

### Community 86 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.22
Nodes (4): SceneDependency, ISceneService, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 87 - ".DisplayItemDetails"
Cohesion: 0.33
Nodes (3): CharacterAttributeStats, ScalingGrade, TMP_Text

### Community 90 - "ItemId"
Cohesion: 0.13
Nodes (14): IReadOnlyList, Sprite, ItemCatalog, ItemId, ItemType, Dictionary, IReadOnlyList, List (+6 more)

### Community 93 - "CoreGameOrchestrator"
Cohesion: 0.10
Nodes (9): string, CharacterFactory, List, CoreGameOrchestrator, GameState, IGameStateNotifier, IGameStateObserver, BaseFactory (+1 more)

### Community 99 - "ICharacterActionExecutor"
Cohesion: 0.47
Nodes (3): CharacterCommandExecutionResult, CharacterCommandExecutionStatus, ICharacterActionExecutor

### Community 100 - "ConsumableDatabase"
Cohesion: 0.21
Nodes (7): Dictionary, IReadOnlyList, List, ConsumableDatabase, float, ConsumableDefinition, ItemUseType

### Community 101 - "CharacterRuntime"
Cohesion: 0.16
Nodes (8): AnimatorStateMachineDto, CharacterAnimationAdapter, bool, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind, CharacterRuntime, CharacterActionStateMachine

### Community 102 - "CoreScope.cs"
Cohesion: 0.33
Nodes (3): IContainerBuilder, CoreScope, SoulsLike.Services.CameraService

### Community 104 - "DamageRequest"
Cohesion: 0.40
Nodes (4): float, int, Vector3, DamageRequest

### Community 119 - ".OnItemFocused"
Cohesion: 0.33
Nodes (3): Action, InventoryEntryId, IReadOnlyCollection

### Community 137 - "WeaponDatabase"
Cohesion: 0.17
Nodes (10): Dictionary, IReadOnlyList, List, WeaponDatabase, Dictionary, LayerMask, LayerName, SerializedDictionary (+2 more)

### Community 140 - "SoulsLike.Services"
Cohesion: 0.13
Nodes (11): CharacterActions, IInputService, InputService, IContainerBuilder, MainMenuScope, SoulsLike.Services, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu (+3 more)

### Community 144 - "AddressableAssetService"
Cohesion: 0.07
Nodes (18): GameObject, AddressableAssetService, IAssetService, LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory (+10 more)

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.29
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 147 - "SharedSceneScope"
Cohesion: 0.22
Nodes (6): ProjectScope, SharedSceneScope, IContainerBuilder, LifetimeScope, OnGuiFpsCounter, PreviewRenderService

### Community 149 - "ScriptableObject"
Cohesion: 0.22
Nodes (6): RuntimeAnimatorController, AnimationProfile, CombatProfile, float, AudioData, ScriptableObject

### Community 157 - "CharacterActionStateMachine"
Cohesion: 0.23
Nodes (8): CharacterActionStateMachine, CharacterActionStateId, CharacterAnimationSignal, CharacterCommand, CharacterCommandBuffer, CharacterCommandExecutionResult, CharacterControlFrame, ICharacterActionExecutor

### Community 162 - "SoulsLike.Interactions"
Cohesion: 0.18
Nodes (4): IPlayerHudPresenter, SoulsLike.Entities.BaseEntity.EntityCommands, SoulsLike.Ui.PlayerHud, SoulsLike.Interactions

### Community 188 - "InteractionController"
Cohesion: 0.08
Nodes (24): CancellationToken, InteractionPrompt, UniTask, InteractionCommand, CancellationToken, IEntity, InteractionPrompt, Transform (+16 more)

### Community 190 - "HealthStatUpdate"
Cohesion: 0.50
Nodes (3): bool, float, HealthStatUpdate

### Community 211 - "PlayerHudUiController"
Cohesion: 0.13
Nodes (11): IPlayerHudPresenter, EquipmentComponent, EquipmentLoadout, HealthStats, InventoryComponent, ItemCatalog, PlayerHudUiController, EquipmentSlotChange (+3 more)

## Knowledge Gaps
- **13 isolated node(s):** `SpeedMultiplierKey`, `AttackType`, `CharacterAttributeStats`, `IPlayerHudPresenter`, `SwapPhase` (+8 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **136 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `PlayerController` (2× useful, score=1.979105975)
- `AmbienceData` (2× useful, score=1.914628208)
- `CharacterCommandFactory` (2× useful, score=1.90262842)
- `ICharacterCommand` (2× useful, score=1.901974332)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `SoulsLike.Items`, `HealthComponent`, `AnimatorComponent`, `GroundItem`, `EquipmentUiController`, `EquipmentUi`, `EquipmentComponent`, `MonoBehaviour`, `CharacterActionStateMachine`, `SoulsLike.Interactions`, `IInitializable`, `InventoryUi`, `ICameraService`, `AttackType`, `InventoryUiController`, `MovementComponent`, `InteractionController`, `.Tick`, `.TryStartAttack`, `CoreGameOrchestrator`, `.Submit`, `CharacterRuntime`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.138) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `SaveService`, `IInitializable`, `IMovementComponent`, `.Tick`, `AnimatorComponent`, `.SetAirborneMotion`, `.Read`, `ICameraService`, `UiService`, `AttackComponent`?**
  _High betweenness centrality (0.078) - this node is a cross-community bridge._
- **Why does `CustomButton` connect `CustomButton` to `MainMenuUiController`, `ICustomButton`, `System.Ui.Base`, `BaseUi`, `PauseMenuUiController`, `CustomButtonMapping`?**
  _High betweenness centrality (0.076) - this node is a cross-community bridge._
- **What connects `SpeedMultiplierKey`, `AttackType`, `CharacterAttributeStats` to the rest of the system?**
  _13 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `IDisposable` be split into smaller, more focused modules?**
  _Cohesion score 0.06938020351526364 - nodes in this community are weakly interconnected._
- **Should `AnimatorStateMachineReceiver` be split into smaller, more focused modules?**
  _Cohesion score 0.0782312925170068 - nodes in this community are weakly interconnected._
- **Should `PreviewRenderService` be split into smaller, more focused modules?**
  _Cohesion score 0.07254623044096728 - nodes in this community are weakly interconnected._