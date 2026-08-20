# Graph Report - SoulsLikeTemplate  (2026-08-20)

## Corpus Check
- 229 files · ~40,990 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1990 nodes · 3478 edges · 231 communities (94 shown, 137 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 281 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `1972a7ff`
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
- bool
- EquipmentUiController
- EquipmentUi
- AudioService
- EquipmentComponent
- CharacterRuntime.cs
- InventorySlotUI
- .StartSwap
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
- .Refresh
- SpeedMultiplierKey
- InputService
- SoulsLike.Entities.Character.Components.Health
- IGameOrchestrator
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
- SoulsLike.Entities.Character.Components.Movement
- InventoryUi
- ColorConverter
- SceneService
- .HandleLockOnInput
- EquipmentSlotUI
- InventoryUiController
- Vector2
- PauseNavigationUi
- MovementComponent
- IContainerBuilder
- CustomButtonMapping
- TargetLockNode
- float
- CoroutineService
- IUniqueIdGenerator
- Quaternion
- IInitializable
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
- CharacterRuntime
- BasePopup
- SoulsLike.Entities.Character.Runtime
- AnimatorRootMotionRelay
- ICameraService
- .Read
- StateMachineName
- SoulsLike.Services.Scenes.Data
- .FormatScaling
- GameObject
- Tween
- ShieldDatabase
- List
- List
- CoreGameOrchestrator
- BaseUi
- ILayerService
- Component
- EquipmentLoadout
- EquipmentPresentation
- ICharacterActionExecutor
- ConsumableDatabase
- CharacterActionStateId
- CoreScope.cs
- IComponentMediator
- DamageRequest
- IComponentMediator
- IComponentMediator
- IComponentMediator
- LayerName
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
- LockOnUi
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
- SoulsLike.Services.Layer
- bool
- InventoryComponent
- .Open
- CharacterActionStateId
- CharacterInputBatch
- LayerData
- MovementModel
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
- CharacterAttributeStats.cs
- Transform
- AnimatorComponent
- Tween
- EquipmentComponent
- .ShowPicker
- int
- CharacterActionStateMachine
- AnimatorComponent
- int
- Inject
- Transform
- PlayerHudUiController.cs
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
- bool
- float
- HandMode
- RuntimeAnimatorController
- CharacterFactory
- bool
- IPauseNavigationRoute
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
- .ApplyDamage
- .SetAirborneMotion
- GameObject
- string
- float
- Inject
- ITimer
- LandingType
- Transform
- Vector2
- Vector3
- MovementModel

## God Nodes (most connected - your core abstractions)
1. `AnimatorComponent` - 59 edges
2. `Character` - 59 edges
3. `MovementComponent` - 56 edges
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
- `MovementComponent` --references--> `MovementModel`  [EXTRACTED]
  Assets/Scripts/Components/Movement/MovementComponent.cs → Assets/Scripts/Components/Movement/MovementModel.cs
- `ItemCatalog` --references--> `ConsumableDatabase`  [EXTRACTED]
  Assets/Scripts/Items/ItemCatalog.cs → Assets/Scripts/Items/ConsumableDatabase.cs
- `ConsumableDefinition` --references--> `ItemId`  [EXTRACTED]
  Assets/Scripts/Items/ConsumableDefinition.cs → Assets/Scripts/Items/ItemTypes.cs
- `ConsumableDefinition` --references--> `ItemUseType`  [EXTRACTED]
  Assets/Scripts/Items/ConsumableDefinition.cs → Assets/Scripts/Items/ItemTypes.cs

## Import Cycles
- None detected.

## Communities (231 total, 137 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.19
Nodes (8): SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character, SoulsLike.Ui.Inventory, SoulsLike.Ui.Equipment

### Community 1 - "SceneReference"
Cohesion: 0.16
Nodes (7): bool, string, SceneReference, IComparable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.24
Nodes (4): DamageRequest, HealthStats, HealthStatUpdate, HealthComponent

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
Cohesion: 0.18
Nodes (7): Camera, Transform, Vector3, PreviewRenderService, Bounds, Light, RenderTexture

### Community 7 - "SceneType"
Cohesion: 0.21
Nodes (6): Scene, SerializedDictionary, SceneData, Scene, SceneModel, SceneType

### Community 8 - "PlayerHudUiController"
Cohesion: 0.07
Nodes (25): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud (+17 more)

### Community 9 - "EquipmentSlotId"
Cohesion: 0.19
Nodes (7): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode

### Community 10 - "AnimatorComponent"
Cohesion: 0.12
Nodes (11): AnimationEvent, Animator, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, AnimatorComponent, BaseComponent, HandMode (+3 more)

### Community 11 - "Character"
Cohesion: 0.07
Nodes (17): AnimatorComponent, CharacterAttributeStats, float, HealthStats, Inject, Quaternion, Vector3, Character (+9 more)

### Community 12 - "ITimer"
Cohesion: 0.12
Nodes (6): ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 14 - "EquipmentUiController"
Cohesion: 0.10
Nodes (11): CharacterAttributeStats, EquipmentSlotId, EquipmentSlotId, IInputService, InventoryChange, InventoryEntry, InventoryEntryId, EquipmentUiController (+3 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.13
Nodes (9): Dictionary, GameObject, IEquipmentPresenter, Image, int, List, TMP_Text, Transform (+1 more)

### Community 16 - "AudioService"
Cohesion: 0.06
Nodes (17): IAudioSettingsData, IObserver, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, MusicType (+9 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.11
Nodes (12): EquipmentSlotGroup, EquipmentSlotId, HandMode, IEquipmentLoadoutSink, Inject, InventoryChange, InventoryEntryId, EquipmentComponent (+4 more)

### Community 18 - "CharacterRuntime.cs"
Cohesion: 0.16
Nodes (15): UnityCharacterClock, float, Vector2, AttackIntent, AttackRequest, CharacterCommand, CharacterCommandBuffer, CharacterCommandKind (+7 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.12
Nodes (12): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+4 more)

### Community 20 - ".StartSwap"
Cohesion: 0.18
Nodes (11): EquipmentSlotGroup, AnimatorStateMachineDto, CharacterCommandExecutionStatus, EquipmentComponent, EquipmentLoadout, EquipmentSlotGroup, EquippedItemContext, EquipmentSwapCoordinator (+3 more)

### Community 21 - "InventoryEntry"
Cohesion: 0.18
Nodes (8): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "UiService"
Cohesion: 0.18
Nodes (8): IUiService, UiService, BaseUi, Canvas, Inject, List, Transform, UiFactory

### Community 23 - "CameraService"
Cohesion: 0.13
Nodes (6): CameraService, Camera, CinemachineCamera, CinemachineThirdPersonFollow, Ease, Tween

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 28 - "PauseMenuUiController"
Cohesion: 0.14
Nodes (4): ICoreGameOrchestrator, IPauseMenuPresenter, PauseMenuUi, PauseMenuUiController

### Community 29 - "AmbienceService"
Cohesion: 0.07
Nodes (25): AudioClip, float, IAudioService, IAudioSettingsData, MusicType, SceneType, SfxType, AmbienceService (+17 more)

### Community 31 - "AttackComponent"
Cohesion: 0.14
Nodes (13): AnimatorStateMachineDto, AttackRequest, bool, CombatProfile, float, HandMode, Inject, AttackComponent (+5 more)

### Community 32 - "InventoryData.cs"
Cohesion: 0.21
Nodes (9): HealthData, IHealthData, IReadOnlyList, List, InitialInventoryEntry, InventoryData, Data, Model (+1 more)

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - ".Refresh"
Cohesion: 0.20
Nodes (5): EquipmentSlotId, InventoryEntryId, EquipmentSlotChange, InventoryChange, InventorySubCategory

### Community 36 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 37 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.16
Nodes (8): bool, float, DamageResult, HealthModel, bool, float, HealthStats, SoulsLike.Entities.Character.Components.Health

### Community 38 - "IGameOrchestrator"
Cohesion: 0.25
Nodes (3): UniTask, UniTaskVoid, IGameOrchestrator

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
Cohesion: 0.16
Nodes (4): string, InventoryEntryId, IEquipmentPresenter, IEquatable

### Community 43 - "IHealthComponent"
Cohesion: 0.18
Nodes (5): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, IHealthComponent

### Community 44 - "InventoryItemViewData"
Cohesion: 0.18
Nodes (9): CharacterAttributeStats, EquipmentSlotId, InventoryEntry, InventoryEntryId, InventoryPrimaryCategory, Sprite, InventoryItemViewData, CharacterAttributeStats (+1 more)

### Community 46 - "LockOnUiController"
Cohesion: 0.24
Nodes (5): bool, Camera, LockOnUiController, IPostLateTickable, UiController

### Community 47 - "PlayerController"
Cohesion: 0.18
Nodes (8): ICameraService, IInputService, PlayerController, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable, ITargetingService

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 50 - "SoulsLike.Entities.Character.Components.Movement"
Cohesion: 0.25
Nodes (4): LandingType, LocomotionState, MovementMode, SoulsLike.Entities.Character.Components.Movement

### Community 51 - "InventoryUi"
Cohesion: 0.12
Nodes (11): Color, IInventoryPresenter, Image, int, IReadOnlyList, List, TMP_Text, Transform (+3 more)

### Community 52 - "ColorConverter"
Cohesion: 0.31
Nodes (6): ColorConverter, JsonReader, JsonSerializer, JsonWriter, Type, Color

### Community 53 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Color, EquipmentSlotId, GameObject, Image, MPImage, PointerEventData (+3 more)

### Community 56 - "InventoryUiController"
Cohesion: 0.19
Nodes (7): bool, IInputService, InventoryPrimaryCategory, InventoryUiController, HashSet, IInventoryPresenter, IInventoryRoute

### Community 59 - "MovementComponent"
Cohesion: 0.06
Nodes (22): MovementComponent, LandingType, Vector2, IMovementPresentationSink, JsonReader, JsonSerializer, JsonWriter, Type (+14 more)

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 62 - "TargetLockNode"
Cohesion: 0.21
Nodes (7): Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService, TargetingService

### Community 64 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 65 - "IUniqueIdGenerator"
Cohesion: 0.33
Nodes (3): IUniqueIdGenerator, UniqueIdGenerator, SoulsLike.Services.IdGeneration

### Community 67 - "IInitializable"
Cohesion: 0.10
Nodes (7): IMainMenuOrchestrator, MainMenuOrchestrator, IMainMenuPresenter, MainMenuUi, MainMenuUiController, IInitializable, IStartable

### Community 68 - "SaveService"
Cohesion: 0.09
Nodes (10): string, FileService, SaveKeys, string, ISaveService, SaveService, string, SaveStore (+2 more)

### Community 70 - "InventoryItemSO"
Cohesion: 0.12
Nodes (10): InventoryPrimaryCategory, InventorySubCategory, bool, float, int, Sprite, string, InventoryItemSO (+2 more)

### Community 71 - ".Open"
Cohesion: 0.25
Nodes (3): Action, InventoryEntryId, IReadOnlyCollection

### Community 73 - "InventoryViewStateController"
Cohesion: 0.29
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 74 - "ItemId"
Cohesion: 0.16
Nodes (11): InventoryEntry, IReadOnlyList, HandMode, InventoryEntry, EquipmentLoadout, EquippedItemContext, IReadOnlyList, Sprite (+3 more)

### Community 75 - "InventoryComponent"
Cohesion: 0.24
Nodes (7): Inject, InventoryEntry, InventoryEntryId, IReadOnlyList, InventoryComponent, InventoryData, InventoryModel

### Community 76 - "GameOrchestrator"
Cohesion: 0.20
Nodes (6): UniTask, UniTaskVoid, GameOrchestrator, UniTask, ISceneService, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "WeaponDefinition"
Cohesion: 0.10
Nodes (20): AnimationProfile, float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot, ScalingGrade (+12 more)

### Community 79 - "CharacterRuntime"
Cohesion: 0.17
Nodes (7): bool, CharacterCommandDisposition, CharacterRuntime, MovementGate, MovementGateReason, MovementPolicy, CharacterActionStateMachine

### Community 80 - "BasePopup"
Cohesion: 0.10
Nodes (13): Action, GenericPopupService, IGenericPopupService, AcceptPopup, Button, AlertPopup, Action, Button (+5 more)

### Community 82 - "AnimatorRootMotionRelay"
Cohesion: 0.07
Nodes (18): AnimatorStateMachineDto, AnimatorModel, Animator, bool, string, AnimatorRootMotionRelay, BaseComponent, IComponent (+10 more)

### Community 84 - ".Read"
Cohesion: 0.11
Nodes (11): PlayerCharacterInputAdapter, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, SoulsLike.Entities.Character.Input, float, HeavyAttackGestureResolver (+3 more)

### Community 86 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.32
Nodes (3): SceneDependency, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 90 - "ShieldDatabase"
Cohesion: 0.21
Nodes (7): Dictionary, IReadOnlyList, List, ShieldDatabase, float, GameObject, ShieldDefinition

### Community 93 - "CoreGameOrchestrator"
Cohesion: 0.14
Nodes (5): List, CoreGameOrchestrator, GameState, IGameStateNotifier, IGameStateObserver

### Community 94 - "BaseUi"
Cohesion: 0.21
Nodes (5): bool, CanvasGroup, Transform, BaseUi, IBaseUi

### Community 95 - "ILayerService"
Cohesion: 0.22
Nodes (4): GameObject, LayerMask, ILayerService, Inject

### Community 99 - "ICharacterActionExecutor"
Cohesion: 0.47
Nodes (3): CharacterCommandExecutionResult, CharacterCommandExecutionStatus, ICharacterActionExecutor

### Community 100 - "ConsumableDatabase"
Cohesion: 0.21
Nodes (7): Dictionary, IReadOnlyList, List, ConsumableDatabase, float, ConsumableDefinition, ItemUseType

### Community 101 - "CharacterActionStateId"
Cohesion: 0.32
Nodes (5): AnimatorStateMachineDto, CharacterAnimationAdapter, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind

### Community 102 - "CoreScope.cs"
Cohesion: 0.33
Nodes (3): SoulsLike.Services.Targeting, SoulsLike.Services.CameraService, SoulsLike.Ui.LockOn

### Community 104 - "DamageRequest"
Cohesion: 0.19
Nodes (7): float, int, Vector3, DamageRequest, bool, float, HealthStatUpdate

### Community 108 - "LayerName"
Cohesion: 0.46
Nodes (4): LayerName, GameObject, LayerMask, LayerService

### Community 119 - "LockOnUi"
Cohesion: 0.32
Nodes (5): Camera, RectTransform, Transform, Vector3, LockOnUi

### Community 134 - ".Open"
Cohesion: 0.29
Nodes (4): Action, IReadOnlyCollection, IInventoryRoute, IReadOnlyCollection

### Community 137 - "LayerData"
Cohesion: 0.33
Nodes (6): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, Data

### Community 138 - "MovementModel"
Cohesion: 0.31
Nodes (7): AnimationCurve, LayerMask, IMovementData, MovementData, AnimationCurve, LayerMask, MovementModel

### Community 140 - "SoulsLike.Services"
Cohesion: 0.13
Nodes (8): UiController, IPreviewRenderService, IContainerBuilder, MainMenuScope, SoulsLike.Services, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu, SoulsLike.Ui.Base

### Community 144 - "AddressableAssetService"
Cohesion: 0.07
Nodes (18): GameObject, AddressableAssetService, IAssetService, LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory (+10 more)

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 147 - "SharedSceneScope"
Cohesion: 0.17
Nodes (8): IContainerBuilder, CoreScope, ProjectScope, SharedSceneScope, IContainerBuilder, LifetimeScope, OnGuiFpsCounter, PreviewRenderService

### Community 149 - "ScriptableObject"
Cohesion: 0.22
Nodes (6): RuntimeAnimatorController, AnimationProfile, CombatProfile, float, AudioData, ScriptableObject

### Community 157 - "CharacterActionStateMachine"
Cohesion: 0.18
Nodes (11): CharacterActionStateMachine, bool, CharacterActionStateId, CharacterAnimationSignal, CharacterCommand, CharacterCommandBuffer, CharacterCommandDisposition, CharacterCommandExecutionResult (+3 more)

### Community 180 - "EquipmentPresentation"
Cohesion: 0.12
Nodes (14): bool, GameObject, Inject, Quaternion, Transform, Vector3, EquipmentPresentation, float (+6 more)

### Community 188 - "CharacterFactory"
Cohesion: 0.25
Nodes (5): CharacterFactory, BaseFactory, Character, GameObject, string

### Community 219 - ".ApplyDamage"
Cohesion: 0.40
Nodes (3): DamageResult, DamageRequest, DamageResult

## Knowledge Gaps
- **14 isolated node(s):** `AttackType`, `CharacterAttributeStats`, `IPlayerHudPresenter`, `SpeedMultiplierKey`, `SwapPhase` (+9 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **137 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `AmbienceData` (2× useful, score=1.952473907)
- `CharacterCommandFactory` (2× useful, score=1.940236924)
- `ICharacterCommand` (2× useful, score=1.939569908)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `SoulsLike.Items`, `HealthComponent`, `AnimatorComponent`, `EquipmentUiController`, `EquipmentUi`, `EquipmentComponent`, `CharacterActionStateMachine`, `AttackComponent`, `.Refresh`, `PlayerController`, `EquipmentPresentation`, `.HandleLockOnInput`, `.BeginRootMotionAction`, `InventoryUiController`, `MovementComponent`, `IInitializable`, `.Tick`, `ItemId`, `InventoryComponent`, `.ApplyDamage`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.152) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `IInitializable`, `AnimatorComponent`, `Character`, `MovementModel`, `SoulsLike.Entities.Character.Components.Movement`, `AmbienceService`, `.Read`, `.HandleLockOnInput`, `UiService`, `.SetAirborneMotion`, `CharacterActionStateMachine`, `AttackComponent`?**
  _High betweenness centrality (0.111) - this node is a cross-community bridge._
- **Why does `AmbienceService` connect `AmbienceService` to `IInitializable`, `IDisposable`, `GameOrchestrator`, `AudioService`, `CameraService`, `CharacterFactory`?**
  _High betweenness centrality (0.062) - this node is a cross-community bridge._
- **What connects `AttackType`, `CharacterAttributeStats`, `IPlayerHudPresenter` to the rest of the system?**
  _14 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `ItemDefinition` be split into smaller, more focused modules?**
  _Cohesion score 0.1368421052631579 - nodes in this community are weakly interconnected._
- **Should `IDisposable` be split into smaller, more focused modules?**
  _Cohesion score 0.06845513413506013 - nodes in this community are weakly interconnected._
- **Should `AnimatorStateMachineReceiver` be split into smaller, more focused modules?**
  _Cohesion score 0.0797872340425532 - nodes in this community are weakly interconnected._