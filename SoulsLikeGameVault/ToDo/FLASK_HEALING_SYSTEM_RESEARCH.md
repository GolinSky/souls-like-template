# Elden Ring Flask Mechanics & SoulsLike Codebase Research

## 1. Executive Summary

This document provides a comprehensive analysis of the **Flask Healing System** in *Elden Ring* and a complete architectural survey of the existing **SoulsLikeTemplate** codebase. It is prepared as a reference dossier so that other AI agents/engineers can review the exact state of existing systems and design an implementation plan.

> [!NOTE]
> This document contains **system research and architectural discovery only**. It does not propose implementation code, modifications, or execution plans.

---

## 2. Elden Ring Reference Flask Mechanics

In *Elden Ring* (and the broader Soulsborne lineage), flask drinking is a core tactical mechanic characterized by **commitment**, **action locking**, and **punishment windows**.

```
[Use Item Input]
       │
       ▼
[Check Charges] ──(Charges == 0)──► [Play Empty Flask Animation] ──► [End Action]
       │ (Charges > 0)
       ▼
[Enter Drink State] ──► Lock Actions (Attacks, Rolls, Weapon Swaps, Jumps)
       │             ──► Reduce Locomotion Speed (Slow Walk, No Sprint)
       ▼
[Windup Phase] (~0.0s - ~0.8s: Character pulls flask from belt and raises to mouth)
       │
       ├─► (Interrupted by Stagger/Knockdown) ──► Cancel Drink, No Charge Consumed, No Heal
       ▼
[Sip Event Frame] (~0.8s: Flask touches lips)
       │
       ├─► Decrement Flask Quantity (-1)
       ├─► Apply Instant HP / FP / Buff to Character
       ├─► Trigger Drink VFX (particle splash) and SFX ("glug" audio)
       ▼
[Chug / Chain Drinking Window] (Early Recovery Phase)
       │
       ├─► (Player presses Use Item again) ──► Loop Sip Animation (Fast consecutive drink, -1 Charge, +Heal)
       ▼
[Recovery Phase] (~0.8s - ~1.8s: Lower flask, return to hip)
       │
       ├─► (Interrupted by Stagger) ──► Heal already applied; cut recovery to hit reaction
       ├─► (Input Queue Window Opens) ──► Buffer subsequent Roll / Attack inputs
       ▼
[Exit Drink State] ──► Restore Normal Locomotion & Action Permissions
```

### 2.1. Flask Variants
1. **Flask of Crimson Tears**: Consumes 1 charge to restore a flat or scaled chunk of Health Points (HP).
2. **Flask of Cerulean Tears**: Consumes 1 charge to restore Focus Points (FP / Mana).
3. **Flask of Wondrous Physick**: Consumes 1 charge to apply custom mixed Crystal Tear effects (e.g., heal over time, explosive damage, stamina recovery boost, temporary damage absorption bubble).

### 2.2. Locomotion & Control Restrictions
- **Slow Walk**: While drinking on foot, the character transitions to a restricted walk speed (~30%–40% of standard move speed). Sprinting and crouching are disabled.
- **Action Lock**: The player cannot initiate light/heavy attacks, weapon skills, spells, weapon swaps, rolls, backsteps, or jumps during the windup and initial sip phase.
- **Rotation**: Character maintains directional control and can turn at a dampened rate while slow-walking.
- **Input Queueing / Buffering**: Pressing roll or attack during the late recovery phase buffers the action, triggering it on the first available exit frame.

### 2.3. Chugging / Multi-Sip Mechanic
- If the player presses the **Use Item** button again during the post-sip window before the flask is put back on the belt:
  - The character does **not** lower the flask.
  - The animation transitions into a rapid repeat sip cycle ("chug loop").
  - An additional charge is consumed and another heal payload is applied.

### 2.4. Empty Flask Interaction
- If current charges are **zero**:
  - The character attempts to drink, lifts the flask, tilts/inverts it, looks inside, shakes it, and performs a frustrated head-scratch / arm drop animation (`Item_Drink_Not`).
  - No health is restored.
  - The player is locked into this animation for ~1.5–2.0 seconds, leaving them completely vulnerable.
  - A distinct dry/empty audio sound effect plays.

