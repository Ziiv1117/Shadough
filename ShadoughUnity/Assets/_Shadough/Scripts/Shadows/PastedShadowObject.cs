using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class PastedShadowObject : MonoBehaviour
{
    [SerializeField] private ShadowItemData sourceData;
    [SerializeField] private ShadowType shadowType = ShadowType.None;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D shapeCollider;
    [SerializeField] private bool canStandOn;
    [SerializeField] private bool canPress;
    [SerializeField] private bool canUnlock;
    [SerializeField] private bool canAttractEnemy;
    [SerializeField] private bool canBlock;
    [SerializeField] private bool recallBlocked;
    [SerializeField] private string recallBlockedMessage = "This shadow cannot be recalled now.";

    [Header("Legacy")]
    [Tooltip("Legacy compatibility only. New logic should prefer CanPress / CanUnlock / CanAttractEnemy / CanBlock.")]
    [SerializeField] private bool canTriggerMechanism;

    public ShadowItemData SourceData => sourceData;
    public ShadowType ShadowType => shadowType;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Collider2D ShapeCollider => shapeCollider;
    public bool CanStandOn => canStandOn;
    public bool CanPress => canPress;
    public bool CanUnlock => canUnlock;
    public bool CanAttractEnemy => canAttractEnemy;
    public bool CanBlock => canBlock;
    public bool CanTriggerMechanism => canTriggerMechanism;
    public bool RecallBlocked => recallBlocked || (sourceData != null && sourceData.recallBlocked);
    public string RecallBlockedMessage => !string.IsNullOrEmpty(recallBlockedMessage)
        ? recallBlockedMessage
        : "This shadow cannot be recalled now.";

    private void Reset()
    {
        CacheComponents();
    }

    private void OnValidate()
    {
        CacheComponents();
    }

    public void Initialize(ShadowItemData data)
    {
        CacheComponents();

        sourceData = data;
        shadowType = data.shadowType;
        canStandOn = data.canStandOn;
        canPress = data.canPress;
        canUnlock = data.canUnlock;
        canAttractEnemy = data.canAttractEnemy;
        canBlock = data.canBlock;
        canTriggerMechanism = data.canTriggerMechanism || data.canPress;
        recallBlocked = data.recallBlocked;
        recallBlockedMessage = string.IsNullOrEmpty(data.recallBlockedMessage)
            ? "This shadow cannot be recalled now."
            : data.recallBlockedMessage;

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = data.sprite;
            ApplyRendererShape(spriteRenderer, data);
            spriteRenderer.color = new Color(0f, 0f, 0f, 0.65f);
            spriteRenderer.sortingOrder = 20;
        }

        transform.localScale = data.localScale;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;

        if (shapeCollider is BoxCollider2D boxCollider)
        {
            boxCollider.size = data.colliderSize;
            boxCollider.offset = data.colliderOffset;
            boxCollider.isTrigger = !canStandOn;
        }
        else
        {
            shapeCollider.isTrigger = !canStandOn;
        }
    }

    public void BlockRecall(string message)
    {
        recallBlocked = true;
        recallBlockedMessage = string.IsNullOrEmpty(message)
            ? "This shadow cannot be recalled now."
            : message;

        if (sourceData != null)
        {
            sourceData.recallBlocked = true;
            sourceData.recallBlockedMessage = recallBlockedMessage;
        }
    }

    public ShadowItemData CreateItemData()
    {
        CacheComponents();

        Vector2 colliderSize = Vector2.one;
        Vector2 colliderOffset = Vector2.zero;
        if (shapeCollider is BoxCollider2D boxCollider)
        {
            colliderSize = boxCollider.size;
            colliderOffset = boxCollider.offset;
        }
        else if (shapeCollider != null)
        {
            colliderSize = shapeCollider.bounds.size;
        }

        Vector2 approximateSize = colliderSize;
        if (spriteRenderer != null)
        {
            Bounds bounds = spriteRenderer.bounds;
            approximateSize = new Vector2(bounds.size.x, bounds.size.y);
        }

        string itemName = sourceData != null && !string.IsNullOrEmpty(sourceData.displayName)
            ? sourceData.displayName
            : name;

        return new ShadowItemData
        {
            shadowType = shadowType,
            displayName = itemName,
            sprite = spriteRenderer != null ? spriteRenderer.sprite : null,
            spriteDrawMode = spriteRenderer != null ? spriteRenderer.drawMode : SpriteDrawMode.Simple,
            spriteSize = spriteRenderer != null ? spriteRenderer.size : Vector2.one,
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
            sourceInteractable = sourceData != null ? sourceData.sourceInteractable : null,
            returnsToPlayer = sourceData != null && sourceData.returnsToPlayer,
            recallBlocked = RecallBlocked,
            recallBlockedMessage = RecallBlockedMessage
        };
    }

    private void ApplyRendererShape(SpriteRenderer renderer, ShadowItemData data)
    {
        renderer.drawMode = data.spriteDrawMode;

        if (data.spriteDrawMode == SpriteDrawMode.Sliced || data.spriteDrawMode == SpriteDrawMode.Tiled)
        {
            Vector2 size = data.spriteSize;
            if (size.x <= 0f || size.y <= 0f)
            {
                size = data.colliderSize;
            }

            renderer.size = size;
        }
    }

    private void CacheComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (shapeCollider == null)
        {
            shapeCollider = GetComponent<Collider2D>();
        }
    }
}
