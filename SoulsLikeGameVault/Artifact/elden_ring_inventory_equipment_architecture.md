# Elden Ring–Style Inventory & Equipment Architecture Plan

Your current structure is actually a good starting point for this. The main architectural rule I would keep is exactly what you described:

**Inventory/Equipment own state → `Character` mediates changes → Animator / Attack / weapon presentation react.**

You do **not** need an active-slot state machine. Active equipment can remain ordinary model data.

## 1. What you have now

`Character` is already functioning as the component mediator. It wires Movement, Animator, Attack, Equipment and Health together rather than having those components talk directly to each other.

That is the architecture I would keep.

Right now equipment is barely implemented:

- `EquipmentComponent` has an `_equipmentParent`, `_weaponAnchor`, mediator reference, and an unused `_activeSlotIndex`.
- its only real behavior is switching `HandMode`.
- `EquipmentModel` contains only `ActiveHandMode`.
- `InventoryComponent` is empty.
- `InventoryData` is also empty.

So this is a good moment to define the data correctly rather than adapting an existing wrong inventory structure.

There is one current coupling I would change as part of this work. `Character.UpdateBehaviour()` currently says:

`Equipment.SwitchHandMode()` → `Animator.TransitionHandMode()`.

So `Character` already performs the mediation, which is good, but hand switching currently operates independently from the actual equipped item.

That needs to become equipment-aware.

---

# 2. The central data architecture

I would split the system into four layers:

**Item definition**

> What is a Long Sword?

**Inventory entry**

> Alex owns this particular Long Sword.

**Equipment slot**

> That inventory Long Sword is assigned to Right Hand Slot 1.

**Active equipment**

> Right Hand Slot 1 is currently selected, therefore that Long Sword is currently in the character's hand.

This distinction becomes extremely important once you add duplicate weapons, upgrades, infusions, quantities, etc.

---

# 3. ItemId — your foreign-key approach

I agree with your idea.

Have one stable enum:

**ItemId**

Conceptually:

- None
- LongSword
- Claymore
- WoodenShield
- KnightShield
- CrimsonFlask
- LightningGrease
- FireGrease
- GoldenRuneSmall
- GoldenRuneLarge
- etc.

Every inventory entry and every ground item has an `ItemId`.

It works exactly like a database foreign key:

`InventoryEntry.ItemId → ItemDatabase.ItemId`

`GroundItem.ItemId → ItemDatabase.ItemId`

`Equipment → InventoryEntry → ItemId → ItemDatabase`

The important part is:

**do not put all gameplay information directly inside `InventoryItem`.**

`InventoryItem` should reference the database.

---

# 4. Item database

I would create a dedicated **ItemDatabase / ItemCatalog**.

Do not turn the current `InventoryData` into the item database. `InventoryData` should later mean something like initial inventory/save inventory configuration.

Your global database should contain the definitions.

### ItemDefinition

Every item has common information:

| Field | Meaning |
|---|---|
| ItemId | Primary key |
| ItemType | Weapon / Shield / Consumable / etc |
| Name | UI |
| Description | UI |
| Icon | UI |
| MaxStack | 1 / 10 / 99 / etc |
| IsConsumable | gameplay/UI |
| IsEquipable | gameplay/UI |
| WorldPickupPresentation | optional world appearance |
| EquipmentGroups | where this item is allowed |

And then item-specific data is referenced separately.

I would **not** build one monster `ItemData` containing SwordDamage, ShieldBlock, FlaskHeal, SoulAmount, InfusionPower, etc.

Normalize it just like your database analogy.

Conceptually:

```text
ItemDefinition
       |
       +---- WeaponDefinition
       |
       +---- ShieldDefinition
       |
       +---- ConsumableDefinition
       |
       +---- etc.
```

The `ItemId` is effectively the foreign key between those datasets.

---

# 5. Use capabilities, not only ItemType

There are actually two different concepts.

### ItemType

Primarily describes what something **is**:

