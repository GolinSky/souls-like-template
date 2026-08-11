# Character.cs Mediator — Architectural Refactoring ToDo

> **Source**: Architectural audit of [`Character.cs`](file:///F:/Private/SoulsLikeTemplate/Assets/Scripts/Entities/Character/Character.cs)
> **Tags**: #todo #architecture #refactoring #souls-like #character #mediator

---

## 🎯 Overview

`Character` acts as a **Mediator** between 6 components:
- `MovementComponent`
- `AnimatorComponent`
- `AttackComponent`
- `EquipmentComponent`
- `HealthComponent`
- `InventoryComponent`

This note outlines identified architectural issues and actionable refactoring tasks.

---

## 📋 Task Checklist

### 🔴 High Priority Tasks

- [ ] **Split `IComponentMediator` into Role-Specific Interfaces (ISP)**
  - Current state: `IComponentMediator` has 16 methods; every component sees all methods.
  - Action: Create `IMovementNotifier`, `IAnimationNotifier`, `IAttackNotifier`, `IHealthNotifier`, and `IMovementController`.
  - Impact: Compile-time safety and clear component dependencies.

- [ ] **Refactor `NotifyAnimatorStateChanged` God Method**
  - Current state: Handles attack state tracking, hand-mode switch completion, spawn input blocking, and action queue windows in a single 40-line method.
  - Action: Extract state handling into an animation state dispatcher / router pattern.
  - Fix: Correct mis-indented `if` statement at line 252.

- [ ] **Extract Input Processing Pipeline from `UpdateBehaviour`**
  - Current state: `UpdateBehaviour` contains ~80 lines of input parsing, sprint timers, roll buffering, and attack capture logic.
  - Action: Extract input evaluation to a dedicated `CharacterInputProcessor`.

---

### 🟡 Medium Priority Tasks

- [ ] **Remove Dead `SetMediator` in `EquipmentComponent`**
  - `EquipmentComponent` receives `IComponentMediator` via `SetMediator` but never uses it. Remove dead field and setup call.

- [ ] **Decouple or Implement `InventoryComponent`**
  - `InventoryComponent` is an empty script but exposed publicly via `Character.InventoryComponent`.
  - Action: Hide direct component property from public API to preserve mediator encapsulation.

- [ ] **Define `ICharacterController` for External API Surface**
  - Methods like `SetMovementBlocked` and `SetLockOnTarget` coordinate components but are missing from interfaces.
  - Action: Add `ICharacterController` interface for external callers like `PlayerController`.

- [ ] **Eliminate Health Notification Bounce-Back Pattern**
  - `HealthComponent` calls mediator to notify stats changes, which routes right back to `HealthComponent.Model`.
  - Action: Update `HealthModel` directly within `HealthComponent` without intermediate mediator round-trip.

- [ ] **Introduce `AttackIntent` DTO**
  - Abstract multiple granular attack calls (`NotifyAttack`, `SetChargedAttackSpeed`) into a single intent object.

---

### 🟢 Low Priority / Hygiene Tasks

- [ ] **Clean Up Debug Log Calls**
  - Remove leftover `Debug.Log` statements in `Character.cs` (lines 251 & 270).

- [ ] **Move `Cursor.lockState` out of `Character.Initialize()`**
  - Move cursor locking to game state / UI manager level.

- [ ] **Remove Unused Serialized Fields**
  - Delete `_aimTargetDistance` and `_aimLayerMask` from `Character.cs`.

- [ ] **Review `HealthStats` Public Accessor**
  - Encapsulate direct `HealthStats` property on `Character`.

---

## 🏗 Component Communication Summary

| Component | Calls Mediator? | Mediator Calls It? | Status |
|---|---|---|---|
| `MovementComponent` | Yes | Yes | Active |
| `AnimatorComponent` | Yes (via relay) | Yes | Active |
| `AttackComponent` | Yes | Yes | Active |
| `EquipmentComponent` | No | Yes | Dead `SetMediator` |
| `HealthComponent` | Yes | Yes | Contains bounce-back |
| `InventoryComponent` | No | No | Empty placeholder |
