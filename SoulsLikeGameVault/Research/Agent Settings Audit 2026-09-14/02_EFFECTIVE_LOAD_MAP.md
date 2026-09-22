# Effective load map

## Observed scenarios

The read-only 0.154.0 app-server protocol was generated from the installed executable, then config/read, configRequirements/read and skills/list were called. No thread/start or turn/start was sent. Separately, codex debug prompt-input rendered the actual prompt inputs for each cwd without a model turn.

| Scenario | Instruction evidence | Config evidence | Skills |
|---|---|---|---|
| Repository root | Global + repository headings and final project marker present | Repository .codex > user config > empty system layer; profile null | 45 entries |
| Assets/Scripts/Items | Same chain; no nested instruction/config override found | Same repository/user/system layers | 45 entries |
| C:/Users/golin/Documents | Global instructions present; repository heading/marker absent | User > empty system; no project agent defaults | 19 entries |

Raw source bytes: 4,052 + 20,176 = 24,228, before merge separators. Config reports project_doc_max_bytes=32,768 and no fallback filenames. No truncation was observed. The rendered JSON character counts (63,791 root; 63,983 nested; 30,541 outside) include other context and encoding; they are not instruction-byte or token counts.

## Configuration provenance

| Value | Winning source |
|---|---|
| model=gpt-6-astra; reasoning=xhigh | Personal config |
| approvals_reviewer=user | Personal config |
| agents.enabled=true; max_concurrent_threads_per_session=4 | Repository config |
| default subagent Terra/high | Repository config; individual role files override |
| official Unity/Serena/vault | Repository config |
| legacy unityMCP disabled | Repository override over global transport |
| CV filesystem and Penpot | Global config inherited into repository |
| approval_policy and sandbox_mode | Null in config/read: unset in these file layers; do not interpret null as actual runtime mode |
| Current task approval never / danger-full-access | Live task runtime, separately observed |
| Curator sandbox | Role file read-only; live child reports parent danger-full-access/never and broad tools |

The system layer was represented by C:/ProgramData/OpenAI/Codex/config.toml with an empty-object hash. configRequirements/read returned null. This establishes the fresh diagnostic client's result, not every policy of an existing desktop task.

## Separate precedence rules

Instructions select at most one candidate per directory and combine root toward cwd. Config layers merge settings. Skill duplicates remain separate discoveries. Execution rules use restrictive matching. Roles inherit unspecified configuration and may receive parent runtime permission overrides. No single “local always wins” rule describes all five surfaces.

Both Graphify copies and the personal legacy Unity skill appear in root/nested skills/list and in the current desktop skill catalog. That proves availability, not which one an ambiguous future prompt would choose. Project Graphify's source contains Codex-specific execution changes; personal Graphify is not a safe inferred substitute.

## Role observation

The curator used in this audit reported 536 available tool metadata entries, including 143 Unity, 18 vault, 14 CV filesystem, Serena mutations, connector writes and local shell/patch tools. It declared no mutations and none were requested. Its initial statement that vault MCP was unavailable was corrected after direct metadata inspection; the parent then successfully queried a vault outline.

The graph explorer reported its Graphify MCP tools unavailable in its child session, despite configured enablement. Parent inspection found the CLI absent from PATH but an interpreter marker and existing graph. A bounded in-memory graph filename-vocabulary query found zero indexed .codex/.agents/AGENTS source hits. Live files therefore supplied the configuration evidence. No graph rebuild or query-memory write was performed.

Actual tool filtering behavior for all eight roles is not established. Current role exposure is not proof that a remote write would be approved or succeed.

## Other clients and portability

GEMINI.md delegates project policy to AGENTS.md. .agents/mcp_config.json supplies explicit project-path Unity routing for another client. Neither is counted as a native Codex config layer. Gemini global settings contain Context Mode hooks and an Obsidian binding; no Gemini session was started. Claude adapter was preserved without testing its loader.

Windows paths resolve on this host. macOS deployment, settings-source mapping and regeneration are unresolved; no Mac repair is claimed.