- Weapon
- Shield
- Consumable
- KeyItem
- Material
- Currency/Soul
- etc.

### Item capabilities/data

Describe what it can **do**.

For example:

**Lightning Grease**

- ItemType = Consumable
- has UseData
- use target = active weapon
- applies temporary lightning infusion

**Crimson Flask**

- ItemType = Consumable
- has UseData
- heals character

**Golden Rune**

- ItemType = Consumable
- has UseData
- grants souls

So you don't end up with:

`if ItemType == LightningConsumable...`

The type is mostly classification/filtering.

The actual behavior comes from the item's use/equipment/combat data.

---

# 6. Weapon definition

Your weapon-specific definition should contain things such as:

**WeaponDefinition**

- ItemId
- weapon family/type
  - StraightSword
  - GreatSword
  - Axe
  - etc.
- attack/combat profile
- base damage/stat data
- animation profile
- weapon prefab/presentation
- right-hand anchor/configuration
- two-handed capability
- blocking capability
- infusion capability
- potentially special attack/weapon art later

One important separation:

### Don't store RuntimeAnimatorController directly as "the sword controller" if many swords share animations.

Instead:

```text
LongSword
    AnimationProfile = StraightSword

Broadsword
    AnimationProfile = StraightSword

Claymore
    AnimationProfile = GreatSword
```

And:

```text
StraightSwordAnimationProfile
    RuntimeAnimatorController = ...

GreatSwordAnimationProfile
    RuntimeAnimatorController = ...
```

That will scale much better.

---

# 7. Your animator setup fits this particularly well

Your `AnimatorComponent` currently owns all the generic character parameters:

- locomotion
- jump
- roll
- crouch
- turn
- lock-on
- attacks
- blocking

And your no-weapon/sword animators have the **same controller structure**.

That's exactly what you want.

So:

```text
No weapon
    animation profile = Unarmed

Sword
    animation profile = StraightSword
```

When active equipment changes:

```text
Equipment
     ↓
Character mediator
     ↓
resolve WeaponDefinition
     ↓
AnimatorComponent.ApplyAnimationProfile(...)
```

`EquipmentComponent` should **never** access the Animator directly.

The mediator remains responsible for that relationship.

Your Animator already handles one-handed/two-handed layer transitions separately.

So controller selection and hand-mode selection can stay two different concepts:

```text
Controller:
    StraightSword

HandMode:
    OneHanded

             ↓

Controller:
    StraightSword

HandMode:
    TwoHanded
```

Perfectly valid.

---

# 8. Inventory runtime data

Here is one place where I would slightly modify your "current equipment only needs ItemId" idea.

An inventory entry should be approximately:

### InventoryEntry

| State | Purpose |
|---|---|
| EntryId | unique runtime/save identity |
| ItemId | FK to ItemDatabase |
| Quantity | stack amount |
| InstanceState | optional mutable data |

Why both `EntryId` and `ItemId`?

Imagine:

```text
LongSword
+0
Lightning affinity

LongSword
+10
Fire affinity
```

Both have:

`ItemId = LongSword`

but they are not the same inventory item anymore.

Elden Ring-style equipment eventually requires this distinction.

So:

```text
ItemId
```

identifies **what kind of item it is**.

```text
InventoryEntryId
```

identifies **the item owned by this character**.

For Flask/Grease/etc. the entry can simply be stackable:

```text
LightningGrease
Quantity = 12
```

For equipment:

```text
LongSword
Quantity = 1
WeaponInstanceState = ...
```

You don't have to implement upgrades/affinities now, but reserving this concept now prevents a painful rewrite later.

---

# 9. Ground item

Your world item should use exactly the same identity system.

### GroundItem

Runtime world representation:

```text
ItemId
Quantity
Optional instance state
```

That's it from the inventory perspective.

Then the component can resolve:

```text
ItemId
 ↓
ItemDatabase
 ↓
name
icon
pickup text
type
inventory rules
etc.
```

So one `GroundItem` implementation can represent:

