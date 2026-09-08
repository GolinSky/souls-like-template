---
title: ProBuilder Integration and Agent Skills
type: plan
domains:
  - unity-editor
  - level-prototyping
  - agent-skills
status: in-progress
authority: advisory
updated: 2026-09-08
aliases:
  - ProBuilder AI Level Prototyping Plan
tags:
  - work/plan
  - status/in-progress
---

# ProBuilder Integration and Agent Skills

## Local Execution — 2026-09-08

The user explicitly approved execution from the dedicated worktree and requested no tests. This adopts the Drive handoff as the canonical local plan with status `in-progress`; the historical draft wording below describes its original research state. Edit Mode, Play Mode, smoke tests, and behavioral acceptance experiments are not authorized for this run. Package resolution, compilation, source review, asset import/persistence checks, and creating the requested fixture remain implementation work.

- Starting commit: `c1e2385f`; initially clean detached worktree.
- Integration branch: `feature/pro-builder-integration`.
- Scope: `.agents/skills/`, attribution notices, narrow agent routing, `Packages/`, `Assets/Editor/Automation/ProBuilder/`, `Assets/Prototypes/ProBuilder/`, and canonical vault records.
- The existing running Editor belongs to the main checkout. All commands that mutate Unity must explicitly target this worktree's separate Editor.
- Initial worktree compilation failed because binary assets were Git LFS pointers (including DOTween). Hydrate tracked `Assets/**` before establishing the compile baseline. This is workspace preparation, not a dependency upgrade.
- Keep Unity `6000.3.11f1`, Pipeline `0.5.0-exp.1`, and HDRP `17.3.0`. ProBuilder `6.1.2` is available in Unity's package registry and requires Unity `6000.0` or newer; local resolution/compilation is still required.

Tests skipped by user request are validation gaps, not passing checks. Do not mark the original full acceptance gates complete without evidence.

## Plan Contract

### Goal

Integrate Unity ProBuilder into the existing SoulsLike project so Codex and Gemini-based agents can create and revise small, editable prototype levels through the **existing official Unity CLI and its MCP bridge**. Reuse maintained external skills where appropriate, adapt ProBuilder-specific guidance to the project's real command surface, and add only the small amount of Editor tooling necessary for reliable level generation.

**Recommended route:** official `unity` CLI / `unity mcp` + existing `com.unity.pipeline` + Unity ProBuilder + one project ProBuilder skill + a small, reusable Editor builder. **Do not introduce a second MCP server.**

This document is a researched implementation plan, not an implementation report. No Unity packages, repository files, agent settings, scenes, or Editor state were changed while preparing it. Live Unity execution, the local working tree, and the locally installed CLI version were not inspected.

**Delivery:** Markdown in the requested Google Drive `BaseSettings` folder. When this plan is adopted into the repository, its canonical project location should be `SoulsLikeGameVault/Work/Plans/ProBuilder Integration and Agent Skills.md`, matching the existing plan template and vault naming rules. The Drive copy is a handoff/export, not another independently maintained project authority. [R1][R5][R6]

### Source Research and Decisions

#### Verified committed project baseline

The following was read from `GolinSky/souls-like-template` on its default `main` branch on **September 8, 2026**. These are committed-repository observations, not proof of the current local Editor state.

| Item | Observed evidence | Consequence |
|---|---|---|
| Unity Editor | `ProjectSettings/ProjectVersion.txt`: `6000.3.11f1` | Keep this Editor version; do not upgrade Unity as part of ProBuilder setup. [R2] |
| Existing automation package | `com.unity.pipeline`: `0.5.0-exp.1` | Extend the existing Pipeline command surface instead of installing another bridge. [R3] |
| ProBuilder | No direct `com.unity.probuilder` entry in the inspected manifest | Check the local lockfile and Package Manager before deciding whether installation is necessary. [R3] |
| Render-pipeline dependency | `com.unity.render-pipelines.high-definition`: `17.3.0` | Do not assume URP. Inspect the active Graphics/Quality pipeline settings and use compatible materials. [R3] |
| Navigation | `com.unity.ai.navigation`: `2.0.11` | Reuse the installed navigation system when needed; do not install another navigation package. [R3] |
| Other relevant packages | Cinemachine `3.1.3`, Input System `1.19.0`, VContainer Git dependency pinned to `1.19.0` | Reuse existing player, camera, input, bootstrap, and DI conventions. No gameplay-framework migration. [R3] |
| Tooling policy | `AGENTS.md` explicitly requires official `unity`, with `unity mcp` backed by Pipeline, and excludes legacy Coplay tooling | Do not use the similarly named `unity-cli` / `unity-mcp-cli` executables or substitute another MCP implementation. [R1] |
| Skill ownership | `.agents/skills/` is canonical; existing `soulslike-context`, `soulslike-unity-assets`, and validation workflows already exist | Compose with the existing skills. Do not duplicate them under `.codex/skills/`. [R1][R4] |
| Existing safety command | `Assets/Editor/Automation/AgentTestSafetyCommands.cs` registers `assert_test_ready` through `[CliCommand]` | Use this as a local extension example and preserve its behavior. [R7] |
| Test policy | Ordinary agent validation excludes Play Mode tests and requires dirty-scene preflight and asynchronous test polling | Split structural/Edit Mode validation from separately assigned gameplay validation. [R1] |

One important implementation detail: the current `AssertTestReady()` returns `TEST_READY` when clean and throws `BLOCKED_DIRTY_SCENE` when dirty. Untitled scenes are identified with `[UNTITLED]` in the message. The policy's `BLOCKED_DIRTY_UNTITLED_SCENE` is a classification the agent should report; do not assume the existing method already returns that exact error code. [R7]

