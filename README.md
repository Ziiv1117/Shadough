# Shadough / 《剪影师》

Shadough / 《剪影师》 is a Unity 2D puzzle demo about cutting and pasting shadows to change the playable scene.

The current playable target is the top-down graybox scene:

`ShadoughUnity/Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity`

The player cuts shadows from the world, keeps their original silhouette shape, and pastes them back into the level. Mechanisms respond to pasted shadow properties and collider contact, not to hard-coded shadow source types.

## Environment And Reproduction

This repo is a Unity project. Open the Unity project folder at `ShadoughUnity`, not the repository root.

### Required Software

| Tool | Version / Requirement | Notes |
| --- | --- | --- |
| Unity Editor | `2021.3.45f1` (`0da89fac8e79`) | Recorded in `ShadoughUnity/ProjectSettings/ProjectVersion.txt`. Use this exact editor version for best reproducibility. |
| Unity Hub | Any version that can install/open Unity `2021.3.45f1` | Used only to install and open the project. |
| Git | Current stable Git for Windows/macOS/Linux | Required to clone the repo and fetch Git-based Unity packages. |
| Network access | Unity Package Manager + GitHub access | Required on first open to restore packages, including `com.coplaydev.unity-mcp`. |
| Code editor | Visual Studio, Rider, or VS Code | Optional for running the demo, useful for script work. The project includes Unity IDE integration packages. |

No Node.js, Python, npm, pip, external game server, or custom CLI setup is required to open and run the current demo.

### Unity Packages

Unity restores these from `ShadoughUnity/Packages/manifest.json` and `ShadoughUnity/Packages/packages-lock.json`.

| Package | Version / Source | Purpose |
| --- | --- | --- |
| `com.unity.render-pipelines.universal` | `12.1.15` | URP 2D rendering pipeline. |
| `com.unity.feature.2d` | `2.0.1` | Unity 2D feature set. |
| `com.unity.2d.pixel-perfect` | `5.0.3` | Pixel-art camera support for future Chinese pixel-art pass. |
| `com.unity.2d.animation` | `7.1.1` | 2D animation tooling. |
| `com.unity.2d.psdimporter` | `6.0.9` | Layered 2D art import support. |
| `com.unity.2d.spriteshape` | `7.0.7` | Sprite shape tooling. |
| `com.unity.2d.tilemap.extras` | `2.2.7` | Tilemap helper tooling. |
| `com.unity.textmeshpro` | `3.0.6` | Text rendering. |
| `com.unity.ugui` | `1.0.0` | Unity UI. |
| `com.unity.visualscripting` | `1.9.4` | Unity visual scripting package, currently not required by the core loop. |
| `com.unity.test-framework` | `1.1.33` | Unity test framework. |
| `com.coplaydev.unity-mcp` | Git package, lock hash `85c101f5329ec1b0c6f70cba44614166dd78f53c` | Optional editor automation / MCP tooling; Unity Package Manager restores it from GitHub. |

### Clone And Open From Scratch

1. Install Unity Hub.
2. Install Unity Editor `2021.3.45f1`.
3. Install Git.
4. Clone the cloud repo:

```powershell
git clone <repo-url>
cd Shadough
```

5. In Unity Hub, choose `Add project from disk`.
6. Select `ShadoughUnity`.
7. Open it with Unity `2021.3.45f1`.
8. Wait for Unity to rebuild `Library/` and restore packages. The first open can take several minutes.
9. If Unity asks to enter Safe Mode because packages are still compiling, wait for Package Manager restore first, then reopen normally.

### Run The Current Demo

1. Open scene:

```text
Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity
```

2. Press Play in the Unity Editor.
3. Use the controls in the `Controls` section below.
4. The expected playable route is:

```text
Lantern -> TreeShadow crossing -> CanPress plate -> CanUnlock lock -> PlayerShadow lure -> FinalClockCore
```

Build Settings should include only:

```text
Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity
```

### Versioned Vs Generated Files

Keep these in the repo:

