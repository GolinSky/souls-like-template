# Antigravity + Obsidian: no manual Obsidian startup

**Target:** Windows 11, local Antigravity agent.  
**Vault:** `F:\Private\SoulsLikeTemplate\SoulsLikeGameVault`  
**Project:** `F:\Private\SoulsLikeTemplate`  
**Prepared:** 2026-09-05. This is an implementation plan, not a claim that the machine has been configured or tested.

## Decision — implement this setup only

Use **MCPVault by bitbonsai**, package **`@bitbonsai/mcpvault`**, as a local STDIO MCP server. Antigravity starts its server process; the server reads and writes the existing vault directly. Obsidian does not need to start. MCPVault is a community project, not an official Obsidian product. [1][2][3]

```text
Antigravity
    └── starts MCPVault automatically through STDIO
            └── reads/writes SoulsLikeGameVault on F:

Obsidian: optional viewer/editor, not a prerequisite for the agent
```

Do **not** install the Obsidian CLI, a CLI-to-MCP wrapper, a second filesystem MCP, an HTTP bridge, or an Obsidian startup service for this task. Do not add `obsidian --vault-path` or any Obsidian launch command to the normal agent workflow. The official CLI still depends on the desktop application; it is unnecessary for this solution. [4]

This implements the direct-file approach in the supplied `obsidian-mcp-alternatives.md`, specifically its MCPVault alternative. It does not depend on that file's REST API or CLI claims.

**Scope:** normal local note reading, writing, searching, patching, and frontmatter/tag updates. This is not a replacement for running plugin commands, reading the active editor tab, or executing Dataview. Do not assume filesystem moves repair incoming wikilinks. This setup also does not configure cross-PC synchronization or access from a separate chat service.

## 1. Inspect the existing setup and back up only what will change

Work in native Windows PowerShell with native Windows Node.js. Do not pass the Windows vault path to a WSL or remote MCP process.

1. Confirm the vault and project directories exist. Never create a replacement vault when the expected path is missing.
2. Identify the installed Antigravity product/version and its **active** MCP configuration. In the IDE, use **MCP Servers → Manage MCP Servers → View raw config** where available. Current documentation lists the user configuration at `~/.gemini/config/mcp_config.json` and workspace configuration at `.agents/mcp_config.json`. An older installation may differ; use the configuration actually loaded by the installed client, not a guessed path. [2]
3. Inspect existing Obsidian MCP entries and relevant agent rules. Identify entries that depend on the Obsidian REST plugin. Do not print API tokens or entire secret-bearing configurations into reports.
4. Preserve the existing configuration scope when replacing an Obsidian integration. If none exists, use the confirmed user-level configuration. Do not register the same vault server at both user and workspace scopes.
5. Make timestamped backups of every configuration/rule file that will be edited. Store backups under `%LOCALAPPDATA%\AI-Tools\mcpvault\backups`, outside the Unity repository. Record the initial Git status; do not commit, reset, or discard existing user changes.

Do not alter Obsidian's `.obsidian` folder, uninstall its plugins, rotate keys, or change other clients' MCP configurations. Do not remove the old connection yet.

## 2. Install MCPVault once, outside the Unity repository

Use an existing supported Node.js installation that meets the selected package's `engines` requirement. If Node.js is missing, install the current official LTS using the machine's existing package/version-management approach. Do not blindly replace a working Node installation used by other tools. [5][9]

**Installation choice:** install an exact package version into a dedicated local folder, then launch its installed JavaScript entry with the absolute `node.exe` path. This avoids runtime `npx` downloads, executable-shim ambiguity, and shell quoting in the MCP configuration. It is a deployment choice for this plan, rather than a required MCPVault installation method.

For the first installation, run the following in one PowerShell session. On subsequent runs, reuse the recorded working version instead of automatically upgrading it.

```powershell
$ErrorActionPreference = 'Stop'
$Vault = 'F:\Private\SoulsLikeTemplate\SoulsLikeGameVault'
$Package = '@bitbonsai/mcpvault'

if (-not (Test-Path -LiteralPath $Vault -PathType Container)) {
    throw "Expected vault not found: $Vault"
}

$Node = (Get-Command node.exe -ErrorAction Stop).Source
$Npm = (Get-Command npm.cmd -ErrorAction Stop).Source
& $Node --version
if ($LASTEXITCODE -ne 0) { throw 'Node.js could not run.' }

# Inspect registry metadata; resolve latest only during initial installation.
$Raw = & $Npm view "$Package@latest" --json
if ($LASTEXITCODE -ne 0) { throw 'Could not retrieve MCPVault package metadata.' }
$Meta = ($Raw -join "`n") | ConvertFrom-Json
if ($Meta.name -ne $Package -or
    [string]$Meta.repository.url -notmatch 'github\.com[:/]bitbonsai/mcpvault') {
    throw 'Unexpected MCPVault package identity. Review before installing.'
}

