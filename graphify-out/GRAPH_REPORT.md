# Graph Report - SoulsLikeTemplate  (2026-08-18)

## Corpus Check
- 194 files · ~34,874 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1785 nodes · 3216 edges · 165 communities (75 shown, 90 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 254 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `d63e2b74`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- ItemDefinition
- Entity
- AnimatorStateMachineReceiver
- PreviewRenderService
- SceneData
- PlayerHudUi
- EquipmentSlotId
- AnimatorComponent
- Character
- GameState
- MovementComponent
- EquipmentUiController
- EquipmentUi
- InventoryUi
- EquipmentComponent
- CharacterRuntime.cs
- InventorySlotUI
- .Refresh
- InventoryEntryId
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
- InputService
- HealthStats
- MainMenuOrchestrator.cs
- StorageRegistry
- IMovementComponent
- CustomButton
- IEquipmentPresenter
- BaseUi
- EquipmentPresentation
- AudioClip
- LockOnUiController
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
- IInitializable
- MainMenuUi
- IContainerBuilder
- CustomButtonMapping
- BaseUi.cs
- float
- .TryStartAttack
- IMovementData
- MainMenuUiController
- IInventoryPresenter
- FileService
- ICustomButton
- InventoryItemSO
- PauseNavigationUi
- .Tick
- ICoreGameOrchestrator
- .StartSwap
- InventoryComponent
- SceneType
- CustomButtonEditor
- WeaponDefinition
- InventoryEntry.cs
- InventoryData
- SoulsLike.Entities.Character.Runtime
- SoulsLike.Entities.Character.Components.Health
- Transform
- PlayerCharacterInputAdapter
- StateMachineName
- SoulsLike.Services.Scenes.Data
- .HandleLockOnInput
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
- AttackType
- .SetAirborneMotion
- HandMode
- SharedSceneScope
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
- Quaternion
- HealthStats
- InventoryComponent
- CharacterActionStateId
- CharacterInputBatch
- CharacterAnimationSignal
- CharacterCommandBuffer
- CharacterCommandExecutionResult
- CharacterControlFrame
- ICharacterCommand
- IEquipmentCommandReceiver
- AddressableAssetService
- SceneReferencePropertyDrawer
- SceneService
- AnimatorControllerParameterType
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
- .BeginRootMotionAction
- Vector3
- SoulsLike.Services

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
- `Character` --references--> `EquipmentPresentation`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Components/Equipment/EquipmentPresentation.cs
- `Character` --references--> `EquipmentSwapCoordinator`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Character.cs → Assets/Scripts/Entities/Character/Adapters/EquipmentSwapCoordinator.cs
- `PlayerController` --references--> `Character`  [EXTRACTED]
  Assets/Scripts/Entities/Character/PlayerController.cs → Assets/Scripts/Entities/Character/Character.cs

## Import Cycles
- None detected.

## Communities (165 total, 90 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.16
Nodes (10): SpeedMultiplierKey, SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Entities.Character.Components, SoulsLike.Entities.Character (+2 more)

### Community 1 - "SceneReference"
Cohesion: 0.15
Nodes (8): bool, string, SceneReference, IComparable, IEquatable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.09
Nodes (13): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+5 more)

### Community 3 - "ItemDefinition"
Cohesion: 0.15
Nodes (10): EquipmentLoadout, EquippedItemContext, float, int, IReadOnlyList, List, Sprite, string (+2 more)

### Community 4 - "Entity"
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
Cohesion: 0.13
Nodes (10): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode, CharacterAttributeStats (+2 more)

### Community 10 - "AnimatorComponent"
Cohesion: 0.10
Nodes (14): AnimationEvent, Animator, AnimatorModel, AnimatorRootMotionRelay, AnimatorStateMachineReceiver, bool, float, IAnimationStateSink (+6 more)