### 2.5. Interruption & Poise Rules
- **Pre-Sip Interruption**: If the character takes poise damage sufficient to stagger, knock down, or launch them *before* the sip frame event, the action is cancelled:
  - No flask charge is consumed.
  - No HP is restored.
- **Post-Sip Interruption**: If hit *after* the sip frame during recovery:
  - The HP has already been granted and charge consumed.
  - The stagger animation cancels the remaining recovery duration.

### 2.6. Replenishment at Checkpoints
- Resting at a **Site of Grace** fully refills all flask charges to their maximum allocation.
- In Elden Ring open world, defeating specific enemy groups / crimson teardrop scarabs also refills flask charges dynamically.

---

## 3. Existing Codebase Architecture Survey

This section catalogs all existing subsystems in `f:\Private\SoulsLikeTemplate` that interface with flask usage, items, character actions, health, movement blocking, animations, and UI.

### 3.1. Item & Inventory Subsystem

| File Path | Key Types / Assets | Current State / Responsibility |
|---|---|---|
| [`Assets/Scripts/Items/ItemTypes.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Items/ItemTypes.cs) | `ItemId`, `ItemType`, `EquipmentGroup`, `ItemUseType` | Defines `ItemId.CrimsonFlask = 3`, `ItemType.Consumable = 5`, `EquipmentGroup.QuickItem = 9`, `ItemUseType.Heal = 1`. |
| [`Assets/Scripts/Items/ItemDefinition.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Items/ItemDefinition.cs) | `ItemDefinition` | Stores metadata: `ItemId`, `DisplayName`, `Description`, `Icon`, `Weight`, `MaxStack`, `EquipmentGroups`. |
| [`Assets/Scripts/Items/ConsumableDefinition.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Items/ConsumableDefinition.cs) | `ConsumableDefinition` | Stores `ItemId`, `ItemUseType`, `EffectAmount`, `DurationSeconds`. |
| [`Assets/Scripts/Items/ConsumableDatabase.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Items/ConsumableDatabase.cs) | `ConsumableDatabase` | ScriptableObject database indexing consumable items by `ItemId`. |
| [`Assets/Scripts/Items/ItemCatalog.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Items/ItemCatalog.cs) | `ItemCatalog` | Central VContainer-registered service providing `GetItem(ItemId)` and `GetConsumable(ItemId)`. |
| [`Assets/Settings/Items/ConsumableDatabase.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Settings/Items/ConsumableDatabase.asset) | YAML Asset | Contains entry for `itemId: 3` (`CrimsonFlask`), `useType: 1` (`Heal`), `effectAmount: 60`, `durationSeconds: 0`. |
| [`Assets/Settings/Items/ItemDatabase.asset`](file:///f:/Private/SoulsLikeTemplate/Assets/Settings/Items/ItemDatabase.asset) | YAML Asset | Contains entry for `itemId: 3` (`Crimson Flask`), `icon: b8dd92f11f6bdcb468bf094ff75ff713`, `maxStack: 10`, `equipmentGroups: QuickItem`. |
| [`Assets/Scripts/Components/Inventory/InventoryComponent.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Inventory/InventoryComponent.cs) | `InventoryComponent`, `InventoryEntry` | Manages player inventory entries: `Add()`, `Remove()`, `Consume(InventoryEntryId, quantity)`. |
| [`Assets/Scripts/Components/Equipment/EquipmentSlots.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Equipment/EquipmentSlots.cs) | `EquipmentSlotGroup`, `EquipmentSlotId`, `EquipmentSlotCatalog` | Defines `EquipmentSlotGroup.QuickItem` spanning 10 slots (`QuickItem1` to `QuickItem10`). `IsCyclable(QuickItem)` is `true`. |
| [`Assets/Scripts/Components/Equipment/EquipmentComponent.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Equipment/EquipmentComponent.cs) | `EquipmentComponent`, `EquipmentModel` | Tracks assigned equipment and active slots. `SwitchActive(EquipmentSlotGroup.QuickItem)` advances the active quick item slot. |

#### Current Item Consumption Behavior in Character
In [`Character.cs` (lines 651–686)](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Character.cs#L651-L686):
```csharp
private bool TryUseActiveQuickItem()
{
    EquippedItemContext quickItem = equipmentComponent.BuildLoadout().ActiveQuickItem;
    if (quickItem == null) return false;
    ItemDefinition item = _itemCatalog.GetItem(quickItem.ItemId);
    if (item.ItemType != ItemType.Consumable)
    {
        throw new InvalidOperationException($"Quick-item slot contains non-consumable '{item.DisplayName}'.");
    }

    ConsumableDefinition consumable = _itemCatalog.GetConsumable(quickItem.ItemId);

    switch (consumable.UseType)
    {
        case ItemUseType.Heal:
            Heal(consumable.EffectAmount);
            break;
        case ItemUseType.GrantCurrency:
            GrantCurrency(Mathf.RoundToInt(consumable.EffectAmount));
            break;
        case ItemUseType.InfuseActiveWeapon:
            WeaponRuntime runtime = equipmentPresentation.ActiveRightWeaponRuntime;
            if (runtime == null) return false;
            runtime.ApplyLightningInfusion(consumable.EffectAmount, consumable.DurationSeconds);
            break;
        default:
            throw new ArgumentOutOfRangeException(nameof(consumable.UseType), consumable.UseType, null);
    }

    inventoryComponent.Consume(quickItem.Entry.EntryId);
    return true;
}
```
**Key Observations**:
- Item usage is currently **synchronous and instant**.
- `Heal()` is called immediately on button press.
- `inventoryComponent.Consume()` immediately decreases quantity.
- No state machine state is entered; no animation trigger is fired.
- No empty check or empty-state feedback occurs if the item is not present.

---

### 3.2. Health & Stat Subsystem

| File Path | Key Types | Current State / Responsibility |
|---|---|---|
| [`Assets/Scripts/Components/Health/HealthComponent.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Health/HealthComponent.cs) | `HealthComponent`, `IHealthComponent` | Authoritative health manager. Contains `CalculateHeal()`, `ApplyDamage()`, `ConsumeFocus()`, `RestoreFocus()`, `ConsumeStamina()`, `TickStaminaRecovery()`. |
| [`Assets/Scripts/Components/Health/HealthModel.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Health/HealthModel.cs) | `HealthModel` | Emits `OnStatsChanged`, `OnDamageApplied`, `OnDied`. |
| [`Assets/Scripts/Components/Health/HealthStats.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Health/HealthStats.cs) | `HealthStats` | Struct holding `CurrentHealth`, `MaxHealth`, `CurrentFocus`, `MaxFocus`, `CurrentStamina`, `MaxStamina`, `IsAlive`. |
| [`Assets/Scripts/Entities/Character/Character.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Character.cs) | `Character` | Exposes `public void Heal(float amount) => healthComponent.ApplyAuthoritativeStats(healthComponent.CalculateHeal(healthComponent.Stats, amount));` (lines 595–596). |
| [`Assets/Scripts/Entities/Character/PlayerController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/PlayerController.cs) | `PlayerController` | Refills HP, FP, and Stamina on `GameState.OnGraceSit` (lines 76–85). |

**Key Observations**:
- The health system already has full support for receiving authoritative heals via `CalculateHeal` and `ApplyAuthoritativeStats`.
- `HealthModel.OnStatsChanged` notifies the UI immediately when health changes.
- Focus points (FP) can also be modified with `RestoreFocus(float)`.

---

### 3.3. Input & Character Action State Machine

| File Path | Key Types | Current State / Responsibility |
|---|---|---|
| [`Assets/Scripts/Services/Input/ProjectInputActions.inputactions`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Services/Input/ProjectInputActions.inputactions) | Input Actions | Defines `UseItem` (default binding: 'R' key / Gamepad X/Square) and `SwitchFlask` (default binding: 'Down Arrow' / D-Pad Down). |
| [`Assets/Scripts/Entities/Character/Input/PlayerInputReader.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Input/PlayerInputReader.cs) | `PlayerInputReader` | Reads inputs in `Read(CharacterAction.State currentState)`: `actions.UseItem.WasPressedThisFrame()` generates `CharacterAction.Equipment(CharacterAction.EquipmentKind.UseQuickItem)`. `actions.SwitchFlask.WasPressedThisFrame()` generates `CharacterAction.Equipment(CharacterAction.EquipmentKind.SwitchQuickItem)`. |
| [`Assets/Scripts/Entities/Character/Runtime/CharacterAction.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Runtime/CharacterAction.cs) | `CharacterAction` | Defines `Kind` (`Attack`, `Roll`, `Jump`, `Equipment`), `EquipmentKind` (`SwitchRightWeapon`, `SwitchLeftWeapon`, `SwitchQuickItem`, `UseQuickItem`, `ToggleHandMode`), and `State` (`Neutral`, `Attack`, `Roll`, `EquipmentSwap`, `Critical`). |
| [`Assets/Scripts/Entities/Character/Runtime/CharacterActionStateMachine.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Runtime/CharacterActionStateMachine.cs) | `CharacterActionStateMachine` | Manages active action state, 1.0s input buffer (`Buffer`, `TryGetBufferedAction`), queue windows (`_queueWindowOpen`, `HandleQueueCheck`), and execution gates (`CanExecute`). |

**Key Observations**:
- `CharacterAction.State` currently does **not** include an `ItemUse` or `Drinking` state.
- In `CharacterAction.cs`, `CanBuffer` is currently `ActionKind != Kind.Equipment`.
- In `CharacterActionStateMachine.cs`, states `Attack` and `Roll` open a queue window via `HandleQueueCheck` during their animation, allowing buffered actions to transition smoothly.

---

### 3.4. Movement & Locomotion Blocking Subsystem

| File Path | Key Types | Current State / Responsibility |
|---|---|---|
| [`Assets/Scripts/Components/Movement/MovementComponent.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Movement/MovementComponent.cs) | `MovementComponent`, `MovementModel` | Character locomotion controller. Has `SetMovementBlocked(bool)`, `SetSpeedMultiplier(SpeedMultiplierKey, float)`, and `RemoveSpeedMultiplier(SpeedMultiplierKey)`. |
| [`Assets/Scripts/Entities/Character/SpeedMultiplierKey.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/SpeedMultiplierKey.cs) | `SpeedMultiplierKey` | Enum of speed modifier sources (`InventoryWeight`, `WeaponZoom`, `WeaponTestRiffle`, `Slide`). |
| [`Assets/Scripts/Entities/Character/Character.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Character.cs) | `MovementLockReason` | Bitmask enum: `None = 0, Manual = 1, Animation = 2, Spawn = 4, Parry = 8, Critical = 16`. Evaluated in `SetMovementLock()`. |
| [`Assets/Scripts/Components/Animator/AnimatorRootMotionRelay.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Animator/AnimatorRootMotionRelay.cs) | `AnimatorRootMotionRelay` | Scans active animator state tags for `"RootMotion"` and `"MovementBlocked"`. Calls `Character.SetAnimationMotionContract(movementBlocked)`. |

**Key Observations**:
- `MovementComponent` can enforce either a complete movement lock (`SetMovementBlocked(true)`) or a reduced speed modifier via `SetSpeedMultiplier(key, float)`.
- If a state has the `"MovementBlocked"` or `"RootMotion"` tag in the Animator, `AnimatorRootMotionRelay` automatically coordinates with `Character.SetAnimationMotionContract()`.

---

### 3.5. Animator, StateMachineBehaviours, and Art Assets

| File Path | Key Types | Current State / Responsibility |
|---|---|---|
| [`Assets/Scripts/Components/Animator/AnimatorComponent.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Animator/AnimatorComponent.cs) | `AnimatorComponent` | Central animator controller interface. Manages layer weights (`OneHandedLayer`, `TwoHandedLayer`, `UpperBodyActions`, `FullBodyActions`), parameter hashes, triggers, and state machine observation. |
| [`Assets/Scripts/Components/Animations/AnimatorStateMachine.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Animations/AnimatorStateMachine.cs) | `AnimatorStateMachine` (`StateMachineBehaviour`) | Unity `StateMachineBehaviour` attached to animator states. Reports `OnEnter`, `OnProgress` (at normalized time), `OnQueueCheck` (at normalized time), and `OnExit` to `IAnimatorStateMachineReceiver`. |
| [`Assets/Scripts/Components/Animations/AnimatorStateMachineReceiver.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Animations/AnimatorStateMachineReceiver.cs) | `AnimatorStateMachineReceiver` | MonoBehaviour on character root that receives callbacks from `AnimatorStateMachine` and forwards them to `AnimatorComponent.UpdateState()`. |
| [`Assets/Scripts/Components/Animations/StateMachineName.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Components/Animations/StateMachineName.cs) | `StateMachineName` | Enum of state machines (`Idle`, `LightAttack`, `Roll`, `Spawn`, `EquipmentSwapOut`, `GraceRestStart`, `HitReaction`, `ParryStun`, etc.). |
| [`Assets/Art/Animation/CharacterGreatSwordAnimator.controller`](file:///f:/Private/SoulsLikeTemplate/Assets/Art/Animation/CharacterGreatSwordAnimator.controller) | AnimatorController | The character's primary Animator Controller containing locomotion blend trees, full-body action layers, and upper-body overlay layers. |

#### Available Animation Assets in Repository

The project already contains dedicated item interaction and drinking animations in the DoubleL asset library:

1. [`Assets/ThirdParty/DoubleL/FBX_Animations/Actions/Item/Item_Drink.fbx`](file:///f:/Private/SoulsLikeTemplate/Assets/ThirdParty/DoubleL/FBX_Animations/Actions/Item/Item_Drink.fbx)
   - Full drinking animation clip (drawing item, raising to mouth, drinking, lowering item).
2. [`Assets/ThirdParty/DoubleL/FBX_Animations/Actions/Item/Item_Drink_Not.fbx`](file:///f:/Private/SoulsLikeTemplate/Assets/ThirdParty/DoubleL/FBX_Animations/Actions/Item/Item_Drink_Not.fbx)
   - Empty/failed drink animation clip (lifting flask, inverting/shaking, inspecting, head scratch / disappointed body language).
3. [`Assets/ThirdParty/DoubleL/FBX_Animations/Actions/Item/Item_Use.fbx`](file:///f:/Private/SoulsLikeTemplate/Assets/ThirdParty/DoubleL/FBX_Animations/Actions/Item/Item_Use.fbx)
   - Generic consumable item usage clip.
4. [`Assets/ThirdParty/DoubleL/FBX_Animations/Actions/Item/Item_NotHave.fbx`](file:///f:/Private/SoulsLikeTemplate/Assets/ThirdParty/DoubleL/FBX_Animations/Actions/Item/Item_NotHave.fbx)
   - Item missing / gesture clip.

---

### 3.6. UI / HUD Subsystem

| File Path | Key Types | Current State / Responsibility |
|---|---|---|
| [`Assets/Scripts/Ui/PlayerHud/PlayerHudUi.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/PlayerHud/PlayerHudUi.cs) | `PlayerHudUi`, `StatBar` | Manages visual stat bars (HP bar with trailing yellow damage buffer, FP bar, Stamina bar) and 4 directional equipment HUD slots (`topSlot`, `leftSlot`, `rightSlot`, `bottomSlot`). |
| [`Assets/Scripts/Ui/PlayerHud/EquipmentSlotHud.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/PlayerHud/EquipmentSlotHud.cs) | `EquipmentSlotHud` | Renders individual HUD slot: icon sprite, quantity text count, active/normal border outline color, canvas group alpha dimming (`isDimmed`). |
| [`Assets/Scripts/Ui/PlayerHud/PlayerHudUiController.cs`](file:///f:/Private/SoulsLikeTemplate/Assets/Scripts/Ui/PlayerHud/PlayerHudUiController.cs) | `PlayerHudUiController` | Subscribes to `_healthModel.OnStatsChanged`, `_equipmentComponent.LoadoutChanged`, `_equipmentComponent.SlotChanged`, and `_inventoryComponent.Model.Changed`. Invokes `_playerHudUi.UpdateEquipment()` and `_playerHudUi.UpdateStats()`. |

**Key Observations**:
- The UI already reflects the active Quick Item (icon and quantity) in `bottomSlot`.
- `EquipmentSlotHud` already has built-in support for `SetItem(itemIcon, quantity, isDimmed)` and `SetEmpty(isDimmed)`.
- When an item is consumed or swapped, `PlayerHudUiController` automatically pushes updated view data to `PlayerHudUi`.

---

## 4. Subsystem Interaction Matrix

```
                      ┌───────────────────────────┐
                      │    PlayerInputReader      │
                      └─────────────┬─────────────┘
                                    │ (UseItem / SwitchFlask)
                                    ▼
                      ┌───────────────────────────┐
                      │ CharacterActionStateMach. │
                      └─────────────┬─────────────┘
                                    │ (TryDispatch / Execute)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│                              Character                                 │
│                                                                        │
│  ┌──────────────────────┐  ┌────────────────────┐  ┌────────────────┐  │
│  │  EquipmentComponent  │  │  HealthComponent   │  │  MovementComp. │  │
│  │  (Active Quick Item) │  │  (Apply Heal / FP) │  │  (Slow Walk /  │  │
│  └──────────┬───────────┘  └──────────┬─────────┘  │   Block Lock)  │  │
│             │                         │            └───────┬────────┘  │
│             ▼                         ▼                    │           │
│  ┌──────────────────────┐  ┌────────────────────┐          │           │
│  │  InventoryComponent  │  │    HealthModel     │          │           │
│  │  (Consume Quantity)  │  │  (OnStatsChanged)  │          │           │
│  └──────────┬───────────┘  └──────────┬─────────┘          │           │
│             │                         │                    │           │
└─────────────┼─────────────────────────┼────────────────────┼───────────┘
              │                         │                    │
              ▼                         ▼                    ▼
   ┌──────────────────────────────────────────────────────────────┐
   │                    PlayerHudUiController                     │
   │           ┌───────────────────────────────────────┐          │
   │           │              PlayerHudUi              │          │
   │           │  [HP Bar]  [FP Bar]  [QuickItem Slot] │          │
   │           └───────────────────────────────────────┘          │
   └──────────────────────────────────────────────────────────────┘
```

---

## 5. Summary of Key Discovery Findings

1. **Item Definition & Database**:
   - `CrimsonFlask` already exists with `ItemId = 3`, `ItemType = Consumable`, `EquipmentGroup = QuickItem`, `ItemUseType = Heal`, and `effectAmount = 60`.
   - The icon asset [`CrimsonFlaskIcon.png`](file:///f:/Private/SoulsLikeTemplate/Assets/Art/Textures/ItemIcons/CrimsonFlaskIcon.png) is configured.
2. **Current Character Execution Gap**:
   - `Character.TryUseActiveQuickItem()` executes instant synchronous healing without animation, delay, state tracking, or movement reduction.
3. **Existing Animation Assets**:
   - High-quality FBX clips for drinking (`Item_Drink.fbx`) and empty flask inspection (`Item_Drink_Not.fbx`) are already present in `Assets/ThirdParty/DoubleL/FBX_Animations/Actions/Item/`.
4. **Animation & State Machine Pipeline**:
   - The project uses `AnimatorStateMachine` (`StateMachineBehaviour`) with `OnProgress` and `OnQueueCheck` callbacks routed to `Character.OnAnimationStateChanged()`.
   - `MovementComponent` provides speed multiplier infrastructure (`SetSpeedMultiplier`) and full movement lock (`SetMovementBlocked`).
5. **Health & UI Readiness**:
   - `HealthComponent` and `HealthModel` have complete calculation and notification pipelines for healing.
   - `PlayerHudUi` and `EquipmentSlotHud` already support dynamic stat bars and quantity rendering for quick items.
