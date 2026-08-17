# Graph Report - SoulsLikeTemplate  (2026-08-17)

## Corpus Check
- 180 files · ~32,699 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1772 nodes · 3320 edges · 144 communities (81 shown, 63 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 267 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `4b54bf44`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- WeaponDefinition
- IDisposable
- AnimatorStateMachineReceiver
- PreviewRenderService
- ItemDefinition
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
- CharacterRuntime
- InventorySlotUI
- InventoryItemViewData
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
- Inject
- CharacterCommandBuffer
- ScriptableObject
- StorageRegistry
- IMovementComponent
- CustomButton
- IEquipmentPresenter
- BaseUi
- MainMenuUiController
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
- ICharacterCommand
- EquipmentSlotHud
- UiService
- SharedSceneScope
- CustomButtonMapping
- PlayerHudUiController
- AmbienceManagerWrapper
- ItemTypes.cs
- .BeginRootMotionAction
- .Read
- InventoryViewStateController
- FileService
- ICustomButton
- InventoryItemSO
- PauseNavigationUi
- .PlayAttack
- LockOnUi
- .BuildLoadout
- InventoryComponent
- GameOrchestrator
- CustomButtonEditor
- EquipmentPresentation
- SoulsLike.Entities.Character.Components.Movement
- IPauseNavigationRouteNavigation
- CharacterFactory.cs
- DamageRequest
- InputService
- IPauseMenuPresenter
- .OnAnimationStateChanged
- SoulsLike.Services
- .HandleLockOnInput
- SoulsLike.Extensions
- MovementGate
- .ApplyAnimationProfile
- .ApplyRootMotion
- InventoryEntryId
- MovementModel
- IAnimationStateSink
- CharacterFactory
- ItemType
- .Select
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

## God Nodes (most connected - your core abstractions)
1. `Character` - 63 edges
2. `AnimatorComponent` - 57 edges
3. `MovementComponent` - 51 edges
4. `EquipmentUi` - 37 edges
5. `InventoryUiController` - 37 edges
6. `InventoryEntryId` - 34 edges
7. `EquipmentUiController` - 33 edges
8. `EquipmentSlotUI` - 33 edges
9. `EquipmentComponent` - 32 edges
10. `InventoryUi` - 31 edges

## Surprising Connections (you probably didn't know these)
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `EquipmentSwapCoordinator` --references--> `AnimatorComponent`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Adapters/EquipmentSwapCoordinator.cs → Assets/Scripts/Components/Animator/AnimatorComponent.cs
- `Character` --references--> `AnimatorComponent`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Components/Animator/AnimatorComponent.cs
- `Character` --references--> `CharacterActionStateId`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs
- `Character` --references--> `CharacterRuntime`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs

## Import Cycles
- None detected.

## Communities (144 total, 63 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.06
Nodes (28): HealthData, IHealthData, IReadOnlyList, List, InitialInventoryEntry, InventoryData, InventoryInstanceState, AnimationCurve (+20 more)

### Community 1 - "SceneReference"
Cohesion: 0.06
Nodes (28): UniTask, Scene, SerializedDictionary, SceneData, Scene, SceneModel, bool, GUIStyle (+20 more)

### Community 2 - "HealthComponent"
Cohesion: 0.09
Nodes (14): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+6 more)

### Community 3 - "WeaponDefinition"
Cohesion: 0.19
Nodes (9): float, WeaponRuntime, bool, float, GameObject, int, Sprite, string (+1 more)