- sword
- shield
- flask
- grease
- rune/soul
- key item
- whatever

Even if visually they all currently look like the same soul pickup.

Pickup flow:

```text
GroundItem
    ↓
Character / pickup mediator
    ↓
InventoryComponent.Add(...)
    ↓
InventoryModel changed
    ↓
UI refresh / pickup notification
    ↓
destroy GroundItem
```

Later, dropping an upgraded weapon can transfer its instance state back into a `GroundItem`.

---

# 10. Equipment is NOT inventory

This separation is particularly important.

Inventory means:

> What does the player own?

Equipment means:

> Which owned things are assigned to usable slots?

So the EquipmentModel should reference inventory entries.

Conceptually:

```text
Inventory

#17 LongSword
#18 WoodenShield
#19 CrimsonFlask x5
#20 LightningGrease x12
```

Equipment:

```text
RightWeapon[0] = #17
RightWeapon[1] = Empty
RightWeapon[2] = Empty

LeftShield[0] = #18
LeftShield[1] = Empty
LeftShield[2] = Empty

QuickItem[0] = #19
QuickItem[1] = #20
...
```

Equipment doesn't create copies of those items.

---

# 11. Equipment slot groups

This is where your "mapping group" belongs.

I would define an **EquipmentSlotGroupDefinition**.

Example groups:

| Group | Allowed |
|---|---|
| RightWeapon | sword/weapon |
| LeftShield | shield |
| QuickItem | flask + usable consumables |
| Pouch | usable consumables, optional later |

For your project initially:

```text
RightWeapon
    Slot count: 3
    Allowed group: Weapons

LeftShield
    Slot count: 3
    Allowed group: Shields

QuickItem
    Slot count: N
    Allowed group:
        Flask
        Consumable
        SoulConsumable
```

Don't scatter logic like:

```text
if sword -> weapon slot
if shield -> shield slot
if flask -> consumable slot
```

through UI/EquipmentComponent.

Instead the definition says:

```text
LongSword
    EquipGroups = Weapon

KnightShield
    EquipGroups = Shield

CrimsonFlask
    EquipGroups = QuickItem

LightningGrease
    EquipGroups = QuickItem
```

The UI and EquipmentComponent ask the same mapping data.

That prevents inventory UI and gameplay equipment validation from diverging.

---

# 12. EquipmentModel

This is where your current `EquipmentModel` needs its main expansion.

Currently it only contains:

```text
ActiveHandMode
```

Target conceptually:

```text
EquipmentModel

RightWeaponSlots
LeftShieldSlots
QuickItemSlots

ActiveRightWeaponSlot
ActiveLeftShieldSlot
ActiveQuickItemSlot

HandMode
```

That's sufficient.

There is **no active-slot FSM**.

---

# 13. Active-slot logic

This part can remain extremely simple.

For every group:

```text
Slots[]
ActiveIndex
```

Therefore:

```text
ActiveItem = Slots[ActiveIndex]
```

That's your state.

Example:

```text
RightWeaponSlots

0 = Empty
1 = LongSword
2 = Empty

ActiveIndex = 0
ActiveItem = None
```

Input:

```text
SwitchRightWeapon
```

Equipment changes:

```text
ActiveIndex = 1
ActiveItem = LongSword
```

and reports it.

Next switch:

```text
ActiveIndex = 2
ActiveItem = None
```

Exactly your:

> empty → sword → empty

requirement.

You can later choose per group whether switching:

- includes empty slots,
- or skips empty slots.

For your described design I would initially **include empty**, because `None` is a legitimate active equipment state.

---

# 14. Don't make `_activeSlotIndex` global

Your current `EquipmentComponent` already contains:

`private int _activeSlotIndex = -1;`

That won't be sufficient.

There isn't one active slot.

There is:

```text
ActiveRightWeaponIndex
ActiveLeftShieldIndex
ActiveQuickItemIndex
```

or, more generally:

```text
GroupId → ActiveIndex
```

