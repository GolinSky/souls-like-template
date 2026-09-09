---
title: Architecture and Systems Audit 2026-09-08
type: research
domains: [architecture, animation, spawn, lifecycle, persistence, ui]
status: draft
authority: evidence
updated: 2026-09-08
source_commit: 30e5f953d4fe5824f3923c5628b3dc8748e10a1a
aliases: []
tags: [research/package, audit/architecture]
---

# Architecture and Systems Audit 2026-09-08

## Required Package

### Question and Desired Decision

Compare [[Research/Architecture and Systems Audit 2026-09-07]] with current architecture, emphasizing system weaknesses that allow a single failed animation to strand character startup or recovery. The user reports three animation-notification failures; their individual reproductions are not available, so this audit identifies the failure class without claiming to diagnose all three incidents.

**Result:** none of the 11 previous findings is demonstrated fixed by the inspected current source. Four additional architecture issue notes identify missing lifecycle completion, missing animation-request correlation, incomplete scene/spawn failure ownership, and abandoned fade completion. Prior conditional findings remain conditional. No runtime reproduction is claimed.

### Scope and Non-Goals

Executed the worktree-safe investigation: read the prior audit and issue evidence, compare source changes since `3925ea83e7cd80b111331a6105835c3e84eeb2b6`, trace current startup/death/grace/scene/fade paths, inspect representative serialized animation wiring, and inspect relevant existing test coverage. Source baseline is the commit in frontmatter; the worktree was clean before this documentation work.

The previous research note explicitly contains recommendations rather than an executable plan. No linked architecture implementation plan exists in `Work/Plans`. This pass executes the requested investigation and creates the dedicated local follow-up; it does not interpret historical recommendations as permission to make unvalidated runtime or asset changes.

No production C#, Unity assets, packages, scenes, or Obsidian configuration changed. No Unity tests, compilation, builds, Play Mode, profiling, or fault injection ran. Interaction discovery has changed since the old audit; its previous migration prose is not current implementation guidance. Enemy AI, ladders, elevators, and rendering were not exhaustively re-audited.

### Current System Map

| Boundary                      | Current owner and weakness                                                                                                                                                     |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Character creation            | CoreGameOrchestrator consumes pending spawn before CharacterFactory finishes; Character.Initialize installs a gameplay lock before triggering presentation.                    |
| Spawn/death/grace progression | Character owns locks and grace protection, but Animator notifications determine when to release them or request respawn.                                                       |
| Animation delivery            | AnimatorStateMachine → AnimatorStateMachineReceiver → AnimatorComponent → Character. DTO carries state/layer information but no originating request identity.                  |
| Action sequencing             | CharacterActionStateMachine checks action categories and counts expected obsolete exits for chained actions. Delivery count substitutes for identifying which execution ended. |
| Scene and destination state   | SceneService owns the load sequence; a separate singleton CharacterSpawnService owns a mutable pending destination. No common operation outcome/commit boundary exists.        |
| Fades                         | Shared FadeUi owns one replaceable tween; callers receive only successful completion callbacks.                                                                                |
| Resources and saves           | The previous load-release ownership and save-success/last-good-file weaknesses remain.                                                                                         |

### Entry Points, Dependencies, and Consumers

- MainMenuOrchestrator.PlayGame → PrepareResume → GameOrchestrator.LoadLevel → SceneService.LoadScene.
- TravelService.Travel → PrepareGraceSpawn → LoadLevel; GraceSystem.Construct resolves and saves the pending grace; CoreGameOrchestrator.Initialize consumes it and creates the character.
- Character.Initialize → SetInputBlocked(true) → TriggerSpawn → Spawn/Exit → SetInputBlocked(false).
- HealthModel death notification → PlayerController.HandleDied → Character.PlayDeath → Death/Exit → OnDeathAnimationCompleted → RespawnAtLastGrace.
- Grace interaction → CoreGameOrchestrator.OnGraceSit → Character.EnterGraceRest → GraceRestIdle/Enter → save checkpoint and publish OnGraceSit.
- RespawnAtLastGrace → FadeIn callback → Ended observers → checkpoint lookup/teleport → FadeOut callback → Idle.

### Evidence and Findings

#### Comparison with September 7