**ProBuilder version candidate:** Unity's current Unity 6000.3 package page lists **ProBuilder 6.1.2** as released for that Editor series. Use this as the first candidate, not as a blanket instruction to upgrade an already working installation. Confirm the candidate through the local Package Manager and a compilation smoke test on `6000.3.11f1`. Older community skill pins such as `6.0.9` are not project requirements. [S8]

#### Existing skills to reuse

These are actual source repositories/files, not marketplace descriptions. Their contents or upstream documentation were inspected. No external skill was installed into the project during this planning task.

| Source | What to take | How to use it here | License shown by upstream |
|---|---|---|---|
| **Unity-Technologies/skills — `unity-cli`** [S1] | The selected skill folder and the references it needs, especially `references/integration-advanced.md` | Preferred external foundation for official CLI discovery and Pipeline command authoring. First check for an existing installation to avoid duplicate skill names. | Unity Companion License for Unity-dependent projects. [S3] |
| **Unity-Technologies/skills — `unity-package-management`** [S2] | The package discovery/installation skill and relevant reference files | Preferred package-management reference. Adapt its headless examples to the already-running Editor; never execute a headless script's `EditorApplication.Exit(...)` in the user's interactive Editor. | Same Unity Companion License. [S3] |
| **AlexeyPerov/Unity-Open-MCP — `skills/extensions/probuilder/SKILL.md`** [S4] | Its inspect-before-edit workflow, semantic face-selection guidance, and focused shape-editing recipes | **Primary ProBuilder workflow reference**, rewritten against this project's Pipeline commands. Do not install Unity Open MCP or retain its activation/tool-prefix requirements as executable instructions. | MIT in the repository. [S5] |
| **batihandev/unity-mcp-skills — `skills/probuilder/SKILL.md`** [S6] | Bounded batch-building guidance, explicit geometry dimensions/pivots, and separation of whole-object versus face material edits | Supplement the local skill. Inspect any recipe separately before porting; its advertised `Unity_RunCommand` / `IRunCommand` execution contract is not this project's Pipeline contract. | MIT in the repository. [S7] |
| **IvanMurzak/Unity-AI-ProBuilder** [S12] | Optional reference for how a dedicated integration organizes mesh operations | Not a required dependency and not a drop-in extension to `unity mcp`. Do not install it in this plan. | Apache-2.0 in the repository. |

**Selection:** reuse the two Unity-authored skills when they are not already available, and create **one** project-domain skill, `soulslike-probuilder`, adapted from the two ProBuilder guides. Do not install an entire skill marketplace, dozens of unrelated Unity skills, or a second tool server merely to obtain their documentation.

The Open MCP guide is not directly portable: it names its own tool prefix and activation mechanism. The batihandev library requires the Unity AI Assistant MCP contract, even though its README calls that “official Unity MCP.” That is different from the **official Unity CLI Pipeline bridge** used here. Treat execution compatibility as something to verify, not something implied by a repository's title. [S4][S7]

#### Required adaptations, not optional cleanup

| Upstream assumption | Required project adaptation |
|---|---|
| `unity_open_mcp_*` tools and `probuilder` group activation | Remove these from the active workflow. Bind to commands actually discovered through the existing Pipeline bridge. |
| `Unity_RunCommand`, `IRunCommand`, or `Unity_PackageManager_ExecuteAction` | Do not invoke or implement these contracts just to use the documentation. Use supported existing commands or a local Pipeline `[CliCommand]` implementation. |
| Automatically install a hard-coded ProBuilder version | Resolve the local state, test the candidate version, and pin the verified version. |
| All geometry positions have a center pivot | Make the local builder's coordinate/pivot contract explicit, then verify bounds. Do not generalize a wrapper's convention to every ProBuilder API. |
| A wrapper already rebuilds the mesh | Inspect the actual called wrapper. In new C# code, own the required mesh-rebuild/refresh steps explicitly. |
| Instance IDs or face indexes are durable identifiers | Treat instance IDs as session-local handles; re-resolve targets after reload. Reinspect topology before using face indexes after a topology change. |
| Typical human dimensions are sufficient | Use this project's player, camera, combat, and enemy measurements. Record any provisional dimensions. |
| Playtest after every edit | Perform ordinary structural/Edit Mode checks first; defer Play Mode coverage under the repository's test policy. |
| Setup instructions can change agent/global configuration | Keep installation project-local; preserve unrelated MCP entries, global settings, credentials, and Obsidian configuration. |

### Assumptions and Non-Goals

This plan targets the SoulsLike repository identified above. Local source and tool output override the inspected remote snapshot. It is a prototype-authoring workflow, not a runtime procedural-world system.

Out of scope: final environment art, automatic “good level design,” terrain/world streaming, new AI navigation frameworks, Boolean-heavy modeling, lightmap optimization, universal mesh-editing APIs, new gameplay systems, automatic publishing, and Editor/CLI/Pipeline upgrades unrelated to a demonstrated blocker.

A door, ladder, elevator, spawn point, or shortcut represented by geometry is **not** an implementation of its gameplay behavior. Reuse an existing project prefab/configuration when suitable; otherwise label a placeholder. Do not independently instantiate another player/camera/service stack without checking the project's bootstrap conventions.

Do not change `.obsidian`, vault credentials, ports, plugins, or MCP authentication. This integration does not require Obsidian to be open. [R1]

### Success Criteria

**Core integration is complete only when:**

- The selected ProBuilder package resolves and compiles without new errors while existing Pipeline automation still works.
- Exactly one active project ProBuilder skill is discoverable, its sources and changes are recorded, and its instructions reference only available or clearly planned commands.
- The agent can create, inspect, save, and safely reload a small editable ProBuilder fixture through the existing bridge.
- Repeating a build does not duplicate content, damage unrelated objects, or overwrite manual edits silently.
- Materials, colliders, ownership, and scene/asset persistence pass structural validation.
- Relevant Edit Mode tests finish with recorded results; skipped or blocked checks are explicit.
- Gameplay validation is clearly marked **deferred/not run** until its separate phase is actually executed.

