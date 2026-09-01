# GPT Pro Review Package: Documentation, Skills, Agents, and MCP Architecture

## Review request

Review this repository snapshot as a senior AI-agent platform and Unity tooling architect. Focus on ownership, authority, hierarchy, portability, security boundaries, duplication, stale guidance, and whether the current skills/agents/MCP design is the smallest coherent system.

Do not review gameplay implementation from this package. Treat paths and configuration facts below as evidence, but request source verification before recommending destructive moves or renames.

Requested output:

1. Findings ordered by severity and architectural impact.
2. A canonical ownership model for documentation, skills, agents, and MCP configuration.
3. A minimal migration plan that preserves working integrations.
4. Explicit keep/change/remove decisions for every inconsistency listed here.
5. A proposed Obsidian vault taxonomy and context-registry policy.
6. Security recommendations that distinguish loopback-only risk from repository-sharing risk.

## Snapshot metadata

| Field | Value |
|---|---|
| Repository root | `F:/Private/SoulsLikeTemplate` |
| Git branch | `main` |
| HEAD | `4c1e9047` |
| Snapshot date | `2026-09-01` |
| Unity version | `6000.3.11f1` |
| Obsidian vault root | `SoulsLikeGameVault/` |
| Project skills root | `.agents/skills/` |
| Custom Codex agents root | `.codex/agents/` |
| Primary Codex config | `.codex/config.toml` |

This is a dirty working-tree snapshot. Transitional and untracked state is identified explicitly below.

## Executive topology

```text
AGENTS.md
  ├─ defines repository policy and role/skill routing
  ├─ routes custom roles from .codex/agents/*.toml
  │    └─ each role invokes workflow skills from .agents/skills/*/SKILL.md
  ├─ routes curated project context through
  │    SoulsLikeGameVault/ai/Skill_Context_Index.md
  └─ configures runtime tools through .codex/config.toml
       ├─ Unity official CLI MCP bridge
       ├─ Serena project-local symbol server
       ├─ Obsidian local REST/MCP server
       └─ Graphify graph server (disabled in parent; enabled for graph_explorer)
```

The Obsidian vault is project knowledge. `.agents`, `.codex`, and `.serena` are operational agent configuration and intentionally live outside the vault.

## Markdown inventory summary

Counts include this review package and exclude package caches, Unity `Library`, temporary output, and non-Markdown files.

| Scope | Markdown files | Role |
|---|---:|---|
| Repository root | 3 | General README and agent/tool policy |
| `.agents/` | 19 | Project-skill contracts, Graphify references, local skill README |
| `.codex/` | 1 | Custom-agent operating guide |
| `SoulsLikeGameVault/` | 20 | Obsidian project knowledge, plans, specifications, and this package |
| `Assets/ThirdParty/` | 1 | Vendor documentation |
| Non-generated total | 44 | Reviewable project and vendor Markdown |
| `graphify-out/` | 132 | Generated/local reports, query memories, and reflections |

Generated Graphify Markdown consists of 15 graph reports, 116 query-memory files, and one reflections file. It is operational output rather than canonical project documentation.

## Root and operational Markdown

| Path | Intended authority | Current observation |
|---|---|---|
| `AGENTS.md` | Required repository policy | Main authority for Unity tooling, navigation, role orchestration, naming, persistence, and test execution. Modified in the working tree. |
| `README.md` | Introductory | Short public project overview; not an architecture authority. |
| `GEMINI.md` | Legacy agent guidance | Contains only a generic instruction to use Unity MCP for UI prefabs; underspecified relative to `AGENTS.md`. |
| `.agents/README.md` | Operational | Explains sibling project skills. Untracked. |
| `.codex/agents/README.md` | Operational | Describes the eight-role Sol/Terra/Luna setup. Modified. |
| `Assets/ThirdParty/Jorjouto/ACS/README.md` | Vendor | Third-party package documentation; should not define project policy. |

## Obsidian vault hierarchy

