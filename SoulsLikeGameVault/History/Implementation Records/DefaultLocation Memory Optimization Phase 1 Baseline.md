---
title: "DefaultLocation Memory Optimization Phase 1 Baseline"
type: implementation-record
domains:
  - performance
  - rendering
  - scenes
status: done
authority: historical
updated: 2026-09-07
aliases: []
tags:
  - history/change
---

# DefaultLocation Memory Optimization Phase 1 Baseline

## Implementation Record Contract

### Outcome

Completed the safe Editor portion of phase 1 using sequential scene loading. The unrelated `ElevatorDemo` scene was closed after confirming it was clean; no project asset or scene content was changed.

### Why

The existing crash evidence shows a native `VertexData` allocation failure after the zones load. A controlled baseline was required before quality, importer, mesh, or loader changes.

### Changed Files and Assets

- Updated plan status/checklist: `SoulsLikeGameVault/Work/Plans/DefaultLocation Memory Optimization.md`.
- Added this measurement record. No Unity assets, scenes, import settings, or runtime code were modified.

### Decisions and Tradeoffs

- Unity 6000.3.11f1 Editor, HDRP 17.3.0, Addressables 2.9.1, StandaloneWindows64, High Fidelity quality (index 0), active location scenes clean and stopped.
- Loads were performed one scene at a time: empty/bootstrap, main only, Rocks, Zone_01 through Zone_08. The known concurrent nine-way load was not rerun.
- Unity allocator counters are runtime telemetry, not OS private bytes and not dedicated VRAM. The immediate Zone_08 sample was treated as the sequential observed peak; the later all-ten sample was recorded separately after settling.

### Validation Evidence

| State | Allocated | Reserved | Increment from prior sample |
|---|---:|---:|---:|
| Empty/bootstrap | 2.430 GB | 7.052 GB | — |
| Main only | 2.824 GB | 7.312 GB | +394 MB allocated |
| + Rocks | 2.829 GB | 7.312 GB | +4.9 MB |
| + Zone_01 | 2.838 GB | 7.312 GB | +9.6 MB |
| + Zone_02 | 2.846 GB | 7.312 GB | +7.7 MB |
| + Zone_03 | 2.862 GB | 7.312 GB | +15.8 MB |
| + Zone_04 | 2.872 GB | 7.312 GB | +10.5 MB |
| + Zone_05 | 2.893 GB | 7.312 GB | +20.3 MB |
| + Zone_06 | 2.900 GB | 7.312 GB | +7.0 MB |
| + Zone_07 | 2.913 GB | 7.312 GB | +13.6 MB |
| + Zone_08 observed peak | 2.923 GB | 7.312 GB | +9.2 MB |
| All ten settled | 2.882 GB | 7.081 GB | non-monotonic settling |

All ten scenes ended loaded and clean; Unity status was ready; captured Console errors were zero. Final OS sample: 61.81/69.70 GB committed (88.7%), 4.26 GB free physical memory of 31.16 GB, main Unity Editor private bytes 12.62 GB, AssetImportWorker processes 1.44 GB and 1.20 GB, and RTX 5070 memory 3,707/12,227 MiB used.

The preserved crash log remains at `%LOCALAPPDATA%/Temp/Unity/Editor/Crashes/Crash_2026-09-07_153032988/Editor.log`: first failure is a 3,397,312-byte `VertexData` allocation at `Mesh/VertexData.cpp:261`, with `ALLOC_GFX` 4,756,527,144 bytes and `ALLOC_DEFAULT` 2,508,235,588 bytes.

### Documentation Updated

- [[Work/Plans/DefaultLocation Memory Optimization]]
- [[Work/Issues/DefaultLocation Memory and Rendering Issues]] remains evidence-only and open.

### Follow-Up

Run the separate Development Player baseline only after defining a Player scene/build configuration and recovering sufficient system commit headroom. Record fixed frame dimensions and camera. Phase 2 can then create the Editor Low Memory preset from the measured baseline.
