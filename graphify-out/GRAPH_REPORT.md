# Graph Report - SoulsLikeTemplate  (2026-08-18)

## Corpus Check
- 193 files · ~34,687 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1766 nodes · 3201 edges · 170 communities (84 shown, 86 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 251 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `6693f3d4`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- SoulsLike.Entities.BaseEntity
- IEntity
- AnimatorStateMachineReceiver
- PreviewRenderService
- SceneType
- PlayerHudUiController
- EquipmentSlotId
- AnimatorComponent
- Character
- AudioService
- MovementComponent
- EquipmentUiController
- EquipmentUi
- InventoryUi
- EquipmentComponent
- CharacterActionStateMachine
- InventorySlotUI
- IInitializable
- InventoryEntry
- SoulsLike.Services.Audio.Data
- CameraService
- PauseNavigationUiController
- KeyValue
- OnGuiFpsCounter
- CharacterActions
- .Tick
- AmbienceService
- Animator
- AttackComponent
- CoroutineService
- CustomButtonToggle
- AnimatorRootMotionRelay
- SpeedMultiplierKey
- InputService
- TargetLockNode
- ScriptableObject
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntryId
- BaseUi
- EquipmentPresentation
- AudioClip
- .Hide
- CharacterRuntime.cs
- System.Ui.Base
- .CreateButton
- SoulsLike.Entities.Character.Ports
- InventoryUiController
- .ShowPicker
- ICameraService
- WeaponDefinition
- EquipmentSlotUI
- PlayerController
- Vector2
- IGameOrchestrator
- AssetMappingData
- IContainerBuilder
- CustomButtonMapping
- .Open
- float
- .PlayAttack
- MovementModel
- .Read
- WeaponRuntime
- FileService
- ICustomButton
- InventoryItemSO
- PauseNavigationUi
- .Tick
- EquipmentSwapCoordinator
- ItemDefinition
- GameOrchestrator
- CustomButtonEditor
- ItemTypes.cs
- InventoryEntry.cs
- InventoryData
- SoulsLike.Entities.Character.Runtime
- SoulsLike.Entities.Character.Components.Health
- IMovementData
- LockOnUi
- StateMachineName
- SoulsLike.Services.Scenes.Data
- .HandleLockOnInput
- GameObject
- Tween
- .ApplyAnimationProfile
- List
- UiService
- CoreGameOrchestrator
- CharacterRuntime
- .UpdateState
- Component
- InventoryItemViewData
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
- SpeedMultiplierKey.cs
- ICharacterActionExecutor
- AnimatorStateMachineDto
- AnimatorComponent
- bool
- EquipmentComponent
- EquipmentSwapCoordinator
- int
- LandingType
- AnimatorComponent
- int
- Transform
- Vector2
- string
- CharacterActionStateId
- CharacterInputBatch
- CharacterCommandFactory
- Inject
- .TriggerRoll
- Vector3
- SoulsLike.Services
- MonoBehaviour

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
- `CoreGameOrchestrator` --references--> `IGameOrchestrator`  [EXTRACTED]
  Assets/Scripts/Orchestrators/Core/CoreGameOrchestrator.cs → Assets/Scripts/Orchestrators/Game/IGameOrchestrator.cs
- `GameOrchestrator` --implements--> `IGameOrchestrator`  [EXTRACTED]
  Assets/Scripts/Orchestrators/Game/GameOrchestrator.cs → Assets/Scripts/Orchestrators/Game/IGameOrchestrator.cs
- `MainMenuOrchestrator` --references--> `IGameOrchestrator`  [EXTRACTED]
  Assets/Scripts/Orchestrators/MainMenu/MainMenuOrchestrator.cs → Assets/Scripts/Orchestrators/Game/IGameOrchestrator.cs

## Import Cycles
- None detected.

## Communities (170 total, 86 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.17
Nodes (8): SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character, SoulsLike.Ui.Inventory, SoulsLike.Ui.Equipment

### Community 1 - "SceneReference"
Cohesion: 0.15
Nodes (8): bool, string, SceneReference, IComparable, IEquatable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.08
Nodes (13): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+5 more)

### Community 3 - "SoulsLike.Entities.BaseEntity"
Cohesion: 0.18
Nodes (8): IContainerBuilder, EntityRegistrationExt, EntityType, Inject, IViewEntity, ViewEntity, SoulsLike.Entities.BaseEntity.EntityCommands, SoulsLike.Entities.BaseEntity

