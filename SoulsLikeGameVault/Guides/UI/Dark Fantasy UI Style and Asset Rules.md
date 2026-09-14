---
title: Dark Fantasy UI Style and Asset Rules
type: guide
domains:
  - ui
  - ui-art
  - penpot
status: current
authority: required
verified: 2026-09-12
updated: 2026-09-12
context_keys:
  - ui-style
aliases:
  - Restrained Dark-Fantasy UI UX
  - DARK_FANTASY_UI_UX_STYLE_AND_ASSET_RULES
source_urls:
  - https://drive.google.com/file/d/1ig1Wdt2SmeoNUpgLukj3AjdhkQuNPfuQ/view
companion: "[[Unity UI Asset Layout and Troubleshooting Rules]]"
tags:
  - agent/context
---

# Dark Fantasy UI Style and Asset Rules

## Project Application Rules

- For every UI, Penpot, mockup, UI-art, or generated-asset task, resolve `ui-style`, read this heading and [[#Task-Conditional Reading Map]], then read only the mapped detailed headings needed for the task.
- Source statements marked **MUST** are required for relevant work unless higher-priority project guidance or live implementation evidence conflicts. Report such a conflict; do not silently introduce an exception.
- Palette values, opacity ranges, reference dimensions, font sizes, timing values, and other values described as proposed, initial, working, or starting defaults are guidance for a reviewed composite. They do not authorize global project-setting changes or override measured readability, runtime performance, installed-package behavior, or product requirements.
- Preserve readable text, semantic bounds, input accessibility, and distinct focus/selection/equipped states before applying atmosphere. Decorative glow, haze, frames, and grain must remain independent layers and must not change layout or hit bounds.
- Existing [[Agent Guide/Vault Guide|vault workflow]], `AGENTS.md`, the official Unity CLI workflow, Unity clean-scene safeguards, and the project's no-normal-Play-Mode-validation rule outrank examples in the source specification.

## Task-Conditional Reading Map

| Task signals | Read these detailed headings after the mandatory rules |
|---|---|
| Penpot screen, mockup, composition, visual hierarchy | `2. Interaction foundations`, `3. Visual grammar`, `4. Measurement language`, `5. Palette tokens`, `6. Opacity, compositing, haze, and glow`, `9. Component recipes`, `10. State specification` |
| UI art, generated icons, frames, glows, haze, or texture briefs | `3. Visual grammar`, `4. Measurement language`, `6. Opacity, compositing, haze, and glow`, `8. Icon and illustration families`, `12. Universal asset brief`, `13. Reusable generation prompts`, `14. Production workflow`, `15. Acceptance checklist` |
| Controls, navigation, selection, comparison, dialog, accessibility, or text | `2. Interaction foundations`, `7. Readability and accessibility gates`, `9. Component recipes`, `10. State specification`, `11. Motion and feedback`, `15. Acceptance checklist` |
| UI validation or handoff | `7. Readability and accessibility gates`, `14. Production workflow`, `15. Acceptance checklist`, `16. Sources and evidence scope` |

## Source Specification

# Restrained Dark-Fantasy UI/UX
## Style system, interaction rules, and asset-generation instructions

## 1. Purpose and authority

Create a solemn, weathered, understated fantasy interface: charcoal and olive-grey surfaces, restrained antique-gold emphasis, warm ivory text, fine engraved symbols, and carefully illustrated inventory objects. The interface should feel like a practical archive of equipment and knowledge, not a collection of ornate golden picture frames.

This is an original, reusable production specification. The palette, opacity ranges, measurements, and animation timings below are **proposed working defaults**, not recovered settings from another product. A composited screenshot does not uniquely reveal its source textures, alpha values, blend mode, or font settings.

**MUST** means a release requirement. **SHOULD** means the default unless an explicit, documented exception improves the result. **MAY** means optional. Apply this priority order:

1. Readability, understandable interactions, and input accessibility.
2. Correct semantic bounds, asset contracts, and responsive layout.
3. Consistent component states and visual hierarchy.
4. Decorative atmosphere and surface detail.

An agent must not sacrifice a higher priority to improve a lower one. Do not make the screen darker, smaller, or more ornate merely to make it appear more atmospheric.

Use this document to design assets and interactions. Use the companion Unity document for importing, sizing, layout ownership, materials, rendering, and troubleshooting.

## 2. Interaction foundations

The following are production applications of established usability principles, rather than a requirement to copy any existing menu. The underlying research emphasizes feedback, consistency, discoverability, and recovery.[^ux]

| Rule | Required behavior |
|---|---|
| Make consequences visible | Show which object is focused, what is equipped, what changed, and whether an operation succeeded. |
| Use familiar meanings | Prefer recognizable equipment silhouettes, plain labels, and platform-consistent inputs. |
| Provide a reliable exit | Back closes the current level first; closing a submenu restores its previous context. |
| Reuse the same vocabulary | A color, icon, label, or button must not silently acquire a different meaning elsewhere. |
| Prevent costly mistakes | Separate destructive actions from ordinary confirmation; explain the affected object. |
| Keep choices discoverable | Expose relevant actions and help instead of requiring memorized gestures. |
| Support repeated tasks | Preserve filters, scroll position, and selection when sensible; avoid repeated confirmation for harmless actions. |
| Reduce irrelevant detail | Show the information needed for the current decision; reveal secondary detail on request. |
| Explain failures | Describe what happened, why an action is unavailable, and the next useful action. |
| Make help contextual | Place explanations near the selected attribute or control. |

### 2.1 Focus, selection, and equipment are different states

**Focus** identifies the control that will receive navigation/confirmation. **Selection** identifies the chosen entry in a data view. **Equipped/active** identifies persistent game state. A hovered object is not necessarily selected, and a selected object is not necessarily equipped.

For a grid, render focus as a clearly bounded highlight. Render equipped state with a small, persistent badge or marker. Permit the two to coexist. Opening an item comparison must not silently equip the previewed item.

Keyboard and controller users must be able to find the current focus, enter and leave dialogs, and recover their previous position after a dialog closes. Use an explicit initial focus and a safe fallback when the previous object disappears.[^nav][^focus]

### 2.2 Required screen states

Design populated, empty, loading, failed, unavailable, and fully completed states before producing decorative assets. Include a visible response when filtering returns no results. A loading operation must not resemble an empty inventory. An unavailable item must remain understandable: use a reason or requirement message instead of simply making it nearly invisible.

Provide useful comparison changes with signs, arrows, labels, or icons in addition to color. Do not encode improvements and penalties only as green versus red.

### 2.3 Destructive actions

Identify the object and consequence in the confirmation. Initially focus the safe action. Keep Back available. Prefer undo for actions that support it; require stronger confirmation only for meaningfully costly or irreversible operations. Do not style a destructive action as the most attractive ordinary gold action by default.[^destructive]

## 3. Visual grammar

### 3.1 Defining characteristics

Use rectilinear, practical organization. Panels should be flat or very shallow, with restrained edge wear. Leave broad quiet areas around important content. Gold is a selective signal, not the default surface material. Texture should suggest age without making every surface look dirty.

The overall visual hierarchy is: **selected content → readable information → controls and grouping → atmosphere**. During gameplay, vital status and interaction prompts may take precedence over selected content.

| Property | Direction | Avoid |
|---|---|---|
| Mood | Solemn, calm, archival, slightly austere | Festive, glossy, playful, theatrical everywhere |
| Geometry | Rectangles, squares, fine horizontal rules, small functional circles | Pill buttons, oversized rounded cards, hexagonal science-fiction grids |
| Material | Worn ink, dark cloth-like haze, oxidized metal hints | Polished chrome, plastic, thick extruded gold |
| Color | Low-chroma olive, charcoal, warm grey, antique gold | Neon yellow, saturated purple framing, rainbow rarity borders |
| Depth | Mostly flat; shallow engraving and soft local shading | Floating 3D buttons, heavy bevels, large drop shadows |
| Decoration | Sparse irregular grain and peripheral wear | Symmetric baroque borders around every field |
| Focus | Quiet gold fill plus a precise readable boundary | A large luminous cloud as the only state indicator |
| Typography | Readable serif character with a clear alternate | Blackletter paragraphs, excessively thin strokes, generated lettering |

### 3.2 Do not confuse asset families

**Category symbols** are simplified, mostly monochrome engraved signs. **Inventory objects** are more detailed material illustrations, allowed to retain restrained object-specific colors. **Functional glyphs** are even simpler and prioritize recognition over engraving. **Decoration** is not allowed to masquerade as an actionable symbol.

Do not apply the same metallic rendering to every sword, herb, flask, arrow, status symbol, and menu tab. Consistency comes from shared lighting, framing, scale, palette restraint, and state treatment—not identical materials.

## 4. Measurement language

Use **1920 × 1080 logical reference units** for initial desktop/television composition. This is a design coordinate system, not a promise that every device has that resolution.

Always distinguish:

- **Source pixels:** dimensions inside the exported image.
- **Reference units:** the intended layout size in the reference UI.
- **Rendered pixels:** the final screen footprint after UI scaling.
- **Visible/semantic bounds:** the meaningful object or panel body, excluding decorative overflow.

A 512 × 512 icon does not automatically need a 512 × 512 control. A 96-unit control does not automatically need a 96-pixel source texture. A 48-unit icon frame may contain a visible symbol narrower than 48 units.

### 4.1 Spacing scale

Use the initial spacing tokens `4, 8, 12, 16, 24, 32, 48, 64` reference units. Prefer a consistent rhythm over forcing every irregular illustration onto identical optical geometry.

| Use | Initial setting |
|---|---:|
| Small icon-to-label gap | 8–12 units |
| Related attribute row gap | 8–12 units |
| Grid cell gap | 8–12 units |
| Standard panel content inset | 24–32 units |
| Compact tooltip inset | 16–24 units |
| Major column gutter | 32–48 units |
| Section separation | 24–32 units |
| Outer desktop composition inset | 48–64 units, then safe-area validation |

Do not use decorative glow radius as a spacing token. Content spacing is measured from the panel body, not from its outer haze.

### 4.2 Shape and edge tokens

| Element | Initial geometry |
|---|---|
| Standard panel | Rectangular; corner radius 0–2 reference units |
| Item cell | Square; radius 0–2; the underlying semantic rectangle remains exact |
| Fine separator | Approximately 1 rendered pixel at the baseline; strengthen where scaling makes it disappear |
| Important focus boundary | Approximately 2 rendered pixels at the baseline; contrast-checked |
| Worn edge band | 1–3 reference units; intermittent, not uniformly chipped |
| Shallow bevel suggestion | At most 1–2 reference units; no large raised frame |
| Checkbox | Square; unmistakable checked mark independent of color |
| Slider | Thin horizontal track, visible handle, numeric value where useful |

For one-pixel geometry and exact repeating shapes, use deterministic drawing or vector/procedural construction. Image generation may supply texture, but it is not the authority for exact slice lines, distances, or symmetry.

## 5. Palette tokens

These are sRGB design colors. The final composited result—not the swatch by itself—must pass readability checks.

| Token | Hex | Role |
|---|---|---|
| `ink.deep` | `#151713` | Deep scrims and the darkest backing |
| `surface.base` | `#252820` | Main readable panel body |
| `surface.raised` | `#34392D` | Secondary surface and selected-row support |
| `edge.quiet` | `#6E6D54` | Decorative lines; not automatically accessible as a functional boundary |
| `metal.antique` | `#A89970` | Default engraved symbol and minor accent |
| `metal.focus` | `#C7B78A` | Focus boundary and selected title emphasis |
| `text.primary` | `#E1DDCC` | Primary readable information |
| `text.secondary` | `#B8B39F` | Secondary readable information |
| `text.muted` | `#8B8879` | Low-priority, nonessential text only unless contrast passes |
| `text.disabled` | `#736F61` | Inactive decoration; not the only explanation of unavailability |
| `semantic.warning` | `#C6A566` | Caution, accompanied by a symbol or label |
| `semantic.negative` | `#C78779` | Penalty or failure, accompanied by sign/text |
| `semantic.positive` | `#9BAE83` | Improvement, accompanied by sign/text |
| `semantic.info` | `#93ADB8` | Informational or magical accent |

Use at most one main accent family per ordinary menu. Item illustrations may use muted material colors without making all interface chrome colorful. Do not introduce universal bright rarity colors unless the product actually has a rarity system and that system needs them.

## 6. Opacity, compositing, haze, and glow

### 6.1 Define which alpha is being specified

Every brief must identify whether a value means **texture alpha**, **component alpha**, or **final local contribution**. Unless a recipe explicitly says otherwise, the table below targets the layer's local alpha with parent opacity at 1.

Author geometry masks at full interior alpha when runtime control is intended. Author the spatial falloff of glows into the texture; apply any further runtime multiplier once. Do not bake `0.35` into a texture and then set its component to `0.35` while expecting a 35% result.

For an ordinary alpha-controlled layer:

```text
local effective alpha = texture alpha × component alpha × parent opacity multipliers

Example: 0.80 × 0.75 × 0.80 = 0.48

Two overlapping identical alpha layers, A over A:
combined coverage = 1 − (1 − A)²
Example: two 0.30 layers produce 0.51 coverage, not 0.30.
```

The RGB blend must be evaluated in the renderer's working color space. A photograph-like preview in an editor is not a guarantee of the same output under different rendering or display settings.

### 6.2 Initial alpha ranges

| Layer | Local alpha starting range | Default | Constraint |
|---|---:|---:|---|
| Fullscreen world dimmer | 0.25–0.50 | 0.35 | Separate from the content panel; adjust to scene brightness |
| Primary readable panel body | 0.82–0.94 | 0.88 | Increase toward opaque if text contrast fails |
| Secondary noncritical panel | 0.65–0.85 | 0.76 | Not sufficient for arbitrary busy backgrounds |
| Ordinary empty-cell fill | 0.25–0.45 | 0.34 | Empty state remains distinguishable from a disabled item |
| Quiet frame decoration | 0.35–0.65 | 0.50 | Must not be the sole required boundary |
| Important focus boundary | 0.85–1.00 | 0.95 | Must meet functional contrast after compositing |
| Primary text | 1.00 | 1.00 | Use color hierarchy rather than fading all labels |
| Secondary text | 1.00 | 1.00 | Use `text.secondary`; verify contrast |
| Engraving texture variation | 0.08–0.20 | 0.12 | Preserve the silhouette |
| Selected-row interior tint | 0.10–0.20 | 0.14 | Keep text readable |
| Selected-cell glow peak | 0.22–0.38 | 0.32 | Broad, soft, subdued; not a substitute for focus geometry |
| Selected-cell glow center | 0.00–0.04 | 0.02 | Preserve item detail beneath the center |
| Central atmosphere haze | 0.02–0.08 | 0.04 | No recognizable objects or symbols |
| Peripheral vignette | 0.12–0.28 | 0.20 | Keep the central information field quiet |
| Fine static grain | 0.02–0.05 | 0.03 | Must remain subordinate at final size |

These ranges are not additive targets. A panel with a dimmer beneath it, haze above it, and a tinted parent needs to be checked as a whole. The stronger accessibility variant takes precedence over preserving translucency.

### 6.3 Glow construction

A selected-cell glow should follow a square or near-square semantic outline. Use soft antique-gold diffusion around its perimeter, not a circular lens flare. Keep the center clear. Avoid saturated yellow, a white-hot rim, sparks, rays, or starbursts.

Use three independent ingredients when needed: a low-alpha interior tint, a crisp focus boundary, and a soft outer glow. Independent layers allow focus clarity without excessive brightness.

Reserve texture space for the complete falloff. For a Gaussian-style blur, approximately four blur standard deviations is a useful initial overscan estimate; the actual acceptance rule is that the exported outer edge has negligible alpha, not that a particular blur parameter was used. Do not hard-crop a bright glow at the file edge.

Glow dimensions MUST NOT define content anchors, grid spacing, or click areas. The companion document supplies the exact geometry pattern.

### 6.4 Haze is not background blur

A transparent painted haze overlays color. It does not sample and blur the scene behind the UI. Request a runtime blur effect separately when actual background blur is needed. Prefer one restrained atmosphere layer over several full-screen fog textures.

## 7. Readability and accessibility gates

Use the following as explicit product acceptance targets informed by accessibility guidance. WCAG is a web standard; adopting selected thresholds does not establish full game accessibility or certification.[^contrast][^nontext]

**Text:** target at least 4.5:1 for ordinary readable text. A deliberately high-contrast mode should aim higher, for example 7:1. Large text has a 3:1 reference threshold in WCAG, but do not classify a label as large using its texture dimensions or a nominal Unity font value alone.

**Meaningful non-text cues:** target 3:1 against adjacent colors for the parts needed to identify controls and focus. Decorative scratches are exempt from the product's functional-boundary requirement; an essential focus ring is not.

Test final composited states over bright daylight, dark interiors, mixed foliage, fire, and moving high-detail backgrounds. Increase local backing opacity, reduce interference, or strengthen the text before adding heavier decorative outlines.

### 7.1 Typography

Use a readable, moderate-contrast serif for identity-bearing titles and ordinary menu labels. Avoid extremely fine hairlines, blackletter body text, excessive flourishes, or distress applied directly to small text. Offer a clear sans-serif alternate.

Microsoft's game text guidance measures visible body height, not the font-size field: its baseline examples are at least 18 rendered pixels for PC and 26 for console at 1080p. It also recommends text scaling up to 200% without losing meaning or function.[^text]

Project starting values for actual font settings are only provisional:

| Role | Initial reference font setting | Treatment |
|---|---:|---|
| Screen title | 36–44 | Restrained serif; no baked lettering |
| Section heading | 28–32 | Same family, modest hierarchy change |
| Primary body / item name | 26–30 | Readable weight; real text |
| Dense attribute table | 24–28 | Stable columns and aligned values |
| Secondary help / control prompt | 24–26 | Never shrink below the validated visible-body-height floor |
| Major discovery message | 40–56 | More spacing permitted; brief content only |

Increase these settings when the chosen font's actual glyphs are too small. A TMP value of 26 is not proof of a 26-pixel visible body height. Use reflow or a more compact information view when text scaling makes the original column arrangement impossible.

Use sentence case for prose, short labels for actions, and consistent attribute names. Right-align numeric columns. Use aligned numerals where supported. Keep paragraphs left-aligned for left-to-right languages; choose appropriate direction and shaping for other scripts. Do not align columns with manually inserted spaces.

### 7.2 Target size and alternate input

Treat a roughly 48 × 48 reference-unit interaction box as an initial design target for ordinary pointer controls, with larger rows where practical. It is not a universal physical-size guarantee. Touch and distant-viewing layouts require separate testing.

WCAG 2.2's minimum pointer-target criterion uses 24 × 24 **CSS pixels** and contains exceptions. Those units are not Unity units and should not be mechanically transferred to a game canvas.[^target]

A symbol may be 32 pixels while its click target is larger. Do not require clicking only the opaque pixels of a narrow sword or ring. All important tooltips/help must also be available through focus or a help action, not hover alone.

### 7.3 Accessibility variants

Provide a high-contrast/backing variant, scalable text and prompts, a clear font alternate, reduced-motion mode, and a color-independent state language. Keep disabled-action explanations readable. These are system states, not separate improvised art packs.

## 8. Icon and illustration families

### 8.1 Family A — Category and attribute symbols

Use one dominant silhouette, a restrained front or consistent shallow view, a muted ivory/antique-metal value range, and shallow engraved internal detail. At small sizes, silhouette outranks realism.

Initial master: 512 × 512 RGBA PNG. Keep the major silhouette within approximately 72–82% of the canvas's longest dimension; record its actual semantic bounds. Match apparent visual weight across the set, not only bounding-box size. Use a documented optical-center adjustment when a bow, spear, or key appears off-center.

Review the full icon canvas at 32, 48, and 64 rendered pixels. Remove details that collapse into noise. For a 512-pixel master reduced directly to 32 pixels, a 16-pixel source stroke becomes only one screen pixel; decisive narrow features often need around 24–32 source pixels to survive that reduction. This is scale arithmetic, not a demand that every internal line use the same thickness.

| Category | Suggested generic silhouette | Small-size priority |
|---|---|---|
| Armor | Cuirass or simple breastplate | Shoulder and torso shape |
| Talisman | Hanging medallion or amulet | Outer contour, central opening, attachment |
| Consumables | Stoppered flask | Neck/body separation |
| Key items | Simple key | Distinct bow and teeth |
| Ranged weapons | Bow with one arrow | Bow curve versus straight arrow |
| Melee weapons | Single blade or restrained crossed blades | Blade/hilt distinction |
| Shields | Simple shield face | Clear outer shape |

Do not add a background frame, caption, platform button, or selection glow to the symbol file. Those belong to other layers.

### 8.2 Family B — Inventory object illustrations

Use hand-painted or carefully rendered material appearance: weathered steel, dark leather, faded cloth, cloudy glass, or botanical matter. Retain muted object-specific color. Use one shared soft upper-left key-light convention and restrained fill; avoid a strong cast shadow onto an imaginary floor.

Show one complete object, with no environment. Use one agreed category-specific orientation. Long weapons may use a consistent diagonal; armor may use a front or mild three-quarter view. Avoid changing camera perspective randomly between neighboring assets.

Keep object silhouettes separate from UI borders and quantities. A 512-pixel source is a suitable initial master for ordinary objects; larger inspection artwork is a separate deliverable when the UI actually needs it. Do not assume upscaling a tiny icon produces good detail art.

### 8.3 Family C — Functional glyphs

Chevrons, sort markers, plus/minus signs, checkbox marks, scroll handles, comparison arrows, and close indicators should be clean and geometric. Apply little or no texture. Do not use delicate engraving where a two-pixel line is carrying the whole meaning.

Input-device prompts should remain recognizable and use the project's approved glyph library. Do not ask an image model to invent consistent, legible controller lettering.

### 8.4 Family D — Decorative motifs

Use sparse linework, small dividers, subtle corner wear, or faint nonliteral engraving. Keep decoration out of dense text columns. A motif must never look like an unavailable menu item, a reward icon, or a navigation arrow.

### 8.5 Family E — State markers

Equipped, new, locked, favorite, warning, and requirement-failed markers need a consistent placement and symbol language. Use a shape or mark in addition to color. Keep badges separate from the item illustration so states can coexist and change without regenerating art.

## 9. Component recipes

### 9.1 Standard readable panel

Use a rectangular dark body, a separate faint grain layer, optional fine edging, and real text in a dedicated content region. Start with 24–32 units of content inset. Keep the panel center quiet and free from high-contrast scratches. Use stronger backing behind long descriptions and stat tables.

Separate the body from any vignette, glow, or frame. Do not generate an entire window with headings, values, buttons, and fog baked into one texture.

### 9.2 Equipment cell

Start with a 96 × 96 logical cell and an 80 × 80 content area, or a coordinated size appropriate to screen density. The item illustration, quantity, equipped badge, focus border, and glow are independent.

Maintain identical layout size in every state. Focus may change color/alpha and add outer decoration; it must not move adjacent cells or shrink the icon. An empty slot may show a faint category ghost. An unavailable occupied slot must not reuse the empty-slot appearance.

### 9.3 Selected list row

Use a broad, faint horizontal band with soft ends, a clear focus cue, and readable text. Start with a 48–56-unit row height before text scaling. Do not make the entire row a bright gold button. Hover should be weaker than keyboard/controller focus, with an explicit policy for switching input devices.

### 9.4 Tabs

Use consistent category symbols and labels where space permits. Clearly distinguish the active page from the currently focused tab. Keep the tab band and content heading stable while changing pages. Do not hide all category names behind hover-only tooltips.

### 9.5 Attribute/comparison table

Use stable left labels and right-aligned numeric columns. Group related statistics with spacing and faint separators. Show signed changes and readable requirement labels. Do not use tiny icons for every word or make all secondary values equally dim.

### 9.6 Tooltip and item details

Use a quiet, more opaque backing than the surrounding atmosphere. Place the title, short functional description, requirements, and optional secondary detail in that order. Keep the tooltip within the safe content area and avoid covering the selected item when another placement is available. Provide focus-based access and a dismiss action.

### 9.7 Dialog

Use the same visual vocabulary with stronger local contrast. Show one primary question, clear consequences, and clearly separated actions. A confirmation should not look like an ordinary information toast. Do not put decorative fog over the action labels.

### 9.8 Sliders, toggles, and settings

Use simple square toggles, clear checked marks, readable slider handles, and numeric values where they help precision. Describe what a setting changes. Separate apply/revert behavior from immediate settings. Do not make a slider's active region indistinguishable from its background track.

### 9.9 Gameplay HUD

Use compact clusters near screen edges, with strong priority for vital resources and current interactions. Keep the central action view unobstructed. Resource colors may differ, but shape, order, labels/help, and optional values must provide redundancy. Decorative borders must not obscure the actual fill boundary.

Allow player-controlled HUD visibility and scaling. Automatic hiding must not conceal critical changes or be the only way to reduce clutter. Temporary messages should not compete with subtitles, prompts, or essential status.

### 9.10 Fullscreen atmosphere

Provide haze and vignette as separate assets or independently controlled layers. Initial composition: 1920 × 1080, transparent, low-frequency olive-grey haze, fine restrained grain, darkened periphery, and an open central information field.

No characters, objects, architecture, symbols, lettering, stars, particles, sunbeams, or focal scene. For ultrawide displays, extend or recompose the atmosphere without stretching interface content. Background imagery may crop; text and interaction areas may not.

## 10. State specification

| State | Visual treatment | Interaction meaning |
|---|---|---|
| Default | Quiet dark body and readable neutral label | Available, not emphasized |
| Hover | Slight local tint or edge lift | Pointer presence only |
| Focus | Precise gold boundary, restrained outer glow | Navigation/confirmation target |
| Pressed | Brief contrast shift or interior deepening | Input received; no layout movement |
| Selected | Persistent chosen-row/cell treatment | Current data selection |
| Equipped / active | Small persistent badge | Actual game state |
| Disabled | Reduced illustration emphasis plus readable reason | Action unavailable |
| Locked | Explicit lock/requirement marker | Access condition not met |
| Error | Clear negative symbol/message | Operation failed; recovery available |
| Loading | Stable placeholder and progress/status treatment | Result pending |
| New / unread | Small secondary mark | Newly available content, not focus |

States may combine. Resolve conflicts with **error/critical communication → focus visibility → persistent state badge → hover → decoration**. This is a rendering priority, not permission to erase other state information.

Use one state owner per component. Do not let the hover script, focus animator, and selection presenter overwrite the same image color independently.

## 11. Motion and feedback

These are working timing tokens, not research claims about a universal optimal duration:

| Change | Initial duration |
|---|---:|
| Hover/focus tint | 80–120 ms |
| Press feedback | 60–100 ms |
| Tooltip entry | 100–160 ms after an intentional dwell/focus rule |
| Small panel transition | 120–180 ms |
| Full menu fade | 160–220 ms |

Use gentle ease-out for entry and simple fading. Avoid bounce, spring overshoot, elastic scale, and continuous decorative pulsing by default. Keep the logical target stationary throughout the animation. Feedback should begin promptly even when an operation continues asynchronously.

Reduced-motion mode should remove drift, scale, and pulsing, retaining an immediate or short fade-based state change. Sound and optional haptics may reinforce focus/confirmation, but visual feedback must stand on its own.

## 12. Universal asset brief

Complete this brief before generation. A missing semantic rectangle or intended display size is a production gap, not something to guess during prefab assembly.

```yaml
asset_id: ui_category_armor_default_v01
family: category_symbol
purpose: category_navigation
source_canvas_px: [512, 512]
smallest_review_canvas_px: [32, 32]
normal_review_canvas_px: [48, 48]
semantic_bounds: "One breastplate silhouette; measured after generation"
optical_center: "Record after review; do not auto-trim each state separately"
shape_language: "Practical rectangular composition; one clear silhouette"
materials: "Shallow worn engraving, subdued antique-metal values"
lighting: "Soft upper-left; weak fill; no environmental cast shadow"
palette_tokens: [metal.antique, text.secondary]
alpha_contract: "Real transparency; opaque main silhouette; soft anti-aliased edge"
background: "None; no frame; no ground plane"
state_baked_into_asset: false
resize_policy: "Uniform only; do not distort object aspect ratio"
separate_layers: [category_symbol, focus_border, focus_glow]
exclusions: "No text, lettering, logo, watermark, scene, neon, thick bevel, or rays"
review: "Inspect alpha plus 32/48/64-pixel composites on light and dark backings"
```

For panels add slice borders, semantic body bounds, content insets, and whether grain tiles. For glows add peak/center alpha and effect overscan. For sets add an approved style reference and shared optical-weight targets.

The generator's prompt is not proof of output dimensions, transparency, color precision, or nine-slice safety. Verify and, where necessary, perform a deterministic finishing/export pass.

## 13. Reusable generation prompts

### 13.1 Master style block

```text
Create one original production UI asset for a restrained, solemn dark-fantasy
interface. Use charcoal and olive-grey restraint, warm ivory and subtle antique
gold, practical geometry, shallow worn engraving, and quiet surface detail.
Prioritize silhouette and readability at the specified smallest display size.
Keep decoration subordinate to function. Use the supplied asset-family rules,
semantic body bounds, source canvas, and alpha contract.

No lettering, logo, watermark, scene, mockup backdrop, glossy plastic, neon,
excessive filigree, thick golden bevel, lens flare, or dramatic cast shadow.
Deliver only the requested layer. Do not merge content, frame, glow, and text.
```

### 13.2 Category-symbol prompt

```text
One [CATEGORY] category symbol: [GENERIC OBJECT/SILHOUETTE].
512×512 RGBA PNG with real transparent background. One centered silhouette
occupying approximately three quarters of the canvas; preserve breathing room.
Muted antique-metal / warm-grey shallow engraved treatment, front-facing or the
approved shared orientation, minimal internal detail. Match the approved set's
apparent scale, value range, lighting, and line weight. Must remain recognizable
when the entire canvas is reduced to 32 and 48 pixels.
No frame, badge, glow, label, ground plane, extra object, or checkerboard pixels.
```

### 13.3 Item-illustration prompt

```text
One isolated [ITEM], complete and unclipped, for an inventory illustration.
512×512 transparent RGBA PNG. Weathered believable materials, restrained natural
color, soft upper-left lighting, subtle painterly realism, category-consistent
orientation. Clear silhouette at 64 pixels and readable materials at 128 pixels.
Keep the same canvas registration and apparent scale as the approved item set.
No UI frame, quantity, selection glow, pedestal, environment, text, or watermark.
```

### 13.4 Selected-cell glow prompt

```text
One outer selection-glow layer on a 512×512 transparent canvas.
The intended square body is x=96, y=96, width=320, height=320 source pixels.
Create a soft, restrained antique-gold diffusion around that square perimeter.
Keep the center nearly transparent. Target peak alpha about 0.32; central alpha
at most 0.04; decay smoothly to negligible alpha before the canvas edge.
No solid panel fill, thick frame, text, symbol, object, sparks, or white hotspot.
The 320×320 semantic square is alignment metadata, not an extra visible square.
Exact bounds and alpha must be measured and corrected in the finishing pass.
```

### 13.5 Panel-surface prompt

```text
Create only the quiet surface texture for a practical dark rectangular UI panel.
Muted olive-charcoal, extremely subtle age and low-contrast grain, no focal point.
Keep the central reading field visually calm. No text, labels, icons, controls,
large stains, heavy border, outer shadow, or glow. The base color and opacity
will be controlled separately in the UI. Deliver a seamless tile when requested;
otherwise do not claim that the texture tiles.
```

### 13.6 Atmosphere prompt

```text
1920×1080 transparent RGBA atmospheric overlay: restrained olive-grey haze,
fine low-contrast static grain, slightly darker edges, and a nearly clear center.
No characters, items, architecture, symbols, text, particles, bright light shafts,
strong circular spotlight, or baked interface. Preserve subdued alpha and an
open central information field. This is an overlay, not an opaque background.
Verify final resolution and true alpha after generation.
```

### 13.7 Scalable frame prompt

```text
Create a thin, practical rectangular UI frame as an isolated transparent layer.
Subdued aged-metal line, shallow edge wear, minimal ornament. Keep all four
corners small and compatible, with straight quiet edge runs. No text, panel fill,
outer glow, and no irregular detail crossing planned slice boundaries.
Use [CANVAS SIZE] and the supplied slice-guide coordinates. Treat generated
geometry as a draft: rebuild exact borders and slice-safe seams deterministically.
```

## 14. Production workflow

**Brief → one approved sample → small-size review → set generation → deterministic cleanup → metadata → composite review → engine review.**

Approve one symbol, one item illustration, one cell, and one panel before generating a large collection. Use those approved examples to hold apparent scale, material treatment, lighting, and alpha behavior constant. Do not iterate each icon independently until it becomes a different art style.

Keep editable masters and export records outside the runtime asset folder. Export each deliverable separately. A contact sheet is a review artifact, not a replacement for separate PNG files. Save the prompt/brief and token revision with the asset so future additions can match the set.

For tiny glyphs, scalable frames, gradients, and simple glows, prefer a deterministic vector/procedural build when it gives cleaner geometry. Use generated art where texture and material interpretation add value.

PNG exports should preserve a real alpha channel, rather than a painted transparency checkerboard. PNG stores unassociated alpha; do not pre-bake a black or white background into semi-transparent edge colors.[^png]

## 15. Acceptance checklist

### Visual and semantic

- [ ] The asset's family is correct: symbol, object illustration, functional glyph, decoration, or state marker.
- [ ] Its meaning is recognizable at the smallest required size without zooming in.
- [ ] The set has consistent apparent weight, orientation, lighting, and canvas registration.
- [ ] Colors and opacity follow the recorded token revision or an approved exception.
- [ ] The central reading field remains calm; decorative wear does not cut through important text.
- [ ] Focus, selected data, equipped state, unavailable state, and empty state are distinguishable.

### Asset integrity

- [ ] Dimensions and alpha were inspected, not assumed from the prompt.
- [ ] No checkerboard, matte fringe, label, unintended frame, or cast shadow is baked in.
- [ ] Semantic body bounds, content padding, and effect overflow are recorded separately.
- [ ] All state variants share compatible bounds and optical registration.
- [ ] The glow reaches negligible alpha before its outer edge, unless intentional clipping is documented.
- [ ] Text remains editable/localizable; counts and badges are separate layers.

### Screen and interaction

- [ ] Final composited text and meaningful boundaries meet the chosen contrast targets.
- [ ] Bright, dark, mixed-detail, and moving-world backgrounds were tested.
- [ ] Keyboard/controller navigation works without hover and preserves context.
- [ ] Large text, clear-font, high-contrast, and reduced-motion modes remain functional.
- [ ] Empty, loading, error, and destructive-action states are designed.
- [ ] No glow, shadow, or ornament changes layout spacing or hit targets.

Do not label an asset production-ready solely because it looks attractive in an image viewer. Production readiness includes correct geometry, usable states, actual output-size readability, and an engine composite review.

## 16. Sources and evidence scope

Research checked on 2026-09-12. These references support interaction, accessibility, and file-format principles. The aesthetic tokens and component recipes are the project's own working specification.

[^ux]: Nielsen Norman Group, [10 Usability Heuristics for User Interface Design](https://www.nngroup.com/articles/ten-usability-heuristics/).
[^nav]: Microsoft, [Xbox Accessibility Guideline 112: UI navigation](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/112).
[^focus]: Microsoft, [Xbox Accessibility Guideline 113: UI focus handling](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/113).
[^destructive]: Microsoft, [Xbox Accessibility Guideline 115: Error messages and destructive actions](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/115).
[^contrast]: W3C, [Understanding 1.4.3: Contrast (Minimum)](https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum.html); Microsoft, [Xbox Accessibility Guideline 102: Contrast](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/102).
[^nontext]: W3C, [Understanding 1.4.11: Non-text Contrast](https://www.w3.org/WAI/WCAG21/Understanding/non-text-contrast.html).
[^text]: Microsoft, [Xbox Accessibility Guideline 101: Text display](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/101).
[^target]: W3C, [Understanding 2.5.8: Target Size (Minimum)](https://www.w3.org/WAI/WCAG22/Understanding/target-size-minimum.html).
[^png]: W3C, [Portable Network Graphics Specification, Third Edition](https://www.w3.org/TR/png-3/).