```text
SoulsLikeGameVault/
  ├─ ai/
  │   ├─ Skill_Context_Index.md
  │   └─ GPT_PRO_REVIEW_PACKAGE.md
  ├─ Arhitecture/
  │   ├─ CHARACTER_ECOSYSTEM_ARCHITECTURE_ANALYSIS.md
  │   └─ PROJECT_ORGANIZATION.md
  ├─ Artifact/
  │   └─ elden_ring_inventory_equipment_architecture.md
  ├─ features/
  │   ├─ Advanced Locomotion Architecture Prompt Specification.md
  │   ├─ Current Jump and Roll System.md
  │   ├─ Locomotion Architecture Technical Specification.md
  │   ├─ Movement Mechanics Explained.md
  │   ├─ System Specification - Souls-like Locomotion & Camera System.md
  │   └─ Technical Specification - Roll & Backstep Vectoring Logic.md
  ├─ ToDo/
  │   ├─ Character_Command_HSM_Runtime_Refactor_Plan.md
  │   ├─ Character_Mediator_Architectural_Analysis.md
  │   ├─ First Steps.md
  │   ├─ Hitbox System.md
  │   └─ LIGHTING_BAKE_PLAN.md
  ├─ ui/
  │   ├─ Equipment UI-UX Architecture & Unity Implementation Guide.md
  │   ├─ Inventory UI-UX Architecture & Unity Implementation Guide.md
  │   └─ UI_Code_Build_Guide.md
  └─ Welcome.md
```

The vault has six content folders, not three. The number three refers to the three entries in the AI context allow-list.

### Vault note inventory

| Vault-relative path | Lines before this package | Git state | Intended role |
|---|---:|---|---|
| `ai/Skill_Context_Index.md` | 16 | Untracked | Exact-key context allow-list for agent workflows |
| `Arhitecture/CHARACTER_ECOSYSTEM_ARCHITECTURE_ANALYSIS.md` | 1,541 | Untracked | Raw character architecture inventory and simplification analysis |
| `Arhitecture/PROJECT_ORGANIZATION.md` | 88 | Tracked | Asset organization guide; currently names `MirrorMultiplayerTemplate` |
| `Artifact/elden_ring_inventory_equipment_architecture.md` | 1,813 | Tracked | Large inventory/equipment architecture artifact |
| `features/Advanced Locomotion Architecture Prompt Specification.md` | 3 | Tracked | Compact/raw prompt artifact; no H1 |
| `features/Current Jump and Roll System.md` | 160 | Tracked | Current-system description |
| `features/Locomotion Architecture Technical Specification.md` | 29 | Tracked | Locomotion technical prompt/specification |
| `features/Movement Mechanics Explained.md` | 1,286 | Tracked | Large mixed movement explanation/specification |
| `features/System Specification - Souls-like Locomotion & Camera System.md` | 74 | Tracked | Locomotion and camera specification |
| `features/Technical Specification - Roll & Backstep Vectoring Logic.md` | 49 | Tracked | Roll/backstep vectoring specification |
| `ToDo/Character_Command_HSM_Runtime_Refactor_Plan.md` | 1,571 | Tracked | Large behavior-preserving refactor plan |
| `ToDo/Character_Mediator_Architectural_Analysis.md` | 89 | Tracked | Character mediator refactor TODO |
| `ToDo/First Steps.md` | 2 | Tracked | Minimal placeholder; no H1 |
| `ToDo/Hitbox System.md` | 202 | Untracked | Hitbox-system notes; no H1 |
| `ToDo/LIGHTING_BAKE_PLAN.md` | 58 | Tracked | Lighting bake plan |
| `ui/Equipment UI-UX Architecture & Unity Implementation Guide.md` | 202 | Tracked | Advisory equipment UI/UX guidance |
| `ui/Inventory UI-UX Architecture & Unity Implementation Guide.md` | 202 | Tracked | Advisory inventory UI/UX guidance |
| `ui/UI_Code_Build_Guide.md` | 145 | Untracked | Required UI workflow source referenced by `AGENTS.md` |
| `Welcome.md` | 5 | Tracked | Default Obsidian welcome boilerplate; no H1 |

### Context registry

`SoulsLikeGameVault/ai/Skill_Context_Index.md` is an allow-list, not a vault directory index.

| Key | Note | Authority | Intended trigger |
|---|---|---|---|
| `ui-code` | `ui/UI_Code_Build_Guide.md` | Required | UI controllers, presenters, views, prefabs, and Addressables |
| `inventory-ui` | `ui/Inventory UI-UX Architecture & Unity Implementation Guide.md` | Advisory | Inventory UI layout, state, focus, and input |
| `equipment-ui` | `ui/Equipment UI-UX Architecture & Unity Implementation Guide.md` | Advisory | Equipment UI layout, comparison, navigation, and state |