“Generation succeeded,” “structural validation passed,” “gameplay verified,” and “design approved” must remain separate outcomes.

## Execution Plan

### Phase 0 — Confirm the local baseline and safe working boundary

**Owner:** parent agent, with a narrowly scoped read-only context/architecture handoff when useful.

- [ ] Read `AGENTS.md` and resolve `plan-workflow`, `vault-usage`, and applicable registry entries through `soulslike-context`. For relevant implementation details, inspect `layer-architecture`, `character-architecture`, and `locomotion-current` selectively rather than loading the whole vault. Add `entity-locator` only when interaction/targeting behavior enters scope. [R1][R4]
- [ ] Record local branch/commit and `git status --short`. Never discard, auto-stash, or overwrite unrelated uncommitted work. Use an isolated integration branch according to the current workflow; do not push it without instruction.
- [ ] Read the local Editor version, package manifest/lockfile, existing Editor tooling, and current skill installations. Inspect only relevant Unity configuration entries; do not export credentials into reports.
- [ ] Confirm the installed **official** CLI path/version, selected project, reachable Editor, actual Pipeline version, compile state, current Console baseline, open scenes, and active render pipeline.
- [ ] Discover available commands and parameter schemas before invoking unfamiliar commands. Record only the commands needed for this task, not a giant catalog in always-loaded instructions.
- [ ] Locate actual player collider/controller settings, step/slope handling, camera collision/offset settings, collision layers, and representative enemy navigation parameters. Record source asset paths and measurements.
- [ ] Agree the task's owned scene/asset paths in the plan record. Prefer a new dedicated prototype scene, not the production gameplay scene.

Useful **existing CLI discovery examples**; run from the verified project root and confirm flags with the installed binary:

```powershell
Get-Command unity
unity --version
unity --help
unity status --json
unity command --json
unity command assert_test_ready --json
```

Use `unity command list_open_scenes --json` when that command is present. Always inspect all loaded scenes before any test or scene transition. Dirty unrelated scenes are a blocker, not permission to save or discard them. A dirty untitled scene requires a blocked report; do not switch scenes to escape it. [R1][R7]

**Exit gate:** the target Editor/project and write scope are unambiguous; source and current errors are recorded; no dirty-scene hazard is unresolved. If live tools are unavailable, finish the read-only audit but do not claim integration has run.

### Phase 1 — Acquire, review, and register selected external skills

**Owner:** parent or one assigned documentation writer. No Unity scene mutation in this phase.

- [ ] Check whether `unity-cli` or `unity-package-management` already exists in repository/user discovery locations. Reuse an approved copy instead of creating duplicate names. Do not overwrite or modify the user's global copy.
- [ ] Fetch each selected source into a temporary review location **outside** active skill-discovery directories and outside Unity's `Assets` folder. Fetch by repository URL, then record and pin the exact reviewed commit. Do not use a floating branch as an enduring installation pin.
- [ ] Read selected `SKILL.md` files, their needed references/scripts, and license notices before activation. Reject unexpected installers, global configuration edits, auto-updaters, external uploads, secret handling, or instructions that conflict with repository policy.
- [ ] Prefer a selective manual copy from the reviewed checkout. The Unity repository documents a skills installer, but installing everything is unnecessary; any installer used must be checked for version, supported flags, destinations, and side effects. [S1]
- [ ] Copy the selected Unity-authored folders only when needed, preserving their reference structure and license notices. Keep upstream code under its original license; do not relabel everything MIT. [S3]
- [ ] Adapt the two ProBuilder sources into `.agents/skills/soulslike-probuilder/`; do not install their original server-bound playbooks as active skills.
- [ ] Create a provenance record with source URL, repository, source path, reviewed commit SHA, source blob SHA where known, fetched date, license, locally changed sections, and a content hash of the adopted copy.
- [ ] Review the earlier conversation's `unity-probuilder-prototyping/SKILL.md` as a **local draft reference only**. It has not been installed or project-tested. Correct its generic Play Mode instructions and keep just one active ProBuilder entry point.

Recommended layout; new paths are proposed, not existing files:

```text
.agents/
  skills/
    unity-cli/                         # reuse existing approved copy when present
    unity-package-management/         # selected upstream skill, only if needed
    soulslike-probuilder/
      SKILL.md                        # compact project workflow
      SOURCES.md                      # provenance and adaptation notes
      references/
        setup.md                      # package + official CLI integration
        recipes.md                    # verified local shape/room recipes
        validation.md                 # Edit Mode and deferred gameplay checks
        command-contract.md           # actual implemented command schemas
      templates/
        level-spec.example.json
  third-party-notices/
    unity-skills-LICENSE.md
    unity-open-mcp-LICENSE
    unity-mcp-skills-LICENSE
```

Keep inactive upstream snapshots outside `.agents/skills/`, or rename them as clearly labeled reference text with no discoverable `SKILL.md` frontmatter. Do not make the agent choose between several contradictory ProBuilder skills.

**Exit gate:** source review and attribution are complete; selected files are known; no additional MCP server or unrelated skill bundle is installed.

### Phase 2 — Install and validate ProBuilder without disturbing the current bridge

**Owner:** `unity_operator`, using `soulslike-unity-assets` plus the reviewed setup guidance.

