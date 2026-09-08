---
title: Architecture Lifecycle Remediation and Local Validation
type: plan
domains: [architecture, character, animation, spawn, scenes, lifecycle]
status: draft
authority: advisory
updated: 2026-09-08
source_commit: 30e5f953d4fe5824f3923c5628b3dc8748e10a1a
aliases: []
tags: [work/plan, status/draft]
---

# Architecture Lifecycle Remediation and Local Validation

## Plan Contract

### Goal

Execute the Unity-dependent work in the local checkout after review: prevent missing animation/fade notifications from permanently blocking gameplay, and give scene/spawn failures one recoverable outcome. Verify the actual reported spawn failure class before broadening fixes.

### Source Research and Decisions

- Current comparison: [[Research/Architecture and Systems Audit 2026-09-08]]. Historical baseline: [[Research/Architecture and Systems Audit 2026-09-07]].
- Priority order: character lifecycle progress; action-event ownership; scene/spawn recovery; fade cancellation. Existing high-priority roll and inventory defects remain separate bounded fixes, not forgotten or included in an animation rewrite.
- New issues: [[Work/Issues/Character Lifecycle Can Stall When Animation Notifications Are Missing]], [[Work/Issues/Animation Completion Is Not Correlated To The Owning Action]], [[Work/Issues/Scene And Spawn Failures Have No Recovery Transaction]], [[Work/Issues/Interrupted Fades Leave Lifecycle Awaiters Unsettled]].
- Proposed design: gameplay owns operation outcomes; animation reports normal progression. A missing notification becomes an explicit recoverable failure or the reviewed death/spawn recovery policy, never an indefinite wait or a fabricated grace success.
- Draft status is intentional. The user requested investigation plus a dedicated plan for work that requires local Unity. No architecture implementation plan preceded this audit. Review the concrete decisions below and move to ready before executing this local remediation.

### Assumptions and Non-Goals

- The current worktree has no connected Editor. On 2026-09-08, official CLI status found only `F:\Private\SoulsLikeTemplate`, Unity `6000.3.11f1`, port 7800. Do not validate different source using that Editor and report it as worktree validation.
- Bring the reviewed changes into the local checkout through normal Git coordination; first inspect its branch, HEAD, uncommitted changes, and scene state. Do not overwrite local work or automatically hand off a running task.
- Preserve VContainer ownership, locator-mediated entity boundaries, semantic action buffering, and current Animator sub-state-machine conventions. No replacement animation framework, generic watchdog service, broad controller rewrite, balance adjustment, or global cleanup is planned.
- Do not make a new animation clip mandatory just to rename crouch-to-stand presentation. Verify the existing spawn state/behavior/exit contract first.
- Normal validation excludes Play Mode. Gameplay reproduction remains a separate follow-up phase under project policy.

### Success Criteria

- Fresh spawn, death recovery, grace entry/exit, and interrupted fades always reach one defined terminal outcome; no abandoned task or operation-owned lock remains.
- Recovery still runs when PlayerController skips Character.Tick. Cleanup cannot release another operation's lock or protection.
- Failed grace interaction does not unlock/save a grace or award a successful result. Death initiates exactly one respawn despite duplicate/late callbacks.
- Old, duplicate, wrong-layer, and same-state previous-execution notifications cannot complete the new action; loss of an old exit cannot suppress the current completion.
- Scene admission and spawn intent belong to the same accepted operation. Failure leaves a recoverable scene, coherent pending spawn, and the reviewed last-good save state.
- All relevant local Edit Mode tests pass after safe preflight. Serialized changes, if required, are fully imported and saved with no import/serialization errors. Gameplay gaps remain explicit until the separate phase runs.

## Execution Plan

### Phase 0 — Establish the local baseline

- [ ] Inspect local Git state and record the exact source commit containing the reviewed work. Compare it with this audit and refresh stale line references.
- [ ] Use official `unity --version` and `unity status --json`; bind all subsequent Editor operations with the exact verified `--project-path`. Inspect `unity command --json` for current registered schemas before issuing unfamiliar commands. Do not guess test/filter/cancellation parameter names.
- [ ] Read required context keys `animation-code`, `ui-code`, `entity-locator`, `plan-workflow`, `issue-workflow`, and relevant `character-architecture`; use registered headings. Source uses `long` entity IDs despite stale `ulong` prose.
- [ ] Run discovered `assert_test_ready` or `list_open_scenes` before any test. Dirty scenes block testing; dirty Untitled means `BLOCKED_DIRTY_UNTITLED_SCENE`. Do not save or replace a scene to bypass this gate.