The registry explicitly excludes unreviewed architecture and locomotion drafts. Review whether a narrow allow-list is correct, whether more exact keys are needed, and whether required policy may safely depend on untracked files.

## Project skills

All project skills are sibling packages under `.agents/skills`. Each contains `SKILL.md` plus `agents/openai.yaml`; Graphify additionally contains eight reference Markdown files.

| Skill | Responsibility | Primary consumer |
|---|---|---|
| `graphify` | Persistent repository graph, queries, paths, reports | `graph_explorer`, parent for broad repository questions |
| `soulslike-change-review` | Read-only bounded Unity diff review | `unity_reviewer` |
| `soulslike-code-navigation` | Bounded code-path and dependency mapping | `graph_explorer` |
| `soulslike-context` | Exact-key curated vault retrieval with disk fallback | `context_curator`; conditional for other roles |
| `soulslike-csharp-change` | One bounded production C# implementation | `csharp_worker` |
| `soulslike-performance-analysis` | Evidence-first profiling analysis | `unity_profiler` |
| `soulslike-ui-workflow` | Registered UI architecture workflow | Conditional for architect, C# worker, operator, reviewer, test runner |
| `soulslike-unity-architecture` | High-risk Unity system/refactor design | `unity_architect` |
| `soulslike-unity-assets` | One bounded Unity Editor/serialized-asset mutation | `unity_operator` |
| `soulslike-validation` | One explicitly assigned validation task | `unity_test_runner` |

### Graphify references

`graphify/references/` contains:

- `add-watch.md`
- `exports.md`
- `extraction-spec.md`
- `github-and-merge.md`
- `hooks.md`
- `query.md`
- `transcribe.md`
- `update.md`

These are upstream workflow instructions, not SoulsLike project policy. Some text still references `CLAUDE.md`, which should be reviewed for relevance in this Codex/AGENTS-based repository.

## Custom agent routing

| Agent | Model / effort | Sandbox | Required workflow skill | Conditional skills |
|---|---|---|---|---|
| `context_curator` | `gpt-5.6-luna` / low | Read-only | `soulslike-context` | None |
| `graph_explorer` | `gpt-5.6-luna` / medium | Read-only | `graphify`, `soulslike-code-navigation` | `soulslike-context` with an exact key |
| `unity_architect` | `gpt-5.6-sol` / high | Read-only | `soulslike-unity-architecture` | `soulslike-context`, `soulslike-ui-workflow` |
| `csharp_worker` | `gpt-5.6-terra` / high | Workspace-write | `soulslike-csharp-change` | `soulslike-context`, `soulslike-ui-workflow` |
| `unity_operator` | `gpt-5.6-terra` / medium | Workspace-write | `soulslike-unity-assets` | `soulslike-context`, `soulslike-ui-workflow` |
| `unity_profiler` | `gpt-5.6-terra` / high | Read-only | `soulslike-performance-analysis` | `soulslike-context` with an exact key |
| `unity_reviewer` | `gpt-5.6-terra` / high | Read-only | `soulslike-change-review` | Same domain skill used by the reviewed change |
| `unity_test_runner` | `gpt-5.6-luna` / low | Read-only | `soulslike-validation` | Same domain skill used by the validation target |

All eight custom agents disable recursive subagents. The parent remains responsible for orchestration, conflict resolution, and final decisions.

## MCP topology

### Codex parent configuration

Source: `.codex/config.toml`.

| Server | Transport | State | Ownership and purpose |
|---|---|---|---|
| `unity` | Stdio: `unity mcp --project-path ...` | Enabled by default | Official Unity CLI bridge backed by `com.unity.pipeline` |
| `serena` | Stdio via `uvx`, project from CWD | Enabled by default | Live C# symbol navigation and surgical language-server operations |
| `obsidian` | Streamable HTTP on loopback port `27123` | Enabled by default | Targeted vault reads through the local Obsidian plugin |
| `graphify` | Local Python stdio server over `graphify-out/graph.json` | Disabled in parent | Broad graph navigation; exposed by the custom `graph_explorer` role |
| `unityMCP` | No command; disabled placeholder | Disabled | Legacy/alias entry retained beside the official `unity` server |

