---
name: unity-package-management
description: Add, remove, upgrade, or inspect Unity packages through the project's supported Package Manager and official Unity/Pipeline workflow. Use for a bounded UPM package change after confirming the local resolved state.
---

# Unity Package Management

Use the Unity Package Manager's supported API or an already exposed project
command. Inspect `Packages/manifest.json`, `Packages/packages-lock.json`, the
resolved local package list, and the connected Editor's command schemas before
deciding that a package change is needed. Do not hand-edit package files as a
shortcut and do not modify `Library/PackageCache`.

For a requested addition, first resolve the actual package id and a compatible
candidate version from the Package Manager or official Unity documentation.
If the package is already installed and works, record that state instead of
upgrading it. Keep unrelated Editor, render-pipeline, navigation, input, and
bridge versions unchanged unless a demonstrated compatibility issue requires a
separately authorized change.

UPM operations are asynchronous. Use a callback or update-tick completion path
when working through `UnityEditor.PackageManager.Client`; do not block the
Editor main thread waiting for a request. In the user's active Editor, do not
use a headless installer pattern and never call `EditorApplication.Exit(...)`.
Do not launch another Editor against the open worktree for package work.

After the operation, wait for package resolution/domain reload as needed, then
verify the manifest and lockfile diff, the resolved package version, compilation
state, and the relevant command discovery. Report a failed or interrupted
resolution rather than blindly retrying it.

Read [package selection notes](references/select-packages.md) only when a
request needs help choosing a package. This project adaptation is informed by
Unity Technologies' pinned upstream source recorded in
[`third-party-notices/probuilder-sources.md`](../../third-party-notices/probuilder-sources.md).