$Version = [string]$Meta.version
if ([string]::IsNullOrWhiteSpace($Version)) { throw 'Package version is missing.' }
Write-Output "Selected MCPVault: $Version"
Write-Output "Required Node.js: $($Meta.engines.node)"

$ToolDir = Join-Path $env:LOCALAPPDATA "AI-Tools\mcpvault\$Version"
New-Item -ItemType Directory -Path $ToolDir -Force | Out-Null
& $Npm install --prefix $ToolDir --save-exact --engine-strict "$Package@$Version"
if ($LASTEXITCODE -ne 0) {
    throw 'MCPVault installation failed. Resolve the reported cause before continuing.'
}

$PackageDir = Join-Path $ToolDir 'node_modules\@bitbonsai\mcpvault'
$Installed = Get-Content -Raw -LiteralPath (Join-Path $PackageDir 'package.json') |
    ConvertFrom-Json
if ($Installed.name -ne $Package -or $Installed.version -ne $Version) {
    throw 'Installed package does not match the selected package/version.'
}

# Derive the executable entry from the installed package, not a guessed path.
$Bin = if ($Installed.bin -is [string]) { $Installed.bin } else { $Installed.bin.mcpvault }
if ([string]::IsNullOrWhiteSpace([string]$Bin)) { throw 'MCPVault executable entry is missing.' }
$Entry = [IO.Path]::GetFullPath((Join-Path $PackageDir $Bin))
if (-not (Test-Path -LiteralPath $Entry -PathType Leaf)) {
    throw "MCPVault executable entry not found: $Entry"
}

# Generate a MERGE SNIPPET with real machine paths. Do not replace the whole config.
$Snippet = @{
    mcpServers = @{
        'soulslike-vault' = @{
            command = $Node
            args = @($Entry, $Vault)
        }
    }
} | ConvertTo-Json -Depth 8
$SnippetPath = Join-Path $ToolDir 'antigravity-mcp-snippet.json'
[IO.File]::WriteAllText($SnippetPath, $Snippet, [Text.UTF8Encoding]::new($false))
Write-Output "Configuration merge snippet: $SnippetPath"
```

The package exposes an executable through its `bin` metadata. npm supports exact-version installation and lockfiles. Retain the generated `package.json` and `package-lock.json`; record the resolved executable paths and package version. [5][6]

## 3. Connect Antigravity to the installed server

Merge **only** `mcpServers.soulslike-vault` from the generated snippet into the active configuration identified in step 1. Preserve unrelated servers and other configuration fields. Use a JSON-aware edit, validate the result, and avoid rewriting unrelated data.

The entry must launch the absolute `node.exe` path with two arguments: the installed MCPVault entry file and the exact vault directory. This is a STDIO configuration: no server URL, HTTP port, API token, certificate, or separate terminal is required. Antigravity supports `command` and `args` for this transport. [2][3]

Refresh MCP servers in Antigravity and inspect the actual discovered tools. Confirm `soulslike-vault` provides the note operations needed below. Tool schemas from the installed server are authoritative; do not invent arguments or require a particular total tool count. [7]

Do not run the MCP server in a terminal and wait for it to exit as a health test. It is a protocol process, not a one-shot command. A successful MCP connection and actual tool calls are the tests.

If the current session cannot reload new tools, record the completed work and the exact pending verification before requesting a **one-time Antigravity reload**. Do not terminate the active client unexpectedly or claim tests passed before the reloaded client can call the tools.

## 4. Set the agent's default vault behavior

Add or update one **Always On workspace rule** through Antigravity's active rules mechanism. Current documentation uses `.agents/rules` and also supports the older `.agent/rules`. Reuse the existing active convention; do not create duplicate rules in both places. [8]

Use this content, preserving unrelated existing instructions:

```markdown
# SoulsLikeGameVault access

These rules apply to the SoulsLikeTemplate project and its vault.

Vault: F:\Private\SoulsLikeTemplate\SoulsLikeGameVault
Primary note server: soulslike-vault (MCPVault)

Use this server for normal note discovery, reading, content search,
creation, patching, frontmatter updates, and tag updates.
Use vault-relative paths in tool calls, for example ToDo/MyPlan.md.

Do not start Obsidian, check a REST endpoint, call the Obsidian CLI,
or ask the user to open Obsidian before ordinary note work.

Read existing notes before editing. Preserve unrelated content,
frontmatter, wikilinks, folder layout, and naming conventions.
Do not restructure the vault or edit .obsidian.

Do not assume a file rename updates incoming links. For renames of
existing linked notes, inspect and deliberately update affected links,
or defer the operation rather than silently breaking references.
Do not delete existing user notes without explicit authorization.

Do not claim MCPVault ran an Obsidian plugin command or accessed the
active editor tab. Report such operations as outside this integration.

