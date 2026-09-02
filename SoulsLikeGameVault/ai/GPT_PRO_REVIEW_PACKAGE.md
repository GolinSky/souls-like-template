# GPT Pro Review Package: Local Subagents, Skills, and Markdown

This document is the single review package for the local AI-agent setup in `F:/Private/SoulsLikeTemplate`.

It inventories the project-defined subagents, their models and reasoning effort, project-local skills, Markdown used for policy/context/workflows, and local MCP topology. It separates authoritative guidance from advisory notes and generated output.

## Audit snapshot

| Field | Value |
|---|---|
| Audit date | 2026-09-02 |
| Repository | `F:/Private/SoulsLikeTemplate` |
| Branch | `main` |
| HEAD | `5a158fbb834d3940d931ab2be344787bfbf033b8` |
| Unity | `6000.3.11f1` |
| Canonical agent config | `.codex/config.toml` |
| Custom-agent definitions | `.codex/agents/*.toml` |
| Project skills | `.agents/skills/` |
| Context registry | `SoulsLikeGameVault/ai/Skill_Context_Index.md` |
| Non-generated Markdown discovered | 45 files; all tracked |
| Generated Graphify Markdown | 144 files under `graphify-out/`; ignored/local |

The inventory is based on live files. Generated Graphify output and prior review text are not authoritative when they conflict with current configuration.

## Executive topology

```text
User / parent Codex session
  └─ GPT-5.6 Sol, high reasoning (required by AGENTS.md)
       ├─ multi_agent = true; max 4 concurrent children
       ├─ default child = GPT-5.6 Terra, high reasoning
       └─ one named role per bounded task
            ├─ .codex/agents/*.toml
            │    └─ required workflow skill(s)
            ├─ optional exact-key project context
            │    └─ SoulsLikeGameVault/ai/Skill_Context_Index.md
            └─ role-specific MCP surface where declared

Policy: AGENTS.md
  ├─ role boundaries: .codex/agents/*.toml
  ├─ reusable workflows: .agents/skills/*/SKILL.md
  ├─ exact-key context: Skill_Context_Index.md + registered note
  └─ runtime servers/defaults: .codex/config.toml
```

The parent remains the final decision-maker. Writers are not run concurrently over overlapping files. Review and validation are separate read-only roles after implementation.

## Authority and ownership

The repository states this precedence order in `AGENTS.md`:

1. Active user request and system/developer instructions.
2. `AGENTS.md` for repository-wide policy and routing.
3. `.codex/agents/*.toml` for one custom role’s operating boundary.
4. `.agents/skills/*/SKILL.md` for the selected workflow.
5. `SoulsLikeGameVault/ai/Skill_Context_Index.md` and the exact registered vault note for domain context.

Live source, serialized Unity assets, and current tool output take precedence over generated Graphify output and advisory vault notes. Required registry notes add constraints but cannot override higher-level policy.

| Concern | Owner | Notes |
|---|---|---|
| Repository policy and routing | `AGENTS.md` | Unity tooling, navigation, UI workflow, orchestration, naming, persistence, and test rules |
| Codex runtime/MCP defaults | `.codex/config.toml` | Canonical project-local Codex configuration |
| Role model, effort, sandbox, instructions | `.codex/agents/*.toml` | One bounded role per file |
| Reusable workflows | `.agents/skills/*` | Do not duplicate under `.codex/skills/` |
| Curated project context | `Skill_Context_Index.md` | Exact-key allow-list; not a complete vault index |
| Domain knowledge | Registered vault Markdown | Required or advisory according to registry |
| Generated graph state | `graphify-out/` | Local operational output, never project policy |

## Runtime defaults

Source: `.codex/config.toml`, `AGENTS.md`, and `.codex/agents/README.md`.

| Setting | Value |
|---|---|
| Multi-agent feature | Enabled |
| Maximum concurrent child threads | 4 |
| Default child model | `gpt-5.6-terra` |
| Default child reasoning | `high` |
| Interrupt message | Enabled |
| Required parent model/reasoning | `gpt-5.6-sol` / `high` |
| Per-role recursion | Disabled by `[agents] enabled = false` in every role file |

The eight role files are named role definitions. Their `[agents] enabled = false` setting is documented as preventing recursive child creation; it should not be read as disabling the role itself. The repository also contains an empty legacy `.codex/skills/` directory; no live skill files are present there.