### Community 4 - "IEntity"
Cohesion: 0.13
Nodes (8): IEntity, Collider, Dictionary, RaycastHit, EntityLocator, Collider, RaycastHit, IEntityLocator

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (19): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+11 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.06
Nodes (24): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerMask, ILayerService (+16 more)

### Community 7 - "SceneType"
Cohesion: 0.21
Nodes (6): Scene, SerializedDictionary, SceneData, Scene, SceneModel, SceneType

### Community 8 - "PlayerHudUiController"
Cohesion: 0.06
Nodes (25): bool, float, DamageResult, HealthModel, bool, float, HealthStats, CanvasGroup (+17 more)

### Community 9 - "EquipmentSlotId"
Cohesion: 0.18
Nodes (7): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode

### Community 10 - "AnimatorComponent"
Cohesion: 0.12
Nodes (16): AnimationEvent, Animator, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, bool, float, IAnimationStateSink (+8 more)

### Community 11 - "Character"
Cohesion: 0.08
Nodes (19): EquipmentComponent, float, Inject, Vector3, Character, AttackComponent, CharacterAnimationAdapter, CharacterAttributeStats (+11 more)

### Community 12 - "AudioService"
Cohesion: 0.21
Nodes (7): IAudioSettingsData, IObserver, AudioService, AudioData, AudioSettingsData, IAudioService, List