The latter is cleaner if you want it fully generic.

---

# 15. Assignment versus activation

These must be completely different operations.

### Equip/Assign

Inventory UI:

```text
LongSword
 → assign to RightWeapon Slot 1
```

This modifies the equipment slot content.

### Activate

Gameplay input:

```text
SwitchRightWeapon
```

This changes which assigned slot is active.

Don't combine the two.

This distinction is how you get the Elden Ring behavior cleanly.

---

# 16. Active equipment notification

This should go through `Character`.

I would have Equipment produce one meaningful payload conceptually like:

### ActiveEquipmentChanged

```text
Group
PreviousSlot
CurrentSlot

PreviousItem
CurrentItem

HandMode
```

`CurrentItem` can contain/reference:

```text
InventoryEntryId
ItemId
resolved ItemDefinition
```

Then:

```text
EquipmentComponent
        ↓
Character mediator
        ├── AnimatorComponent
        ├── AttackComponent
        └── EquipmentPresentation
```

This is exactly the mediator architecture you're describing.

---

# 17. Character should coordinate the result

For weapon:

```text
Right weapon:
None → LongSword
```

Equipment notifies Character.

Character resolves what changed and does:

```text
Animator
    StraightSword animation profile

Attack
    LongSword combat profile

Equipment presentation
    spawn/show LongSword prefab
```

For:

```text
LongSword → None
```

Character does:

```text
Animator
    Unarmed animation profile

Attack
    Unarmed / no weapon profile

Equipment presentation
    remove/hide LongSword
```

You do not need:

```text
UnarmedState
SwordState
ShieldState
...
```

The item data itself determines the configuration.

---

# 18. AttackComponent

I haven't been given `AttackComponent`, so this part is architectural rather than based on its internal implementation.

From `Character` I can see that Attack already interacts through the mediator: Character tells Animator to play an `AttackType`, while animator state notifications are routed back into Attack.

That relationship should remain.

The missing addition is:

```text
AttackComponent
    CurrentCombatProfile
    CurrentWeaponRuntime
```

When equipment changes:

```text
Character
    ↓
AttackComponent.SetActiveWeapon(...)
```

Then Attack shouldn't need to query Equipment on every attack.

It simply operates with its current combat context.

---

# 19. Weapon runtime component

I agree that the spawned weapon itself should have a **small runtime component**.

But its responsibility needs to stay narrow.

Something like conceptually:

### WeaponRuntime

Responsible for:

- weapon hitbox
- active damage modifiers
- temporary infusion
- weapon VFX
- perhaps weapon-specific collision data
- current runtime weapon state

Not responsible for:

- inventory
- slot switching
- deciding whether it is equipped
- UI
- changing animator controllers

So Lightning Grease would roughly flow:

```text
QuickItem = LightningGrease

Use
 ↓
Consumable effect
 ↓
Character
 ↓
Current active WeaponRuntime
 ↓
Apply temporary Lightning modifier
 ↓
damage + weapon VFX updated
```

That's a very good use for a component on the instantiated weapon.

---

# 20. Temporary infusion versus weapon identity

Do not modify `ItemDefinition`.

For example:

```text
LongSword definition
PhysicalDamage = 100
```

while runtime:

```text
LongSword WeaponRuntime

TemporaryModifiers:
    Lightning +40
    duration 60 sec
```

The database stays immutable.

Same principle for:

- buffs
- durability if you ever use it
- temporary enchants
- coatings
- status effects

---

# 21. Hand mode

This is the area I would change most carefully.

Currently `HandMode` only has:

- OneHanded
- TwoHanded

And Character simply toggles it regardless of equipment.

Instead:

```text
RequestTwoHandMode
       ↓
Equipment validates active equipment
       ↓
new HandMode
       ↓
Character
       ↓
Animator
Attack
Presentation
```

For a sword:

```text
LongSword
CanTwoHand = true
```

so:

```text
OneHanded ⇄ TwoHanded
```

