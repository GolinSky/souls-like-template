# ProBuilder setup

Use the project `unity-package-management` skill and the official `unity` CLI.
Inspect the local Editor version, manifest, resolved packages, active Graphics
and Quality render pipeline, existing errors, and open scenes first. In a fresh
worktree, Git LFS pointers must be hydrated before binary assets can import.

The selected candidate is ProBuilder `6.1.2` for Unity `6000.3.11f1`. The registry
package declares Unity `6000.0` minimum and dependencies on Shader Graph
`17.0.3`, Settings Manager `1.0.3`, IMGUI and physics. The project already uses
Shader Graph/HDRP `17.3.0`; preserve those versions. Do not upgrade Pipeline or
other packages as a side effect of setup.

Discover `package_search`, `package_add`, `package_status`, and `recompile_status`
before use. Package addition is asynchronous: an acknowledgment is not proof
of successful resolution or compilation. Reconnect after domain reload and
check the manifest/lockfile and actual compiler result. Never retry an uncertain
mutation until its status has been inspected.

Unity's [installation page](https://docs.unity3d.com/Packages/com.unity.probuilder@6.1/manual/installing.html)
describes render-pipeline support samples. The actual `6.1.2` package declares
only Editor Examples and Runtime Examples; it does not expose an HDRP support
sample. Do not invent that sample or install a different version to obtain it.
Assign a verified existing HDRP material explicitly. The package also ships a
Standard Vertex Color shader graph; inspect its targets before using it.

Use the live Editor's supported package commands or public UPM API, never a
headless example that exits the user's Editor. No second MCP server, package
cache patches, global configuration, or Obsidian changes are needed.
