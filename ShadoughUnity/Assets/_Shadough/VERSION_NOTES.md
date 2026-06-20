# Version Notes

## Topdown v0.5 Core Properties Prototype

Status: playable topdown graybox prototype.

Implemented:

* Topdown movement.
* Lantern-driven TreeShadow_Topdown.
* CanStandOn / CanTraverse tree shadow crossing.
* CanPress pressure plate door.
* CanUnlock key shadow lock.
* CanAttractEnemy player shadow lure.
* Prototype Complete goal feedback.

Design rule:

ShadowType is source metadata only. Puzzle logic reads PastedShadowObject properties:
CanStandOn, CanPress, CanUnlock, CanAttractEnemy, and CanBlock.

Notes:

* TreeShadow_Topdown is the only lantern-driven shadow in the topdown prototype.
* Pasted shadows do not continue to respond to lantern movement.
* ClockTower_Demo remains the old side-view demo and is not part of this topdown prototype pass.

## Topdown v0.6 Demo Loop

* Replaced simple Goal_Marker completion with FinalClockCore_Topdown.
* Player must reach the final clock core and press E to complete the demo.
* Topdown demo now has a complete playable loop:
  Lantern -> TreeShadow crossing -> CanPress plate -> CanUnlock lock -> PlayerShadow lure -> FinalClockCore.

## Cleanup before Topdown Build

* ClockTower_TopdownPrototype is now the main playable demo scene.
* Old side-scroller demo scene and side-scroller-only assets were moved to Assets/_Shadough/_Archive/SideScroller/.
* Shared shadow system scripts were preserved.
* Build Settings now includes only ClockTower_TopdownPrototype.
