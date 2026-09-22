---
title: Serena MCP Setup — Windows and macOS
status: current
tags:
  - ai
  - mcp
  - serena
  - setup
updated: 2026-09-07
type: guide
domains:
  - tooling
  - ai
authority: advisory
aliases:
  - Serena_MCP_Setup_Windows_macOS
---
# Serena MCP setup — Windows 11 and macOS

## Purpose

This project uses a single portable Serena MCP definition in `.codex/config.toml`. Do not commit an operating-system-specific `DOTNET_ROOT` to the project configuration. Serena runs on the current Codex host, so Windows and macOS must each resolve `uvx` and `dotnet` from their own `PATH`.

Serena `1.7.0` uses the Roslyn Language Server for this project's C# code. The current Serena documentation requires .NET 10 or newer for that backend.

## Portable project configuration

Use this configuration on both Windows 11 and macOS:

```toml
[mcp_servers.serena]
command = "uvx"
args = [
  "-p", "3.13",
  "--from", "serena-agent==1.7.0",
  "serena", "start-mcp-server",
  "--context", "codex",
  "--project-from-cwd",
  "--enable-web-dashboard", "false",
]
startup_timeout_sec = 60
tool_timeout_sec = 120

[mcp_servers.serena.env]
SERENA_HOME = ".serena/home"
PYTHONUTF8 = "1"
```

Why it is portable:

- `uvx` is resolved from the host's `PATH`.
- `--project-from-cwd` activates the trusted repository opened by Codex.
- `SERENA_HOME` is relative to the repository and contains Serena configuration, logs, and downloaded language-server files.
- `DOTNET_ROOT` is omitted, allowing each host to find its own .NET installation.

## Windows 11 setup

1. Install Git and Codex if they are not already installed.
2. Install `uv`, which provides both `uv` and `uvx`. One supported option is:

   ```powershell
   winget install --id astral-sh.uv --exact
   ```

3. Install the .NET 10 SDK or newer. Use Microsoft's installer or:

   ```powershell
   winget install --id Microsoft.DotNet.SDK.10 --exact
   ```

4. Open a new PowerShell window and verify:

   ```powershell
   uvx --version
   dotnet --version
   Get-Command uvx
   Get-Command dotnet
   ```

5. Open the repository in Codex and trust the project so Codex loads `.codex/config.toml`.
6. Restart Codex after adding or changing an MCP server. Use the MCP settings page or `/mcp` to confirm that `serena` is connected.

Expected standard .NET location:

```text
C:\Program Files\dotnet
```

Normally, do not add this path as `DOTNET_ROOT` because `dotnet.exe` is already on `PATH`.

## macOS setup

1. Install Homebrew if it is not already installed.
2. Install `uv` and .NET:

   ```bash
   brew install uv
   brew install dotnet
   ```

3. Open a new terminal and verify:

   ```bash
   uvx --version
   dotnet --version
   command -v uvx
   command -v dotnet
   ```

4. Open the repository in Codex and trust the project so Codex loads `.codex/config.toml`.
5. Restart Codex after adding or changing an MCP server. Use the MCP settings page or `/mcp` to confirm that `serena` is connected.

Typical Homebrew paths are:

- Apple silicon: `/opt/homebrew/bin/uvx`
- Intel Mac: `/usr/local/bin/uvx`

If Codex Desktop cannot find `uvx` even though Terminal can, the desktop process is not receiving the same `PATH`. Prefer making the Homebrew binary directory available to GUI applications. As a host-local fallback, replace `command = "uvx"` with the exact result of `command -v uvx` on that Mac, but do not commit that machine-specific edit.

Homebrew may install .NET under `$(brew --prefix dotnet)/libexec`. Do not put that expanded macOS path in the shared project configuration. If a particular Mac requires `DOTNET_ROOT`, define it in that machine's environment before launching Codex:

```bash
export DOTNET_ROOT="$(brew --prefix dotnet)/libexec"
export PATH="$DOTNET_ROOT:$PATH"
```

## Verification and troubleshooting

> [!NOTE]
> Verified on Windows 11 on 2026-09-07: the TOML parsed, Serena 1.7.0 completed the MCP initialize handshake without `DOTNET_ROOT`, detected the repository, and started the C# language server with .NET SDK 10.0.103. The macOS instructions use the same portable configuration but must be run on the Mac host to verify that host's `PATH`.

Run these checks on the host where Codex is running:

```text
uvx --version
dotnet --version
```

Then confirm:

1. `dotnet --version` reports 10.x or newer.
2. Codex reports the `serena` MCP server as connected.
3. Serena activates the repository and reports the C# language server as active.
4. A Serena symbol lookup or C# diagnostics request succeeds.

Common failures:

- **`uvx` not found:** install `uv`, restart Codex, and verify the executable is on the host process's `PATH`.
- **C# language server does not start:** verify .NET 10+, remove stale or wrong `DOTNET_ROOT`, restart Codex, and inspect `.serena/home/logs`.
- **Wrong project is active:** open Codex from the repository root or activate the repository explicitly.
- **First launch is slow:** `uvx` may download the pinned Serena package and Serena may download its language-server runtime.
- **Config change is ignored:** confirm the repository is trusted and restart/reload the MCP server.

## References

- [Codex MCP documentation](https://learn.chatgpt.com/docs/extend/mcp)
- [Codex configuration reference](https://learn.chatgpt.com/docs/config-file/config-reference)
- [Serena documentation](https://github.com/oraios/serena)