Verify: command evidence identifies the intended checkout, compilation state, and clean scenes. No unrelated Editor was operated on.

### Phase 1 — Capture failures and agree on outcomes

- [ ] Trace one actual affected spawn setup: character prefab, animation profile/controller, hand mode, startup route (fresh/resume/grace/respawn), Animator enabled state, requested transition, entered state, and received completion. Record only evidence needed to distinguish the cause; avoid general per-frame log spam.
- [ ] Build minimal Edit Mode fixtures for missing-start and missing-completion lifecycle outcomes. Inspect current tests first; preserve `GraceAnimationTests` and `CharacterActionStateMachineTests` as regression baselines.
- [ ] Review the per-operation failure policy: spawn releases only spawn-owned protection after domain readiness or reports recoverable startup failure; death progresses to respawn exactly once; grace failure cancels without committing a checkpoint; interrupted fade returns cancellation.
- [ ] Review the recovery deadline/time source per operation. It must progress while gameplay input is blocked; avoid coupling it to Character.Tick. Define pause behavior explicitly instead of choosing an arbitrary universal timeout.

Verify: each source-backed issue has a deterministic failing fixture or a clearly documented fixture limitation; distinguish Edit Mode simulation from actual gameplay reproduction. The user's individual incidents remain unconfirmed until matching evidence exists.

### Phase 2 — Repair character lifecycle ownership

- [ ] Assign one `csharp_worker` the bounded spawn/death change in `Character`, `PlayerController`, and necessary orchestrator/Animator integration; use `$soulslike-csharp-change`, `$soulslike-animation-workflow`, and exact context keys above. Use `unity_architect` only to resolve the lifetime/ownership ambiguity before writing.
- [ ] Introduce the smallest operation-owned success/failure/cancellation contract, including start acknowledgment and missing-completion recovery outside blocked gameplay ticks. Ensure disposal invalidates outstanding work and late events cannot initiate another respawn.
- [ ] Extend the proven contract to grace waits, preserving current cancellation/finally cleanup and checkpoint-on-success semantics. Avoid blanket `SetInputBlocked(false)` cleanup that releases unrelated operations.
- [ ] Inspect controller transitions, behavior bindings, and short state hashes through Unity. Assign a separate `unity_operator` only if actual serialized defects require change; no overlapping writer on these assets.

Verify: missing Spawn entry/exit; disabled Animator; missing Death exit; grace entry/exit interruption; normal fresh spawn and direct grace-idle initialization; both hand modes. Domain outcomes must be asserted, not just current Animator state.

### Phase 3 — Correlate animation events

- [ ] Own edits in `AnimatorStateMachine`, `AnimatorStateMachineReceiver`, `AnimatorStateMachineDto`, `AnimatorComponent`, and `CharacterActionStateMachine` as one bounded scope after Phase 2 settles.
- [ ] Capture originating action/state-entry identity before completion delivery. Define invalidation on controller replacement and the owning layer/state. Preserve existing contradictory-category filtering and normal queue/buffer behavior.
- [ ] Replace or narrow `_pendingExitsToIgnore` only when the new correlation contract proves the same-state chain cases. Do not merely stamp arriving events with the newest generation.

Verify: A→B same-category chain with missing A exit; duplicate A exit; B exit after controller replacement; wrong-layer/category events; repeated identical state; expected chained rolls; queue windows and existing buffer tests.

### Phase 4 — Own fade outcomes and scene/spawn transitions

- [ ] Assign the respawn operation identity and CancellationTokenSource to the CoreGameOrchestrator entry point; implement its disposal hook so CoreScope teardown cancels and disposes outstanding work before it can publish more game-state changes. The current entry point has no IDisposable/CTS, while FadeService lives in SharedSceneScope. Pass that operation token/identity into the fade result API; do not make the shared fade service own the core gameplay lifetime.
- [ ] Settle each fade on success, replacement, destruction, and scope cancellation in `FadeUi`, `FadeService`, and respawn callers. Replacement/destruction must settle exactly the originating operation once; a late callback must not advance a later operation. Confirm installed DOTween Kill semantics with the local fixture; do not use Kill(true) to fake success.
- [ ] Admit scene loading and spawn intent together through the existing GameOrchestrator/SceneService/CharacterSpawnService boundary. Proposed policy: reject while busy before mutating pending spawn state. Resolve what can be validated before Loading in Single mode.
- [ ] Define a recoverable failure destination, per-operation cleanup, retry behavior, and the save commit point after character initialization. Audit GraceSystem resolution and CoreGameOrchestrator consumption against that contract.
- [ ] Integrate the existing cross-scene respawn issue. Review fresh direct non-Workshop entry versus normal MainMenu→Workshop explicitly. Resolve destination before publishing Ended side effects where required by the chosen contract.

