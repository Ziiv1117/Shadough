# Version Notes

## Art Direction Update: Chinese Pixel Shadow Style

Date: 2026-07-06

Status: Design direction update only; no gameplay, scene, or script change in this note.

## Completed

- The long-term art direction is now Chinese pixel art with shadow-puppet, paper-cut, lantern, old-town, and ink-shadow influences.
- The current top-down graybox remains the playable foundation.
- `ClockTower_TopdownPrototype` should be visually reinterpreted as a Chinese bell / drum tower style space during future art passes.
- Future art should prioritize readable pixel silhouettes for cuttable and pasted shadows.

## Design Rules

- Do not change the core rule that pasted shadows preserve the shape captured when cut.
- Do not make mechanisms depend directly on `ShadowType` during the art pass.
- Keep gameplay shadows legible at top-down camera distance.
- Use Chinese pixel-art props and tile language to clarify routes, doors, locks, pressure plates, lanterns, shadow creatures, and the final clock core.
- Avoid western gothic, pure horror, and realistic ink-painting direction unless a later design note explicitly changes the target.

## Not Changed

- The current playable flow remains:
  `Lantern -> TreeShadow crossing -> CanPress plate -> CanUnlock lock -> PlayerShadow lure -> FinalClockCore`.
- Visual shadows and gameplay shadows remain separate.
- Unity Play Mode verification is still required for release validation.

## Topdown v0.6 Demo Loop

Date: 2026-06-20

Status: Current main playable demo loop.

## Completed

- `ClockTower_TopdownPrototype` is the current main playable scene.
- The simple `Goal_Marker` completion flow was replaced by `FinalClockCore_Topdown`.
- The player must reach the final clock core and press E to complete the demo.
- The top-down demo now has a complete playable loop:
  `Lantern -> TreeShadow crossing -> CanPress plate -> CanUnlock lock -> PlayerShadow lure -> FinalClockCore`.
- `Topdown Demo Complete` is shown when the final clock is started.

## Not Changed

- Cutting scene shadows still requires Reveal View.
- Cutting the player's own shadow still requires Reveal View.
- Pasting still does not require Reveal View.
- `FinalClockCore_Topdown` E interaction is not blocked by Reveal View or lantern state.
- Puzzle mechanisms still read shadow properties instead of hard-coding `ShadowType`.

## Known Issues

- No blocking issue is recorded in the static notes.
- Unity Play Mode verification is still required when validating a release build.

## Topdown Build Cleanup

Date: 2026-06-20

Status: Build scene cleanup completed by file-level verification.

## Completed

- Build Settings now include only `Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity`.
- Old side-scroller demo scene and side-scroller-only assets were moved to `Assets/_Shadough/_Archive/SideScroller/`.
- Shared shadow system scripts were preserved because the top-down scene still depends on them.
- File-level validation found no broken local scene fileID references and no missing-script `m_Script: {fileID: 0}` entries in `ClockTower_TopdownPrototype.unity`.

## Known Issues

- This cleanup was structurally verified from files; it is not the same as a full Unity Play Mode test.

## Topdown Inactive GameObjects Audit

Date: 2026-06-21

Status: Audit-only cleanup recommendation.

## Completed

- Audited inactive GameObjects in `ClockTower_TopdownPrototype`.
- Found 16 inactive objects.
- Classified all 16 as safe to archive/delete from the current Topdown v0.6 playable flow.
- No inactive object was classified as Must Keep or Unsure.
- No scene object was deleted, moved, or changed by the audit.

## Recommendation

- Prefer archiving these objects under `Assets/_Shadough/_Archive/TopdownUnusedSceneObjects/` before deleting them.
- Treat old visual, decorative, test, and path-guide objects as iteration history unless a cleanup pass explicitly decides otherwise.

## Topdown v0.5 Core Properties Prototype

Date: 2026-06-20

Status: Playable top-down graybox prototype.

## Completed

- WASD top-down movement.
- Player-held lantern.
- G to plant or retrieve the lantern.
- Lantern-driven `TreeShadow_Topdown`.
- `TreeShadow_Topdown` requires the lantern to be planted before it can be cut.
- `CanStandOn` / `CanTraverse` tree shadow crossing.
- `CanPress` pressure plate door.
- `CanUnlock` key shadow lock.
- `CanAttractEnemy` player shadow lure.
- Prototype completion feedback.

## Design Rule

`ShadowType` is source metadata only. Puzzle logic reads `PastedShadowObject` properties:
`CanStandOn`, `CanPress`, `CanUnlock`, `CanAttractEnemy`, and `CanBlock`.

## Not Changed

- Pasted shadows do not continue to respond to lantern movement.
- `TreeShadow_Topdown` is the only lantern-driven shadow in the top-down prototype.
- Key shadows, player shadows, and pasted shadows are not driven by the lantern.

## v0.3 Lantern Driven TreeShadow

Date: 2026-06-20

Status: Earlier side-view / shared-system milestone.

## Completed

- Player-held lantern.
- G to plant or retrieve the lantern.
- TreeShadow changes length and direction based on lantern position.
- TreeShadow requires the lantern to be planted before it can be cut.
- Pasted TreeShadow no longer responds to the lantern.
- v0.2 Reveal Cut Rule remains active.
- Lantern is connected only to TreeShadow, not to KeyShadow, PlayerShadow, or pasted shadows.

## v0.2 Reveal Cut Rule

Date: 2026-06-20

Status: Earlier playable milestone with Reveal View gated cutting.

## Completed

- Basic Reveal View.
- Cuttable shadows become clearer while Reveal View is active.
- Scene shadows can only be cut while Reveal View is active.
- The player's own shadow can only be cut while Reveal View is active.
- Pasting shadows does not require Reveal View.
- FinalClockCore E interaction is not blocked by Reveal View.

## v0.1 Graybox Demo

Date: 2026-06-20

Status: Earlier graybox demo foundation.

## Completed

- Player movement and jumping.
- E to cut scene shadows.
- F to freely paste shadows.
- Q to cut the player's own shadow.
- Shadow data properties for gameplay behavior.
- TreeShadow gap crossing through `CanStandOn`.
- Pressure plate door using `CanPress`.
- Key shadow lock door using `CanUnlock`.
- Player shadow lure using `CanAttractEnemy`.
- Shadow seeker that follows attractable pasted shadows.
- FinalClockCore demo victory flow.
- Basic interaction / victory UI prompts.
