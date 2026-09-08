# Project Pipeline integration notes

The connected Editor's command catalog is the contract. Start with
`unity command --json` or `unity list --json`, then pass parameters as
`--parameter value`; do not invent command names, aliases, or parameter names.

`[CliCommand]` methods belong in an Editor assembly and must be static. Unity
state and Editor API work requires `MainThreadRequired = true` (the default).
After a command change, use the supported recompile command if it is present,
poll its status to completion, and rediscover the command before relying on it.

When a package import or domain reload interrupts the bridge, reconnect to the
same verified project and inspect the resolved package/compile state before
retrying. Do not launch a second Editor against this worktree merely to make a
bridge example work.

Do not treat upstream headless examples as instructions for a live Editor.
In particular, never add or execute `EditorApplication.Exit(...)` in a command
used by the user's active Editor.
