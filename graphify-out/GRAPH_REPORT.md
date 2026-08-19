# Graph Report - SoulsLikeTemplate  (2026-08-19)

## Corpus Check
- 198 files · ~35,582 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1788 nodes · 3220 edges · 181 communities (91 shown, 90 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 254 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `4602f03a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- ItemDefinition
- IInitializable
- AnimatorStateMachineReceiver
- PreviewRenderService
- SceneData
- PlayerHudUi
- EquipmentSlotId
- AnimatorComponent
- Character
- ITimer
- MovementComponent
- EquipmentUiController
- EquipmentUi
- InventoryUi
- EquipmentComponent
- CharacterRuntime.cs
- InventorySlotUI
- .Refresh
- InventoryEntry
- AudioService
- CameraService
- PauseNavigationUiController
- SoulsLike
- OnGuiFpsCounter
- CharacterActions
- PauseMenuUiController
- AmbienceService
- Animator
- AttackComponent
- CoroutineService
- CustomButtonToggle
- AnimatorRootMotionRelay
- SpeedMultiplierKey
- IDisposable
- SoulsLike.Entities.Character.Components.Health
- MainMenuUiController
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntryId
- BaseUi
- EquipmentPresentation
- AudioClip
- .Hide
- InventoryViewStateController
- System.Ui.Base
- .CreateButton
- SoulsLike.Entities.Character.Ports
- InventoryUiController
- EquipmentSlotHud
- ICameraService
- PlayerHudUiController
- EquipmentSlotUI
- PlayerController
- Vector2
- MainMenuOrchestrator
- .Move
- IContainerBuilder
- CustomButtonMapping
- TargetLockNode
- float
- AttackType
- MovementModel
- .TryStartRoll
- IInventoryPresenter
- FileService
- ICustomButton
- InventoryItemSO
- PauseNavigationUi
- .Tick
- CharacterActionStateMachine
- .BuildLoadout
- InventoryComponent
- SceneType
- CustomButtonEditor
- WeaponDefinition
- CharacterRuntime
- Data
- SoulsLike.Entities.Character.Runtime
- DamageRequest
- .SetGrounded
- .Read
- StateMachineName
- SoulsLike.Services.Scenes.Data
- CharacterCommand
- GameObject
- Tween
- .ApplyAnimationProfile
- List
- UiService
- CoreGameOrchestrator
- GameObject
- .UpdateState
- Component
- EquipmentLoadout
- EquipmentPresentation
- ICharacterActionExecutor
- .SetAirborneMotion
- UiFactory
- SoulsLike.Services
- IComponentMediator
- .ShowPicker
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
- Quaternion
- HealthStats
- InventoryComponent
- .DisplayItemDetails
- CharacterActionStateId
- CharacterInputBatch
- CharacterAnimationSignal
- CharacterCommandBuffer
- InventoryItemViewData
- CharacterCommandExecutionResult
- CharacterControlFrame
- ICharacterCommand
- IEquipmentCommandReceiver
- AddressableAssetService
- SceneReferencePropertyDrawer
- SceneService
- SoulsLike.Entities.Character.Components
- AnimatorControllerParameterType
- ScriptableObject
- AnimatorStateMachineDto
- AnimatorComponent
- bool
- EquipmentComponent
- EquipmentSwapCoordinator
- int
- IMovementData
- AnimatorComponent
- int
- MonoBehaviour
- SpeedMultiplierKey.cs
- string
- CharacterActionStateId
- CharacterInputBatch
- CharacterCommandFactory
- AnimationProfile
- .PlayAttack
- EquipmentSlotGroup
- IAnimationStateSink
- Inject
- IRootMotionSink
- SoulsLike.Ui.Base
- LandingType
- Transform
- Vector2
- Vector3
- ITimer
- StateMachineName
- HandMode
- RuntimeAnimatorController

## God Nodes (most connected - your core abstractions)
1. `AnimatorComponent` - 59 edges
2. `Character` - 58 edges
3. `MovementComponent` - 51 edges
4. `EquipmentUi` - 37 edges
5. `InventoryUiController` - 37 edges
6. `AmbienceService` - 36 edges
7. `InventoryEntryId` - 34 edges
8. `EquipmentUiController` - 33 edges
9. `EquipmentSlotUI` - 33 edges
10. `InventoryUi` - 31 edges

## Surprising Connections (you probably didn't know these)
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `Character` --references--> `AnimatorComponent`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Components/Animator/AnimatorComponent.cs
- `WeaponDefinition` --references--> `AnimationProfile`  [EXTRACTED]
  Assets/Scripts/Items/WeaponDefinition.cs → Assets/Scripts/Items/AnimationProfile.cs
- `CoreScope` --references--> `CameraService`  [EXTRACTED]
  Assets/Scripts/Services/VContainer/CoreScope.cs → Assets/Scripts/Services/CameraService/CameraService.cs
- `SharedSceneScope` --references--> `PreviewRenderService`  [EXTRACTED]
  Assets/Scripts/Services/VContainer/SharedSceneScope.cs → Assets/Scripts/Services/PreviewRender/PreviewRenderService.cs

## Import Cycles
- None detected.

## Communities (181 total, 90 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.21
Nodes (8): SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character, SoulsLike.Ui.Inventory, SoulsLike.Ui.Equipment

### Community 1 - "SceneReference"
Cohesion: 0.15
Nodes (8): bool, string, SceneReference, IComparable, IEquatable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.08
Nodes (13): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+5 more)

### Community 3 - "ItemDefinition"
Cohesion: 0.20
Nodes (8): float, int, IReadOnlyList, List, Sprite, string, ItemDefinition, EquipmentGroup

### Community 4 - "IInitializable"
Cohesion: 0.07
Nodes (21): List, Entity, IEntity, EntityCommand, Collider, Dictionary, RaycastHit, EntityLocator (+13 more)

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
Cohesion: 0.11
Nodes (14): AnimationEvent, Animator, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, bool, float, RuntimeAnimatorController (+6 more)

### Community 11 - "Character"
Cohesion: 0.07
Nodes (24): CharacterCommandExecutionStatus, EquipmentComponent, float, Inject, Quaternion, Vector3, Character, AttackComponent (+16 more)

### Community 12 - "ITimer"
Cohesion: 0.12
Nodes (7): LandingType, ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 13 - "MovementComponent"
Cohesion: 0.13
Nodes (13): bool, float, ITimer, LandingType, Transform, MovementComponent, CharacterController, Dictionary (+5 more)

### Community 14 - "EquipmentUiController"
Cohesion: 0.20
Nodes (3): EquipmentLoadout, EquippedItemContext, EquipmentUiController

### Community 15 - "EquipmentUi"
Cohesion: 0.15
Nodes (8): Dictionary, GameObject, Image, int, List, TMP_Text, Transform, EquipmentUi

### Community 16 - "InventoryUi"
Cohesion: 0.17
Nodes (8): Color, Image, int, IReadOnlyList, List, Transform, InventoryUi, ScrollRect

### Community 17 - "EquipmentComponent"
Cohesion: 0.10
Nodes (16): HandMode, Inject, InventoryComponent, EquipmentComponent, EquipmentLoadout, IEquipmentLoadoutSink, BaseComponent, EquipmentActionRequest (+8 more)

### Community 18 - "CharacterRuntime.cs"
Cohesion: 0.23
Nodes (8): AttackIntent, AttackRequest, CharacterCommandExecutionResult, CharacterCommandExecutionStatus, CharacterCommandKind, EquipmentActionKind, EquipmentActionRequest, JumpRequest

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+3 more)

### Community 21 - "InventoryEntry"
Cohesion: 0.17
Nodes (8): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "AudioService"
Cohesion: 0.05
Nodes (21): IAudioSettingsData, IObserver, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, MusicType (+13 more)

### Community 23 - "CameraService"
Cohesion: 0.12
Nodes (8): bool, Camera, float, Tween, CameraService, CinemachineCamera, CinemachineThirdPersonFollow, Ease

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.13
Nodes (6): IEquipmentRoute, IInventoryRoute, IPauseNavigationRoute, IPauseNavigationRouteNavigation, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 28 - "PauseMenuUiController"
Cohesion: 0.15
Nodes (4): ICoreGameOrchestrator, IPauseMenuPresenter, PauseMenuUi, PauseMenuUiController

### Community 29 - "AmbienceService"
Cohesion: 0.07
Nodes (26): AudioClip, float, IAudioService, IAudioSettingsData, MusicType, SceneType, SfxType, AmbienceService (+18 more)

### Community 31 - "AttackComponent"
Cohesion: 0.14
Nodes (15): AnimatorStateMachineDto, AttackType, bool, float, HandMode, AttackComponent, AttackExecutionContext, AttackResolution (+7 more)

### Community 32 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "AnimatorRootMotionRelay"
Cohesion: 0.17
Nodes (7): Animator, bool, string, AnimatorRootMotionRelay, Quaternion, Vector3, IRootMotionSink

### Community 36 - "IDisposable"
Cohesion: 0.27
Nodes (7): CharacterActions, IInputService, InputService, IDisposable, InputAction, ProjectInputActions, UIActions

### Community 37 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.16
Nodes (8): bool, float, DamageResult, HealthModel, bool, float, HealthStats, SoulsLike.Entities.Character.Components.Health

### Community 38 - "MainMenuUiController"
Cohesion: 0.09
Nodes (10): IMainMenuOrchestrator, IContainerBuilder, MainMenuScope, IMainMenuPresenter, MainMenuUi, MainMenuUiController, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu (+2 more)

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
Cohesion: 0.12
Nodes (3): string, InventoryEntryId, IEquipmentPresenter

### Community 43 - "BaseUi"
Cohesion: 0.17
Nodes (5): bool, CanvasGroup, Transform, BaseUi, IBaseUi

### Community 44 - "EquipmentPresentation"
Cohesion: 0.20
Nodes (8): bool, EquipmentLoadout, EquippedItemContext, Quaternion, Transform, Vector3, EquipmentPresentation, GameObject

### Community 46 - ".Hide"
Cohesion: 0.13
Nodes (9): Camera, RectTransform, Transform, Vector3, LockOnUi, bool, Camera, LockOnUiController (+1 more)

### Community 47 - "InventoryViewStateController"
Cohesion: 0.33
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 50 - "SoulsLike.Entities.Character.Ports"
Cohesion: 0.20
Nodes (5): LandingType, LocomotionState, MovementMode, SoulsLike.Entities.Character.Ports, SoulsLike.Entities.Character.Components.Movement

### Community 51 - "InventoryUiController"
Cohesion: 0.19
Nodes (5): InventoryPrimaryCategory, InventorySubCategory, bool, InventoryUiController, HashSet

### Community 52 - "EquipmentSlotHud"
Cohesion: 0.22
Nodes (8): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud

### Community 53 - "ICameraService"
Cohesion: 0.12
Nodes (4): Transform, Vector2, ICameraService, Ray

### Community 54 - "PlayerHudUiController"
Cohesion: 0.26
Nodes (3): EquipmentSlotChange, IPlayerHudPresenter, PlayerHudUiController

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.15
Nodes (10): BaseEventData, Color, GameObject, Image, MPImage, PointerEventData, TMP_Text, EquipmentSlotUI (+2 more)

### Community 56 - "PlayerController"
Cohesion: 0.14
Nodes (9): Transform, ICameraService, IInputService, PlayerController, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable (+1 more)

### Community 58 - "MainMenuOrchestrator"
Cohesion: 0.18
Nodes (4): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator

### Community 59 - ".Move"
Cohesion: 0.15
Nodes (3): Inject, Vector2, IMovementPresentationSink

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 62 - "TargetLockNode"
Cohesion: 0.21
Nodes (7): Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService, TargetingService

### Community 65 - "MovementModel"
Cohesion: 0.29
Nodes (5): AnimatorModel, AnimationCurve, LayerMask, MovementModel, Model

### Community 66 - ".TryStartRoll"
Cohesion: 0.26
Nodes (3): Quaternion, Vector2, Vector3

### Community 68 - "FileService"
Cohesion: 0.36
Nodes (3): string, FileService, SoulsLike.Services.Save

### Community 70 - "InventoryItemSO"
Cohesion: 0.29
Nodes (7): bool, float, int, Sprite, string, InventoryItemSO, ScalingGrade

### Community 72 - ".Tick"
Cohesion: 0.15
Nodes (3): AnimatorStateMachineDto, AttackRequest, CharacterInputBatch

### Community 73 - "CharacterActionStateMachine"
Cohesion: 0.32
Nodes (5): bool, CharacterActionStateMachine, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind

### Community 74 - ".BuildLoadout"
Cohesion: 0.16
Nodes (12): EquipmentLoadout, EquipmentSlotGroup, AnimatorStateMachineDto, CharacterCommandExecutionStatus, EquipmentComponent, EquipmentLoadout, EquipmentSlotGroup, EquippedItemContext (+4 more)

### Community 75 - "InventoryComponent"
Cohesion: 0.12
Nodes (15): Inject, IReadOnlyList, InventoryComponent, IReadOnlyList, List, InitialInventoryEntry, InventoryData, Collider (+7 more)

### Community 76 - "SceneType"
Cohesion: 0.18
Nodes (7): UniTask, UniTaskVoid, GameOrchestrator, SceneType, UniTask, ISceneService, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "WeaponDefinition"
Cohesion: 0.08
Nodes (26): float, WeaponRuntime, float, ConsumableDefinition, float, int, string, AttributeRequirements (+18 more)

### Community 79 - "CharacterRuntime"
Cohesion: 0.23
Nodes (6): bool, CharacterRuntime, MovementGate, MovementGateReason, MovementPolicy, CharacterActionStateMachine

### Community 80 - "Data"
Cohesion: 0.28
Nodes (5): HealthData, IHealthData, Data, Model, SoulsLike.Model

### Community 81 - "SoulsLike.Entities.Character.Runtime"
Cohesion: 0.22
Nodes (5): AnimatorStateMachineDto, CharacterAnimationAdapter, UnityCharacterClock, ICharacterClock, SoulsLike.Entities.Character.Runtime

### Community 82 - "DamageRequest"
Cohesion: 0.19
Nodes (7): float, int, Vector3, DamageRequest, bool, float, HealthStatUpdate

### Community 84 - ".Read"
Cohesion: 0.10
Nodes (11): PlayerCharacterInputAdapter, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, SoulsLike.Entities.Character.Input, float, HeavyAttackGestureResolver (+3 more)

### Community 86 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.32
Nodes (3): SceneDependency, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 87 - "CharacterCommand"
Cohesion: 0.29
Nodes (4): float, CharacterCommand, CharacterCommandBuffer, CharacterCommandDisposition

### Community 90 - ".ApplyAnimationProfile"
Cohesion: 0.17
Nodes (4): HandMode, EquipmentLoadout, RuntimeAnimatorController, AnimationProfile

### Community 92 - "UiService"
Cohesion: 0.24
Nodes (6): Inject, List, Transform, IUiService, UiService, Canvas

### Community 93 - "CoreGameOrchestrator"
Cohesion: 0.11
Nodes (8): CharacterFactory, List, CoreGameOrchestrator, GameState, IGameStateNotifier, IGameStateObserver, BaseFactory, string

### Community 95 - ".UpdateState"
Cohesion: 0.33
Nodes (3): AnimatorStateMachineDto, AnimatorStateMachineDto, IAnimationStateSink

### Community 99 - "ICharacterActionExecutor"
Cohesion: 0.29
Nodes (5): Vector2, CharacterControlFrame, CharacterInputBatch, ICharacterActionExecutor, RollRequest

### Community 101 - "UiFactory"
Cohesion: 0.22
Nodes (7): LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory, SoulsLike.Factory, IObjectResolver

### Community 102 - "SoulsLike.Services"
Cohesion: 0.14
Nodes (9): IContainerBuilder, CoreScope, IContainerBuilder, SharedSceneScope, SoulsLike.Services.Targeting, SoulsLike.Services, SoulsLike.Services.CameraService, SoulsLike.Ui.LockOn (+1 more)

### Community 134 - ".DisplayItemDetails"
Cohesion: 0.29
Nodes (3): CharacterAttributeStats, ScalingGrade, TMP_Text

### Community 144 - "AddressableAssetService"
Cohesion: 0.11
Nodes (11): GameObject, AddressableAssetService, IAssetService, CanvasGroup, CanvasGroupExt, AssetMappingData, IContainerBuilder, VContainerExt (+3 more)

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 146 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 149 - "ScriptableObject"
Cohesion: 0.33
Nodes (4): CombatProfile, float, AudioData, ScriptableObject

### Community 157 - "IMovementData"
Cohesion: 0.70
Nodes (4): AnimationCurve, LayerMask, IMovementData, MovementData

### Community 160 - "MonoBehaviour"
Cohesion: 0.83
Nodes (3): BaseComponent, IComponent, MonoBehaviour

### Community 167 - ".PlayAttack"
Cohesion: 0.19
Nodes (3): AttackType, Vector2, Vector2

### Community 172 - "SoulsLike.Ui.Base"
Cohesion: 0.20
Nodes (3): UiController, SoulsLike.Ui.PlayerHud, SoulsLike.Ui.Base

## Knowledge Gaps
- **11 isolated node(s):** `SpeedMultiplierKey`, `AttackType`, `LandingType`, `LocomotionState`, `MovementMode` (+6 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **90 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `PlayerCharacterInputAdapter` (5× useful, score=4.866808065)
- `AmbienceData` (2× useful, score=1.964537034)
- `CharacterCommandFactory` (2× useful, score=1.952224446)
- `ICharacterCommand` (2× useful, score=1.951553309)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `MonoBehaviour`, `HealthComponent`, `.SetAirborneMotion`, `IInitializable`, `.PlayAttack`, `.Tick`, `AnimatorComponent`, `.BuildLoadout`, `EquipmentPresentation`, `EquipmentUiController`, `EquipmentComponent`, `SoulsLike.Entities.Character.Components`, `.Refresh`, `InventoryUiController`, `.SetMovementBlocked`, `PlayerController`, `.ApplyAnimationProfile`, `CoreGameOrchestrator`?**
  _High betweenness centrality (0.226) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `.TryStartRoll`, `IInitializable`, `ITimer`, `EquipmentComponent`, `SoulsLike.Entities.Character.Ports`, `.SetGrounded`, `.SetMovementBlocked`, `.Move`?**
  _High betweenness centrality (0.081) - this node is a cross-community bridge._
- **Why does `InventoryUiController` connect `InventoryUiController` to `SoulsLike.Items`, `IInventoryPresenter`, `IDisposable`, `ItemDefinition`, `IInitializable`, `MainMenuUiController`, `InventoryEntryId`, `InventoryComponent`, `Character`, `InventoryItemViewData`, `.Hide`, `SceneType`, `InventoryUi`, `EquipmentComponent`, `.Refresh`, `InventoryEntry`, `PlayerHudUiController`, `PauseNavigationUiController`?**
  _High betweenness centrality (0.079) - this node is a cross-community bridge._
- **What connects `SpeedMultiplierKey`, `AttackType`, `LandingType` to the rest of the system?**
  _11 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.14705882352941177 - nodes in this community are weakly interconnected._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.08246225319396051 - nodes in this community are weakly interconnected._
- **Should `IInitializable` be split into smaller, more focused modules?**
  _Cohesion score 0.06845513413506013 - nodes in this community are weakly interconnected._