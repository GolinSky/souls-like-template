---
title: Codex and Serena Memory Exhaustion 2026-09-14
type: research
domains:
  - agent-tooling
status: draft
authority: evidence
updated: 2026-09-14
source_commit: e5180a25173576f4e7219cbe34a8bf93540a8608
aliases: []
tags:
  - research/package
---

# Codex and Serena Memory Exhaustion 2026-09-14

> **Applied follow-up, September 14:** The user approved remediation after this investigation. D: is now configured for system-managed paging; the shared HTTP Serena service, role overrides, guarded C# adapter patch, and sign-in launcher are installed. Two-client live validation passed, and eight old idle stdio backends were retired. A Windows restart is still required. See [[Serena Shared Backend and Pagefile Headroom]] for the applied changes and evidence. Earlier observations and proposed changes below are retained as the investigation record.

## Required Package

### Question and Desired Decision

Determine why Codex became unusable on September 14, whether Serena was configured correctly, what the numerous .NET failures mean, and how to prevent recurrence.

**Conclusion:** Windows exhausted its system commit capacity. Serena's simultaneous C# language-server startups and repeated Unity project restores are a strongly evidenced contributor and likely immediate trigger. Unity and Rider also consumed substantial memory. The evidence does not establish that Serena alone consumed the remaining memory, or that Codex suffered a distinct executable crash.

The recommended sequence is a measured baseline followed by one shared C# analysis backend per exact checkout. The follow-up below confirms that all nine simultaneous starts belonged to subagents and that a role with Serena disabled could still call its tools. Role-level disable flags alone are therefore insufficient containment in this session. Increasing pagefile headroom is a supporting measure. There is also a separate Codex history-import request flood worth reporting.

### Scope and Non-Goals

Read-only inspection of project/global Codex configuration, project/global Serena configuration, installed Serena 1.7.0 source, today's Serena and Codex desktop logs, Codex SQLite diagnostic logs, the earlier CLI investigation, Windows event records, crash artifacts, and current process/pagefile state.

All incident times below are **September 14, 2026, local UTC+03:00**. Desktop logs use UTC; Serena log text uses local time. Current memory measurements were taken after the user-initiated reboot and must not be substituted for the incident peak.

This report, evidence exports, and a temporary administrator script were created. The authorized pagefile change was attempted through normal Windows elevation, but Windows returned a cancellation before the script ran; its setting remains unchanged. No application configuration, Unity assets, Obsidian settings, or credentials were changed. The follow-up used one bounded read-only subagent probe and an isolated Python discovery fixture; no memory stress reproduction was attempted.

### Current System Map

Each applicable Codex connection can launch this chain:

`Codex app-server → uvx/uv → Serena Python server → Roslyn dotnet language server → project loading/restore → MSBuild workers`

Separate Serena processes can share binaries and cache files in `SERENA_HOME`; sharing that directory does **not** share their in-memory Roslyn workspaces.

Unity and Rider run alongside these chains and maintain their own memory and code-analysis state. Windows must back the combined committed allocations with RAM and pagefile capacity.

### Entry Points, Dependencies, and Consumers

| Component | Observed state |
|---|---|
| Codex desktop package | `OpenAI.Codex` version `26.908.4834.0`; process name includes `ChatGPT.exe` |
| Standalone CLI | `codex-cli 0.154.0` |
| Desktop task runtime | Incident subagent records in the app's `state_5.sqlite` identify `0.154.0-alpha.6.2`; direct bundled `--version` execution was denied |
| Serena launch | `.codex/config.toml`, existing `[mcp_servers.serena]` at line 5 |
| Serena version | Pinned `serena-agent==1.7.0`, Python 3.13 |
| Backend | LSP, C#, Roslyn package `5.5.0-2.26078.4`; .NET SDK `10.0.103` appears in failures |
| Activation | `--project-from-cwd`, which activates this checkout during startup |
| Effective Serena home | `.serena/home`, confirmed in live startup logs |
| Project settings | `.serena/project.yml`: `language_servers: [csharp]`, `ls_workspace_folders: ["."]`, Git ignore enabled, `ignored_paths: []` |
| Solution size | 199 solution project entries; 203 root `.csproj` files |
| Memory | 31.16 GiB usable physical RAM |
| Pagefile | `D:\pagefile.sys`, manually configured 16 GiB initial, 48 GiB maximum |

