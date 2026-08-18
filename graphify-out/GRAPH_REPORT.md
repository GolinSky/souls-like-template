# Graph Report - SoulsLikeTemplate  (2026-08-18)

## Corpus Check
- 187 files · ~33,781 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1748 nodes · 3237 edges · 169 communities (94 shown, 75 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 255 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `68cbfa40`
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
- CoreGameOrchestrator
- AmbienceService
- Animator
- AttackComponent
- CoroutineService
- CustomButtonToggle
- AnimatorRootMotionRelay
- SpeedMultiplierKey
- LayerData
- MonoBehaviour
- ScriptableObject
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntryId
- BaseUi
- MainMenuUiController
- AmbienceData
- LockOnUiController
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
- CoreScope.cs
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
- EquipmentPresentation
- CharacterCommandExecutionStatus
- InventoryComponent
- GameOrchestrator
- CustomButtonEditor
- ItemTypes.cs
- SoulsLike.Entities.Character.Ports
- InventoryData
- SoulsLike.Entities.Character.Runtime
- DamageRequest
- InputService
- WeaponDefinition
- StateMachineName
- SoulsLike.Services
- .HandleLockOnInput
- VContainerExt
- .TryStartRoll
- .ApplyAnimationProfile
- Vector3
- UiService
- BaseComponent
- CharacterRuntime
- IAnimationStateSink
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
- IMovementData
- .HasParameter
- .SetGrounded
- MovementGate
- InventoryEntry.cs
- AudioSettingsData
- SpeedMultiplierKey.cs
- int
- AnimatorComponent
- int
- EquipmentComponent
- CharacterAnimationAdapter
- string
- CharacterActionStateId
- CharacterInputBatch
- CharacterCommandFactory
- Inject
- LandingType
- Transform

## God Nodes (most connected - your core abstractions)
1. `AnimatorComponent` - 58 edges
2. `Character` - 58 edges
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
- `Character` --references--> `CharacterActionStateId`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs
- `Character` --references--> `CharacterRuntime`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs
- `Character` --implements--> `ICharacterActionExecutor`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs
- `PlayerController` --references--> `Character`  [EXTRACTED]
  Assets/Scripts/Entities/Character/PlayerController.cs → Assets/Scripts/Entities/Character/Character.cs

## Import Cycles
- None detected.

## Communities (169 total, 75 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.18
Nodes (9): SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character, SoulsLike.Ui.PlayerHud, SoulsLike.Ui.Inventory, SoulsLike.Ui.Base (+1 more)

### Community 1 - "SceneReference"
Cohesion: 0.13
Nodes (9): SceneDependency, bool, string, SceneReference, IComparable, IEquatable, ISerializationCallbackReceiver, Obsolete (+1 more)

### Community 2 - "HealthComponent"
Cohesion: 0.08
Nodes (14): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+6 more)

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
Cohesion: 0.09
Nodes (12): LayerMask, ILayerService, IPreviewRenderService, Camera, Inject, Transform, Vector3, PreviewRenderService (+4 more)

### Community 7 - "SceneType"
Cohesion: 0.21
Nodes (6): Scene, SerializedDictionary, SceneData, Scene, SceneModel, SceneType

### Community 8 - "PlayerHudUi"
Cohesion: 0.20
Nodes (8): bool, Color, float, MPImage, RectTransform, PlayerHudUi, StatBar, StatBar

### Community 9 - "EquipmentSlotId"
Cohesion: 0.12
Nodes (10): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode, CharacterAttributeStats (+2 more)

### Community 10 - "AnimatorComponent"
Cohesion: 0.12
Nodes (15): AnimationEvent, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, bool, float, IAnimationStateSink, Inject (+7 more)

### Community 11 - "Character"
Cohesion: 0.10
Nodes (21): AnimatorStateMachineDto, CharacterAnimationAdapter, float, Inject, Vector3, Character, AttackComponent, CharacterAttributeStats (+13 more)

### Community 12 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.16
Nodes (8): bool, float, DamageResult, HealthModel, bool, float, HealthStats, SoulsLike.Entities.Character.Components.Health

### Community 13 - "MovementComponent"
Cohesion: 0.12
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
Cohesion: 0.19
Nodes (8): bool, CharacterActionStateMachine, float, Vector2, CharacterCommandBuffer, CharacterControlFrame, CharacterInputBatch, ICharacterActionExecutor

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+3 more)

### Community 20 - ".Move"
Cohesion: 0.14
Nodes (3): Inject, Vector2, IMovementPresentationSink

### Community 21 - "InventoryEntry"
Cohesion: 0.24
Nodes (7): InventoryEntry, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "AudioService"
Cohesion: 0.21
Nodes (5): List, AudioService, IAudioSettingsData, IAudioService, IObserver