## Local subagents

All rows below are project-defined roles. The model and reasoning values are explicit in the corresponding TOML file and override the runtime default where different.

| Role | Model | Reasoning | Sandbox | Required skill(s) | Conditional/domain skill(s) |
|---|---|---:|---|---|---|
| `context_curator` | `gpt-5.6-luna` | low | read-only | `soulslike-context` | None |
| `graph_explorer` | `gpt-5.6-luna` | medium | read-only | `graphify`, `soulslike-code-navigation` | `soulslike-context` when parent supplies an exact key |
| `unity_architect` | `gpt-5.6-sol` | high | read-only | `soulslike-unity-architecture` | `soulslike-context`, `soulslike-ui-workflow` for UI scope |
| `csharp_worker` | `gpt-5.6-terra` | high | workspace-write | `soulslike-csharp-change` | `soulslike-context`, `soulslike-ui-workflow` for UI code |
| `unity_operator` | `gpt-5.6-terra` | medium | workspace-write | `soulslike-unity-assets` | `soulslike-context`, `soulslike-ui-workflow` for UI assets |
| `unity_profiler` | `gpt-5.6-terra` | high | read-only | `soulslike-performance-analysis` | `soulslike-context` when a registered domain key matters |
| `unity_reviewer` | `gpt-5.6-terra` | high | read-only | `soulslike-change-review` | Same domain skill used by the reviewed change |
| `unity_test_runner` | `gpt-5.6-luna` | low | read-only | `soulslike-validation` | Same domain skill used by the validation target |

### Role responsibilities and boundaries

| Role | Responsibility | Important constraints |
|---|---|---|
| `context_curator` | Retrieve and compress only registered project context needed for a handoff | Exact context key only; no unrelated inspection; no edits; no child agents |
| `graph_explorer` | Map bounded Unity code paths, callers/callees, ownership, and impact using Graphify plus live-source verification | Read-only; focused lookups; stale graph output is never source truth |
| `unity_architect` | Design high-risk or ambiguous Unity systems, refactors, and migrations before implementation | Produces an implementation-ready plan; no code, asset, scene, or project-setting edits |
| `csharp_worker` | Implement one bounded production C# change after path and ownership are assigned | One writer for overlapping scope; no scenes/prefabs/assets/project settings unless assigned |
| `unity_operator` | Perform one bounded Unity Editor or serialized-asset mutation | Official Unity tooling; verifies import, persistence, and console state |
| `unity_profiler` | Collect and interpret measured performance evidence before optimization | No optimization or mutation; separates Editor-only cost from runtime cost |
| `unity_reviewer` | Review an assigned Unity diff for correctness, regressions, lifecycle, DI, async, serialization, and validation gaps | Read-only; severity-ordered concrete findings |
| `unity_test_runner` | Run one explicitly assigned test, build, static check, or reproduction | Read-only; no repairs; reports exact evidence and remaining scope |

Every role TOML instructs the child not to spawn subagents. Parent orchestration rules are in `AGENTS.md`: investigate with `graph_explorer`, use `unity_architect` for high-risk design, assign exactly one writer, then use reviewer/test-runner validation as appropriate.

### Role definition files

| File | Role |
|---|---|
| `.codex/agents/context-curator.toml` | `context_curator` |
| `.codex/agents/graph-explorer.toml` | `graph_explorer` |
| `.codex/agents/unity-architect.toml` | `unity_architect` |
| `.codex/agents/csharp-worker.toml` | `csharp_worker` |
| `.codex/agents/unity-operator.toml` | `unity_operator` |
| `.codex/agents/unity-profiler.toml` | `unity_profiler` |
| `.codex/agents/unity-reviewer.toml` | `unity_reviewer` |
| `.codex/agents/unity-test-runner.toml` | `unity_test_runner` |

### Non-project runtime roles

The local multi-agent runtime also exposes generic `default`, `explorer`, and `worker` roles. They are not defined by this repository and have no project-specific model/effort values in the Markdown/TOML inventory. GPT Pro should distinguish them from the eight project-owned roles above.

Graphify also documents semantic extraction workers for doc/paper/image chunks. That is skill-internal behavior, not a ninth project role; the skill does not pin a separate model or reasoning value. Workers inherit the runtime unless an already-configured Gemini backend is used.