- `ShadoughUnity/Assets`
- `ShadoughUnity/Packages/manifest.json`
- `ShadoughUnity/Packages/packages-lock.json`
- `ShadoughUnity/ProjectSettings`
- Root planning and asset docs such as `策划.md`, `VERSION_NOTES.md`, and `素材库/完整素材清单.md`

Do not commit Unity-generated local folders:

- `ShadoughUnity/Library`
- `ShadoughUnity/Temp`
- `ShadoughUnity/Obj`
- `ShadoughUnity/Build`
- `ShadoughUnity/Builds`
- `ShadoughUnity/Logs`
- `ShadoughUnity/UserSettings`

These are already covered by `.gitignore` and will be regenerated locally.

### Troubleshooting

- If the project opens with missing package errors, confirm network access to `packages.unity.com` and GitHub, then reopen Unity.
- If Unity opens the wrong folder, close it and add `ShadoughUnity` in Unity Hub instead of the outer `Shadough` folder.
- If URP/2D lighting looks wrong, check that `Assets/Settings/UniversalRP.asset` is assigned in Project Settings > Graphics.
- If the demo scene is not in Build Settings, add `Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity` and remove old demo scenes.
- If scripts appear missing right after clone, wait for Unity compilation and package restore to finish before editing scene objects.

## Art Direction Target

The locked visual direction is Chinese pixel shadow-puppet style.

- Keep the current top-down prototype as the playable graybox foundation, then replace graybox objects with Chinese pixel-art tiles, props, shadows, UI, VFX, and audio.
- Reinterpret `ClockTower_TopdownPrototype` as a Chinese bell / drum tower space rather than a western clock tower or generic dungeon.
- Primary visual language: lantern light, tiled roofs, timber beams, carved windows, paper screens, old stone paths, copper locks, shadow-puppet silhouettes, paper-cut edges, ink-shadow blocks, cinnabar highlights, and ritual clock-core motifs.
- Readability comes first: every cuttable and pasted shadow must remain clear at gameplay distance, including its length, width, angle, collision footprint, and interaction state.
- UI should use paper, plaque, seal, lantern, and paper-cut motifs with pixel-readable Chinese text.
- Audio should lean toward folk object textures such as paper, bamboo, wood, bell, gong, drum, lantern flame, and cloth screen movement.
- Avoid drifting toward western gothic fantasy, pure horror, realistic ink painting, or generic dark fantasy.
- The art pass must preserve the existing rule that pasted shadows keep their captured shape and interact through properties and colliders.

## Current Demo State

- Top-down 2D graybox prototype
- `ClockTower_TopdownPrototype` is the main playable demo scene
- Build Settings include only `ClockTower_TopdownPrototype`
- Old side-scroller demo content has been archived under `Assets/_Shadough/_Archive/SideScroller/`
- Current playable loop ends at `FinalClockCore_Topdown`

## Current Demo Features

- WASD top-down player movement
- Basic Reveal View for reading shadow objects
- Player-held lantern
- G to plant or retrieve the lantern
- Lantern-driven `TreeShadow_Topdown` length and direction
- E to cut scene shadows while Reveal View is active
- F to freely paste the carried shadow
- Q to cut the player's own shadow while Reveal View is active
- `TreeShadow_Topdown` crossing through `CanStandOn` / `CanTraverse`
- `BeamShadow_Topdown` pressure plate door through `CanPress`
- `KeyShadow_Topdown` lock door through `CanUnlock`
- Player shadow lure for `ShadowSeeker_Topdown` through `CanAttractEnemy`
- `FinalClockCore_Topdown` demo victory flow
- Basic interaction / victory UI prompts

## Core Design Principles

