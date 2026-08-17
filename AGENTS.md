# Project Instructions


## Unity Tooling

- Use Unity's official `unity` CLI command; do not call `unity-cli` or `unity-mcp-cli`.
- Use direct `unity` commands for project, Editor, build, package, and diagnostic operations.
- For interactive Editor automation through MCP, use the official Unity CLI bridge (`unity mcp`) backed by `com.unity.pipeline`; do not use the legacy Coplay/mcp-for-unity server.

## Dependency Injection

- Treat constructor-injected dependencies as required and rely on VContainer to fail fast when a binding cannot be resolved.
- Assign injected dependencies directly. Do not add `?? throw new ArgumentNullException(nameof(...))` constructor boilerplate.
- Do not add defensive null guards, routine guard exceptions, or exception-heavy control flow. Let required-reference failures surface naturally at the point of use.
- Use null-conditional invocation for optional events instead of throwing when no subscriber exists.