- [ ] Inspect the local resolved package list. When ProBuilder is already present and working, record it and avoid an unnecessary upgrade.
- [ ] When absent, query Package Manager for `com.unity.probuilder`; evaluate `6.1.2` against the actual Editor/project. Capture the resolved version and required dependency changes. [S8]
- [ ] Prefer an already exposed supported package command. Otherwise use the public `UnityEditor.PackageManager.Client` API via the existing Editor connection and a bounded asynchronous operation. Do not busy-wait on the Editor main thread. [S2]
- [ ] Do not copy the external headless installer's process-exit behavior into a live Editor. Do not launch a second Editor on the same working directory. A separate closed-project/isolated-checkout batch route is only a fallback, not the normal workflow.
- [ ] Reconnect after package import/domain reload as needed. Confirm completion from Package Manager state, manifest/lockfile, successful compilation, and re-discovered commands—not just from an install-request acknowledgment.
- [ ] Review the package diff. Keep the Editor, Pipeline, HDRP, navigation, input, and camera versions unchanged unless a specific conflict proves otherwise and that additional change is separately approved.
- [ ] Inspect the active render pipeline and assign existing compatible project materials explicitly. A package being installed does not prove its default preview material is compatible with the active pipeline.
- [ ] Create one editable ProBuilder cube in an owned smoke-test scene through supported Editor APIs; inspect mesh/component data, attach the intended collider, persist it, and verify no new import/serialization errors.

UPM changes should be recorded in `Packages/manifest.json` and `Packages/packages-lock.json` through the normal resolution workflow. Do not alter `Library/PackageCache` to patch packages in place.

**Exit gate:** ProBuilder compiles, its API is usable, the cube is persisted and editable, and existing `assert_test_ready`/Pipeline discovery still function.

### Phase 3 — Prove the execution adapter before building a generator

**Owner:** `csharp_worker` for Editor code; `unity_operator` for serialized scene changes. One overlapping writer at a time.

Use `Assets/Editor/Automation/AgentTestSafetyCommands.cs` as the existing local registration pattern. Add the smallest useful ProBuilder command class under the same tooling area, with separate files for separate top-level types. Inspect the installed Pipeline attribute/signature support before coding. Unity's upstream CLI reference documents project-defined `[CliCommand]` methods as the extension point. [R7][S9]

- [ ] Discover whether suitable ProBuilder commands already exist. Reuse them when their behavior, scope, and persistence are acceptable.
- [ ] Otherwise add a minimal Editor-only integration using the installed package's public API. Keep engine/scene operations on the main thread. Do not fork ProBuilder, use private reflection to patch it, or create a custom HTTP server.
- [ ] First support a read-only capability check and one scoped generation operation. Use a small `eval` call only as a smoke test if it is actually available; do not build a permanent workflow from huge shell-escaped C# snippets.
- [ ] Compile, rediscover the new command schema, and prove it through direct `unity command` execution.
- [ ] Prove the same local capability is available through the existing MCP connection. Record its **actual** exposed tool name; reconnect the agent client after discovery changes if necessary.
- [ ] Test a floor, segmented doorway, and one stair/ramp element. Inspect rendered bounds and collision opening independently.
- [ ] Save and safely reload the fixture after a clean-scene preflight; verify topology/materials/colliders survived.

**Do not build the full level specification framework until these operations work.**

**Exit gate:** a small editable fixture is reliably generated, inspected, persisted, and reloaded through the existing official integration.

### Phase 4 — Add a bounded, repeatable level builder

**Owner:** `csharp_worker`, followed by `unity_operator` for scene output.

Implement only the primitives and composition rules needed for the first prototype: floor/platform, wall segment, doorway assembled from segments, straight stairs, and ramp. Rooms and corridors should be compositions of those elements, not a second mesh-authoring framework.

Suggested files, adjusted to existing project conventions during implementation:

```text
Assets/Editor/Automation/ProBuilder/
  ProBuilderCommands.cs
  PrototypeLevelBuilder.cs
  PrototypeLevelSpec.cs
  PrototypeElementSpec.cs
  PrototypeLevelValidator.cs
  # Additional result/state types only when justified; one top-level type per file.

Assets/Prototypes/ProBuilder/
  Scenes/ProBuilder Smoke Test.unity
  Scenes/Combat Traversal Graybox.unity
  Specs/combat-traversal-graybox.json
  Materials/                           # only if suitable shared materials do not exist
  Metadata/                            # ownership/persistence state, not a gameplay service
```

Do not automatically add an assembly definition if the existing Editor assembly arrangement is adequate. When an asmdef is necessary, verify exact installed assembly names and references. Never attach an Editor-only `MonoBehaviour` to a persisted gameplay scene; keep authoring metadata in an appropriate Editor asset/sidecar or a verified existing identity mechanism.

#### Required builder contract

| Concern | Required behavior |
|---|---|
| Scope | A verified scene path, owned root, and output folder. Reject writes outside that scope. |
| Input | A versioned level specification with stable element IDs and explicit dimensions/transforms; no executable code embedded in input. |
| Layout | Explicit room/route connections and intended entry/exit; no arbitrary “random interesting level” generation in the MVP. |
| Coordinates | Document units, up/forward axes, shape origin/pivot, local/world transform convention, and whether dimensions are mesh size or transform scale. |
| Identity | Stable element IDs resolved through persisted ownership metadata; session instance IDs are not persistent identity. |
| Repetition | Same spec produces no duplicates or gratuitous GUID/reference churn. A second unchanged build should be a semantic no-op. |
| Manual changes | Compare the current owned content with the recorded generation state. On conflict, stop and report, preserve it as manually owned, or reconcile explicitly. Never silently overwrite. |
| Deletion | Delete only elements positively identified as generated and removed from the spec. An object name alone is not deletion authority. |
| Persistence | Save the intended scene and affected generated assets inside the successful operation; verify clean state. |
| Errors | Return actionable failure information and the changed/unchanged scope. Never report partial output as a complete build. |
| Failure recovery | Validate first; use an Undo group and track newly created objects/assets. Undo is not a substitute for disk rollback. Clean up only task-created resources whose ownership is proven. |
| Cost | Small bounded builds; cancel/check state between chunks when needed. Do not rebuild the entire level for one corridor edit. |