## Project-local skills

Each skill package contains `SKILL.md` and `agents/openai.yaml`. `graphify` additionally contains eight reference Markdown files.

| Skill | Path | Responsibility | Primary consumer |
|---|---|---|---|
| `graphify` | `.agents/skills/graphify/SKILL.md` | Build/query a persistent project knowledge graph; use existing graph first; verify important claims against source | `graph_explorer`; parent for broad questions |
| `soulslike-change-review` | `.agents/skills/soulslike-change-review/SKILL.md` | Review a bounded Unity diff for correctness, regressions, architecture, and validation gaps | `unity_reviewer` |
| `soulslike-code-navigation` | `.agents/skills/soulslike-code-navigation/SKILL.md` | Map a bounded code path, dependencies, entry points, ownership, and impact | `graph_explorer` |
| `soulslike-context` | `.agents/skills/soulslike-context/SKILL.md` | Resolve exact-key curated vault context with Obsidian-first and disk fallback | `context_curator`; conditional for other roles |
| `soulslike-csharp-change` | `.agents/skills/soulslike-csharp-change/SKILL.md` | Implement one bounded production C# change after scope is understood | `csharp_worker` |
| `soulslike-performance-analysis` | `.agents/skills/soulslike-performance-analysis/SKILL.md` | Collect measured evidence for profiling, allocations, spikes, rendering, loading, or synchronization | `unity_profiler` |
| `soulslike-ui-workflow` | `.agents/skills/soulslike-ui-workflow/SKILL.md` | Apply registered UI architecture to controllers, presenters, views, prefabs, Addressables, inventory, and equipment UI | Conditional for architect, worker, operator, reviewer, test runner |
| `soulslike-unity-architecture` | `.agents/skills/soulslike-unity-architecture/SKILL.md` | Design high-risk Unity ownership, lifetime, DI, serialization, asset, and lifecycle changes | `unity_architect` |
| `soulslike-unity-assets` | `.agents/skills/soulslike-unity-assets/SKILL.md` | Perform and persist one bounded Unity scene/prefab/asset/import/serialization mutation | `unity_operator` |
| `soulslike-validation` | `.agents/skills/soulslike-validation/SKILL.md` | Execute one assigned validation task and return compact evidence without repair | `unity_test_runner` |

### Skill package files

| Package | `SKILL.md` | UI/agent metadata |
|---|---:|---|
| `graphify` | 708 lines | `agents/openai.yaml`; implicit invocation allowed |
| `soulslike-change-review` | 16 lines | `agents/openai.yaml` |
| `soulslike-code-navigation` | 13 lines | `agents/openai.yaml` |
| `soulslike-context` | 16 lines | `agents/openai.yaml` |
| `soulslike-csharp-change` | 14 lines | `agents/openai.yaml` |
| `soulslike-performance-analysis` | 14 lines | `agents/openai.yaml` |
| `soulslike-ui-workflow` | 13 lines | `agents/openai.yaml` |
| `soulslike-unity-architecture` | 16 lines | `agents/openai.yaml` |
| `soulslike-unity-assets` | 15 lines | `agents/openai.yaml` |
| `soulslike-validation` | 14 lines | `agents/openai.yaml` |

The ten `openai.yaml` files provide display names, short descriptions, and default prompts. Only `graphify` explicitly sets `allow_implicit_invocation = true`.

### Graphify reference Markdown

These are supporting workflow instructions, not SoulsLike policy. They are loaded only for the trigger described below.

| File | Trigger/purpose |
|---|---|
| `.agents/skills/graphify/references/add-watch.md` | Add a URL to the graph or watch a folder |
| `.agents/skills/graphify/references/exports.md` | Extra exports and large-corpus benchmark |
| `.agents/skills/graphify/references/extraction-spec.md` | Semantic extraction subagent prompt for docs/papers/images |
| `.agents/skills/graphify/references/github-and-merge.md` | GitHub clone or multi-path graph merge |
| `.agents/skills/graphify/references/hooks.md` | Commit hook or native `CLAUDE.md` integration |
| `.agents/skills/graphify/references/query.md` | Query/path/explain against an existing graph |
| `.agents/skills/graphify/references/transcribe.md` | Video/audio transcription when detected |
| `.agents/skills/graphify/references/update.md` | Incremental update or cluster-only flow |

