# Project Instructions

## Authority and Configuration Ownership

Apply project guidance in this order:

1. The active user request and system/developer instructions.
2. `AGENTS.md` for repository-wide policy and routing.
3. `.codex/agents/*.toml` for one custom role's operating boundary.
4. `.agents/skills/*/SKILL.md` for the selected workflow.
5. `SoulsLikeGameVault/Agent Guide/Agent Context Registry.md` and the exact registered vault note for domain context.

Live source, serialized assets, and current tool output take precedence over generated Graphify output and advisory vault notes. Required registry notes may add constraints but cannot override higher-level policy.

- `.codex/config.toml` is the canonical Codex MCP and multi-agent-defaults file.
- `.codex/agents/` owns role-specific model, sandbox, and tool restrictions.
- `.agents/skills/` owns reusable project workflows; do not duplicate them under `.codex/skills/`.
- `graphify-out/` is generated local state and never authoritative documentation.

## Obsidian Configuration Protection

- Never edit, rotate, redact, regenerate, untrack, ignore, delete, or otherwise change Obsidian configuration, credentials, API keys, certificates, cryptographic material, ports, plugin state, or MCP authentication unless the user explicitly requests an Obsidian configuration change in the active request.
- Audits, reviews, security scans, documentation cleanup, MCP work, and general optimization requests do not grant permission to change Obsidian configuration. Report findings only and wait for an explicit user request.

## Vault Context Discovery

- Before a non-trivial implementation, investigation, review, or Unity asset task, use `$soulslike-context` to resolve relevant task signals against `SoulsLikeGameVault/Agent Guide/Agent Context Registry.md`.
- Load every directly matching `required` entry and only the `advisory` entries relevant to the assigned scope. Read registered headings rather than entire notes.
- Registered notes may be discovered by an exact context key or by the registry's task signals. Tags and frontmatter support discovery, but only a registry entry can make a note project policy.
- Live source, serialized assets, and current tool output override advisory or stale notes. Report conflicts and mark affected documentation for review; do not silently follow it.
- Persistent plans belong in `SoulsLikeGameVault/Work/Plans/`, issues in `SoulsLikeGameVault/Work/Issues/`, research handoffs in `SoulsLikeGameVault/Research/`, and completed work records in `SoulsLikeGameVault/History/Implementation Records/`. Use the matching note template.
- When Plan mode produces a project implementation plan, create or update its note in `Work/Plans/` using the plan template. Leave it `draft` until reviewed; execute it only when the user explicitly requests execution and its status is `ready` or `in-progress`.


## Unity Tooling

- Use Unity's official `unity` CLI command; do not call `unity-cli` or `unity-mcp-cli`.
- Use direct `unity` commands for project, Editor, build, package, and diagnostic operations.
- For interactive Editor automation through MCP, use the official Unity CLI bridge (`unity mcp`) backed by `com.unity.pipeline`; do not use the legacy Coplay/mcp-for-unity server.

## Code Navigation: Serena and Graphify

- Serena MCP is configured only for this repository in `.codex/config.toml`. Never install or register Serena in user/global Codex configuration. If its tools are unavailable, verify the local entry and start a new Codex task from this repository; do not run a global `serena setup codex` or `codex mcp add serena`.
- Use Graphify first for broad, cross-cutting questions about architecture, subsystem relationships, ownership, or multi-hop flows. When `graphify-out/graph.json` exists, query that graph instead of rebuilding it unless an update was explicitly requested.
- The parent uses the Graphify skill/CLI. Its parent MCP entry stays disabled to reduce tool noise; `graph_explorer` enables the inherited, bounded Graphify MCP tool set for its own role.
- Exclude `graphify-out/` from broad repository searches. Keep only the current graph/report/visualization, cost and manifest state, and the 20 most recent query-memory files; older generated snapshots are disposable and may be rebuilt.
- Use Serena for live C# symbol work: symbol/file overviews, definitions, callers and references, implementations, diagnostics, symbol-aware renames, and surgical symbol-body edits. Prefer Serena over reading entire source files when the target can be identified semantically.
- For architecture-driven changes, use Graphify to identify the relevant subsystem or path, then use Serena to confirm the current symbols and references before editing. Source and Serena's live language-server results take precedence when they disagree with Graphify's indexed snapshot.
- Use built-in search/read/patch tools for non-code files, exact text searches, and small line-oriented edits. Use Unity tooling, not Serena, for scenes, prefabs, assets, Editor state, imports, serialization, and play/build operations.
- The local MCP launch auto-activates `SoulsLikeTemplate`. If Serena reports that no project is active, activate `F:\Private\SoulsLikeTemplate` before using symbol tools.
- Do not run Serena onboarding or write Serena memories automatically. `AGENTS.md` is the source of durable agent instructions; use Serena memories only when the user explicitly requests them.

