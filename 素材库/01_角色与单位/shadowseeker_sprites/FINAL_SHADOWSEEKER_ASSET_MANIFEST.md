# ShadowSeeker Sprite Final Asset Manifest
Updated: 2026-07-07

This folder contains the finalized ShadowSeeker enemy unit sprite package for `01_角色与单位`.
The package keeps the existing concept identity: black paper-shadow body, gold-trimmed mask face, vertical glowing eyes, red stitches, torn paper ears, ragged cloak/tail, and Chinese shadow-cutting pixel style.

## Final Assets

| Asset | Folder | Runtime Use |
| --- | --- | --- |
| Design turnaround | `shadowseeker_design_turnaround_v1` | visual reference / identity lock |
| Idle | `shadowseeker_idle_4dir_v1` | 4-direction idle loop |
| Patrol move | `shadowseeker_patrol_move_4dir_v1` | normal patrol movement |
| Alert | `shadowseeker_alert_4dir_v1` | target acquired / warning state |
| Chase | `shadowseeker_chase_4dir_v1` | fast pursuit movement |
| Attack player | `shadowseeker_attack_player_4dir_v1` | close-range claw attack against the player |
| Attracted | `shadowseeker_attracted_4dir_v1` | tracking pasted player shadow lure |
| Lure reached | `shadowseeker_lure_reached_4dir_v1` | stopped / entranced at lure |
| Stunned | `shadowseeker_stunned_4dir_v1` | briefly disabled or confused |
| Hurt | `shadowseeker_hurt_4dir_v1` | hit / light damage feedback |
| Dissolve | `shadowseeker_dissolve_4dir_v1` | death / retreat / vanish |

## Direction Order

For all runtime `4x4` sheets: row 1 down/front, row 2 left, row 3 right, row 4 up/back. Columns are the 4 animation frames.

## Folder Contents

Each runtime folder contains `raw-sheet.png`, `raw-sheet-clean.png`, `sheet-transparent.png`, 16 split frame PNGs, `animation.gif`, `prompt-used.txt`, `source-file.txt`, and `pipeline-meta.json`. The attack-player folder also includes `final-alpha-qc.json`.

## QC

All runtime transparent sheets were checked for frame count and final alpha edge touch. The attack-player sheet has 16 frames and `final_alpha_edge_touch_frames` is empty in `final-alpha-qc.json`.

## Unity Import Notes

Use `sheet-transparent.png` for Unity import. Suggested settings: Sprite Mode Multiple, 4x4 grid, 256x256 cells, Point filter, no compression, bottom/feet pivot.
