# Lantern Sprite Final Asset Manifest
Updated: 2026-07-07

This folder contains the finalized long-lantern assets for `01_?????`.
The protagonist's held-lantern frames remain in `hero_sprites`; this package contains the independent lantern prop used when placed, picked up, or disabled.

## Final Assets

| Asset | Folder | Runtime Use |
| --- | --- | --- |
| Design turnaround | `lantern_design_turnaround_v1` | visual reference / shape lock |
| Placed idle lit lantern | `lantern_world_idle_4dir_v1` | ground lantern idle, 4 directions x 4 flicker frames |
| Place sync | `lantern_place_sync_4dir_v1` | overlays with `hero_place_lantern_4dir_v1`, 4 directions x 4 frames |
| Pickup sync | `lantern_pickup_sync_4dir_v1` | overlays with `hero_pickup_lantern_4dir_v1`, 4 directions x 4 frames |
| Off / disabled | `lantern_off_disabled_4dir_v1` | inactive lantern, 4 directions in 2x2 |

## Direction Order

For 4x4 sheets: row 1 down/front, row 2 left, row 3 right, row 4 up/back. Columns are animation frames.
For the 2x2 off sheet: top-left down, top-right left, bottom-left right, bottom-right up.

## Import Notes

Use `sheet-transparent.png` for Unity import. Suggested import settings: Sprite Mode Multiple, Point filter, no compression, pivot bottom/feet. Keep the original generated `raw-sheet.png` files as source records.

## QC

All runtime transparent sheets were checked for frame count and alpha edge touch. `edge_touch_frames` is empty in the final metadata for all runtime sheets.