## UI Workflow

- Before any UI work—including Penpot design, mockups, UI art, generated UI assets, asset import, layout, UI rendering, or UI interaction—resolve the required `ui-style` and `ui-asset-layout` contexts. Read each note's `Project Application Rules` and `Task-Conditional Reading Map`, then load only the map-selected detailed headings through vault MCP (`soulslike-vault`) or the checked-in Markdown fallback. Do not start Obsidian or check REST endpoints.
- Before working on UI controllers, presenters, views, UI prefabs, or Addressables, also resolve the `ui-code` context and read `SoulsLikeGameVault/Guides/UI/UI Code Build Guide.md`. The architecture route remains required alongside the style and asset-layout rules.

## Animation Workflow

- Before modifying Animator Controllers, animation states, transitions, sub-state machines, or ActionExecutor animation code, resolve the `animation-code` context and read `SoulsLikeGameVault/Guides/Animation/Animator Sub-State Machine Guide.md` through vault MCP (`soulslike-vault`). If vault MCP is unavailable, read the checked-in Markdown note directly. Do not start Obsidian or check REST endpoints. Ensure all animations are grouped into sub-state machines, coordinate standards matching `CharacterGreatSwordAnimator.controller` are followed, action sub-state machines contain an inert `Empty` default state, and runtime CrossFade calls use short state names/hashes.

## Subagent orchestration

Keep the parent on GPT-5.6 Sol High. Use the named project agents proactively for non-trivial tasks.

1. Trivial or isolated change: parent works directly.
2. Investigation: run `graph_explorer` and, when useful, `context_curator` in parallel.
3. Architecture: parent creates the plan; use `unity_architect` only for high-risk or ambiguous design.
4. Implementation: assign exactly one writer—`csharp_worker` or `unity_operator`—for overlapping scope.
5. Validation: after implementation, run `unity_reviewer` and `unity_test_runner` in parallel.
6. Performance tasks: use `unity_profiler` before proposing optimization.
7. The parent must synthesize all results, resolve conflicts, and make the final decision.

Use 2–4 children only when work is genuinely independent. Give every child a narrow objective, exact files or symbols, constraints, and required output. Never run overlapping writers or spawn every agent by default.

### Project skill routing

For ProBuilder or graybox geometry work, compose `soulslike-probuilder` with
`unity_operator`'s required asset skill, `csharp_worker`'s required C# skill for
Editor builder changes, and the review/validation roles' existing required
skills when their assigned scope includes this integration. The canonical
workflow is `.agents/skills/soulslike-probuilder/SKILL.md`; use the existing
official Unity CLI/Pipeline bridge and do not install another MCP server.

Project skills are sibling packages under `.agents/skills`; the role hierarchy is explicit composition, not nested discovery or inheritance.

| Agent | Required skills | Conditional/domain skills |
|---|---|---|
| `context_curator` | `$soulslike-context` | — |
| `graph_explorer` | `$graphify`, `$soulslike-code-navigation` | `$soulslike-context` |
| `unity_architect` | `$soulslike-unity-architecture` | `$soulslike-context`, `$soulslike-ui-workflow`, `$soulslike-animation-workflow` |
| `csharp_worker` | `$soulslike-csharp-change` | `$soulslike-context`, `$soulslike-ui-workflow`, `$soulslike-animation-workflow` |
| `unity_operator` | `$soulslike-unity-assets` | `$soulslike-context`, `$soulslike-ui-workflow`, `$soulslike-animation-workflow` |
| `unity_profiler` | `$soulslike-performance-analysis` | `$soulslike-context` |
| `unity_reviewer` | `$soulslike-change-review` | The same domain skill used by the reviewed change |
| `unity_test_runner` | `$soulslike-validation` | The same domain skill used by the validation target |

Every parent handoff must name the required skill and only the conditional/domain skills applicable to that assignment. Use `$soulslike-context` with an exact key or explicit task signals from `SoulsLikeGameVault/Agent Guide/Agent Context Registry.md`; do not ask a child to search the vault broadly.

## Dependency Injection

