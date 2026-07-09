using UnityEngine;

[System.Serializable]
public class ShadowItemData
{
    public ShadowType shadowType = ShadowType.None;
    public string displayName;
    public Sprite sprite;
    public Sprite pastedSprite;
    public Sprite inventoryIcon;
    public SpriteDrawMode spriteDrawMode = SpriteDrawMode.Simple;
    public Vector2 spriteSize = Vector2.one;
    public Vector3 localScale = Vector3.one;
    public Quaternion rotation = Quaternion.identity;
    public Vector2 approximateSize = Vector2.one;
    public Vector2 colliderSize = Vector2.one;
    public Vector2 colliderOffset = Vector2.zero;
    public bool canStandOn;

    public bool canPress;
    public bool canUnlock;
    public bool canAttractEnemy;
    public bool canBlock;

    // Legacy compatibility. New logic should prefer canPress / canUnlock / canAttractEnemy / canBlock.
    public bool canTriggerMechanism;

    [Header("Recall Source")]
    public ShadowInteractable sourceInteractable;
    public bool returnsToPlayer;
    public bool recallBlocked;
    public string recallBlockedMessage = "This shadow cannot be recalled now.";

    public bool IsValid()
    {
        return shadowType != ShadowType.None;
    }

    public bool HasRecallSource()
    {
        return returnsToPlayer || sourceInteractable != null;
    }

    public bool TryRestoreSource()
    {
        if (returnsToPlayer)
        {
            return true;
        }

        if (sourceInteractable == null)
        {
            return false;
        }

        sourceInteractable.Restore();
        return true;
    }
}

