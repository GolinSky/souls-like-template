# Project Agent Policy

`AGENTS.md` is the repository policy authority. Follow its tool routing, Unity asset-persistence rules, and role/skill boundaries.

For interactive Unity Editor work, use the official `unity mcp` bridge backed by `com.unity.pipeline`. Do not use legacy `unity-cli`, `unity-mcp-cli`, or Coplay/mcp-for-unity commands.

For ProBuilder graybox geometry, read the canonical project workflow at
`.agents/skills/soulslike-probuilder/SKILL.md` and compose it with the role skills
required by `AGENTS.md`. Do not copy upstream server-specific ProBuilder commands
into this project's execution workflow.
