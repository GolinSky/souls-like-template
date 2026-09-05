---
title: Obsidian MCPVault Setup Result
status: completed
provider: mcpvault
date: 2026-09-05
---
# Obsidian MCPVault Setup Result

## Executive Summary
The local STDIO MCPVault bridge (@bitbonsai/mcpvault) has been installed and configured for both Antigravity and OpenAI Codex in the SoulsLikeTemplate workspace. Direct file-based note discovery, outline inspection, reading, searching, writing, patching, and metadata management are now fully active without requiring the Obsidian desktop application to be running.

## Configuration Details
- **Installed Package**: @bitbonsai/mcpvault@0.16.0
- **Node.js Runtime**: C:\Program Files\nodejs\node.exe (v24.14.1)
- **MCP Entrypoint**: C:\Users\golin\AppData\Local\AI-Tools\mcpvault\0.16.0\node_modules\@bitbonsai\mcpvault\dist\server.js
- **Vault Location**: F:\Private\SoulsLikeTemplate\SoulsLikeGameVault
- **MCP Server Name**: soulslike-vault
- **Transport**: stdio

## Modified Local Configurations
1. **Antigravity Workspace**:
   - F:\Private\SoulsLikeTemplate\.agents\mcp_config.json (configured soulslike-vault, retired old REST obsidian entry)
   - F:\Private\SoulsLikeTemplate\.antigravity\mcp_config.json (configured soulslike-vault, retired old REST obsidian entry)
2. **Codex Local Settings**:
   - F:\Private\SoulsLikeTemplate\.codex\config.toml (added [mcp_servers.soulslike-vault], disabled retired REST [mcp_servers.obsidian])
   - F:\Private\SoulsLikeTemplate\.codex\agents\graph-explorer.toml (disabled [mcp_servers.soulslike-vault])
3. **Agent Rules**:
   - Added: F:\Private\SoulsLikeTemplate\.agents\rules\soulslike-vault.md (Always On workspace rule)
   - Amended: F:\Private\SoulsLikeTemplate\AGENTS.md (updated UI and Animation workflows to use soulslike-vault without manual Obsidian launch)

## Backup Location
Timestamped backup directory:
C:\Users\golin\AppData\Local\AI-Tools\mcpvault\backups\20260905_212510
Backed up files:
- gents_mcp_config.json
- ntigravity_mcp_config.json
- config.toml
- graph-explorer.toml
- gemini_antigravity_mcp_config.json
- AGENTS.md

## Acceptance Test Matrix (Obsidian Closed)
All tests executed via MCP protocol JSON-RPC against soulslike-vault with the Obsidian desktop application closed.

| Test | Status | Evidence |
|---|---|---|
| List and Read | **PASS** | Successfully listed vault root dirs and read Welcome.md |
| Create | **PASS** | Created ToDo/_mcpvault_test_<id>.md with frontmatter status: test, search marker, and [[Welcome]] |
| Search | **PASS** | BM25 content search matched marker in newly created note |
| Edit and Metadata | **PASS** | Patched body string, updated frontmatter to status: passed, added mcpvault-test-tag, preserved wikilink |
| Cleanup | **PASS** | Safely deleted disposable test note using confirmation parameter |
| Process Independence | **PASS** | Obsidian process remained stopped throughout; zero Obsidian processes detected |
| Fresh-session, Obsidian-closed Verification | **PASS** | Spawns via Node STDIO independently of desktop application or network ports |