References below are repository-relative paths and current source lines, not the old audit's line numbers. Full scope and original acceptance criteria remain in the linked issue notes.

| #   | Prior finding                                                                 | Current assessment and evidence                                                                                                                                                                                                                                                                                                                                 |
| --- | ----------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | [[Work/Issues/Locked Roll State Is Cleared Before Root Motion]]               | Remains. `Assets/Scripts/Entities/Character/Character.cs:190`; `Assets/Scripts/Components/Movement/MovementComponent.cs:142,158`. Lock updates still clear metadata consumed by root motion.                                                                                                                                                                    |
| 2   | [[Work/Issues/Stance Recovery Is Gated By Poise Delay]]                       | Remains a code/documentation mismatch requiring a balance decision. `Assets/Scripts/Entities/Combat/CombatDefenseComponent.cs:223,238`. Do not silently change the intended recovery rule.                                                                                                                                                                      |
| 3   | [[Work/Issues/Inventory Category Controls Are Not Connected]]                 | Remains. `Assets/Scripts/Ui/Inventory/InventoryUiController.cs:85` has presenter operations without corresponding view category wiring; `Assets/Prefabs/Ui/Inventory/InventoryUi.prefab` assigns both category containers to the same template RectTransform.                                                                                                   |
| 4   | [[Work/Issues/Equipment Picker Compares Against The Wrong Slot]]              | Remains. `Assets/Scripts/Ui/PauseNavigation/PauseNavigationUiController.cs:175` passes broad types; `Assets/Scripts/Ui/Inventory/InventoryUiController.cs:99` uses right-attack comparison. Existing slot-aware `EquipmentUiController.FocusCandidate` is bypassed.                                                                                             |
| 5   | [[Work/Issues/Scene Transitions Allow Concurrent Load Operations]]            | Remains. `Assets/Scripts/Services/Scenes/SceneService.cs:45,69` has no in-flight gate. Extended by A3 below; do not create a second concurrency issue.                                                                                                                                                                                                          |
| 6   | [[Work/Issues/Addressable Asset Loads Have No Release Owner]]                 | Remains. `Assets/Scripts/Services/AssetService/AddressableAssetService.cs:16` and its other load methods discard load handles; matching authored release ownership remains absent. This does not quantify retained memory.                                                                                                                                      |
| 7   | [[Work/Issues/Settings Apply Hides Persistence Failures]]                     | Remains. `Assets/Scripts/Services/Save/SaveService.cs:67` catches write failure; `Assets/Scripts/Services/Settings/SettingsService.cs:250` clears editing state after the void save boundary.                                                                                                                                                                   |
| 8   | [[Work/Issues/Save Writes Can Replace The Last Valid File With Partial Data]] | Remains a resilience gap. `Assets/Scripts/Services/Save/SaveService.cs:79` and `Assets/Scripts/Services/Storage/StorageRegistry.cs` directly overwrite live files; `Assets/Scripts/Services/Save/SaveStore.cs:25` falls back to new data on failed load.                                                                                                        |
| 9   | [[Work/Issues/Respawn Assumes The Last Grace Is In The Current Scene]]        | Conditional, remains. `Assets/Scripts/Orchestrators/Core/CoreGameOrchestrator.cs:163–179` resets death/world state before checkpoint lookup, then only clears the respawning flag on failure. `Assets/Scripts/Services/Spawn/CharacterSpawnService.cs:137–150` throws for another scene or missing grace. `PrepareRespawn` exists at line 66 but has no caller. |
| 10  | [[Work/Issues/Equipment Picker Does Not Filter Exact Slot Compatibility]]     | Conditional, remains. `Assets/Scripts/Ui/Inventory/InventoryUiController.cs:173` filters broad types while `Assets/Scripts/Components/Equipment/EquipmentComponent.cs:49` enforces exact compatibility. Required armor/ammunition reproduction content is absent.                                                                                               |
| 11  | [[Work/Issues/Architecture And Roll Issue Notes Contain Superseded Evidence]] | Remains. In addition to historical migration prose, the required Entity Locator note lists `GetEntity(ulong)` while live `Assets/Scripts/Entities/BaseEntity/IEntityLocator.cs` uses `long`. Preserve the locator boundary rule; use the live signature.                                                                                                        |

