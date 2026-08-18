# Graph Report - SoulsLikeTemplate  (2026-08-18)

## Corpus Check
- 187 files · ~33,753 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1750 nodes · 3231 edges · 169 communities (85 shown, 84 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 255 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `7a99ea0d`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- .Tick
- IDisposable
- AnimatorStateMachineReceiver
- PreviewRenderService
- SceneData
- PlayerHudUi
- EquipmentSlotId
- AnimatorComponent
- Character
- HealthStats
- MovementComponent
- EquipmentUiController
- EquipmentUi
- InventoryUi
- EquipmentComponent
- CharacterActionStateMachine
- InventorySlotUI
- IPauseMenuPresenter
- InventoryEntry
- AudioService
- CameraService
- PauseNavigationUiController
- SoulsLike
- OnGuiFpsCounter
- CharacterActions
- CoreGameOrchestrator
- AmbienceService
- Animator
- AttackComponent
- CoroutineService
- CustomButtonToggle
- AnimatorRootMotionRelay
- SpeedMultiplierKey
- InventoryComponent
- TargetLockNode
- WeaponDefinition
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntryId
- BaseUi
- MainMenuUi
- AmbienceData
- .Hide
- CharacterCommand
- System.Ui.Base
- .CreateButton
- IInventoryPresenter
- InventoryUiController
- PauseMenuUiController
- ICameraService
- IInitializable
- EquipmentSlotUI
- PlayerController
- Vector2
- EquipmentSlotHud
- UiFactory
- SharedSceneScope
- CustomButtonMapping
- PlayerHudUiController
- AmbienceManagerWrapper
- InventoryItemViewData
- .TriggerRoll
- .Read
- InventoryViewStateController
- FileService
- ICustomButton
- InventoryItemSO
- PauseNavigationUi
- .PlayAttack
- EquipmentLoadout
- ICharacterActionExecutor
- ItemDatabase
- SceneType
- CustomButtonEditor
- ItemTypes.cs
- MainMenuUiController
- Data
- SoulsLike.Entities.Character.Runtime
- SoulsLike.Entities.Character.Components.Health
- InputService
- LockOnUi
- StateMachineName
- SoulsLike.Services
- .HandleLockOnInput
- VContainerExt
- InventoryData
- .ApplyEquipmentLoadout
- .Open
- UiService
- IGameStateNotifier
- CharacterRuntime
- AnimatorStateMachineDto
- CharacterFactory
- ItemDefinition
- .ShowPicker
- AttackType
- .SetAirborneMotion
- HandMode
- MainMenuOrchestrator.cs
- IComponentMediator
- RuntimeAnimatorController
- IComponentMediator
- IComponentMediator
- IComponentMediator
- IReadOnlyList
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
- GameObject
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
- EquipmentLoadout
- HealthStats
- InventoryComponent
- Quaternion
- CharacterActionStateId
- CharacterInputBatch
- CharacterAnimationSignal
- CharacterCommandBuffer
- CharacterCommandDisposition
- CharacterCommandExecutionResult
- CharacterControlFrame
- ICharacterCommand
- IEquipmentCommandReceiver
- AddressableAssetService
- SceneReferencePropertyDrawer
- SceneService
- IPauseNavigationRouteNavigation
- .ApplyAnimationProfile
- bool
- CharacterRuntime.cs
- float
- IAnimationStateSink
- Inject
- IRootMotionSink
- LandingType
- int
- Transform
- AnimatorComponent
- int
- EquipmentComponent
- CharacterAnimationAdapter
- string
- CharacterActionStateId
- CharacterInputBatch
- CharacterCommandFactory
- .InjectSinks
- Vector2
- Vector3

## God Nodes (most connected - your core abstractions)
1. `Character` - 58 edges
2. `AnimatorComponent` - 57 edges
3. `MovementComponent` - 51 edges
4. `EquipmentUi` - 37 edges
5. `InventoryUiController` - 37 edges
6. `InventoryEntryId` - 34 edges
7. `EquipmentUiController` - 33 edges
8. `EquipmentSlotUI` - 33 edges
9. `InventoryUi` - 31 edges
10. `EquipmentComponent` - 31 edges

## Surprising Connections (you probably didn't know these)
- `Character` --references--> `AnimatorComponent`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Components/Animator/AnimatorComponent.cs
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `Character` --references--> `CharacterAnimationAdapter`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Adapters/CharacterAnimationAdapter.cs
- `Character` --references--> `CharacterActionStateId`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs
- `Character` --references--> `CharacterRuntime`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs

## Import Cycles
- None detected.

## Communities (169 total, 84 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.06
Nodes (28): BaseComponent, IComponent, GameObject, Quaternion, Transform, Vector3, EquipmentPresentation, float (+20 more)

### Community 1 - "SceneReference"
Cohesion: 0.12
Nodes (9): SceneDependency, bool, string, SceneReference, IComparable, IEquatable, ISerializationCallbackReceiver, Obsolete (+1 more)

### Community 2 - "HealthComponent"
Cohesion: 0.09
Nodes (13): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+5 more)