The parent enables multi-agent mode with four concurrent child slots and Terra/high defaults. Individual custom agent files override their model and effort.

### Role-specific MCP

`graph-explorer.toml` enables only the Graphify server and whitelists:

- `query_graph`
- `get_node`
- `get_neighbors`
- `god_nodes`
- `graph_stats`
- `shortest_path`

No other custom role declares a dedicated MCP server. They inherit the parent configuration unless overridden by their session/runtime rules.

### Other client configurations

| Config | Servers | Obsidian transport | Unity transport |
|---|---|---|---|
| `.agents/mcp_config.json` | `obsidian`, `unity` | HTTPS loopback port `27124` | Stdio official `unity mcp` bridge |
| `.antigravity/mcp_config.json` | `unity`, `obsidian` | HTTPS loopback port `27124` | Stdio official `unity mcp` bridge |

### Backing local state

| Path | Purpose |
|---|---|
| `.serena/project.yml` | Project-local Serena language-server configuration |
| `graphify-out/graph.json` | Current persistent Graphify graph |
| `SoulsLikeGameVault/.obsidian/plugins/obsidian-local-rest-api/` | Obsidian REST/MCP plugin code and runtime configuration |
| `SoulsLikeGameVault/.obsidian/workspace.json` | Obsidian UI/workspace state, currently modified |

## Credential and portability boundaries

No credential, certificate, or private-key value is reproduced in this package.

- The three client configurations contain an Obsidian bearer authorization value.
- The Obsidian plugin `data.json` contains its API key and TLS cryptographic material, including a private key.
- These files are currently tracked rather than represented by sanitized templates.
- Loopback endpoints reduce remote reachability but do not make the credentials read-only. The plugin can read, write, and delete vault files and execute registered Obsidian commands.
- Several MCP commands and arguments contain absolute paths under `F:/Private/SoulsLikeTemplate`.
- Graphify uses a user-specific Python executable path under `C:/Users/golin/...`.
- Codex uses insecure loopback HTTP on `27123`; `.agents` and Antigravity use HTTPS on `27124`. The split uses the plugin's two configured ports.

The reviewer should distinguish:

1. Local single-user convenience.
2. Repository cloning onto another machine.
3. Repository sharing or archival.
4. Local compromise, where filesystem access may already expose the vault.
5. Local untrusted processes, where the bearer token enables API-level mutation while Obsidian is running.

## Transitional Git state

The following state is relevant to this architecture review:

- `.codex/skills/graphify/` is recorded as deleted.
- `.agents/skills/` is new and untracked.
- All eight `.codex/agents/*.toml` files and their README are modified.
- `AGENTS.md` is modified.
- `.agents/README.md` is untracked.
- `SoulsLikeGameVault/ai/Skill_Context_Index.md` is untracked.
- `SoulsLikeGameVault/ui/UI_Code_Build_Guide.md`, which `AGENTS.md` treats as required, is untracked.
- The character architecture analysis and hitbox note are untracked.
- `graphify-out/GRAPH_REPORT.md` is modified even though `/graphify-out/` is now listed as local operational state in `.gitignore`; existing tracked files remain tracked despite the ignore rule.

Do not infer that the `.codex/skills` to `.agents/skills` migration is complete until the intended deletions/additions are committed together and verified from a clean clone.

## Known inconsistencies and review targets

