---
title: Texture Audio and Mesh RAM Audit 2026-09-09
type: research
domains:
  - performance
  - rendering
  - audio
status: draft
authority: evidence
updated: 2026-09-09
source_commit: 7e7b1ebe
tags:
  - research/package
---

# Texture Audio and Mesh RAM Audit 2026-09-09

## Required Package

### Question and Desired Decision

Have all textures, audio, and meshes been optimized to reduce RAM? **No.** The completed DefaultLocation work optimized a bounded texture-streaming and mesh-readability set. It did not establish a project-wide RAM policy, and long audio tracks still decompress on load.

### Scope and Non-Goals

Static audit of the current working tree at `7e7b1ebe`: TextureImporter, AudioImporter, and ModelImporter metadata under Assets; quality settings; DefaultLocation direct scene/material texture references; ambience asset ownership; and previous implementation records. Counts include vendor/editor/sample assets and are not counts of resident objects or Player build contents. Model importer counts include animation FBXs and do not count individual imported mesh subassets.

`unity status --json` returned no connected Editor. No asset imports, scene loads, tests, builds, or runtime captures were performed. Imported effective formats, native RAM, GPU buffers, bundle duplication, and actual savings remain unmeasured. Generated/embedded font atlases, render targets, package assets, and runtime-created textures/meshes are outside the importer inventory. Existing unrelated working-tree edits were preserved.

### Current System Map

- Editor Low Memory enables quality-level texture streaming and mip limit 1. High Fidelity, Balanced, and Performant retain streaming off and mip limit 0. Standalone defaults to High Fidelity.
- Editor texture streaming and CPU texture loading on demand are enabled in `ProjectSettings/EditorSettings.asset`.
- AmbienceData directly references scene and combat tracks. Its project-scope registration loads the ScriptableObject through the synchronous Addressables helper; AmbienceSystem retains the data and persistent audio sources.
- DefaultLocation still loads all nine dependencies and then its main scene. Compression/import changes do not implement spatial unloading.

### Entry Points, Dependencies, and Consumers

- `ProjectSettings/QualitySettings.asset:10,63,116,169` and `ProjectSettings/EditorSettings.asset:22`.
- `Assets/Settings/Data/AmbienceData.asset:15`, `Assets/Scripts/Services/Audio/AmbienceSystem.cs`, and `Assets/Scripts/Utilities/Extensions/VContainerExt.cs`.
- `Assets/ThirdParty/LeartesStudios/FantasyCastle/HDRP/Art/Textures/` and `Art/Meshes/`.
- `Assets/Art/DefaultLocation/Colliders/` and `Assets/Scenes/DefaultLocation/`.
- [[History/Implementation Records/DefaultLocation Memory Optimization Phase 4 Asset Residency]] and [[Work/Plans/DefaultLocation Memory Optimization]].

### Evidence and Findings

#### Textures: existing compression, partial streaming coverage

The scan found 414 TextureImporters. Read/Write is disabled on 413; the only enabled importer is an MPUIKit editor background. Streaming is explicitly enabled on 66, disabled on 337, and absent in 11 older metadata files. There are no enabled Standalone overrides in the parsed platform blocks.

Default-platform compression flags: 369 Normal, 25 High Quality, 16 uncompressed, and four legacy files without a default-platform block. These are requested settings, not proof of effective GPU formats. Six StarterAssets mobile UI textures explicitly select format 4 (RGBA32); one also has a compression flag of Normal, so the flag alone must not be counted as proof of compression. Eleven of the uncompressed entries are 72-by-72 editor icons. The sample UI images are 1024 or 2048 pixels square, but their presence does not establish runtime use.

All 165 FantasyCastle textures request automatic Normal compression, Read/Write off, and a 2048 maximum. Only 64 enable streaming. A bounded GUID scan from ten DefaultLocation scene files through 72 directly referenced material assets found 144 texture importers: 64 stream and 80 do not. This scan excludes prefab expansion, Volume sky references, and built-in resources. It is not a complete build dependency closure.

Remaining scene-material candidates include `T_Tiles1_BC`, `T_Tiles1_N`, `T_Tiles1_ORM` through `M_Tiles_1.mat`, and `T_Fountain_N` through `M_Fountain.mat`. Their importers still disable streaming. Special masks, alpha, effects, and glass require separate visual review; a non-streaming flag is not automatically a defect.

The previous work changed streaming/residency, not the texture format or compression quality. Normal shipping presets currently disable streaming globally, so enabling it on individual importers does not by itself create a shipping streaming policy. The original HDR sky remains unchanged after the rejected lower-resolution experiment.

