# Validation evidence

Audit-only results. There is no after-repair success claim.

| Test | Result | Evidence and limit |
|---|---|---|
| V01 Root/nested/outside loading | PASS for CLI render/config probes | prompt_load_checks.json and config_*.sanitized.json. Expected instructions/config boundary observed without model turns; not a macOS or full desktop task test. |
| V02 Override/fallback/budget fixtures | NOT TESTED as synthetic fixtures | Real chain totals 24,228 raw bytes; final marker present; no override/fallback candidates found. Boundary truncation fixture not run. |
| V03 Profile/override/trust | PASS for current provenance; alternate scenarios NOT TESTED | User model/effort and repository agent defaults observed; trusted repo, profile null; managed requirements null. No trust or profile changes. |
| V04 Required paths/resources | FAIL on baseline | ProBuilder command-contract absent; two legacy Unity resource links absent. SOURCES.md exists. Proposed replacements checked separately. |
| V05 Skill behavior examples | NOT TESTED | No model benchmark/implicit invocation experiment; test prompts listed below. |
| V06 Duplicates/disabled skills | Baseline duplicate discovery CONFIRMED; after-change NOT TESTED | Both Graphify copies and legacy Unity enabled at root/nested. Scoped disable proposal parses, not yet loaded as live config. |
| V07 Parsing/client validation | TOML PASS; strict startup FAIL | Ten relevant TOML files parse. CLI 0.154.0 and app CLI 0.154.0-alpha.6.2 reject features.rmcp_client. Ordinary config/read succeeds with warning. |
| V08 MCP | PASS for selected checks; others NOT TESTED | mcp list parsed; executable/path checks pass; Drive fetch and vault outline succeeded; Unity status/schema/scenes pass. No full all-server launch. |
| V09 Tool/approval boundary | Observed capability mismatch; enforcement fixture NOT TESTED | Curator inherited full access and broad write tools. No production write probe. |
| V10 Execution rules | PASS as parser/matching test; policy review pending | add/login fictional server matched allow; list unmatched. Checker did not run those commands. |
| V11 Subagents | Partial PASS / isolation finding | Two bounded auditors spawned; model-role catalog recognized; no overlapping writers. Curator runtime differs from declared read-only. All-role isolation not tested. |
| V12 Hooks/offline | Static review only | Vendor cleanup hooks inspected; no hook replay, outage simulation, or third-party setup script executed. |
| V13 Unity integration | PASS read-only smoke | Correct F:/Private/SoulsLikeTemplate Editor 6000.3.11f1, ready, not compiling, Play Mode stopped; ElevatorDemo clean. No UTF/build/gameplay test needed for audit. |
| V14 Sync/rollback | NOT TESTED live | Upstream sync source/Mac unavailable. Five staged patches pass independent git apply --check; live rollback not applicable before repair. |

## Exact successful checks

- Python 3.13 tomllib parsed personal/project config and eight role definitions.
- Installed CLI-generated protocol schemas were used for initialize, config/read, configRequirements/read, skills/list.
- codex debug prompt-input succeeded from root, Assets/Scripts/Items and personal Documents. The audit stored booleans/counts, not full prompts.
- codex mcp list --json succeeded. Its “unsupported” authentication label for STDIO is not treated as a service failure.
- unity --version, status --json, command --json, command editor_status --json and command list_open_scenes --json succeeded.
- Vault get_note_outline on Templates/Plan Template.md succeeded.
- Five proposal files individually pass git apply --check; proposed TOML edits parse. No patch was applied to a live target.

## Failed or corrected diagnostics

- codex --strict-config features list is an unsupported diagnostic combination in this version. Replaced with strict app-server startup; that exposes the real rmcp_client error.
- Web retrieval of .md endpoints failed due to unsupported text/markdown content type; corresponding HTML bodies were retrieved instead.
- Initial auditor claimed ProBuilder SOURCES.md absent; direct filesystem/Git evidence corrected it.
- Initial curator claimed vault tool unavailable; metadata plus parent read proved availability. No false “offline” finding retained.
- One first patch dry run failed due to newline handling. Regenerated patches preserve target line endings; all five then passed.
- An early redaction failure is disclosed in 00_SCOPE_AND_ENVIRONMENT.md; saved artifact scans must exclude both configured credentials.

## Repair acceptance prompts

Run with the same model/effort, tool state and input before/after. Require observed selected skill paths, not self-report alone.

1. Positive Graphify: “Explain ownership across Character and GameOrchestrator without edits.”
2. Positive Graphify: explicit project Graphify query of an existing graph; no rebuild.
3. Negative Graphify: “Correct one typo in a named Markdown sentence.”
4. Negative Graphify: “Report current Codex configuration layers.”
5. Positive Unity: inspect the connected Editor and an existing prefab through official Pipeline.
6. Positive ProBuilder: inspect supported commands and existing geometry, read-only.
7. Negative legacy Unity: ordinary C# rename must not select Coplay tools.
8. Negative UI: config audit must not load UI art/style guidance.
9. Explicit invocation: project skill by exact folder/path is selected.
10. Ambiguous request: new graph build should reach an authorized build-capable coordinator, never cause a read-only graph explorer to recursively spawn.

After scoped disabling, test root/nested and outside-repository discovery again. Outside checks must use a fresh process without the project's override.

## Measurements and limits

Two temporary auditors; one writer of report/proposals. A final independent read-only review found no blocking issues; its wording clarification about discovery versus actual skill invocation was incorporated. No model comparison, quota attribution, repeated benchmark, or savings claim. The 24,228 number is raw instruction bytes, not tokens. Only the selected integration checks passed; unknowns remain explicit.

Final artifact checks: all 436 inventoried source files retained their hashes; generated JSON parsed; generated local document links resolved; no configured credential value appeared in saved artifacts. A second list_open_scenes query confirmed the same clean ElevatorDemo scene. Unrelated gameplay/UI working-tree changes occurred concurrently and were not touched by this audit.