For a shield, according to your restriction, simply make that capability unavailable.

So don't put:

```text
if ItemType == Shield
```

inside Character.

Put the capability in the relevant equipment/weapon data.

---

# 22. Effective equipment versus assigned equipment

This will matter when two-handing.

Suppose:

```text
Right active = LongSword
Left active = KnightShield

HandMode = OneHanded
```

Effective loadout:

```text
Right = LongSword
Left = KnightShield
```

Switch sword to two-handed:

```text
HandMode = TwoHanded
```

Equipment assignments have **not changed**:

```text
Right slot = LongSword
Left slot = KnightShield
```

But effective combat equipment becomes:

```text
Right = LongSword / TwoHanded
Left = None
```

The shield remains assigned.

It just isn't currently usable/presented as the active hand.

Going back to one-handed restores it automatically.

This distinction will save you from a lot of ugly special-case equipment manipulation.

---

# 23. Animator responsibility after this change

Your existing Animator should remain mostly presentation-oriented.

It already has:

- attack triggers
- locomotion
- roll
- jump
- blocking
- one/two-handed layers

and receives notifications rather than deciding gameplay.

Add only the concept:

```text
ApplyAnimationProfile
```

It should not receive:

```text
ItemId.LongSword
```

and internally know what a LongSword means.

Instead:

```text
Character
    ItemId
      ↓
ItemDatabase
      ↓
WeaponDefinition
      ↓
AnimationProfile
      ↓
Animator
```

That keeps the Animator completely independent of inventory data.

---

# 24. AnimatorModel

Your `AnimatorModel` is currently empty.

I wouldn't force equipment state into it.

Animator doesn't need its own copy of:

- current ItemId
- equipped weapon
- inventory entry
- slot index

Those belong to Equipment.

At most AnimatorModel might eventually contain animation-state-related information, but that's unrelated to this inventory work.

---

# 25. Equipment presentation

I would separate one additional concept from `EquipmentComponent`.

You currently already have `_equipmentParent` and `_weaponAnchor`.

Long term, equipment has two responsibilities if you leave these there:

1. equipment state
2. GameObject presentation

I would conceptually separate them as:

```text
EquipmentComponent
    state/selection/validation

EquipmentPresentation
    weapon prefab
    shield prefab
    right-hand anchor
    left-hand anchor
    show/hide/spawn/despawn
```

It can still be a component under Character.

Character mediator coordinates them.

This also makes equipment logic testable without needing instantiated sword prefabs.

---

# 26. UI becomes straightforward

Inventory UI works entirely from:

```text
InventoryEntry
    ↓ ItemId
ItemDatabase
```

Generic inventory slot can therefore display:

```text
Icon
Name
Quantity
```

Details panel resolves the same item:

### Sword

```text
Description
Physical Damage
Scaling
Requirements
Weight
etc.
```

### Shield

```text
Description
Guard
Resistance
etc.
```

### Flask

```text
Description
Heal amount
Quantity
```

### Soul/rune

```text
Description
Soul amount
```

Same generic inventory slot.

Different details renderer according to available item data/capabilities.

That's very close to the data architecture you were describing.

---

# 27. Equipment UI

Equipment UI should **not** contain its own classification rules.

When the user opens:

```text
Right Weapon Slot 1
```

UI asks:

```text
EquipmentSlotGroupDefinition
AllowedGroup = Weapon
```

and filters Inventory entries against that.

Shield slot:

```text
AllowedGroup = Shield
```

Quick slot:

```text
AllowedGroup = QuickUsable
```

So both gameplay validation and UI filtering use the same mapping.

---

# 28. Recommended data hierarchy

Putting everything together, I would aim for this:

```text
ITEM DATABASE

ItemDefinition
 ├─ ItemId
 ├─ DisplayData
 ├─ ItemType
 ├─ StackData
 ├─ EquipmentGroups
 │
 ├─ WeaponDefinition? --------→ CombatProfile
 │                         └──→ AnimationProfile
 │                         └──→ WeaponPrefab
 │
 ├─ ShieldDefinition?
 │
 └─ UseDefinition? ----------→ Heal
                            → InfuseWeapon
                            → GrantSouls
                            → etc.
```

