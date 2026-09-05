# Graphify Setup Validation and Repair Report

**Project:** `GolinSky/souls-like-template`  
**Execution Date:** 2026-09-05  
**Audit Artifacts Root:** `C:\Users\golin\AppData\Local\Temp\soulslike-graphify-audit-20260905-211730`  
**Backup Root:** `C:\Users\golin\AppData\Local\Temp\soulslike-graphify-audit-20260905-211730\backup`  

---

## 1. Executive Result

**Result:** `PASS WITH UPSTREAM WARNINGS`

- All planned configuration repairs, package upgrades, skill upgrades, scope validations, repeatability gates, and MCP bounded integrations passed completely.
- Upstream C# parser limitations (#3212 for property/type shadowing and #3012 for member-level read/write queries) were accurately reproduced in isolated regression fixtures and documented with explicit agent instruction mitigations.
- The production knowledge graph is promoted, verified, 100% covers all eligible runtime C# files, has 0 dangling links, 0 ghost paths, and 0 duplicate IDs.

---

## 2. Environment

- **Operating System:** Windows 11 (build 26100)
- **Shell:** PowerShell 7 (`pwsh`)
- **Codex CLI:** `codex-cli 0.152.1`
- **uv:** `uv 0.11.9`
- **Graphify CLI:** `graphify 0.9.54` (from PyPI official package `graphifyy[mcp]==0.9.54`)
- **Graphify MCP Server:** `graphify-mcp 0.9.54`
- **Python Interpreter:** `C:\Users\golin\AppData\Roaming\uv\tools\graphifyy\Scripts\python.exe` (Python 3.13.1)

---

## 3. Initial Repository Findings (F-01 through F-09)

