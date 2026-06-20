using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class DoorController : MonoBehaviour
{
    [SerializeField] private bool isOpen;
    [SerializeField] private Collider2D doorCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float openAlpha = 0.28f;
    [SerializeField] private Color closedColor = Color.white;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        CacheComponents();
        if (spriteRenderer != null && closedColor.a <= 0f)
        {
            closedColor = spriteRenderer.color;
        }

        ApplyOpenState();
    }

    private void Reset()
    {
        CacheComponents();
    }

    private void OnValidate()
    {
        CacheComponents();
        openAlpha = Mathf.Clamp01(openAlpha);
        if (spriteRenderer != null && !isOpen)
        {
            closedColor = spriteRenderer.color;
        }

        ApplyOpenState();
    }

    public void Open()
    {
        SetOpen(true);
    }

    public void Close()
    {
        SetOpen(false);
    }

    public void SetOpen(bool open)
    {
        isOpen = open;
        ApplyOpenState();
    }

    private void ApplyOpenState()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = !isOpen;
        }

        if (spriteRenderer == null)
        {
            return;
        }

        Color color = closedColor;
        if (isOpen)
        {
            color.a = openAlpha;
        }

        spriteRenderer.color = color;
    }

    private void CacheComponents()
    {
        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider2D>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
