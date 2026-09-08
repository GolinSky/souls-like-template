# ProBuilder skill-source review

Reviewed on 2026-09-08. Source checkouts were created only under the local
temporary review directory
`C:\Users\golin\AppData\Local\Temp\codex-probuilder-skill-review-20260908`,
outside active skill-discovery directories and outside `Assets`. No upstream
installer, script, MCP server, global configuration, Unity command, or package
operation was run. The original ProBuilder playbooks remain inactive in that
temporary review checkout; they were not copied into `.agents/skills`.

## Active adapted skills

`unity-cli` and `unity-package-management` are compact project adaptations,
not verbatim copies. They retain only the decisions needed here: use the
official `unity` CLI and existing `com.unity.pipeline` bridge; discover the
runtime command schema; use UPM's asynchronous API; and do not force a live
Editor to exit. Their local content hashes are recorded after this file's
initial creation. The Unity Companion License notice is preserved in
[`unity-skills-LICENSE.md`](unity-skills-LICENSE.md).

| Upstream repository and source | Reviewed commit and source blob | Commit date | License | Local adaptation |
| --- | --- | --- | --- | --- |
| `https://github.com/Unity-Technologies/skills`, `skills/unity-cli/SKILL.md` | `e91a6c4f9acf252e542c1228db1fc36b5a22a8d0`; blob `95b7ddcff818c2100365b505b45aef9d7aa04777` | 2026-09-03T22:04:10-04:00 | Unity Companion License; notice blob `869c724c8cab8764b548fd5bd8cb8ea1fc849100` | Replaced broad CLI/global configuration and installer guidance with project-scoped Pipeline discovery and scene safety. |
| `https://github.com/Unity-Technologies/skills`, `skills/unity-cli/references/integration-advanced.md` | `e91a6c4f9acf252e542c1228db1fc36b5a22a8d0`; blob `40e6d6a094be32e6cb23201699f327c43a3287ee` | 2026-09-03T22:04:10-04:00 | Unity Companion License | Retained only command-schema, reconnect, and `[CliCommand]` guidance; expressly excludes live-Editor exit behavior. |
| `https://github.com/Unity-Technologies/skills`, `skills/unity-package-management/SKILL.md` | `e91a6c4f9acf252e542c1228db1fc36b5a22a8d0`; blob `facc8d7a9409f6de2642a265cab2dffcde296649` | 2026-09-03T22:04:10-04:00 | Unity Companion License | Replaced its headless bootstrap examples with supported live-Editor UPM workflow and verification. |
| `https://github.com/Unity-Technologies/skills`, `skills/unity-package-management/references/select-packages.md` | `e91a6c4f9acf252e542c1228db1fc36b5a22a8d0`; blob `09326aab01afbff9082cb305640305c67f517285` | 2026-09-03T22:04:10-04:00 | Unity Companion License | Narrowed generic package catalogue to actual-package and render-pipeline compatibility decisions. |

## Inactive ProBuilder sources

| Upstream repository and source | Reviewed commit and source blob | Commit date | License | Lessons adopted by the project ProBuilder author |
| --- | --- | --- | --- | --- |
| `https://github.com/AlexeyPerov/Unity-Open-MCP`, `skills/extensions/probuilder/SKILL.md` | `a5c2f520539705bc1bb30193a5071b008f29a793`; blob `4e767fe7d2ae1686f0dcaab47cb597c6e4470033` | 2026-08-28T22:37:59+03:00 | MIT; notice blob `3c7fe049043edfff9364e3caa968f18c12ce18f0`, preserved in [unity-open-mcp-LICENSE](unity-open-mcp-LICENSE) | Inspect topology before a face edit; choose semantic normal direction for ordinary face operations and indices only after inspection; re-resolve session-local targets after reload. Its `unity_open_mcp_*` activation and tool names are not project contracts. |
| `https://github.com/batihandev/unity-mcp-skills`, `skills/probuilder/SKILL.md` | `090c8e3eaefa23d57a98e237572a4ffbd19df444`; blob `889caeedf11935004993b8a1d9429714a9389a7f` | 2026-08-01T13:05:03+03:00 | MIT; notice blob `f00dfa3a504257485d567db602a725b7099111d4`, preserved in [unity-mcp-skills-LICENSE](unity-mcp-skills-LICENSE) | State explicit dimensions and pivot contract; use bounded batch construction only when supported; distinguish whole-object from face material edits. Its `Unity_RunCommand`, `IRunCommand`, `Unity_PackageManager_ExecuteAction`, and fixed `6.0.9` requirement are not project contracts. |

## Review result

The upstream Unity package skill supplies a headless `Client.AddAndRemove`
example that calls `EditorApplication.Exit(...)`; that pattern is unsafe for a
connected user Editor and is excluded from the active project skills. The
ProBuilder sources require different MCP server/tool contracts. They are source
material only and do not authorize installation of Unity Open MCP, Unity AI
Assistant MCP, another server, or global settings changes.

Local adapted-copy hashes (SHA-256, fetched/adapted 2026-09-08):

| File | SHA-256 |
| --- | --- |
| `.agents/skills/unity-cli/SKILL.md` | `8e639c863a1a03fed909db544b7107759a8a881ba296d235c07ebaeac724cc0a` |
| `.agents/skills/unity-cli/references/integration-advanced.md` | `db1c43ca24fcef6cee93261b5dfc3cdc14f5549b28deae18785caa0fc04a48f9` |
| `.agents/skills/unity-package-management/SKILL.md` | `b976173c92293bc142bc7795ab12822b8ba39907651393a3032e1b28e0dfe9c3` |
| `.agents/skills/unity-package-management/references/select-packages.md` | `22c88220d5fd971e257d933d4f7a24146eeaf32146fc50a031dfbdd2a754df9f` |
