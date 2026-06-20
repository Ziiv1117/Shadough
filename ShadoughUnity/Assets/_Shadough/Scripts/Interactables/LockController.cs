using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class LockController : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool isUnlocked;
    [SerializeField] private DoorController targetDoor;

    [Header("Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color lockedColor = new Color(0.55f, 0.48f, 0.25f, 1f);
    [SerializeField] private Color unlockedColor = new Color(0.25f, 0.85f, 0.55f, 0.55f);

    public bool IsUnlocked => isUnlocked;
    public DoorController TargetDoor => targetDoor;

    private void Awake()
    {
        CacheComponents();
        ApplyUnlockedState();
    }

    private void Reset()
    {
        CacheComponents();
    }

    private void OnValidate()
    {
        CacheComponents();
        ApplyUnlockedState();
    }

    public void Unlock()
    {
        SetUnlocked(true);
    }

    public void Lock()
    {
        SetUnlocked(false);
    }

    public void SetUnlocked(bool unlocked)
    {
        if (isUnlocked == unlocked)
        {
            return;
        }

        isUnlocked = unlocked;
        ApplyUnlockedState();
    }

    private void ApplyUnlockedState()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isUnlocked ? unlockedColor : lockedColor;
        }

        if (targetDoor == null)
        {
            return;
        }

        if (isUnlocked)
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
