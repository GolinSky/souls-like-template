# Graph Report - SoulsLikeTemplate  (2026-08-17)

## Corpus Check
- 179 files · ~32,497 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1743 nodes · 3319 edges · 129 communities (84 shown, 45 thin omitted)
- Extraction: 92% EXTRACTED · 8% INFERRED · 0% AMBIGUOUS · INFERRED: 264 edges (avg confidence: 0.8)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `55181af8`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- SoulsLike.Items
- SceneReference
- HealthComponent
- WeaponDefinition
- IInitializable
- AnimatorStateMachineReceiver
- PreviewRenderService
- ItemDefinition
- PlayerHudUiController
- EquipmentSlotId
- AnimatorComponent
- Character
- ITimer
- MovementComponent
- EquipmentUiController
- EquipmentUi
- InventoryUi
- EquipmentComponent
- CharacterActionState
- InventorySlotUI
- InventoryItemViewData
- SoulsLike.Entities.Character
- AudioService
- CameraService
- PauseNavigationUiController
- SoulsLike.Services
- OnGuiFpsCounter
- CharacterActions
- CoreGameOrchestrator
- AmbienceService
- .HasParameter
- AttackComponent
- CoroutineService
- CustomButtonToggle
- AnimatorRootMotionRelay
- SpeedMultiplierKey
- Inject
- UiFactory
- SoulsLike.Services.Audio.Data
- StorageRegistry
- IMovementComponent
- CustomButton
- InventoryEntryId
- BaseUi
- MainMenuUiController
- AmbienceData
- .Hide
- CharacterRuntime.cs
- System.Ui.Base
- .CreateButton
- IInventoryPresenter
- InventoryUiController
- UiService
- ICameraService
- IAmbienceSystem
- EquipmentSlotUI
- PlayerController
- .Submit
- TargetLockNode
- AddressableAssetService
- SharedSceneScope
- CustomButtonMapping
- .Move
- AmbienceManagerWrapper
- ItemTypes.cs
- .TriggerRoll
- .Read
- InventoryViewStateController
- FileService
- ICustomButton
- InventoryItemSO
- PauseNavigationUi
- .PlayAttack
- LockOnUi
- .BuildLoadout
- InventoryEntry
- GameOrchestrator
- CustomButtonEditor
- EquipmentPresentation
- CharacterCommandExecutionStatus
- IPauseNavigationRouteNavigation
- Character.cs
- SoulsLike.Entities.Character.Components.Health
- SceneType
- .TryStartRoll
- EquipmentSwapCoordinator
- SoulsLike.Services.Scenes.Data
- MonoBehaviour
- SceneReferencePropertyDrawer
- SceneService
- .ApplyAnimationProfile
- .ApplyAnimationMovement
- .SetGrounded
- MovementModel
- CharacterRuntime
- IAnimationStateSink
- .Open
- .Select
- WeaponRuntime
- .SetAirborneMotion
- AudioSettingsData
- ProjectScope
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
- OnGuiFpsCounter.cs
- ICameraService
- IInputService
- float

## God Nodes (most connected - your core abstractions)
1. `Character` - 62 edges
2. `AnimatorComponent` - 56 edges
3. `MovementComponent` - 52 edges
4. `EquipmentUi` - 37 edges
5. `InventoryUiController` - 37 edges
6. `InventoryEntryId` - 34 edges
7. `EquipmentUiController` - 33 edges
8. `EquipmentComponent` - 33 edges
9. `EquipmentSlotUI` - 33 edges
10. `InventoryUi` - 31 edges

## Surprising Connections (you probably didn't know these)
- `SceneDependency` --references--> `SceneReference`  [EXTRACTED]
  Assets/Scripts/Services/Scenes/Data/SceneDependency.cs → Assets/Scripts/Services/Scenes/Data/SceneReference.cs
- `PlayerController` --references--> `PlayerCharacterInputAdapter`  [EXTRACTED]
  Assets/Scripts/Entities/Character/PlayerController.cs → Assets/Scripts/Entities/Character/Input/PlayerCharacterInputAdapter.cs
