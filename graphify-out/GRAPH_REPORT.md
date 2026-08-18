# Graph Report - SoulsLikeTemplate  (2026-08-18)

## Corpus Check
- 184 files · ~33,252 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1743 nodes · 3224 edges · 168 communities (89 shown, 79 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 250 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `7ba043e4`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- ITimer
- IDisposable
- AnimatorStateMachineReceiver
- PreviewRenderService
- SceneType
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
- .Move
- InventoryEntry
- AudioService
- CameraService
- PauseNavigationUiController
- SoulsLike
- OnGuiFpsCounter
- CharacterActions
- GameState
- AmbienceService
- Animator
- AttackComponent
- CoroutineService
- CustomButtonToggle
- AnimatorRootMotionRelay
- SpeedMultiplierKey
- Inject
- TargetLockNode
- AudioData
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntryId
- BaseUi
- MainMenuUi
- AmbienceData
- .Hide
- CharacterRuntime.cs
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
- CoreScope
- CustomButtonMapping
- PlayerHudUiController
- AmbienceManagerWrapper
- MusicType
- .TriggerRoll
- .Read
- InventoryViewStateController
- FileService
- ICustomButton
- InventoryItemSO
- PauseNavigationUi
- .PlayAttack
- LockOnUi
- ICharacterActionExecutor
- ItemDefinition
- GameOrchestrator
- CustomButtonEditor
- MonoBehaviour
- SoulsLike.Entities.Character.Ports
- IPauseNavigationRouteNavigation
- SoulsLike.Entities.Character.Runtime
- DamageRequest
- InputService
- IPauseMenuPresenter
- .OnAnimationStateChanged
- SoulsLike.Services
- Transform
- SharedSceneScope
- .TryStartRoll
- .ApplyAnimationProfile
- .ApplyAnimationMovement
- UiService
- MovementModel
- CharacterRuntime
- IAnimationStateSink
- CharacterFactory
- CoreGameOrchestrator
- .ShowPicker
- AttackType
- .SetAirborneMotion
- HandMode
- Inject
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
- MainMenuUiController
- .HasParameter
- .SetGrounded
- VContainerExt
- .Build
- HealthStatUpdate
- InventoryChange
- Camera
- int
- LandingType
- Vector3
- string
- CharacterActionStateId
- CharacterInputBatch
- CharacterCommandFactory
- EquipmentComponent
- EquipmentSwapCoordinator

## God Nodes (most connected - your core abstractions)
1. `Character` - 58 edges
2. `AnimatorComponent` - 55 edges
3. `MovementComponent` - 51 edges
4. `EquipmentUi` - 37 edges
5. `InventoryUiController` - 37 edges
6. `InventoryEntryId` - 34 edges
7. `EquipmentUiController` - 33 edges
8. `EquipmentSlotUI` - 33 edges
9. `InventoryUi` - 31 edges
10. `EquipmentComponent` - 31 edges

## Surprising Connections (you probably didn't know these)
- `Character` --references--> `CharacterActionStateId`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs
- `Character` --references--> `CharacterRuntime`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs
- `Character` --implements--> `ICharacterActionExecutor`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs
- `PlayerController` --references--> `Character`  [EXTRACTED]
  Assets/Scripts/Entities/Character/PlayerController.cs → Assets/Scripts/Entities/Character/Character.cs
- `EquipmentUiController` --references--> `Character`  [EXTRACTED]
  Assets/Scripts/Ui/Equipment/EquipmentUiController.cs → Assets/Scripts/Entities/Character/Character.cs

## Import Cycles
- None detected.

## Communities (168 total, 79 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.06
Nodes (29): IReadOnlyList, List, InitialInventoryEntry, InventoryData, AnimationCurve, LayerMask, IMovementData, MovementData (+21 more)

### Community 1 - "SceneReference"
Cohesion: 0.13
Nodes (9): SceneDependency, bool, string, SceneReference, IComparable, IEquatable, ISerializationCallbackReceiver, Obsolete (+1 more)

### Community 2 - "HealthComponent"
Cohesion: 0.08
Nodes (13): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+5 more)

