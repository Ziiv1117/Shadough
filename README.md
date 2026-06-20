# Shadough / 《剪影师》

Shadough / 《剪影师》 is a Unity 2D puzzle demo about cutting and pasting shadows to change the playable scene.

The player cuts shadows from the world, keeps their original silhouette shape, and pastes them back into the level. Mechanisms respond to pasted shadow properties and collider contact, not to hard-coded shadow source types.

## Current Demo Features

- Player movement and jumping
- Basic Reveal View for reading shadow objects
- Player-held lantern
- G to plant or retrieve the lantern
- Lantern-driven TreeShadow length and direction
- E to cut scene shadows while Reveal View is active
- F to freely paste the carried shadow
- Q to cut the player's own shadow while Reveal View is active
- TreeShadow gap crossing
- CanPress shadow pressure plate door
- CanUnlock key shadow lock door
- CanAttractEnemy player shadow lure for the shadow seeker
- FinalClockCore demo victory flow
- Basic interaction / victory UI prompts

## Core Design Principles

- `ShadowType` only represents the shadow source label.
- Mechanisms should not decide functionality directly from `shadowType`.
- Mechanisms detect shadow properties such as `CanStandOn`, `CanPress`, `CanUnlock`, and `CanAttractEnemy`.
- Cutting scene shadows requires Reveal View.
- Cutting the player's own shadow requires Reveal View.
- Pasting shadows does not require Reveal View.
- FinalClockCore E interaction does not require Reveal View.
- The player starts with a handheld lantern.
- The lantern only affects uncut cuttable shadows.
- In v0.3, the lantern only drives `TreeShadow`.
- The lantern does not affect carried `ShadowItemData`.
- The lantern does not affect pasted `PastedShadowObject` instances.
- `TreeShadow` must be cut while Reveal View is active and the lantern is planted.
- Once cut, `ShadowItemData` stores the current TreeShadow shape, scale, rotation, collider size, and collider offset.
- Pasted `TreeShadow` no longer responds to the lantern.
- KeyShadow and PlayerShadow are not driven by the lantern in v0.3.
- Pasted shadows preserve the shape captured when they were cut.
- Visual shadows and gameplay shadows are separated.

## Controls

- A / D: Move
- Space: Jump
- Hold Shift: Reveal Shadows
- E while Revealing: Cut Shadow
- Q while Revealing: Cut Self Shadow
- F: Paste Shadow
- G: Plant / Retrieve Lantern
- E near Clock: Start Clock

## Demo Walkthrough

1. Move near the tree and watch the handheld lantern change `TreeShadow` length and direction.
2. Press G to plant the lantern, then hold Shift and press E to cut the stable `TreeShadow`.
3. Paste `TreeShadow` with F to cross the first gap.
4. Use a press-capable shadow to open the pressure plate door.
5. Use the key shadow to unlock the lock door.
6. Hold Shift to reveal the player's own shadow, then cut and paste it to distract the shadow seeker.
7. Reach the final clock core and press E to start the clock without requiring Reveal View.

## Project Structure

- `ShadoughUnity/Assets/_Shadough/Scenes`
- `ShadoughUnity/Assets/_Shadough/Scripts/Player`
- `ShadoughUnity/Assets/_Shadough/Scripts/Shadows`
- `ShadoughUnity/Assets/_Shadough/Scripts/Interactables`
- `ShadoughUnity/Assets/_Shadough/Scripts/Enemies`
- `ShadoughUnity/Assets/_Shadough/Scripts/UI`

## Next Steps

- Shadow dissipation system
- Strong light destroying shadows
- Final art and audio
- Level pacing polish
