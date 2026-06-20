# Version Notes

## v0.3 Lantern Driven TreeShadow

Date: 2026-06-20

Status: Playable target with handheld lantern gating the first TreeShadow cut.

## Completed

- Player-held lantern
- G to plant or retrieve the lantern
- TreeShadow changes length and direction based on lantern position
- TreeShadow requires the lantern to be planted before it can be cut
- Pasted TreeShadow no longer responds to the lantern
- v0.2 Reveal Cut Rule remains active
- Lantern is currently connected only to TreeShadow, not to KeyShadow, PlayerShadow, or pasted shadows

## Not Changed

- Pasting still does not require Reveal View
- FinalClockCore E interaction is not blocked by Reveal View or lantern state
- Pressure plates still use `CanPress`
- Locks still use `CanUnlock`
- Shadow seeker still uses `CanAttractEnemy`
- `ShadowType` remains a source label, not a mechanism rule

## Known Issues

- No blocking issues found by static inspection. Unity Play Mode verification is still required.

## v0.2 Reveal Cut Rule

Date: 2026-06-20

Status: Playable from the start area to Demo Complete with Reveal View gated cutting.

## Completed

- Basic Reveal View
- Cuttable shadows become clearer while Reveal View is active
- Scene shadows can only be cut while Reveal View is active
- The player's own shadow can only be cut while Reveal View is active
- Pasting shadows does not require Reveal View
- FinalClockCore E interaction is not blocked by Reveal View

## Not Changed

- Pressure plates still use `CanPress`
- Locks still use `CanUnlock`
- Shadow seeker still uses `CanAttractEnemy`
- `ShadowType` remains a source label, not a mechanism rule

## Known Issues

- No blocking issues found in current graybox demo.

## v0.1 Graybox Demo

Date: 2026-06-20

Status: Playable from the start area to Demo Complete.

## Completed

- Player movement and jumping
- E to cut scene shadows
- F to freely paste shadows
- Q to cut the player's own shadow
- Shadow data properties for gameplay behavior
- TreeShadow gap crossing through `CanStandOn`
- Pressure plate door using `CanPress`
- Key shadow lock door using `CanUnlock`
- Player shadow lure using `CanAttractEnemy`
- Shadow seeker that follows attractable pasted shadows
- FinalClockCore demo victory flow
- Basic interaction / victory UI prompts
- Demo test checklist

## Not Completed

- Final art
- Final audio
- Light sources changing shadow form
- Shadow dissipation
- More complex levels

## Known Issues

- No blocking issues found in current graybox demo.
