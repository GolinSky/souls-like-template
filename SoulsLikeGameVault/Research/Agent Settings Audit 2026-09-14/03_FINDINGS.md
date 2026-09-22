# Findings and priorities

No confirmed P0 incident or evidence of unauthorized remote writes was found. The repair plan distinguishes demonstrated defects, capability risks, and optional cleanup.

## F01 — Read-only role isolation is overstated

Priority P1 for the isolation assumption; confirmed, high confidence. Applies to this desktop task and observed curator child. Sources: .codex/agents/context-curator.toml:5; .codex/agents/README.md:24; current parent/child runtime metadata.

The README says only a role's required MCP server is enabled. Seven role files contain no MCP narrowing. The curator inherited danger-full-access/never and write-capable Unity, vault, CV filesystem and connectors. Its read-only label is not an enforced boundary in this task. This matches documented parent-runtime inheritance; it is not evidence of a Codex defect.

Minimal repair: correct the misleading README (patch 05), then choose and test a real role capability policy. If enforced read-only operation is desired, use a parent launch that actually supplies it and explicit server/plugin tool restrictions supported by that client. Do not assume changing a role's sandbox line alone solves this. Keep single ownership and no recursive delegation. Obsidian policy changes require separate authorization.

Validation: V09/V11 with a disposable fixture and capability inventory, not a production write. Rollback: reverse only approved role-policy/README edits. No exploitation was attempted.

## F02 — Unknown rmcp_client feature fails strict startup

Priority P2; confirmed, high confidence. C:/Users/golin/.codex/config.toml:257. Both PATH 0.154.0 and app-associated 0.154.0-alpha.6.2 reject features.rmcp_client under --strict-config. Normal app-server startup succeeds but warns “unknown feature key in config: rmcp_client.”

This is an actually unrecognized field, not a supported legacy alias. Remove only this line (patch 01); keep js_repl, multi_agent, models, approval reviewer and working integrations. No replacement flag is required by observed behavior.

Validation: V07 strict startup on both clients; ordinary config/read and MCP inventory unchanged. Rollback: restore the single line from the protected prechange baseline. Current normal use is not claimed broken.

## F03 — Competing skill packages are exposed together

Priority P2 for conflicting routing; confirmed availability and source divergence, medium confidence of future behavioral impact. Sources: both Graphify SKILL.md:3 and extraction sections near 253; C:/Users/golin/.codex/skills/unity-mcp-skill/SKILL.md:2 and 181; root/nested skills_discovery.json; AGENTS.md:35.

Both same-name Graphify entries are discoverable/enabled in root and nested skill inventories; runtime misrouting was not reproduced. Project extraction uses Codex spawn/wait while personal extraction requires a generic Task tool. The globally discoverable Unity skill prescribes Coplay manage_* and mcpforunity resources, conflicting with this project's official Pipeline route.

Minimal repair: disable the two competing personal skills only in this Windows repository using per-path skills.config overrides (patch 03). Preserve the installed packages for other repositories. This is a project routing choice, not a claim that duplicate names are forbidden by OpenAI. The override uses folder paths as documented.

Validation: V05/V06: only project Graphify and official Unity skills enabled in root/nested; personal copies remain available outside. Test two positive, two negative and explicit invocations for changed routing. Rollback: remove only these overrides. Implicit misrouting was not reproduced with a model turn.

## F04 — A required ProBuilder documentation link is broken

Priority P2; confirmed, high confidence. .agents/skills/soulslike-probuilder/SKILL.md:16 links absent references/command-contract.md. SOURCES.md at line 34 exists and is tracked; an initial auditor false positive was corrected.

Minimal repair: replace that link with live unity command --json discovery (patch 02), which this skill and project already require. This audit executed schema discovery successfully. Do not fabricate a command-contract file from remembered APIs.

Preserve exact Editor verification, single writer, scene safety, attribution, and geometry validation. Validation V04: link check and relevant schema query; no gameplay tests needed for a documentation-only fix. Rollback: reverse the two-line change.

## F05 — An unrelated project's filesystem service is inherited

Priority P2; confirmed exposure, high confidence. Personal config.toml:238 roots cv_filesystem at F:/CvCreationProject; the SoulsLike parent and curator expose its read/write tools.

This may be intentional for the CV project. It does not need to be globally removed. Proposed scope hardening: disable this inherited server only in SoulsLike (patch 04). Local shell access in a full-access task remains a separate boundary; this patch alone does not sandbox the filesystem.