### Community 23 - "CameraService"
Cohesion: 0.12
Nodes (8): bool, Camera, float, Tween, CameraService, CinemachineCamera, CinemachineThirdPersonFollow, Ease

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.13
Nodes (8): IEquipmentRoute, Action, IReadOnlyCollection, IInventoryRoute, IPauseNavigationRoute, IReadOnlyCollection, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 28 - "CoreGameOrchestrator"
Cohesion: 0.14
Nodes (5): List, CoreGameOrchestrator, GameState, IGameStateNotifier, IGameStateObserver

### Community 29 - "AmbienceService"
Cohesion: 0.18
Nodes (5): float, GameObject, Tween, AmbienceService, AudioSource

### Community 31 - "AttackComponent"
Cohesion: 0.15
Nodes (15): AnimatorStateMachineDto, AttackType, bool, float, HandMode, ITimer, StateMachineName, AttackComponent (+7 more)

### Community 32 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "AnimatorRootMotionRelay"
Cohesion: 0.17
Nodes (7): Animator, bool, string, AnimatorRootMotionRelay, Quaternion, Vector3, IRootMotionSink

### Community 36 - "LayerData"
Cohesion: 0.15
Nodes (12): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerName, GameObject (+4 more)

### Community 37 - "MonoBehaviour"
Cohesion: 0.17
Nodes (9): Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService, TargetingService, SoulsLike.Services.Targeting (+1 more)

### Community 38 - "ScriptableObject"
Cohesion: 0.22
Nodes (6): RuntimeAnimatorController, AnimationProfile, CombatProfile, float, AudioData, ScriptableObject

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
Cohesion: 0.13
Nodes (5): bool, CanvasGroup, Transform, BaseUi, IBaseUi

### Community 44 - "MainMenuUiController"
Cohesion: 0.15
Nodes (5): IMainMenuOrchestrator, IMainMenuPresenter, MainMenuUi, MainMenuUiController, IStartable

### Community 45 - "AmbienceData"
Cohesion: 0.19
Nodes (10): float, AmbienceData, MusicEntry, SceneMusicEntry, SfxEntry, SfxType, AudioClip, MusicEntry (+2 more)

### Community 46 - "LockOnUiController"
Cohesion: 0.13
Nodes (11): Camera, RectTransform, Transform, Vector3, LockOnUi, bool, Camera, LockOnUiController (+3 more)

### Community 47 - "CharacterRuntime.cs"
Cohesion: 0.16
Nodes (12): AttackIntent, AttackRequest, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind, CharacterCommand, CharacterCommandDisposition, CharacterCommandExecutionResult (+4 more)

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 51 - "InventoryUiController"
Cohesion: 0.17
Nodes (6): InventorySubCategory, Action, bool, IReadOnlyCollection, InventoryUiController, HashSet

### Community 52 - "PauseMenuUiController"
Cohesion: 0.13
Nodes (4): ICoreGameOrchestrator, IPauseMenuPresenter, PauseMenuUi, PauseMenuUiController

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
Cohesion: 0.20
Nodes (8): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud

### Community 59 - "UiFactory"
Cohesion: 0.18
Nodes (8): LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory, Inject, SoulsLike.Factory, IObjectResolver

### Community 60 - "CoreScope.cs"
Cohesion: 0.14
Nodes (8): IContainerBuilder, CoreScope, IContainerBuilder, ProjectScope, IContainerBuilder, SharedSceneScope, SoulsLike.Services.CameraService, LifetimeScope

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 63 - "AmbienceManagerWrapper"
Cohesion: 0.27
Nodes (3): float, AmbienceManagerWrapper, Component

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
Cohesion: 0.18
Nodes (8): InventoryPrimaryCategory, bool, float, int, Sprite, string, InventoryItemSO, ScalingGrade

### Community 71 - "PauseNavigationUi"
Cohesion: 0.15
Nodes (4): IPauseNavigationPresenter, IPauseNavigationRouteNavigation, PauseNavigationUi, SoulsLike.Ui.PauseNavigation

### Community 73 - "EquipmentPresentation"
Cohesion: 0.25
Nodes (7): EquipmentLoadout, EquippedItemContext, GameObject, Quaternion, Transform, Vector3, EquipmentPresentation

### Community 74 - "CharacterCommandExecutionStatus"
Cohesion: 0.20
Nodes (8): AnimatorComponent, EquipmentComponent, EquipmentSwapCoordinator, SwapPhase, CharacterCommandExecutionStatus, RollRequest, EquipmentSlotGroup, SwapPhase

### Community 75 - "InventoryComponent"
Cohesion: 0.15
Nodes (11): Inject, IReadOnlyList, InventoryComponent, Collider, int, GroundItem, Dictionary, IReadOnlyList (+3 more)

### Community 76 - "GameOrchestrator"
Cohesion: 0.18
Nodes (6): UniTask, UniTaskVoid, GameOrchestrator, UniTask, ISceneService, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "ItemTypes.cs"
Cohesion: 0.16
Nodes (14): float, ConsumableDefinition, float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot (+6 more)

### Community 79 - "SoulsLike.Entities.Character.Ports"
Cohesion: 0.20
Nodes (5): LandingType, LocomotionState, MovementMode, SoulsLike.Entities.Character.Ports, SoulsLike.Entities.Character.Components.Movement

