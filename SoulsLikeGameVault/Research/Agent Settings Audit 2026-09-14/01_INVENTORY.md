# Inventory

The detailed manifest is inventory_manifest.json: 436 selected files with absolute path, scope, size, SHA-256, timestamp, resolved path, and symlink status. It records presence/hashes; content-review depth is narrower.

## Configuration and instructions

| Asset | Status and ownership |
|---|---|
| C:/Users/golin/.codex/AGENTS.md | Personal instructions; 4,052 bytes, 98 lines; present in rendered root/nested/outside prompts |
| F:/Private/SoulsLikeTemplate/AGENTS.md | Repository authority; 20,176 bytes, 305 lines; present through final marker in root/nested prompts |
| GEMINI.md | Repository adapter; 629 bytes, 10 lines; not a Codex fallback because no fallback filenames configured |
| Personal .codex/CLAUDE.md | Present; not selected by this Codex instruction chain; not an inferred native config surface |
| Personal .claude/CLAUDE.md | Explicit Graphify adapter; preserved; Claude invocation not tested |
| Personal .gemini/GEMINI.md | Empty; preserved |
| Personal .codex/config.toml | Parsed; app and user settings, global MCP, plugin declarations, trust, hook trust hashes |
| Repository .codex/config.toml | Parsed; project MCP, agent defaults; tracked |
| Repository .agents/mcp_config.json | Parsed; another client adapter for vault and Unity; not a native Codex layer |
| Repository .agents/rules/soulslike-vault.md | Supporting other-client prose; not a Starlark execution rule |
| Personal .codex/rules/default.rules | 15 prefix rules; checker loads them; includes broad MCP add/login allowances |
| .codex/agents/*.toml | Eight valid TOML definitions, discovered in active task role catalog |
| .codex/agents/README.md | Claims exclusive role MCP access; contradicted by inherited runtime capabilities |

No global/project AGENTS.override.md, nested AGENTS.md, alternate profile file, or subagents.toml was found in the scoped scan. No optional file is proposed merely because it is absent.

## Skills

Fresh CLI skills/list returned 45 entries for root and nested cwd: 26 repository, 13 user, 6 system. Outside the repository it returned 19. No parser/discovery errors were returned. Desktop connector/plugin skill catalog is broader than the standalone CLI probe; the CLI count is not the total desktop catalog.

Repository packages: Graphify; 12 Penpot skills; 13 SoulsLike/Unity workflows. Their resource trees were inventoried. Entry-point and linked-resource review found one broken ProBuilder link. Personal Graphify and the legacy unity-mcp-orchestrator are both discovered and enabled in the project.

| Skill | Size / relevant state |
|---|---|
| Project graphify | 41,318 bytes; Codex-specific extraction changes; broad description; references exist |
| Personal graphify | 41,000 bytes; same name, different host Task-tool extraction contract |
| Personal unity-mcp-orchestrator | 14,170 bytes; legacy Coplay tools; two missing resource links |
| soulslike-context | 2,231 bytes; deterministic registry routing and disk fallback |
| soulslike-code-navigation | 834 bytes; focused project navigation wrapper |
| soulslike-probuilder | 2,747 bytes; SOURCES.md exists; references/command-contract.md is absent |

A 41 KB skill body is not an AGENTS byte-budget violation. Optional agents/openai.yaml is not required for every skill. Vendor-owned files are retained.

## Agent matrix

| Role | Declared model / effort | Declared sandbox | Explicit server narrowing |
|---|---|---|---|
| context_curator | Luna / low | read-only | None |
| graph_explorer | Luna / medium | read-only | Graphify enabled with six-tool allowlist; Unity, Serena, vault, Obsidian, legacy Unity disabled |
| unity_architect | Sol / high | read-only | None |
| csharp_worker | Terra / high | workspace-write | None |
| unity_operator | Terra / medium | workspace-write | None |
| unity_profiler | Terra / high | read-only | None |
| unity_reviewer | Terra / high | read-only | None |
| unity_test_runner | Luna / low | read-only | None |

All eight declare agents.enabled=false. Roles have distinct responsibilities and single-writer rules. These are good boundaries to preserve. Unspecified MCP/connector settings inherit; Graphify's allowlist does not restrict every other global service.

## MCP capability register

| Service | Effective root state / provenance | Capabilities and validation |
|---|---|---|
| unity | Enabled, repository, official unity mcp | 143 advertised tools including mutations; correct Editor status and scenes queried via CLI |
| serena | Enabled, repository, uvx serena-agent==1.7.0, cwd-based project activation | Live C# read/write tools advertised; no code operation needed |
| soulslike-vault | Enabled, repository, pinned local MCPVault 0.16.0 Node script + exact vault path | Read/write note tools; harmless outline read passed |
| graphify | Disabled for parent; enabled in graph_explorer config | uvx graphifyy[mcp]==0.9.54; six read tools allowed; graph query logging disabled |
| obsidian | Disabled, repository | Inline Authorization binding present in tracked config; report-only |
| unityMCP | Disabled by repository override; enabled globally outside repository | Legacy server retained for possible other-client/project use; not invoked |
| context-mode | Enabled globally; installed npm shim | Executes/indexes context; distinct purpose from Context docs server |
| context | Enabled globally; installed npm shim | Versioned documentation server; not duplicate of context-mode |
| context7 | Enabled globally; npx unpinned package, environment key reference | Current docs; no package install/run requested by audit |
| deepwiki | Enabled globally; HTTP | Read documentation tools; no health probe needed |
| cv_filesystem | Enabled globally, rooted at F:/CvCreationProject | 14 read/write tools exposed in unrelated SoulsLike task and curator |
| penpot | Enabled globally, HTTP with inline query credential | Four advertised tools; not invoked during config audit |
| github MCP | Disabled globally | Separate GitHub connector remains available; not an accidental double enabled MCP registration |
| node_repl / cua_repl | User/app/plugin managed | Browser/computer runtime tools; keep manifests and hook wiring intact |
| Connector plugins | Desktop-managed, broader than CLI catalog | Google Drive fetch succeeded; remote write tools advertised but not used |

Every configured local executable checked resolved, and referenced absolute MCPVault paths existed. Presence/advertisement is not a complete service health test. STDIO auth_status=unsupported from mcp list is not a connection failure.

## Hooks and sync

Global notify points to the installed computer-use turn-ended helper. Browser, Chrome, Computer Use and unified Computer Use manifests have Stop/Interrupt/SubagentStop cleanup hooks. The first three overlap on node_repl; unified uses cua_repl. No user-authored recursive Codex startup command was found. Duplicate hook execution or cost was not measured; do not remove vendor hooks speculatively.

The agy 1.3.0 manifest has five default prompts; 0.154.0 warns that a maximum of three is supported and ignores the field. Two icon metadata warnings were also observed, without a reliable owning-path attribution.

Repository .codex and .agents are ordinary directories, not symlinks; project policy files are Git tracked. Personal config is an installed file; its upstream sync source is unresolved. Windows absolute paths are real host bindings, not automatically invalid syntax.