The specifically cited Character, Animator notification, action-state-machine, SceneService, and CharacterSpawnService behavior files have no diff from the old audit baseline. CharacterFactory did change: it now constructor-injects IUniqueIdGenerator instead of resolving it through RootScope.Container. That improves explicit dependency ownership but does not alter the consume-before-create ordering or add a startup completion/failure outcome. Interaction probing and support-hand grip changes likewise do not resolve the findings above.

#### A1 — Authoritative lifecycle progress depends on animation delivery (high)

[[Work/Issues/Character Lifecycle Can Stall When Animation Notifications Are Missing]]

Fresh spawn blocks input before setting a trigger (`Character.cs:118–129`) and releases only on Spawn/Exit (`455–465`). Death similarly reaches respawn only through Death/Exit (`219–225,469–475`; `PlayerController.cs:176–183`). Grace has cancellation/finally cleanup (`Character.cs:234–303`), but missing notification does not itself cancel a live interaction. Therefore cancellation support is not a missing-event recovery policy.

The lifecycle has no guaranteed completion/failure path independent of presentation. **Any recovery clock added only to Character.Tick would also stop:** `PlayerController.cs:91–100` returns while dead or input-blocked. This is the highest-priority architectural explanation for the reported class of bugs.

Mitigations already present: grace startup explicitly calls `EnterGraceRestIdle`, resets the pending Spawn trigger, and uses grace-phase protection. `GraceAnimationTests` covers that happy path with/without a pending Spawn trigger and both hand modes. It does not prove fresh-spawn or death liveness under missing callbacks.

Representative content: `Assets/Art/Animation/CharacterGreatSwordAnimator.controller:6773` has `Crouch_Idle_To_Stand_Idle_A` with a Spawn behavior (`8484`, serialized enum 11). A crouch-to-stand clip is valid presentation; its name or reuse is not independently a bug. The architecture weakness is allowing that presentation route to be the sole authority for startup completion. Receiver null-conditional delivery in `Assets/Scripts/Components/Animations/AnimatorStateMachine.cs:49,61,69,75` adds a silent loss path if initialization is wrong; current OnEnable/controller-rebind initialization exists, so an actual ordering race remains unproven.

#### A2 — Chained actions identify completion by event count (medium)

[[Work/Issues/Animation Completion Is Not Correlated To The Owning Action]]

`Assets/Scripts/Components/Animations/AnimatorStateMachineDto.cs:5` carries state/layer data but no execution generation. Character lifecycle handlers consume category/event kind; the receiver forwards every layer. `Assets/Scripts/Entities/Character/Runtime/CharacterActionStateMachine.cs:193–235` increments `_pendingExitsToIgnore` for same-category chains and decrements it on exits.

Source-level counterexample: start attack A, chain attack B, omit A's exit, deliver B's only exit. The counter consumes B's completion as the obsolete exit, leaving Attack active. Conversely, duplicate obsolete exits can exhaust the count early. Current tests at `Assets/Scripts/Tests/CharacterRuntime/CharacterActionStateMachineTests.cs:105,120` cover expected counts, not loss/duplication. Contradictory-category filtering exists and must be preserved. Runtime frequency and layer-specific triggers remain unmeasured.

#### A3 — Scene/spawn failure has no common owner or commit boundary (medium)

[[Work/Issues/Scene And Spawn Failures Have No Recovery Transaction]]

`SceneService.cs:69–93` loads Loading in Single mode before validating/loading the destination. A later error exits without an authored recovery result or cleanup branch. `TravelService.cs:20–21` prepares mutable singleton spawn data separately; `CharacterSpawnService.cs:85–93` saves the destination during grace resolution; `96–113` clears it before `CoreGameOrchestrator.cs:58–60` finishes creating the character. A failed transition can leave either an unresolved request or a persisted destination without a successfully initialized player. Concurrent requests can also overwrite each other's pending destination (existing issue 5).

These are verified ordering/ownership gaps, with failure symptoms conditional on injected load/initialization errors. No successful-travel corruption or runtime leak is claimed. A service-level load lock alone would not fix mutation performed before admission or the save/consume ordering.