Boundary validation is needed because this tool accepts external level specifications and can delete assets. Keep it concentrated at that boundary; do not introduce defensive wrappers around every ordinary Unity/framework call or duplicate identifier systems.

#### Geometry and material rules

Modify `ProBuilderMesh` data, not only the compiled `MeshFilter.sharedMesh`. Own the necessary mesh rebuild/refresh sequence in the builder; the upstream API guide describes `ToMesh()` and `Refresh()`, but exact overloads must be verified against the installed version. [S10]

Prefer solid wall segments around openings over experimental Boolean subtraction. Preserve source mesh editability; do not merge the entire level, strip authoring data, or export over source geometry during the prototype stage. Experimental feature preferences are not a prerequisite for this plan. [S11]

Assign appropriate static colliders explicitly. A single enclosing box or convex hull can close a visually open doorway; validate actual collision geometry. For stairs, distinguish the visible step mesh from a smooth movement collider when the existing controller requires that arrangement. Do not globally enable convex collision or alter project collision layers.

Read material roles from the specification and resolve them to real compatible assets before a write. Avoid one new material per object. Preserve existing lighting settings and avoid a project-wide lightmap bake in the first pass.

#### Proposed CLI command surface

**These names are proposed project commands to implement, not commands claimed to exist today.** Adjust them if the local project already exposes equivalent commands.

| Proposed command | Purpose | Mutation policy |
|---|---|---|
| `pb_status` | Return package/API availability and the supported local operations | Read-only; should remain diagnostic where possible. |
| `pb_build` | Validate a spec and build/update the owned prototype; support a no-write preview mode | Writes only after the preview/scope checks pass. |
| `pb_inspect` | Resolve a stable element and report transform, bounds, mesh counts, materials, collider, and topology revision | Read-only. |
| `pb_validate` | Run structural/spec/persistence checks and report results | No scene mutation in normal validation; any navigation bake must be a separately declared write. |

Start with these four capabilities at most. Face extrusion, deletion, beveling, and subdivision are a later extension only when a real level-editing task needs them. For future topology edits, return a topology revision/hash, reject stale selections, and make directional selection explicitly local-space or world-space.

Every operation should return machine-readable status, project/scene identity, changed element IDs, changed assets, warnings, errors, validation results, and checks not run. Expected failure classifications may include `SPEC_INVALID`, `PACKAGE_UNAVAILABLE`, `TARGET_OUT_OF_SCOPE`, `MANUAL_EDIT_CONFLICT`, and the project's existing dirty-scene classifications. These are design requirements, not statements about current Pipeline errors.

**Exit gate:** same-spec rebuild is stable; a targeted spec change affects only the intended region; manual content and unrelated assets are preserved.

### Phase 5 — Finalize and test the project ProBuilder skill

**Owner:** one documentation writer, reviewed alongside the code.

- [ ] Keep `SKILL.md` compact: scope, trigger conditions, routing, safety boundaries, build loop, and required report. Put detailed schemas/recipes under `references/` and load them only when needed.
- [ ] Use a unique skill name (`soulslike-probuilder`) with matching folder name and valid YAML frontmatter. Add the missing frontmatter when adapting upstream text that does not have it.
- [ ] Remove source-server commands, speculative flags, stale version pins, raw install snippets, and unsupported model/tool-specific metadata from the adapted skill.
- [ ] Document actual command parameter names **after** the implementation is compiled and discovered. Proposed names in this plan are not evidence of registration.
- [ ] Add a narrow conditional reference in `AGENTS.md`: `unity_operator` uses the domain skill for ProBuilder/graybox tasks; reviewer/test-runner use the same skill when reviewing/validating that work. Preserve existing required role skills.
- [ ] Keep any Gemini entry-point instructions as routing to the canonical workflow, not another copy of the ProBuilder manual. Gemini CLI documents `.agents/skills/` workspace discovery, as does Codex; verify discovery in the user's actual client. Do not assume Antigravity shares every Gemini CLI discovery behavior. [S13][S14]
- [ ] Verify positive activation for “build a ProBuilder test arena” and negative activation for an unrelated C# bug fix. Check no duplicate ProBuilder skill is selected.
- [ ] After integration succeeds, add a short canonical workflow note in the vault and a narrowly scoped registry entry, if needed. Do not mark untested new guidance as verified policy before the tests pass.

#### Draft local SKILL.md body

This is an original project adaptation outline informed by [S4] and [S6], not a verbatim copy or a claim that the referenced commands already exist. Finalize its command reference only after Phase 3/4.

```markdown
---
name: soulslike-probuilder
description: Build or revise editable ProBuilder graybox rooms, corridors, stairs, ramps, arenas, and traversal test spaces in the SoulsLike project through its official Unity CLI/Pipeline bridge. Use only for prototype geometry and its validation, not unrelated gameplay code, final art, or runtime world generation.
---

# SoulsLike ProBuilder

## Before a write

Read AGENTS.md and compose with soulslike-unity-assets. Resolve the relevant
vault context. Confirm the actual project, Editor, package, pipeline, scene
state, command schema, output scope, and current changes. Do not add another
MCP server or change global/Obsidian settings.

Use the implemented operations documented in references/command-contract.md.
Never invoke upstream-server command names just because a reference lists them.

## Build loop

1. Resolve the brief to a small route/layout spec and measured project clearances.
2. Inspect existing content and preview changes before mutation.
3. Build or update only the owned elements; preserve manual edits and references.
4. Keep ProBuilder meshes editable; verify rendering and collision separately.
5. Save the intended scene/assets and confirm imported, persisted output.
6. Inspect the result and run the applicable structural/Edit Mode checks.

Use recipes.md for supported shape composition; use setup.md only for package
or tooling setup. Read validation.md before any tests or scene reload.

## Hard boundaries

Never save or discard unrelated dirty scenes, bypass a modal prompt, overwrite
unowned geometry, or perform overlapping Editor writes. Do not infer gameplay
success from screenshots or geometry creation. Normal validation does not run
Play Mode gameplay tests; report that coverage as deferred under project policy.

## Report

Return changed scene/spec/asset paths, changed regions, commands actually run,
checks passed/failed/not run, persistence status, screenshots actually captured,
manual-edit conflicts, and the remaining gameplay-validation handoff.
```

