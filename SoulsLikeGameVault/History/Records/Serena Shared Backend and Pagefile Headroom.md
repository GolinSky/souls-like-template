---
title: Serena Shared Backend and Pagefile Headroom
type: implementation-record
domains:
  - agent-tooling
status: done
authority: historical
updated: 2026-09-14
aliases: []
tags:
  - history/change
---

# Serena Shared Backend and Pagefile Headroom

## Implementation Record Contract

### Outcome

Applied the user-approved remediation from [[Research/Codex and Serena Memory Exhaustion 2026-09-14]]. The existing D: pagefile is configured for system-managed sizing. Serena now runs as one shared HTTP service for this checkout on loopback port 9121, with a dedicated patched Python environment and a per-user Windows sign-in launcher. A Windows restart is still required to apply the pagefile startup setting and refresh existing Codex task configuration.

### Why

Today's system commit exhaustion coincided with multiplied Serena/Roslyn startups. Nine simultaneous startups were traced to old subagent tasks, and a disabled role could still call Serena. The C# adapter also opened both a solution and recursively discovered project files. Sharing the backend bounds instance count even when role tool filtering is ineffective.

### Changed Files and Assets

- `.codex/config.toml`: replaced the Serena stdio command/environment with one HTTP URL.
- `.codex/agents/{context-curator,graph-explorer,unity-operator,unity-profiler,unity-test-runner}.toml`: explicit disabled HTTP Serena entries. C# worker, reviewer, and architect retain inherited access.
- `.codex/agents/README.md` and `AGENTS.md`: documented actual role inheritance, shared ownership, and the prohibition on switching the shared project's activation.
- `.serena/.gitignore`: exclude the dedicated runtime and service state.
- `Tools/Serena/`: setup/start/stop scripts, pinned package lock, guarded adapter patch, isolated tests, and lifecycle documentation.
- Windows: `D:\pagefile.sys 0 0`; current-user Startup shortcut `SoulsLikeTemplate Serena.lnk`.

No Unity assets, gameplay files, Obsidian configuration, or global Codex configuration were changed by this task. Unrelated working-tree edits were retained.

### Decisions and Tradeoffs

- Use one backend for this exact checkout. Another worktree requires its own endpoint and lifecycle configuration.
- Keep Serena 1.7.0 in a dedicated copied environment rather than modifying the shared uv cache. Package versions are locked, and the adapter patch refuses version or source-block drift.
- Open root `SoulsLikeTemplate.sln` once; use deterministic root solution selection or root C# project fallback. Root generated files remain usable despite Git ignore. Nested Library/obj projects are not discovered by this method.
- The root solution still triggers 199 Roslyn restores. The measured benefit is one shared initialization and no second project-open pass, not elimination of all restore work.
- Retired eight verified idle legacy stdio process trees. Each was identified by executable/creation time and a project-local log showing completed initialization and no current tool work. The shared service remained running.
- Startup runs at sign-in and can be invoked idempotently. It is not a continuous crash-restart watchdog. No automatic Windows reboot was initiated.

### Validation Evidence

- Four isolated Python regression tests passed: solution-only loading, root project fallback with nested exclusion, ignored generated root projects, and patch idempotence/source-drift rejection. Tests did not start .NET.
- All modified TOML files parsed. `codex mcp get serena --json` confirms enabled `streamable_http` on `http://127.0.0.1:9121/mcp`.
- Start scripts parsed; the launcher was exercised through both PowerShell 7 and the Windows PowerShell executable used by the sign-in shortcut. Repeat starts returned `AlreadyRunning`, PID 39592.
- The shared service's only Roslyn PID was 36708. Before, during, and after two concurrent clients, the same PID remained and the restore count stayed 199; project-open log count stayed zero.
- Live `get_symbols_overview` resolved `SettingsService`; `find_symbol` resolved the fully qualified `CharacterFactory` class; `find_referencing_symbols` returned consumers including `CoreScope` and `CoreGameOrchestrator` after the second client disconnected. Final query times were about 0.16, 0.16, and 2.34 seconds.
- Initial validation used an ambiguous class/constructor name and then an incorrect assumption about the reference-result JSON shape. Those harness assertions were corrected to use the full symbol path and actual returned schema; final validation passed.
- Shared process-tree private memory was about 1.93 GiB after symbol/reference queries. This is a bounded snapshot, not a long-duration leak test.
- Eight old roots and their descendants disappeared during retirement; the final process snapshot contained one Roslyn server. System commit fell from 46,828,392,448 to 37,338,820,608 bytes during cleanup. Concurrent system activity means this is not an exact per-process allocation attribution.
- Pagefile CIM and registry readback both confirm zero initial/maximum sizes (system-managed D:). The currently allocated pagefile remains 16 GiB until Windows applies startup changes.
- The original uv-cache adapter SHA-256 remains `c719160d222ca72be7236813d59c937c86b16612abddea75610367f9438b6265`; dedicated patched adapter SHA-256 is `16091d671e06ba7c2578c7efb10497685a7f585f739cfadecbc2dbc0cc97982a`.

Evidence is preserved beside the originating research note in `Shared Service Validation.json`, `Legacy Process Retirement.json`, `Pagefile Change Result.json`, and `Pagefile Before.json`.

### Documentation Updated

The originating research note points to this applied outcome. `Tools/Serena/README.md` is the operational reference for setup, lifecycle, validation, and maintaining the local patch.

### Follow-Up

Save current work and restart Windows once. The sign-in shortcut will start the shared backend, and Codex can load the HTTP configuration consistently. Existing app task snapshots may still hold old transport settings until restart; any reappearing stdio backend before then must not be counted as a second shared HTTP service.

After restart, verify system-managed pagefile configuration, the listener on port 9121, and one Roslyn process across ordinary Codex use. Long-duration stability and sign-in startup after an actual reboot were not exercised in this session. The separately observed Codex history-import request flood is not addressed by this change.
