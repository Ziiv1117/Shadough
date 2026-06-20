using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PressurePlateController : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool isPressed;
    [SerializeField] private DoorController targetDoor;

    [Header("Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color releasedColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color pressedColor = new Color(0.2f, 0.85f, 0.45f, 1f);

    public bool IsPressed => isPressed;
    public DoorController TargetDoor => targetDoor;

    private void Awake()
    {
        CacheComponents();
        ApplyPressedState();
    }

    private void Reset()
    {
        CacheComponents();
    }

    private void OnValidate()
    {
        CacheComponents();
        ApplyPressedState();
    }

    public void Activate()
    {
        SetPressed(true);
    }

    public void Deactivate()
    {
        SetPressed(false);
    }

    public void SetPressed(bool pressed)
    {
        if (isPressed == pressed)
        {
            return;
        }

        isPressed = pressed;
        ApplyPressedState();
    }

    private void ApplyPressedState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isPressed ? pressedColor : releasedColor;
        }

        if (targetDoor == null)
        {
            return;
        }

        if (isPressed)
        {
            targetDoor.Open();
        }
        else
        {
            targetDoor.Close();
        }
    }

    private void CacheComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