#### A4 — Replacing or destroying a fade abandons its caller (medium, conditional integration)

[[Work/Issues/Interrupted Fades Leave Lifecycle Awaiters Unsettled]]

`Assets/Scripts/Ui/Fade/FadeUi.cs:15–63` kills the previous tween and only invokes caller callbacks from OnComplete. `CoreGameOrchestrator.cs:159–173` waits on callback-backed completion sources without cancellation. A replacement or destruction before completion leaves the old wait unsettled; its finally block cannot run while the await remains pending. The shared service also serves `GraceUiController.cs:108`.

DOTween documents `Kill(bool complete = false)` separately from completion and provides OnKill ([upstream contract](https://github.com/Demigiant/dotween/blob/develop/_autodocs/02-tween-extensions.md), retrieved through Context7). The exact behavior must be verified against the installed package locally. The completion-only API and kill paths are source-confirmed; an ordinary gameplay overlap causing a visible deadlock was not reproduced. Do not fix this by calling every interrupted operation successful.

### Options and Tradeoffs

1. Fix lifecycle ownership first: spawn/death/grace need explicit success, cancellation, and failure outcomes with cleanup that continues while gameplay input is blocked. Animation can signal normal progress without owning the only escape path. Avoid silently granting grace rewards/checkpoint writes when presentation fails.
2. Correlate action completion with the execution that started it. A state hash/layer distinguishes different states, but repeated executions of the same state still require an instance/generation contract. Do not attach the current generation to every arriving event: that would relabel stale events as current.
3. Admit scene changes and spawn intent together; resolve destination before destructive loading where possible, define a recoverable failure outcome, and commit persistent spawn data after successful initialization. Preserve existing cross-scene respawn issue scope.
4. Give fade replacement/destruction an explicit cancelled/failed outcome. Avoid a general-purpose lifecycle framework before one bounded owner is proven.
5. Then address retained-resource ownership and persistence outcomes using the previous issues. Keep UI routing fixes separate from lifecycle work; settle stance intent before balance changes.

### Risks, Unknowns, and Open Questions

- The exact three user incidents need local reproduction evidence; no incident-specific causality is asserted.
- No graph snapshot exists in this worktree. Serena tools are absent from the session; the repository-scoped entry was verified without altering configuration. Source search was the fallback. No main-checkout language-server activation or graph rebuild occurred.
- Vault MCP points to the main checkout. Required headings were read there where callable and checked against worktree fallbacks; all output is written in this worktree only.
- Official CLI `1.0.0-beta.6`, `unity status --json`: one ready Unity `6000.3.11f1` Editor, project `F:\Private\SoulsLikeTemplate`, port 7800. No Editor targets this worktree. This is a validation boundary, not proof the worktree fails compilation.
- Ordinary scene residency, measured memory leaks, missing-content equipment behavior, and hypothetical callback races remain distinct evidence categories.

### Recommended Review Questions

- Choose spawn/death/grace failure outcomes: proposed spawn recovery releases only spawn-owned protection after domain readiness; death proceeds exactly once; failed grace entry reports failure without saving a checkpoint.
- Choose scene-request admission behavior: proposed rejection while one transition is active, before changing pending spawn data.
- Choose safe transition recovery destination and when checkpoint persistence commits.
- Confirm stance recovery intent and supported direct scene-entry workflows in their existing issue scopes.

### Handoff

Use [[Work/Plans/Architecture Lifecycle Remediation and Local Validation]] for the dedicated local phase. Review the proposed contracts, promote that draft only when executable, then use one writer per bounded scope. Do not mark the old or new issues resolved from static inspection or from an unrelated checkout's passing tests.

## Evidence Rules

- Verified source: current code, representative controller YAML, test source, baseline diff, CLI identity/status. No code was executed to simulate a runtime failure.
- Inferred impact: outcomes when notifications are omitted/duplicated, scene creation fails, or fades are interrupted. Each issue includes a future reproduction and acceptance gate.
- External source: only DOTween's documented kill/completion contract supports A4; it is not runtime evidence for this project.
- Required context: vault-usage, research-workflow, plan-workflow, issue-workflow, work-routing, animation-code, ui-code, entity-locator. Existing advisory/stale architecture notes do not override live symbols.