### Community 3 - "ITimer"
Cohesion: 0.11
Nodes (7): LandingType, ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 4 - "IDisposable"
Cohesion: 0.07
Nodes (21): List, Entity, IEntity, EntityCommand, Collider, Dictionary, RaycastHit, EntityLocator (+13 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (19): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+11 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.06
Nodes (24): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerMask, ILayerService (+16 more)

### Community 7 - "SceneType"
Cohesion: 0.19
Nodes (6): Scene, SerializedDictionary, SceneData, Scene, SceneModel, SceneType

### Community 8 - "PlayerHudUi"
Cohesion: 0.20
Nodes (8): bool, Color, float, MPImage, RectTransform, PlayerHudUi, StatBar, StatBar

### Community 9 - "EquipmentSlotId"
Cohesion: 0.12
Nodes (10): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode, CharacterAttributeStats (+2 more)

### Community 10 - "AnimatorComponent"
Cohesion: 0.12
Nodes (15): AnimationEvent, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, bool, float, IAnimationStateSink, int (+7 more)

### Community 11 - "Character"
Cohesion: 0.10
Nodes (20): EquipmentSwapCoordinator, SwapPhase, AnimatorComponent, EquipmentComponent, float, Character, AttackComponent, CharacterAnimationAdapter (+12 more)

### Community 12 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.14
Nodes (10): bool, float, DamageResult, HealthData, IHealthData, HealthModel, bool, float (+2 more)

### Community 13 - "MovementComponent"
Cohesion: 0.13
Nodes (13): bool, float, ITimer, LandingType, Transform, MovementComponent, CharacterController, Dictionary (+5 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.15
Nodes (8): Dictionary, GameObject, Image, int, List, TMP_Text, Transform, EquipmentUi

### Community 16 - "InventoryUi"
Cohesion: 0.13
Nodes (10): Color, Image, int, IReadOnlyList, List, ScalingGrade, TMP_Text, Transform (+2 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.10
Nodes (15): EquipmentLoadout, HandMode, Inject, InventoryComponent, EquipmentComponent, EquipmentLoadout, IEquipmentLoadoutSink, EquipmentModel (+7 more)

### Community 18 - "CharacterActionStateMachine"
Cohesion: 0.25
Nodes (5): bool, CharacterActionStateMachine, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+3 more)

### Community 20 - ".Move"
Cohesion: 0.15
Nodes (3): Inject, Vector2, IMovementPresentationSink

### Community 21 - "InventoryEntry"
Cohesion: 0.22
Nodes (6): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryModel

### Community 22 - "AudioService"
Cohesion: 0.16
Nodes (8): List, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, IAudioService, IObserver

### Community 23 - "CameraService"
Cohesion: 0.11
Nodes (8): bool, float, Transform, Tween, CameraService, CinemachineCamera, CinemachineThirdPersonFollow, Ease

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.18
Nodes (5): IEquipmentRoute, IInventoryRoute, IPauseNavigationRoute, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.16
Nodes (7): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, SoulsLike

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 28 - "GameState"
Cohesion: 0.15
Nodes (3): GameState, IGameStateNotifier, IGameStateObserver

### Community 29 - "AmbienceService"
Cohesion: 0.19
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

### Community 37 - "TargetLockNode"
Cohesion: 0.21
Nodes (7): Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService, TargetingService

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
Cohesion: 0.15
Nodes (4): EquipmentSlotChange, string, InventoryEntryId, IEquipmentPresenter

### Community 43 - "BaseUi"
Cohesion: 0.17
Nodes (5): bool, CanvasGroup, Transform, BaseUi, IBaseUi

### Community 44 - "MainMenuUi"
Cohesion: 0.16
Nodes (4): IMainMenuPresenter, MainMenuUi, SoulsLike.Ui.MainMenu, IStartable

### Community 45 - "AmbienceData"
Cohesion: 0.19
Nodes (9): float, AmbienceData, MusicEntry, SfxEntry, SfxType, AudioClip, MusicEntry, SceneMusicEntry (+1 more)

### Community 46 - ".Hide"
Cohesion: 0.20
Nodes (5): bool, Camera, LockOnUiController, IPostLateTickable, UiController

### Community 47 - "CharacterRuntime.cs"
Cohesion: 0.16
Nodes (15): float, Vector2, AttackIntent, AttackRequest, CharacterCommand, CharacterCommandBuffer, CharacterCommandDisposition, CharacterCommandKind (+7 more)

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 51 - "InventoryUiController"
Cohesion: 0.13
Nodes (8): EquipmentGroup, InventoryPrimaryCategory, InventorySubCategory, Action, bool, IReadOnlyCollection, InventoryUiController, HashSet

### Community 53 - "ICameraService"
Cohesion: 0.18
Nodes (3): Vector2, ICameraService, Ray

### Community 54 - "IInitializable"
Cohesion: 0.16
Nodes (5): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator, IInitializable

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.15
Nodes (10): BaseEventData, Color, GameObject, Image, MPImage, PointerEventData, TMP_Text, EquipmentSlotUI (+2 more)

### Community 56 - "PlayerController"
Cohesion: 0.14
Nodes (9): ICameraService, IInputService, PlayerController, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable, ITargetingService (+1 more)

### Community 58 - "EquipmentSlotHud"
Cohesion: 0.20
Nodes (8): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud

### Community 59 - "UiFactory"
Cohesion: 0.18
Nodes (8): LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory, Inject, SoulsLike.Factory, IObjectResolver

### Community 60 - "CoreScope"
Cohesion: 0.15
Nodes (8): IContainerBuilder, CoreScope, IContainerBuilder, MainMenuScope, IContainerBuilder, ProjectScope, SoulsLike.Orchestrators.MainMenu, LifetimeScope

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 63 - "AmbienceManagerWrapper"
Cohesion: 0.27
Nodes (3): float, AmbienceManagerWrapper, Component

### Community 64 - "MusicType"
Cohesion: 0.18
Nodes (3): SceneMusicEntry, MusicType, IAmbienceSystem

### Community 66 - ".Read"
Cohesion: 0.11
Nodes (11): PlayerCharacterInputAdapter, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, SoulsLike.Entities.Character.Input, float, HeavyAttackGestureResolver (+3 more)

### Community 67 - "InventoryViewStateController"
Cohesion: 0.39
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 68 - "FileService"
Cohesion: 0.36
Nodes (3): string, FileService, SoulsLike.Services.Save

### Community 70 - "InventoryItemSO"
Cohesion: 0.29
Nodes (7): bool, float, int, Sprite, string, InventoryItemSO, ScalingGrade

### Community 73 - "LockOnUi"
Cohesion: 0.32
Nodes (5): Camera, RectTransform, Transform, Vector3, LockOnUi

### Community 74 - "ICharacterActionExecutor"
Cohesion: 0.23
Nodes (5): AnimatorComponent, EquipmentComponent, CharacterCommandExecutionResult, CharacterCommandExecutionStatus, ICharacterActionExecutor

### Community 75 - "ItemDefinition"
Cohesion: 0.10
Nodes (18): Inject, IReadOnlyList, InventoryComponent, Collider, int, GroundItem, Dictionary, IReadOnlyList (+10 more)

### Community 76 - "GameOrchestrator"
Cohesion: 0.20
Nodes (6): UniTask, UniTaskVoid, GameOrchestrator, UniTask, ISceneService, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "MonoBehaviour"
Cohesion: 0.06
Nodes (36): BaseComponent, IComponent, EquipmentLoadout, EquippedItemContext, GameObject, Quaternion, Transform, Vector3 (+28 more)

### Community 79 - "SoulsLike.Entities.Character.Ports"
Cohesion: 0.20
Nodes (5): LandingType, LocomotionState, MovementMode, SoulsLike.Entities.Character.Ports, SoulsLike.Entities.Character.Components.Movement

### Community 81 - "SoulsLike.Entities.Character.Runtime"
Cohesion: 0.33
Nodes (3): UnityCharacterClock, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Runtime

### Community 82 - "DamageRequest"
Cohesion: 0.40
Nodes (4): float, int, Vector3, DamageRequest

### Community 83 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 85 - ".OnAnimationStateChanged"
Cohesion: 0.17
Nodes (5): AnimatorStateMachineDto, StateMachineName, CharacterAnimationAdapter, AnimatorStateMachineDto, AnimatorStateMachineDto

### Community 86 - "SoulsLike.Services"
Cohesion: 0.29
Nodes (5): SoulsLike.Services, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes, SoulsLike.Services.Audio.Data, SoulsLike.Services.Audio

### Community 88 - "SharedSceneScope"
Cohesion: 0.18
Nodes (6): IContainerBuilder, SharedSceneScope, CanvasGroup, CanvasGroupExt, SoulsLike.Services.Repository, SoulsLike.Extensions

### Community 90 - ".ApplyAnimationProfile"
Cohesion: 0.20
Nodes (3): AnimationProfile, EquipmentLoadout, HandMode

### Community 91 - ".ApplyAnimationMovement"
Cohesion: 0.29
Nodes (3): Quaternion, Quaternion, Vector3

### Community 92 - "UiService"
Cohesion: 0.21
Nodes (6): UiController, List, Transform, IUiService, UiService, Canvas

### Community 93 - "MovementModel"
Cohesion: 0.20
Nodes (6): AnimatorModel, AnimationCurve, LayerMask, MovementModel, SoulsLike.Entities.Character.Components, Model

### Community 94 - "CharacterRuntime"
Cohesion: 0.12
Nodes (5): bool, CharacterRuntime, MovementGate, MovementGateReason, MovementPolicy

### Community 95 - "IAnimationStateSink"
Cohesion: 0.33
Nodes (3): AnimatorStateMachineDto, AnimatorStateMachineDto, IAnimationStateSink

### Community 96 - "CharacterFactory"
Cohesion: 0.33
Nodes (4): CharacterFactory, BaseFactory, GameObject, string

### Community 144 - "AddressableAssetService"
Cohesion: 0.24
Nodes (3): GameObject, AddressableAssetService, IAssetService

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 146 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 151 - "VContainerExt"
Cohesion: 0.47
Nodes (4): AssetMappingData, IContainerBuilder, VContainerExt, RegistrationBuilder

### Community 152 - ".Build"
Cohesion: 0.40
Nodes (3): Dictionary, UnityDictionaryFactory, IEnumerable

### Community 154 - "HealthStatUpdate"
Cohesion: 0.50
Nodes (3): bool, float, HealthStatUpdate

## Knowledge Gaps
- **10 isolated node(s):** `SwapPhase`, `SpeedMultiplierKey`, `Model`, `LandingType`, `LocomotionState` (+5 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **79 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `CharacterActionStateMachine` (5× useful, score=4.968713738)
- `PlayerCharacterInputAdapter` (4× useful, score=3.983211069)
- `Character` (3× useful, score=2.998205928) _(code changed — re-verify)_
- `CharacterRuntime` (2× useful, score=1.999056406)
- `RollState` (2× useful, score=1.97050781)
- `SprintRollGestureResolver` (2× useful, score=1.97050781)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `HealthComponent`, `AnimatorComponent`, `EquipmentUiController`, `CharacterActionStateMachine`, `.SetCrouch`, `.SetTurn`, `CharacterRuntime.cs`, `InventoryUiController`, `IInitializable`, `PlayerController`, `.TriggerRoll`, `.PlayAttack`, `ICharacterActionExecutor`, `MonoBehaviour`, `SoulsLike.Entities.Character.Runtime`, `.OnAnimationStateChanged`, `.ApplyAnimationProfile`, `.ApplyAnimationMovement`, `CharacterRuntime`, `CharacterFactory`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.229) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `ITimer`, `SoulsLike.Entities.Character.Ports`, `.Move`, `.SetGrounded`, `IInitializable`, `.TryStartRoll`, `.ApplyAnimationMovement`, `CharacterRuntime`, `AttackComponent`?**
  _High betweenness centrality (0.106) - this node is a cross-community bridge._
- **Why does `EquipmentUiController` connect `EquipmentUiController` to `SoulsLike.Items`, `IDisposable`, `EquipmentSlotId`, `InventoryEntryId`, `ItemDefinition`, `Character`, `GameOrchestrator`, `.Hide`, `EquipmentUi`, `EquipmentComponent`, `InputService`, `IInitializable`, `PauseNavigationUiController`?**
  _High betweenness centrality (0.100) - this node is a cross-community bridge._
- **What connects `SwapPhase`, `SpeedMultiplierKey`, `Model` to the rest of the system?**
  _10 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SoulsLike.Items` be split into smaller, more focused modules?**
  _Cohesion score 0.0625 - nodes in this community are weakly interconnected._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.1286549707602339 - nodes in this community are weakly interconnected._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.08246225319396051 - nodes in this community are weakly interconnected._