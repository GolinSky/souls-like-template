---
title: Character Architecture and Readability Improvement
type: plan
domains:
  - character
  - architecture
status: draft
authority: advisory
updated: 2026-09-21
verified: 2026-09-21
source_commit: 8d54481f
tags:
  - work/plan
  - status/draft
---
# Character Architecture and Readability Improvement

## Plan Contract

### Goal

Make the Character code easier to understand and change from an architecture point of view while preserving its gameplay behavior. Apply Clean Architecture dependency direction and SOLID where they clarify real ownership. Keep the useful Unity-oriented Facade, State, Observer, and Factory patterns already present. Avoid creating dozens of small classes or interfaces.

This is a draft proposal only. The current request authorizes analysis and Obsidian notes, not project implementation.

### Source Research and Decisions

Reviewed at commit 8d54481f on 2026-09-21. The companion [[Work/Issues/Character Aggregate Architecture and Readability]] records source locations and the current call paths. [[Knowledge/Architecture/Systems/Character System]] is useful advisory context; live source is authoritative. [[Work/Plans/Character Factory DI Refactor]] already owns the factory/scope refactor and must not be duplicated.

#### Current architecture worth preserving

| Area | Current role | Decision |
|---|---|---|
| Character.cs | Aggregate facade and coordinator | Keep one clear entry point for workflows crossing components. A large facade is acceptable when its methods are easy to locate and its state ownership is explicit. |
| CharacterActionStateMachine, CharacterAction, CharacterInput | Action state and semantic commands | Keep the compact state machine and one-slot buffer. Do not turn each action enum value into a Strategy class. |
| PlayerInputReader, PlayerController | Device translation and session tick/camera coordination | Keep gesture recognition in the reader; keep gameplay rules out of the controller where practical. |
| MovementComponent/Model, AnimatorComponent/Model, CharacterAudioComponent, AnimatorRootMotionRelay | Motor and presentation edges | Keep Unity API and serialized data close to their components. Do not force pure-domain wrappers around every Unity call. |
| AttackComponent, CombatDefenseComponent, CriticalAttackController, HealthComponent/Model, PlayerMeleeCombatRelay | Combat and resources | Keep state with the component that owns it; Character coordinates cross-component use cases. |
| EquipmentComponent/Model/Presentation, InventoryComponent/Model | Loadout and inventory state | Keep specialized components. Clarify the one-way notification seam with Character. |
| CharacterFactory, CharacterScopeInstaller | Creation and lifetime | Keep the child scope. One scope may register gameplay and local-player UI if their lifetime is the same. |
| TargetLockComponent, VisibilityComponent, data objects, enums, DTOs | Adjacent support code | No new abstraction or edit proposed from this review. |

#### Architectural decisions

1. **Use responsibility boundaries, not file size as the target.** Character.cs is about 1,050 lines because it coordinates many existing components. Its Tick and OnAnimationStateChanged methods are the main navigation costs. First group them into named, cohesive steps within Character. Extract a new collaborator only if it owns independent state and reduces the number of places a reader must jump.
2. **Make dependency direction visible.** Character already owns EquipmentComponent, but EquipmentComponent also calls Character.ApplyEquipmentLoadout after raising LoadoutChanged. Use the existing local event so Character observes the loadout and applies presentation. Avoid a global bus or new interface for a single callback.
3. **Give gameplay policy one home.** PlayerController currently restores health, focus, stamina, and flask charges on grace state changes; LevelUpUiFormatter calculates level/currency costs. Put those rules behind meaningful Character operations and preview queries. Keep UI formatting in the formatter and session state observation in PlayerController. Preserve the existing values and behavior.
4. **Document direct component access as a deliberate exception.** A component may expose a narrow, single-component read or operation directly to a UI controller if it owns that rule. Use Character for operations that coordinate several components or must be atomic. Do not add one-line Character wrappers for every EquipmentComponent or InventoryComponent method.
5. **Keep patterns where they fit.** Character is a Facade, CharacterActionStateMachine is a State holder, local events are Observer, CharacterFactory is a Factory. The project's Controller-Presenter-View guidance governs UI. MVC/MVP/MVVM are not useful labels for motor/combat gameplay code. Dependency inversion should be added only at an external boundary or a demonstrated replacement seam, not between every MonoBehaviour.
6. **Treat local API cleanup as secondary.** BaseComponent<TModel>.Model has a public setter and AttackComponent exposes a context that Character passes straight back to it. Narrow those APIs only after checking current VContainer injection and all callers. No behavior change is intended.

### Assumptions and Non-Goals

- Preserve current player behavior, action timing, animation callbacks, prefab field names, asset references, and public use cases.
- Preserve existing gameplay rules and balance; this plan concerns code structure and readability.
- No wholesale layer rewrite, command bus, service per action, interface per component, or split scope merely for conceptual purity.
- Do not edit scenes, prefabs, Animator Controllers, ScriptableObjects, packages, project settings, or Obsidian configuration as part of this plan.
- UI code changes, if later authorized, follow [[Knowledge/Guides/UI/UI Code Build Guide]] and the required ui-style and ui-asset-layout rules. No visual UI redesign is proposed.

