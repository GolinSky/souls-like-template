---
title: Unity UI Asset Layout and Troubleshooting Rules
type: guide
domains:
  - ui
  - unity
  - ui-assets
status: current
authority: required
verified: 2026-09-12
updated: 2026-09-12
context_keys:
  - ui-asset-layout
aliases:
  - UNITY_UI_ASSET_LAYOUT_AND_TROUBLESHOOTING_RULES
  - Unity UI Asset Bounds Layout and Troubleshooting
source_urls:
  - https://drive.google.com/file/d/1XntBalmtuiFXjKqI0r0bqa2IEYUif_fm/view
companion: "[[Dark Fantasy UI Style and Asset Rules]]"
tags:
  - agent/context
---

# Unity UI Asset Layout and Troubleshooting Rules

## Project Application Rules

- For every Unity UI asset import, UI prefab/layout, UI rendering, or UI interaction task, resolve `ui-asset-layout`, read this heading and [[#Task-Conditional Reading Map]], then read only the mapped detailed headings needed for the task.
- Keep the logical component as the authoritative layout body. Separate semantic body, content, effect, and hit bounds; decorative overflow never defines anchors, grid spacing, navigation, or pointer area.
- Source statements marked **MUST** are required for relevant work unless higher-priority project guidance or live project evidence conflicts. Record the conflict and do not silently replace existing UI architecture, factories, presenters, input ownership, or asset-loading conventions.
- Numeric tables, package-version references, presets, and worked examples described as initial, recommended, default, or starting values are proposed defaults. Inspect installed Unity and package versions and current project settings before applying them; they do not authorize upgrades or global configuration changes.
- Existing `AGENTS.md` Unity CLI requirements, serialized-asset persistence rules, clean-scene test safety, and the prohibition on normal Play Mode test runs outrank source examples. This note does not change them.

## Task-Conditional Reading Map

| Task signals | Read these detailed headings after the mandatory rules |
|---|---|
| New/imported UI asset, Sprite settings, alpha, texture, atlas, or frame | `2. A vocabulary of bounds and units`, `3. The core pattern: logical body with independent visual overflow`, `4. Asset metadata: make the geometry explicit`, `5. Formats, resolution, and export sizes`, `6. uGUI import presets and native sizing`, `7. Scalable backgrounds, nine-slicing, and tiling`, `9. Alpha, color, and filtering problems`, `10. Atlases, padding, and compression`, `18. Agent workflow and acceptance gates` |
| UI prefab, Canvas, RectTransform, panel, slot, ScrollRect, safe area, or responsive layout | `2. A vocabulary of bounds and units`, `3. The core pattern: logical body with independent visual overflow`, `7. Scalable backgrounds, nine-slicing, and tiling`, `8. Responsive Canvas and layout ownership`, `11. Masks, effects, sorting, and rendering paths`, `12. Text, localization, and icon alignment`, `13. Interaction and focus ownership`, `18. Agent workflow and acceptance gates` |
| UI state, focus, input routing, tooltip, modal, inventory grid, or comparison | `3. The core pattern: logical body with independent visual overflow`, `11. Masks, effects, sorting, and rendering paths`, `12. Text, localization, and icon alignment`, `13. Interaction and focus ownership`, `16. Reusable implementation recipes`, `18. Agent workflow and acceptance gates` |
| UI performance or a visual/layout defect | `9. Alpha, color, and filtering problems`, `10. Atlases, padding, and compression`, `11. Masks, effects, sorting, and rendering paths`, `14. Performance and resource patterns`, `15. Troubleshooting catalog`, `18. Agent workflow and acceptance gates` |
| UI Toolkit | `17. UI Toolkit boundary`, plus the detailed headings relevant to the task; do not transfer uGUI component instructions literally. |

## Source Specification

# Unity UI Asset, Layout, and Troubleshooting Rules

## 1. Scope and operating contract

This document turns an art direction into reliable, reusable Unity UI assets and prefabs. It emphasizes a common failure: transparent padding, shadows, and glows accidentally becoming the geometry used for child layout.

**Default implementation:** screen-space uGUI using `Canvas`, `RectTransform`, `Image`, layout components, `TextMeshProUGUI`, and the project's existing input and presentation architecture. The recommendations are applicable to restrained dark-fantasy interfaces, but are not tied to a particular game.

Research used Unity 6.3 documentation (`6000.3`), uGUI 2.0 documentation and public source, and relevant Input System and TextMeshPro documentation. A documentation package version is **not** an instruction to upgrade or install that version. Before modifying a project, inspect `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, and `Packages/packages-lock.json`. Confirm APIs and behavior against installed packages. The supplied utility and recipes require compilation and Play Mode validation in the target project; they were not executed in that project during preparation of this guide.

### Non-negotiable agent instructions

1. **Let the logical component own its size.** Its textures do not own layout merely because they have a particular pixel size.
2. **Separate body, content, effect, and hit bounds.** A glow is a decorative child, not the parent against which content is fitted.
3. **Use one authoritative layout writer per axis.** Do not let a layout group, a fitter, a tween, and a script repeatedly overwrite the same rectangle.
4. **Preserve semantic registration across states.** Default, hover, focus, and selected variants must not move the item or change the hit target.
5. **Use nine-slicing for suitable resizable surfaces, not for arbitrary illustrations.** Sprite borders are not automatic content padding.
6. **Make decoration non-interactive.** Disable its raycast participation, and keep hidden UI from intercepting input.
7. **Validate final compositing and final rendered size.** A clean PNG preview does not prove that a compressed, scaled, masked UI asset is correct.
8. **Preserve project structure.** Reuse current factories, presenters, input ownership, asset loading, and prefab conventions. Do not introduce a parallel UI framework or service for each visual concern.

`MUST` denotes a production requirement; `SHOULD` denotes the default with documented exceptions. Measurements in this document are recommended project defaults unless explicitly identified as Unity behavior.

## 2. A vocabulary of bounds and units

### 2.1 Different rectangles have different jobs

| Term | Meaning | Typical owner |
|---|---|---|
| Source canvas | Complete exported image, including transparent texels | PNG file and asset metadata |
| Semantic body | The intended surface or slot boundary, excluding external effects | Component root `RectTransform` |
| Content rectangle | Safe area inside the body for icon, text, or child controls | Explicit `ContentRoot` |
| Effect rectangle | Space needed for glow, shadow, feathering, or ornament overflow | Decorative child |
| Hit rectangle | Area that accepts pointer interaction | Interactive root or explicit hit-area graphic |
| Nine-slice borders | Source strips that should preserve edge/corner proportions | Sprite Editor border values |
| Optical center | Perceived visual center of a symbol or illustration | Asset metadata and icon placement |

None of these should be inferred from another without an explicit contract. An alpha bounding box is useful for diagnostics, but is not a reliable definition of a semantic body: a soft glow can occupy nearly the entire image, and a sword's empty corners are intentional.

Unity anchors and offsets relate a child to its **parent rectangle**. They do not inspect texture alpha to discover a visible frame. A Sprite pivot, a tight mesh, `Preserve Aspect`, or a nine-slice border does not change that rule.[^rect][^layout]

### 2.2 Keep these units separate

- **Source texels:** coordinates in the exported PNG or untrimmed source sprite rectangle.
- **Reference UI units:** component sizes before the Canvas scale factor, such as a 96-unit slot.
- **Rendered screen pixels:** the actual device output after canvas scaling and any deliberate UI magnification.
- **CSS pixels:** web accessibility measurements; these are not interchangeable with Unity units.

Write dimensions with their units. Avoid ambiguous notes such as “padding = 20” or “make it 2×.” A correct note is: “96 × 96 reference-unit body; 8-unit content inset; 512 × 512 source canvas; semantic body at x=96, y=96, w=320, h=320 source texels.”

## 3. The core pattern: logical body with independent visual overflow

### 3.1 Recommended hierarchy

```text
SlotRoot                                  96 × 96 logical units
  RectTransform + LayoutElement + Button
  HitArea Image                           transparent; Raycast Target = true
  FXRoot                                  fills SlotRoot; no interaction
    Glow                                  expands beyond SlotRoot
  Body                                    fills SlotRoot; Raycast Target = false
  ContentRoot                             stretch; inset 8 on each side
    ItemIcon                              fitted to 80 × 80 content area
  FocusBorder                             follows body, not glow
  EquippedBadge                           explicit corner anchor
  QuantityLabel                           TextMeshProUGUI; separate from art
```

`HitArea Image` denotes an Image component on the root, not a requirement to create another hierarchy level. A zero-alpha Image can serve as the rectangular hit graphic with the normal alpha-hit threshold of zero. The `Button` can target a separate body graphic, or a presenter can explicitly control visual states. Use one state-color owner; do not combine a `Selectable` color transition and an independent tween that both overwrite the same `Image.color`.

The root's size is the **semantic body size**. `ContentRoot` is inset from that body. `Glow` is allowed to extend outward without changing root size, grid spacing, navigation, or pointer area. Make all decorative Images and text non-raycast targets. Where a decorative transform is a direct child managed by a layout group, apply `LayoutElement.ignoreLayout = true`; that flag does not repair arbitrary descendant layout conflicts.

A parent layout group should normally see `SlotRoot`, not the individual layers of a slot. Place its `LayoutElement` preferred/minimum dimensions on the logical root. Avoid installing a layout group inside a simple layered slot when anchors already express the geometry.

### 3.2 Worked example: a glow that contains substantial transparent margin

**Source asset:** 512 × 512 texels. The intended square body occupies x=96…416 and y=96…416, giving a 320 × 320 semantic body. There are 96 texels of effect space on each side.

**Desired body in Unity:** 96 × 96 reference units.

```text
source-to-UI scale        = 96 / 320 = 0.3 UI units per source texel
full glow rectangle      = 512 × 0.3 = 153.6 UI units
outward extent per side  =  96 × 0.3 =  28.8 UI units
content rectangle        =  96 − 2 × 8 = 80 UI units
```

Correct result: the logical parent remains **96 × 96**, the glow Image renders across **153.6 × 153.6**, and the item is fitted inside **80 × 80**. No child is anchored to the outer glow rectangle.

Two incorrect approaches:

- Fitting the complete 512-texel glow into a 96-unit Image makes the semantic body only **60 units** wide: `96 × 320 / 512`.
- Enlarging the logical parent to 153.6 units makes layout, selection spacing, and child anchors follow the glow margin instead of the intended 96-unit body.

### 3.3 General mapping formula

For source dimensions `W × H` and effect margins `L, B, R, T`:

```text
bodySourceWidth  = W − L − R
bodySourceHeight = H − B − T
scaleX           = desiredBodyWidth  / bodySourceWidth
scaleY           = desiredBodyHeight / bodySourceHeight
```

For an undistorted illustration or fixed-aspect glow, require `scaleX ≈ scaleY`. Then either set a stretched decorative child with:

```text
offsetMin = (−L × scaleX, −B × scaleY)
offsetMax = ( R × scaleX,  T × scaleY)
```

or encode proportional expansion directly into anchors:

```text
anchorMin = (−L / bodySourceWidth, −B / bodySourceHeight)
anchorMax = (1 + R / bodySourceWidth, 1 + T / bodySourceHeight)
offsetMin = offsetMax = (0, 0)
```

For the example above, anchors are `(-0.3, -0.3)` and `(1.3, 1.3)`. Anchor coordinates outside 0…1 are intentional. They express visual overflow; they do not change the parent rectangle.

**Boundary of this technique:** proportional expansion is appropriate for fixed-aspect slots and uniformly scaled icons. It will stretch blur widths if the body is resized independently along each axis. For arbitrarily wide panels, use a properly designed nine-sliced effect, separate corner/edge pieces, or a compatible procedural material with constant UI-unit falloff.

### 3.4 Reference utility for authoring or initialization

This utility expresses the anchor method without an `Update()` loop. Coordinates in `semanticBodyPx` are bottom-left-origin and relative to the exported, untrimmed source canvas, not an atlas page. Use a direct decorative child with no other component driving its anchors or offsets.

```csharp
using System;
using UnityEngine;

public static class UiSemanticBounds
{
    public static void PlaceArtAroundBody(
        RectTransform body,
        RectTransform art,
        Vector2 sourceCanvasPx,
        Rect semanticBodyPx)
    {
        if (body == null) throw new ArgumentNullException(nameof(body));
        if (art == null) throw new ArgumentNullException(nameof(art));
        if (art.parent != body)
            throw new ArgumentException("Art must be a direct child of body.", nameof(art));

        if (!PositiveFinite(sourceCanvasPx.x) || !PositiveFinite(sourceCanvasPx.y))
            throw new ArgumentOutOfRangeException(nameof(sourceCanvasPx));

        if (!Finite(semanticBodyPx.x) || !Finite(semanticBodyPx.y) ||
            !PositiveFinite(semanticBodyPx.width) || !PositiveFinite(semanticBodyPx.height) ||
            semanticBodyPx.xMin < 0f || semanticBodyPx.yMin < 0f ||
            semanticBodyPx.xMax > sourceCanvasPx.x ||
            semanticBodyPx.yMax > sourceCanvasPx.y)
        {
            throw new ArgumentOutOfRangeException(nameof(semanticBodyPx),
                "Semantic body must be a positive rectangle inside the source canvas.");
        }

        float left = semanticBodyPx.xMin;
        float bottom = semanticBodyPx.yMin;
        float right = sourceCanvasPx.x - semanticBodyPx.xMax;
        float top = sourceCanvasPx.y - semanticBodyPx.yMax;

        art.localScale = Vector3.one;
        art.localRotation = Quaternion.identity;
        art.pivot = new Vector2(0.5f, 0.5f);
        art.anchorMin = new Vector2(
            -left / semanticBodyPx.width,
            -bottom / semanticBodyPx.height);
        art.anchorMax = new Vector2(
            1f + right / semanticBodyPx.width,
            1f + top / semanticBodyPx.height);
        art.offsetMin = Vector2.zero;
        art.offsetMax = Vector2.zero;
        var position = art.anchoredPosition3D;
        position.z = 0f;
        art.anchoredPosition3D = position;
    }

    private static bool Finite(float value) =>
        !float.IsNaN(value) && !float.IsInfinity(value);

    private static bool PositiveFinite(float value) => Finite(value) && value > 0f;
}
```

Assumptions: the decorative Image uses the complete source mapping; no custom UV crop, incompatible mesh trimming, `AspectRatioFitter`, or `Preserve Aspect` letterboxing changes that mapping. Keep source-body and parent-body aspect ratios equal for uniform scaling. Reapply only when metadata changes, not every frame. The code checks geometric inputs, not those component-level assumptions.

### 3.5 Legacy assets that cannot yet be separated

For a combined frame-plus-glow asset, introduce a **semantic body wrapper** inside its full image rectangle. In the 512 example, its normalized anchors are `(0.1875, 0.1875)` and `(0.8125, 0.8125)`. Put content under that wrapper, then apply content padding there.

This repairs child alignment but does **not** automatically repair the outer layout footprint. If a grid still measures the oversized full-image parent, the spacing remains wrong. Prefer eventually reversing the relationship: make the semantic body the layout root and let the art overflow it, as in section 3.1.

Do not crop away glow to solve this problem. Do not tighten a Sprite mesh and expect it to redefine a RectTransform. Do not use alpha-based hit testing as a replacement for semantic geometry.

## 4. Asset metadata: make the geometry explicit

Store source and layout contracts in a small sidecar JSON, an existing project asset-definition type, or a ScriptableObject that the current pipeline already supports. Do not create a new framework solely to hold these values.

```json
{
  "id": "ui_fx_cell_focus_gold_v01",
  "source_file": "ui_fx_cell_focus_gold_v01.png",
  "coordinate_origin": "bottom-left",
  "source_canvas_px": [512, 512],
  "semantic_body_px": { "x": 96, "y": 96, "width": 320, "height": 320 },
  "body_reference_units": [96, 96],
  "content_padding_units_lbrt": [8, 8, 8, 8],
  "optical_center_normalized": [0.5, 0.5],
  "sprite_border_px_lbrt": [0, 0, 0, 0],
  "usage": "decorative-overflow",
  "alpha_encoding": "straight",
  "sampling": "bilinear-clamp",
  "raycast": false,
  "sizing_mode": "semantic-body-proportional"
}
```

`content_padding_units_lbrt` belongs to the logical component contract; it is not a claim that the glow itself has a content container. A nine-sliced frame would declare a different sizing mode and actual border values.

Record coordinate origins explicitly. Image-editing tools frequently report top-left coordinates, while the formulas here use bottom-left. Convert using `yBottom = imageHeight − yTop − rectHeight`. Unity's Sprite border order is **left, bottom, right, top**; do not silently substitute the CSS-style order top/right/bottom/left.[^image-source]

Require all variants in a component family to use compatible source canvases, semantic bounds, pivot conventions, and optical registration. If packing or trimming changes the source representation, preserve or convert the metadata during import. Never interpret original PNG coordinates as atlas-page coordinates.

## 5. Formats, resolution, and export sizes

### 5.1 Production formats

For ordinary colored UI art with transparency, use **PNG with RGB plus alpha, 8 bits per channel**, normally called 32-bit RGBA. PNG's alpha is unassociated/straight, not premultiplied. Export real transparency, not a checkerboard drawn into RGB.[^png]

Use opaque RGB assets when alpha is genuinely unnecessary. Keep text, numeric values, state badges, and control labels out of background textures. Keep vector or layered source files for editing; only use vector assets at runtime through a pipeline deliberately supported by the project. A filename ending in `.svg` is not an instruction to treat it as an ordinary Sprite PNG.

Lossless disk export does not guarantee lossless runtime rendering. Unity import resizing, GPU compression, filtering, atlas processing, materials, and compositing still affect the result.

### 5.2 Size by final use, not by a universal “512 px” rule

The following are starting ranges, not engine requirements:

| Asset family | Typical visible size at 1080p baseline | Sensible source strategy |
|---|---:|---|
| Tiny functional glyph | 16–32 rendered px | Clean vector/raster master; export around 64–128 px and verify the smallest size |
| Category symbol | 32–48 rendered px | 128–256 px can suffice; a 512 px generation master is acceptable after cleanup |
| Inventory item | 64–128 rendered px | 256–512 px, depending on body occupancy and detail |
| Large item preview | 256–512 rendered px | 512–1024 px; consider a separate detail asset rather than enlarging a thumbnail |
| Thin divider or simple line | Layout-defined | A small stretchable asset or geometry, not a full-screen PNG |
| Reusable panel frame | Variable | Compact 64–256 px nine-slice source with deliberately sized corners |
| Fixed-aspect glow | Usually one slot or row | 256–512 px including measured effect margin; calculate body resolution separately |
| Grain or repeatable material | Tiled | 128–256 px seamless source as an initial budget |
| Full-screen atmosphere | Screen-filling | 1920 × 1080 initial master; derive other exports only when quality and budget require them |

These ranges assume the intended visual style, not pixel art. Decide target resolutions, UI magnification, and handheld/TV viewing before finalizing sizes.

For a source canvas of width `W`, whose semantic body has width `C`, the minimum source-canvas width needed to avoid enlarging its body is:

```text
requiredCanvasWidth >= maximumRenderedBodyWidth × W / C
```

For the glow example, a body rendered at 192 px needs at least `192 × 512 / 320 = 307.2` source texels across the full asset. The 512 master is sufficient. If a 4K canvas doubles scale and an additional UI magnification doubles it again, a 96-unit body becomes 384 rendered px. Preserving the same body fraction then requires at least **615 source texels**, so a 768 or 1024 export is more appropriate.

Avoid assuming that 4K output and 200% UI settings are the same scaling operation. Test their combined effect. Conversely, do not automatically ship every small icon at 4K: its transparent margins still consume texture area.

Modern UI source textures need not all be power-of-two. Do not add arbitrary padding just to reach a power-of-two dimension. Respect actual platform, compression, atlas, and mip constraints, but preserve the semantic metadata when any padding or resizing is introduced.

## 6. uGUI import presets and native sizing

### 6.1 Baseline preset for colored screen-space UI

Use this as a reference-quality baseline, then profile platform-specific overrides.[^texture]

| Setting | Baseline | Exception or reason |
|---|---|---|
| Texture Type | Sprite (2D and UI) | Use the appropriate type for explicit data textures or RenderTextures |
| Sprite Mode | Single for independent files | Multiple only for intentionally authored sheets |
| Pixels Per Unit | 100 as a project convention | Match the project's existing convention; this is not a universal physical size |
| Mesh Type | Full Rect for frames, glows, and predictable UI quads | Tight meshes may suit some art; they do not redefine child layout |
| Alpha Source | Input texture alpha | Data assets may intentionally differ |
| Alpha Is Transparency | On for ordinary straight-alpha color art | Do not apply indiscriminately to packed data channels |
| sRGB | On for color images | Off for linear data/masks when the material expects data, not display color |
| Filter Mode | Bilinear | Point only for deliberate pixel art or diagnostic comparison |
| Wrap Mode | Clamp | Repeat only for an explicitly designed, compatible tiling path |
| Generate Mip Maps | Off as the initial fixed screen-space UI preset | Strong minification and world-space UI can benefit from mips; test and separate presets |
| Compression | None for the visual reference pass | Use supported alpha-capable platform formats only after comparison |
| Max Size | High enough to preserve the required source detail | Check platform overrides and atlas output, not just the source file |
| Read/Write | Off | Enable only for a justified CPU pixel-access path; it adds a readable copy |

Inspect the **imported texture and built target**, not only the Inspector thumbnail. Do not apply one preset to color artwork, monochrome data masks, world-space labels, and screen-space frames indiscriminately.

`Alpha Is Transparency` helps prevent edge artifacts by extending color information into transparent regions. It does not recover pixels that were already flattened against an unwanted background.[^texture]

### 6.2 `SetNativeSize()` is not “fit the visible object”

For a standard uGUI Image, native size follows the Sprite rectangle and the relationship between Sprite PPU and the Canvas reference PPU. It is not calculated from the opaque body or the usable content area. The public implementation also sets `anchorMax` to `anchorMin`, so invoking it on a stretched element changes its layout assumptions.[^image-source]

Conceptually:

```text
nativeWidthUI = spriteRectWidthPx × canvasReferencePPU / spritePPU
```

With both PPU values at 100, a 512-wide Sprite has a 512-unit native width, even if most of the image is transparent. Do not call `SetNativeSize()` on responsive panel backgrounds, on layout-driven cells, or after each sprite swap without a deliberate sizing contract.

Prefer explicit logical dimensions for ordinary components. A high-resolution icon can still occupy a 48-unit Image. `Preserve Aspect` protects the artwork from distortion inside that rectangle; it does not alter the rectangle or align siblings to visible alpha.

### 6.3 Nine-slice border scale and PPU

For an unshrunk sliced Image, the working relationship is:

```text
borderWidthUI = borderWidthPx × canvasReferencePPU
                / (spritePPU × image.pixelsPerUnitMultiplier)
```

At multiplier 1, an 8-px border at Sprite PPU 100 and reference PPU 100 occupies 8 UI units. A 2× export with a 16-px border needs Sprite PPU 200 to preserve the same 8-unit edge. Keeping PPU at 100 would make it 16 units.

Changing only the Image rectangle does not reliably preserve the intended source border scale. When a target rectangle becomes smaller than the combined border widths/heights, uGUI adjusts the borders to fit; do not promise invariant corners below that minimum.[^image-source]

Adopt either an explicit fixed-PPU authoring policy with correctly sized border sources, or a documented source-density policy. Do not mix both by accident.

## 7. Scalable backgrounds, nine-slicing, and tiling

### 7.1 A correct nine-slice source

Place stable corners in the four corner regions, repeating or stretch-tolerant edge detail in the border strips, and low-frequency content in the center. Set Sprite Editor borders, then use `Image.Type = Sliced`. Test the smallest, largest, widest, and tallest supported rectangle.[^image]

The border is a rendering instruction. **It does not create a child content inset.** Set `ContentRoot` offsets or layout-group padding explicitly.

Use separate layers for:

- A flat or softly textured panel surface.
- A nine-sliced frame with a transparent or optional center.
- Grain that should tile rather than stretch.
- A focus effect or shadow that needs independent opacity and overflow.

This separation also avoids unintentionally tinting a panel's text or item illustrations when changing its background color.

### 7.2 Where nine-slicing fails

Do not stretch an illustration, unique central engraving, circular seal, or non-repeatable ornament as though it were an ordinary panel. Do not put sharp decorative transitions exactly on uncertain slice seams. Use separately anchored ornaments when geometry must stay rigid.

For a scalable glow, design the edge and corner falloff for slicing. Reserve enough pixels for the glow, transition, and a usable stretchable center. Otherwise the glow becomes thicker horizontally than vertically or produces visible corner joins. A focus outline and its soft halo can be separate layers.

### 7.3 Repeat is not a universal atlas operation

A texture's repeat sampling repeats its full sampled texture domain; this is not automatically equivalent to repeating one region of a packed atlas. uGUI's tiled Image implementation can generate tiled geometry for packed sprites, which has different costs from a standalone repeat-sampled texture.[^image-source]

For grain, prefer a deliberate tiling implementation with a controlled material/UV contract. Verify that it tiles the intended asset rather than neighboring atlas content. Keep high-frequency grain subtle and test moiré at fractional canvas scales.

## 8. Responsive Canvas and layout ownership

### 8.1 A practical starting configuration

For a desktop-oriented interface, start with `CanvasScaler` set to **Scale With Screen Size**, reference resolution **1920 × 1080**, and Match Width Or Height around **0.5**. These are project defaults to test, not universal settings. Use anchors and responsive composition to express edge attachment, centering, stretch regions, and maximum content widths.[^scaler]

For the Match Width Or Height mode, the scale factor is effectively:

```text
s = (screenWidth / referenceWidth)^(1 − match)
    × (screenHeight / referenceHeight)^match
```

A midpoint match does not guarantee that every wide or narrow screen preserves the same visible composition. Avoid absolute coordinates for all edges. On ultrawide displays, keep dense menu content within a readable maximum width while allowing decorative backgrounds to extend farther.

Prefer changing layout dimensions over applying arbitrary nonuniform Transform scales. A scaled root can also scale border thickness, hit geometry, and text in ways the layout system did not intend.

### 8.2 Safe areas are a separate concern

`Screen.safeArea` describes the usable region in screen pixels; it is not the same as a fixed cinematic margin or the reference resolution.[^safe]

For a full-screen canvas whose root maps to the full screen, normalize the safe-area rectangle by `Screen.width` and `Screen.height` and apply it to a dedicated `SafeAreaRoot`. Keep edge-sensitive controls under it; let nonessential atmosphere bleed outside it. Reapply when resolution, orientation, or the reported safe area changes.

For a camera viewport or a canvas rendered into a sub-rectangle, convert using that viewport's mapping instead of blindly dividing by the full screen size. Test device cutouts and TV-safe layout policies separately.

### 8.3 One writer per axis

Unity's automatic layout system combines minimum, preferred, and flexible sizing, and evaluates width before dependent height. A fitter or group can drive RectTransform properties, overriding manual edits.[^auto][^fit]

Good patterns:

- A parent `HorizontalLayoutGroup` sizes child cells; each cell reports its intended size through `LayoutElement`.
- A vertical list has fixed available width, `VerticalLayoutGroup` on its content object, and `ContentSizeFitter` controlling that content object's vertical size. Children do not also fight the group with fitters on the same driven axes.
- A grid has explicit column constraints or another bounded width policy before deriving vertical content extent.
- Wrapped text receives a stable width before its preferred height is measured.

Bad patterns:

- A child fitter and its parent's layout group both control that child's width/height.
- A panel's width depends on wrapped text height while the text width depends on that panel's height.
- An `AspectRatioFitter`, layout group, and animation all write the same cell rectangle.
- A script calls `SetNativeSize()` after layout and the layout group immediately restores its previous size.

Layout is deferred. When an immediate measurement is genuinely necessary, update or rebuild at a controlled boundary, not continuously from `Update()`. Prefer normal dirty/rebuild behavior; indiscriminate force-rebuild loops create cost and can hide a circular dependency.[^auto]

## 9. Alpha, color, and filtering problems

### 9.1 Diagnose edges in the correct order

Composite the asset over black, white, mid-grey, and a saturated background. Then inspect the source alpha, transparent RGB, import preset, material blending, atlas neighbors, and final scale.

| Symptom | Likely class of problem | Correct direction |
|---|---|---|
| White edge around a dark icon | White matte was baked in, or incompatible alpha treatment | Re-export/unmatte the source; use the expected straight-alpha pipeline |
| Dark outline around a light glow | Dark transparent RGB is leaking through filtering, or alpha was multiplied twice | Check transparent-edge color and shader/blend agreement |
| Colored line appears only in atlas | Sampling bleed from neighboring content or insufficient padding | Fix packing/extrusion/padding and inspect packed output |
| Soft banding in a gradient | Quantization, compression, or repeated low-opacity compositing | Compare uncompressed reference; improve gradient/dither deliberately |
| Entire UI looks unexpectedly washed out | Color-space/material mismatch or overly bright composite | Verify sRGB/data distinction and the rendering path before changing artwork |

A shader may premultiply sampled RGB internally while accepting a straight-alpha PNG. That is valid when its blend operation matches. The mistake is assuming every material expects preprocessed premultiplied files, or preprocessing an asset that the shader multiplies again. Keep the file-format contract and shader contract separate.[^png]

When resizing transparent art offline, use an alpha-aware resampling workflow. A controlled premultiply → resize → unpremultiply conversion can prevent unrelated transparent RGB from contaminating edges, while the final PNG remains straight alpha. Do not let cleanup replace soft falloff with a binary cutout.

### 9.2 Opacity compounds through the hierarchy

Canvas Group alpha multiplies child alpha; Image tint and texture alpha also contribute.[^group]

```text
0.8 texture alpha × 0.75 Image alpha × 0.8 CanvasGroup alpha = 0.48
```

A designer asking for “80% opacity” must specify which layer is being adjusted. Use a background Image to change panel opacity rather than fading an entire group that includes text. Keep the theme color, semantic state, and screen transition contributions explicit.

Overlapping glows also accumulate. Under ordinary alpha-over compositing, two identical layers at alpha 0.3 produce combined opacity `1 − (1 − 0.3)^2 = 0.51`. An apparent brightness bug may be a duplicated state layer rather than an incorrect PNG.

### 9.3 Fractional scales and fine strokes

A 1-source-texel line is not guaranteed to become one rendered pixel. Inspect the final scale and pixel alignment. Slightly thicker source features and optical size variants can be better than forcing Point filtering onto non-pixel-art assets.

Canvas pixel-perfect settings cannot rescue undersized texture detail, a poorly drawn source, or every animated fractional transformation. Test thin frames at the actual resolutions and magnifications you support. If a line disappears at 720p, repair its design or rendering strategy rather than accepting that it looks correct only at the reference resolution.

## 10. Atlases, padding, and compression

Unity's Sprite Atlas has independent packing and texture settings. Canvas UI should have atlas rotation disabled; tight packing, padding, alpha dilation, filters, mips, and platform output settings need deliberate review.[^atlas]

Recommended baseline:

- **Allow Rotation: off** for Canvas UI.
- **Tight Packing: off** for the general frame/glow atlas when predictable rectangular mapping matters.
- **Padding:** begin with the documented default of 4 texels; increase when filtering, mips, or target compression requires it. Eight texels is a reasonable experiment, not a universal fix.
- Use alpha dilation/extrusion where appropriate, and inspect the resulting packed pages.
- Separate assets by rendering requirements and loading lifetime: shared small symbols, screen-specific item art, large atmospheric overlays, and mipmapped world-space assets should not automatically share one giant atlas.

Atlas padding prevents interaction with neighboring packed sprites. It is **not** content padding and is **not** the 96-texel glow margin in the worked example. These are three separate quantities.

Check both source importer and atlas platform overrides when investigating downscaled or blurry UI. Avoid comparing only loose textures in the Editor when builds use atlased output.

An atlas does not guarantee one draw call. Materials, hierarchy order, clipping, canvas boundaries, and other batching constraints still matter. Use the Frame Debugger to identify actual batch breaks instead of reordering layers blindly.

Do not choose Crunch compression to solve GPU texture memory based on the compressed file size alone. Runtime storage format, decompression behavior, resolution, and mip levels determine the relevant GPU allocation.[^atlas]

## 11. Masks, effects, sorting, and rendering paths

### 11.1 Clip content, not the entire decorated shell

A typical panel structure is:

```text
PanelRoot
  ShadowOrGlow                 outside content clipping
  Surface
  ContentViewport              RectMask2D when rectangular clipping is needed
    ScrollContent
  Frame
  FocusAndStatusDecorations
```

`RectMask2D` is designed for rectangular clipping of appropriate coplanar UI without the same stencil workflow as `Mask`. A `Mask` uses stencil-based shape masking. Neither component should be treated as a general-purpose soft per-pixel alpha-compositing system.[^rectmask][^mask]

Putting `Glow` under the masked content viewport will clip its overscan. Moving it outside that clipping subtree is usually the correct fix. Enlarging the glow texture does not help if its parent mask still cuts it off.

### 11.2 Scrollable grids need an explicit effect policy

Choose one behavior:

- Cell focus effects stay inside the viewport and are deliberately clipped at the scroll boundary.
- A shared focus-effect layer follows the focused cell, but applies a deliberate viewport clipping policy.
- A special tooltip/preview escapes the viewport through a separate overlay root.

Do not let a cell's decorative halo obscure unrelated UI merely because it was moved out of the mask. Do not assume that adding a nested Canvas automatically provides the correct clipping, sorting, and interaction semantics.

For an overlay follower, convert through the appropriate screen/local-coordinate APIs and the correct camera for the canvas render mode. Overlay canvases commonly use a null camera for these conversions; camera-space canvases require the relevant camera. Recalculate after layout/scroll changes, not using stale cell positions.

### 11.3 Custom UI materials have responsibilities

A custom UI shader must cooperate with the features actually used by the project: UI vertex color, transparency, stencil properties for masks, rectangle clipping, and relevant alpha clipping behavior. A shader that looks correct on an isolated Image can fail inside a ScrollRect.

Expanding a glow in the shader does not create geometry outside the Image's mesh. Provide adequate geometry/RectTransform extent for the effect, and verify batching and clipping compatibility. A material per cell can destroy an otherwise efficient batching strategy; reuse materials when the rendering approach permits it.

### 11.4 Bloom and background blur are separate from PNG glow

A transparent, blurred PNG produces a glow-like appearance through compositing. It does not automatically become HDR emission or participate in camera bloom. Screen-space Overlay UI is drawn through a different ordering from ordinary camera scene objects; verify where post-processing occurs rather than assuming all UI is affected.[^canvas]

Likewise, a translucent smoky texture does not blur the live scene behind it. True background blur requires an intentional render capture/pass and sampling path with a measured cost. Use opaque/translucent surfaces as a robust fallback.

For HDR output, test UI brightness and paper-white behavior in the project's actual rendering path. Do not assume SDR swatches produce the same perceived brightness on every HDR display.[^hdr]

## 12. Text, localization, and icon alignment

Use `TextMeshProUGUI` for text in the uGUI implementation. Keep labels, counts, descriptions, and prompts editable and localizable. Use licensed font assets and a fallback chain appropriate to supported scripts.[^tmp-font][^tmp-fallback]

### Text rules

- Define minimum readable text roles instead of enabling unconstrained auto-sizing everywhere.
- Use auto-size only within explicit limits. A long translation must not silently shrink a critical instruction until it is unreadable.
- Measure actual visible glyph height on target output. A numeric font-size setting is not equivalent to a guaranteed rendered body height.
- Stress-test long strings, multiline labels, number formatting, and a 30–50% artificial expansion scenario. That percentage is a test input, not a promise about every language.
- Right-to-left support requires correct shaping and layout behavior for the supported language; reversing a string is not localization.
- Preserve room for input prompts to change width when the active device changes.

For text accessibility, use the companion guide's contrast and size targets and verify large-text behavior in the actual layout.[^text-access]

### Typical TMP failures

Missing glyphs require coverage/fallback work, not merely a larger font size. Dynamic font assets need a conscious atlas growth and character-loading policy; static assets need representative coverage. Fallback fonts can also change visual metrics and rendering cost.[^tmp-font][^tmp-fallback]

Clipped outlines and shadows can come from insufficient SDF padding, insufficient graphic geometry padding, or a parent mask/rectangle. These are different failures. Increase the correct budget; extra mesh padding does not recreate missing distance information outside the font atlas's stored gradient.[^tmp-sdf]

Match icon alignment to the optical baseline of adjacent text, not just geometric center. Put icon and label in separate elements with a consistent alignment contract. Do not bake a text label into a symbol to avoid baseline work.

## 13. Interaction and focus ownership

### 13.1 Pointer routing

Use the semantic hit rectangle for ordinary buttons and cells. Make the hit area large enough for the intended device without changing its artwork. Most such controls do not need pixel-perfect alpha testing.

uGUI Image alpha-hit testing above zero requires readable sprite texture data and has atlas-related restrictions; it tests texture alpha rather than the full perceived composited appearance. It is a poor default for glowing UI and can introduce extra memory and asset constraints.[^image-source]

Disable Raycast Target on decorative Images and text. A large transparent decoration must not cover adjacent controls with an unintended hit rectangle.

### 13.2 Hidden is not necessarily non-interactive

When a screen is hidden, explicitly control its interaction state. `CanvasGroup.alpha = 0` is not the same operation as disabling its raycast blocking or making its controls non-interactable.[^group]

A common transition policy is:

```text
opening: establish modal/input ownership → establish focus → fade in
closing: stop accepting commands → fade out → release ownership → restore focus
hidden: alpha 0; interactable false; blocksRaycasts false, or inactive as appropriate
```

The exact order may differ where animation and focus announcements require it, but invisible UI must not accept commands or leave an invisible pointer barrier. An active modal backdrop should deliberately block the covered screen instead.

### 13.3 Keyboard/controller and Input System

Use the project's intended EventSystem configuration and UI input module. Avoid accidentally enabling old and new UI input modules for the same interaction flow. Local multiplayer may legitimately require a different setup; do not delete additional systems without understanding their ownership.

For controller-first menus, review the UI module's background-click deselection behavior. Disabling `Deselect On Background Click` can preserve keyboard/gamepad navigation focus when clicking a non-interactive background.[^input]

Keep **focus**, **selected data**, **equipped state**, and **hover** separate. Preserve a sensible focused control when opening/closing dialogs, changing tabs, removing items, or virtualizing rows.

Gameplay actions and UI actions do not automatically become mutually exclusive. Explicitly route or suspend gameplay commands while the UI owns them. Avoid a submit/cancel event also causing an attack, roll, or immediate reopen.[^input]

Do not rely blindly on `EventSystem.IsPointerOverGameObject()` inside an Input System action callback: UI processing may not yet reflect that input, and the documented behavior can involve the previous UI update. Prefer explicit input ownership and a deliberate processing phase; use raycasts only with understood timing and pointer data.[^input]

## 14. Performance and resource patterns

Optimize measured bottlenecks, not the number of GameObjects in isolation. A few full-screen translucent layers can cost more than many small static UI elements.

### Layout and rendering

Keep frequent state changes from unnecessarily rebuilding a large static screen. Split canvases by meaningful update/lifetime boundaries when profiling supports it, but do not create a Canvas for every cell. Avoid expensive deeply nested layout groups in hot lists, and update text only when its displayed value changes.[^perf]

For large inventories, pool or virtualize rows/cells. Virtualization must preserve the selected item identity and focus behavior; recycled GameObjects are not stable item identities. When reusing pooled UI, follow a lifecycle that avoids needless dirtying and stale callbacks.

Only interactive canvases need a Graphic Raycaster for pointer input. Decorative graphics should not take part in raycast candidate work. Disabling rendering alone does not necessarily stop all scripts, subscriptions, or animations associated with a screen.[^perf]

### Texture and fill-rate budgets

Uncompressed RGBA8 memory, excluding other overhead, is:

```text
width × height × 4 bytes
2048 × 2048 = 16 MiB
4096 × 4096 = 64 MiB
```

A complete mip chain adds approximately one third for a large square texture. A readable CPU copy adds another memory consideration. PNG file size on disk is not a GPU-memory estimate.

The worked glow's rectangle covers `(512 / 320)^2 = 2.56` times the area of its semantic body. Large transparent areas can still incur rendering work, depending on geometry, shader behavior, and rejection. Avoid stacking multiple unnecessary full-screen haze, vignette, and blur layers.

Check the Frame Debugger, CPU/GPU profiler, memory profiling tools, and a target-device build. A lower draw-call count does not by itself prove lower GPU cost, and a visually attractive Editor preview does not establish a mobile or WebGL budget.

## 15. Troubleshooting catalog

This catalog covers common production failure classes; it is not a claim to enumerate every engine, platform, or package defect. Reproduce the issue with the simplest material and uncompressed source, then restore complexity one variable at a time.

### Geometry and layout

| ID | Symptom | Likely cause and repair | Acceptance test |
|---|---|---|---|
| G01 | Child aligns with the glow's outer edge | Parent represents full texture, not semantic body. Use the section 3 shell/overflow hierarchy. | Content inset stays 8 units when glow margin changes. |
| G02 | Visual slot is smaller than its hit area unexpectedly | Full source canvas was fitted into the logical body. Apply semantic-body scaling. | A 320/512 body maps to the requested 96 units. |
| G03 | Icon jumps between default and selected | Different trims, pivots, or semantic registration. Standardize metadata and source canvases. | Repeated state switches do not move the item's optical center. |
| G04 | Borders grow thicker on wide panels | Entire frame is stretched as a Simple Image. Use a valid sliced source and PPU policy. | Wide/tall variants preserve intended edge width above minimum size. |
| G05 | Children ignore the Sprite border | Nine-slice borders affect rendering, not content layout. Add explicit padding. | Long labels never touch the frame. |
| G06 | Image size resets after manual edits | A fitter/group/script drives that axis. Remove competing writers. | Layout remains stable after content and resolution changes. |
| G07 | UI stretches after an icon is assigned | `SetNativeSize()` or aspect fitting changed layout. Keep logical size authoritative. | Sprite swaps preserve anchors and cell dimensions. |
| G08 | Preferred height is wrong until the next frame | Text width/layout was not resolved before measurement. Measure at a controlled layout boundary. | Multiline content opens without one-frame clipping. |
| G09 | Panel oscillates or causes repeated rebuilds | Circular width/height dependencies or continuous forced rebuilding. Break the dependency graph. | Stable dimensions and no recurring unnecessary layout work. |
| G10 | UI drifts on ultrawide or 16:10 | Hard-coded positions or reference-resolution assumptions. Use anchors and a maximum-width policy. | Core content remains readable across supported aspect ratios. |
| G11 | Edge controls are hidden by a cutout | Safe area was ignored or converted using the wrong viewport. Use a mapped SafeAreaRoot. | Controls remain within the usable area on target devices. |

### Export, sampling, and source quality

| ID | Symptom | Likely cause and repair | Acceptance test |
|---|---|---|---|
| A01 | Checkerboard appears in the game | Transparency was rendered into RGB. Re-export a real alpha channel. | Composite over several colors without a checker pattern. |
| A02 | White/black fringe appears on soft edges | Matte contamination or alpha/blend mismatch. Inspect source and material contracts. | No contrasting halo on black, white, and saturated tests. |
| A03 | Glow ends in a visible square | Falloff reaches the texture boundary or a mask clips it. Add effect overscan or fix clipping. | Outer edge reaches negligible alpha in the intended composite. |
| A04 | Asset is sharp in source, blurry in Unity | Import/atlas Max Size, compression, or insufficient effective body resolution. Inspect imported dimensions. | Required detail remains legible at final output size. |
| A05 | Tiny engraved icon becomes noise | Detail was designed for the master, not 32–48 px. Simplify silhouette/strokes or make an optical-size variant. | The symbol is distinguishable without zooming. |
| A06 | Thin frame flickers while moving | Subpixel sampling, undersized stroke, or unsuitable minification. Adjust geometry/design and test filtering. | No distracting shimmer during normal movement. |
| A07 | Gradient has blocks/bands | Compression or insufficient smooth-gradient precision. Compare reference-quality import; adjust gradient strategy. | No objectionable banding on target output. |
| A08 | Colors differ sharply from the design preview | Color-space, material, alpha, or preview-background mismatch. Compare controlled composites. | Theme swatches are approved through the actual UI render path. |
| A09 | Grain stretches or forms moiré | A unique texture was stretched, or tile frequency conflicts with output sampling. Separate and retune tiling. | Grain stays subdued at multiple scales. |

### Atlas and rendering

| ID | Symptom | Likely cause and repair | Acceptance test |
|---|---|---|---|
| R01 | Neighboring sprite colors leak at edges | Insufficient atlas separation/extrusion or incompatible mip/compression settings. Inspect packed output. | No bleed at smallest supported size in a build. |
| R02 | Packed UI changes orientation or shape | Rotation/tight packing is incompatible with the UI path. Disable rotation and use the appropriate rectangular baseline. | Packed and loose reference appearances match. |
| R03 | A repeated pattern shows unrelated symbols | Shader repeats an atlas domain, not one sprite region. Use a correct tiled geometry/UV path. | Only the intended tile repeats. |
| R04 | Glow disappears at viewport edges | The effect is inside a clipping subtree. Apply an explicit content/effect hierarchy. | Clipping matches the selected viewport policy. |
| R05 | Custom material ignores ScrollRect clipping | Required UI clipping/stencil support is missing. Use a compatible shader implementation. | Material behaves correctly under every used mask type. |
| R06 | Tooltip renders behind another panel | Wrong hierarchy/canvas/sorting or portal placement. Use an established overlay root. | Tooltip is visible without breaking modal ordering. |
| R07 | Shader glow cannot extend outward | Mesh/RectTransform does not cover the effect area. Expand decorative geometry, not logical layout. | Complete falloff renders while hit bounds stay unchanged. |
| R08 | Camera bloom does not affect menu glow | UI is outside that post-processing path. Choose a composited glow or deliberately integrate camera-space effects. | Visual result is stable in the chosen render mode. |
| R09 | “Blur background” only adds a fog texture | No live background sampling exists. Implement and budget real blur, or specify an honest translucent fallback. | Background detail is genuinely blurred when blur is enabled. |
| R10 | Panel is far more transparent than intended | Texture, Image, parent alpha, and duplicate layers compound. Audit the full chain. | Measured composite matches the opacity target. |

### Text, states, and input

| ID | Symptom | Likely cause and repair | Acceptance test |
|---|---|---|---|
| T01 | Missing boxes replace translated characters | Font coverage/fallback is incomplete. Populate the correct glyphs and fallback assets. | Supported-language samples render without missing glyphs. |
| T02 | Text outline or shadow is clipped | Insufficient SDF/mesh padding or parent clipping. Identify which boundary cuts it off. | Largest allowed outline remains intact. |
| T03 | Long text becomes unreadably small | Unbounded auto-size hides layout defects. Set a minimum and wrap/reflow/scroll appropriately. | Long translations remain above the approved text floor. |
| T04 | Numbers jump or columns wobble | Unstable alignment or proportional numeric metrics. Use appropriate alignment and numeral treatment. | Rapid value changes preserve column readability. |
| T05 | Icon looks vertically wrong beside text | Geometric center was used instead of optical baseline alignment. Apply a consistent icon/text contract. | Representative labels align across font sizes. |
| I01 | Adjacent buttons cannot be clicked | Decorative transparent Images intercept raycasts. Disable their Raycast Target. | Pointer access matches semantic hit rectangles. |
| I02 | Invisible screen blocks or accepts input | Only alpha/rendering was disabled. Update interaction, raycast, and action ownership. | Hidden screen receives no commands and blocks nothing unintended. |
| I03 | Navigation stops after background click | Focus was cleared by the UI module. Review deselection policy and restore a valid target. | Gamepad navigation remains usable after pointer input. |
| I04 | One submit also performs gameplay action | UI/gameplay action ownership overlaps. Gate or route commands explicitly. | Each input produces only its intended action. |
| I05 | Click-through checks are one frame late | UI hit state was queried in an unsuitable Input System callback phase. Fix routing/timing. | No gameplay click leaks through an interactive menu. |
| I06 | Equipped item appears unfocused when browsing | Focus and equipped state share one flag/graphic. Separate state dimensions. | Focus can move while equipment markers remain stable. |
| I07 | Focus points to a recycled or deleted cell | Selection is stored only as a pooled GameObject reference. Preserve item identity and restore a valid target. | Filtering/removal/scrolling never strands focus. |

### Resources and lifecycle

| ID | Symptom | Likely cause and repair | Acceptance test |
|---|---|---|---|
| P01 | Inventory opening causes a CPU spike | Mass instantiation, layout rebuilds, or synchronous binding. Pool/virtualize and batch data presentation. | Open/scroll cost meets the project's measured budget. |
| P02 | Static screen rebuilds continuously | Repeated assignments, nested layout dirtiness, or broad canvas invalidation. Update on change and isolate hot regions. | Idle UI has no avoidable repeated rebuild pattern. |
| P03 | GPU cost is high despite few draw calls | Large translucent layers, blur passes, or excessive overdraw. Reduce covered area/layers and profile. | Target-device GPU time meets budget in the worst composite. |
| P04 | Texture memory is much larger than PNG size | Runtime dimensions/formats, atlases, mips, and readable copies were ignored. Audit actual allocations. | Memory report matches the documented asset budget. |
| P05 | Hidden or pooled UI still runs logic | Subscriptions, tweens, coroutines, or callbacks outlive the intended screen lifecycle. Bind/unbind and cancel deliberately. | Closed UI performs no unintended updates or stale actions. |

## 16. Reusable implementation recipes

### A. Inventory cell

Use the logical-root hierarchy in section 3. Store item identity in the existing presentation model, not in the glow or pooled object identity. One binding operation updates icon, amount, availability, equipment marker, and accessible label. State changes affect visual layers without relayout. Focus and equipped markers can coexist.

### B. Resizable panel

Use `PanelRoot` → Surface, Frame, Effect, ContentRoot. Frame uses a tested nine-slice; content padding is explicit. Surface opacity does not change text opacity. Keep minimum dimensions large enough for borders plus content. Unique corner decorations are separately anchored.

### C. Scrollable inventory with focus effect

Use a fixed viewport, a bounded grid/list layout, and pooled content if needed. Choose whether focus glow is clipped inside the viewport or followed by an overlay. Restore navigation by item identity after sorting or filtering. Never let a recycled view retain the previous item's equipped or selected visual state.

### D. Tooltip or item comparison

Create under the established overlay root, not as an unconstrained child inside a masked cell. Position from a screen-space anchor and clamp to the safe readable area. Resolve content width before preferred height. Show through focus as well as hover. Separate original value, comparison value, and change indicator; do not convey improvement solely through color.

### E. Modal confirmation

Use a dedicated modal layer with an intentional pointer-blocking backdrop. Route UI actions to the modal, preserve the previous focus target, and default to the safe option for destructive actions. Closing must release ownership and restore a valid target, including when the original item no longer exists. Do not make fading alpha the only lifecycle control.

### F. Atmospheric background

Keep dim surface, vignette, and grain conceptually separate; merge only when profiling and art requirements justify it. Use no input interception. Preserve legibility under bright gameplay. Treat live blur as an optional explicit rendering feature with a fallback, not as a property of a transparent PNG.

### G. Character or item RenderTexture preview

Use a dedicated preview setup and a UI presentation element suited to the texture, commonly `RawImage`. Define RenderTexture resolution, graphics format/alpha support, camera clear behavior, framing, and lifetime. Keep its display rectangle separate from character silhouette bounds. Test transparency through the full render pipeline; a clear alpha of zero alone does not guarantee every rendering stage preserves alpha. Do not silently allocate a new RenderTexture every frame or for every inventory cell.

## 17. UI Toolkit boundary

Do not apply uGUI component instructions literally to UI Toolkit. The conceptual pattern still holds: a logical layout element, explicit content padding, non-picking decorative visuals, and separate overflow/clipping rules. However, `VisualElement`, USS sizing, picking, background scaling/slicing, and clipping use different APIs and behavior.

If a screen uses UI Toolkit, write a separate implementation adapter and verify the installed version's documentation. Do not attach a `ContentSizeFitter`, `CanvasGroup`, or uGUI `Image` assumption to a `VisualElement`. Do not mix two UI frameworks merely to avoid understanding the current screen's layout system.

## 18. Agent workflow and acceptance gates

### 18.1 Implementation sequence

1. Inspect the installed Unity/packages, current UI root, Canvas settings, input module, asset-loading conventions, and relevant prefab variants. Do not change global settings during diagnosis unless evidence requires it.
2. Classify each proposed asset: illustration, symbolic icon, nine-slice frame, tiled material, fixed-aspect effect, data mask, or full-screen surface.
3. Write semantic bounds, effect overflow, content inset, optical center, and expected physical output size before generating/importing the asset.
4. Create one representative component in isolation using the simplest compatible material. Validate source alpha and imported dimensions.
5. Validate layout, hit targets, clipping, focus states, and localization before applying decorative polish.
6. Establish reference-quality import output, then test atlases and platform compression against it.
7. Integrate through the project's existing presenter/factory/lifetime model. Preserve `.meta` GUIDs and prefab relationships; do not replace assets or reset importers indiscriminately.
8. Test the actual build target, not only Game View. Record exceptions, material dependencies, and import overrides with the asset or component.

### 18.2 Required visual matrix

For supported platforms, test at least 1280 × 720, 1920 × 1080, 2560 × 1440, and 3840 × 2160, plus representative 16:10 and ultrawide ratios. These are a starting desktop matrix, not a requirement to support every display shape. Add actual handheld/mobile layouts where applicable.

Test standard and large UI/text settings, bright and dark moving backgrounds, minimum/maximum panel sizes, and short/long localized content. Include pointer, keyboard, and controller flows where supported.

### 18.3 Geometry and interaction checks

- [ ] Logical bounds are unchanged when replacing a no-glow visual with a glow visual.
- [ ] In the worked example, the body is 96 units, the effect is 153.6, and content is 80.
- [ ] Asymmetric source margins map correctly without moving the semantic body.
- [ ] Body aspect ratio stays correct for proportional-effect assets.
- [ ] States do not move the optical center, change preferred size, or alter the hit area.
- [ ] Frame borders and content padding remain separate at every supported size.
- [ ] Decorations do not block input, and hidden screens do not receive commands.
- [ ] Focus survives modal close, background clicks, list recycling, filtering, and item removal.
- [ ] Tooltips and glows follow their declared clipping/sorting policies.

### 18.4 Asset and rendering checks

- [ ] Source dimensions, alpha mode, and semantic metadata were inspected rather than assumed from a prompt.
- [ ] Black/white/saturated-background tests reveal no matte fringe or unwanted opaque background.
- [ ] Atlas output has no rotation, bleed, or unexpected scaling in the actual UI path.
- [ ] Smallest glyphs and symbols are readable at final physical output size.
- [ ] Font coverage, large text, and translated labels pass without uncontrolled shrinking.
- [ ] Custom materials cooperate with every mask and canvas mode actually used.
- [ ] Texture memory, overdraw, CPU rebuild cost, and pooled-object lifecycle were measured on target.

### 18.5 Test automation opportunities

Add Edit Mode checks for metadata validity, Sprite import conventions, effect geometry calculations, state-variant registration, and forbidden raycast targets on declared decoration. Do not make alpha bounding boxes the sole expected geometry assertion.

Add Play Mode checks for navigation restoration, hidden-screen input blocking, pooling reset, modal ownership, and fixed logical bounds across state changes. Capture representative screenshots for visual regression, but account for antialiasing, font rendering, platform differences, and animation timing before using strict pixel equality.

A final handoff should include source assets, import settings, semantic metadata, prefab usage, state behavior, tested resolutions, measured budgets, and unresolved platform-specific limitations. Do not describe a generated asset as integrated or tested until the relevant engine checks have actually run.

## 19. Sources and evidence scope

Research checked on 2026-09-12. Engine behavior is supported by the official documentation and public implementation below. Hierarchies, size presets, calculations, workflows, and troubleshooting remedies are engineering recommendations derived from those behaviors. Package documentation should be checked against the project's installed versions.

[^rect]: Unity, [Rect Transform](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/class-RectTransform.html).
[^layout]: Unity, [Basic Layout](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/UIBasicLayout.html).
[^auto]: Unity, [Auto Layout](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/UIAutoLayout.html).
[^fit]: Unity, [Making UI elements fit the size of their content](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/HOWTO-UIFitContentSize.html).
[^scaler]: Unity, [Canvas Scaler](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-CanvasScaler.html).
[^image]: Unity, [Image component](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-Image.html).
[^image-source]: Unity Technologies, [uGUI Image implementation](https://raw.githubusercontent.com/Unity-Technologies/uGUI/main/com.unity.ugui/Runtime/UGUI/UI/Core/Image.cs). The main branch can change; verify the installed package when exact implementation matters.
[^texture]: Unity, [Sprite texture import settings](https://docs.unity3d.com/6000.3/Documentation/Manual/texture-type-sprite.html).
[^atlas]: Unity, [Sprite Atlas reference](https://docs.unity3d.com/6000.3/Documentation/Manual/sprite/atlas/sprite-atlas-reference.html).
[^group]: Unity, [Canvas Group](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/class-CanvasGroup.html).
[^rectmask]: Unity, [Rect Mask 2D](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-RectMask2D.html).
[^mask]: Unity, [Mask](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-Mask.html).
[^canvas]: Unity, [Canvas](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/UICanvas.html).
[^safe]: Unity, [Screen.safeArea](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Screen-safeArea.html).
[^input]: Unity, [Input System: UI support](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/UISupport.html).
[^perf]: Unity, [Unity UI optimization tips](https://unity.com/how-to/unity-ui-optimization-tips).
[^tmp-font]: Unity, [TextMeshPro font assets](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/FontAssets.html).
[^tmp-fallback]: Unity, [TextMeshPro font asset fallbacks](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/FontAssetsFallback.html).
[^tmp-sdf]: Unity, [TextMeshPro distance field shaders](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.2/manual/ShadersDistanceField.html).
[^hdr]: Unity, [HDR output in URP](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/post-processing/hdr-output.html).
[^png]: W3C, [Portable Network Graphics Specification, Third Edition](https://www.w3.org/TR/png-3/).
[^text-access]: Microsoft, [Xbox Accessibility Guideline 101: Text display](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/101).
