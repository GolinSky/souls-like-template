# Graph Report - SoulsLikeTemplate  (2026-08-19)

## Corpus Check
- 228 files · ~40,411 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1986 nodes · 3467 edges · 220 communities (93 shown, 127 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 281 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `e73e0003`
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
- PlayerHudUiController
- EquipmentSlotId
- AnimatorComponent
- Character
- ITimer
- MovementComponent
- EquipmentUiController
- EquipmentUi
- AudioService
- EquipmentComponent
- CharacterRuntime.cs
- InventorySlotUI
- .BuildLoadout
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
- InputService
- SoulsLike.Entities.Character.Components.Health
- IInitializable
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntryId
- MainMenuUiController
- InventoryItemViewData
- AudioClip
- LockOnUiController
- PlayerController
- System.Ui.Base
- .CreateButton
- SoulsLike.Entities.Character.Components.Movement
- InventoryUi
- ColorConverter
- SceneService
- ICameraService
- EquipmentSlotUI
- InventoryUiController
- Vector2
- PauseNavigationUi
- .Move
- IContainerBuilder
- CustomButtonMapping
- TargetLockNode
- float
- CoroutineService
- IUniqueIdGenerator
- .TryStartRoll
- MainMenuUi
- SaveService
- ICustomButton
- InventoryItemSO
- .Open
- .Tick
- InventoryViewStateController
- ItemId
- InventoryComponent
- GameOrchestrator
- CustomButtonEditor
- WeaponDefinition
- MovementGate
- BasePopup
- PlayerCharacterInputAdapter
- AnimatorRootMotionRelay
- .SetGrounded
- .Read
- StateMachineName
- SoulsLike.Services.Scenes.Data
- .DisplayItemDetails
- GameObject
- Tween
- ShieldDatabase
- List
- List
- SoulsLike.Services
- BaseUi
- .UpdateState
- Component
- EquipmentLoadout
- EquipmentPresentation
- ICharacterActionExecutor
- ConsumableDatabase
- CoreGameOrchestrator
- CoreScope.cs
- IComponentMediator
- DamageRequest
- IComponentMediator
- IComponentMediator
- IComponentMediator
- MovementModel
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
- Quaternion
- bool
- InventoryComponent
- .TryStartAttack
- CharacterActionStateId
- CharacterInputBatch
- WeaponDatabase
- IMovementData
- Camera
- SoulsLike.Ui.Base
- AttackType.cs
- ICharacterCommand
- IEquipmentCommandReceiver
- AddressableAssetService
- SceneReferencePropertyDrawer
- float
- SharedSceneScope
- AnimatorControllerParameterType
- ScriptableObject
- CharacterAttributeStats.cs
- Transform
- AnimatorComponent
- Tween
- EquipmentComponent
- IPauseMenuPresenter
- int
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
- SpeedMultiplierKey.cs
- EquipmentSlotGroup
- IAnimationStateSink
- Inject
- IRootMotionSink
- IPauseNavigationRouteNavigation
- LandingType
- Transform
- Vector2
- Vector3
- ITimer
- StateMachineName
- Vector2
- EquipmentPresentation
- AnimatorStateMachineDto
- AttackType
- Vector2
- bool
- float
- HandMode
- RuntimeAnimatorController
- CharacterFactory
- bool
- AttackType
- EquipmentLoadout
- InventoryComponent
- EquipmentLoadout
- EquippedItemContext
- CharacterCommandExecutionStatus
- EquipmentComponent
- EquipmentLoadout
- LandingType
- Vector2
- Action
- IReadOnlyCollection
- RectTransform
- AttackComponent
- AttackRequest
- CharacterAttributeStats
- CombatProfile
- EquipmentSlotId
- EquippedItemContext
- HealthStats
- InventoryChange
- InventoryComponent
- InventoryEntry
- InventoryEntryId
- IReadOnlyList
- ItemDatabase
- WeaponDefinition
- WeaponRuntime
- GameObject
- string

## God Nodes (most connected - your core abstractions)
1. `AnimatorComponent` - 59 edges
2. `Character` - 59 edges
3. `MovementComponent` - 51 edges
4. `InventoryUiController` - 37 edges
5. `EquipmentUi` - 36 edges
6. `AmbienceService` - 36 edges
7. `EquipmentUiController` - 33 edges
8. `EquipmentSlotUI` - 33 edges
9. `EquipmentComponent` - 32 edges
10. `SoulsLike.Items` - 32 edges

## Surprising Connections (you probably didn't know these)
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `CoreScope` --references--> `CameraService`  [EXTRACTED]
  Assets/Scripts/Services/VContainer/CoreScope.cs → Assets/Scripts/Services/CameraService/CameraService.cs
- `MovementModel` --implements--> `IMovementData`  [EXTRACTED]
  Assets/Scripts/Components/Movement/MovementModel.cs → Assets/Scripts/Components/Movement/MovementData.cs
- `Character` --references--> `AttackComponent`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Components/Attack/AttackComponent.cs
- `Character` --references--> `EquipmentComponent`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Components/Equipment/EquipmentComponent.cs

## Import Cycles
- None detected.

## Communities (220 total, 127 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.17
Nodes (9): SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character, SoulsLike.Entities.Character.Components.Attack, SoulsLike.Ui.Inventory (+1 more)

### Community 1 - "SceneReference"
Cohesion: 0.16
Nodes (7): bool, string, SceneReference, IComparable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.09
Nodes (12): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+4 more)

