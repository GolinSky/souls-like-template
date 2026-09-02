# Graphify reference: commit hook and agent-policy integration

Load this when the user asks to install the post-commit hook or add persistent Graphify routing to an agent-policy file.

## For git commit hook

Install a post-commit hook that auto-rebuilds the graph after every commit. No background process needed - triggers once per commit, works with any editor.

```bash
graphify hook install    # install
graphify hook uninstall  # remove
graphify hook status     # check
```

After every `git commit`, the hook detects which code files changed (via `git diff HEAD~1`), re-runs AST extraction on those files, and rebuilds `graph.json` and `GRAPH_REPORT.md`. Doc/image changes are ignored by the hook - run `/graphify --update` manually for those.

If a post-commit hook already exists, graphify appends to it rather than replacing it.

---

## For agent-policy integration

In an `AGENTS.md`-first repository, add the routing policy directly to `AGENTS.md`: use an existing graph for broad codebase questions, verify important claims against live source, and rebuild only when an update is explicitly requested. Do not run a client-specific installer that creates a second policy authority.

For a project that deliberately uses Claude Code and `CLAUDE.md`, Graphify also provides this compatibility command:

```bash
graphify claude install
```

This writes a `## graphify` section to the local `CLAUDE.md`. Treat that file as client compatibility guidance and keep the repository's declared authority order intact.

```bash
graphify claude uninstall  # remove the section
```
