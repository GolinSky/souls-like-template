---
title: Character Aggregate Architecture and Readability
type: issue
domains:
  - character
  - architecture
status: open
authority: evidence
priority: medium
updated: 2026-09-22
verified: 2026-09-21
source_commit: 8d54481f
tags:
  - work/issue
  - status/open
---
# Character Aggregate Architecture and Readability

## Issue Contract

### Observed Behavior

This is a structural code review. No gameplay malfunction is asserted.

The current design already has useful boundaries: Character is a facade for the player entity; PlayerInputReader translates hardware input; CharacterActionStateMachine owns action state and buffering; movement, animation, attack, equipment, health, and inventory each have focused components; CharacterFactory and CharacterScopeInstaller own construction and lifetime. Keep those choices.

| Architectural concern | Source evidence | Why it costs comprehension |
|---|---|---|
| Character coordinates too many distinct use cases in one file: action dispatch, locomotion, combat gating, animation feedback, equipment presentation, item use, grace, death, currency, leveling, ladder, and platform riding. | Assets/Scripts/Entities/Character/Character.cs:25-1055; Tick at 166-243; OnAnimationStateChanged at 482-584 | A reader must scan unrelated workflows to locate one change. The facade role is sound, but the internal organization lacks a short path for each use case. |
| Character and EquipmentComponent depend on each other as concrete objects. | Character.cs:41,793-814; Assets/Scripts/Components/Equipment/EquipmentComponent.cs:25-33,247-253 | The direction of ownership is unclear: Character coordinates equipment, yet EquipmentComponent calls Character.ApplyEquipmentLoadout after publishing LoadoutChanged. |
| PlayerController contains character resource restoration, and LevelUpUiFormatter contains progression cost rules. | Assets/Scripts/Entities/Character/PlayerController.cs:67-86; Assets/Scripts/Ui/LevelUp/LevelUpUiFormatter.cs:14-41; Assets/Scripts/Entities/Character/Character.cs:742-763 | Session/input and UI presentation classes know gameplay policy. That scatters the answer to “where does Character progression/resting live?” |
| The aggregate boundary is inconsistent: some callers use Character, while PlayerController and UI controllers also inject or expose HealthComponent, InventoryComponent, and EquipmentComponent. | Character.cs:74-77; PlayerController.cs:21-37,67-86; Assets/Scripts/Ui/Equipment/EquipmentUiController.cs:20-38,132-145 | Without a rule for direct component access, new code can choose either path arbitrarily. A blanket facade wrapper for every component method would also add noise. |
| Input translation knows action states, and the scope installer mixes gameplay and local-player UI registrations. | Assets/Scripts/Entities/Character/Input/PlayerInputReader.cs:30-115; Assets/Scripts/Services/VContainer/CharacterScopeInstaller.cs:34-99 | These are understandable today but need an explicit boundary: gesture recognition belongs to input; action eligibility belongs to action state; one scope is fine when lifetime is shared. |
| A few APIs obscure ownership: public injection setter on BaseComponent<TModel>.Model and an attack context read from AttackComponent that is immediately passed back to AttackComponent. | Assets/Scripts/Components/BaseComponent.cs:14-18; Character.cs:387-388; Assets/Scripts/Components/Attack/AttackComponent.cs:64-65,99-103 | Readers must infer whether replacement or external interpretation is intended. These are local cleanup candidates, not reasons for a new layer. |

### Expected Behavior

The code should keep its present behavior while making ownership and navigation explicit:
- Character remains the public coordinator for workflows spanning multiple components.
- Each component remains the owner of its own state; simple, single-component operations may be called directly when that is clearer.
- UI controllers present and collect intent; progression and rest policy live with character gameplay code.
- Dependencies point from Character toward its components. A component reports a result or change without needing to call back into the concrete Character facade.
- Input gesture recognition remains in PlayerInputReader; action eligibility remains in Character/action state.
- Existing Facade, State, Observer, and Factory uses remain. MVC/MVP/MVVM are considered for UI only, where the project already uses Controller-Presenter-View.

### Reproduction

This is a static architectural observation. To see the reading cost, trace a weapon change through EquipmentUiController -> EquipmentComponent -> Character -> EquipmentPresentation/AnimatorComponent/AttackComponent, or trace a level-up through LevelUpUiController -> LevelUpUiFormatter -> Character. No runtime reproduction is required.

### Impact and Priority

Priority: medium, maintainability. The present structure increases the effort to understand and change Character workflows. This note does not claim incorrect game rules or a player-visible failure.

### Evidence

Reviewed live source at commit 8d54481f on 2026-09-21. Graphify was used to locate relationships, then the paths above were checked against current source. [[Knowledge/Architecture/Systems/Character System]] is advisory and partially verified; its aggregate-facade intent is useful, while its detailed claims should be checked against source before updating it.

Related active work: [[Work/Plans/Character Factory DI Refactor]] owns factory/scope lifecycle, and [[Work/Plans/Architecture Lifecycle Remediation and Local Validation]] owns broader lifecycle work. This review does not duplicate those plans.

### Hypotheses

- Reordering Character methods and extracting only cohesive internal blocks may provide most of the readability benefit without new classes.
- The existing LoadoutChanged event can remove the EquipmentComponent -> Character dependency without a new abstraction.
- A small Character-owned progression/rest API can clarify policy ownership while allowing UI to keep direct read access to component snapshots where useful.

These are design options for review, not verified gameplay problems.

### Open Questions

- Which direct component calls should remain because they are simpler than one-line Character wrappers? The proposed rule is direct access for one-component operations, Character for multi-component workflows.
- Does the team prefer one Character file with clearly grouped private methods, or one small collaborator for a stateful workflow after the low-change cleanup is assessed?

## Resolution Handoff

### Approved Fix Scope

The user authorized execution of [[Work/Plans/Character Clean Architecture]] on 2026-09-22. Its Option A implementation preserves serialized Unity references and existing factory/entity infrastructure, with VContainer composition, a small pure rule core, and a local session boundary. No generic service framework or pattern-only class splitting was added.

### Implementation Ready for Review

The input/controller path now uses session contracts; action priority and dispatch use a pure coordinator and the existing state machine; actor ticking is independent of local input/UI/camera. Equipment publishes its existing event instead of calling Character. Attack reads its own context. Shared progression/stamina calculations live in the engine-independent runtime, and Character owns the rest operation. Short summaries explain new and touched production classes.

The public Model setter remains explicitly documented for VContainer and existing isolated component fixtures, as allowed by the plan. Scoped actor registration is separated from local-player registration. The existing entity-command boundary and serialized fields are preserved.

Initial evidence: successful Unity compilation, 19/19 runtime tests, 6/6 formatter/progression tests, zero current console errors, all scenes clean, and no active test run. Source review found no remaining material defect after correcting camera update parity. See [[History/Records/Character Clean Architecture Implementation]]. This issue remains open for the user's review and deferred gameplay parity checks; the original observations above are historical evidence, not a claim that every old coupling remains.

### Acceptance Criteria

- A reader can identify the owner and call path for input, action, movement, animation feedback, equipment, progression, grace, and combat from the architecture map.
- The EquipmentComponent/Character dependency direction is explicit.
- UI and controller classes contain presentation/session responsibilities; cross-component character policy has one owner.
- Any extracted class has independent state or a clear reason to change; no wrapper exists only to satisfy a pattern.
- Existing gameplay behavior remains unchanged after any future implementation.

### Validation

Current review: source and dependency inspection only. After an authorized refactor, compare call paths and serialized fields, compile, run relevant existing Edit Mode tests, and perform only the specific gameplay checks needed to show behavior was preserved.