### Community 80 - "InventoryData"
Cohesion: 0.19
Nodes (9): HealthData, IHealthData, IReadOnlyList, List, InitialInventoryEntry, InventoryData, Data, Model (+1 more)

### Community 81 - "SoulsLike.Entities.Character.Runtime"
Cohesion: 0.29
Nodes (4): UnityCharacterClock, ICharacterClock, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Runtime

### Community 82 - "DamageRequest"
Cohesion: 0.19
Nodes (7): float, int, Vector3, DamageRequest, bool, float, HealthStatUpdate

### Community 83 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 84 - "WeaponDefinition"
Cohesion: 0.19
Nodes (9): float, WeaponRuntime, bool, float, GameObject, int, Sprite, string (+1 more)

### Community 86 - "SoulsLike.Services"
Cohesion: 0.28
Nodes (5): SoulsLike.Services, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes, SoulsLike.Services.Audio.Data, SoulsLike.Services.Audio

### Community 88 - "VContainerExt"
Cohesion: 0.20
Nodes (7): CanvasGroup, CanvasGroupExt, AssetMappingData, IContainerBuilder, VContainerExt, SoulsLike.Extensions, RegistrationBuilder

### Community 89 - ".TryStartRoll"
Cohesion: 0.26
Nodes (3): Quaternion, Vector2, Vector3

### Community 90 - ".ApplyAnimationProfile"
Cohesion: 0.20
Nodes (3): AnimationProfile, EquipmentLoadout, HandMode

### Community 92 - "UiService"
Cohesion: 0.21
Nodes (6): UiController, List, Transform, IUiService, UiService, Canvas

### Community 93 - "BaseComponent"
Cohesion: 0.28
Nodes (5): AnimatorModel, BaseComponent, IComponent, SoulsLike.Entities.Character.Components, Model

### Community 94 - "CharacterRuntime"
Cohesion: 0.10
Nodes (5): AnimatorStateMachineDto, AnimatorStateMachineDto, bool, CharacterRuntime, CharacterActionStateMachine

### Community 95 - "IAnimationStateSink"
Cohesion: 0.33
Nodes (3): AnimatorStateMachineDto, AnimatorStateMachineDto, IAnimationStateSink

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

### Community 147 - "IMovementData"
Cohesion: 0.31
Nodes (7): AnimationCurve, LayerMask, IMovementData, MovementData, AnimationCurve, LayerMask, MovementModel

### Community 150 - "MovementGate"
Cohesion: 0.43
Nodes (3): MovementGate, MovementGateReason, MovementPolicy

### Community 152 - "AudioSettingsData"
Cohesion: 0.50
Nodes (3): bool, float, AudioSettingsData

## Knowledge Gaps
- **10 isolated node(s):** `SpeedMultiplierKey`, `Model`, `SwapPhase`, `AttackType`, `LandingType` (+5 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **75 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `PlayerCharacterInputAdapter` (5× useful, score=4.978904866)
- `CharacterActionStateMachine` (5× useful, score=4.964074422)
- `CharacterRuntime` (2× useful, score=1.997189876) _(code changed — re-verify)_
- `CharacterCommandFactory` (2× useful, score=1.997189876)
- `ICharacterCommand` (2× useful, score=1.99650328)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `HealthComponent`, `AnimatorComponent`, `EquipmentUiController`, `CharacterActionStateMachine`, `.Move`, `.SetJump`, `.SetCrouch`, `MonoBehaviour`, `CharacterRuntime.cs`, `InventoryUiController`, `IInitializable`, `PlayerController`, `.TriggerRoll`, `.PlayAttack`, `EquipmentPresentation`, `CharacterCommandExecutionStatus`, `SoulsLike.Entities.Character.Runtime`, `.HandleLockOnInput`, `.ApplyAnimationProfile`, `CharacterRuntime`, `CharacterFactory`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.240) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `HealthComponent`, `ITimer`, `SoulsLike.Entities.Character.Ports`, `.Move`, `.SetGrounded`, `IInitializable`, `.TryStartRoll`, `CharacterRuntime`?**
  _High betweenness centrality (0.086) - this node is a cross-community bridge._
- **Why does `EquipmentUiController` connect `EquipmentUiController` to `SoulsLike.Items`, `IDisposable`, `EquipmentSlotId`, `InventoryEntryId`, `InventoryComponent`, `Character`, `BaseUi`, `GameOrchestrator`, `EquipmentUi`, `LockOnUiController`, `EquipmentComponent`, `InputService`, `IInitializable`, `PauseNavigationUiController`?**
  _High betweenness centrality (0.075) - this node is a cross-community bridge._
- **What connects `SpeedMultiplierKey`, `Model`, `SwapPhase` to the rest of the system?**
  _10 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.1286549707602339 - nodes in this community are weakly interconnected._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.07973421926910298 - nodes in this community are weakly interconnected._
- **Should `ITimer` be split into smaller, more focused modules?**
  _Cohesion score 0.10952380952380952 - nodes in this community are weakly interconnected._