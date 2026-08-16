# Project Instructions

## Graphify Prohibition

- Never use or invoke the `graphify` skill, MCP server, CLI/workflow, or any graphify-specific subagent.
- Do not generate, query, update, or consult graphify outputs. Handle this project directly with the available local tools and instructions.

## Test Execution

- Do not run tests or test suites unless the user directly and explicitly requests test execution.
- Do not treat tests as an automatic verification step; report that they were not run when relevant.

## Unity Tooling

- Use Unity's official `unity` CLI command; do not call `unity-cli` or `unity-mcp-cli`.
- Use direct `unity` commands for project, Editor, build, package, and diagnostic operations.
- For interactive Editor automation through MCP, use the official Unity CLI bridge (`unity mcp`) backed by `com.unity.pipeline`; do not use the legacy Coplay/mcp-for-unity server.

## Dependency Injection

- Treat constructor-injected dependencies as required and rely on VContainer to fail fast when a binding cannot be resolved.
- Assign injected dependencies directly. Do not add `?? throw new ArgumentNullException(nameof(...))` constructor boilerplate.
- Do not add defensive null guards, routine guard exceptions, or exception-heavy control flow. Let required-reference failures surface naturally at the point of use.
- Use null-conditional invocation for optional events instead of throwing when no subscriber exists.