Validation V08/V09: effective project server disabled and tool absent in a fresh task; outside/CV usage retains its service. Rollback: remove only project override. No CV content was read.

## F06 — Inline credential material needs a separate decision

Priority P2; presence confirmed, validity/external exposure unknown. Repository .codex/config.toml:32 contains an inline Authorization value and is Git tracked; that Obsidian server is disabled. Personal config.toml:277 contains Penpot userToken in its URL. Values are omitted from artifacts.

A disabled server does not remove a credential from stored configuration. However, neither public repository disclosure nor valid live access was established. One audit tool output accidentally included the Penpot parameter before redaction was corrected; see scope disclosure.

No patch is staged. Obsidian configuration is explicitly protected by AGENTS.md:20. If requested separately, assess the supported credential mechanism and existing exposure before changing bindings; do not blindly rotate, scrub history, untrack files, or invent URL environment substitution support. Preserve functioning accounts and endpoints. V08/V09 and an explicit sanitized-sharing check would follow any authorized change.

## F07 — Parent model policy disagrees with installed defaults

Priority P2 for instruction/config inconsistency; confirmed, high confidence. AGENTS.md:64 and .codex/agents/README.md:22 request GPT-5.6 Sol High; config/read reports global gpt-6-astra/xhigh and no project model override.

Prose cannot reconfigure a running parent. This may be a deliberate recent user model choice with stale documentation, or a missing project override. No automatic model change is proposed. Decide which intent is current, then update only documentation or the explicit project default. Keep model fixed while evaluating instruction changes. V03 and a fresh task must verify the choice.

## F08 — Additional legacy skill links are missing

Priority P2 when this skill is used; confirmed, high confidence. Personal unity-mcp-skill/SKILL.md:191 references references/probuilder-guide.md; lines 279–280 reference references/resources-reference.md. Both absent; tools-reference.md and workflows.md exist.

Patch 03 prevents this package being selected for SoulsLike; it does not repair its use elsewhere. Restore accurate resources from the package's source or retire links only after preserving needed guidance. Owner/source unresolved. No vendor file changed. V04 and the owning client's invocation tests required.

## F09 — agy plugin prompt metadata is rejected

Priority P3; confirmed, high confidence. Installed agy 1.3.0 .codex-plugin/plugin.json has five interface.defaultPrompt entries; 0.154.0 warns that at most three are supported and ignores the field. Main skill discovery still succeeds.

Fix in the plugin source, then reinstall through its supported development workflow after authorization; do not edit generated cache. Choose three useful entry prompts and preserve all operations in the skill body. Source checkout not identified. Also observed two icon-path warnings; owning skill was not established, so no speculative icon patch.

## F10 — Broad execution allowances merit review

Priority P3, policy recommendation; rule match confirmed. Personal rules/default.rules:5 and :9 allow every codex mcp add and codex mcp login prefix. Checker tests using a fictional server matched allow; codex mcp list did not match.

These may be intentionally accepted rules; no unauthorized command was run. Review breadth if future sandboxed sessions should ask for new servers/accounts. Do not infer that rules constrain this current danger-full-access runtime. Do not delete all historical allow rules or auto-change approval mode. V10 boundary tests required for any selected edit.

## F11 — Optional Graphify workflow modernization

Priority P3; source observations confirmed, impact not benchmarked. Broad descriptions route virtually any code/project-content question to Graphify; the 41 KB root mixes query and full-build procedures, Bash snippets and agent-based extraction. This project's graph_explorer forbids recursive spawning; build procedures therefore need clear role eligibility.

Keep the existing-graph fast path, no silent rebuilds, source verification, logging controls and attribution. As a later change, narrow descriptions and move full-build recipes behind an explicit build trigger; add a verified Windows route only for commands actually supported. Do not shorten solely to meet a line target. No rebuild or wholesale skill rewrite is staged.

## Accepted or unproven items

- AGENTS length is below the observed budget; no 100-line requirement or truncation defect.
- Project skill ownership and conditional vault routing are already well designed.
- max_concurrent_threads_per_session is recognized; no agents.max_threads migration needed.
- approvals_reviewer=user is recognized. It selects the reviewer when approval occurs; it does not force prompts under the current never mode.
- Context, Context Mode and Context7 have different jobs; not accidental duplicates.
- Disabled legacy Unity/Obsidian/GitHub registrations are not active duplicate connections in this project.
- Optional missing openai.yaml or CLAUDE.md is not a defect.
- Absolute Windows launch paths work locally; Mac portability and sync remain untested.
- Vendor cleanup-hook overlap and token/quota savings remain unmeasured.