### Community 4 - "IDisposable"
Cohesion: 0.07
Nodes (21): List, Entity, IEntity, EntityCommand, Collider, Dictionary, RaycastHit, EntityLocator (+13 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (19): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+11 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.06
Nodes (24): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerMask, ILayerService (+16 more)

### Community 7 - "ItemDefinition"
Cohesion: 0.20
Nodes (8): float, int, IReadOnlyList, List, Sprite, string, ItemDefinition, EquipmentGroup

### Community 8 - "PlayerHudUi"
Cohesion: 0.20
Nodes (8): bool, Color, float, MPImage, RectTransform, PlayerHudUi, StatBar, StatBar

### Community 9 - "EquipmentSlotId"
Cohesion: 0.18
Nodes (7): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode

### Community 10 - "AnimatorComponent"
Cohesion: 0.11
Nodes (18): AnimationEvent, Animator, AnimatorControllerParameterType, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, bool, float (+10 more)

### Community 11 - "Character"
Cohesion: 0.09
Nodes (18): float, int, Character, AttackComponent, CharacterAnimationAdapter, CharacterAttributeStats, EquipmentComponent, EquipmentPresentation (+10 more)

### Community 12 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.16
Nodes (8): bool, float, DamageResult, HealthModel, bool, float, HealthStats, SoulsLike.Entities.Character.Components.Health

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
Cohesion: 0.14
Nodes (11): Inject, InventoryComponent, EquipmentComponent, IEquipmentLoadoutSink, EquipmentModel, EquipmentSlotId, InventoryChange, InventoryEntry (+3 more)

### Community 18 - "CharacterRuntime"
Cohesion: 0.14
Nodes (12): bool, AttackState, CharacterActionState, CharacterActionStateMachine, EquipmentSwapState, NeutralState, RollState, bool (+4 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (12): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+4 more)

### Community 20 - "InventoryItemViewData"
Cohesion: 0.21
Nodes (4): CharacterAttributeStats, IReadOnlyList, InventoryItemViewData, IReadOnlyDictionary

### Community 21 - "InventoryEntry"
Cohesion: 0.22
Nodes (7): InventoryEntry, Dictionary, IReadOnlyList, List, InventoryChange, InventoryChangeType, InventoryModel

### Community 22 - "AudioService"
Cohesion: 0.16
Nodes (8): List, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, IAudioService, IObserver

### Community 23 - "CameraService"
Cohesion: 0.12
Nodes (8): bool, Camera, float, Tween, CameraService, CinemachineCamera, CinemachineThirdPersonFollow, Ease

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.13
Nodes (6): IEquipmentRoute, IInventoryRoute, IPauseNavigationRoute, IReadOnlyCollection, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.07
Nodes (19): BaseComponent, IComponent, Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService (+11 more)

### Community 28 - "CoreGameOrchestrator"
Cohesion: 0.14
Nodes (5): List, CoreGameOrchestrator, GameState, IGameStateNotifier, IGameStateObserver

### Community 29 - "AmbienceService"
Cohesion: 0.17
Nodes (5): float, GameObject, Tween, AmbienceService, AudioSource

### Community 31 - "AttackComponent"
Cohesion: 0.08
Nodes (19): AnimatorStateMachineDto, AttackType, bool, float, HandMode, ITimer, StateMachineName, AttackComponent (+11 more)

### Community 32 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "AnimatorRootMotionRelay"
Cohesion: 0.17
Nodes (7): Animator, bool, string, AnimatorRootMotionRelay, Quaternion, Vector3, IRootMotionSink

### Community 37 - "CharacterCommandBuffer"
Cohesion: 0.17
Nodes (6): UnityCharacterClock, float, CharacterCommandBuffer, CharacterControlFrame, CharacterInputBatch, ICharacterClock

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

### Community 43 - "BaseUi"
Cohesion: 0.21
Nodes (4): bool, CanvasGroup, Transform, BaseUi

### Community 44 - "MainMenuUiController"
Cohesion: 0.09
Nodes (9): IMainMenuOrchestrator, IContainerBuilder, MainMenuScope, IMainMenuPresenter, MainMenuUi, MainMenuUiController, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu (+1 more)

### Community 45 - "AmbienceData"
Cohesion: 0.11
Nodes (12): float, AmbienceData, MusicEntry, SceneMusicEntry, SfxEntry, MusicType, SfxType, IAmbienceSystem (+4 more)

### Community 46 - ".Hide"
Cohesion: 0.18
Nodes (5): bool, Camera, LockOnUiController, IPostLateTickable, UiController

### Community 47 - "CharacterRuntime.cs"
Cohesion: 0.13
Nodes (16): Vector2, AttackCommand, AttackIntent, AttackRequest, CharacterCommandBufferPolicy, CharacterCommandExecutionStatus, CharacterCommandKind, EquipmentActionRequest (+8 more)

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
Cohesion: 0.21
Nodes (3): UiController, ICoreGameOrchestrator, PauseMenuUiController

### Community 53 - "ICameraService"
Cohesion: 0.17
Nodes (3): Transform, ICameraService, Ray

### Community 54 - "IInitializable"
Cohesion: 0.16
Nodes (5): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator, IInitializable

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.16
Nodes (9): BaseEventData, Color, GameObject, Image, MPImage, PointerEventData, TMP_Text, EquipmentSlotUI (+1 more)

### Community 56 - "PlayerController"
Cohesion: 0.18
Nodes (9): ICameraService, IInputService, PlayerController, Vector2, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable (+1 more)

### Community 57 - "ICharacterCommand"
Cohesion: 0.20
Nodes (5): Vector2, CharacterCommandFactory, CharacterCommandDisposition, CharacterCommandExecutionResult, ICharacterCommand

### Community 58 - "EquipmentSlotHud"
Cohesion: 0.20
Nodes (8): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud

### Community 59 - "UiService"
Cohesion: 0.07
Nodes (20): GameObject, AddressableAssetService, IAssetService, LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory (+12 more)

### Community 60 - "SharedSceneScope"
Cohesion: 0.20
Nodes (7): IContainerBuilder, CoreScope, IContainerBuilder, ProjectScope, IContainerBuilder, SharedSceneScope, LifetimeScope

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 63 - "AmbienceManagerWrapper"
Cohesion: 0.27
Nodes (3): float, AmbienceManagerWrapper, Component

### Community 64 - "ItemTypes.cs"
Cohesion: 0.22
Nodes (10): float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot, ScalingGrade, float (+2 more)

### Community 66 - ".Read"
Cohesion: 0.11
Nodes (13): CharacterActionStateId, CharacterInputBatch, PlayerCharacterInputAdapter, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, CharacterCommandFactory (+5 more)

### Community 67 - "InventoryViewStateController"
Cohesion: 0.20
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 68 - "FileService"
Cohesion: 0.36
Nodes (3): string, FileService, SoulsLike.Services.Save

### Community 70 - "InventoryItemSO"
Cohesion: 0.18
Nodes (8): InventoryPrimaryCategory, bool, float, int, Sprite, string, InventoryItemSO, ScalingGrade

### Community 73 - "LockOnUi"
Cohesion: 0.32
Nodes (5): Camera, RectTransform, Transform, Vector3, LockOnUi

### Community 74 - ".BuildLoadout"
Cohesion: 0.13
Nodes (8): EquipmentLoadout, HandMode, EquipmentSwapCoordinator, SwapPhase, EquipmentLoadout, EquipmentSlotGroup, EquippedItemContext, SwapPhase

### Community 75 - "InventoryComponent"
Cohesion: 0.15
Nodes (11): Inject, IReadOnlyList, InventoryComponent, Collider, int, GroundItem, Dictionary, IReadOnlyList (+3 more)

### Community 76 - "GameOrchestrator"
Cohesion: 0.33
Nodes (3): UniTaskVoid, GameOrchestrator, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "EquipmentPresentation"
Cohesion: 0.25
Nodes (7): EquipmentLoadout, EquippedItemContext, GameObject, Quaternion, Transform, Vector3, EquipmentPresentation

### Community 79 - "SoulsLike.Entities.Character.Components.Movement"
Cohesion: 0.22
Nodes (5): LandingType, LocomotionState, MovementMode, SoulsLike.Entities.Character.Ports, SoulsLike.Entities.Character.Components.Movement

### Community 81 - "CharacterFactory.cs"
Cohesion: 0.22
Nodes (5): AttackType, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Components, SoulsLike.Entities.Character.Components.Attack, SoulsLike.Entities.Character.Runtime

### Community 82 - "DamageRequest"
Cohesion: 0.19
Nodes (7): float, int, Vector3, DamageRequest, bool, float, HealthStatUpdate

### Community 83 - "InputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

### Community 85 - ".OnAnimationStateChanged"
Cohesion: 0.14
Nodes (5): AnimatorStateMachineDto, StateMachineName, CharacterAnimationAdapter, AnimatorStateMachineDto, AnimatorStateMachineDto

### Community 86 - "SoulsLike.Services"
Cohesion: 0.20
Nodes (7): SceneDependency, SoulsLike.Services, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes, SoulsLike.Services.Repository, SoulsLike.Services.Audio.Data, SoulsLike.Services.Audio

### Community 88 - "SoulsLike.Extensions"
Cohesion: 0.22
Nodes (4): IBaseUi, CanvasGroup, CanvasGroupExt, SoulsLike.Extensions

### Community 89 - "MovementGate"
Cohesion: 0.43
Nodes (3): MovementGate, MovementGateReason, MovementPolicy

### Community 90 - ".ApplyAnimationProfile"
Cohesion: 0.16
Nodes (3): AnimationProfile, EquipmentLoadout, HandMode

### Community 93 - "MovementModel"
Cohesion: 0.29
Nodes (5): AnimatorModel, AnimationCurve, LayerMask, MovementModel, Model

### Community 95 - "IAnimationStateSink"
Cohesion: 0.33
Nodes (3): AnimatorStateMachineDto, AnimatorStateMachineDto, IAnimationStateSink

### Community 96 - "CharacterFactory"
Cohesion: 0.40
Nodes (4): string, CharacterFactory, BaseFactory, GameObject

### Community 97 - "ItemType"
Cohesion: 0.25
Nodes (6): float, ConsumableDefinition, ItemType, ItemUseType, Action, IReadOnlyCollection

## Knowledge Gaps
- **10 isolated node(s):** `SpeedMultiplierKey`, `Model`, `AttackType`, `LandingType`, `LocomotionState` (+5 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **63 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `HealthComponent`, `AnimatorComponent`, `EquipmentUiController`, `CharacterRuntime`, `OnGuiFpsCounter`, `CharacterRuntime.cs`, `InventoryUiController`, `IInitializable`, `PlayerController`, `ICharacterCommand`, `.BeginRootMotionAction`, `.BuildLoadout`, `EquipmentPresentation`, `CharacterFactory.cs`, `.OnAnimationStateChanged`, `.HandleLockOnInput`, `.ApplyAnimationProfile`, `.ApplyRootMotion`, `.Tick`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.271) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `IInitializable`, `HealthComponent`, `.Tick`, `SoulsLike.Entities.Character.Components.Movement`?**
  _High betweenness centrality (0.075) - this node is a cross-community bridge._
- **Why does `EquipmentUiController` connect `EquipmentUiController` to `SoulsLike.Items`, `IDisposable`, `EquipmentSlotId`, `IEquipmentPresenter`, `InventoryComponent`, `Character`, `GameOrchestrator`, `.Hide`, `EquipmentUi`, `EquipmentComponent`, `InputService`, `InventoryEntry`, `IInitializable`, `PauseNavigationUiController`?**
  _High betweenness centrality (0.074) - this node is a cross-community bridge._
- **What connects `SpeedMultiplierKey`, `Model`, `AttackType` to the rest of the system?**
  _10 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SoulsLike.Items` be split into smaller, more focused modules?**
  _Cohesion score 0.06349206349206349 - nodes in this community are weakly interconnected._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.05747126436781609 - nodes in this community are weakly interconnected._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.08717948717948718 - nodes in this community are weakly interconnected._