If the MCP connection fails, inspect its configuration/logs first.
Do not silently substitute shell access and report the MCP as working.
```

Keep the client's existing tool-approval policy. A deletion tool's confirmation argument is not a substitute for user authorization. Do not enable unrestricted approval for every MCP server.

## 5. Prove the exact everyday workflow works

First close Obsidian gracefully for this **one-time acceptance test**. Do not force-kill it or discard unsaved work. Check that no Obsidian process remains:

```powershell
Get-Process -Name Obsidian -ErrorAction SilentlyContinue
```

No output means no process with that name was found. Check again after the tests. Do not open Obsidian to make a failing test pass.

Perform these operations through **the actual `soulslike-vault` MCP tools**, not shell file commands. Discover their schemas before calling them.

| Test | Required evidence |
|---|---|
| List and read | List the vault root and read one existing note without changing it. |
| Create | Create `ToDo/_mcpvault_test_<unique-id>.md`; confirm the path does not already exist. Put a separate random search marker in the body, a wikilink, and `status: test` in frontmatter. |
| Search | Use content search for the body-only marker; the new note must be returned. |
| Edit and metadata | Patch the test body, update frontmatter to `status: passed`, and add a test tag. Read back and verify the expected changes and unchanged wikilink. |
| Cleanup | Delete only the disposable note created by this run, using the installed tool's confirmation schema. Never delete the containing `ToDo` folder or any pre-existing note. |
| Process independence | Confirm Obsidian remained stopped throughout. Do not count a result produced by the old REST-backed connection. |

If the expected `ToDo` folder is absent, use a unique temporary note at the confirmed vault root rather than creating a new permanent folder structure.

Inspect the Git diff/status against the baseline. Aside from the deliberately added agent rule and setup report, existing project/vault files must remain unchanged. Verify `.obsidian` was not edited by this installation; compare after graceful shutdown and before any optional visual reopening.

## 6. Retire the old connection and verify a fresh session

**Only after step 5 passes**, disable the old REST-backed Obsidian MCP entry for this vault **in Antigravity**. Use the installed client's supported disable mechanism; keep the backup. Remove or amend only the relevant Antigravity rules that still require starting Obsidian or calling the old server. Leave other servers, other clients, and the actual Obsidian plugin installation untouched.

Refresh and repeat a read through `soulslike-vault`. Then verify from a fresh Antigravity session, with Obsidian still closed and without manually running MCPVault. The client must launch/connect to MCPVault on its own. Record this test as pending if a client restart cannot be completed within the current session; configuration alone is not proof.

Do not add Windows login tasks, a scheduled service, an HTTP listener, or an Obsidian auto-launch fallback to mask a failed connection.

### Completion report

Create a concise report through MCPVault at `ToDo/OBSIDIAN_MCPVAULT_SETUP_RESULT.md` when that folder exists; otherwise use the vault root. Update an existing report carefully rather than discarding its content.

Include the installed version, Node path, MCP entry/configuration location, backup locations, rule location, old entries disabled, and a PASS/FAIL/PENDING result for every acceptance test. Redact secrets. State explicitly whether the **fresh-session, Obsidian-closed test** passed. If the MCP is broken, save the report in the local tooling directory instead and report its exact path.

### Rollback

On failure, remove/disable only the newly added entry and undo only this plan's rule changes. Restore the old entry from its backup without overwriting unrelated edits made in the meantime. Do not delete the vault, reset Git, or uninstall other software.

## Opening the Obsidian interface later

This is optional and is **not a startup prerequisite**. For the already registered vault, the agent can open/focus its interface using the documented Obsidian URI: [10]

```powershell
Start-Process 'obsidian://open?vault=SoulsLikeGameVault'
```

Use a verified vault ID instead if multiple registered vaults have the same name. Do not execute this during the closed-app acceptance tests. No Obsidian CLI installation is needed for this URI command.

## Final success condition

**Open Antigravity → ask it to read or write a vault note → the operation succeeds through MCPVault while Obsidian stays closed.**

The existing vault stays where it is. Obsidian remains available when the user chooses to view or edit notes visually.

## Sources and verification scope

The supplied MD provided the starting alternative. The configuration details below were checked against primary project/vendor documentation on 2026-09-05. Paths and package versions must still be verified on the actual machine. Installation commands and acceptance tests above are proposed implementation steps, not remotely executed results.

1. MCPVault features and closed-app behavior: https://mcpvault.org/features/
2. Google Antigravity MCP configuration: https://antigravity.google/docs/mcp
3. MCP STDIO process lifecycle: https://modelcontextprotocol.io/specification/2025-06-18/basic/transports
4. Official Obsidian CLI requirements: https://obsidian.md/help/cli
5. MCPVault package metadata; use installed/registry metadata for the selected version: https://github.com/bitbonsai/mcpvault/blob/main/package.json
6. npm installation and exact-version options: https://docs.npmjs.com/cli/v11/commands/npm-install/
7. MCPVault tool reference: https://github.com/bitbonsai/mcpvault/blob/main/README.md
8. Google Antigravity rules: https://antigravity.google/docs/rules-workflows/
9. Official Node.js distribution: https://nodejs.org/en/download
10. Obsidian URI reference: https://obsidian.md/help/uri