- `CharacterRuntime` --references--> `CharacterActionStateMachine`  [EXTRACTED]
  Assets/Scripts/Entities/Character/Runtime/CharacterRuntime.cs → Assets/Scripts/Entities/Character/Runtime/CharacterActionStateMachine.cs
- `AnimatorComponent` --references--> `AnimatorRootMotionRelay`  [EXTRACTED]
  Assets/Scripts/Components/Animator/AnimatorComponent.cs → Assets/Scripts/Components/Animator/AnimatorRootMotionRelay.cs
- `AnimatorComponent` --references--> `IAnimationStateSink`  [EXTRACTED]
  Assets/Scripts/Components/Animator/AnimatorComponent.cs → Assets/Scripts/Entities/Character/Ports/IAnimationStateSink.cs

## Import Cycles
- None detected.

## Communities (129 total, 45 thin omitted)

### Community 0 - "SoulsLike.Items"
Cohesion: 0.19
Nodes (7): SoulsLike.Items, SoulsLike.Entities.Character.Components.Inventory, SoulsLike.Ui.Inventory.Data, SoulsLike.Ui.PauseNavigation, SoulsLike.Entities.Character.Components.Equipment, SoulsLike.Ui.Inventory, SoulsLike.Ui.Equipment

### Community 1 - "SceneReference"
Cohesion: 0.15
Nodes (8): bool, string, SceneReference, IComparable, IEquatable, ISerializationCallbackReceiver, Obsolete, SceneAsset

### Community 2 - "HealthComponent"
Cohesion: 0.08
Nodes (13): DamageRequest, DamageResult, HealthStats, HealthStatUpdate, HealthComponent, DamageRequest, DamageResult, HealthStats (+5 more)

### Community 3 - "WeaponDefinition"
Cohesion: 0.15
Nodes (11): RuntimeAnimatorController, AnimationProfile, CombatProfile, bool, float, GameObject, int, Sprite (+3 more)

### Community 4 - "IInitializable"
Cohesion: 0.06
Nodes (28): List, Entity, IEntity, EntityCommand, Collider, Dictionary, RaycastHit, EntityLocator (+20 more)

### Community 5 - "AnimatorStateMachineReceiver"
Cohesion: 0.08
Nodes (19): Animator, AnimatorStateInfo, bool, float, int, AnimatorStateMachine, AnimatorStateInfo, AnimatorStateMachineDto (+11 more)

### Community 6 - "PreviewRenderService"
Cohesion: 0.06
Nodes (24): Dictionary, LayerMask, LayerName, SerializedDictionary, LayerData, GameObject, LayerMask, ILayerService (+16 more)

### Community 7 - "ItemDefinition"
Cohesion: 0.20
Nodes (8): float, int, IReadOnlyList, List, Sprite, string, ItemDefinition, EquipmentGroup

### Community 8 - "PlayerHudUiController"
Cohesion: 0.06
Nodes (25): bool, float, DamageResult, HealthModel, bool, float, HealthStats, CanvasGroup (+17 more)

### Community 9 - "EquipmentSlotId"
Cohesion: 0.17
Nodes (7): Dictionary, EquipmentModel, IReadOnlyList, EquipmentSlotCatalog, EquipmentSlotGroup, EquipmentSlotId, HandMode

### Community 10 - "AnimatorComponent"
Cohesion: 0.10
Nodes (12): AnimationEvent, AnimatorModel, AnimatorStateMachineReceiver, bool, float, int, string, Transform (+4 more)

### Community 11 - "Character"
Cohesion: 0.18
Nodes (8): float, HealthStats, int, InventoryComponent, Character, CharacterAttributeStats, EquipmentPresentation, LayerMask

### Community 12 - "ITimer"
Cohesion: 0.11
Nodes (7): LandingType, ITimer, bool, float, Timer, TimerFactory, Prospector.Utility.Timer

