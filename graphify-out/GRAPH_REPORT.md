# Graph Report - SoulsLikeTemplate  (2026-08-18)

## Corpus Check
- 187 files · ~33,642 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1749 nodes · 3224 edges · 176 communities (91 shown, 85 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 255 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a55b5ac6`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- RollRequest
- IInitializable
- AnimatorStateMachineReceiver
- PreviewRenderService
- SceneData
- PlayerHudUi
- EquipmentSlotId
- AnimatorComponent
- Character
- SoulsLike.Entities.Character.Components.Health
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
- InventoryEntryId
- TargetLockNode
- WeaponDefinition
- StorageRegistry
- IMovementComponent
- CustomButton
- IEquipmentPresenter
- BaseUi
- ITimer
- AmbienceData
- .Hide
- CharacterCommand
- System.Ui.Base
- .CreateButton
- IInventoryPresenter
- InventoryUiController
- PauseMenuUiController
- ICameraService
- MainMenuOrchestrator
- EquipmentSlotUI
- PlayerController
- Vector2
- EquipmentSlotHud
- UiFactory
- SharedSceneScope
- CustomButtonMapping
- PlayerHudUiController
- AmbienceManagerWrapper
- .Create
- .Move
- .Read
- InventoryViewStateController
- FileService
- ICustomButton
- InventoryItemSO
- PauseNavigationUi
- .OnAnimationStateChanged
- .TryStartRoll
- CharacterCommandExecutionStatus
- ItemDatabase
- SceneType
- CustomButtonEditor
- ItemTypes.cs
- .BuildLoadout
- Data
- SoulsLike.Entities.Character.Runtime
- DamageRequest
- MusicType
- LockOnUi
- StateMachineName
- SoulsLike.Services
- .HandleLockOnInput
- SoulsLike.Extensions
- InventoryData
- .ApplyAnimationProfile
- ItemType
- UiService
- IGameStateObserver
- CharacterRuntime
- AnimatorStateMachineDto
- CharacterFactory
- ItemDefinition
- .SetGrounded
- AttackType
- .SetAirborneMotion
- HandMode
- MainMenuUiController
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
- AnimatorControllerParameterType
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
- SoulsLike.Services.Scenes.Data
- .GetRequired
- IEquipmentLoadoutSink
- PlayerHudUi.cs
- VContainerExt
- MonoBehaviour
- HealthStatUpdate

## God Nodes (most connected - your core abstractions)
1. `Character` - 58 edges
2. `AnimatorComponent` - 56 edges
3. `MovementComponent` - 51 edges
4. `EquipmentUi` - 37 edges
5. `InventoryUiController` - 37 edges
6. `InventoryEntryId` - 34 edges
7. `EquipmentUiController` - 33 edges
8. `EquipmentSlotUI` - 33 edges
9. `InventoryUi` - 31 edges
10. `EquipmentComponent` - 31 edges

## Surprising Connections (you probably didn't know these)
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `Character` --references--> `AnimatorComponent`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Components/Animator/AnimatorComponent.cs
- `InventoryComponent` --inherits--> `BaseComponent`  [EXTRACTED]
  Assets/Scripts/Components/Inventory/InventoryComponent.cs → Assets/Scripts/Components/BaseComponent.cs
- `WeaponRuntime` --references--> `InventoryEntryId`  [EXTRACTED]
  Assets/Scripts/Components/Equipment/WeaponRuntime.cs → Assets/Scripts/Components/Inventory/InventoryEntry.cs
- `WeaponRuntime` --references--> `WeaponDefinition`  [EXTRACTED]
  Assets/Scripts/Components/Equipment/WeaponRuntime.cs → Assets/Scripts/Items/WeaponDefinition.cs

## Import Cycles
- None detected.

## Communities (176 total, 85 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.07
Nodes (23): EquipmentLoadout, EquippedItemContext, GameObject, Quaternion, Transform, Vector3, EquipmentPresentation, float (+15 more)

### Community 1 - "SceneReference"
Cohesion: 0.15
Nodes (8): bool, string, SceneReference, IComparable, IEquatable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.08
Nodes (13): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+5 more)

### Community 3 - "RollRequest"
Cohesion: 0.40
Nodes (3): Vector2, CharacterControlFrame, RollRequest

### Community 4 - "IInitializable"
Cohesion: 0.06
Nodes (28): List, Entity, IEntity, EntityCommand, Collider, Dictionary, RaycastHit, EntityLocator (+20 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (19): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+11 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.06
Nodes (24): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerMask, ILayerService (+16 more)

### Community 7 - "SceneData"
Cohesion: 0.19
Nodes (5): Scene, SerializedDictionary, SceneData, Scene, SceneModel

### Community 8 - "PlayerHudUi"
Cohesion: 0.20
Nodes (8): bool, Color, float, MPImage, RectTransform, PlayerHudUi, StatBar, StatBar

### Community 9 - "EquipmentSlotId"
Cohesion: 0.17
Nodes (7): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode

### Community 10 - "AnimatorComponent"
Cohesion: 0.10
Nodes (15): AnimationEvent, Animator, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, AnimatorComponent, Vector2, AttackType (+7 more)

### Community 11 - "Character"
Cohesion: 0.09
Nodes (17): float, Inject, Vector3, Character, AttackComponent, CharacterAttributeStats, EquipmentComponent, EquipmentPresentation (+9 more)

### Community 12 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.16
Nodes (8): bool, float, DamageResult, HealthModel, bool, float, HealthStats, SoulsLike.Entities.Character.Components.Health

### Community 13 - "MovementComponent"
Cohesion: 0.12
Nodes (14): bool, float, ITimer, LandingType, Quaternion, Transform, MovementComponent, CharacterController (+6 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.14
Nodes (7): Dictionary, GameObject, Image, int, TMP_Text, Transform, EquipmentUi

### Community 16 - "InventoryUi"
Cohesion: 0.11
Nodes (12): IReadOnlyList, InventoryItemViewData, Color, Image, int, IReadOnlyList, List, ScalingGrade (+4 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.22
Nodes (7): InventoryComponent, EquipmentComponent, EquipmentModel, EquipmentSlotId, InventoryChange, InventoryEntryId, ItemDatabase

### Community 18 - "CharacterActionStateMachine"
Cohesion: 0.24
Nodes (6): bool, CharacterActionStateMachine, float, CharacterCommandBuffer, CharacterInputBatch, ICharacterActionExecutor

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (12): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+4 more)

### Community 21 - "InventoryEntry"
Cohesion: 0.16
Nodes (8): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "AudioService"
Cohesion: 0.16
Nodes (8): List, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, IAudioService, IObserver

### Community 23 - "CameraService"
Cohesion: 0.11
Nodes (9): bool, Camera, float, Transform, Tween, CameraService, CinemachineCamera, CinemachineThirdPersonFollow (+1 more)

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.16
Nodes (5): IEquipmentRoute, IInventoryRoute, IPauseNavigationRoute, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 29 - "AmbienceService"
Cohesion: 0.18
Nodes (5): float, GameObject, Tween, AmbienceService, AudioSource

### Community 31 - "AttackComponent"
Cohesion: 0.14
Nodes (16): AnimatorStateMachineDto, AttackType, bool, float, HandMode, ITimer, StateMachineName, AttackComponent (+8 more)

### Community 32 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "AnimatorRootMotionRelay"
Cohesion: 0.17
Nodes (7): Animator, bool, string, AnimatorRootMotionRelay, Quaternion, Vector3, IRootMotionSink

### Community 36 - "InventoryEntryId"
Cohesion: 0.18
Nodes (7): IReadOnlyList, InventoryComponent, string, InventoryEntryId, Collider, int, GroundItem

### Community 37 - "TargetLockNode"
Cohesion: 0.21
Nodes (7): Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService, TargetingService

### Community 38 - "WeaponDefinition"
Cohesion: 0.15
Nodes (11): RuntimeAnimatorController, AnimationProfile, CombatProfile, bool, float, GameObject, int, Sprite (+3 more)

### Community 39 - "StorageRegistry"
Cohesion: 0.20
Nodes (6): Enum, IStorageRegistry, Enum, string, StorageRegistry, SoulsLike.Services.Storage

### Community 40 - "IMovementComponent"
Cohesion: 0.06
Nodes (19): AnimatorStateMachineDto, AnimatorModel, Quaternion, SpeedMultiplierKey, Transform, Vector2, Vector3, IMovementComponent (+11 more)

### Community 41 - "CustomButton"
Cohesion: 0.13
Nodes (8): bool, ColorBlock, Image, SelectionState, Sprite, TMP_Text, CustomButton, Button

### Community 43 - "BaseUi"
Cohesion: 0.17
Nodes (5): bool, CanvasGroup, Transform, BaseUi, IBaseUi

### Community 44 - "ITimer"
Cohesion: 0.11
Nodes (7): LandingType, ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 45 - "AmbienceData"
Cohesion: 0.19
Nodes (9): float, AmbienceData, MusicEntry, SfxEntry, SfxType, AudioClip, MusicEntry, SceneMusicEntry (+1 more)

### Community 46 - ".Hide"
Cohesion: 0.18
Nodes (5): bool, Camera, LockOnUiController, IPostLateTickable, UiController

### Community 47 - "CharacterCommand"
Cohesion: 0.26
Nodes (5): CharacterCommand, CharacterCommandDisposition, CharacterCommandExecutionResult, EquipmentActionKind, EquipmentActionRequest

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 51 - "InventoryUiController"
Cohesion: 0.12
Nodes (8): EquipmentSlotChange, InventoryPrimaryCategory, InventorySubCategory, Action, bool, IReadOnlyCollection, InventoryUiController, HashSet

### Community 52 - "PauseMenuUiController"
Cohesion: 0.17
Nodes (4): ICoreGameOrchestrator, GameState, IGameStateNotifier, PauseMenuUiController

### Community 53 - "ICameraService"
Cohesion: 0.20
Nodes (3): Vector2, ICameraService, Ray

### Community 54 - "MainMenuOrchestrator"
Cohesion: 0.18
Nodes (4): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Color, GameObject, Image, MPImage, PointerEventData, TMP_Text (+3 more)

### Community 56 - "PlayerController"
Cohesion: 0.15
Nodes (13): PlayerCharacterInputAdapter, ICameraService, IInputService, PlayerController, GameState, HeavyAttackGestureResolver, ICameraService, IGameStateNotifier (+5 more)

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

### Community 65 - ".Move"
Cohesion: 0.15
Nodes (3): Inject, Vector2, IMovementPresentationSink

### Community 66 - ".Read"
Cohesion: 0.20
Nodes (5): bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, float

### Community 67 - "InventoryViewStateController"
Cohesion: 0.29
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 68 - "FileService"
Cohesion: 0.36
Nodes (3): string, FileService, SoulsLike.Services.Save

### Community 70 - "InventoryItemSO"
Cohesion: 0.29
Nodes (7): bool, float, int, Sprite, string, InventoryItemSO, ScalingGrade

### Community 72 - ".OnAnimationStateChanged"
Cohesion: 0.19
Nodes (3): AnimatorStateMachineDto, CharacterAnimationAdapter, AnimatorStateMachineDto

### Community 74 - "CharacterCommandExecutionStatus"
Cohesion: 0.16
Nodes (8): AnimatorComponent, AnimatorStateMachineDto, EquipmentComponent, EquipmentSwapCoordinator, SwapPhase, CharacterCommandExecutionStatus, JumpRequest, SwapPhase

### Community 75 - "ItemDatabase"
Cohesion: 0.31
Nodes (5): Dictionary, IReadOnlyList, List, ItemDatabase, ItemId

### Community 76 - "SceneType"
Cohesion: 0.18
Nodes (7): UniTask, UniTaskVoid, GameOrchestrator, SceneType, UniTask, ISceneService, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "ItemTypes.cs"
Cohesion: 0.22
Nodes (10): float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot, ScalingGrade, float (+2 more)

### Community 79 - ".BuildLoadout"
Cohesion: 0.24
Nodes (4): EquipmentLoadout, HandMode, EquipmentSlotGroup, EquippedItemContext

### Community 80 - "Data"
Cohesion: 0.21
Nodes (9): HealthData, IHealthData, AnimationCurve, LayerMask, IMovementData, MovementData, Data, Model (+1 more)

### Community 81 - "SoulsLike.Entities.Character.Runtime"
Cohesion: 0.18
Nodes (5): UnityCharacterClock, ICharacterClock, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Input, SoulsLike.Entities.Character.Runtime

### Community 82 - "DamageRequest"
Cohesion: 0.40
Nodes (4): float, int, Vector3, DamageRequest

### Community 83 - "MusicType"
Cohesion: 0.20
Nodes (3): SceneMusicEntry, MusicType, IAmbienceSystem

### Community 84 - "LockOnUi"
Cohesion: 0.38
Nodes (5): Camera, RectTransform, Transform, Vector3, LockOnUi

### Community 86 - "SoulsLike.Services"
Cohesion: 0.30
Nodes (5): float, AudioData, SoulsLike.Services, SoulsLike.Services.Audio.Data, SoulsLike.Services.Audio

### Community 88 - "SoulsLike.Extensions"
Cohesion: 0.33
Nodes (3): CanvasGroup, CanvasGroupExt, SoulsLike.Extensions

### Community 89 - "InventoryData"
Cohesion: 0.33
Nodes (5): Inject, IReadOnlyList, List, InitialInventoryEntry, InventoryData

### Community 90 - ".ApplyAnimationProfile"
Cohesion: 0.16
Nodes (3): AnimationProfile, EquipmentLoadout, HandMode

### Community 91 - "ItemType"
Cohesion: 0.20
Nodes (7): float, ConsumableDefinition, ItemType, ItemUseType, Action, IReadOnlyCollection, IReadOnlyCollection

### Community 92 - "UiService"
Cohesion: 0.21
Nodes (6): UiController, List, Transform, IUiService, UiService, Canvas

### Community 94 - "CharacterRuntime"
Cohesion: 0.11
Nodes (6): bool, CharacterRuntime, MovementGate, MovementGateReason, MovementPolicy, CharacterActionStateMachine

### Community 96 - "CharacterFactory"
Cohesion: 0.33
Nodes (4): CharacterFactory, BaseFactory, GameObject, string

### Community 97 - "ItemDefinition"
Cohesion: 0.20
Nodes (8): float, int, IReadOnlyList, List, Sprite, string, ItemDefinition, EquipmentGroup

### Community 102 - "MainMenuUiController"
Cohesion: 0.09
Nodes (9): IMainMenuOrchestrator, IContainerBuilder, MainMenuScope, IMainMenuPresenter, MainMenuUi, MainMenuUiController, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu (+1 more)

### Community 144 - "AddressableAssetService"
Cohesion: 0.22
Nodes (4): GameObject, AddressableAssetService, IAssetService, SoulsLike.Services.Repository

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 146 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 150 - "CharacterRuntime.cs"
Cohesion: 0.31
Nodes (6): AttackIntent, AttackRequest, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind, CharacterCommandKind

### Community 166 - ".InjectSinks"
Cohesion: 0.50
Nodes (3): IAnimationStateSink, Inject, IRootMotionSink

### Community 169 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.32
Nodes (3): SceneDependency, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 171 - "IEquipmentLoadoutSink"
Cohesion: 0.33
Nodes (3): Inject, EquipmentLoadout, IEquipmentLoadoutSink

### Community 173 - "VContainerExt"
Cohesion: 0.47
Nodes (4): AssetMappingData, IContainerBuilder, VContainerExt, RegistrationBuilder

### Community 174 - "MonoBehaviour"
Cohesion: 0.83
Nodes (3): BaseComponent, IComponent, MonoBehaviour

### Community 175 - "HealthStatUpdate"
Cohesion: 0.50
Nodes (3): bool, float, HealthStatUpdate

## Knowledge Gaps
- **10 isolated node(s):** `SpeedMultiplierKey`, `SwapPhase`, `AttackType`, `LandingType`, `LocomotionState` (+5 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **85 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `PlayerCharacterInputAdapter` (5× useful, score=4.965550439)
- `CharacterRuntime` (3× useful, score=2.989236686)
- `CharacterCommandFactory` (2× useful, score=1.99183301)
- `ICharacterCommand` (2× useful, score=1.991148256)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `SoulsLike.Items`, `HealthComponent`, `IInitializable`, `AnimatorComponent`, `EquipmentUiController`, `CharacterActionStateMachine`, `CharacterRuntime.cs`, `.InjectSinks`, `MonoBehaviour`, `CharacterCommand`, `InventoryUiController`, `PlayerController`, `.OnAnimationStateChanged`, `.TryStartRoll`, `CharacterCommandExecutionStatus`, `.BuildLoadout`, `SoulsLike.Entities.Character.Runtime`, `.HandleLockOnInput`, `.ApplyAnimationProfile`, `CharacterRuntime`, `CharacterFactory`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.256) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `.Move`, `.SetGrounded`, `IInitializable`, `IMovementComponent`, `.TryStartRoll`, `ITimer`, `CharacterRuntime`, `AttackComponent`?**
  _High betweenness centrality (0.088) - this node is a cross-community bridge._
- **Why does `BaseUi` connect `BaseUi` to `MainMenuUiController`, `PauseNavigationUi`, `PlayerHudUi`, `MonoBehaviour`, `.Hide`, `InventoryUi`, `EquipmentUi`, `LockOnUi`, `IPauseMenuPresenter`, `UiService`?**
  _High betweenness centrality (0.086) - this node is a cross-community bridge._
- **What connects `SpeedMultiplierKey`, `SwapPhase`, `AttackType` to the rest of the system?**
  _10 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SoulsLike.Items` be split into smaller, more focused modules?**
  _Cohesion score 0.0711864406779661 - nodes in this community are weakly interconnected._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.14705882352941177 - nodes in this community are weakly interconnected._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.08246225319396051 - nodes in this community are weakly interconnected._