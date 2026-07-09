# 03-04 Shadow And Lighting Validation

Use this checklist after opening `ShadoughUnity` in Unity `2021.3.45f1c2`.

## Before Play

1. Open `Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity`.
2. Wait until Unity finishes importing the new shadow and lighting sprites.
3. Confirm there are no missing script errors in the Console.
4. Select these scene objects and confirm their SpriteRenderer sprites:

| Object | Expected source sprite |
| --- | --- |
| `TreeShadow_Topdown` | `tree_shadow_base.png` |
| `BeamShadow_Topdown` | `beam_shadow_base.png` |
| `KeyShadow_Topdown` | `key_shadow_base.png` |

5. Select `Player_Topdown` and confirm:

| Component | Expected binding |
| --- | --- |
| `PlayerSelfShadowCutter.playerShadowSprite` | `player_shadow_base.png` |
| `PlayerSelfShadowCutter.pastedPlayerShadowSprite` | `player_shadow_lure_pasted.png` |
| `PlayerLanternController.lanternHaloSprite` | `lantern_warm_halo.png` |
| `PlayerLanternController.lanternRoundMaskSprite` | `lantern_light_mask_round.png` |
| `PlayerLanternController.lanternDirectionalMaskSprite` | `lantern_light_mask_directional.png` |

6. Select `UI` and confirm `RevealViewController.overlayTextureAsset` is `reveal_view_overlay.png`.

## Full Demo Route

Run Play Mode and test the full current game loop:

1. Move with WASD from the lower-left start area.
2. Press `G` to plant the lantern.
3. Confirm the lantern shows a warm halo and light masks.
4. Hold `Shift` for Reveal View.
5. Confirm the Reveal View overlay appears and shadows become easier to read.
6. Press `E` near `TreeShadow_Topdown` to cut the tree shadow.
7. Confirm the shadow inventory shows the tree shadow icon and `CanStandOn`.
8. Press `F` to paste the tree shadow across the river/gap.
9. Confirm the pasted tree shadow uses the pasted visual and still lets the player cross.
10. Cut `BeamShadow_Topdown`.
11. Confirm the inventory shows the beam icon and `CanPress`.
12. Paste the beam shadow onto `PressurePlate_Topdown`.
13. Confirm the pressure door opens.
14. Cut `KeyShadow_Topdown`.
15. Confirm the inventory shows the key icon and `CanUnlock`.
16. Paste the key shadow into `Lock_Topdown_Trigger`.
17. Confirm the lock door opens.
18. Hold `Shift` and press `Q` to cut the player's own shadow.
19. Confirm the inventory shows the player shadow icon and `CanAttractEnemy`.
20. Paste the player shadow into `LureArea_Topdown`.
21. Confirm `ShadowSeeker_Topdown` is attracted away from the passage.
22. Reach `FinalClockCore_Topdown`.
23. Press `E` to start the clock.
24. Confirm the demo completion UI appears.

## Ability Regression Tests

The visual pass is correct only if these negative tests still hold:

| Shadow | Must not do |
| --- | --- |
| Tree shadow | Must not press the pressure plate, unlock the lock, or attract the enemy |
| Beam shadow | Must not work as a bridge, unlock the lock, or attract the enemy |
| Key shadow | Must not work as a bridge, press the pressure plate, or attract the enemy |
| Player shadow | Must not work as a bridge, press the pressure plate, or unlock the lock |

## Visual Checks

- The tree shadow still changes length and direction before it is cut.
- Once pasted, a shadow no longer changes shape with the lantern.
- Pasted shadows keep the cut-time collider behavior.
- The Reveal View overlay must not hide the player, doors, lock, pressure plate, enemy, or final clock core.
- The lantern light visuals should make the lantern easier to locate without blocking gameplay objects.
- The inventory icon should match the currently carried shadow type.

## Current Game Feature Showcase

After this pass, the current playable demo demonstrates:

- Top-down player movement.
- Planting and retrieving a lantern.
- Lantern-driven tree shadow direction and length.
- Reveal View for reading and cutting shadows.
- Cutting scene shadows with `E`.
- Cutting the player's own shadow with `Q`.
- Free-pasting carried shadows with `F`.
- Shadow inventory display with type icon and ability text.
- Tree shadow bridge traversal through `CanStandOn`.
- Beam shadow pressure-plate solving through `CanPress`.
- Key shadow lock solving through `CanUnlock`.
- Player shadow enemy lure through `CanAttractEnemy`.
- Final clock core completion interaction.
- Initial Chinese pixel shadow-puppet lighting and shadow visual pass.

## If Something Looks Wrong

- If a shadow looks too large or too small, check Sprite import `Pixels Per Unit = 160` first.
- If a pasted shadow triggers the wrong mechanism, inspect the source object's `ShadowInteractable` ability flags.
- If crossing the river fails, inspect `TreeShadow_Topdown` collider size and `CanStandOn`.
- If Reveal View does not show the overlay, check `UI > RevealViewController > overlayTextureAsset`.
- If lantern light visuals do not appear, check the three `PlayerLanternController` lighting sprite fields and press Play so runtime child renderers are created.