### Community 3 - ".Tick"
Cohesion: 0.22
Nodes (3): Vector2, CharacterControlFrame, CharacterInputBatch

### Community 4 - "IDisposable"
Cohesion: 0.07
Nodes (21): List, Entity, IEntity, EntityCommand, Collider, Dictionary, RaycastHit, EntityLocator (+13 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (19): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+11 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.07
Nodes (23): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerMask, ILayerService (+15 more)

### Community 7 - "SceneData"
Cohesion: 0.22
Nodes (5): Scene, SerializedDictionary, SceneData, Scene, SceneModel

### Community 8 - "PlayerHudUi"
Cohesion: 0.20
Nodes (8): bool, Color, float, MPImage, RectTransform, PlayerHudUi, StatBar, StatBar

### Community 9 - "EquipmentSlotId"
Cohesion: 0.18
Nodes (7): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode

### Community 10 - "AnimatorComponent"
Cohesion: 0.15
Nodes (11): AnimationEvent, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, AnimatorComponent, bool, int, IObserver (+3 more)

### Community 11 - "Character"
Cohesion: 0.09
Nodes (17): float, Inject, Vector3, Character, AttackComponent, CharacterAttributeStats, EquipmentComponent, EquipmentPresentation (+9 more)

### Community 12 - "HealthStats"
Cohesion: 0.20
Nodes (7): bool, float, DamageResult, HealthModel, bool, float, HealthStats

### Community 13 - "MovementComponent"
Cohesion: 0.07
Nodes (21): bool, float, Inject, ITimer, LandingType, Quaternion, Transform, Vector2 (+13 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.15
Nodes (8): Dictionary, GameObject, Image, int, List, TMP_Text, Transform, EquipmentUi

### Community 16 - "InventoryUi"
Cohesion: 0.13
Nodes (10): Color, Image, int, IReadOnlyList, List, ScalingGrade, TMP_Text, Transform (+2 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.07
Nodes (21): EquipmentLoadout, HandMode, Inject, InventoryComponent, EquipmentComponent, AnimatorComponent, EquipmentComponent, EquipmentSwapCoordinator (+13 more)

### Community 18 - "CharacterActionStateMachine"
Cohesion: 0.28
Nodes (4): bool, CharacterActionStateMachine, float, CharacterCommandBuffer

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IMoveHandler (+3 more)

### Community 21 - "InventoryEntry"
Cohesion: 0.16
Nodes (8): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "AudioService"
Cohesion: 0.17
Nodes (8): List, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, IAudioService, IObserver

### Community 23 - "CameraService"
Cohesion: 0.12
Nodes (8): bool, Camera, float, Tween, CameraService, CinemachineCamera, CinemachineThirdPersonFollow, Ease

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.17
Nodes (4): IEquipmentRoute, IPauseNavigationRoute, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 28 - "CoreGameOrchestrator"
Cohesion: 0.18
Nodes (4): List, CoreGameOrchestrator, GameState, IGameStateObserver

### Community 29 - "AmbienceService"
Cohesion: 0.18
Nodes (5): float, GameObject, Tween, AmbienceService, AudioSource

### Community 31 - "AttackComponent"
Cohesion: 0.07
Nodes (22): AnimatorStateMachineDto, AttackType, bool, float, HandMode, ITimer, StateMachineName, AttackComponent (+14 more)

### Community 32 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "AnimatorRootMotionRelay"
Cohesion: 0.17
Nodes (7): Animator, bool, string, AnimatorRootMotionRelay, Quaternion, Vector3, IRootMotionSink

### Community 37 - "TargetLockNode"
Cohesion: 0.21
Nodes (7): Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService, TargetingService

### Community 38 - "WeaponDefinition"
Cohesion: 0.12
Nodes (13): RuntimeAnimatorController, AnimationProfile, CombatProfile, bool, float, GameObject, int, Sprite (+5 more)

### Community 39 - "StorageRegistry"
Cohesion: 0.20
Nodes (6): Enum, IStorageRegistry, Enum, string, StorageRegistry, SoulsLike.Services.Storage

### Community 40 - "IMovementComponent"
Cohesion: 0.06
Nodes (19): AnimatorStateMachineDto, AnimatorModel, Quaternion, SpeedMultiplierKey, Transform, Vector2, Vector3, IMovementComponent (+11 more)

### Community 41 - "CustomButton"
Cohesion: 0.13
Nodes (8): bool, ColorBlock, Image, SelectionState, Sprite, TMP_Text, CustomButton, Button

### Community 42 - "InventoryEntryId"
Cohesion: 0.16
Nodes (3): string, InventoryEntryId, IEquipmentPresenter

### Community 43 - "BaseUi"
Cohesion: 0.19
Nodes (5): bool, CanvasGroup, Transform, BaseUi, IBaseUi

### Community 44 - "MainMenuUi"
Cohesion: 0.22
Nodes (3): IMainMenuPresenter, MainMenuUi, IStartable

### Community 45 - "AmbienceData"
Cohesion: 0.11
Nodes (12): float, AmbienceData, MusicEntry, SceneMusicEntry, SfxEntry, MusicType, SfxType, IAmbienceSystem (+4 more)

### Community 46 - ".Hide"
Cohesion: 0.18
Nodes (5): bool, Camera, LockOnUiController, IPostLateTickable, UiController

### Community 47 - "CharacterCommand"
Cohesion: 0.31
Nodes (5): CharacterCommand, CharacterCommandDisposition, CharacterCommandKind, EquipmentActionKind, EquipmentActionRequest

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 51 - "InventoryUiController"
Cohesion: 0.16
Nodes (7): InventoryPrimaryCategory, InventorySubCategory, Action, bool, IReadOnlyCollection, InventoryUiController, HashSet

### Community 53 - "ICameraService"
Cohesion: 0.15
Nodes (3): Transform, ICameraService, Ray

### Community 54 - "IInitializable"
Cohesion: 0.16
Nodes (5): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator, IInitializable

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.15
Nodes (10): BaseEventData, Color, GameObject, Image, MPImage, PointerEventData, TMP_Text, EquipmentSlotUI (+2 more)

### Community 56 - "PlayerController"
Cohesion: 0.18
Nodes (9): ICameraService, IInputService, PlayerController, Vector2, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable (+1 more)

### Community 58 - "EquipmentSlotHud"
Cohesion: 0.22
Nodes (8): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud

### Community 59 - "UiFactory"
Cohesion: 0.18
Nodes (8): LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory, Inject, SoulsLike.Factory, IObjectResolver

### Community 60 - "SharedSceneScope"
Cohesion: 0.18
Nodes (7): IContainerBuilder, CoreScope, IContainerBuilder, ProjectScope, IContainerBuilder, SharedSceneScope, LifetimeScope

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 63 - "AmbienceManagerWrapper"
Cohesion: 0.27
Nodes (3): float, AmbienceManagerWrapper, Component

### Community 64 - "InventoryItemViewData"
Cohesion: 0.32
Nodes (3): CharacterAttributeStats, InventoryItemViewData, IReadOnlyDictionary

### Community 66 - ".Read"
Cohesion: 0.11
Nodes (11): PlayerCharacterInputAdapter, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, SoulsLike.Entities.Character.Input, float, HeavyAttackGestureResolver (+3 more)

### Community 67 - "InventoryViewStateController"
Cohesion: 0.20
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 68 - "FileService"
Cohesion: 0.36
Nodes (3): string, FileService, SoulsLike.Services.Save

### Community 70 - "InventoryItemSO"
Cohesion: 0.29
Nodes (7): bool, float, int, Sprite, string, InventoryItemSO, ScalingGrade

### Community 73 - "EquipmentLoadout"
Cohesion: 0.38
Nodes (3): EquipmentLoadout, EquipmentSlotChange, EquippedItemContext

### Community 74 - "ICharacterActionExecutor"
Cohesion: 0.29
Nodes (5): CharacterCommandExecutionResult, CharacterCommandExecutionStatus, ICharacterActionExecutor, JumpRequest, RollRequest

### Community 75 - "ItemDatabase"
Cohesion: 0.36
Nodes (5): Dictionary, IReadOnlyList, List, ItemDatabase, ItemId

### Community 76 - "SceneType"
Cohesion: 0.18
Nodes (7): UniTask, UniTaskVoid, GameOrchestrator, SceneType, UniTask, ISceneService, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "ItemTypes.cs"
Cohesion: 0.16
Nodes (14): float, ConsumableDefinition, float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot (+6 more)

### Community 80 - "Data"
Cohesion: 0.21
Nodes (9): HealthData, IHealthData, AnimationCurve, LayerMask, IMovementData, MovementData, Data, Model (+1 more)

### Community 81 - "SoulsLike.Entities.Character.Runtime"
Cohesion: 0.29
Nodes (4): UnityCharacterClock, ICharacterClock, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Runtime

### Community 82 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.15
Nodes (8): float, int, Vector3, DamageRequest, bool, float, HealthStatUpdate, SoulsLike.Entities.Character.Components.Health

### Community 83 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 84 - "LockOnUi"
Cohesion: 0.32
Nodes (5): Camera, RectTransform, Transform, Vector3, LockOnUi

### Community 86 - "SoulsLike.Services"
Cohesion: 0.18
Nodes (6): IPreviewRenderService, SoulsLike.Services, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes, SoulsLike.Services.Audio.Data, SoulsLike.Services.Audio

### Community 88 - "VContainerExt"
Cohesion: 0.20
Nodes (7): CanvasGroup, CanvasGroupExt, AssetMappingData, IContainerBuilder, VContainerExt, SoulsLike.Extensions, RegistrationBuilder

### Community 89 - "InventoryData"
Cohesion: 0.33
Nodes (5): Inject, IReadOnlyList, List, InitialInventoryEntry, InventoryData

### Community 91 - ".Open"
Cohesion: 0.29
Nodes (4): Action, IReadOnlyCollection, IInventoryRoute, IReadOnlyCollection

### Community 92 - "UiService"
Cohesion: 0.21
Nodes (6): UiController, List, Transform, IUiService, UiService, Canvas

### Community 94 - "CharacterRuntime"
Cohesion: 0.11
Nodes (5): AnimatorStateMachineDto, AnimatorStateMachineDto, bool, CharacterRuntime, CharacterActionStateMachine

### Community 96 - "CharacterFactory"
Cohesion: 0.33
Nodes (4): CharacterFactory, BaseFactory, GameObject, string

### Community 97 - "ItemDefinition"
Cohesion: 0.20
Nodes (8): float, int, IReadOnlyList, List, Sprite, string, ItemDefinition, EquipmentGroup

### Community 102 - "MainMenuOrchestrator.cs"
Cohesion: 0.20
Nodes (4): IContainerBuilder, MainMenuScope, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu

### Community 144 - "AddressableAssetService"
Cohesion: 0.22
Nodes (4): GameObject, AddressableAssetService, IAssetService, SoulsLike.Services.Repository

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 146 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 148 - ".ApplyAnimationProfile"
Cohesion: 0.21
Nodes (3): AnimationProfile, Animator, AnimatorControllerParameterType

### Community 150 - "CharacterRuntime.cs"
Cohesion: 0.18
Nodes (10): AnimatorStateMachineDto, CharacterAnimationAdapter, AttackIntent, AttackRequest, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind, MovementGate (+2 more)

### Community 166 - ".InjectSinks"
Cohesion: 0.50
Nodes (3): IAnimationStateSink, Inject, IRootMotionSink

## Knowledge Gaps
- **10 isolated node(s):** `SpeedMultiplierKey`, `AttackType`, `SwapPhase`, `LandingType`, `LocomotionState` (+5 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **84 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `PlayerCharacterInputAdapter` (5× useful, score=4.978152387)
- `CharacterRuntime` (3× useful, score=2.996822996)
- `CharacterCommandFactory` (2× useful, score=1.996888034)
- `ICharacterCommand` (2× useful, score=1.996201542)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `SoulsLike.Items`, `HealthComponent`, `.Tick`, `AnimatorComponent`, `EquipmentUiController`, `EquipmentComponent`, `.ApplyAnimationProfile`, `CharacterRuntime.cs`, `.InjectSinks`, `CharacterCommand`, `InventoryUiController`, `IInitializable`, `PlayerController`, `.TriggerRoll`, `.PlayAttack`, `EquipmentLoadout`, `ICharacterActionExecutor`, `SoulsLike.Entities.Character.Runtime`, `.HandleLockOnInput`, `.ApplyEquipmentLoadout`, `CharacterRuntime`, `CharacterFactory`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.243) - this node is a cross-community bridge._
- **Why does `InventoryUiController` connect `InventoryUiController` to `SoulsLike.Items`, `InventoryItemViewData`, `ItemDefinition`, `InventoryViewStateController`, `InventoryComponent`, `IDisposable`, `EquipmentLoadout`, `ItemDatabase`, `Character`, `SceneType`, `.Hide`, `InventoryUi`, `EquipmentComponent`, `IInventoryPresenter`, `InputService`, `InventoryEntry`, `IInitializable`, `.Open`?**
  _High betweenness centrality (0.104) - this node is a cross-community bridge._
- **Why does `AmbienceService` connect `AmbienceService` to `IDisposable`, `SceneType`, `AmbienceData`, `SoulsLike.Services`, `AudioService`, `IInitializable`?**
  _High betweenness centrality (0.077) - this node is a cross-community bridge._
- **What connects `SpeedMultiplierKey`, `AttackType`, `SwapPhase` to the rest of the system?**
  _10 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SoulsLike.Items` be split into smaller, more focused modules?**
  _Cohesion score 0.06201923076923077 - nodes in this community are weakly interconnected._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.11904761904761904 - nodes in this community are weakly interconnected._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.08846153846153847 - nodes in this community are weakly interconnected._