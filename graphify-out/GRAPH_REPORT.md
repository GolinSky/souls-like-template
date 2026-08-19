# Graph Report - SoulsLikeTemplate  (2026-08-19)

## Corpus Check
- 227 files · ~40,025 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1986 nodes · 3467 edges · 223 communities (95 shown, 128 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 281 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `66d609c3`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- ItemDefinition
- IEntity
- AnimatorStateMachineReceiver
- PreviewRenderService
- SceneType
- PlayerHudUi
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
- EquipmentPresentation
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
- PlayerHudUiController
- SpeedMultiplierKey
- InputService
- SoulsLike.Entities.Character.Components.Health
- MainMenuOrchestrator
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntryId
- SoulsLike.Services
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
- MainMenuUiController
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
- IInitializable
- List
- List
- CoreGameOrchestrator
- BaseUi
- .UpdateState
- Component
- EquipmentLoadout
- EquipmentPresentation
- ICharacterActionExecutor
- .SetAirborneMotion
- AttackType
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
- CharacterAnimationSignal
- CharacterCommandBuffer
- Camera
- CharacterCommandExecutionResult
- CharacterControlFrame
- ICharacterCommand
- IEquipmentCommandReceiver
- AddressableAssetService
- SceneReferencePropertyDrawer
- float
- SharedSceneScope
- AnimatorControllerParameterType
- ScriptableObject
- CharacterRuntime
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
- HealthStatUpdate
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
- WeaponRuntime
- AnimatorStateMachineDto
- AttackType
- Vector2
- bool
- float
- HandMode
- RuntimeAnimatorController
- CharacterFactory
- BaseComponent
- CharacterInputBatch
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
- MonoBehaviour
- .GetRay
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
- `CoreGameOrchestrator` --references--> `CharacterFactory`  [EXTRACTED]
  Assets/Scripts/Orchestrators/Core/CoreGameOrchestrator.cs → Assets/Scripts/Entities/Character/CharacterFactory.cs
- `CoreScope` --references--> `CameraService`  [EXTRACTED]
  Assets/Scripts/Services/VContainer/CoreScope.cs → Assets/Scripts/Services/CameraService/CameraService.cs
- `MovementModel` --implements--> `IMovementData`  [EXTRACTED]
  Assets/Scripts/Components/Movement/MovementModel.cs → Assets/Scripts/Components/Movement/MovementData.cs
- `Character` --references--> `AttackComponent`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Components/Attack/AttackComponent.cs

## Import Cycles
- None detected.

## Communities (223 total, 128 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.15
Nodes (10): CharacterAttributeStats, SpeedMultiplierKey, SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character (+2 more)

### Community 1 - "SceneReference"
Cohesion: 0.16
Nodes (7): bool, string, SceneReference, IComparable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.09
Nodes (13): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+5 more)

### Community 3 - "ItemDefinition"
Cohesion: 0.14
Nodes (12): Dictionary, IReadOnlyList, List, ItemDatabase, float, int, IReadOnlyList, List (+4 more)