Unity distinguishes GPU texture formats from source PNG/JPEG/TGA storage. Supported lower-bit-rate formats reduce texture memory; Crunch adds disk compression but has no runtime memory benefit. GPU texture storage is not interchangeable with process-private RAM on this discrete-GPU workstation. [Unity GPU texture format fundamentals](https://docs.unity3d.com/6000.3/Documentation/Manual/texture-compression-fundamentals.html).

#### Audio: strongest newly identified RAM candidate

All 58 AudioImporters use Decompress On Load. Of these, 57 select Vorbis (`compressionFormat: 1`) and one selects PCM (`Reload.wav`). None select Streaming or Compressed In Memory. All disable force-to-mono and background loading; 40 enable preload and 18 disable it. All retain the default sample-rate setting; serialized override values are not evidence that an override is active. There are no platform overrides.

`Assets/Audio/AmbienceMusic/` contains 29 long stereo tracks. AmbienceData directly references 15 unique tracks: ten ambient tracks and five action loops. All 15 enable preload. WAV-header verification gives stereo, 44.1 kHz, 16-bit PCM sources, lasting approximately 80–231 seconds, with **432,903,168 bytes (412.85 MiB)** of source PCM payload in total. This calculation is not measured native audio RAM, a guaranteed saving, or an upper bound on engine memory.

The AmbienceData reference graph keeps these clips reachable while the service retains the data; changing the active AudioSource clip does not remove the other references. The inspected Addressables loading helper has no matching asset-release owner. This is a residency candidate, not a newly demonstrated runtime leak.

Vorbis reduces stored audio size, but Decompress On Load retains decoded data. Streaming is the first experiment for these long tracks; it introduces buffers and disk/decoder work that require transition and playback checks. Short latency-sensitive SFX should keep a separately measured policy rather than being mass-switched. [Unity AudioClip import settings](https://docs.unity3d.com/6000.3/Documentation/Manual/class-AudioClip.html). Numeric codec mapping was checked against [Unity Audio bindings](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/Audio/Public/ScriptBindings/Audio.bindings.cs): Vorbis is 1, ADPCM is 2.

#### Meshes: partial CPU-copy reduction, not a missing universal compression switch

All 14,759 scanned ModelImporters have Mesh Compression Off. This includes animation FBXs and must not be interpreted as 14,759 loaded meshes. FantasyCastle contains 140 ModelImporters: five disable Read/Write and 135 retain it.

The five optimized visual models are Bridge 02, Tower Bot, chandelier, Gate 01, and candle holder. The Phase 4 record documents 68 collider replacements using derived readable hulls and a reduction from approximately 13.65 million referenced collider triangles to 120,000. Gate 02 remains readable because it has no authored hull. These historical validation results were not rerun in this audit.

Other remaining readable candidates include Tower Roof 3, Bridge 01, Tower Top Deco, Tower Roof 2, and Gate 02. They were shortlisted by source file size only; rank actual imported mesh memory before choosing a batch. Collider cooking, negative/nonuniform transforms, navigation, and script CPU access constrain Read/Write removal.

`ProjectSettings/ProjectSettings.asset:194` configures `VertexChannelCompressionMask: 4054`; this audit does not decode or verify each effective imported channel. Standalone static batching is enabled and dynamic batching disabled at approximately line 490. Static-batch allocations still require Player evidence.

Unity's **Vertex Compression** reduces in-memory vertex-channel precision. Importer **Mesh Compression** principally targets serialized size and can add decompression costs. Eligible vertex compression requires Read/Write off, a non-skinned mesh, platform support, Mesh Compression Off, and compatible batching conditions. Mass-enabling Mesh Compression is therefore not a demonstrated RAM fix and can prevent vertex compression. [Unity compression types](https://docs.unity3d.com/6000.3/Documentation/Manual/types-of-mesh-data-compression.html), [vertex compression requirements](https://docs.unity3d.com/6000.3/Documentation/Manual/configure-vertex-compression.html).

### Options and Tradeoffs

1. Measure and trial streaming on the 15 referenced long ambience tracks. This is the clearest newly found residency candidate. Check preload behavior, playback starts, crossfades, loops, decoder CPU, and buffers.
2. Rank remaining loaded FantasyCastle meshes by actual memory and audit consumers before another Read/Write/collider batch. Verify effective vertex compression in a Player.
3. Review the 80 remaining directly referenced non-streaming world textures and define whether shipping quality tiers should stream. Keep deliberate exclusions; measure textures, sky, and render targets separately.
4. Inspect a fresh Addressables layout and asset release ownership. Import optimization does not fix unnecessary residency or duplicated bundles.

### Risks, Unknowns, and Open Questions

No connected Editor means no current native-memory baseline. Source PCM, source image dimensions, and FBX file size are different from runtime accounting. Asset presence does not establish Player inclusion or simultaneous residency. Earlier successful sequential Editor cycles do not validate current concurrent loading peaks. Target Player budget and acceptable visual/audio tradeoffs remain undefined.

### Recommended Review Questions

- Should the first bounded change cover the 15 referenced ambience tracks, preserving short-SFX latency policy?
- Which shipping quality tiers should enable texture streaming, and at what measured budget?
- Which remaining readable meshes have the largest verified CPU copies and safe consumer requirements?

### Handoff

This is an analysis, not an execution plan or optimization result. Use a connected, clean Editor and a controlled Player baseline for the first approved experiment. No import settings or runtime code were changed by this audit.