### Community 13 - "MovementComponent"
Cohesion: 0.13
Nodes (13): bool, float, ITimer, LandingType, Transform, MovementComponent, CharacterController, Dictionary (+5 more)

### Community 15 - "EquipmentUi"
Cohesion: 0.15
Nodes (7): Dictionary, GameObject, Image, int, TMP_Text, Transform, EquipmentUi

### Community 16 - "InventoryUi"
Cohesion: 0.12
Nodes (10): Color, Image, int, IReadOnlyList, List, ScalingGrade, TMP_Text, Transform (+2 more)

### Community 17 - "EquipmentComponent"
Cohesion: 0.16
Nodes (10): Inject, InventoryComponent, EquipmentComponent, EquipmentModel, EquipmentSlotId, InventoryChange, InventoryEntry, InventoryEntryId (+2 more)

### Community 18 - "CharacterActionState"
Cohesion: 0.07
Nodes (20): UnityCharacterClock, bool, CharacterActionStateId, CharacterInputBatch, AttackState, CharacterActionState, CharacterActionStateMachine, EquipmentSwapState (+12 more)

### Community 19 - "InventorySlotUI"
Cohesion: 0.13
Nodes (12): AxisEventData, BaseEventData, Image, MPImage, PointerEventData, TMP_Text, InventorySlotUI, IDeselectHandler (+4 more)

### Community 20 - "InventoryItemViewData"
Cohesion: 0.22
Nodes (4): CharacterAttributeStats, IReadOnlyList, InventoryItemViewData, IReadOnlyDictionary

### Community 21 - "SoulsLike.Entities.Character"
Cohesion: 0.27
Nodes (4): SpeedMultiplierKey, SoulsLike.Services.Targeting, SoulsLike.Services.CameraService, SoulsLike.Entities.Character

### Community 22 - "AudioService"
Cohesion: 0.21
Nodes (5): List, AudioService, IAudioSettingsData, IAudioService, IObserver

### Community 23 - "CameraService"
Cohesion: 0.11
Nodes (9): bool, Camera, float, Transform, Tween, CameraService, CinemachineCamera, CinemachineThirdPersonFollow (+1 more)

### Community 24 - "PauseNavigationUiController"
Cohesion: 0.17
Nodes (4): IEquipmentRoute, IPauseNavigationRoute, PauseNavigationUiController, Stack

### Community 25 - "SoulsLike.Services"
Cohesion: 0.10
Nodes (12): AssetMappingData, IKeyValue, KeyValue, Dictionary, List, SerializedDictionary, Dictionary, UnityDictionaryFactory (+4 more)

### Community 26 - "OnGuiFpsCounter"
Cohesion: 0.14
Nodes (8): bool, Color, float, GUIStyle, int, OnGuiFpsCounter, Key, Texture2D

### Community 28 - "CoreGameOrchestrator"
Cohesion: 0.06
Nodes (13): string, CharacterFactory, List, CoreGameOrchestrator, ICoreGameOrchestrator, GameState, IGameStateNotifier, IGameStateObserver (+5 more)

### Community 29 - "AmbienceService"
Cohesion: 0.18
Nodes (5): float, GameObject, Tween, AmbienceService, AudioSource

### Community 31 - "AttackComponent"
Cohesion: 0.17
Nodes (14): AnimatorStateMachineDto, AttackType, bool, float, HandMode, ITimer, StateMachineName, AttackComponent (+6 more)

### Community 32 - "CoroutineService"
Cohesion: 0.24
Nodes (7): Action, CoroutineService, ICoroutineService, Coroutine, SoulsLike.Services.Coroutines, Func, IEnumerator

### Community 33 - "CustomButtonToggle"
Cohesion: 0.13
Nodes (9): bool, ColorBlock, GameObject, Image, SelectionState, Sprite, TMP_Text, CustomButtonToggle (+1 more)

### Community 34 - "AnimatorRootMotionRelay"
Cohesion: 0.15
Nodes (8): Inject, Animator, bool, string, AnimatorRootMotionRelay, Quaternion, Vector3, IRootMotionSink