The upstream `hooks.md` and parts of `SKILL.md` still mention Claude/`CLAUDE.md`; those references may be irrelevant in an AGENTS/Codex-first repository and should be reviewed before presenting them as local policy.

## Curated project context

`SoulsLikeGameVault/ai/Skill_Context_Index.md` is an exact-key allow-list, not a directory-wide authority map.

| Key | Note | Authority | Used for |
|---|---|---|---|
| `ui-code` | `ui/UI_Code_Build_Guide.md` | Required | UI controllers, presenters, views, prefabs, Addressables |
| `inventory-ui` | `ui/Inventory UI-UX Architecture & Unity Implementation Guide.md` | Advisory | Inventory layout, cells, state, focus, input |
| `equipment-ui` | `ui/Equipment UI-UX Architecture & Unity Implementation Guide.md` | Advisory | Equipment layout, comparison, navigation, state |

Context resolution is exact-key only. Obsidian MCP is preferred; the checked-in disk fallback is used when Obsidian is unavailable. Unregistered vault notes cannot silently become project policy.

## Markdown inventory

The following is the complete non-generated Markdown inventory at audit time.

### Active policy and operational Markdown

| Path | Lines | Classification |
|---|---:|---|
| `AGENTS.md` | 228 | Repository policy authority |
| `GEMINI.md` | 5 | Client-specific Unity policy pointer; subordinate to `AGENTS.md` |
| `README.md` | 25 | General project overview; not architecture authority |
| `.agents/README.md` | 11 | Project skill/package ownership notes |
| `.codex/agents/README.md` | 27 | Eight-role Sol/Terra/Luna operating guide |
| `Assets/ThirdParty/Jorjouto/ACS/README.md` | vendor | Third-party documentation; not project policy |

### Skill and workflow Markdown

The ten `.agents/skills/*/SKILL.md` files are listed in the skill table above. The eight Graphify reference files are listed in the Graphify table above. They are all project-tracked and operationally discoverable from the skills root.

### Vault Markdown

| Path | Lines | Classification |
|---|---:|---|
| `SoulsLikeGameVault/ai/GPT_PRO_REVIEW_PACKAGE.md` | this file | Review package / inventory |
| `SoulsLikeGameVault/ai/Skill_Context_Index.md` | 16 | Context policy allow-list |
| `SoulsLikeGameVault/Arhitecture/CHARACTER_ECOSYSTEM_ARCHITECTURE_ANALYSIS.md` | 1,541 | Unregistered architecture analysis; advisory/draft |
| `SoulsLikeGameVault/Arhitecture/PROJECT_ORGANIZATION.md` | 88 | Organization guide; stale project name requires review |
| `SoulsLikeGameVault/Artifact/elden_ring_inventory_equipment_architecture.md` | 1,813 | Architecture artifact; not registered context |
| `SoulsLikeGameVault/features/Advanced Locomotion Architecture Prompt Specification.md` | 3 | Prompt artifact; no H1 |
| `SoulsLikeGameVault/features/Current Jump and Roll System.md` | 160 | Feature/system note; not registered context |
| `SoulsLikeGameVault/features/Locomotion Architecture Technical Specification.md` | 29 | Technical specification; not registered context |
| `SoulsLikeGameVault/features/Movement Mechanics Explained.md` | 1,286 | Feature explanation/specification; not registered context |
| `SoulsLikeGameVault/features/System Specification - Souls-like Locomotion & Camera System.md` | 74 | Feature specification; not registered context |
| `SoulsLikeGameVault/features/Technical Specification - Roll & Backstep Vectoring Logic.md` | 49 | Feature specification; not registered context |
| `SoulsLikeGameVault/ToDo/Character_Command_HSM_Runtime_Refactor_Plan.md` | 1,571 | Refactor plan; advisory/draft |
| `SoulsLikeGameVault/ToDo/Character_Mediator_Architectural_Analysis.md` | 89 | Architectural TODO/analysis; contains machine-specific `file:///` link |
| `SoulsLikeGameVault/ToDo/First Steps.md` | 2 | Placeholder; no H1 |
| `SoulsLikeGameVault/ToDo/Hitbox System Implementation Plan.md` | 452 | Implementation plan; not registered context |
| `SoulsLikeGameVault/ToDo/Hitbox System.md` | 327 | System note; not registered context |
| `SoulsLikeGameVault/ToDo/LIGHTING_BAKE_PLAN.md` | 58 | Operational plan; not registered context |
| `SoulsLikeGameVault/ui/Equipment UI-UX Architecture & Unity Implementation Guide.md` | 202 | Advisory registered context: `equipment-ui` |
| `SoulsLikeGameVault/ui/Inventory UI-UX Architecture & Unity Implementation Guide.md` | 202 | Advisory registered context: `inventory-ui` |
| `SoulsLikeGameVault/ui/UI_Code_Build_Guide.md` | 145 | Required registered context: `ui-code` |
| `SoulsLikeGameVault/Welcome.md` | 5 | Obsidian boilerplate; no H1 |