1. `AGENTS.md` requires the official `unity` CLI and rejects `unity-cli`, but its asset-persistence section still instructs `unity-cli editor refresh` and `unity-cli reserialize`.
2. `GEMINI.md` is only two lines and says “Use Unity MCP,” without identifying the official bridge or the project policy hierarchy.
3. `.codex/config.toml` retains a disabled `unityMCP` alias beside the official `unity` server.
4. Obsidian uses HTTP for Codex and HTTPS for other clients without a documented canonical transport policy.
5. MCP configs duplicate server definitions and absolute project paths across three clients.
6. The Graphify parent server is disabled, while Graphify remains a mandatory first route for broad codebase questions; the custom explorer supplies it role-locally.
7. The `.codex/skills` to `.agents/skills` migration is visible only as dirty-tree deletions plus untracked additions.
8. Required skill/context policy depends on untracked files.
9. `Arhitecture` is misspelled as a folder name. Renaming it may break Obsidian links, workspace state, scripts, and agent references.
10. `PROJECT_ORGANIZATION.md` still names `MirrorMultiplayerTemplate`, suggesting stale origin text.
11. `UI_Code_Build_Guide.md` contains a machine-specific `file:///f:/...` link.
12. Four vault notes have no H1: the advanced locomotion prompt, `First Steps`, `Hitbox System`, and `Welcome`.
13. `Welcome.md` is unchanged Obsidian boilerplate.
14. Multiple architecture/specification notes exceed 1,200 lines and may duplicate or contradict one another without status metadata.
15. The context registry exposes only three UI keys and intentionally excludes character/locomotion drafts; this is safe but incomplete if it is also expected to act as a vault knowledge map.
16. `graphify-out` contains 132 Markdown artifacts, which can overwhelm broad Markdown discovery if ignore rules are bypassed.
17. The tracked Obsidian plugin state includes private cryptographic material, which is a different risk class from a loopback bearer token.
18. Imported Graphify reference text includes Claude-specific integration language that may not belong in the project-facing workflow.

## Decisions requested from GPT Pro

1. What is the single canonical policy order among `AGENTS.md`, custom-agent TOMLs, project `SKILL.md` files, the context registry, and vault notes?
2. Should `.agents/skills` fully replace `.codex/skills`, and what clean-clone acceptance check proves the migration?
3. Should Graphify remain role-only, or should the parent MCP server be enabled when broad repository questions require Graphify first?
4. Should Serena and Graphify responsibilities be expressed in one routing document rather than repeated across `AGENTS.md`, skills, and TOMLs?
5. Which MCP configuration is canonical, and should the Codex, generic-agent, and Antigravity configs be generated from one sanitized source?
6. Should the disabled `unityMCP` alias and minimal `GEMINI.md` be removed, rewritten, or retained for compatibility?
7. Which Unity CLI spelling is valid for asset refresh/reserialization, and how should the contradictory `unity-cli` instructions be corrected?
8. Should Obsidian use secure `27124`, insecure loopback `27123`, or an explicitly documented per-client split?
9. Should the Obsidian plugin `data.json` and MCP bearer configurations remain tracked? If yes, document the threat model; if no, propose a migration that does not break local clients.
10. Should `Arhitecture` be renamed to `Architecture`, and what link/workspace migration is required?
11. Which vault notes are required policy, advisory guidance, drafts, historical artifacts, or generated output?
12. Should large overlapping movement/character documents be split, consolidated, indexed, or retained with provenance/status frontmatter?
13. Should every vault folder receive an index/MOC note, or is exact-key retrieval sufficient for AI workflows?
14. Should the three-entry context registry stay narrow, expand to reviewed architecture/TODO keys, or be replaced by a hierarchical manifest?
15. How should generated Graphify reports and query memories be retained, expired, or excluded from review/search contexts?
16. Which dirty-tree changes belong to one atomic commit, and which should be separated or discarded?

## Suggested review acceptance criteria

A satisfactory redesign should make all of the following true:

- A fresh clone discovers every intended project skill without relying on untracked files.
- Every custom role resolves its required skill and only the MCP tools it needs.
- One document states the authority order and does not conflict with tool-specific files.
- Unity CLI instructions use one valid command surface throughout.
- Required vault context is committed, identifiable, and reachable by exact stable keys.
- Advisory and draft notes cannot silently override live source or required policy.
- MCP configuration has an explicit canonical source and portability strategy.
- No secret or private cryptographic material is shared accidentally.
- Obsidian hierarchy and context-registry hierarchy are clearly distinguished.
- Generated Graphify output cannot masquerade as authoritative project documentation.
- Renames and migrations include compatibility checks for Obsidian links, absolute paths, agent configuration, and clean-clone discovery.

## Evidence limitations

- The existing Graphify graph is gameplay-heavy and did not reliably map dot-directories, new untracked skills, or the current documentation/MCP topology.
- This package therefore uses direct filesystem, Git, Markdown frontmatter, TOML, and JSON inspection for configuration facts.
- Credential values and TLS key material were deliberately excluded.
- Unity was not opened and no tests or builds were run because this is a documentation/configuration review package.