**Exit gate:** the skill is discoverable and routes correctly; an agent can follow it without inventing tools or bypassing existing project policy.

### Phase 6 — Build the first acceptance fixture and run normal validation

**Owner:** `unity_operator`, then `unity_reviewer` and `unity_test_runner` with non-overlapping responsibilities. Only one agent may drive mutable Editor state or tests at a time; a read-only code review may run independently.

Fixture: **Combat Traversal Graybox**. Keep it small enough to inspect in one overhead view. It should contain an arena, a narrow corridor, a segmented doorway, a straight stair to a platform, a ramp/return route, and a loop to the entrance. Use marked spawn/camera test positions. Resolve actual gameplay prefabs through the existing project workflow or leave clearly identified placeholders.

Dimensions must be explicit in the spec, but chosen from real controller/camera/enemy settings. Do not copy generic “human-scale” values as proven traversal requirements. Avoid combat-sensitive sizing based only on capsule width.

- [ ] Build the fixture and capture the structural report.
- [ ] Run the unchanged spec again; check object counts, identities, references, and diffs.
- [ ] Change one corridor width; prove that unrelated geometry remains unchanged.
- [ ] Make a controlled manual change to an owned test object; prove regeneration detects the conflict instead of overwriting it.
- [ ] Save, reload safely, and repeat inspection. Verify generated mesh/material/collider persistence, not just object names.
- [ ] Capture an overhead and player-height/camera-oriented view through supported tooling when available. Capturing an Edit Mode view does not require entering Play Mode; otherwise defer the screenshot instead of violating test policy.
- [ ] Run the validation matrix below. Keep a baseline so pre-existing errors are not incorrectly attributed to this integration.

**Exit gate:** core integration acceptance criteria pass, with gameplay/design coverage explicitly deferred.

### Phase 7 — Separate gameplay validation and review

This phase is intentionally separate from routine agent validation. Assign it through the project's permitted follow-up validation workflow; do not smuggle Play Mode testing into Phase 6 or mark this phase complete automatically. [R1]

Validate real controller movement, step/slope behavior, repeated rolls, camera obstruction, lock-on framing, doorway passage, and representative enemy navigation through the fixture. For navigation, first verify the existing NavMesh setup, bake only the intended prototype surface when authorized, and distinguish geometric path connectivity from successful movement by an actual enemy.

Ladders, elevators, jumps, and dynamic shortcuts must not be represented as ordinary walkable links unless the real gameplay/navigation integration supports that behavior. Human review should judge combat spacing and route readability; those are not proved by compile success.

**Exit gate:** attach the actual results or an explicit unresolved gap. Only completed, evidenced checks can change the fixture status to gameplay-verified.

## Risks and Rollback

| Risk | Prevention | Recovery |
|---|---|---|
| A reusable skill targets another MCP server | Source review and command remapping before activation | Disable/remove only the newly added conflicting skill; retain the original bridge. |
| New package causes compilation or dependency errors | Pin one tested package; inspect the dependency diff | Restore the pre-task manifest/lockfile changes for this integration and let Unity resolve again. Do not reset unrelated working-tree edits. |
| Package changes interrupt the connection | Track operation state and re-discover after reload | Reconnect to the same verified project; inspect completion before retrying. |
| An upstream installer closes the live Editor | Do not run headless `Exit` logic in the warm session | Prevent this through review; never respond by force-killing the user's Editor. |
| Dirty-scene/modal deadlock | Existing preflight and no dialog-producing APIs | Inspect Editor/test state; report a blocker. Never blindly repeat the timed-out command. |
| Generated geometry overwrites manual work | Persistent ownership plus conflict detection and a preview | Restore only task-owned changes from the saved pre-operation state/branch; preserve unrelated edits. |
| Undo leaves generated asset files behind | Track task-created assets and save boundaries | Remove only confirmed new owned artifacts; report anything not automatically recoverable. |
| Mesh/collider lost on reload | Explicit save and round-trip fixture test | Fix persistence before building more geometry. Do not accept “looks right until restart.” |
| Materials render incorrectly | Inspect active pipeline; reuse compatible materials | Replace only owned material assignments/assets, not project-wide rendering settings. |
| Playtests stall or are not permitted in normal validation | Separate gameplay phase and bounded test execution | Record coverage as not run; inspect status before any later run. |
| Skills drift after upstream updates | Pin reviewed commits and keep adaptation notes | Update via a reviewed diff, then rerun discovery and fixture tests; no automatic startup refresh. |

Rollback is scoped to this feature's changes. Never use a destructive repository-wide reset, broad asset reserialization, blanket `save_all`, or deletion of untracked work as a recovery shortcut.

## Validation

### Normal validation matrix

Use the repository's existing asynchronous test runner and supported parameter schemas. Before any tests, inspect open scenes / run `assert_test_ready`; all loaded scenes must be clean. If a supported command provides `async_tests`, use it and poll `test_status` with a deadline. On timeout, inspect the active run and Editor state; do not retry until no prior run remains active. [R1][R7]

