using UnityEngine;

[System.Serializable]
public class ShadowItemData
{
    public ShadowType shadowType = ShadowType.None;
    public string displayName;
    public Sprite sprite;
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

    public bool IsValid()
    {
        return shadowType != ShadowType.None;
    }
}