### Community 4 - "IEntity"
Cohesion: 0.08
Nodes (16): IEntity, Collider, Dictionary, RaycastHit, EntityLocator, IContainerBuilder, EntityRegistrationExt, EntityType (+8 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (19): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+11 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.07
Nodes (23): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerMask, ILayerService (+15 more)

### Community 7 - "SceneType"
Cohesion: 0.18
Nodes (7): Scene, SerializedDictionary, SceneData, Scene, SceneModel, SceneType, ISceneService

### Community 8 - "PlayerHudUi"
Cohesion: 0.09
Nodes (20): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud (+12 more)

### Community 9 - "EquipmentSlotId"
Cohesion: 0.16
Nodes (8): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode, EquipmentSlotId

### Community 10 - "AnimatorComponent"
Cohesion: 0.14
Nodes (11): AnimationEvent, Animator, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, AnimatorComponent, HandMode, IAnimationStateSink (+3 more)

### Community 11 - "Character"
Cohesion: 0.07
Nodes (20): AnimatorComponent, CharacterAttributeStats, float, HealthStats, Inject, Quaternion, Vector3, Character (+12 more)

### Community 12 - "ITimer"
Cohesion: 0.11
Nodes (7): LandingType, ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 13 - "MovementComponent"
Cohesion: 0.12
Nodes (13): bool, float, ITimer, LandingType, Transform, MovementComponent, CharacterController, Dictionary (+5 more)

### Community 14 - "EquipmentUiController"
Cohesion: 0.13
Nodes (8): EquipmentSlotId, IInputService, InventoryChange, InventoryEntry, InventoryEntryId, EquipmentUiController, IEquipmentPresenter, IEquipmentRoute

### Community 15 - "EquipmentUi"
Cohesion: 0.12
Nodes (8): Dictionary, GameObject, IEquipmentPresenter, Image, int, List, Transform, EquipmentUi

### Community 16 - "AudioService"
Cohesion: 0.06
Nodes (17): IAudioSettingsData, IObserver, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, MusicType (+9 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.12
Nodes (12): EquipmentSlotGroup, EquipmentSlotId, HandMode, IEquipmentLoadoutSink, Inject, InventoryChange, InventoryEntry, InventoryEntryId (+4 more)

### Community 18 - "CharacterRuntime.cs"
Cohesion: 0.21
Nodes (11): AttackIntent, AttackRequest, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind, CharacterCommand, CharacterCommandDisposition, CharacterCommandKind (+3 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (12): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+4 more)

### Community 20 - "EquipmentPresentation"
Cohesion: 0.10
Nodes (18): bool, EquipmentSlotGroup, GameObject, Inject, Quaternion, Transform, Vector3, EquipmentPresentation (+10 more)

### Community 21 - "InventoryEntry"
Cohesion: 0.18
Nodes (8): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "UiService"
Cohesion: 0.20
Nodes (7): IUiService, UiService, BaseUi, Canvas, List, Transform, UiFactory

### Community 23 - "CameraService"
Cohesion: 0.13
Nodes (5): CameraService, Camera, CinemachineCamera, CinemachineThirdPersonFollow, Ease

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.12
Nodes (10): IReadOnlyList, ItemType, IEquipmentRoute, Action, IReadOnlyCollection, IInventoryRoute, IPauseNavigationRoute, IReadOnlyCollection (+2 more)

### Community 25 - "SoulsLike"
Cohesion: 0.13
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 29 - "AmbienceService"
Cohesion: 0.07
Nodes (26): AudioClip, float, IAudioService, IAudioSettingsData, MusicType, SceneType, SfxType, AmbienceService (+18 more)

### Community 31 - "AttackComponent"
Cohesion: 0.13
Nodes (15): AnimatorStateMachineDto, AttackRequest, bool, CombatProfile, float, HandMode, Inject, AttackComponent (+7 more)

### Community 32 - "InventoryData.cs"
Cohesion: 0.19
Nodes (11): IReadOnlyList, List, InitialInventoryEntry, InventoryData, AnimationCurve, LayerMask, IMovementData, MovementData (+3 more)

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "PlayerHudUiController"
Cohesion: 0.14
Nodes (11): EquipmentSlotId, HandMode, InventoryEntryId, EquipmentLoadout, EquipmentSlotChange, HealthStats, InventoryChange, PlayerHudUiController (+3 more)

### Community 36 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 37 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.14
Nodes (10): bool, float, DamageResult, HealthData, IHealthData, HealthModel, bool, float (+2 more)

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
Cohesion: 0.16
Nodes (4): string, InventoryEntryId, IEquipmentPresenter, IEquatable

### Community 43 - "SoulsLike.Services"
Cohesion: 0.15
Nodes (6): IPreviewRenderService, IContainerBuilder, MainMenuScope, SoulsLike.Services, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu

### Community 44 - "InventoryItemViewData"
Cohesion: 0.14
Nodes (9): CharacterAttributeStats, EquipmentSlotId, IReadOnlyList, TMP_Text, InventoryEntryId, InventoryPrimaryCategory, Sprite, InventoryItemViewData (+1 more)

### Community 46 - "LockOnUiController"
Cohesion: 0.15
Nodes (10): Camera, RectTransform, Transform, Vector3, LockOnUi, bool, Camera, LockOnUiController (+2 more)

### Community 47 - "PlayerController"
Cohesion: 0.15
Nodes (8): ICameraService, IInputService, PlayerController, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable, ITargetingService

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 50 - "SoulsLike.Entities.Character.Components.Movement"
Cohesion: 0.17
Nodes (7): LandingType, LocomotionState, MovementMode, EquipmentLoadout, IEquipmentLoadoutSink, SoulsLike.Entities.Character.Ports, SoulsLike.Entities.Character.Components.Movement

### Community 51 - "InventoryUi"
Cohesion: 0.14
Nodes (10): Color, IInventoryPresenter, Image, int, IReadOnlyList, List, Transform, InventoryUi (+2 more)

### Community 52 - "ColorConverter"
Cohesion: 0.14
Nodes (13): ColorConverter, JsonReader, JsonSerializer, JsonWriter, Type, JsonReader, JsonSerializer, JsonWriter (+5 more)

### Community 53 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Color, EquipmentSlotId, GameObject, Image, MPImage, PointerEventData (+3 more)

### Community 56 - "InventoryUiController"
Cohesion: 0.16
Nodes (9): bool, IInputService, InventoryChange, InventoryPrimaryCategory, InventoryUiController, HashSet, IInventoryPresenter, IInventoryRoute (+1 more)

### Community 58 - "PauseNavigationUi"
Cohesion: 0.15
Nodes (4): UiController, IPauseNavigationPresenter, PauseNavigationUi, SoulsLike.Ui.Base

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

### Community 67 - "MainMenuUiController"
Cohesion: 0.15
Nodes (5): IMainMenuOrchestrator, IMainMenuPresenter, MainMenuUi, MainMenuUiController, IStartable

### Community 68 - "SaveService"
Cohesion: 0.08
Nodes (11): string, FileService, SaveKeys, string, ISaveService, SaveService, string, SaveStore (+3 more)

### Community 70 - "InventoryItemSO"
Cohesion: 0.12
Nodes (10): InventoryPrimaryCategory, InventorySubCategory, bool, float, int, Sprite, string, InventoryItemSO (+2 more)

### Community 73 - "InventoryViewStateController"
Cohesion: 0.20
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 74 - "ItemId"
Cohesion: 0.26
Nodes (7): InventoryEntry, EquippedItemContext, Sprite, ItemCatalog, ItemId, CharacterAttributeStats, InventoryEntry

### Community 75 - "InventoryComponent"
Cohesion: 0.24
Nodes (7): Inject, InventoryEntry, InventoryEntryId, IReadOnlyList, InventoryComponent, InventoryData, InventoryModel

### Community 76 - "GameOrchestrator"
Cohesion: 0.25
Nodes (4): UniTask, UniTaskVoid, GameOrchestrator, UniTask

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "WeaponDefinition"
Cohesion: 0.05
Nodes (34): AnimationProfile, Dictionary, IReadOnlyList, List, ConsumableDatabase, float, ConsumableDefinition, float (+26 more)

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

### Community 90 - "IInitializable"
Cohesion: 0.26
Nodes (6): List, Entity, EntityCommand, IEntityComponent, IDisposable, IInitializable

### Community 93 - "CoreGameOrchestrator"
Cohesion: 0.14
Nodes (5): List, CoreGameOrchestrator, GameState, IGameStateNotifier, IGameStateObserver

### Community 94 - "BaseUi"
Cohesion: 0.14
Nodes (8): bool, CanvasGroup, Transform, BaseUi, IBaseUi, CanvasGroup, CanvasGroupExt, SoulsLike.Extensions

### Community 95 - ".UpdateState"
Cohesion: 0.33
Nodes (3): AnimatorStateMachineDto, AnimatorStateMachineDto, IAnimationStateSink

### Community 99 - "ICharacterActionExecutor"
Cohesion: 0.44
Nodes (3): CharacterCommandExecutionResult, CharacterCommandExecutionStatus, ICharacterActionExecutor

### Community 102 - "CoreScope.cs"
Cohesion: 0.33
Nodes (3): IContainerBuilder, CoreScope, SoulsLike.Services.CameraService

### Community 104 - "DamageRequest"
Cohesion: 0.40
Nodes (4): float, int, Vector3, DamageRequest

### Community 108 - "MovementModel"
Cohesion: 0.29
Nodes (5): AnimatorModel, AnimationCurve, LayerMask, MovementModel, Model

### Community 134 - ".TryStartAttack"
Cohesion: 0.20
Nodes (4): AttackRequest, CharacterCommandExecutionStatus, JumpRequest, RollRequest

### Community 144 - "AddressableAssetService"
Cohesion: 0.09
Nodes (15): GameObject, AddressableAssetService, IAssetService, LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory (+7 more)

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 147 - "SharedSceneScope"
Cohesion: 0.22
Nodes (6): ProjectScope, SharedSceneScope, IContainerBuilder, LifetimeScope, OnGuiFpsCounter, PreviewRenderService

### Community 149 - "ScriptableObject"
Cohesion: 0.22
Nodes (6): RuntimeAnimatorController, AnimationProfile, CombatProfile, float, AudioData, ScriptableObject

### Community 150 - "CharacterRuntime"
Cohesion: 0.24
Nodes (4): AnimatorStateMachineDto, bool, CharacterRuntime, CharacterActionStateMachine

### Community 157 - "CharacterActionStateMachine"
Cohesion: 0.28
Nodes (4): bool, CharacterActionStateMachine, float, CharacterCommandBuffer

### Community 167 - "HealthStatUpdate"
Cohesion: 0.50
Nodes (3): bool, float, HealthStatUpdate

### Community 180 - "WeaponRuntime"
Cohesion: 0.33
Nodes (3): float, InventoryEntryId, WeaponRuntime

### Community 188 - "CharacterFactory"
Cohesion: 0.25
Nodes (5): CharacterFactory, BaseFactory, Character, GameObject, string

### Community 189 - "BaseComponent"
Cohesion: 0.47
Nodes (3): BaseComponent, IComponent, SoulsLike.Entities.Character.Components

### Community 190 - "CharacterInputBatch"
Cohesion: 0.33
Nodes (4): Vector2, CharacterControlFrame, CharacterInputBatch, RollRequest

### Community 219 - "MonoBehaviour"
Cohesion: 0.40
Nodes (4): Collider, int, GroundItem, MonoBehaviour

## Knowledge Gaps
- **14 isolated node(s):** `AttackType`, `IPlayerHudPresenter`, `CharacterAttributeStats`, `SpeedMultiplierKey`, `SwapPhase` (+9 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **128 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `PlayerCharacterInputAdapter` (6× useful, score=5.83759903)
- `AmbienceData` (2× useful, score=1.953105075)
- `CharacterCommandFactory` (2× useful, score=1.940864136)
- `ICharacterCommand` (2× useful, score=1.940196904)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `SoulsLike.Items`, `HealthComponent`, `.TryStartAttack`, `AnimatorComponent`, `EquipmentUiController`, `EquipmentUi`, `EquipmentComponent`, `EquipmentPresentation`, `CharacterRuntime`, `AttackComponent`, `PlayerController`, `WeaponRuntime`, `ICameraService`, `Vector2`, `InventoryUiController`, `.Tick`, `ItemId`, `InventoryComponent`, `IInitializable`, `MonoBehaviour`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.157) - this node is a cross-community bridge._
- **Why does `AnimatorComponent` connect `AnimatorComponent` to `.SetAirborneMotion`, `AttackType`, `AnimatorStateMachineReceiver`, `.TryStartAttack`, `.Tick`, `SaveService`, `Character`, `AmbienceService`, `CharacterFactory`, `EquipmentPresentation`, `.Read`, `ICameraService`, `Vector2`, `UiService`, `ColorConverter`, `AttackComponent`, `BaseComponent`, `.UpdateState`?**
  _High betweenness centrality (0.081) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `.TryStartRoll`, `.Tick`, `ITimer`, `SoulsLike.Entities.Character.Components.Movement`, `.SetGrounded`, `IInitializable`, `.Move`, `AttackComponent`?**
  _High betweenness centrality (0.075) - this node is a cross-community bridge._
- **What connects `AttackType`, `IPlayerHudPresenter`, `CharacterAttributeStats` to the rest of the system?**
  _14 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SoulsLike.Items` be split into smaller, more focused modules?**
  _Cohesion score 0.14962121212121213 - nodes in this community are weakly interconnected._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.08846153846153847 - nodes in this community are weakly interconnected._
- **Should `ItemDefinition` be split into smaller, more focused modules?**
  _Cohesion score 0.1368421052631579 - nodes in this community are weakly interconnected._