### Community 13 - "MovementComponent"
Cohesion: 0.07
Nodes (21): bool, float, Inject, ITimer, LandingType, Quaternion, Transform, Vector2 (+13 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.15
Nodes (8): Dictionary, GameObject, Image, int, List, TMP_Text, Transform, EquipmentUi

### Community 16 - "InventoryUi"
Cohesion: 0.08
Nodes (13): Color, Image, int, IReadOnlyList, List, ScalingGrade, TMP_Text, Transform (+5 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.10
Nodes (16): EquipmentLoadout, HandMode, Inject, InventoryComponent, EquipmentComponent, EquipmentLoadout, IEquipmentLoadoutSink, EquipmentModel (+8 more)

### Community 18 - "CharacterActionStateMachine"
Cohesion: 0.23
Nodes (7): AnimatorStateMachineDto, CharacterAnimationAdapter, bool, CharacterActionStateMachine, CharacterActionStateId, CharacterAnimationSignal, CharacterAnimationSignalKind

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+3 more)

### Community 20 - "IInitializable"
Cohesion: 0.26
Nodes (6): List, Entity, EntityCommand, IEntityComponent, IDisposable, IInitializable

### Community 21 - "InventoryEntry"
Cohesion: 0.22
Nodes (7): InventoryEntry, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "SoulsLike.Services.Audio.Data"
Cohesion: 0.09
Nodes (11): bool, float, AudioSettingsData, IAudioSettingsData, MusicType, SfxType, IAmbienceSystem, IAudioService (+3 more)

### Community 23 - "CameraService"
Cohesion: 0.11
Nodes (9): bool, Camera, float, Transform, Tween, CameraService, CinemachineCamera, CinemachineThirdPersonFollow (+1 more)

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.16
Nodes (5): IEquipmentRoute, IInventoryRoute, IPauseNavigationRoute, PauseNavigationUiController, Stack

### Community 25 - "KeyValue"
Cohesion: 0.20
Nodes (5): IKeyValue, KeyValue, Dictionary, UnityDictionaryFactory, IEnumerable

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 28 - ".Tick"
Cohesion: 0.25
Nodes (5): UnityCharacterClock, float, CharacterCommandBuffer, CharacterInputBatch, ICharacterClock

### Community 29 - "AmbienceService"
Cohesion: 0.07
Nodes (24): AudioClip, float, IAudioService, IAudioSettingsData, MusicType, SceneType, SfxType, AmbienceService (+16 more)

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

### Community 36 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 37 - "TargetLockNode"
Cohesion: 0.21
Nodes (7): Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService, TargetingService

### Community 38 - "ScriptableObject"
Cohesion: 0.22
Nodes (6): RuntimeAnimatorController, AnimationProfile, CombatProfile, float, AudioData, ScriptableObject

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
Nodes (3): string, InventoryEntryId, IEquipmentPresenter

### Community 43 - "BaseUi"
Cohesion: 0.17
Nodes (5): bool, CanvasGroup, Transform, BaseUi, IBaseUi

### Community 44 - "EquipmentPresentation"
Cohesion: 0.25
Nodes (7): EquipmentLoadout, EquippedItemContext, GameObject, Quaternion, Transform, Vector3, EquipmentPresentation

### Community 46 - ".Hide"
Cohesion: 0.19
Nodes (4): bool, Camera, LockOnUiController, IPostLateTickable

### Community 47 - "CharacterRuntime.cs"
Cohesion: 0.29
Nodes (7): AttackIntent, AttackRequest, CharacterCommand, CharacterCommandDisposition, CharacterCommandKind, EquipmentActionKind, EquipmentActionRequest

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 50 - "SoulsLike.Entities.Character.Ports"
Cohesion: 0.22
Nodes (5): LandingType, LocomotionState, MovementMode, SoulsLike.Entities.Character.Ports, SoulsLike.Entities.Character.Components.Movement

### Community 51 - "InventoryUiController"
Cohesion: 0.10
Nodes (11): EquipmentSlotChange, InventoryPrimaryCategory, InventorySubCategory, IInventoryPresenter, Action, bool, IReadOnlyCollection, InventoryUiController (+3 more)

### Community 53 - "ICameraService"
Cohesion: 0.18
Nodes (3): Vector2, ICameraService, Ray

### Community 54 - "WeaponDefinition"
Cohesion: 0.25
Nodes (7): bool, float, GameObject, int, Sprite, string, WeaponDefinition

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.15
Nodes (10): BaseEventData, Color, GameObject, Image, MPImage, PointerEventData, TMP_Text, EquipmentSlotUI (+2 more)

### Community 56 - "PlayerController"
Cohesion: 0.15
Nodes (13): PlayerCharacterInputAdapter, ICameraService, IInputService, PlayerController, GameState, HeavyAttackGestureResolver, ICameraService, IGameStateNotifier (+5 more)

### Community 58 - "IGameOrchestrator"
Cohesion: 0.25
Nodes (3): UniTask, UniTaskVoid, IGameOrchestrator

### Community 59 - "AssetMappingData"
Cohesion: 0.25
Nodes (4): AssetMappingData, Dictionary, List, SerializedDictionary

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 62 - ".Open"
Cohesion: 0.33
Nodes (3): Action, IReadOnlyCollection, IReadOnlyCollection

### Community 65 - "MovementModel"
Cohesion: 0.29
Nodes (5): AnimatorModel, AnimationCurve, LayerMask, MovementModel, Model

### Community 66 - ".Read"
Cohesion: 0.13
Nodes (7): Vector2, CharacterControlFrame, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, float

### Community 68 - "FileService"
Cohesion: 0.36
Nodes (3): string, FileService, SoulsLike.Services.Save

### Community 70 - "InventoryItemSO"
Cohesion: 0.29
Nodes (7): bool, float, int, Sprite, string, InventoryItemSO, ScalingGrade

### Community 74 - "EquipmentSwapCoordinator"
Cohesion: 0.26
Nodes (7): EquipmentSlotGroup, AnimatorStateMachineDto, EquipmentComponent, EquipmentSlotGroup, EquipmentSwapCoordinator, SwapPhase, SwapPhase

### Community 75 - "ItemDefinition"
Cohesion: 0.10
Nodes (19): Inject, IReadOnlyList, InventoryComponent, Collider, int, GroundItem, Dictionary, IReadOnlyList (+11 more)

### Community 76 - "GameOrchestrator"
Cohesion: 0.22
Nodes (5): UniTask, UniTaskVoid, GameOrchestrator, UniTask, ISceneService

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "ItemTypes.cs"
Cohesion: 0.16
Nodes (14): float, ConsumableDefinition, float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot (+6 more)

### Community 80 - "InventoryData"
Cohesion: 0.19
Nodes (9): HealthData, IHealthData, IReadOnlyList, List, InitialInventoryEntry, InventoryData, Data, Model (+1 more)

### Community 81 - "SoulsLike.Entities.Character.Runtime"
Cohesion: 0.19
Nodes (4): SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Input, SoulsLike.Entities.Character.Components, SoulsLike.Entities.Character.Runtime

### Community 82 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.17
Nodes (8): float, int, Vector3, DamageRequest, bool, float, HealthStatUpdate, SoulsLike.Entities.Character.Components.Health

### Community 83 - "IMovementData"
Cohesion: 0.70
Nodes (4): AnimationCurve, LayerMask, IMovementData, MovementData

### Community 84 - "LockOnUi"
Cohesion: 0.38
Nodes (5): Camera, RectTransform, Transform, Vector3, LockOnUi

### Community 86 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.32
Nodes (3): SceneDependency, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 90 - ".ApplyAnimationProfile"
Cohesion: 0.16
Nodes (3): AnimationProfile, EquipmentLoadout, HandMode

### Community 92 - "UiService"
Cohesion: 0.13
Nodes (10): UiController, AssetMappingData, Transform, UiFactory, Inject, List, Transform, IUiService (+2 more)

### Community 93 - "CoreGameOrchestrator"
Cohesion: 0.06
Nodes (13): CharacterFactory, List, CoreGameOrchestrator, ICoreGameOrchestrator, GameState, IGameStateNotifier, IGameStateObserver, IPauseMenuPresenter (+5 more)

### Community 94 - "CharacterRuntime"
Cohesion: 0.23
Nodes (6): bool, CharacterRuntime, MovementGate, MovementGateReason, MovementPolicy, CharacterActionStateMachine

### Community 95 - ".UpdateState"
Cohesion: 0.33
Nodes (3): AnimatorStateMachineDto, AnimatorStateMachineDto, IAnimationStateSink

### Community 97 - "InventoryItemViewData"
Cohesion: 0.25
Nodes (3): CharacterAttributeStats, InventoryItemViewData, IReadOnlyDictionary

### Community 102 - "MainMenuUiController"
Cohesion: 0.06
Nodes (18): IMainMenuOrchestrator, MainMenuOrchestrator, IContainerBuilder, CoreScope, IContainerBuilder, MainMenuScope, ProjectScope, IContainerBuilder (+10 more)

### Community 144 - "AddressableAssetService"
Cohesion: 0.09
Nodes (15): GameObject, AddressableAssetService, IAssetService, LifetimeScope, BaseFactory, CanvasGroup, CanvasGroupExt, AssetMappingData (+7 more)

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 146 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 150 - "ICharacterActionExecutor"
Cohesion: 0.26
Nodes (5): CharacterCommandExecutionResult, CharacterCommandExecutionStatus, ICharacterActionExecutor, JumpRequest, RollRequest

### Community 172 - "SoulsLike.Services"
Cohesion: 0.17
Nodes (7): SoulsLike.Services.Targeting, SoulsLike.Services, SoulsLike.Services.CameraService, SoulsLike, SoulsLike.Ui.PlayerHud, SoulsLike.Ui.LockOn, SoulsLike.Ui.Base

### Community 174 - "MonoBehaviour"
Cohesion: 0.83
Nodes (3): BaseComponent, IComponent, MonoBehaviour

## Knowledge Gaps
- **11 isolated node(s):** `SpeedMultiplierKey`, `AttackType`, `LandingType`, `LocomotionState`, `MovementMode` (+6 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **86 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `PlayerCharacterInputAdapter` (5× useful, score=4.954194053)
- `AmbienceService` (2× useful, score=1.999811285)
- `AmbienceData` (2× useful, score=1.999811285)
- `SceneType` (2× useful, score=1.999811285)
- `CharacterCommandFactory` (2× useful, score=1.987277619)
- `ICharacterCommand` (2× useful, score=1.986594431)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `HealthComponent`, `AnimatorComponent`, `EquipmentUiController`, `InventoryUi`, `EquipmentComponent`, `CharacterActionStateMachine`, `IInitializable`, `ICharacterActionExecutor`, `.TriggerRoll`, `EquipmentPresentation`, `MonoBehaviour`, `CharacterRuntime.cs`, `InventoryUiController`, `PlayerController`, `.PlayAttack`, `.Tick`, `.SetMovementBlocked`, `EquipmentSwapCoordinator`, `SoulsLike.Entities.Character.Runtime`, `.HandleLockOnInput`, `.ApplyAnimationProfile`, `CoreGameOrchestrator`, `CharacterRuntime`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.258) - this node is a cross-community bridge._
- **Why does `EquipmentUiController` connect `EquipmentUiController` to `SoulsLike.Items`, `InputService`, `EquipmentSlotId`, `InventoryEntryId`, `ItemDefinition`, `Character`, `.Hide`, `EquipmentUi`, `EquipmentComponent`, `InventoryUiController`, `IInitializable`, `PauseNavigationUiController`?**
  _High betweenness centrality (0.077) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `.SetMovementBlocked`, `SoulsLike.Entities.Character.Ports`, `IInitializable`, `AttackComponent`?**
  _High betweenness centrality (0.074) - this node is a cross-community bridge._
- **What connects `SpeedMultiplierKey`, `AttackType`, `LandingType` to the rest of the system?**
  _11 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.14705882352941177 - nodes in this community are weakly interconnected._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.08246225319396051 - nodes in this community are weakly interconnected._
- **Should `IEntity` be split into smaller, more focused modules?**
  _Cohesion score 0.13333333333333333 - nodes in this community are weakly interconnected._