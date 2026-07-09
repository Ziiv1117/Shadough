using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class ShadowInteractable : MonoBehaviour
{
    [Header("Shadow")]
    [SerializeField] private ShadowType shadowType = ShadowType.None;
    [SerializeField] private string displayName;
    [SerializeField] private bool canBeCut = true;
    [SerializeField] private float respawnTime = 0f;

    [Header("Shadow Properties")]
    [SerializeField] private bool canStandOn = true;
    [SerializeField] private bool canPress;
    [SerializeField] private bool canUnlock;
    [SerializeField] private bool canAttractEnemy;
    [SerializeField] private bool canBlock;

    [Header("Legacy")]
    [Tooltip("Legacy compatibility only. New logic should prefer canPress / canUnlock / canAttractEnemy / canBlock.")]
    [SerializeField] private bool canTriggerMechanism;

    [Header("Visual")]
    [SerializeField] private Sprite pastedSprite;
    [SerializeField] private Sprite inventoryIcon;
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField, Range(0f, 1f)] private float cutAlpha = 0f;

    private Collider2D triggerCollider;
    private Color originalColor;
    private bool isCut;

    public ShadowType ShadowType => shadowType;
    public bool CanBeCut => canBeCut && !isCut;
    public float RespawnTime => respawnTime;
    public bool IsCut => isCut;
    public bool CanStandOn => canStandOn;
    public bool CanPress => canPress;
    public bool CanUnlock => canUnlock;
    public bool CanAttractEnemy => canAttractEnemy;
    public bool CanBlock => canBlock;
    public bool CanTriggerMechanism => canTriggerMechanism;

    private void Awake()
    {
        CacheComponents();
        originalColor = shadowRenderer.color;
    }

    private void Reset()
    {
        CacheComponents();
        SetTriggerCollider();
    }

    private void OnValidate()
    {
        CacheComponents();
        SetTriggerCollider();
        respawnTime = Mathf.Max(0f, respawnTime);
    }

    public bool Cut()
    {
        if (!CanBeCut)
        {
            return false;
        }

        isCut = true;
        SetInteractableVisible(false);

        if (respawnTime > 0f)
        {
            CancelInvoke(nameof(Restore));
            Invoke(nameof(Restore), respawnTime);
        }

        return true;
    }

    public ShadowItemData CreateItemData()
    {
        CacheComponents();

        Vector2 colliderSize = Vector2.one;
        Vector2 colliderOffset = Vector2.zero;
        if (triggerCollider is BoxCollider2D boxCollider)
        {
            colliderSize = boxCollider.size;
            colliderOffset = boxCollider.offset;
        }
        else if (triggerCollider != null)
        {
            colliderSize = triggerCollider.bounds.size;
        }

        Vector2 approximateSize = colliderSize;
        if (shadowRenderer != null)
        {
            Bounds bounds = shadowRenderer.bounds;
            approximateSize = new Vector2(bounds.size.x, bounds.size.y);
        }

        return new ShadowItemData
        {
            shadowType = shadowType,
            displayName = string.IsNullOrEmpty(displayName) ? name : displayName,
            sprite = shadowRenderer != null ? shadowRenderer.sprite : null,
            pastedSprite = pastedSprite,
            inventoryIcon = inventoryIcon,
            spriteDrawMode = shadowRenderer != null ? shadowRenderer.drawMode : SpriteDrawMode.Simple,
            spriteSize = shadowRenderer != null ? shadowRenderer.size : Vector2.one,
            localScale = transform.localScale,
            rotation = transform.rotation,
            approximateSize = approximateSize,
            colliderSize = colliderSize,
            colliderOffset = colliderOffset,
            canStandOn = canStandOn,
            canPress = canPress,
            canUnlock = canUnlock,
            canAttractEnemy = canAttractEnemy,
            canBlock = canBlock,
            canTriggerMechanism = canTriggerMechanism || canPress,
            sourceInteractable = this,
            returnsToPlayer = false,
            recallBlocked = false
        };
    }

    public void Restore()
    {
        isCut = false;
        SetInteractableVisible(true);
    }

    private void CacheComponents()
    {
        if (shadowRenderer == null)
        {
            shadowRenderer = GetComponent<SpriteRenderer>();
        }

        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }
    }

    private void SetTriggerCollider()
    {
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void SetInteractableVisible(bool visible)
    {
        if (triggerCollider != null)
        {
            triggerCollider.enabled = visible;
        }

        if (shadowRenderer == null)
        {
            return;
        }

        if (visible)
        {
            shadowRenderer.enabled = true;
            shadowRenderer.color = originalColor;
            return;
        }

        Color cutColor = originalColor;
        cutColor.a = cutAlpha;
        shadowRenderer.color = cutColor;
        shadowRenderer.enabled = cutAlpha > 0f;
    }
}