- Treat constructor-injected dependencies as required and rely on VContainer to fail fast when a binding cannot be resolved.
- Assign injected dependencies directly. Do not add `?? throw new ArgumentNullException(nameof(...))` constructor boilerplate.
- Do not add defensive null guards, routine guard exceptions, or exception-heavy control flow. Let required-reference failures surface naturally at the point of use.
- Never silently skip required behavior when a required reference or configuration value is null. If an explicitly required null check prevents a Unity API call, log a clear `Debug.LogError` with context before returning. Silent null handling is allowed only for explicitly optional events or subscribers.
- Use null-conditional invocation for optional events instead of throwing when no subscriber exists.


## Code Simplicity

- `UnityEvent` is forbidden in project-authored gameplay and presentation code. Use typed interfaces or C# events; route cross-system behavior through the owning service. Views call presenter interfaces, and camera effects are requested by gameplay logic through the camera service.
- Keep one source of truth for identifiers. Do not pass duplicate string names alongside typed, hashed, or otherwise canonical identifiers solely for validation or error messages.
- Do not wrap direct framework calls in helpers that only pre-check state and throw. Call the framework API directly and let required-state failures surface naturally.
- Add conditions and validation only when they change required behavior or are explicitly requested; do not add routine defensive checks around straightforward code.


## Naming

- Use `_camelCase` for non-serialized private fields.
- Unity `[SerializeField]` fields use unprefixed `camelCase` so serialized property names remain stable.

## C# File Organization

- Define one top-level type per C# script and name the file exactly after that type. Do not group multiple classes or interfaces in a differently named `*Contracts.cs` file.


## Test Execution

- Use the Unity Test Framework (UTF), package `com.unity.test-framework`, for Unity C# behavior and regression tests whenever the behavior can be tested meaningfully. Reuse relevant existing tests; add focused coverage for changed behavior when needed. Documentation-only or other low-impact changes do not need artificial tests.
- Before authoring, selecting, running, or reviewing Unity tests, resolve `unity-testing` with `$soulslike-context` and read the registered headings in `SoulsLikeGameVault/Guides/Testing/Unity Test Framework Test Flow.md`.
- Run UTF through the official Unity CLI/Pipeline bridge. Discover the live schema, use `list_tests --mode editor` to confirm the intended fixture or assembly, and explicitly pass `--mode editor`, a bounded filter, `--async_tests true`, and a timeout to `run_tests`. Never rely on its defaults (`all` modes and synchronous execution). The current Pipeline async runner does not enforce `--timeout`; enforce a caller-side wall-clock budget and use `test_status` / `cancel_tests` for timeout recovery.
- Validate the nested test result, executed count, failures, and cancellation state. A successful CLI request, an empty selection, or compilation alone is not a passing test run. Report an unavailable Editor or blocked UTF run as a validation gap; static checks do not replace execution evidence.
- Execute relevant tests to verify changes, strictly adhering to the Unity Test Safety protocol below.
- Always run preflight checks (`assert_test_ready` / `list_open_scenes`) prior to starting any test run.
- Run tests asynchronously (`async_tests=true`) and poll `test_status` until completion to prevent blocking and modal deadlocks.
- Skip all Play Mode tests during normal agent validation. This includes gameplay tests that require a character to move, attack, use equipment, interact with objects, enemies, UI, triggers, or other scene content.
- Report skipped Play Mode coverage as a validation gap and move it into a separate follow-up validation phase assigned to `unity_test_runner` or another faster AI agent when that coverage is still required.
- Time-box all remaining test runs. If a test exceeds its expected time budget, stop waiting and do not blindly retry it. Record the exact test, elapsed time, and last known status, then either skip it with the validation gap reported or move it into the follow-up validation phase.
- Before moving on from a timed-out or deferred test, inspect `test_status` and the Editor state and confirm that no test run remains active.

## Unity Test Safety

Before calling any Unity test command:

1. Run: `unity command list_open_scenes --json` (or `unity command assert_test_ready --json`).
2. Inspect every open scene:
   - If all scenes have `isDirty=false`, testing may continue.
   - If a dirty scene has a non-empty asset path and the task explicitly permits scene saving, save it with `save_scene`, then inspect again.
   - If a dirty scene has an empty path / is Untitled, stop with `BLOCKED_DIRTY_UNTITLED_SCENE`.
   - Never start tests while any scene is dirty.
3. Never call these while scene state is unknown:
   - `run_tests`
   - `open_scene`
   - `save_all`
   - Enter Play Mode
   - Close or reload scenes
4. Never invoke dialog-producing APIs in agent automation:
   - `EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo`
   - `EditorSceneManager.EnsureUntitledSceneHasBeenSaved`
   - `EditorUtility.DisplayDialog` or `DisplayDialogComplex`
