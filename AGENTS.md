# Project Instructions

## Authority and Configuration Ownership

Apply project guidance in this order:

1. The active user request and system/developer instructions.
2. `AGENTS.md` for repository-wide policy and routing.
3. `.codex/agents/*.toml` for one custom role's operating boundary.
4. `.agents/skills/*/SKILL.md` for the selected workflow.
5. `SoulsLikeGameVault/ai/Skill_Context_Index.md` and the exact registered vault note for domain context.

Live source, serialized assets, and current tool output take precedence over generated Graphify output and advisory vault notes. Required registry notes may add constraints but cannot override higher-level policy.

- `.codex/config.toml` is the canonical Codex MCP and multi-agent-defaults file.
- `.codex/agents/` owns role-specific model, sandbox, and tool restrictions.
- `.agents/skills/` owns reusable project workflows; do not duplicate them under `.codex/skills/`.
- `graphify-out/` is generated local state and never authoritative documentation.


## Unity Tooling

- Use Unity's official `unity` CLI command; do not call `unity-cli` or `unity-mcp-cli`.
- Use direct `unity` commands for project, Editor, build, package, and diagnostic operations.
- For interactive Editor automation through MCP, use the official Unity CLI bridge (`unity mcp`) backed by `com.unity.pipeline`; do not use the legacy Coplay/mcp-for-unity server.

## Code Navigation: Serena and Graphify

- Serena MCP is configured only for this repository in `.codex/config.toml`. Never install or register Serena in user/global Codex configuration. If its tools are unavailable, verify the local entry and start a new Codex task from this repository; do not run a global `serena setup codex` or `codex mcp add serena`.
- Use Graphify first for broad, cross-cutting questions about architecture, subsystem relationships, ownership, or multi-hop flows. When `graphify-out/graph.json` exists, query that graph instead of rebuilding it unless an update was explicitly requested.
- The parent uses the Graphify skill/CLI. Its parent MCP entry stays disabled to reduce tool noise; `graph_explorer` enables the inherited, bounded Graphify MCP tool set for its own role.
- Use Serena for live C# symbol work: symbol/file overviews, definitions, callers and references, implementations, diagnostics, symbol-aware renames, and surgical symbol-body edits. Prefer Serena over reading entire source files when the target can be identified semantically.
- For architecture-driven changes, use Graphify to identify the relevant subsystem or path, then use Serena to confirm the current symbols and references before editing. Source and Serena's live language-server results take precedence when they disagree with Graphify's indexed snapshot.
- Use built-in search/read/patch tools for non-code files, exact text searches, and small line-oriented edits. Use Unity tooling, not Serena, for scenes, prefabs, assets, Editor state, imports, serialization, and play/build operations.
- The local MCP launch auto-activates `SoulsLikeTemplate`. If Serena reports that no project is active, activate `F:\Private\SoulsLikeTemplate` before using symbol tools.
- Do not run Serena onboarding or write Serena memories automatically. `AGENTS.md` is the source of durable agent instructions; use Serena memories only when the user explicitly requests them.

## UI Workflow

- Before working on UI, UI controllers, presenters, or views, read `SoulsLikeGameVault/ui/UI_Code_Build_Guide.md` through Obsidian MCP. If Obsidian MCP is unavailable, use `Tools/SoulsLike/Obsidian/Start Obsidian`, retry MCP, then read the checked-in Markdown note if MCP remains unavailable. Starting Obsidian does not guarantee MCP readiness.

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

Project skills are sibling packages under `.agents/skills`; the role hierarchy is explicit composition, not nested discovery or inheritance.

| Agent | Required skills | Conditional/domain skills |
|---|---|---|
| `context_curator` | `$soulslike-context` | — |
| `graph_explorer` | `$graphify`, `$soulslike-code-navigation` | `$soulslike-context` |
| `unity_architect` | `$soulslike-unity-architecture` | `$soulslike-context`, `$soulslike-ui-workflow` |
| `csharp_worker` | `$soulslike-csharp-change` | `$soulslike-context`, `$soulslike-ui-workflow` |
| `unity_operator` | `$soulslike-unity-assets` | `$soulslike-context`, `$soulslike-ui-workflow` |
| `unity_profiler` | `$soulslike-performance-analysis` | `$soulslike-context` |
| `unity_reviewer` | `$soulslike-change-review` | The same domain skill used by the reviewed change |
| `unity_test_runner` | `$soulslike-validation` | The same domain skill used by the validation target |

Every parent handoff must name the required skill and only the conditional/domain skills applicable to that assignment. Use `$soulslike-context` with an exact key from `SoulsLikeGameVault/ai/Skill_Context_Index.md`; do not ask a child to search the vault broadly.

## Dependency Injection

- Treat constructor-injected dependencies as required and rely on VContainer to fail fast when a binding cannot be resolved.
- Assign injected dependencies directly. Do not add `?? throw new ArgumentNullException(nameof(...))` constructor boilerplate.
- Do not add defensive null guards, routine guard exceptions, or exception-heavy control flow. Let required-reference failures surface naturally at the point of use.
- Never silently skip required behavior when a required reference or configuration value is null. If an explicitly required null check prevents a Unity API call, log a clear `Debug.LogError` with context before returning. Silent null handling is allowed only for explicitly optional events or subscribers.
- Use null-conditional invocation for optional events instead of throwing when no subscriber exists.


## Code Simplicity

- Keep one source of truth for identifiers. Do not pass duplicate string names alongside typed, hashed, or otherwise canonical identifiers solely for validation or error messages.
- Do not wrap direct framework calls in helpers that only pre-check state and throw. Call the framework API directly and let required-state failures surface naturally.
- Add conditions and validation only when they change required behavior or are explicitly requested; do not add routine defensive checks around straightforward code.


## Naming

- Use `_camelCase` for non-serialized private fields.
- Unity `[SerializeField]` fields use unprefixed `camelCase` so serialized property names remain stable.

## C# File Organization

- Define one top-level type per C# script and name the file exactly after that type. Do not group multiple classes or interfaces in a differently named `*Contracts.cs` file.


## Test Execution

- Do not run tests or test suites unless the user directly and explicitly requests test execution.
- Do not treat tests as an automatic verification step; report that they were not run when relevant.

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