Runtime:

```text
INVENTORY

InventoryModel
 └─ InventoryEntry[]
       ├─ EntryId
       ├─ ItemId
       ├─ Quantity
       └─ InstanceState
```

Equipment:

```text
EQUIPMENT

EquipmentModel

RightWeaponGroup
 ├─ Slots[]
 └─ ActiveIndex

LeftShieldGroup
 ├─ Slots[]
 └─ ActiveIndex

QuickItemGroup
 ├─ Slots[]
 └─ ActiveIndex

HandMode
```

Then derived:

```text
ActiveRightItem
ActiveLeftItem
ActiveQuickItem

EffectiveRightItem
EffectiveLeftItem
```

---

# 29. How your final runtime flow should look

### Picking up sword

```text
GroundItem(LongSword)
        ↓
InventoryComponent
        ↓
InventoryEntry #17 / LongSword
```

Nothing changes on character equipment yet.

---

### Assigning sword

```text
Equipment UI
        ↓
EquipmentComponent
        ↓
RightWeapon Slot 1 = InventoryEntry #17
```

Still doesn't necessarily make it active unless Slot 1 is currently active.

---

### Switching active weapon

```text
input
 ↓
Character
 ↓
EquipmentComponent.SwitchActive(RightWeapon)
 ↓
EquipmentModel changes ActiveIndex
 ↓
Equipment reports ActiveEquipmentChanged
 ↓
Character
 ├─ Animator → StraightSword profile
 ├─ Attack → sword combat profile
 └─ EquipmentPresentation → sword prefab
```

This is the core architecture.

No FSM.

---

# 30. What I would change in the existing files

### `Character`

**Keep it as mediator.**

Add responsibilities for routing:

- active equipment changed
- hand mode changed
- item used
- weapon runtime changed

But don't put inventory/equipment rules inside it.

Its current role already matches this architecture well.

---

### `InventoryComponent`

Build:

```text
InventoryComponent
InventoryModel
```

Responsibilities:

- Add item
- Remove item
- Consume quantity
- find InventoryEntry
- stack handling
- notify inventory changes

Not equipment.

---

### `InventoryData`

Repurpose as one of:

**InitialInventoryData**

or save/initial configuration.

I would not use it as the global item catalogue.

Create separate:

```text
ItemDatabase
```

---

### `EquipmentModel`

Expand substantially.

Own:

```text
Slot groups
Active index per group
HandMode
```

It becomes the authoritative equipment state.

---

### `EquipmentComponent`

Responsible for operations:

```text
Assign
Unequip
SwitchActive
SwitchHandMode
Validate slot compatibility
```

It should not know animator logic.

And remove the idea of one global `_activeSlotIndex`; active selection belongs per equipment group.

---

### `AnimatorComponent`

Keep all existing locomotion/combat animator responsibility.

Add:

```text
apply animation profile/controller
```

Don't make it inventory-aware.

---

### `AttackComponent`

Add active combat context.

Conceptually:

```text
CurrentWeapon
CurrentCombatProfile
HandMode
```

It gets those changes from Character.

---

### new `WeaponRuntime`

Small component on weapon prefab:

```text
weapon runtime modifiers
infusion
hitbox
VFX
damage source
```

No inventory logic.

---

### new `EquipmentPresentation`

Handles:

```text
spawn sword
remove sword
spawn shield
hide shield while 2H
anchors
```

This can initially live in EquipmentComponent if you want to avoid another component immediately, but architecturally I would plan for the separation.

---

# 31. Implementation order I recommend

Do this in the following order because every next stage depends cleanly on the previous one.

### Phase 1 — Item domain

Define:

```text
ItemId
ItemType
EquipmentGroup
ItemDefinition
ItemDatabase

WeaponDefinition
ShieldDefinition
Consumable/UseDefinition
AnimationProfile
CombatProfile
```