### Community 37 - "UiFactory"
Cohesion: 0.33
Nodes (4): AssetMappingData, Transform, UiFactory, Inject

### Community 38 - "SoulsLike.Services.Audio.Data"
Cohesion: 0.29
Nodes (4): float, AudioData, SoulsLike.Services.Audio.Data, SoulsLike.Services.Audio

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
Cohesion: 0.15
Nodes (4): EquipmentSlotChange, string, InventoryEntryId, IEquipmentPresenter

### Community 43 - "BaseUi"
Cohesion: 0.17
Nodes (5): bool, CanvasGroup, Transform, BaseUi, IBaseUi

### Community 44 - "MainMenuUiController"
Cohesion: 0.07
Nodes (11): UniTask, UniTaskVoid, IGameOrchestrator, IMainMenuOrchestrator, MainMenuOrchestrator, IMainMenuPresenter, MainMenuUi, MainMenuUiController (+3 more)

### Community 45 - "AmbienceData"
Cohesion: 0.18
Nodes (10): float, AmbienceData, MusicEntry, SceneMusicEntry, SfxEntry, MusicType, AudioClip, MusicEntry (+2 more)

### Community 46 - ".Hide"
Cohesion: 0.18
Nodes (5): bool, Camera, LockOnUiController, IPostLateTickable, UiController

### Community 47 - "CharacterRuntime.cs"
Cohesion: 0.17
Nodes (17): Vector2, CharacterCommandFactory, AttackCommand, AttackIntent, AttackRequest, CharacterCommandBufferPolicy, CharacterCommandKind, CharacterControlFrame (+9 more)

### Community 48 - "System.Ui.Base"
Cohesion: 0.17
Nodes (5): SerializedProperty, CustomButtonToggleEditor, System.Ui.Base, UI.Base.Editor, ToggleEditor

### Community 49 - ".CreateButton"
Cohesion: 0.41
Nodes (5): GameObject, CustomButtonHierarchyMenu, CustomButtonMapping, MenuCommand, MenuItem

### Community 51 - "InventoryUiController"
Cohesion: 0.18
Nodes (7): InventoryChange, InventoryChangeType, Action, bool, IReadOnlyCollection, InventoryUiController, HashSet

### Community 52 - "UiService"
Cohesion: 0.24
Nodes (6): UiController, List, Transform, IUiService, UiService, Canvas

### Community 53 - "ICameraService"
Cohesion: 0.18
Nodes (3): Vector2, ICameraService, Ray

### Community 55 - "EquipmentSlotUI"
Cohesion: 0.14
Nodes (10): BaseEventData, Color, GameObject, Image, MPImage, PointerEventData, TMP_Text, EquipmentSlotUI (+2 more)

### Community 56 - "PlayerController"
Cohesion: 0.14
Nodes (9): Transform, ICameraService, IInputService, PlayerController, GameState, IGameStateNotifier, IGameStateObserver, ILateTickable (+1 more)

### Community 58 - "TargetLockNode"
Cohesion: 0.21
Nodes (7): Transform, TargetLockAnchorType, TargetLockNode, float, Transform, ITargetingService, TargetingService

### Community 59 - "AddressableAssetService"
Cohesion: 0.09
Nodes (15): GameObject, AddressableAssetService, IAssetService, LifetimeScope, BaseFactory, CanvasGroup, CanvasGroupExt, AssetMappingData (+7 more)

### Community 60 - "SharedSceneScope"
Cohesion: 0.20
Nodes (7): IContainerBuilder, CoreScope, IContainerBuilder, MainMenuScope, IContainerBuilder, SharedSceneScope, LifetimeScope

### Community 61 - "CustomButtonMapping"
Cohesion: 0.22
Nodes (6): IReadOnlyList, List, ButtonTypeMap, CustomButtonMapping, InputTypes, UI.Base

### Community 62 - ".Move"
Cohesion: 0.14
Nodes (3): Inject, Vector2, IMovementPresentationSink

