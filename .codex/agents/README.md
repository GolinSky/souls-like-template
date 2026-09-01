# Sol High multi-agent setup

Use this set only after a fresh parent session can successfully spawn gpt-5.6-luna.

## Recommended role mapping

| Agent | Model | Effort | Reason |
|---|---|---:|---|
| context_curator | gpt-5.6-luna | low | Narrow retrieval and compression |
| csharp_worker | gpt-5.6-terra | high | Production edits need thorough implementation checks |
| graph_explorer | gpt-5.6-luna | medium | Code-path mapping and impact analysis |
| unity_architect | gpt-5.6-sol | high | Ambiguous architecture and migration decisions |
| unity_operator | gpt-5.6-terra | medium | Stateful Unity mutations need reliable verification |
| unity_profiler | gpt-5.6-terra | high | Evidence interpretation and competing hypotheses |
| unity_reviewer | gpt-5.6-terra | high | Lifecycle, async, serialization, and regression edge cases |
| unity_test_runner | gpt-5.6-luna | low | Clear and repeatable validation work |

## Operational rules

- Use one writer at a time for overlapping Unity/C# scope.
- Run explorers, curator, reviewer, and profiler in parallel only when their scopes do not overlap unnecessarily.
- Keep the parent/orchestrator on GPT-5.6 Sol High as the final decision-maker.
- Each child has `[agents] enabled = false` so it cannot recursively create more subagents.
- Only the MCP server required by the role is enabled. This reduces tool-selection noise and accidental cross-role work.
- `graph_explorer` is now an actual Graphify specialist. If you intentionally do not want Graphify, rename it to `code_explorer`, disable Graphify, and remove Graphify from the description.
- Project skill packages live under `.agents/skills`. Each agent TOML names its required workflow skill; the parent handoff names only applicable conditional/domain skills.
- Obsidian context is resolved by exact registry key through `$soulslike-context`; the checked-in Markdown fallback keeps the workflow available when Obsidian is offline.
