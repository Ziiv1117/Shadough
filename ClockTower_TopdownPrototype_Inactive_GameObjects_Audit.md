# ClockTower_TopdownPrototype Inactive GameObjects Audit

Scope: `Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity`

Rules followed:

- Scene was only read for audit.
- No GameObject was deleted.
- No GameObject was moved.
- No scene object state was changed.

Audit method:

- Searched all scene `GameObject` blocks with `m_IsActive: 0`.
- Resolved hierarchy path from Transform parent links.
- Listed attached components from scene YAML.
- Checked for scene-level reference signals. In the notes below, "No script reference found" means no serialized MonoBehaviour field in the scene appears to rely on the object for the Topdown v0.6 flow. Normal hierarchy parent/child references and self component references are not treated as gameplay references.

## Summary

Inactive GameObjects found: 16

- Must Keep: 0
- Safe to Archive/Delete: 16
- Unsure: 0

No inactive object appears to be required by the current Topdown v0.6 playable flow:

Lantern -> TreeShadow crossing -> CanPress plate -> CanUnlock lock -> PlayerShadow lure -> FinalClockCore_Topdown.

## 1. Must Keep

None found among inactive GameObjects.

Reason:

- Current required Topdown objects such as `CrossingBlocker_01`, doors, triggers, pressure plate, lock trigger, seeker, lure area, UI, and `FinalClockCore_Topdown` are active in the scene.
- No inactive object was identified as a runtime-enabled blocker, trigger, door, goal, clock core, enemy, or UI object for the v0.6 loop.

## 2. Safe to Archive/Delete

These are inactive and do not appear to participate in the current Topdown v0.6 flow.

### Pillar_01

- Hierarchy path: `World/Pillar_01`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`, `BoxCollider2D`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old topdown visual/obstacle test object. Inactive. Not part of current route or puzzle flow.

### Box_01

- Hierarchy path: `World/Box_01`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`, `BoxCollider2D`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old topdown visual/obstacle test object. Inactive. Not part of current route or puzzle flow.

### Bench_01

- Hierarchy path: `World/Bench_01`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`, `BoxCollider2D`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old topdown visual/obstacle test object. Inactive. Not part of current route or puzzle flow.

### Stone_01

- Hierarchy path: `World/Stone_01`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`, `BoxCollider2D`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old topdown visual/obstacle test object. Inactive. Not part of current route or puzzle flow.

### Tree_Shadow

- Hierarchy path: `World/Tree_01/Tree_Shadow`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old tree visual shadow child. Current functional shadow is `ShadowLogic/TreeShadow_Topdown`, so this inactive visual child is not used.

### PlayerShadow_Visual_Test

- Hierarchy path: `ShadowVisuals/PlayerShadow_Visual_Test`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old visual test object. Current player shadow is created at runtime from `PlayerSelfShadowCutter` and pasted through `FreeShadowPlacer`.

### PastedShadow_Visual_Test

- Hierarchy path: `ShadowVisuals/PastedShadow_Visual_Test`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old visual test object. Current pasted shadows are runtime `PastedShadowObject` instances.

### Ground_Details

- Hierarchy path: `Ground/Ground_Details`
- Active: `false`
- Components: `Transform`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old decorative/detail group. Inactive and not part of current clean area-block layout.

### River_Stone_01

- Hierarchy path: `Ground/River_Gap_01/River_Stone_01`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old river decoration. Inactive. Current first puzzle uses `River_Gap_01`, `CrossingHint_01`, `CrossingBlocker_01`, and `TreeShadow_Topdown`.

### River_Stone_02

- Hierarchy path: `Ground/River_Gap_01/River_Stone_02`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old river decoration. Inactive. Not part of current bridge crossing logic.

### River_Stone_03

- Hierarchy path: `Ground/River_Gap_01/River_Stone_03`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Old river decoration. Inactive. Not part of current bridge crossing logic.

### Path_01_StartToRiver

- Hierarchy path: `Ground/Path_01_StartToRiver`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Deprecated Path visual guide. User requested no path. Current layout uses same-sized connected area blocks instead.

### Path_02_RiverToPressure

- Hierarchy path: `Ground/Path_02_RiverToPressure`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Deprecated Path visual guide. Not part of current v0.6 flow.

### Path_03_PressureToLock

- Hierarchy path: `Ground/Path_03_PressureToLock`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Deprecated Path visual guide. Not part of current v0.6 flow.

### Path_04_LockToEnemy

- Hierarchy path: `Ground/Path_04_LockToEnemy`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Deprecated Path visual guide. Not part of current v0.6 flow.

### Path_05_EnemyToGoal

- Hierarchy path: `Ground/Path_05_EnemyToGoal`
- Active: `false`
- Components: `Transform`, `SpriteRenderer`
- Referenced by other objects: Only normal hierarchy/component references found. No script reference found.
- Classification reason: Deprecated Path visual guide. Not part of current v0.6 flow.

## 3. Unsure

None.

Reason:

- Every inactive object has a clear old visual/test/decorative/path role.
- None has a serialized gameplay script component.
- None appears to be referenced by Topdown v0.6 gameplay scripts as a target, trigger, blocker, door, clock core, enemy, UI object, or runtime activation object.

## Recommendation

For a future cleanup pass, the objects in "Safe to Archive/Delete" can be archived together under something like:

`Assets/_Shadough/_Archive/TopdownUnusedSceneObjects/`

Do not delete immediately if you want to preserve visual iteration history. Archiving is safer than deletion.
