# Scope and environment

Audit date: 14 September 2026. Mode: read-only configurations; staged proposals only. No live repairs.

## Included installation

| Surface | Verified observation |
|---|---|
| Host | Native Windows 10.0.26200, x86_64; PowerShell; Europe/Kiev |
| Repository | F:/Private/SoulsLikeTemplate |
| Source commit | e5180a25173576f4e7219cbe34a8bf93540a8608 |
| CODEX_HOME | Environment override unset; client initialize resolves C:/Users/golin/.codex |
| PATH CLI | npm shim C:/Users/golin/AppData/Roaming/npm/codex.ps1; codex-cli 0.154.0 |
| App-associated CLI | C:/Users/golin/AppData/Local/OpenAI/Codex/bin/bffc5354119c8421/codex.exe; 0.154.0-alpha.6.2 |
| Windows app package | OpenAI.Codex 26.908.4834.0; browser runtime config reports 26.908.40834. These are distinct version surfaces. |
| Configured parent default | gpt-6-astra, xhigh, from personal config; project prose asks for GPT-5.6 Sol High |
| Profiles | No personal *.config.toml profile files found; config/read reports profile null |
| Trust | Existing repository trusted; not changed |
| Managed requirements | Fresh 0.154.0 configRequirements/read returns requirements null; system layer resolves empty. Does not establish policy on another host. |
| Current task permissions | Runtime instructions explicitly state danger-full-access and approval never; this overrides expectations based on role files |
| Unity | CLI 1.0.0-beta.6; Pipeline 0.5.0-exp.1; Editor 6000.3.11f1, correct repository, ready |
| Scene | ElevatorDemo, loaded/active, isDirty=false, Play Mode stopped |

## Inputs read

All three requested Drive Markdown documents were fetched through the authenticated Google Drive connector. Complete readable contents are saved in sources/. Their provider IDs and titles match the request. The research package is treated as audit guidance, not authority to execute its repair prompt.

The audit independently reopened official OpenAI documentation on 14 September. The Astra article that was inaccessible during the earlier research was retrieved successfully; its page gives 11 September 2026 as publication date. See 08_SOURCE_REGISTER.md.

## Explicit filesystem scope

Repository AGENTS/GEMINI, .codex, .agents, selected registered vault headings, personal Codex config/instructions/rules, personal Graphify and legacy Unity skill packages, relevant plugin manifests, npm launcher shims, and shallow metadata for plausible settings-source locations.

No recursive scan of the home directory, whole vault, dependency caches, Git object storage, Unity Library/Temp, browser stores, authentication files, session histories, or logs. Selected declarative plugin metadata was inspected; plugin caches were not exhaustively audited. The machine-readable manifest inventories selected roots; it is not a claim that all resource bodies were read.

Source-repository discovery checked shallow directory listings of F:/Private, F:/, C:/Users/golin, source/repos, Documents, RiderProjects, F:/golin/Documents, and F:/Users/golin. No codex-dotfiles checkout or sync mapping was established. This is bounded discovery, not proof none exists.

## Existing work and exclusions

Pre-existing modifications: three font assets; ElevatorEndpoint.cs; ElevatorShaftHazard.cs; ItemDatabase.cs; ItemDefinition.cs; ProBuilder Settings.json; untracked item tests and Design/. They were preserved. This audit made no C# or serialized-asset edits. Additional unrelated gameplay/UI files changed concurrently during the audit; they were neither reverted nor incorporated into these proposals. All 436 inventoried configuration/skill/adapter files still matched their recorded hashes at final verification.

Excluded from repair: all Obsidian configuration/credentials/certificates/ports/plugin state; accounts and endpoint changes; installations/upgrades; publishing; repository resets; model switching; unrelated projects. Existing Obsidian bindings were inspected only to record presence and ownership.

macOS, WSL, remote machines, settings sync, and Gemini/Claude runtime loading were not tested. Existing adapters were inspected and preserved.

## Diagnostic disclosure

An initial config diagnostic redacted header values but failed to redact a Penpot URL parameter named userToken; that value appeared in this task's tool output. Subsequent diagnostics redact all URL queries. Saved audit artifacts contain no configured credential values. Do not share the raw task transcript without removing that diagnostic. No credential validity, third-party access, or external compromise was tested or established. Credential changes remain a separately authorized operation.

Only local audit files, source copies, and a draft plan were authored. Read-only CLI/app-server diagnostics can create ordinary client bookkeeping; no model turn or remote write was requested by those probes.