Verify: fade replaced/destroyed; CoreScope disposed while a fade is pending; shared FadeUi reused by a new core scope; duplicate/late completion; two scene requests; missing target/dependency; initialization failure; retry after failure; matching and cross-scene checkpoint; fresh direct entry. Use disposable test data for persistence faults and preserve user saves.

### Phase 5 — Separate local gameplay validation

- [ ] Assign `unity_test_runner` a dedicated follow-up using `$soulslike-validation` and applicable domain skills. Normal validation must not silently enter Play Mode. Reconfirm applicable policy/authorization before any gameplay run; where automated Play Mode remains prohibited, use a separately arranged local reproduction and record that limitation.
- [ ] Validate the user's exact incident setup, then fresh/resumed/grace startup, weapon-profile changes, death and repeated respawn, cross-scene travel, and interrupted grace/fade flows. Record observed input lock, invulnerability, active action, scene identity, and terminal outcome.
- [ ] Record scenario, exact source/assets, elapsed time, and result. Close only scenarios actually performed. A green Edit Mode suite is not proof that the Animator behaved correctly during gameplay.

## Risks and Rollback

- An unconditional timeout/unlock can expose a half-initialized character, clear another action's lock, or grant grace success. Use operation-owned outcomes and test ownership conflicts.
- Generation assignment at event delivery cannot identify stale events; repeated state entry needs a real originating-execution contract.
- A mutex only inside SceneService leaves PrepareGraceSpawn outside admission. Include intent mutation and commit ordering in the boundary.
- Controller rewiring can affect root motion, layers, and transitions. Keep asset edits separate and reversible; use Unity APIs and explicit asset paths.
- Roll back only the bounded change and its owned assets; preserve local modifications and saved-game data. No broad reset, scene discard, or project-wide reserialization.

## Validation

Worktree evidence completed: source comparison, call traces, representative serialized reads, existing-test inspection, CLI identity/status. No Unity test or runtime claim was produced.

Local validation rules:

- Inspect clean scenes before each run. Start tests with `async_tests=true` using the discovered schema and poll `test_status` to completion.
- Initial budget: 120 seconds per focused Edit Mode fixture group, with status checks at most 30 seconds apart. Revise only from measured baseline evidence. On budget expiry, inspect test status, Editor state/modal condition, Console and Editor.log; use a discovered safe cancellation operation if needed. Do not retry blindly or proceed while a previous run remains active.
- After implementation, run `unity_reviewer` and `unity_test_runner` in parallel for independent review and safe validation. Every handoff names its required skill and exact file/symbol scope.
- If assets change, refresh/import and save through Unity; use explicit-path reserialization where required and verify no serialization/import errors. No manual Ctrl+S step is an acceptable completion condition.
- Required evidence: source commit, test filter, result counts, duration, Console/import result, asset persistence result, and named deferred gameplay scenarios.

## Execution Handoff

Start with Phases 0–2; Phases 3–4 are later bounded assignments, not concurrent overlapping rewrites. Phase 5 is a separate validation assignment. Parent owns synthesis and the final decision. Relevant files are enumerated in the linked issues; existing tests are `Assets/Scripts/Tests/CharacterRuntime/CharacterActionStateMachineTests.cs` and `Assets/Scripts/Editor/Tests/Animation/GraceAnimationTests.cs`.

Deferred work outside this plan: [[Work/Issues/Locked Roll State Is Cleared Before Root Motion]], the inventory category/comparison/compatibility issues, [[Work/Issues/Stance Recovery Is Gated By Poise Delay]], [[Work/Issues/Addressable Asset Loads Have No Release Owner]], both save/settings persistence issues, and [[Work/Issues/Architecture And Roll Issue Notes Contain Superseded Evidence]]. They remain in [[Work/Work Queue]] and the current audit comparison. Each needs its own reviewed bounded assignment or draft plan, applicable domain skills, and acceptance/validation decisions. The scene/spawn work must coordinate its save commit point with the persistence contract without expanding this plan into a storage rewrite.

Required review decisions: failure outcomes and timing/pause policy; scene admission policy; recovery destination and persistence commit point; stance intent only when its slice starts. Proposed defaults are above so the review is concrete. Mark ready only after these contracts are reviewed and the local validation prerequisites are established; do not execute this draft merely because the historical audit recommended fixes.
