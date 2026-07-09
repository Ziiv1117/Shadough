using UnityEngine;

[RequireComponent(typeof(DoorController))]
[RequireComponent(typeof(SpriteRenderer))]
public class DoorStateSpriteAnimator : MonoBehaviour
{
    [SerializeField] private DoorController doorController;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite halfOpenSprite;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private Sprite wideOpenSprite;
    [SerializeField] private float frameDuration = 0.12f;

    private bool lastOpen;
    private bool initialized;
    private float animationStartTime;
    private bool animating;

    private void Awake()
    {
        CacheComponents();
        initialized = true;
        lastOpen = doorController != null && doorController.IsOpen;
        ApplyImmediate(lastOpen);
    }

    private void OnValidate()
    {
        frameDuration = Mathf.Max(0.01f, frameDuration);
        CacheComponents();
        if (!Application.isPlaying && doorController != null)
        {
            ApplyImmediate(doorController.IsOpen);
        }
    }

    private void Update()
    {
        if (doorController == null || spriteRenderer == null)
        {
            return;
        }

        bool open = doorController.IsOpen;
        if (!initialized || open != lastOpen)
        {
            initialized = true;
            lastOpen = open;
            animationStartTime = Time.time;
            animating = true;
        }

        if (animating)
        {
            ApplyAnimated(open);
        }
    }

    private void ApplyAnimated(bool open)
    {
        float elapsed = Time.time - animationStartTime;
        int frame = Mathf.FloorToInt(elapsed / frameDuration);

        if (open)
        {
            if (frame <= 0)
            {
                ApplySprite(closedSprite);
            }
            else if (frame == 1)
            {
                ApplySprite(halfOpenSprite);
            }
            else if (frame == 2)
            {
                ApplySprite(openSprite);
            }
            else
            {
                ApplySprite(wideOpenSprite);
                animating = false;
            }
        }
        else
        {
            if (frame <= 0)
            {
                ApplySprite(wideOpenSprite);
            }
            else if (frame == 1)
            {
                ApplySprite(openSprite);
            }
            else if (frame == 2)
            {
                ApplySprite(halfOpenSprite);
            }
            else
            {
                ApplySprite(closedSprite);
                animating = false;
            }
        }
    }

    private void ApplyImmediate(bool open)
    {
        ApplySprite(open ? wideOpenSprite : closedSprite);
        animating = false;
    }

    private void ApplySprite(Sprite sprite)
    {
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
        }
    }

    private void CacheComponents()
    {
        if (doorController == null)
        {
            doorController = GetComponent<DoorController>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
