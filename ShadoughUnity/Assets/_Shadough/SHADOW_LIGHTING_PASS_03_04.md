# 03-04 Shadow And Lighting Pass

Branch scope: `03-04-shadow-lighting`
Unity version: `2021.3.45f1c2`
Scene target: `Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity`

## Goal

This pass connects the prepared assets from:

- `素材库/03_影子系统`
- `素材库/04_光源与光影`

The change keeps the existing Topdown v0.6 gameplay loop intact. It improves visual readability for cuttable shadows, pasted shadows, the player self-shadow, lantern lighting, Reveal View, and the shadow inventory display.

## Imported Assets

### Shadow System

Imported to `Assets/_Shadough/Sprites/Shadows`:

- `Tree/tree_shadow_base.png`
- `Beam/beam_shadow_base.png`
- `Key/key_shadow_base.png`
- `Player/player_shadow_base.png`
- `Pasted/tree_shadow_pasted.png`
- `Pasted/beam_shadow_pasted.png`
- `Pasted/key_shadow_pasted.png`
- `Pasted/player_shadow_lure_pasted.png`
- `Pasted/pasted_shadow_edge.png`
- `Preview/shadow_preview_ghost.png`
- `Highlights/cuttable_shadow_outline.png`
- `Highlights/cuttable_edge_light.png`
- `Reveal/reveal_shadow_texture.png`

Imported to `Assets/_Shadough/Sprites/UI/ShadowSlot`:

- `shadow_inventory_icon_tree.png`
- `shadow_inventory_icon_beam.png`
- `shadow_inventory_icon_key.png`
- `shadow_inventory_icon_player.png`

Imported to `Assets/_Shadough/Materials/Shadows` as visual references:

- `shadow_material_reference.png`
- `shadow_color_palette.png`
- `shadow_collision_guide.png`

### Lighting

Imported to `Assets/_Shadough/Sprites/Lighting`:

- `LightMasks/lantern_light_mask_round.png`
- `LightMasks/lantern_light_mask_directional.png`
- `Halo/lantern_warm_halo.png`

Imported to `Assets/_Shadough/Sprites/UI/Reveal`:

- `reveal_view_overlay.png`

Imported to `Assets/_Shadough/Materials/Lighting` as visual reference:

- `global_light_palette.png`

## Scene Binding Changes

Updated `ClockTower_TopdownPrototype.unity`:

- `TreeShadow_Topdown` now uses `tree_shadow_base.png` and stores `tree_shadow_pasted.png` for pasted output.
- `BeamShadow_Topdown` now uses `beam_shadow_base.png` and stores `beam_shadow_pasted.png` for pasted output.
- `KeyShadow_Topdown` now uses `key_shadow_base.png` and stores `key_shadow_pasted.png` for pasted output.
- `PlayerSelfShadowCutter` now uses `player_shadow_base.png`, `player_shadow_lure_pasted.png`, and the player shadow inventory icon.
- `PlayerLanternController` now references warm halo, round light mask, and directional light mask sprites.
- `RevealViewController` now references `reveal_view_overlay.png` and uses warmer highlight colors.
- The cut outline, free paste range circle, and paste preview tint were shifted from pure white/black toward lantern-warm colors.

## Script Changes

### Shadow data

`ShadowItemData` now carries:

- `pastedSprite`: optional visual used after a shadow is pasted.
- `inventoryIcon`: optional UI icon shown in the shadow status panel.

### Cuttable shadows

`ShadowInteractable` now exports the source sprite, pasted sprite, inventory icon, collider size, rotation, and gameplay flags into `ShadowItemData` when cut.

### Pasted shadows

`FreeShadowPlacer` and `PastedShadowObject` now prefer `pastedSprite` when creating a pasted shadow. They keep the collider size, collider offset, local scale, rotation, and gameplay properties from the cut moment.

### Player self-shadow

`PlayerSelfShadowCutter` now supports a separate pasted player-shadow lure sprite while keeping `CanAttractEnemy = true` as the player shadow's only active gameplay property.

### Lantern lighting

`PlayerLanternController` now creates optional child sprite renderers for:

- warm lantern halo
- round light mask
- directional light mask

The directional mask follows the player's facing direction. This is visual only and does not recalculate shadow colliders.

### Reveal View

`RevealViewController` can now draw a real overlay texture instead of only a generated black screen texture.

### Shadow UI

`ShadowStatusUI` now draws the carried shadow icon when `ShadowItemData.inventoryIcon` is available, while retaining the text list of gameplay abilities.

## Gameplay Rules Preserved

This pass does not change the core rule system:

- `TreeShadow_Topdown`: `CanStandOn = true`
- `BeamShadow_Topdown`: `CanPress = true`
- `KeyShadow_Topdown`: `CanUnlock = true`
- `PlayerShadow`: `CanAttractEnemy = true`

The visual sprite can change between source and pasted state, but the collider data and ability flags are still captured from the cut moment.

## Known Boundaries

- This pass does not implement strong-light shadow destruction.
- This pass does not add multi-light real-time shadow projection.
- This pass does not replace the whole graybox environment.
- `shadow_preview_ghost.png`, `pasted_shadow_edge.png`, and `reveal_shadow_texture.png` are imported for the 03/04 asset set, but this pass keeps preview/collider behavior conservative to avoid breaking the current demo loop.