- `ShadowType` only represents the shadow source label.
- Mechanisms should not decide functionality directly from `shadowType`.
- Mechanisms detect shadow properties such as `CanStandOn`, `CanPress`, `CanUnlock`, `CanAttractEnemy`, and `CanBlock`.
- Cutting scene shadows requires Reveal View.
- Cutting the player's own shadow requires Reveal View.
- Pasting shadows does not require Reveal View.
- `FinalClockCore_Topdown` E interaction does not require Reveal View.
- The player starts with a handheld lantern.
- The lantern only affects uncut cuttable shadows.
- In the current top-down prototype, the lantern only drives `TreeShadow_Topdown`.
- The lantern does not affect carried `ShadowItemData`.
- The lantern does not affect pasted `PastedShadowObject` instances.
- `TreeShadow_Topdown` must be cut while Reveal View is active and the lantern is planted.
- Once cut, `ShadowItemData` stores the current shadow shape, scale, rotation, collider size, collider offset, and gameplay properties.
- Pasted shadows preserve the shape captured when they were cut.
- Visual shadows and gameplay shadows are separated.

## Controls

- WASD: Move
- Hold Shift: Reveal Shadows
- E while Revealing: Cut Shadow
- Q while Revealing: Cut Self Shadow
- F: Paste Shadow
- G: Plant / Retrieve Lantern
- E near `FinalClockCore_Topdown`: Start Clock

## Demo Walkthrough

1. Start in the lower-left area.
2. Move near `TreeShadow_Topdown` and watch the handheld lantern change its length and direction.
3. Press G to plant the lantern.
4. Hold Shift and press E to cut the stable `TreeShadow_Topdown`.
5. Paste `TreeShadow_Topdown` with F to cross the river / gap.
6. Cut a `CanPress` shadow such as `BeamShadow_Topdown`.
7. Paste it onto `PressurePlate_Topdown` to open the pressure plate door.
8. Cut `KeyShadow_Topdown`.
9. Paste it into `Lock_Topdown_Trigger` to open the locked door.
10. Hold Shift and press Q to cut the player's own shadow.
11. Paste the player shadow into `LureArea_Topdown` to distract `ShadowSeeker_Topdown`.
12. Reach `FinalClockCore_Topdown`.
13. Press E to start the clock and show `Topdown Demo Complete`.

## Scene Cleanup Notes

An inactive-object audit found 16 inactive scene objects in `ClockTower_TopdownPrototype`. None appeared to be required by the current Topdown v0.6 playable flow:

`Lantern -> TreeShadow crossing -> CanPress plate -> CanUnlock lock -> PlayerShadow lure -> FinalClockCore`

Those inactive objects are old visual, test, decorative, or path-guide objects. They are candidates for a future archive pass, preferably under `Assets/_Shadough/_Archive/TopdownUnusedSceneObjects/`, instead of immediate deletion.

## Project Structure

- `ShadoughUnity/Assets/_Shadough/Scenes`
- `ShadoughUnity/Assets/_Shadough/Scripts/Player`
- `ShadoughUnity/Assets/_Shadough/Scripts/Shadows`
- `ShadoughUnity/Assets/_Shadough/Scripts/Interactables`
- `ShadoughUnity/Assets/_Shadough/Scripts/Enemies`
- `ShadoughUnity/Assets/_Shadough/Scripts/Topdown`
- `ShadoughUnity/Assets/_Shadough/Scripts/UI`
- `ShadoughUnity/Assets/_Shadough/_Archive/SideScroller`

## Supporting Docs

- `策划.md`: full Chinese design document
- `VERSION_NOTES.md`: root-level version summary
- `ShadoughUnity/Assets/_Shadough/VERSION_NOTES.md`: Unity project-local version notes
- `ShadoughUnity/Assets/_Shadough/TOPDOWN_PROTOTYPE_CHECKLIST.md`: top-down prototype checklist

## Next Steps

- Archive unused inactive top-down scene objects if they are no longer needed for iteration history
- Convert top-down visual readability toward the Chinese pixel-art direction
- Refresh `素材库/完整素材清单.md` and use it as the production index for Chinese pixel-art replacement
- Replace graybox landmarks with readable Chinese pixel-art tiles, props, lanterns, shadow-puppet silhouettes, UI, VFX, and audio
- Add shadow dissipation system
- Add strong-light shadow destruction
- Add final audio and UI polish