### Community 11 - "Character"
Cohesion: 0.07
Nodes (27): CharacterCommandExecutionStatus, EquipmentComponent, float, Inject, Quaternion, Vector3, Character, AttackComponent (+19 more)

### Community 12 - "GameState"
Cohesion: 0.15
Nodes (3): GameState, IGameStateNotifier, IGameStateObserver

### Community 13 - "MovementComponent"
Cohesion: 0.07
Nodes (21): bool, float, Inject, ITimer, LandingType, Quaternion, Transform, Vector2 (+13 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.13
Nodes (8): Dictionary, GameObject, Image, int, IReadOnlyList, TMP_Text, Transform, EquipmentUi

### Community 16 - "InventoryUi"
Cohesion: 0.15
Nodes (10): Color, Image, int, IReadOnlyList, List, ScalingGrade, TMP_Text, Transform (+2 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.10
Nodes (17): EquipmentLoadout, HandMode, Inject, InventoryComponent, EquipmentComponent, EquipmentLoadout, IEquipmentLoadoutSink, BaseComponent (+9 more)

### Community 18 - "CharacterRuntime.cs"
Cohesion: 0.05
Nodes (37): AnimatorStateMachineDto, CharacterAnimationAdapter, AnimatorStateMachineDto, bool, CharacterActionStateMachine, bool, float, Vector2 (+29 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (11): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+3 more)

### Community 20 - ".Refresh"
Cohesion: 0.18
Nodes (4): InventoryChange, InventoryChangeType, Action, IReadOnlyCollection

### Community 21 - "InventoryEntryId"
Cohesion: 0.14
Nodes (8): EquipmentSlotChange, string, InventoryEntry, InventoryEntryId, Dictionary, IReadOnlyList, List, InventoryModel

### Community 22 - "AudioService"
Cohesion: 0.06
Nodes (18): IAudioSettingsData, IObserver, AudioService, bool, float, AudioSettingsData, IAudioSettingsData, MusicType (+10 more)

### Community 23 - "CameraService"
Cohesion: 0.13
Nodes (8): bool, Camera, float, Tween, CameraService, CinemachineCamera, CinemachineThirdPersonFollow, Ease

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.12
Nodes (6): IEquipmentRoute, IInventoryRoute, IPauseNavigationRoute, IPauseNavigationRouteNavigation, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike"
Cohesion: 0.12
Nodes (10): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+2 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.07
Nodes (19): BaseComponent, IComponent, Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService (+11 more)

### Community 28 - "PauseMenuUiController"
Cohesion: 0.22
Nodes (3): IPauseMenuPresenter, PauseMenuUi, PauseMenuUiController

### Community 29 - "AmbienceService"
Cohesion: 0.07
Nodes (26): AudioClip, float, IAudioService, IAudioSettingsData, MusicType, SceneType, SfxType, AmbienceService (+18 more)

### Community 31 - "AttackComponent"
Cohesion: 0.07
Nodes (21): AnimatorStateMachineDto, AttackType, bool, float, HandMode, ITimer, StateMachineName, AttackComponent (+13 more)

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
Cohesion: 0.20
Nodes (8): GameOrchestrator, CharacterActions, IInputService, InputService, InputAction, ITickable, ProjectInputActions, UIActions

### Community 37 - "HealthStats"
Cohesion: 0.20
Nodes (7): bool, float, DamageResult, HealthModel, bool, float, HealthStats

### Community 38 - "MainMenuOrchestrator.cs"
Cohesion: 0.20
Nodes (4): IContainerBuilder, MainMenuScope, SoulsLike.Ui.MainMenu, SoulsLike.Orchestrators.MainMenu

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
Cohesion: 0.17
Nodes (4): bool, CanvasGroup, Transform, BaseUi

### Community 44 - "EquipmentPresentation"
Cohesion: 0.23
Nodes (8): bool, EquipmentLoadout, EquippedItemContext, Quaternion, Transform, Vector3, EquipmentPresentation, GameObject

### Community 46 - "LockOnUiController"
Cohesion: 0.17
Nodes (10): Camera, RectTransform, Transform, Vector3, LockOnUi, bool, Camera, LockOnUiController (+2 more)

### Community 47 - "InventoryViewStateController"
Cohesion: 0.29
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

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.12
Nodes (12): AxisEventData, BaseEventData, Color, GameObject, Image, MPImage, PointerEventData, TMP_Text (+4 more)

### Community 56 - "PlayerController"
Cohesion: 0.18
Nodes (9): ICameraService, IInputService, PlayerController, Vector2, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable (+1 more)

### Community 58 - "IInitializable"
Cohesion: 0.16
Nodes (5): UniTask, UniTaskVoid, IGameOrchestrator, MainMenuOrchestrator, IInitializable

### Community 59 - "MainMenuUi"
Cohesion: 0.22
Nodes (3): IMainMenuPresenter, MainMenuUi, IStartable

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 62 - "BaseUi.cs"
Cohesion: 0.22
Nodes (4): IBaseUi, CanvasGroup, CanvasGroupExt, SoulsLike.Extensions

### Community 65 - "IMovementData"
Cohesion: 0.21
Nodes (9): AnimatorModel, AnimationCurve, LayerMask, IMovementData, MovementData, AnimationCurve, LayerMask, MovementModel (+1 more)

### Community 68 - "FileService"
Cohesion: 0.36
Nodes (3): string, FileService, SoulsLike.Services.Save

### Community 70 - "InventoryItemSO"
Cohesion: 0.29
Nodes (7): bool, float, int, Sprite, string, InventoryItemSO, ScalingGrade

### Community 74 - ".StartSwap"
Cohesion: 0.18
Nodes (11): EquipmentSlotGroup, EquipmentSlotGroup, AnimatorStateMachineDto, CharacterCommandExecutionStatus, EquipmentComponent, EquipmentLoadout, EquipmentSlotGroup, EquippedItemContext (+3 more)

### Community 75 - "InventoryComponent"
Cohesion: 0.16
Nodes (11): Inject, IReadOnlyList, InventoryComponent, Collider, int, GroundItem, Dictionary, IReadOnlyList (+3 more)

### Community 76 - "SceneType"
Cohesion: 0.24
Nodes (5): UniTask, UniTaskVoid, SceneType, UniTask, ISceneService

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "WeaponDefinition"
Cohesion: 0.06
Nodes (32): float, WeaponRuntime, RuntimeAnimatorController, AnimationProfile, CombatProfile, float, ConsumableDefinition, float (+24 more)

### Community 80 - "InventoryData"
Cohesion: 0.19
Nodes (9): HealthData, IHealthData, IReadOnlyList, List, InitialInventoryEntry, InventoryData, Data, Model (+1 more)

### Community 81 - "SoulsLike.Entities.Character.Runtime"
Cohesion: 0.24
Nodes (4): UnityCharacterClock, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Input, SoulsLike.Entities.Character.Runtime

### Community 82 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.15
Nodes (8): float, int, Vector3, DamageRequest, bool, float, HealthStatUpdate, SoulsLike.Entities.Character.Components.Health

### Community 84 - "PlayerCharacterInputAdapter"
Cohesion: 0.40
Nodes (5): PlayerCharacterInputAdapter, HeavyAttackGestureResolver, ICameraService, IInputService, SprintRollGestureResolver

### Community 86 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.32
Nodes (3): SceneDependency, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 90 - ".ApplyAnimationProfile"
Cohesion: 0.16
Nodes (3): AnimationProfile, EquipmentLoadout, HandMode

### Community 92 - "UiService"
Cohesion: 0.15
Nodes (9): AssetMappingData, Transform, UiFactory, Inject, List, Transform, IUiService, UiService (+1 more)

### Community 93 - "CoreGameOrchestrator"
Cohesion: 0.20
Nodes (5): CharacterFactory, List, CoreGameOrchestrator, BaseFactory, string

### Community 95 - ".UpdateState"
Cohesion: 0.33
Nodes (3): AnimatorStateMachineDto, AnimatorStateMachineDto, IAnimationStateSink

### Community 102 - "SharedSceneScope"
Cohesion: 0.17
Nodes (8): IContainerBuilder, CoreScope, ProjectScope, IContainerBuilder, SharedSceneScope, IContainerBuilder, LifetimeScope, OnGuiFpsCounter

### Community 144 - "AddressableAssetService"
Cohesion: 0.11
Nodes (12): GameObject, AddressableAssetService, IAssetService, LifetimeScope, BaseFactory, AssetMappingData, IContainerBuilder, VContainerExt (+4 more)

### Community 145 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 146 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 172 - "SoulsLike.Services"
Cohesion: 0.13
Nodes (7): UiController, SoulsLike.Services.Targeting, SoulsLike.Services, SoulsLike.Services.CameraService, SoulsLike.Ui.PlayerHud, SoulsLike.Ui.LockOn, SoulsLike.Ui.Base

## Knowledge Gaps
- **11 isolated node(s):** `SwapPhase`, `SpeedMultiplierKey`, `AttackType`, `LandingType`, `LocomotionState` (+6 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **90 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Work-memory lessons

**Preferred sources** — corroborated by past sessions; start here.
- `PlayerCharacterInputAdapter` (5× useful, score=4.952934988)
- `AmbienceData` (2× useful, score=1.99930305)
- `CharacterCommandFactory` (2× useful, score=1.986772569)
- `ICharacterCommand` (2× useful, score=1.986089555)

**Known dead ends** — questions that led nowhere; don't re-derive.
- "I DON'T SEE ANIMATION CLIP = "1Hand_Up_Shield_Block_B_Idle" IN ShieldBlock STATE IN UpperBody LAYER IN ALL ANIMATORS ASSET LOCATED AT PATH = Assets/Art/Animation/CharacterNoWeaponAnimator.controller" -> `Animator`, `AnimatorStateMachine`

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `SoulsLike.Items`, `HealthComponent`, `ItemDefinition`, `AnimatorComponent`, `EquipmentUiController`, `CharacterRuntime.cs`, `.Refresh`, `OnGuiFpsCounter`, `.BeginRootMotionAction`, `EquipmentPresentation`, `InventoryUiController`, `PlayerController`, `IInitializable`, `.TryStartAttack`, `.Tick`, `.StartSwap`, `.HandleLockOnInput`, `.ApplyAnimationProfile`, `CoreGameOrchestrator`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.226) - this node is a cross-community bridge._
- **Why does `MovementComponent` connect `MovementComponent` to `.Tick`, `EquipmentComponent`, `SoulsLike.Entities.Character.Ports`, `IInitializable`?**
  _High betweenness centrality (0.119) - this node is a cross-community bridge._
- **Why does `InventoryUiController` connect `InventoryUiController` to `SoulsLike.Items`, `IInventoryPresenter`, `InputService`, `ItemDefinition`, `Entity`, `EquipmentSlotId`, `InventoryComponent`, `Character`, `BaseUi`, `LockOnUiController`, `InventoryUi`, `EquipmentComponent`, `.Refresh`, `InventoryEntryId`, `PauseNavigationUiController`, `IInitializable`?**
  _High betweenness centrality (0.077) - this node is a cross-community bridge._
- **What connects `SwapPhase`, `SpeedMultiplierKey`, `AttackType` to the rest of the system?**
  _11 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.14705882352941177 - nodes in this community are weakly interconnected._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.09041835357624832 - nodes in this community are weakly interconnected._
- **Should `Entity` be split into smaller, more focused modules?**
  _Cohesion score 0.06845513413506013 - nodes in this community are weakly interconnected._