All 45 non-generated Markdown files are currently tracked. Earlier claims that skill/context additions were untracked are stale for this snapshot.

### Generated Graphify Markdown

`graphify-out/` currently contains 144 Markdown files: 15 `GRAPH_REPORT.md` snapshots, 128 query-memory files, and one reflections file. It is ignored by `.gitignore` and explicitly excluded from project policy. Existing generated files must not be mistaken for current documentation or a complete source inventory.

## MCP and local tool topology

### Parent Codex configuration

Source: `.codex/config.toml`.

| Server | State | Purpose |
|---|---|---|
| `unity` | Enabled | Official `unity mcp` bridge; direct Unity CLI operations |
| `serena` | Enabled | `serena-agent==1.7.0`, Python 3.13, project-from-CWD C# symbol/LSP work |
| `obsidian` | Enabled | Loopback Obsidian MCP at HTTP `127.0.0.1:27123` |
| `graphify` | Disabled in parent | Local Graphify server over `graphify-out/graph.json`; role-local for `graph_explorer` |
| `unityMCP` | Disabled tombstone | Legacy compatibility entry; no active command |

`graph-explorer.toml` enables only Graphify for that role and explicitly disables Unity, Serena, Obsidian, and legacy `unityMCP`. Its allowed Graphify tools are `query_graph`, `get_node`, `get_neighbors`, `god_nodes`, `graph_stats`, and `shortest_path`.

### Other tracked client configurations

| File | Servers | Transport notes |
|---|---|---|
| `.agents/mcp_config.json` | `unity`, `obsidian` | Official `unity mcp`; Obsidian HTTPS `127.0.0.1:27124` |
| `.antigravity/mcp_config.json` | `unity`, `obsidian` | Same Unity and Obsidian HTTPS setup |
| `.serena/project.yml` | Serena project state | C# language server, UTF-8, Git-ignore-aware, writable, workspace root `.` |

Absolute project paths are embedded in the generic-agent and Antigravity MCP configs. The Codex config resolves Unity/Serena from project context. All listed configuration files are tracked.

## Security and portability boundaries

Credential values are intentionally not reproduced here.

- Tracked MCP configuration contains an Obsidian bearer authorization value.
- Tracked Obsidian plugin state contains an API key and cryptographic material, including private-key material under the plugin’s `crypto` configuration.
- Loopback binding limits network reachability but does not make a bearer token read-only; the Obsidian service can mutate the vault.
- Codex uses HTTP on port `27123`, while `.agents` and Antigravity use HTTPS on `27124`; the split is not explained by a single canonical transport policy.
- Duplicate client configurations repeat absolute paths and authentication configuration.
- The repository therefore has both local single-user convenience risk and repository-sharing/clone portability risk.

GPT Pro should review secret rotation/removal, sanitized templates, environment/credential injection, canonical MCP source generation, and whether secure port `27124` can become the documented default without breaking local use.

## Findings for GPT Pro review