No UI and almost no runtime functionality yet.

---

### Phase 2 — Inventory

Define:

```text
InventoryEntry
InventoryEntryId
InventoryModel
InventoryComponent
```

Implement conceptually:

```text
Add
Remove
Stack
Consume
Query
```

Ground items can now work.

---

### Phase 3 — Ground item

Create generic:

```text
GroundItem
ItemId
Quantity
instance state if required
```

Pickup → Inventory.

Now every future item automatically supports world pickup.

---

### Phase 4 — Equipment model

Create:

```text
EquipmentSlotGroupDefinition

EquipmentSlot
EquipmentSlotGroupState
EquipmentModel
```

Start with:

```text
RightWeapon x3
LeftShield x3
QuickItem xN
```

Implement:

```text
Assign
Remove
SwitchActive
```

Still no Animator changes.

---

### Phase 5 — Active equipment mediation

Add:

```text
ActiveEquipmentChanged
```

Equipment → Character.

Character then updates:

```text
Animator
Attack
EquipmentPresentation
```

At this point:

**Empty → sword → empty**

should fully work.

This is the first major gameplay milestone.

---

### Phase 6 — Runtime animator controller

Introduce:

```text
AnimationProfileId
```

For now:

```text
Unarmed
StraightSword
```

Sword's WeaponDefinition references `StraightSword`.

Character applies it when active equipment changes.

Your locomotion/jump/roll structure stays identical across those controllers, which makes this architecture especially suitable.

---

### Phase 7 — Hand mode

Only after active weapons work.

Move from:

```text
press input
→ blindly toggle HandMode
```

to:

```text
press input
→ Equipment requests hand-mode change
→ validate current active item
→ EquipmentModel changes
→ Character notified
→ Animator / Attack / Presentation updated
```

This replaces the current direct `SwitchHandMode()` → `TransitionHandMode()` path.

---

### Phase 8 — Weapon runtime + consumables

Add:

```text
WeaponRuntime
RuntimeWeaponModifiers
```

Then implement first generic item effects:

```text
Heal
GrantSouls
InfuseActiveWeapon
```

Lightning grease becomes your first test of cross-system consumable behavior.

---

### Phase 9 — UI

Only now bind your existing UI prefabs.

Inventory UI works against InventoryModel.

Equipment UI works against EquipmentModel.

Both resolve `ItemId` through `ItemDatabase`.

At that point UI is mostly presentation rather than secretly defining gameplay rules.

---

# 32. The most important architectural distinction

The resulting chain should be:

```text
                 STATIC DATA
                     │
                 ItemDatabase
                     │
                     ▼
GroundItem ─────→ InventoryEntry
                     │
                     ▼
               Equipment Slot
                     │
                     ▼
                 Active Slot
                     │
                     ▼
                  Character
                 /    |     \
                /     |      \
          Animator   Attack   EquipmentPresentation
                               │
                               ▼
                         WeaponRuntime
```

And **not**:

```text
UI
 ↓
Weapon prefab
 ↓
Animator
 ↓
Inventory
```

or:

```text
EquipmentComponent
 ↓
AnimatorComponent
 ↓
AttackComponent
```

The `Character` mediator remains the place where those systems meet.

---

## One modification I strongly recommend to your original idea

Your idea:

> current equipment ItemId = sword/shield/etc.

is fine as a **derived value**, but I would not make `ItemId` the complete equipment reference.

Use:

```text
Equipment Slot
    ↓
InventoryEntryId
    ↓
ItemId
```

Then:

```text
CurrentItemId
```

can simply be obtained from the selected inventory entry.

That small distinction buys you support for **two copies of the same sword with different upgrade/infusion/runtime state** without redesigning inventory/equipment later.

Everything else about your proposed flow—especially **Equipment → Character mediator → Animator/Attack** and avoiding an active-slot FSM—is the direction I would use.