### Success Criteria

- The architecture map names one owner for each Character state and one clear direction of calls between Character and each component.
- A reader can trace input -> action -> movement/combat -> animation feedback without reading unrelated grace, item, or progression code.
- EquipmentComponent no longer needs a concrete Character reference for loadout publication.
- Progression and grace restoration policy are located with gameplay code, while UI and PlayerController retain their input/presentation/session roles.
- Future changes add at most a small number of cohesive methods or collaborators, each justified by a clear reason to change.
- Existing behavior and serialized Unity references are preserved.

## Execution Plan

- [ ] Phase 1 — Write the ownership/call-path map before editing. Identify external Character entry points, component-owned state, direct UI reads/writes, and the actual Unity/VContainer lifecycle. Decide which direct component calls are intentionally allowed by the rule above. Verify: the map covers input, action, movement, combat, animation feedback, equipment, inventory, progression, grace, death, and creation without duplicate owners.
- [ ] Phase 2 — Remove the two-way equipment dependency. In Assets/Scripts/Components/Equipment/EquipmentComponent.cs, publish the existing LoadoutChanged event without calling Character. In Assets/Scripts/Entities/Character/Character.cs, subscribe during initialization, apply the loadout once, and unsubscribe during disposal. Preserve current update order and presentation. Verify: assign, unequip, inventory removal, hand mode, and swap paths produce the same loadout, weapon context, and animation profile.
- [ ] Phase 3 — Move policy out of adapter/presentation code. Keep Assets/Scripts/Entities/Character/PlayerController.cs responsible for game-state observation but delegate grace resource restoration to one meaningful Character operation. Make Character own level-up cost/commit rules and expose the same preview data to Assets/Scripts/Ui/LevelUp/LevelUpUiController.cs; keep text/color formatting in LevelUpUiFormatter.cs. Verify: existing grace and level-up values, notifications, and UI output remain the same.
- [ ] Phase 4 — Improve Character navigation in place. Reorder related fields/methods or introduce a few cohesive private steps for Tick and OnAnimationStateChanged (for example action input, movement/presentation, defense/resources, and animation-signal routing). Do not add helpers that merely forward one call. Verify: a code reviewer can follow each flow locally, and the diff changes structure rather than behavior.
- [ ] Phase 5 — Review small API surfaces only if they still hinder reading. Check references before narrowing BaseComponent<TModel>.Model injection access or simplifying the AttackComponent execution-context round trip. Leave them as-is if a change would add complexity or break VContainer conventions. Verify: dependency resolution and current callers remain intact.
- [ ] Phase 6 — Review and record. Compare behavior and serialized references, run relevant existing bounded Edit Mode tests after any code refactor, inspect only the gameplay paths touched, and update [[Knowledge/Architecture/Systems/Character System]] to match verified source. Record remaining validation limits; close the issue only after approved implementation.

Use one writer for overlapping Character.cs phases, sequentially. Reviewers may work read-only. The plan does not preauthorize execution.

## Risks and Rollback

- Extracting too much from Character may increase navigation cost and create a chain of one-use interfaces. Stop after Phase 4 if grouping inside Character is already clear.
- Event subscription order can alter loadout presentation. Compare before/after call paths, keep subscriptions paired with Character lifetime, and revert Phase 2 independently if behavior differs.
- Moving progression/rest code can accidentally change values or event timing. Preserve existing formulas and operation order; revert that phase independently if comparison fails.
- Renaming serialized fields or Unity animation identifiers can break assets. Avoid such renames; any future asset mutation requires the project's Unity persistence workflow.

## Validation

This document was produced by source and dependency inspection only. No code or Unity assets were changed and no Unity test run was needed for the note. If the refactor is later authorized, use existing focused Edit Mode fixtures where they cover changed paths; follow [[Knowledge/Guides/Testing/Unity Test Framework Test Flow]] for clean-scene preflight, bounded async UTF execution, result inspection, and reporting. Add a new test only when it protects a meaningful behavior that the structural edit could change. Defer Play Mode checks to a separate assigned follow-up when they are needed.

## Execution Handoff

Primary files for a future implementation: Assets/Scripts/Entities/Character/Character.cs, PlayerController.cs, Assets/Scripts/Components/Equipment/EquipmentComponent.cs, Assets/Scripts/Ui/LevelUp/LevelUpUiController.cs, LevelUpUiFormatter.cs. Conditional local cleanup: Assets/Scripts/Components/BaseComponent.cs and Assets/Scripts/Components/Attack/AttackComponent.cs. Read-only comparison dependencies: CharacterActionStateMachine.cs, PlayerInputReader.cs, MovementComponent.cs, AnimatorComponent.cs, HealthComponent.cs, InventoryComponent.cs, CharacterFactory.cs, CharacterScopeInstaller.cs.

Required context keys when executing: character-architecture, vault-usage, plan-workflow, unity-testing; ui-code plus the UI rules for UI controller changes. Use hitbox-architecture or animation-code only if future scope actually reaches those systems. Coordinate with [[Work/Plans/Character Factory DI Refactor]] before touching shared lifetime code.

This plan remains draft for review. The current user request ends with analysis and these Obsidian notes.
