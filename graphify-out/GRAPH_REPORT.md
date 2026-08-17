# Graph Report - SoulsLikeTemplate  (2026-08-17)

## Corpus Check
- cluster-only mode — file stats not available

## Summary
- 1538 nodes · 3084 edges · 82 communities (69 shown, 13 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 237 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `dbd49512`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthStats
- WeaponDefinition
- IDisposable
- AnimatorStateMachineReceiver
- PreviewRenderService
- ItemDefinition
- PlayerHudUiController
- EquipmentSlotId
- AnimatorComponent
- Character
- MainMenuUiController
- MovementComponent
- EquipmentUiController
- EquipmentUi
- InventoryUi
- EquipmentComponent
- EquipmentSlotUI
- InventorySlotUI
- InventoryEntryId
- PauseMenuUiController
- AudioService
- CameraService
- PauseNavigationUiController
- SoulsLike
- OnGuiFpsCounter
- AttackComponent
- PlayerController
- AmbienceService
- HandMode
- ITimer
- CoroutineService
- CustomButtonToggle
- AnimatorRootMotionRelay
- IComponentMediator
- CharacterActionBuffer
- UiFactory
- SoulsLike.Services
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntry
- BaseUi
- IInitializable
- AmbienceData
- .Hide
- .TryStartRoll
- System.Ui.Base
- .CreateButton
- IInventoryPresenter
- InventoryUiController
- UiService
- ICameraService
- MusicType
- SoulsLike.Extensions
- ITargetingService
- .Refresh
- TargetLockNode
- AddressableAssetService
- SharedSceneScope
- CustomButtonMapping
- CoreGameOrchestrator
- AmbienceManagerWrapper
- IInputService
- .BeginRootMotionAction
- InventoryViewStateController
- FileService
- ICustomButton
- InventoryItemSO
- PauseNavigationUi
- AttackType
- LockOnUi
- LandingType
- ScriptableObject
- Transform
- CustomButtonEditor
- EquipmentLoadout
- .Select
- IPauseNavigationRouteNavigation
- MovementMode.cs

## God Nodes (most connected - your core abstractions)
1. `Character` - 63 edges
2. `AnimatorComponent` - 54 edges
3. `MovementComponent` - 52 edges
4. `InventoryEntryId` - 39 edges
5. `EquipmentUi` - 37 edges
6. `InventoryUiController` - 37 edges
7. `EquipmentSlotId` - 33 edges
8. `EquipmentSlotUI` - 33 edges
9. `EquipmentUiController` - 33 edges
10. `EquipmentComponent` - 32 edges

## Surprising Connections (you probably didn't know these)
- `MovementComponent` --references--> `LocomotionState`  [EXTRACTED]
  Assets/Scripts/Components/Movement/MovementComponent.cs → Assets/Scripts/Components/Movement/LocomotionState.cs
- `MovementComponent` --references--> `MovementMode`  [EXTRACTED]
  Assets/Scripts/Components/Movement/MovementComponent.cs → Assets/Scripts/Components/Movement/MovementMode.cs
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `AnimatorStateMachineDto` --references--> `StateMachineState`  [EXTRACTED]
  Assets/Scripts/Components/Animations/AnimatorStateMachineDto.cs → Assets/Scripts/Components/Animations/StateMachineState.cs
- `AnimatorComponent` --references--> `AnimatorStateMachineReceiver`  [EXTRACTED]
  Assets/Scripts/Components/Animator/AnimatorComponent.cs → Assets/Scripts/Components/Animations/AnimatorStateMachineReceiver.cs

## Import Cycles
- None detected.

## Communities (82 total, 13 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.06
Nodes (34): AnimatorModel, HealthData, IHealthData, AnimationCurve, LayerMask, IMovementData, MovementData, AnimationCurve (+26 more)

### Community 1 - "SceneReference"
Cohesion: 0.05
Nodes (31): UniTask, UniTaskVoid, GameOrchestrator, Scene, SerializedDictionary, SceneData, Scene, SceneModel (+23 more)

### Community 2 - "HealthStats"
Cohesion: 0.06
Nodes (18): float, int, Vector3, DamageRequest, bool, float, DamageResult, IComponentMediator (+10 more)

### Community 3 - "WeaponDefinition"
Cohesion: 0.06
Nodes (34): BaseComponent, IComponent, GameObject, Quaternion, Transform, Vector3, EquipmentPresentation, float (+26 more)

### Community 4 - "IDisposable"
Cohesion: 0.07
Nodes (21): List, Entity, IEntity, EntityCommand, Collider, Dictionary, RaycastHit, EntityLocator (+13 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (18): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+10 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.06
Nodes (24): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerMask, ILayerService (+16 more)

### Community 7 - "ItemDefinition"
Cohesion: 0.08
Nodes (23): Inject, Inject, IReadOnlyList, InventoryComponent, IReadOnlyList, List, InitialInventoryEntry, InventoryData (+15 more)

### Community 8 - "PlayerHudUiController"
Cohesion: 0.08
Nodes (18): CanvasGroup, Color, Image, MPImage, RectTransform, Sprite, TMP_Text, EquipmentSlotHud (+10 more)

### Community 9 - "EquipmentSlotId"
Cohesion: 0.13
Nodes (10): IReadOnlyList, Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, CharacterAttributeStats (+2 more)

### Community 10 - "AnimatorComponent"
Cohesion: 0.12
Nodes (13): AnimationEvent, AnimatorControllerParameterType, Animator, bool, float, IComponentMediator, int, RuntimeAnimatorController (+5 more)

### Community 11 - "Character"
Cohesion: 0.09
Nodes (8): bool, float, LayerMask, Quaternion, Vector3, Character, EquipmentSwapPhase, EquipmentSwapPhase

### Community 12 - "MainMenuUiController"
Cohesion: 0.11
Nodes (7): IMainMenuOrchestrator, IMainMenuPresenter, MainMenuUi, MainMenuUiController, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu, IStartable

### Community 13 - "MovementComponent"
Cohesion: 0.16
Nodes (9): bool, Dictionary, float, IComponentMediator, Transform, Vector2, Vector3, MovementComponent (+1 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.13
Nodes (8): Dictionary, GameObject, Image, int, IReadOnlyList, TMP_Text, Transform, EquipmentUi

### Community 16 - "InventoryUi"
Cohesion: 0.13
Nodes (10): Color, Image, int, IReadOnlyList, List, ScalingGrade, TMP_Text, Transform (+2 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.18
Nodes (3): IComponentMediator, EquipmentComponent, CharacterActions

### Community 18 - "EquipmentSlotUI"
Cohesion: 0.13
Nodes (11): BaseEventData, Color, GameObject, Image, MPImage, PointerEventData, TMP_Text, EquipmentSlotUI (+3 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+3 more)

### Community 20 - "InventoryEntryId"
Cohesion: 0.11
Nodes (4): EquipmentSlotChange, string, InventoryEntryId, IEquipmentPresenter

### Community 21 - "PauseMenuUiController"
Cohesion: 0.13
Nodes (4): ICoreGameOrchestrator, IPauseMenuPresenter, PauseMenuUi, PauseMenuUiController

### Community 22 - "AudioService"
Cohesion: 0.16
Nodes (8): List, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, IAudioService, IObserver

### Community 23 - "CameraService"
Cohesion: 0.13
Nodes (8): bool, Camera, float, Tween, CameraService, CinemachineCamera, CinemachineThirdPersonFollow, Ease

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.16
Nodes (5): IEquipmentRoute, IInventoryRoute, IPauseNavigationRoute, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.13
Nodes (9): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, SoulsLike.Ui.FpsCounter, Key (+1 more)

### Community 27 - "AttackComponent"
Cohesion: 0.19
Nodes (7): bool, CharacterActions, float, IComponentMediator, AttackComponent, BaseComponent, BufferedCharacterAction

### Community 28 - "PlayerController"
Cohesion: 0.15
Nodes (5): PlayerController, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable

### Community 29 - "AmbienceService"
Cohesion: 0.18
Nodes (5): float, GameObject, Tween, AmbienceService, AudioSource

### Community 30 - "HandMode"
Cohesion: 0.17
Nodes (3): HandMode, RuntimeAnimatorController, AnimationProfile

### Community 31 - "ITimer"
Cohesion: 0.13
Nodes (6): ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 32 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "AnimatorRootMotionRelay"
Cohesion: 0.15
Nodes (7): Animator, bool, IComponentMediator, string, AnimatorRootMotionRelay, Quaternion, Vector3

### Community 35 - "IComponentMediator"
Cohesion: 0.14
Nodes (4): SpeedMultiplierKey, Vector2, IComponentMediator, SpeedMultiplierKey

### Community 36 - "CharacterActionBuffer"
Cohesion: 0.19
Nodes (7): Inject, bool, float, Vector2, BufferedCharacterAction, CharacterActionBuffer, CharacterActionType

### Community 37 - "UiFactory"
Cohesion: 0.13
Nodes (11): GameObject, string, CharacterFactory, LifetimeScope, BaseFactory, AssetMappingData, Transform, UiFactory (+3 more)

### Community 38 - "SoulsLike.Services"
Cohesion: 0.23
Nodes (6): SceneDependency, SoulsLike.Services, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes, SoulsLike.Services.Audio.Data, SoulsLike.Services.Audio

### Community 39 - "StorageRegistry"
Cohesion: 0.20
Nodes (6): Enum, IStorageRegistry, Enum, string, StorageRegistry, SoulsLike.Services.Storage

### Community 40 - "IMovementComponent"
Cohesion: 0.15
Nodes (6): Quaternion, SpeedMultiplierKey, Transform, Vector2, Vector3, IMovementComponent

### Community 41 - "CustomButton"
Cohesion: 0.13
Nodes (8): bool, ColorBlock, Image, SelectionState, Sprite, TMP_Text, CustomButton, Button

### Community 42 - "InventoryEntry"
Cohesion: 0.22
Nodes (6): InventoryEntry, InventoryInstanceState, Dictionary, IReadOnlyList, List, InventoryModel

### Community 43 - "BaseUi"
Cohesion: 0.17
Nodes (5): bool, CanvasGroup, Transform, BaseUi, IBaseUi

### Community 44 - "IInitializable"
Cohesion: 0.16
Nodes (5): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator, IInitializable

### Community 45 - "AmbienceData"
Cohesion: 0.19
Nodes (9): float, AmbienceData, MusicEntry, SfxEntry, SfxType, AudioClip, MusicEntry, SceneMusicEntry (+1 more)

### Community 46 - ".Hide"
Cohesion: 0.18
Nodes (5): bool, Camera, LockOnUiController, IPostLateTickable, UiController

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 50 - "IInventoryPresenter"
Cohesion: 0.17
Nodes (3): InventoryPrimaryCategory, InventorySubCategory, IInventoryPresenter

### Community 51 - "InventoryUiController"
Cohesion: 0.21
Nodes (5): Action, bool, IReadOnlyCollection, InventoryUiController, HashSet

### Community 52 - "UiService"
Cohesion: 0.21
Nodes (6): UiController, List, Transform, IUiService, UiService, Canvas

### Community 53 - "ICameraService"
Cohesion: 0.17
Nodes (3): Vector2, ICameraService, Ray

### Community 54 - "MusicType"
Cohesion: 0.20
Nodes (3): SceneMusicEntry, MusicType, IAmbienceSystem

### Community 55 - "SoulsLike.Extensions"
Cohesion: 0.20
Nodes (7): CanvasGroup, CanvasGroupExt, AssetMappingData, IContainerBuilder, VContainerExt, SoulsLike.Extensions, RegistrationBuilder

### Community 56 - "ITargetingService"
Cohesion: 0.25
Nodes (3): Transform, Transform, ITargetingService

### Community 57 - ".Refresh"
Cohesion: 0.20
Nodes (3): InventoryChange, InventoryChangeType, EquipmentGroup

### Community 58 - "TargetLockNode"
Cohesion: 0.25
Nodes (5): Transform, TargetLockAnchorType, TargetLockNode, float, TargetingService

### Community 59 - "AddressableAssetService"
Cohesion: 0.22
Nodes (4): GameObject, AddressableAssetService, IAssetService, SoulsLike.Services.Repository

### Community 60 - "SharedSceneScope"
Cohesion: 0.18
Nodes (7): IContainerBuilder, MainMenuScope, IContainerBuilder, ProjectScope, IContainerBuilder, SharedSceneScope, LifetimeScope

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 63 - "AmbienceManagerWrapper"
Cohesion: 0.27
Nodes (3): float, AmbienceManagerWrapper, Component

### Community 64 - "IInputService"
Cohesion: 0.31
Nodes (6): CharacterActions, IInputService, InputService, InputAction, ProjectInputActions, UIActions

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
Cohesion: 0.38
Nodes (5): Camera, RectTransform, Transform, Vector3, LockOnUi

### Community 75 - "ScriptableObject"
Cohesion: 0.33
Nodes (4): CombatProfile, float, AudioData, ScriptableObject

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

## Knowledge Gaps
- **5 isolated node(s):** `SoulsLike.Entities.BaseEntity.EntityCommands`, `EquipmentSwapPhase`, `Model`, `SoulsLike.Services.Coroutines`, `SoulsLike.Services.Save`
  These have ≤1 connection - possible missing edges or undocumented components.
- **13 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `SoulsLike.Items`, `HealthStats`, `WeaponDefinition`, `ItemDefinition`, `EquipmentSlotId`, `AnimatorComponent`, `MovementComponent`, `EquipmentUiController`, `EquipmentComponent`, `AttackComponent`, `PlayerController`, `HandMode`, `ITimer`, `IComponentMediator`, `CharacterActionBuffer`, `IInitializable`, `InventoryUiController`, `ICameraService`, `ITargetingService`, `.Refresh`, `.BeginRootMotionAction`, `AttackType`, `LandingType`, `EquipmentLoadout`?**
  _High betweenness centrality (0.288) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `SoulsLike.Items`, `.Move`, `IComponentMediator`, `IMovementComponent`, `LandingType`, `Character`, `IInitializable`, `.TryStartRoll`, `MovementMode.cs`, `AttackComponent`, `ITimer`?**
  _High betweenness centrality (0.084) - this node is a cross-community bridge._
- **Why does `AnimatorComponent` connect `AnimatorComponent` to `SoulsLike.Items`, `.BeginRootMotionAction`, `AnimatorRootMotionRelay`, `WeaponDefinition`, `AnimatorStateMachineReceiver`, `AttackType`, `LandingType`, `Character`, `EquipmentComponent`, `ITargetingService`, `HandMode`?**
  _High betweenness centrality (0.074) - this node is a cross-community bridge._
- **What connects `SoulsLike.Entities.BaseEntity.EntityCommands`, `EquipmentSwapPhase`, `Model` to the rest of the system?**
  _5 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SoulsLike.Items` be split into smaller, more focused modules?**
  _Cohesion score 0.05555555555555555 - nodes in this community are weakly interconnected._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.05109126984126984 - nodes in this community are weakly interconnected._
- **Should `HealthStats` be split into smaller, more focused modules?**
  _Cohesion score 0.058445353594389245 - nodes in this community are weakly interconnected._