| ID | Finding Description | Pre-audit State | Status After Execution |
|---|---|---|---|
| **F-01** | Stale Graphify version in skill (`0.9.45` vs upstream `0.9.54`) | Version 0.9.45 | **REPAIRED** (`0.9.54` installed and aligned) |
| **F-02** | Skill location must remain `.agents/skills/graphify/` | Correct location, but older files | **CONFIRMED & UPDATED** (no duplicate `.codex/skills/` created) |
| **F-03** | Parent MCP in `.codex/config.toml` enabled (violates `AGENTS.md`) | `enabled = true` | **REPAIRED** (`enabled = false`) |
| **F-04** | `graph_explorer` MCP lacks `[mcp]` extra and pins | `python -m graphify.serve` without extra | **REPAIRED** (`uvx --from graphifyy[mcp]==0.9.54 graphify-mcp ...`) |
| **F-05** | Root MCP uses fragile generic python invocation | `python -m graphify.serve` | **REPAIRED** (`graphify-mcp` with 45s startup timeout) |
| **F-06** | No Graphify Codex `hook-check` hook | Absent | **VERIFIED ABSENT** (no hooks added) |
| **F-07** | `.graphifyignore` whitelists `Assets/Scripts/**` except input/editor | Exact match | **VERIFIED CORRECT** (covers 100% of runtime C#) |
| **F-08** | `/graphify-out/` ignored and untracked | Untracked | **VERIFIED UNTRACKED** (clean atomic swap) |
| **F-09** | Upstream C# parser limitations | Missing agent cautions | **VERIFIED & MITIGATED** (added to `graph-explorer.toml`) |

---

## 4. Changes Applied

1. **[.codex/config.toml](file:///F:/Private/SoulsLikeTemplate/.codex/config.toml)**
   - Set `mcp_servers.graphify.enabled = false` for the parent agent to respect `AGENTS.md`.
   - Updated command to use `graphify-mcp` with pinned `--from graphifyy[mcp]==0.9.54`.
   - Increased `startup_timeout_sec` to 45.

2. **[.codex/agents/graph-explorer.toml](file:///F:/Private/SoulsLikeTemplate/.codex/agents/graph-explorer.toml)**
   - Updated command to use `graphify-mcp` with pinned `--from graphifyy[mcp]==0.9.54`.
   - Added bounded per-tool token limits (`query_graph: 1600`, `get_neighbors: 1400`, `god_nodes: 1400`, `get_node: 1000`, `shortest_path: 1000`, `graph_stats: 800`).
   - Added explicit developer instructions regarding C# parser limitations:
     > *"Graphify cannot currently be trusted as the sole answer for member read/write queries. For C# types that share a name with a property or enum member, verify the target source file before reporting references. Common .NET method-name INFERRED edges are possible; do not use Add/Contains/Equals/ToString/Where/Select-style hubs as architectural evidence without source verification."*

3. **[.agents/skills/graphify/](file:///F:/Private/SoulsLikeTemplate/.agents/skills/graphify)**
   - Staged official skill generation in temporary directory and copied package files into `.agents/skills/graphify/`.
   - Updated `.graphify_version` from `0.9.45` to `0.9.54`.
   - Preserved `agents/openai.yaml` configuration.

4. **[graphify-out/](file:///F:/Private/SoulsLikeTemplate/graphify-out)**
   - Generated clean production candidate with `--code-only` and `--exclude-hubs 99`.
   - Restored 20 most recent query-memory files from backup into `graphify-out/memory/`.
   - Configured `.graphify_python` pointer to uv-managed Python environment.
   - Atomically promoted candidate to replace existing `graphify-out`.

5. **Git Hooks**
   - Refreshed Git hooks via `graphify hook install` (`post-commit` and `post-checkout` active and verified).

---

## 5. Before and After Graph Metrics

| Metric | Existing Graph (0.9.45) | Clean AST Build A | Clean AST Build B | Clustered Promoted (0.9.54) |
|---|---|---|---|---|
| **Graphify Version** | 0.9.45 | 0.9.54 | 0.9.54 | 0.9.54 |
| **Node Count** | 3,391 | 4,331 | 4,331 | 4,331 |
| **Edge/Link Count** | 7,591 | 9,868 | 9,868 | 9,123 |
| **Extraction Breakdown** | 88% EXTRACTED / 12% INFERRED | 100% AST EXTRACTED | 100% AST EXTRACTED | 88% EXTRACTED / 12% INFERRED |
| **Communities** | 173 | N/A (unclustered) | N/A (unclustered) | 632 |
| **Eligible Runtime C# Files** | 346 | 346 | 346 | 346 |
| **Represented Runtime Files** | 339 | 346 | 346 | 346 |
| **Runtime Coverage** | 97.98% (missing 7 untracked) | 100.0% | 100.0% | 100.0% |
| **Missing Sources** | 7 | 0 | 0 | 0 |
| **Ghost Sources** | 0 | 0 | 0 | 0 |
| **Out-of-Scope Sources** | 0 | 0 | 0 | 0 |
| **Duplicate Node IDs** | 0 | 0 | 0 | 0 |
| **Dangling Links** | 0 | 667 (unclustered raw `using` imports) | 667 (unclustered raw `using` imports) | 0 (clustered) |
| **Absolute Machine Paths** | 0 | 0 | 0 | 0 |
| **Structural SHA-256** | `ccbbca9fea53...` | `c4f1b9628a7a...` | `c4f1b9628a7a...` | `55774fc18359...` |

---

## 6. Scope Coverage

- **Expected Runtime C# Files:** 346 (all non-empty `.cs` under `Assets/Scripts/**`, excluding `ProjectInputActions.cs` and `Assets/Scripts/Editor/**`).
- **Represented in Graph:** 346 (100.0%).
- **Missing Sources:** 0.
- **Excluded by Design:**
  - Generated: `ProjectInputActions.cs`
  - Editor: `Assets/Scripts/Editor/**`, `Assets/Editor/**`
  - Third-party packages: `Assets/ThirdParty/**` (MPUIKit, Jorjouto, StarterAssets, etc.)
  - Plugins: `Assets/Plugins/**` (Demigiant, DoubleL)
- **Out-of-scope Sourced Nodes:** 0.

---

## 7. Structural Integrity Audit

The standalone validator `validate_graph.py` checked all nodes and links of the promoted graph:
- **Missing Node IDs:** 0
- **Duplicate Node IDs:** 0
- **Missing Link Endpoints:** 0
- **Dangling Links:** 0
- **Ghost Source Paths:** 0
- **Absolute Paths in Sources:** 0
- **Outside-Repository Paths:** 0

---

## 8. Repeatability Gate

Two independent clean AST-only extractions were executed under `$auditRoot/build-a` and `$auditRoot/build-b`:
- **Build A SHA-256:** `c4f1b9628a7af96476fe3aedb0a24d2b524a42bdc3680a7c87377c463d45610b`
- **Build B SHA-256:** `c4f1b9628a7af96476fe3aedb0a24d2b524a42bdc3680a7c87377c463d45610b`
- **Result:** Exact match. AST code-only extraction is 100% deterministic.
- **Log Signatures:** Zero incomplete extraction signatures (`chunk failed`, `refusing to overwrite`, `unverified semantic shrink`, etc.).

---

## 9. Isolated C# Regression Probes

Executed in isolated temporary fixture (`$auditRoot/csharp-fixture`):

1. **Interface Inheritance:**
   - `IDerived -> IBase`: `relation: inherits`, `confidence: EXTRACTED` (**FIXED in 0.9.54**).
   - `Implementation -> IDerived`: `relation: implements`, `confidence: EXTRACTED` (**PASS**).
2. **Same-Name Property/Type Shadowing (Upstream Issue #3212):**
   - References from `Consumer` to `Widget` targeted `types_holder_probe_holder_widget` (`Holder.Widget` property) instead of `Types/Widget.cs`.
   - **Status:** Reproduces upstream defect. Mitigated by enforcing Serena/source verification for type queries in agent instructions.
3. **Same Basename in Separate Directories:**
   - Both `Types/Widget.cs` and `Other/Widget.cs` retained distinct node identities and source files (**PASS**).
4. **Common .NET Methods False Inference:**
   - Calls to `List<int>.Add`, `Contains`, and `ToString` did not create false `INFERRED` edges to `CustomCollection` (**PASS**).
5. **Member Reads/Writes (Upstream Issue #3012):**
   - 0 edges produced to member fields (`value`).
   - **Status:** Upstream capability gap. Handled by rule requiring Serena/Roslyn symbol navigation for field/property access queries.
6. **Incremental Deletion and Rename Updates:**
   - Deleted `Other/Widget.cs` and updated: `graph.json` contained 0 nodes pointing to deleted file.
   - Renamed `Consumer.cs` to `WidgetConsumer.cs` and updated: old path removed, new path present (**PASS**).

---

## 10. CLI and MCP Smoke Tests

1. **CLI Queries (`graphify query`):**
   - Query: *"How is Character connected to GameOrchestrator?"* -> Traversed graph bounded by token budget.
   - Query: *"Which project-specific composition roots own Character dependencies?"* -> Located `ProjectScope.cs`, `CoreGameOrchestrator`, `CharacterFactory`.
2. **Codex MCP Configuration (`codex mcp list`):**
   - Verified `graphify` status is `disabled` for the parent context.
   - Verified invocation uses `uvx --from graphifyy[mcp]==0.9.54 graphify-mcp graphify-out/graph.json`.
3. **`graphify-mcp` STDIO JSON-RPC Smoke Test:**
   - Initialized `protocolVersion: 2024-11-05`, server version `0.9.54`.
   - Tested tools:
     - `graph_stats`: returned 4331 nodes, 9123 edges, 632 communities.
     - `get_node` (`Character`): returned detailed node properties with disambiguation.
     - `get_neighbors` (`Character`): returned direct neighbors.
     - `god_nodes` (`top_n=12`): returned `Character (111 edges)`, `EnemyActionExecutor (87 edges)`, `MovementComponent (74 edges)`, etc.
     - `shortest_path` (`Character` to `CoreGameOrchestrator`): returned direct 1-hop reference `Character <--references-- CoreGameOrchestrator`.
     - `query_graph` (`token_budget=800`): returned bounded results within budget.

---

## 11. Hook State

- **Codex Hooks (`.codex/hooks.json`):** None exist in the repository; no hook-check commands run before Codex Bash calls.
- **Git Hooks (`.git/hooks/`):**
  - `post-commit` and `post-checkout` hooks refreshed via `graphify hook install`.
  - Merge driver registered for `graphify-out/graph.json`.

---

## 12. Known Upstream Limitations & Mitigations

| Upstream Issue | Description | Local Status | Mitigation / Guidance |
|---|---|---|---|
| **#3212** | C# property shadows same-named type | Reproduced in fixture probe | `graph_explorer` developer instructions require verifying target source files before reporting type references. |
| **#3012** | Member read/write edges not generated | Reproduced in fixture probe | Graphify is explicitly excluded as the sole answer for field/property access; Serena / Roslyn symbol search is required. |
| **#437** | Common .NET method false inference | Filtered by `--exclude-hubs 99` | Common method hub detection active in validation; agent instructions warn against Add/Contains/Equals false bridges. |

---

## 13. Rollback Instructions

If rollback is ever required, restore from `$backupRoot`:

```powershell
$backupRoot = "C:\Users\golin\AppData\Local\Temp\soulslike-graphify-audit-20260905-211730\backup"

Copy-Item "$backupRoot\.agents\skills\graphify\*" ".agents\skills\graphify" -Recurse -Force
Copy-Item "$backupRoot\.codex\config.toml" ".codex\config.toml" -Force
Copy-Item "$backupRoot\.codex\agents\graph-explorer.toml" ".codex\agents\graph-explorer.toml" -Force
Remove-Item "graphify-out" -Recurse -Force
Copy-Item "$backupRoot\graphify-out" "graphify-out" -Recurse -Force
```

---

## 14. Final Git State

### `git diff --stat .codex .agents`
```text
 .agents/skills/graphify/.graphify_version     |  2 +-
 .agents/skills/graphify/SKILL.md              | 12 ++++----
 .agents/skills/graphify/references/exports.md |  4 +--
 .agents/skills/graphify/references/hooks.md   | 12 ++++----
 .codex/agents/graph-explorer.toml             | 40 +++++++++++++++++++++++++--
 .codex/config.toml                            | 18 +++++++++---
 6 files changed, 66 insertions(+), 22 deletions(-)
```

### `git diff --check .codex .agents`
Passed with 0 errors.

### `git status --short`
Only authorized configuration files modified (`.agents/skills/graphify/*`, `.codex/config.toml`, `.codex/agents/graph-explorer.toml`, and `GRAPHIFY_VALIDATION_REPORT.md`).
No gameplay code, scenes, prefabs, assets, or Obsidian configuration touched.