The user-level `C:\Users\golin\.serena\serena_config.yml` is not the active central configuration for this launch. The project-local `.serena/home/serena_config.yml` is. Editing the former alone would not correct this project's active settings.

### Evidence and Findings

#### 1. Confirmed system-wide commit exhaustion

Windows System event 2004, RecordId **111437**, at **15:49:49.657** contains:

| Measurement | Value |
|---|---:|
| System commit charge | 84,729,192,448 bytes = **78.91 GiB** |
| System commit limit | 84,995,461,120 bytes = **79.16 GiB** |
| Commit utilisation | **99.69%** |
| Remaining commit capacity | **254 MiB** |
| Physical memory usage | **28.30 / 31.16 GiB** |
| Process count | **1,137** |
| Aggregate process commit | **68.25 GiB** |

This explains why an application can receive an out-of-memory failure even when a physical-RAM display has some free space. Commit is the memory Windows has promised to back; it cannot exceed its backing capacity. [Microsoft's explanation of commit and pagefile sizing](https://learn.microsoft.com/en-us/troubleshoot/windows-client/performance/how-to-determine-the-appropriate-page-file-size-for-64-bit-versions-of-windows).

The event's three largest reported processes were:

| Process | PID | Commit |
|---|---:|---:|
| Unity.exe | 6180 | **12.356 GiB** |
| rider64.exe | 16708 | **2.023 GiB** |
| dotnet.exe | 36184 | **1.903 GiB** |

These are individual processes, not application-family totals. The event does not enumerate enough of the 1,137 processes to assign all 68.25 GiB of process commit to owners.

PID 36184 was created at 14:41:46.905. Serena log `mcp_20260914-144145_9068.txt`, line 53, records a Roslyn launch at 14:41:46.843. Their close timing is suggestive, but the log does not record the child PID; attribution of that particular 1.903-GiB process to Serena remains an inference.

#### 2. Incident timeline

| Local time | Observed event |
|---|---|
| 13:01–13:09 | Codex records over 127,000 routed history-import responses in the retained early window, before today's first recorded Serena start. |
| 13:10:13 onward | Repeated Serena launches accumulate throughout the working session. |
| **15:36:48.835–48.992** | Codex reports Serena `status=starting` for **nine distinct task IDs** within 157 ms. |
| **15:36:54–55** | Nine corresponding Serena process log files begin; they start the same checkout's C# backend. |
| **15:37:21.382** | Resource-Exhaustion-Detector event 1003 reports low virtual memory. Event 1008 follows when diagnosis fails. |
| **15:37:21–15:38:08** | .NET/MSBuild OOM and application failures occur; seven Serena log files contain OOM messages. |
| **15:37:28** | One additional Serena server starts during the failure cascade. |
| **15:37:44.619** | .NET reports that the paging file is too small while loading `Microsoft.Build.dll`. |
| **15:43:55–15:45:52** | Codex records **38 failed requests** with `App server request expired while queued`, including settings, models, tasks, and MCP status. |
| **15:49:49.657** | Event 2004 records 99.69% commit usage and 1,137 processes. |
| **15:50:18** | Desktop log records IPC `EPIPE`, then `Stopping app-server transport` and `cause=stop_process`. |
| **16:03:19** | Windows records a user-initiated restart. |
| **16:03:47** | Windows boots. |
| **16:06–16:09** | Codex and multiple Serena connections start again; the configuration still permits recurrence. |

The nine task IDs and exact log locations are preserved in the evidence JSON. The logs establish the multi-task startup burst; they do not establish the precise UI action or backend lifecycle decision that initiated it. A server restart, restoration, or connection refresh must not be claimed as proven without further backend tracing.

#### 3. Serena works, but its startup behaviour is unsuitable for this workload

Correct elements: Serena is registered at project scope, absent from global Codex MCP entries, pinned to a version, using the intended C# backend, and has its web dashboard disabled. Logs show successful project activation and symbol calls. This is not an authentication or missing-runtime setup failure.

Problematic elements:

1. **Every auto-activated process loads C#.** `--project-from-cwd` leads to activation before anyone needs a symbol query. Startup uses stdio subprocesses; instances are not a singleton. The burst is actual evidence of multiple independent instances.
2. **Large solution and broad discovery.** Installed `solidlsp/language_servers/csharp_language_server.py`, lines 729–756, first sends `solution/open`, then separately scans and sends `project/open` for discovered `.csproj` files. Its helper at lines 141–160 recursively traverses directories, skips dot-prefixed entries, and does not consult Serena's project Git-ignore matcher. Therefore existing Git ignores do not constrain this particular project-discovery path. Adding `Library` to `ignored_paths` alone cannot be presented as a verified fix for it.
3. **Restore amplification.** Before reboot there were **40 Serena launch logs**, **3,418 `Running dotnet restore` entries**, and **149 OOM-matching lines across seven logs**. These are cumulative counts, not 40 simultaneously live processes or 149 independent crashes.
4. **The workload repeats after reboot.** A single clean startup log, `mcp_20260914-160702_25444.txt`, contains **203 restore starts, 203 unresolved-dependency warnings, and 203 messages saying there are no packages to restore**. The logged restore workload is largely unproductive for these Unity-generated projects. This does not prove that every code reference resolves correctly.
5. **Agent isolation is incomplete.** `.codex/agents/README.md` says only the MCP server required by each role is enabled. Among the inspected TOML role files, only `graph-explorer.toml` explicitly disables Serena. The other seven roles have no MCP overrides. Their `[agents] enabled = false` prevents further delegation; it does not disable inherited MCP servers. The documentation therefore overstates actual isolation.
6. **No process budget.** The project permits four concurrent child tasks per session, but that is not a machine-wide cap on MCP servers, Roslyn, restored tasks, or MSBuild workers.

The installed adapter hardcodes several C# configuration responses, including diagnostic scope `openFiles`. No supported local setting that simply limits this adapter to one chosen solution or turns off all the observed restore work was established. Invented YAML keys would not be a dependable fix.

Installed source used for these findings: `C:\Users\golin\AppData\Local\uv\cache\archive-v0\gVUiKcO_2bAhD0Qs\Lib\site-packages\solidlsp\language_servers\csharp_language_server.py`. This is the executed package source, not an assumption based on the latest branch.

#### 4. Current processes show continued duplication and retained workers

Post-reboot snapshots found two independently verified Codex-owned Serena/Roslyn chains. Their Roslyn processes used approximately **642 MiB private bytes combined**; including their Python/uv processes, the two chains used approximately **1.43 GiB private bytes**.

A later snapshot contained **15 additional MSBuild node processes** with `/nodemode:1 /nodeReuse:true`, totalling **1,557.5 MiB private bytes**. Their recorded parents no longer existed. Reusable build nodes can outlive the restore invocation; this is not by itself evidence of a leak. Their start times align with restore activity, but the available ancestry cannot conclusively assign all of them to Serena rather than other .NET clients.

These measurements demonstrate overhead, not the historical peak of the same processes. There is insufficient evidence to distinguish an internal memory leak from instance multiplication, workspace growth, and worker retention.

#### 5. Codex has additional issues, but a distinct executable crash is unproven

- Retained desktop logs contain **148,151** responses for `externalAgentConfig/import/readHistories`, including more than 127,000 between 13:01 and 13:09. Five main log segments occupy about 52 MB. This unusually repetitive activity precedes the first Serena start and merits separate app investigation. Its contribution to the later peak cannot be quantified.
- Thousands of `unknown conversation` / `Conversation state not found` messages appear, including events for auxiliary and child tasks, plus some invalid task-ID requests. Their count is not a crash count.
- The global configuration still contains `features.rmcp_client = true`; current logs report `unknown feature key in config: rmcp_client`. This is stale configuration, not the primary memory cause.
- Plugin manifest prompt-length/count warnings also occur. No evidence connects them to the exhaustion.
- The legacy `unityMCP` entry remains in global configuration and is disabled by current project configuration. Some older task startup notifications still mention it. This warrants a later effective-configuration check, but does not prove it caused today's memory spike.
- No matching Codex/ChatGPT application crash event or new Codex Crashpad dump was found for today. The inspected dump is from May 29. No renderer OOM/crash marker was found in retained desktop logs.
- Today's WER `BlueScreen 139` reports refer to an August 7 minidump. They are not proof of a new BSOD. Windows recorded an orderly user restart, not Kernel-Power 41 or unexpected shutdown 6008 in the incident interval.

The best-supported description is **Codex became unresponsive during host memory exhaustion, followed by shutdown and reboot**. The inability to relaunch is consistent with exhausted commit and surviving worker processes, but a failed-launch stack trace was not retained.

### Options and Tradeoffs

#### A. Immediate containment and baseline — recommended first

In the existing `[mcp_servers.serena]` table in `.codex/config.toml`, add:

```toml
enabled = false
```

Preserve the rest of the table. This is a documented reversible disable flag. Its effect must be verified in a fresh task: the follow-up runtime probe demonstrates that a disabled role still has callable Serena tools in this app session. Do not assume that adding more role flags alone prevents backend startup. [Official Codex MCP configuration](https://learn.chatgpt.com/docs/extend/mcp?surface=cli).

After preserving active work, close the relevant Codex tasks and restart the app so existing connections use the new configuration. Merely editing the file should not be assumed to terminate every existing child. Since Windows has already been rebooted, another immediate reboot is not inherently necessary. If resource pressure returns and owned worker processes cannot be cleanly retired, a planned reboot clears the remaining process state.

Avoid killing every `dotnet.exe`: Rider and other tools may own some of them. Any targeted cleanup must identify ownership before termination.

#### B. Keep Serena, with controlled activation — simplest next step

Remove `--project-from-cwd` from the stdio launch and activate the project explicitly only in the task that needs live C# symbols. Disable Serena in roles that do not need code navigation. Initially use one C# analysis task at a time.

This removes eager Roslyn startup from idle connections, although each enabled stdio connection can still have Python/server overhead. It changes the current project instruction that expects automatic activation; implementation should update that instruction consistently. It is a proposed configuration, not a change applied by this investigation.

#### C. One shared backend per exact checkout — stronger long-term option

Serena supports a separately managed Streamable HTTP server. Multiple clients for the **same project** can connect to that one instance. This can remove duplicated Roslyn workspaces and repeated startup restores. It adds server lifecycle management and needs validation of concurrent tools and recovery.

Serena is stateful and has one active project. Do not share one instance across different worktrees or projects while allowing clients to switch activation. Keep one backend per exact checkout and preserve a single writer for overlapping edits. [Serena transport and statefulness documentation](https://oraios.github.io/serena/02-usage/020_running.html).

The JetBrains backend is another option to investigate because Rider is already running. Its documentation describes sharing IDE analysis among agents. Rider/C# compatibility for this installed plugin and workflow must be checked before choosing it; no plugin installation or backend migration was performed. [Serena JetBrains backend documentation](https://oraios.github.io/serena/02-usage/025_jetbrains_plugin.html).

#### D. Correct project-loading amplification

If retaining Roslyn, validate an adapter version or bounded upstream fix that opens the intended solution once, respects excluded/generated directories, and avoids redundant restore work for these Unity projects. Avoid modifying the disposable uv cache as a durable fix. Do not delete Unity-generated project files or change package dependencies merely because the external language server reports unresolved references.

An update may help only if its relevant behaviour is verified. The follow-up checked PyPI: 1.7.0 remains the latest Serena release, and current upstream C# discovery retains the relevant behavior. No specific newer Serena, Roslyn, .NET, or Codex version was established as fixing this incident.

#### E. Add memory headroom and report the app issue

Current `AutomaticManagedPagefile` is false. The configured D: maximum is 48 GiB; at failure, the approximately 79.16-GiB commit ceiling was consistent with RAM plus that maximum. D: had approximately **363 GiB free**, so its physical free capacity was not the limiting factor.

Consider changing D: to **System managed size** through `SystemPropertiesAdvanced` → Performance Settings → Advanced → Virtual memory Change → D: → System managed size → Set. Allow Windows to request any needed restart. A larger pagefile provides backing capacity; it does not eliminate excessive process creation or make paging as fast as RAM. [Microsoft pagefile guidance](https://learn.microsoft.com/en-us/troubleshoot/windows-client/performance/how-to-determine-the-appropriate-page-file-size-for-64-bit-versions-of-windows).

Report the history-import request flood and nine-task MCP startup burst to Codex support with the package version, timestamps, and reviewed excerpts. Check for an available app update; updating the standalone CLI does not establish that the app-bundled runtime changed. [Official troubleshooting and feedback guidance](https://learn.chatgpt.com/docs/reference/troubleshooting).

Deleting logs, increasing tool timeouts, or reinstalling .NET would not address the observed instance/restore multiplication. Log cleanup may reclaim disk space, but the stored log byte count is not a measure of process RAM.

### Risks, Unknowns, and Open Questions

- No complete pre-failure process tree, periodic memory capture, or heap dump exists in the inspected evidence. Serena's share of the entire commit charge is unknown.
- The exact action that triggered simultaneous startup for nine old task IDs is not established.
- Logs are rotated and the diagnostic SQLite store is bounded; absence of a retained marker is not proof that a failure never occurred.
- The current configuration may differ from some existing task snapshots. Rechecking effective settings in new tasks is necessary after any fix.
- Restricting project loading can reduce external-symbol coverage; validate representative definitions and references before adopting it.
- Repeatedly starting diagnostic tasks can itself initialize configured MCP servers. One read-only role probe was used in the follow-up; it reached an existing backend and did not produce a new Serena startup log during the observed interval.

### Recommended Review Questions

1. Does disabling Serena stop new Roslyn launches for this project under otherwise comparable Codex use?
2. Does explicit activation or a single shared backend keep the Roslyn count bounded when multiple tasks are opened?
3. Do MSBuild workers retire or remain bounded after startup and after closing the owning tasks?
4. Does the history-import response flood recur with Serena disabled?
5. Can the selected backend resolve representative project and package symbols without the current 203 redundant restores?

### Handoff

Proceed with containment first, then choose controlled activation or one backend per checkout. Acceptance should be based on measured process counts and commit usage, not just a successful MCP connection:

- With Serena disabled, new project tasks do not create project Serena/Roslyn processes.
- With Serena restored in a controlled configuration, the intended checkout has one active C# backend and no nine-instance startup burst.
- Opening/closing representative tasks does not cause sustained worker-count or commit growth.
- No new Resource-Exhaustion-Detector events, .NET OOM failures, or Codex queue-expiry cascade occurs during the bounded validation period.
- Definitions/references needed for normal C# work still resolve.

Observe Task Manager's **Performance → Memory → Committed**, plus process count and private bytes, during the comparison. Use a conservative stop threshold before approaching the commit ceiling; do not deliberately reproduce 99% exhaustion.

Evidence files: [[Codex and Serena Memory Exhaustion 2026-09-14/Log and Configuration Evidence.json]] and [[Codex and Serena Memory Exhaustion 2026-09-14/Windows Resource Exhaustion Event 111437.xml]]. The JSON preserves startup lines, counts, source log locations, configuration summaries, and Serena log hashes. It contains local diagnostics and has not been published externally.

### Follow-up: pagefile action, subagent proof, and specific Serena fixes

Collected September 14, approximately 16:25–16:45 local time. This section supersedes earlier uncertainty about the identity of the nine tasks and the reliability of role-level MCP isolation.

#### Pagefile action: blocked by Windows elevation

The requested change was prepared for the existing `D:\pagefile.sys`: replace the manual 16-GiB initial / 48-GiB maximum with system-managed sizing on D:. D: has about 363 GiB free. No relocation to C: or automatic reboot was requested.

The administrator script is at `F:\Private\SoulsLikeTemplate\tmp\codex-pagefile-20260914\Set-PagefileHeadroom.ps1`. It validates the existing D: entry, saves the previous values, changes its initial/maximum sizes to zero, and checks the retained setting. Windows rejected the normal `RunAs` launch with **“The operation was canceled by the user.”** The current process is not an administrator, and the script did not run. No alternate elevation route was attempted.

Readback after the attempt still shows `InitialSize=16384`, `MaximumSize=49152`. Runtime allocation is 16384 MiB, with 193 MiB in use at the final snapshot. That snapshot has about 31.45 GiB committed against a currently allocated 47.16-GiB limit; this post-reboot limit can grow under the existing setting and is not the 79.16-GiB incident ceiling.

**Pending action:** obtain Windows administrator approval and execute the prepared script; verify `D:\pagefile.sys 0 0` and schedule any required restart after saving work. Do not claim that headroom has already increased. Windows distinguishes startup settings from current pagefile usage. [Microsoft pagefile setting reference](https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-pagefilesetting).

#### All nine simultaneous startups were subagents

The nine desktop `status=starting` notifications at 15:36:48.835–15:36:48.992 were joined by task ID to read-only records in `C:\Users\golin\.codex\state_5.sqlite`:

| Agent path | Role | Task originally created |
|---|---|---|
| `/root/elevator_code` | `csharp_worker` | 13:57:07 |
| `/root/elevator_assets` | `unity_operator` | 13:58:17 |
| `/root/save_storage_trace` | `graph_explorer` | 14:03:59 |
| `/root/elevator_review` | `unity_reviewer` | 14:09:22 |
| `/root/audit_instructions_skills` | `graph_explorer` | 14:46:20 |
| `/root/audit_context` | `context_curator` | 14:46:29 |
| `/root/equipment_runtime` | `csharp_worker` | 14:50:22 |
| `/root/equipment_review` | `unity_reviewer` | 14:55:09 |
| `/root/equipment_tests` | `unity_test_runner` | 15:09:48 |

These tasks belong to four parent tasks and existed before the simultaneous MCP start notifications. This was not simply nine newly created C# workers. The exact app action that caused the later startup burst remains unknown. All nine database records identify runtime `0.154.0-alpha.6.2`.

#### Role audit: seven inherit Serena; the disabled eighth can still call it

The parent project entry enables Serena by default and eagerly activates the current checkout. Seven role TOMLs have no Serena override. Their `[agents] enabled = false` disables recursive agent spawning; it does not disable inherited MCP servers.

| Role | Current Serena setting | Recommended access for its assigned work |
|---|---|---|
| `csharp_worker` | Inherits enabled | Keep live C# symbols |
| `unity_reviewer` | Inherits enabled | Keep for C# review |
| `unity_architect` | Inherits enabled | Keep when architecture requires source symbols |
| `context_curator` | Inherits enabled | Disable; its task is curated vault retrieval |
| `unity_operator` | Inherits enabled | Disable; operate through official Unity tooling |
| `unity_test_runner` | Inherits enabled | Disable; run the assigned validation commands |
| `unity_profiler` | Inherits enabled | Disable by default; source investigation can be assigned separately |
| `graph_explorer` | Explicit `enabled = false` | Keep disabled and verify runtime enforcement |

The role README's claim that only needed MCP servers are enabled is not borne out by these TOMLs. The parent concurrency setting is scoped per session; it is not a machine-wide cap on backends owned by existing tasks.

**Live test:** a fresh `graph_explorer` child listed 23 Serena tools. One read-only `mcp__serena__get_current_config` call succeeded and reported Serena 1.7.0, active `SoulsLikeTemplate`, LSP backend, ready C# server, and Codex context. This is executable evidence that the role's disabled setting did not make Serena unavailable in this runtime. No new Serena startup log appeared during this probe, so the test proves callable access, not a newly spawned process or the precise routing mechanism.

OpenAI has an open Windows report about disabled subagent MCP configuration being ignored. It corroborates the symptom, but is not a confirmed explanation of every detail in this installation. Separately, an OpenAI maintainer states that separate MCP instances for subagents are intentional because their configurations can differ; that duplication issue was closed as not planned, not fixed. [Disabled-MCP report #42000](https://github.com/openai/codex/issues/42000), [maintainer explanation on #12333](https://github.com/openai/codex/issues/12333#issuecomment-3935804687).

#### C# discovery issue reproduced without starting .NET

The installed adapter's exact, unmodified `breadth_first_file_scan` and `_open_solution_and_projects` functions were extracted using Python AST and executed against a temporary fixture with a fake notification receiver. The fixture contained a solution, a root C# project, and `Library/Ignored.csproj`, with Library excluded by Git ignore.

The adapter emitted both `solution/open` and `project/open`; the latter included the ignored Library project. Its scanner checks dot-prefixed directory names but does not consult Serena's ignore settings. The fixture was removed after recording the result so it cannot pollute real project discovery.

This proves that setting `ignore_all_files_in_gitignore = true` is not a reliable exclusion control for this adapter's workspace discovery. It also proves dual solution/project opening. It does **not** prove that this alone doubles all project loads or accounts for a specified amount of memory. Real logs show 203 restore starts for one post-reboot backend, not 406. Across today's pre-reboot logs, there were 3,418 restore starts.

The installed file is `C:\Users\golin\AppData\Local\uv\cache\archive-v0\gVUiKcO_2bAhD0Qs\Lib\site-packages\solidlsp\language_servers\csharp_language_server.py`, functions at lines 141 and 729. Current upstream retains these functions; the last file commit inspected was `1c12156901624ead484cfdbb1f3eee3390e7193d`. PyPI still reports Serena 1.7.0 as latest. [Upstream C# adapter](https://github.com/oraios/serena/blob/1c12156901624ead484cfdbb1f3eee3390e7193d/src/solidlsp/language_servers/csharp_language_server.py), [Serena releases on PyPI](https://pypi.org/project/serena-agent/).

An adapter correction should open the intended solution once, use explicit project discovery only as a fallback, and bound fallback discovery to intended directories. It must still allow the root Unity-generated solution/projects: this repository intentionally Git-ignores `*.sln` and `*.csproj`, so blindly honoring every Git-ignore rule would break analysis. This correction requires a pinned maintained fork or upstream change plus C# navigation validation; editing disposable uv cache files is not a durable fix. A reduction in restore count from this correction has not yet been measured.

#### Supported fix for instance multiplication: one shared backend per checkout

Replace per-client stdio launching with one separately managed Serena Streamable HTTP process for this exact checkout. This changes the resource model from one Python/Roslyn stack per applicable client to one shared stack. The installed `serena/mcp.py` creates one agent and keeps it across HTTP client disconnects (lines 373 and 396–419). Serena documents this transport for multiple clients using the same active project. [Serena transport documentation](https://oraios.github.io/serena/02-usage/020_running.html).

The following recipe is **prepared, not applied**. The arguments were verified against installed 1.7.0 `start-mcp-server --help`; port 9121 had no listener when checked. Start the server once under an explicit lifecycle owner, with output logs and a recorded PID. The foreground command is:

```powershell
$env:SERENA_HOME = 'F:\Private\SoulsLikeTemplate\.serena\home'
$env:PYTHONUTF8 = '1'
uvx -p 3.13 --from serena-agent==1.7.0 serena start-mcp-server --context codex --project 'F:\Private\SoulsLikeTemplate' --transport streamable-http --host 127.0.0.1 --port 9121 --enable-web-dashboard false
```

Replace the existing project Serena table and remove its `.env` subtable, since environment variables now belong to the separately launched process:

```toml
[mcp_servers.serena]
url = "http://127.0.0.1:9121/mcp"
enabled = true
startup_timeout_sec = 60
tool_timeout_sec = 120
```

Replace `graph-explorer.toml`'s old Serena stdio table and its environment subtable with a transport-consistent override; do not leave a `command` merged with the new `url`. For each role intended to disable Serena, use:

```toml
[mcp_servers.serena]
url = "http://127.0.0.1:9121/mcp"
enabled = false
```

Keep all configuration project-local. Give another checkout/worktree its own server and endpoint; clients sharing this server must not switch its active project. Closing one client should leave the shared service available to the others. Existing stdio processes must be retired by restarting their owning app/tasks after work is saved; changing a TOML file is not process cleanup.

This is a supported architecture change, not a claim that it has already been deployed or that it fixes unrelated Codex memory use. Even if role filtering remains ineffective, clients using the HTTP entry will not each launch the configured stdio backend. Old task snapshots and role definitions containing the old command must be checked during migration.

Acceptance: one Serena service and one Roslyn language server for this checkout across several clients; MSBuild worker count allowed to fluctuate but remain bounded; no second 203-project startup batch when another client connects; representative symbol lookup/reference tools succeed; client disconnect does not remove the shared backend. Measure system commit before/after, and do not reproduce exhaustion to validate the fix.

New evidence: [[Codex and Serena Memory Exhaustion 2026-09-14/Subagent Runtime Evidence.json]] and [[Codex and Serena Memory Exhaustion 2026-09-14/Adapter Discovery Proof.json]]. Configuration migration and the adapter correction remain recommendations; the only requested system change attempted was the pagefile update, which remains pending administrator elevation.

## Evidence Rules

Observed facts, causal inferences, and proposed changes are distinguished above. Live files and Windows events take precedence over older role documentation. Review date: September 14, 2026. Source commit: `e5180a25173576f4e7219cbe34a8bf93540a8608`. Documentation-only output; no Unity test execution is relevant to this investigation.
