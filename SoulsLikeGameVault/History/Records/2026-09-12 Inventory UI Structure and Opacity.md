---
title: "2026-09-12 Inventory UI Structure and Opacity"
type: implementation-record
domains:
  - ui
  - penpot
status: done
authority: historical
updated: 2026-09-12
aliases: []
tags:
  - history/change
---

# 2026-09-12 Inventory UI Structure and Opacity

## Implementation Record Contract

### Outcome

Revised the three inventory mockups in [Penpot](https://design.penpot.app/#/workspace?team-id=40e06342-8830-80d6-8008-9cd07d0b9d35&file-id=d8ac01df-6646-81d2-8008-9f872a8dcff4&page-id=2876d2eb-131b-80f3-8008-9f877daa7629): category controls sit above the left item grid; Runes Held is inside Character Status immediately below Level; solid panel and cell fills have independent translucent, feathered backing layers.

### Why

The previous header placed both category navigation and currency outside their reference sections. Solid rectangular surfaces also obscured the atmosphere and produced hard edges. The user requested closer structure and compositing based on `Assets/Art/Reference/InventoryRef_2.png` and the previously supplied `InventoryRef_1.png`, excluding gameplay imagery and the central character silhouette.

### Changed Files and Assets

- Penpot page `01 Experiments`: Armaments, Lore, Simple, and adjacent design notes.
- Main/Lore status order: heading, Level, Runes Held, base attributes.
- Lore now retains the status rail while its middle reading panel fits the same column width as combat details.
- Simple retains the reduced layout with detail/status rails hidden.
- Existing project icons, generated image assets, and six view-switch prototype links are retained.

### Decisions and Tradeoffs

- Preserved the project's five broad categories (Weapons, Armor, Talismans, Consumables, Key items), with Melee/Ranged/Shields under Weapons. This is a project adaptation, not the complete Elden Ring taxonomy.
- Background opacity tokens: primary panel 0.82, secondary/status panel 0.76, ordinary cell body 0.34, selected-cell tint 0.14. Text and structural containers have opacity 1. Panel backings use 24 px blur; cell backings use 8 px blur. Effects do not determine logical layout bounds.
- Values are chosen design defaults based on [[Knowledge/Guides/UI/Dark Fantasy UI Style and Asset Rules]], not recovered Elden Ring settings.
- The existing Cinzel family is Penpot's built-in font, with EB Garamond body text and Inter utility labels. No custom-font upload is claimed.

#### Researched inventory categories

[Elden Ring's Inventory documentation](https://eldenring.wiki.gg/wiki/Inventory) lists 18 categories: Tools; Ashes; Crafting Materials; Bolstering Materials; Key Items; Sorceries; Incantations; Ashes of War; Melee Armaments; Ranged Weapons & Catalysts; Arrows & Bolts; Shields; Head; Chest; Arms; Legs; Talismans; Info. Retrieved 2026-09-12 from the indexed page; direct page access returned 403.

The official [version 1.12 patch notes](https://en.bandainamcoent.eu/elden-ring/news/elden-ring-patch-notes-version-112), dated 2024-06-19, add an optional Recent Items tab and new-item markers, enabled through Display settings. The full taxonomy and sources are also recorded beside the Penpot screens.

### Validation Evidence

- Reviewed the main export and live Lore/Simple browser canvases.
- All three screens retain 25 logical cells and two view-switch prototype links each.
- Category navigation is a child of the left inventory section in every view; screen headers contain only the inventory title/emblem.
- Both status rails place Runes Held after Level. All checked layout children fit their parent bounds after correcting the attribute-list height.
- All text has explicit opacity 1; ordinary cell containers have no opaque fill. Backing-layer opacity and blur values were read back from Penpot.
- Runtime scene compositing, platform input, and accessibility over gameplay backgrounds remain implementation validation; this is a design revision, not a Unity prefab implementation.

### Documentation Updated

Penpot design notes and this record. The supplied rule guides and required AGENTS routing were registered in [[History/Records/2026-09-12 UI Rule Guides Registered]].

### Follow-Up

During implementation, use [[Knowledge/Guides/UI/Unity UI Asset Layout and Troubleshooting Rules]] to keep body, content, effects, and hit bounds independent, and validate the composite in the target Unity rendering path.