5. Start tests with `async_tests=true`. Poll `test_status` until completed, failed, cancelled, or timed out.
6. If a test command times out:
   - Do not immediately retry it.
   - Check Unity for a modal dialog.
   - Inspect `test_status`, Editor status, Console, and Editor.log.
   - Confirm that no previous test run remains active.
7. Never use `open_scene` to escape a dirty-scene condition. It may discard unsaved scene changes.

## Unity Asset Persistence

Unity assets MUST be left fully imported and saved after every agent mutation.
Never require the user to focus Unity, open an asset, press Ctrl+S, or manually
save the project.

### External Unity asset edits

If any serialized Unity asset is modified directly on disk, including:

- `.prefab`
- `.unity`
- `.asset`
- `.mat`
- `.controller`
- `.anim`
- `.overrideController`

the agent MUST synchronize the changed asset through Unity before completing
the task.

For each changed asset:

1. Run:

   `unity command eval --code 'UnityEditor.AssetDatabase.Refresh();'`

2. Re-serialize the specific changed asset:

   `unity command eval --code 'UnityEditor.AssetDatabase.ForceReserializeAssets(new[] { "<asset-path>" }); UnityEditor.AssetDatabase.SaveAssets();'`

Example:

`unity command eval --code 'UnityEditor.AssetDatabase.ForceReserializeAssets(new[] { "Assets/Prefabs/Character.prefab" }); UnityEditor.AssetDatabase.SaveAssets();'`

Do NOT call `ForceReserializeAssets()` without an explicit asset-path collection unless project-wide reserialization is explicitly necessary.

3. Check the Unity console for serialization/import errors.

The task is NOT complete merely because the YAML file was written to disk.

### Unity API asset mutations

When modifying assets using `unity command eval`, save changes inside the same
Unity operation.

For ScriptableObjects and normal asset objects:

- modify the object
- call `EditorUtility.SetDirty(asset)`
- call `AssetDatabase.SaveAssets()`

Prefer `SerializedObject` / `SerializedProperty` where appropriate.

### Prefab mutations

For structural prefab changes, prefer Unity APIs over direct YAML editing.

Use:

- `PrefabUtility.LoadPrefabContents(path)`
- modify the prefab contents
- `PrefabUtility.SaveAsPrefabAsset(root, path)`
- `PrefabUtility.UnloadPrefabContents(root)`
- `AssetDatabase.SaveAssets()`

Do not rely on the user opening or saving the prefab afterward.

### Scene mutations

When changing a scene through Unity APIs:

- mark the scene dirty if necessary
- save it with `EditorSceneManager.SaveScene(...)`

Do not leave scene changes only in Editor memory.

### Completion requirement

After any Unity asset mutation, verify that:

1. Unity has imported the change.
2. The asset has been persisted to disk.
3. Unity reports no import/serialization errors.
4. No manual Unity Editor interaction is required from the user.

A task that requires the user to focus Unity and press Save is incomplete.


## Unity CLI Argument Rules

When using `unity command`:

- ALWAYS use CLI parameters as `--parameter value`.
- NEVER use `parameter=value`.
- NEVER include the parameter name inside the parameter value.

Wrong:
`unity command get_animator_controller controller=Assets/Foo.controller`

Wrong:
`unity command get_animator_controller --controller controller=Assets/Foo.controller`

Correct:
`unity command get_animator_controller --controller "Assets/Foo.controller"`

For ObjectRef parameters, prefer explicit JSON whenever an asset is being referenced:

`unity command get_animator_controller --controller '{"path":"Assets/Foo.controller"}'`

For scene objects use:

`--target '{"hierarchyPath":"/Player/Visual"}'`

Before using an unfamiliar Unity Pipeline command, inspect the registered command schema with:

`unity command`

Do not guess parameter names or CLI syntax.

If a Unity Pipeline error contains a malformed resolved path such as:

`Assets/controller=Assets/...`

STOP and correct the CLI argument serialization. Do not search for another asset, rename the asset, reimport it, or modify the Unity project.

<!-- penpot-ai-kit:begin -->
# Penpot AI Kit — project operating rules
Penpot skills are installed as native, self-contained Codex skills in this project's .agents/skills directory.

Before ANY Penpot design work:
1. Read C:\Users\golin\.penpot-ai-kit/AGENTS.md and follow it.
2. Your FIRST Penpot tool call each session is `high_level_overview` (no arguments).
3. Let the request trigger the matching penpot-* skill; use penpot-router when it spans several skills.

The MCP configuration remains in the user's global Codex config so secrets never land in this project.
<!-- penpot-ai-kit:end -->
