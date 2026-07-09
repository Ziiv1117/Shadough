using UnityEngine;

[RequireComponent(typeof(PressurePlateController))]
[RequireComponent(typeof(SpriteRenderer))]
public class PressurePlateSpriteState : MonoBehaviour
{
    [SerializeField] private PressurePlateController pressurePlate;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite releasedSprite;
    [SerializeField] private Sprite pressedSprite;

    private bool lastPressed;
    private bool initialized;

    private void Awake()
    {
        CacheComponents();
        ApplyState();
    }

    private void OnValidate()
    {
        CacheComponents();
        if (!Application.isPlaying)
        {
            ApplyState();
        }
    }

    private void Update()
    {
        if (pressurePlate == null || spriteRenderer == null)
        {
            return;
        }

        bool pressed = pressurePlate.IsPressed;
        if (!initialized || pressed != lastPressed)
        {
            ApplyState();
        }
    }

    private void ApplyState()
    {
        if (pressurePlate == null || spriteRenderer == null)
        {
            return;
        }

        lastPressed = pressurePlate.IsPressed;
        initialized = true;
        Sprite targetSprite = lastPressed ? pressedSprite : releasedSprite;
        if (targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
        }

        spriteRenderer.color = Color.white;
    }

    private void CacheComponents()
    {
        if (pressurePlate == null)
        {
            pressurePlate = GetComponent<PressurePlateController>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