### Community 63 - "AmbienceManagerWrapper"
Cohesion: 0.27
Nodes (3): float, AmbienceManagerWrapper, Component

### Community 64 - "ItemTypes.cs"
Cohesion: 0.16
Nodes (14): float, ConsumableDefinition, float, int, string, AttributeRequirements, AttributeScaling, ItemStatSnapshot (+6 more)

### Community 66 - ".Read"
Cohesion: 0.11
Nodes (13): CharacterActionStateId, CharacterInputBatch, PlayerCharacterInputAdapter, bool, HeavyAttackGestureResolver, bool, SprintRollGestureResolver, CharacterCommandFactory (+5 more)

### Community 67 - "InventoryViewStateController"
Cohesion: 0.39
Nodes (3): CanvasGroup, InventoryViewState, InventoryViewStateController

### Community 68 - "FileService"
Cohesion: 0.36
Nodes (3): string, FileService, SoulsLike.Services.Save

### Community 70 - "InventoryItemSO"
Cohesion: 0.15
Nodes (9): InventoryPrimaryCategory, InventorySubCategory, bool, float, int, Sprite, string, InventoryItemSO (+1 more)

### Community 73 - "LockOnUi"
Cohesion: 0.28
Nodes (6): Camera, RectTransform, Transform, Vector3, LockOnUi, SoulsLike.Ui.LockOn

### Community 74 - ".BuildLoadout"
Cohesion: 0.17
Nodes (6): EquipmentLoadout, HandMode, EquipmentLoadout, IEquipmentLoadoutSink, EquipmentSlotGroup, EquippedItemContext

### Community 75 - "InventoryEntry"
Cohesion: 0.06
Nodes (30): HealthData, IHealthData, Inject, IReadOnlyList, InventoryComponent, IReadOnlyList, List, InitialInventoryEntry (+22 more)

### Community 76 - "GameOrchestrator"
Cohesion: 0.20
Nodes (6): UniTask, UniTaskVoid, GameOrchestrator, UniTask, ISceneService, ITickable

### Community 77 - "CustomButtonEditor"
Cohesion: 0.33
Nodes (3): SerializedProperty, CustomButtonEditor, ButtonEditor

### Community 78 - "EquipmentPresentation"
Cohesion: 0.30
Nodes (7): EquipmentLoadout, EquippedItemContext, GameObject, Quaternion, Transform, Vector3, EquipmentPresentation

### Community 79 - "CharacterCommandExecutionStatus"
Cohesion: 0.21
Nodes (5): Vector2, CharacterCommandExecutionResult, CharacterCommandExecutionStatus, IAttackCommandReceiver, RollRequest

### Community 81 - "Character.cs"
Cohesion: 0.13
Nodes (10): AttackType, LandingType, LocomotionState, MovementMode, SoulsLike.Entities.Character.Adapters, SoulsLike.Entities.Character.Ports, SoulsLike.Entities.Character.Components.Movement, SoulsLike.Entities.Character.Components (+2 more)

### Community 82 - "SoulsLike.Entities.Character.Components.Health"
Cohesion: 0.14
Nodes (9): float, int, Vector3, DamageRequest, bool, float, HealthStatUpdate, SoulsLike.Entities.Character.Components.Health (+1 more)

### Community 83 - "SceneType"
Cohesion: 0.21
Nodes (6): Scene, SerializedDictionary, SceneData, Scene, SceneModel, SceneType

### Community 85 - "EquipmentSwapCoordinator"
Cohesion: 0.13
Nodes (11): AnimatorStateMachineDto, StateMachineName, CharacterAnimationAdapter, AnimatorStateMachineDto, EquipmentSwapCoordinator, SwapPhase, AnimatorStateMachineDto, CharacterActionStateId (+3 more)

### Community 86 - "SoulsLike.Services.Scenes.Data"
Cohesion: 0.31
Nodes (3): SceneDependency, SoulsLike.Services.Scenes.Data, SoulsLike.Services.Scenes

