# Shadough Demo Test Checklist

## Controls

- A / D: Move
- Space: Jump
- Hold Shift: Reveal Shadows
- E while Revealing: Cut Shadow
- Q while Revealing: Cut Self Shadow
- F: Paste Shadow
- G: Plant / Retrieve Lantern
- E near Clock: Start Clock

## Full Walkthrough

1. Start from the player spawn.
2. Confirm the lantern follows the player.
3. Move around `Tree` and confirm `TreeShadow` length and direction change.
4. Hold Shift and press E near `TreeShadow` before planting the lantern; confirm it does not cut and shows `Plant the lantern first`.
5. Press G to plant the lantern.
6. Confirm `TreeShadow` stays stable while the player moves away.
7. Hold Shift and cut `TreeShadow` with E while the lantern is planted.
8. Paste `TreeShadow` with F to cross the first gap.
9. Cut or use a `CanPress` shadow while Reveal View is active and place it on `PressurePlate_01`.
10. Confirm `Door_Pressure` opens while the pressure plate is held.
11. Cut `KeyShadow` while Reveal View is active.
12. Paste `KeyShadow` into `Lock_01_Trigger`.
13. Confirm `Door_Lock` opens.
14. Press Q while Reveal View is active and the inventory is empty to cut `Player Shadow`.
15. Paste the player shadow near `LureArea_01`.
16. Confirm `ShadowSeeker_01` moves toward the pasted player shadow.
17. Pass through the enemy corridor.
18. Reach `FinalClockCore`.
19. Press E when `Press E to Start Clock` appears.
20. Confirm `Demo Complete` is displayed.

## Puzzle Test Points

### Reveal View Cut Rule

- In normal view, E should not cut `TreeShadow`.
- In normal view, Q should not cut the player's own shadow.
- While Reveal View is active, E can cut normal cuttable shadows.
- `TreeShadow` additionally requires the lantern to be planted before E can cut it.
- While Reveal View is active, Q can cut the player's own shadow when the inventory is empty.
- F can paste the carried shadow without Reveal View.
- FinalClockCore can be started with E without Reveal View.
- The full demo route remains completable with the Reveal View cut rule.

### TreeShadow Gap

- `TreeShadow` has `canStandOn = true`.
- The handheld lantern follows the player by default.
- G plants the lantern in world space.
- G retrieves the planted lantern when the player is within retrieve range.
- G does not retrieve the lantern from too far away and should prompt the player to move closer.
- `TreeShadow` changes length and direction as the lantern position changes.
- E cannot cut `TreeShadow` before the lantern is planted, even while Reveal View is active.
- E cuts `TreeShadow` only while Reveal View is active and the lantern is planted.
- Cutting `TreeShadow` stores the current shape in `ShadowItemData`.
- Pasted `TreeShadow` has a non-trigger collider.
- The player can stand on the pasted shadow.
- Pasted `TreeShadow` does not respond to later lantern movement.
- Lantern-driven shadow logic currently affects `TreeShadow` only, not `KeyShadow` or `PlayerShadow`.

### Pressure Plate

- `ShadowPressureTrigger` checks `PastedShadowObject.CanPress`.
- A pasted shadow with `CanPress = true` opens the pressure door.
- A pasted shadow with `CanPress = false` does not open the pressure door.

### Lock

- `ShadowLockTrigger` checks `PastedShadowObject.CanUnlock`.
- `KeyShadow` creates a pasted shadow with `CanUnlock = true`.
- Shadows with `CanUnlock = false` do not open the lock door.

### Player Shadow

- Q only works when the inventory is empty.
- Q only creates player shadow while Reveal View is active.
- Player shadow data is:
  - `canStandOn = false`
  - `canPress = false`
  - `canUnlock = false`
  - `canAttractEnemy = true`
  - `canBlock = false`

### Shadow Seeker

- `EnemyShadowSeeker` checks `PastedShadowObject.CanAttractEnemy`.
- It does not check `ShadowType`.
- It should ignore `TreeShadow` and `KeyShadow` when `CanAttractEnemy = false`.
- It should move toward pasted player shadow when `CanAttractEnemy = true`.

### Final Clock

- `FinalClockCore` is a trigger.
- It does not depend on any shadow type or shadow ability.
- It displays `Press E to Start Clock` when the player is in range.
- E activates the clock once and shows Demo Complete.

## Known Issues

- No blocking issues found in current graybox demo.
- This checklist is file- and configuration-based unless verified in Unity Play Mode.

## Next Suggestions

- Replace placeholder grayboxes with final art after the gameplay flow is locked.
- Add lightweight camera framing polish after the full route is Play Mode verified.
