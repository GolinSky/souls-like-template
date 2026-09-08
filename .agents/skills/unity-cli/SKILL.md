---
name: unity-cli
description: Use the official Unity CLI and this project's existing Pipeline bridge to inspect or operate the connected Unity Editor. Use for command discovery, project-scoped Editor operations, or custom Pipeline command authoring; do not use for package resolution or an alternate MCP server.
---

# Unity CLI

Use the official `unity` executable only. This project uses its existing
`com.unity.pipeline` bridge through `unity command` and `unity mcp`; do not
install, configure, or substitute another Unity MCP server or a legacy
`unity-cli` / `unity-mcp-cli` executable.

## Discover before operating

From the verified project root, check the CLI and target Editor first:

```powershell
unity --version
unity status --json
unity command --json
```

When more than one Editor might be open, pass the exact project path shown by
`unity status` with `--project-path`. Command names and parameter schemas are
defined by the connected Editor, so inspect `unity command` or `unity list`
before invoking an unfamiliar command. Use `--json` for parsed output.

If the Editor is open but not reachable, inspect `unity pipeline list --json`
for Safe Mode and investigate the reported compilation issue. Do not infer that
an unreachable Editor permits blind serialized-asset edits.

## Scene and asset safety

Use the live Editor for scene, prefab, and serialized-asset mutations whenever
it is reachable. Before a test, scene transition, reload, or other operation
whose safety depends on scene state, follow `AGENTS.md`: run the available
`assert_test_ready` or `list_open_scenes` command, inspect every loaded scene,
and stop for any dirty scene. Never save, discard, close, or reload a scene to
escape an unresolved dirty-scene condition.

Do not call broad `save_all`. Persist only the owned scene or asset using the
project's applicable workflow. Discover a command before use; examples in
external documentation are not proof that this Editor exposes the command.

## Pipeline command authoring

For a project command, put one static `[CliCommand]` method in an Editor
assembly and keep `MainThreadRequired` for Unity/Editor state. Recompile and
poll the command's reported status, then rediscover the command before use.
The installed Pipeline package and command schema are authoritative.

## Boundaries

Do not alter global CLI settings, credentials, licenses, proxy settings, MCP
configuration, or Editor installations unless the active request explicitly
requires it. Do not run upstream installation scripts or command text assembled
from untrusted source material.

This is a project adaptation informed by Unity Technologies' `unity-cli` skill
at the pinned source recorded in
[`third-party-notices/probuilder-sources.md`](../../third-party-notices/probuilder-sources.md).