### Community 87 - "MonoBehaviour"
Cohesion: 0.83
Nodes (3): BaseComponent, IComponent, MonoBehaviour

### Community 88 - "SceneReferencePropertyDrawer"
Cohesion: 0.25
Nodes (6): GUIStyle, SerializedProperty, SceneReferencePropertyDrawer, GUIContent, PropertyDrawer, Rect

### Community 89 - "SceneService"
Cohesion: 0.39
Nodes (5): IReadOnlyList, UniTask, SceneService, AsyncOperation, LoadSceneMode

### Community 90 - ".ApplyAnimationProfile"
Cohesion: 0.16
Nodes (3): AnimationProfile, HandMode, EquipmentLoadout

### Community 91 - ".ApplyAnimationMovement"
Cohesion: 0.29
Nodes (3): Quaternion, Quaternion, Vector3

### Community 93 - "MovementModel"
Cohesion: 0.29
Nodes (5): AnimatorModel, AnimationCurve, LayerMask, MovementModel, Model

### Community 94 - "CharacterRuntime"
Cohesion: 0.16
Nodes (5): bool, CharacterRuntime, MovementGate, MovementGateReason, MovementPolicy

### Community 95 - "IAnimationStateSink"
Cohesion: 0.33
Nodes (3): AnimatorStateMachineDto, AnimatorStateMachineDto, IAnimationStateSink

### Community 97 - ".Open"
Cohesion: 0.29
Nodes (4): Action, IReadOnlyCollection, IInventoryRoute, IReadOnlyCollection

### Community 101 - "AudioSettingsData"
Cohesion: 0.50
Nodes (3): bool, float, AudioSettingsData

## Knowledge Gaps
- **10 isolated node(s):** `SpeedMultiplierKey`, `SwapPhase`, `LandingType`, `LocomotionState`, `Model` (+5 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **45 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Character` connect `Character` to `HealthComponent`, `IInitializable`, `AnimatorComponent`, `MovementComponent`, `EquipmentUiController`, `EquipmentUi`, `EquipmentComponent`, `AttackComponent`, `AnimatorRootMotionRelay`, `CharacterRuntime.cs`, `InventoryUiController`, `PlayerController`, `.Submit`, `.Move`, `.TriggerRoll`, `.PlayAttack`, `.BuildLoadout`, `CharacterCommandExecutionStatus`, `Character.cs`, `EquipmentSwapCoordinator`, `MonoBehaviour`, `.ApplyAnimationProfile`, `.ApplyAnimationMovement`, `CharacterRuntime`, `IAnimationStateSink`, `.SetAirborneMotion`?**
  _High betweenness centrality (0.337) - this node is a cross-community bridge._
- **Why does `AmbienceService` connect `AmbienceService` to `IInitializable`, `SoulsLike.Services.Audio.Data`, `GameOrchestrator`, `AmbienceData`, `IAmbienceSystem`, `AudioService`?**
  _High betweenness centrality (0.069) - this node is a cross-community bridge._
- **Why does `BaseUi` connect `BaseUi` to `PauseNavigationUi`, `PlayerHudUiController`, `LockOnUi`, `MainMenuUiController`, `.Hide`, `EquipmentUi`, `InventoryUi`, `UiService`, `MonoBehaviour`, `CoreGameOrchestrator`?**
  _High betweenness centrality (0.064) - this node is a cross-community bridge._
- **What connects `SpeedMultiplierKey`, `SwapPhase`, `LandingType` to the rest of the system?**
  _10 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SceneReference` be split into smaller, more focused modules?**
  _Cohesion score 0.14705882352941177 - nodes in this community are weakly interconnected._
- **Should `HealthComponent` be split into smaller, more focused modules?**
  _Cohesion score 0.07716701902748414 - nodes in this community are weakly interconnected._
- **Should `IInitializable` be split into smaller, more focused modules?**
  _Cohesion score 0.05565638233514821 - nodes in this community are weakly interconnected._