### Community 3 - "ItemDefinition"
Cohesion: 0.14
Nodes (12): Dictionary, IReadOnlyList, List, ItemDatabase, float, int, IReadOnlyList, List (+4 more)

### Community 4 - "IDisposable"
Cohesion: 0.07
Nodes (21): List, Entity, IEntity, EntityCommand, Collider, Dictionary, RaycastHit, EntityLocator (+13 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (19): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+11 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.06
Nodes (23): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerMask, ILayerService (+15 more)

### Community 7 - "SceneType"
Cohesion: 0.19
Nodes (7): Scene, SerializedDictionary, SceneData, Scene, SceneModel, SceneType, Data

### Community 8 - "PlayerHudUiController"
Cohesion: 0.07
Nodes (25): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud (+17 more)

### Community 9 - "EquipmentSlotId"
Cohesion: 0.18
Nodes (8): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode, EquipmentSlotId

### Community 10 - "AnimatorComponent"
Cohesion: 0.14
Nodes (10): AnimationEvent, Animator, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, AnimatorComponent, HandMode, IAnimationStateSink (+2 more)

### Community 11 - "Character"
Cohesion: 0.08
Nodes (17): AnimatorComponent, CharacterAttributeStats, float, HealthStats, Inject, Quaternion, Vector3, Character (+9 more)

### Community 12 - "ITimer"
Cohesion: 0.11
Nodes (7): LandingType, ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 13 - "MovementComponent"
Cohesion: 0.12
Nodes (13): bool, float, ITimer, LandingType, Transform, MovementComponent, CharacterController, Dictionary (+5 more)

### Community 14 - "EquipmentUiController"
Cohesion: 0.13
Nodes (10): HandMode, EquipmentLoadout, EquipmentSlotId, IInputService, InventoryChange, InventoryEntry, InventoryEntryId, EquipmentUiController (+2 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.12
Nodes (9): Dictionary, GameObject, IEquipmentPresenter, Image, int, List, TMP_Text, Transform (+1 more)

### Community 16 - "AudioService"
Cohesion: 0.06
Nodes (18): IAudioSettingsData, IObserver, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, MusicType (+10 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.10
Nodes (15): EquipmentSlotGroup, EquipmentSlotId, HandMode, IEquipmentLoadoutSink, Inject, InventoryChange, InventoryEntry, InventoryEntryId (+7 more)

### Community 18 - "CharacterRuntime.cs"
Cohesion: 0.15
Nodes (17): float, Vector2, AttackIntent, AttackRequest, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind, CharacterCommand (+9 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (12): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+4 more)

### Community 20 - ".BuildLoadout"
Cohesion: 0.17
Nodes (11): EquipmentSlotGroup, AnimatorStateMachineDto, CharacterCommandExecutionStatus, EquipmentComponent, EquipmentLoadout, EquipmentSlotGroup, EquippedItemContext, EquipmentSwapCoordinator (+3 more)

### Community 21 - "InventoryEntry"
Cohesion: 0.18
Nodes (8): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "UiService"
Cohesion: 0.20
Nodes (7): IUiService, UiService, BaseUi, Canvas, Inject, Transform, UiFactory

### Community 23 - "CameraService"
Cohesion: 0.13
Nodes (5): CameraService, Camera, CinemachineCamera, CinemachineThirdPersonFollow, Ease

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.13
Nodes (8): IEquipmentRoute, Action, IReadOnlyCollection, IInventoryRoute, IPauseNavigationRoute, IReadOnlyCollection, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

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

### Community 34 - "EquipmentSlotChange"
Cohesion: 0.50
Nodes (3): EquipmentSlotId, InventoryEntryId, EquipmentSlotChange

### Community 36 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 37 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.12
Nodes (11): bool, float, DamageResult, HealthModel, bool, float, HealthStats, bool (+3 more)

### Community 38 - "IInitializable"
Cohesion: 0.16
Nodes (5): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator, IInitializable

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
Cohesion: 0.16
Nodes (4): string, InventoryEntryId, IEquipmentPresenter, IEquatable

### Community 43 - "MainMenuUiController"
Cohesion: 0.27
Nodes (3): IMainMenuOrchestrator, MainMenuUiController, UiController

### Community 44 - "InventoryItemViewData"
Cohesion: 0.16
Nodes (8): CharacterAttributeStats, EquipmentSlotId, IReadOnlyList, InventoryEntryId, InventoryPrimaryCategory, Sprite, InventoryItemViewData, IReadOnlyDictionary

### Community 46 - "LockOnUiController"
Cohesion: 0.15
Nodes (10): Camera, RectTransform, Transform, Vector3, LockOnUi, bool, Camera, LockOnUiController (+2 more)

### Community 47 - "PlayerController"
Cohesion: 0.25
Nodes (8): ICameraService, IInputService, PlayerController, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable, ITargetingService

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 50 - "SoulsLike.Entities.Character.Components.Movement"
Cohesion: 0.20
Nodes (6): LandingType, LocomotionState, MovementMode, IEquipmentLoadoutSink, SoulsLike.Entities.Character.Ports, SoulsLike.Entities.Character.Components.Movement

### Community 51 - "InventoryUi"
Cohesion: 0.15
Nodes (10): Color, IInventoryPresenter, Image, int, IReadOnlyList, List, Transform, InventoryUi (+2 more)

### Community 52 - "ColorConverter"
Cohesion: 0.14
Nodes (13): ColorConverter, JsonReader, JsonSerializer, JsonWriter, Type, JsonReader, JsonSerializer, JsonWriter (+5 more)

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
Cohesion: 0.13
Nodes (9): bool, IInputService, InventoryChange, InventoryPrimaryCategory, InventoryUiController, HashSet, IInventoryPresenter, IInventoryRoute (+1 more)

### Community 59 - ".Move"
Cohesion: 0.15
Nodes (3): Inject, Vector2, IMovementPresentationSink

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

### Community 66 - ".TryStartRoll"
Cohesion: 0.26
Nodes (3): Quaternion, Vector2, Vector3

### Community 67 - "MainMenuUi"
Cohesion: 0.13
Nodes (6): IContainerBuilder, MainMenuScope, IMainMenuPresenter, MainMenuUi, SoulsLike.Ui.MainMenu, IStartable

### Community 68 - "SaveService"
Cohesion: 0.09
Nodes (10): string, FileService, SaveKeys, string, ISaveService, SaveService, string, SaveStore (+2 more)

### Community 70 - "InventoryItemSO"
Cohesion: 0.12
Nodes (10): InventoryPrimaryCategory, InventorySubCategory, bool, float, int, Sprite, string, InventoryItemSO (+2 more)

### Community 72 - ".Tick"
Cohesion: 0.11
Nodes (5): AnimatorStateMachineDto, bool, CharacterRuntime, CharacterActionStateMachine, CharacterInputBatch

### Community 73 - "InventoryViewStateController"
Cohesion: 0.29
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 74 - "ItemId"
Cohesion: 0.18
Nodes (7): IReadOnlyList, Sprite, ItemCatalog, ItemId, ItemType, CharacterAttributeStats, InventoryEntry

### Community 75 - "InventoryComponent"
Cohesion: 0.18
Nodes (10): Inject, InventoryEntry, InventoryEntryId, IReadOnlyList, InventoryComponent, Collider, int, GroundItem (+2 more)

### Community 76 - "GameOrchestrator"
Cohesion: 0.20
Nodes (6): UniTask, UniTaskVoid, GameOrchestrator, UniTask, ISceneService, ITickable

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
Cohesion: 0.10
Nodes (13): Action, GenericPopupService, IGenericPopupService, AcceptPopup, Button, AlertPopup, Action, Button (+5 more)

### Community 81 - "PlayerCharacterInputAdapter"
Cohesion: 0.12
Nodes (11): AnimatorStateMachineDto, CharacterAnimationAdapter, UnityCharacterClock, PlayerCharacterInputAdapter, ICharacterClock, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Runtime, HeavyAttackGestureResolver (+3 more)

### Community 82 - "AnimatorRootMotionRelay"
Cohesion: 0.17
Nodes (7): Animator, bool, string, AnimatorRootMotionRelay, Quaternion, Vector3, IRootMotionSink

### Community 84 - ".Read"
Cohesion: 0.16
Nodes (6): bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, SoulsLike.Entities.Character.Input, float

### Community 86 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.32
Nodes (3): SceneDependency, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 87 - ".DisplayItemDetails"
Cohesion: 0.33
Nodes (3): CharacterAttributeStats, ScalingGrade, TMP_Text

### Community 90 - "ShieldDatabase"
Cohesion: 0.18
Nodes (7): Dictionary, IReadOnlyList, List, ShieldDatabase, float, GameObject, ShieldDefinition

### Community 93 - "SoulsLike.Services"
Cohesion: 0.13
Nodes (5): GameState, IGameStateNotifier, IGameStateObserver, SoulsLike.Services, SoulsLike.Orchestrators.MainMenu

### Community 94 - "BaseUi"
Cohesion: 0.21
Nodes (5): bool, CanvasGroup, Transform, BaseUi, IBaseUi

### Community 95 - ".UpdateState"
Cohesion: 0.33
Nodes (3): AnimatorStateMachineDto, AnimatorStateMachineDto, IAnimationStateSink

### Community 100 - "ConsumableDatabase"
Cohesion: 0.21
Nodes (7): Dictionary, IReadOnlyList, List, ConsumableDatabase, float, ConsumableDefinition, ItemUseType

### Community 102 - "CoreScope.cs"
Cohesion: 0.33
Nodes (3): IContainerBuilder, CoreScope, SoulsLike.Services.CameraService

### Community 104 - "DamageRequest"
Cohesion: 0.40
Nodes (4): float, int, Vector3, DamageRequest

### Community 108 - "MovementModel"
Cohesion: 0.20
Nodes (6): AnimatorModel, AnimationCurve, LayerMask, MovementModel, SoulsLike.Entities.Character.Components, Model

### Community 134 - ".TryStartAttack"
Cohesion: 0.20
Nodes (4): AttackRequest, CharacterCommandExecutionStatus, JumpRequest, RollRequest

### Community 137 - "WeaponDatabase"
Cohesion: 0.33
Nodes (4): Dictionary, IReadOnlyList, List, WeaponDatabase

### Community 138 - "IMovementData"
Cohesion: 0.70
Nodes (4): AnimationCurve, LayerMask, IMovementData, MovementData

### Community 144 - "AddressableAssetService"
Cohesion: 0.07
Nodes (18): GameObject, AddressableAssetService, IAssetService, LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory (+10 more)

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 147 - "SharedSceneScope"
Cohesion: 0.22
Nodes (6): ProjectScope, SharedSceneScope, IContainerBuilder, LifetimeScope, OnGuiFpsCounter, PreviewRenderService

### Community 149 - "ScriptableObject"
Cohesion: 0.22
Nodes (6): RuntimeAnimatorController, AnimationProfile, CombatProfile, float, AudioData, ScriptableObject

### Community 157 - "CharacterActionStateMachine"
Cohesion: 0.16
Nodes (11): CharacterActionStateMachine, CharacterCommandDisposition, bool, CharacterActionStateId, CharacterAnimationSignal, CharacterCommand, CharacterCommandBuffer, CharacterCommandDisposition (+3 more)

### Community 180 - "EquipmentPresentation"
Cohesion: 0.15
Nodes (13): BaseComponent, IComponent, bool, GameObject, Inject, Quaternion, Transform, Vector3 (+5 more)

### Community 188 - "CharacterFactory"
Cohesion: 0.25
Nodes (5): CharacterFactory, BaseFactory, Character, GameObject, string

## Knowledge Gaps
- **14 isolated node(s):** `CharacterAttributeStats`, `SpeedMultiplierKey`, `IPlayerHudPresenter`, `SwapPhase`, `AttackType` (+9 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **127 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `PlayerCharacterInputAdapter` (6× useful, score=5.83745847)
- `AmbienceData` (2× useful, score=1.953058047)
- `CharacterCommandFactory` (2× useful, score=1.940817403)
- `ICharacterCommand` (2× useful, score=1.940150187)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `SoulsLike.Items`, `HealthComponent`, `.TryStartAttack`, `IInitializable`, `.Tick`, `AnimatorComponent`, `InventoryComponent`, `ItemId`, `EquipmentUiController`, `PlayerController`, `EquipmentUi`, `EquipmentComponent`, `EquipmentPresentation`, `ICameraService`, `Vector2`, `InventoryUiController`, `CharacterActionStateMachine`, `AttackComponent`?**
  _High betweenness centrality (0.164) - this node is a cross-community bridge._
- **Why does `AnimatorComponent` connect `AnimatorComponent` to `AnimatorStateMachineReceiver`, `.TryStartAttack`, `.Tick`, `Character`, `MovementModel`, `AmbienceService`, `.BuildLoadout`, `.Read`, `ICameraService`, `Vector2`, `UiService`, `ColorConverter`, `CharacterFactory`, `CharacterActionStateMachine`, `.UpdateState`, `AttackComponent`?**
  _High betweenness centrality (0.078) - this node is a cross-community bridge._
- **Why does `AmbienceService` connect `AmbienceService` to `IDisposable`, `IInitializable`, `GameOrchestrator`, `AudioService`, `CharacterFactory`?**
  _High betweenness centrality (0.073) - this node is a cross-community bridge._
- **What connects `CharacterAttributeStats`, `SpeedMultiplierKey`, `IPlayerHudPresenter` to the rest of the system?**
  _14 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.09176788124156546 - nodes in this community are weakly interconnected._
- **Should `ItemDefinition` be split into smaller, more focused modules?**
  _Cohesion score 0.1368421052631579 - nodes in this community are weakly interconnected._
- **Should `IDisposable` be split into smaller, more focused modules?**
  _Cohesion score 0.06845513413506013 - nodes in this community are weakly interconnected._