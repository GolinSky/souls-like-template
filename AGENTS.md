# Project Instructions


## Unity Tooling

- Use Unity's official `unity` CLI command; do not call `unity-cli` or `unity-mcp-cli`.
- Use direct `unity` commands for project, Editor, build, package, and diagnostic operations.
- For interactive Editor automation through MCP, use the official Unity CLI bridge (`unity mcp`) backed by `com.unity.pipeline`; do not use the legacy Coplay/mcp-for-unity server.

## Dependency Injection

- Treat constructor-injected dependencies as required and rely on VContainer to fail fast when a binding cannot be resolved.
- Assign injected dependencies directly. Do not add `?? throw new ArgumentNullException(nameof(...))` constructor boilerplate.
- Do not add defensive null guards, routine guard exceptions, or exception-heavy control flow. Let required-reference failures surface naturally at the point of use.
- Use null-conditional invocation for optional events instead of throwing when no subscriber exists.


## Code Simplicity

- Keep one source of truth for identifiers. Do not pass duplicate string names alongside typed, hashed, or otherwise canonical identifiers solely for validation or error messages.
- Do not wrap direct framework calls in helpers that only pre-check state and throw. Call the framework API directly and let required-state failures surface naturally.
- Add conditions and validation only when they change required behavior or are explicitly requested; do not add routine defensive checks around straightforward code.


## Naming

- Use `_camelCase` for non-serialized private fields.
- Unity `[SerializeField]` fields use unprefixed `camelCase` so serialized property names remain stable.


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

   `unity-cli editor refresh`

2. Re-serialize the specific changed asset:

   `unity-cli reserialize <asset-path>`

Example:

`unity-cli reserialize Assets/Prefabs/Character.prefab`

Do NOT use project-wide `unity-cli reserialize` unless explicitly necessary.

3. Check the Unity console for serialization/import errors.

The task is NOT complete merely because the YAML file was written to disk.

### Unity API asset mutations

When modifying assets using `unity-cli exec`, save changes inside the same
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
