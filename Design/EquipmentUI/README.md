# Equipment UI design

[Open the Equipment board in Penpot](https://design.penpot.app/#/workspace?team-id=40e06342-8830-80d6-8008-9cd07d0b9d35&file-id=d8ac01df-6646-81d2-8008-9f872a8dcff4&page-id=2876d2eb-131b-80f3-8008-9f877daa7629&board-id=8b1ed794-e91e-807d-8008-a328d714c22e)

Created through Penpot MCP on **02 Approved**, beneath the existing inventory boards. The new equipment panel follows `Assets/Art/Reference/EquipmentRef.png`, using the supported project slots. The existing inventory lore layout, Character Status panel, atmosphere, selection glow, header frame and item artwork are reused.

## Deliverables

- Four 1920 × 1080 boards: Long Sword, Wooden Shield, Crimson Flask, and an empty chest slot.
- One empty-slot asset board with ten masters and 32 / 48 / 64 px samples.
- `Icons/`: separate transparent PNGs at 256 × 256 and 512 × 512 for Weapon, Shield, Flask, Ring, Head, Chest, Arms, Legs, Arrow and Bolt.
- `Source/`: accepted image-generation masters. Prompts and provenance are in `source-manifest.json`.
- `Review/`: PNG exports of all boards.
- `icon-metadata.json`: source/export dimensions, alpha bounds and registration.
- `design-quality-report.json`: visual review and remaining API binding limitation.

## Slot map

| Row | Runtime identifiers | Empty icon |
|---|---|---|
| Right hand | RightHand1–3 | Weapon |
| Left hand | LeftHand1–3 | Shield |
| Ammo | Arrow1–2, Bolt1–2 | Arrow / Bolt |
| Armor | Head, Chest, Arms, Legs | Matching armor piece |
| Talismans | Talisman1–4 | Ring |
| Quick items | QuickItem1–5 | Flask |
| Quick items, continued | QuickItem6–10 | Flask |

This is 28 slots, matching `EquipmentSlotCatalog` and the seven rows in `Assets/Prefabs/Ui/Equipment/EquipmentUi.prefab`. Ghost imagery is a visual cue, not a restriction on runtime item compatibility.

Category row labels are omitted from all four equipment screens. The slot grid aligns to the left panel edge; the table above remains the handoff reference.

## Layout and assets

Logical canvas: 1920 × 1080. Main columns: 584 / 648 / 464 px, separated by 48 px. Slot body and hit area: 88 × 88; icon canvas: 64 × 64; gap: 12; focus glow: 120 × 120 with 16 px decorative overflow. Background, item image, focus glow, equipped badge and quantity remain independent layers.

Empty icons use `opacity.equipment.empty-icon = 0.45`; occupied art uses full opacity. PNG artwork itself is opaque over transparent alpha so the UI controls ghost opacity. No frame, badge, label or focus effect is baked into the exports. The Arms export has centered transparent padding at 74% scale to match the other armor silhouettes.

Reuse sources include `Assets/Art/Textures/InventoryUI/InventoryAtmosphereOverlay.png`, `InventorySelectionGlow.png`, the approved inventory cell/panel treatment, `Assets/Art/Textures/MenuIcons/FlatIconFrame.png`, `FlatShieldIcon.png`, and the existing `ItemIcons` sword, shield and flask artwork.

Long Sword, Wooden Shield and Crimson Flask descriptions/lore and weights come from `Assets/Settings/Items/ItemDatabase.asset`. Supplemental headings and empty-state instructions are design copy. Character Status retains the existing illustrative values; equipped flask quantity 5 is also illustrative.

## Preview behavior

In Penpot presentation mode, click RightHand1, LeftHand1, QuickItem1 or Chest to switch between the four selection/lore states. Other slots and the action-key footer remain design previews.

## Verification and handoff

All four screens and the icon board were exported and visually inspected. Each equipment screen has exactly 28 slots, one selection, no missing image fills, and flex layout on every board. Alpha checks confirmed transparent corners on all ten assets; samples were reviewed at 32, 48 and 64 px.

Existing semantic colors, sizes and typefaces are preserved. Penpot rejected native `fontFamilies` token binding, and some shapes expose incomplete token metadata despite correct rendered values. The intended existing tokens/styles are retained in the design notes; no new visual palette was introduced.

## Unity integration

The design is integrated in `Assets/Prefabs/Ui/Equipment/EquipmentUi.prefab`. Its existing GUID and `EquipmentUi` Addressables mapping are preserved. The shared `Assets/Prefabs/Ui/Inventory/LoreCard.prefab` and `CharacterStats.prefab` remain linked nested prefab instances; their source assets are unchanged.

Ten 256px production sprites are imported under `Assets/Art/Textures/EquipmentUI/EmptySlots/`, using Single / Full Rect, PPU 100, straight alpha, sRGB, bilinear filtering, clamp wrapping, no mipmaps and no compression. Slot bodies remain 88px with 64px art and separate focus/equipped/quantity layers.

`EquipmentUi.DisplaySlot` uses the existing `LoreCardUi.Display` for item description and lore. Empty slots replace old text/art through `DisplayEmpty`; slot icons return to full opacity when equipped. Selection survives rebinding and returning from the existing inventory route. The detailed inspector is retained under the legacy picker overlay; the active equipment-change flow continues to use the shared inventory screen.

Unity captures: [1920 × 1080](Unity/Equipment-Unity-1920.png), [1280 × 720](Unity/Equipment-Unity-1280.png), [Wooden Shield](Unity/Equipment-Unity-Shield.png), [Crimson Flask](Unity/Equipment-Unity-Flask.png), and [empty chest](Unity/Equipment-Unity-Empty-Chest.png). The previews use the real ItemDatabase and CharacterData with an illustrative three-item loadout and flask quantity five. The shared runtime fields determine which stats and lore appear; Penpot-only illustrative Level, skill and supplementary lore headings were not added as static runtime data.

`Unity/Render-Equipment-Preview.cs.txt` is an isolated Unity eval capture script. Its temporary HDRP camera disables exposure and postprocessing to preserve overlay UI colors; it closes its preview scene automatically. `Finalize-Equipment-Presentation.cs.txt` records the final Equipment-only presentation overrides. Source prefab links remain intact, with conflicting inherited layout components disabled on the Equipment instances.

Focused regression coverage is in `Assets/Scripts/Editor/Tests/Equipment/EquipmentUiPresentationTests.cs`: all five tests pass. Together with the inventory binding and workspace prefab configuration checks, 26 assigned EditMode checks pass. Final evidence and the deferred Play Mode coverage are recorded in the [implementation record](../../SoulsLikeGameVault/History/Implementation%20Records/2026-09-14%20Equipment%20UI%20Integration.md).

To regenerate export sizes from accepted masters on Windows:

```powershell
& .\Design\EquipmentUI\Export-EquipmentIcons.ps1
```