| Test | Expected result | Evidence |
|---|---|---|
| CLI/bridge baseline | Correct project; existing Pipeline commands still reachable | Version, project identity, command discovery result |
| Package/compile | Chosen ProBuilder resolved; no new compilation errors | Manifest/lockfile diff, import/compile results |
| Skill discovery | One project ProBuilder entry point; no foreign-server calls | Actual client discovery and activation record |
| Editable mesh | Geometry retains a valid ProBuilder representation | Component and topology inspection |
| Dimensions/pivot | Bounds match the explicit spec contract | Measured bounds versus expected dimensions |
| Collision | Intended floors block; intended doorway stays open | Collider inspection and scoped geometric probes |
| Materials | Resolved assets compatible with the active pipeline | Material/shader inspection; screenshot when possible |
| Rebuild stability | Same input produces no duplicates/reference churn | Counts, stable identities, normalized diffs |
| Targeted change | Only the requested owned elements change | Before/after element and asset diff |
| Manual edit conflict | Detected and preserved, not silently overwritten | Conflict result plus unchanged manual content |
| Persistence | Meshes, materials, colliders, and references survive reload | Save/reload inspection |
| Error path | Invalid spec or out-of-scope target produces no unowned writes | Result/error record and scoped diff |
| Dirty-scene safety | Write/test blocked without saving/discarding user work | Controlled fixture result; no real unsaved work used as test data |
| Import/.meta integrity | All created Unity assets have valid imported metadata | AssetDatabase/import check and version-control diff |
| Editor boundary | No new Editor-only component serialized into gameplay content | Scene/prefab/component inspection |
| Regression tests | Relevant allowed Edit Mode tests complete | Test report, duration, final runner state |
| Gameplay checks | Explicitly deferred until Phase 7 | Coverage-gap record, not a fabricated pass |

For tests of dirty-scene handling or destructive failure paths, use a controlled temporary fixture/isolated checkout. Do not manufacture hazards in the user's real unsaved scene.

### Reporting and evidence

Keep one concise implementation record with links to the spec, scene, selected screenshots, and test output. Record source/package/CLI versions and source commits when available. Large logs/screenshots belong in the project's existing generated-artifact storage convention, not in always-loaded instruction files.

Record the result as one of: `PASS`, `FAIL`, `BLOCKED`, or `NOT_RUN` for each check. A lack of new Console errors is not sufficient proof that a level is navigable.

## Execution Handoff

### Files to inspect first

```text
AGENTS.md
GEMINI.md
ProjectSettings/ProjectVersion.txt
Packages/manifest.json
Packages/packages-lock.json
Assets/Editor/Automation/AgentTestSafetyCommands.cs
.agents/skills/soulslike-context/SKILL.md
.agents/skills/soulslike-unity-assets/SKILL.md
.agents/skills/soulslike-validation/SKILL.md
SoulsLikeGameVault/Agent Guide/Agent Context Registry.md
SoulsLikeGameVault/Templates/Plan Template.md
```

Use current exact context keys, and inspect relevant local player/camera/navigation/layer assets before dimension or gameplay decisions. No global scans of generated build/cache folders are necessary. Targeted inspection of the installed ProBuilder/Pipeline package API is allowed when needed; do not edit package-cache contents.

### Suggested bounded work units

| Work unit | Owner | Deliverable |
|---|---|---|
| Baseline and source review | Parent / context curator | Environment report and approved skill-source selection |
| ProBuilder dependency | Unity operator | Verified package change and smoke fixture |
| Editor adapter and builder | C# worker, then Unity operator | Small command surface, deterministic build, persistence |
| Skill adaptation and routing | One documentation writer | Canonical skill, references, attribution, routing diff |
| Structural review and tests | Reviewer + test runner | Recorded normal-validation results |
| Gameplay follow-up | Separately assigned test runner/operator | Real traversal/camera/combat results or explicit blockers |

Do not spawn a new agent role, change models, or alter the existing orchestration defaults solely for this feature. Preserve the current single-writer rule.

### Remaining decisions and stop conditions

The implementing agent can resolve package compatibility, actual command schemas, material selection, and source locations through inspection; these do not require another vague planning round. Stop only for a concrete unresolved boundary such as permission to save an unrelated dirty scene, a necessary extra dependency upgrade, ambiguous ownership of existing content, or an incompatible package/API combination.

This plan remains `draft` until reviewed. Move it to `ready` only when the target scope and installation choices are accepted. Execute only when the user requests execution, following the current repository's lifecycle policy. [R1][R5]

### Ready-to-use execution brief

> Implement the reviewed ProBuilder Integration and Agent Skills plan in this repository. Preserve the official `unity` CLI / `unity mcp` / Pipeline setup. First verify the local baseline and dirty-scene safety. Reuse the selected Unity-authored skills without duplicating existing installations, and adapt the two cited ProBuilder guides into one `soulslike-probuilder` skill. Do not install their MCP servers or Unity AI Assistant to make their examples work. Install only a verified compatible ProBuilder version, prove an editable saved/reloaded fixture, and then add the smallest repeatable Editor builder needed for the acceptance level. Preserve manual changes and unrelated assets. Run permitted structural/Edit Mode validation, report actual evidence, and defer gameplay tests under the project policy. Do not change global agent or Obsidian configuration. Do not push repository changes unless separately instructed.

## References and Provenance

Research date: **2026-09-08**. URLs below identify reviewed source locations; branch URLs can change. Record immutable reviewed commits during installation rather than treating this document's research date as a version pin.

### Project evidence