1. **Authority is mostly explicit.** `AGENTS.md` separates policy, roles, skills, exact-key context, and generated state. Confirm whether it should remain the only policy index.
2. **The eight role model/effort assignments are coherent.** The runtime default is Terra/high; Luna is used for narrow/low-cost tasks; Sol/high is reserved for parent orchestration and architecture.
3. **Role activation versus recursion is easy to misread.** Every role TOML has `[agents] enabled = false`, while the parent has `[agents] enabled = true`. The README explains this as recursion prevention; runtime behavior should be verified.
4. **Graphify is role-local but parent-mandatory by policy.** `AGENTS.md` requires Graphify first for broad questions, while the parent Graphify MCP is disabled. The CLI path works, and `graph_explorer` has dedicated Graphify MCP, but the split should be documented deliberately.
5. **The skill migration has residue.** `.agents/skills/` is the live, tracked root; `.codex/skills/` is empty but still exists.
6. **MCP credentials are a high-priority repository-sharing concern.** The bearer token and plugin crypto state are tracked. Loopback-only exposure and source-control exposure require separate threat models.
7. **MCP transport/configuration is duplicated.** Codex, generic-agent, and Antigravity configuration should be generated from a sanitized canonical source or have an explicit divergence policy.
8. **Upstream Graphify references leak another client’s vocabulary.** `CLAUDE.md` guidance in `hooks.md` should be isolated as upstream reference material or rewritten for this Codex/AGENTS setup.
9. **Some domain Markdown is stale or unregistered.** `Arhitecture/PROJECT_ORGANIZATION.md` names `MirrorMultiplayerTemplate`; one TODO contains legacy Unity command wording; another contains a machine-specific `file:///` link.
10. **The vault is larger than the context registry.** That is safe for policy, but GPT Pro should decide whether reviewed architecture/TODO notes need exact keys or should remain human-only advisory material.
11. **Generated Graphify output is large and historical.** Retention/expiry rules are needed so 144 generated Markdown files do not pollute future broad searches.
12. **The current Graphify index is not evidence for this layer.** Its detection sidecar reports 89 code files and zero documents, and its report predates the current HEAD; direct file inspection is required for agent/skill/config review.

## Decisions requested

1. Keep `AGENTS.md` as the sole repository policy authority, with role TOMLs, skills, and exact-key context strictly subordinate?
2. Keep `.agents/skills/` as the only skill root and remove the empty `.codex/skills/` residue?
3. Keep Graphify role-local, enable it in the parent, or document the CLI/role-MCP split as intentional?
4. Keep the eight role model/effort assignments, especially Luna/low for context and validation and Terra/high for implementation/review?
5. Make one sanitized MCP configuration the source for Codex, generic agents, and Antigravity?
6. Remove tracked bearer/API/private-key material and replace it with local secret provisioning without breaking Obsidian MCP?
7. Standardize Obsidian on secure local transport or document the two-port split explicitly?
8. Rewrite `GEMINI.md` and Graphify’s Claude-specific hook text for current Codex policy, or retain compatibility language?
9. Classify every vault note as required policy, advisory guidance, draft, historical artifact, or generated output?
10. Add exact context keys for reviewed character/locomotion/hitbox notes, or keep the registry narrow?
11. Correct stale project names, machine-specific links, legacy command references, folder spelling, and missing H1s in a separate documentation cleanup?
12. Define Graphify generated-output retention and search-exclusion rules?

## Suggested acceptance criteria

- A clean clone discovers all intended role definitions and skill packages without untracked files.
- One document states the authority order without contradicting the TOMLs or skills.
- Every role has an explicit model, reasoning effort, sandbox, required skill, and recursion policy.
- Default child settings and per-role overrides are documented and runtime-verified.
- Every role receives only the MCP surface it needs.
- UI work resolves `ui-code` first and uses inventory/equipment keys only when applicable.
- Required context is committed, stable, exact-key addressable, and has a disk fallback.
- Advisory/draft notes cannot silently override live source or required policy.
- No bearer token, API key, or private cryptographic material is committed for repository sharing.
- MCP transport and configuration ownership are canonical and portable.
- Generated Graphify output cannot masquerade as authoritative documentation.
- Legacy `.codex/skills` residue, stale client-specific text, and machine-specific links have explicit keep/change/remove decisions.

## Evidence limits

- This package inventories configuration and Markdown; it does not claim that every role has been executed in the current session.
- The existing Graphify graph is gameplay-heavy and does not reliably represent dot-directories, configuration, or all current documentation. The independent audit found no graph nodes for `.codex`, `.agents`, `AGENTS.md`, or project skill/config files.
- The graph report predates the current HEAD; direct filesystem/TOML/JSON inspection is authoritative for this package.
- Unity Editor state, tests, and builds were not run for this documentation task.
- Existing working-tree changes to `Assets/Art/Fonts/Cinzel/Cinzel[wght] SDF.asset` and `UserSettings/Layouts/default-6000.dwlt` were present before this package update and are unrelated to the agent inventory.