- **[R1] Repository policy:** [AGENTS.md](https://github.com/GolinSky/souls-like-template/blob/main/AGENTS.md). Observed content blob SHA: `5e48adb84bda0cb9d253cfb14266d82faf15aff1`.
- **[R2] Editor version:** [ProjectVersion.txt](https://github.com/GolinSky/souls-like-template/blob/main/ProjectSettings/ProjectVersion.txt). Blob SHA: `550b6e8a8cfda4d88fd49e3b42006ceb45fbbd68`.
- **[R3] Package dependencies:** [manifest.json](https://github.com/GolinSky/souls-like-template/blob/main/Packages/manifest.json). Blob SHA: `bc0d38cdcce7674c498725386bbbe191673c4649`. The local resolved lockfile was not inspected during this planning task.
- **[R4] Context and existing skills:** [Agent Context Registry](https://github.com/GolinSky/souls-like-template/blob/main/SoulsLikeGameVault/Agent%20Guide/Agent%20Context%20Registry.md), [soulslike-context](https://github.com/GolinSky/souls-like-template/blob/main/.agents/skills/soulslike-context/SKILL.md), [soulslike-unity-assets](https://github.com/GolinSky/souls-like-template/blob/main/.agents/skills/soulslike-unity-assets/SKILL.md), and [.agents/skills listing](https://github.com/GolinSky/souls-like-template/tree/main/.agents/skills).
- **[R5] Plan structure/lifecycle:** [Plan Template.md](https://github.com/GolinSky/souls-like-template/blob/main/SoulsLikeGameVault/Templates/Plan%20Template.md).
- **[R6] Vault naming/storage:** [Vault Guide.md](https://github.com/GolinSky/souls-like-template/blob/main/SoulsLikeGameVault/Agent%20Guide/Vault%20Guide.md).
- **[R7] Existing Pipeline extension:** [AgentTestSafetyCommands.cs](https://github.com/GolinSky/souls-like-template/blob/main/Assets/Editor/Automation/AgentTestSafetyCommands.cs). Blob SHA: `c84a7c667c35953d11fa45fb2ca4eaa5b86af39c`.

### External primary sources

- **[S1] Unity-authored CLI skill:** [Repository](https://github.com/Unity-Technologies/skills) and [skills/unity-cli/SKILL.md](https://github.com/Unity-Technologies/skills/blob/main/skills/unity-cli/SKILL.md).
- **[S2] Unity-authored package skill:** [skills/unity-package-management/SKILL.md](https://github.com/Unity-Technologies/skills/blob/main/skills/unity-package-management/SKILL.md). Reuse package discovery and async-resolution guidance; review process-exit behavior before adapting examples to a live Editor.
- **[S3] Unity skills license:** [LICENSE.md](https://github.com/Unity-Technologies/skills/blob/main/LICENSE.md). The repository identifies the Unity Companion License for Unity-dependent projects; preserve the actual notice and review its linked terms before redistribution.
- **[S4] Primary ProBuilder workflow reference:** [Unity-Open-MCP ProBuilder SKILL.md](https://github.com/AlexeyPerov/Unity-Open-MCP/blob/master/skills/extensions/probuilder/SKILL.md). Observed blob SHA: `4e767fe7d2ae1686f0dcaab47cb597c6e4470033`. This guide targets that project's server, not Unity CLI.
- **[S5] Unity Open MCP repository/license:** [Repository](https://github.com/AlexeyPerov/Unity-Open-MCP) and [LICENSE](https://github.com/AlexeyPerov/Unity-Open-MCP/blob/master/LICENSE).
- **[S6] Supplementary ProBuilder skill:** [batihandev ProBuilder SKILL.md](https://github.com/batihandev/unity-mcp-skills/blob/main/skills/probuilder/SKILL.md). Observed blob SHA: `889caeedf11935004993b8a1d9429714a9389a7f`. Linked C# recipes require separate review before use; this plan did not validate their execution.
- **[S7] Supplementary library requirements/license:** [Repository README](https://github.com/batihandev/unity-mcp-skills) and [LICENSE](https://github.com/batihandev/unity-mcp-skills/blob/main/LICENSE). The README identifies the Unity AI Assistant MCP dependency and its `Unity_RunCommand` / `IRunCommand` contract.
- **[S8] Official Editor-series compatibility:** [Unity 6000.3 ProBuilder package page](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.probuilder.html). Retrieved page lists `6.1.2` and shows a build date of 2026-08-09. This is a candidate-version source, not proof of a successful project install.
- **[S9] Official CLI/Pipeline extension reference:** [integration-advanced.md](https://github.com/Unity-Technologies/skills/blob/main/skills/unity-cli/references/integration-advanced.md). Runtime command discovery and the installed package remain authoritative.
- **[S10] ProBuilder API concepts:** [Unity ProBuilder scripting API guide](https://docs.unity.cn/Packages/com.unity.probuilder%406.0/manual/api.html). The retrievable page is an older `6.0.1-pre.2` reference, used only for representation/rebuild concepts—not exact 6.1.2 compatibility. Several 6.1 API pages could not be reliably retrieved during this research; verify installed API source/signatures before implementation.
- **[S11] Experimental feature warning:** [Unity ProBuilder preferences](https://docs.unity.cn/Packages/com.unity.probuilder%406.0/manual/preferences.html). Older reference; used as background for the conservative no-experimental-CSG default, not to prescribe current menu locations or preferences.
- **[S12] Optional alternative, not selected:** [IvanMurzak/Unity-AI-ProBuilder](https://github.com/IvanMurzak/Unity-AI-ProBuilder). Its README identifies its AI Game Developer platform dependency and Apache-2.0 license.
- **[S13] Codex skills:** [Build skills / local discovery](https://learn.chatgpt.com/docs/build-skills), reached from the official Codex skills documentation.
- **[S14] Gemini CLI skills:** [Agent Skills](https://geminicli.com/docs/cli/skills/), including workspace `.agents/skills/` discovery.

**Evidence boundary:** repository content and external documentation were read, but no local Unity smoke test, package install, source-skill installation, Editor command, benchmark, or gameplay test was performed. All unchecked execution items above remain work to be done by the implementing agent.
