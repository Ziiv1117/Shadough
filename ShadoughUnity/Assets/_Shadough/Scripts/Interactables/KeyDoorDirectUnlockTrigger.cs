using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KeyDoorDirectUnlockTrigger : MonoBehaviour
{
    [SerializeField] private DoorController targetDoor;
    [SerializeField] private float failurePromptCooldown = 1.5f;

    private Collider2D triggerCollider;
    private PastedShadowObject lastRejectedShadow;
    private float nextFailurePromptTime;
    private bool unlocked;

    public DoorController TargetDoor => targetDoor;
    public bool IsUnlocked => unlocked;

    private void Awake()
    {
        CacheComponents();
        SetTriggerCollider();
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
        failurePromptCooldown = Mathf.Max(0.25f, failurePromptCooldown);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryUnlockFrom(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryUnlockFrom(other);
    }

    private void Update()
    {
        if (unlocked || targetDoor == null || triggerCollider == null)
        {
            return;
        }

        Bounds bounds = triggerCollider.bounds;
        Collider2D[] overlaps = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);
        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider2D overlap = overlaps[i];
            if (overlap == null || overlap == triggerCollider)
            {
                continue;
            }

            TryUnlockFrom(overlap);
            if (unlocked)
            {
                return;
            }
        }
    }

    public void ResetDoor()
    {
        unlocked = false;
        if (targetDoor != null)
        {
            targetDoor.Close();
        }
    }

    private void TryUnlockFrom(Collider2D other)
    {
        if (unlocked || targetDoor == null || other == null)
        {
            return;
        }

        PastedShadowObject pastedShadow = GetPastedShadow(other);
        if (pastedShadow == null)
        {
            return;
        }

        if (!pastedShadow.CanUnlock)
        {
            ShowFailurePrompt(pastedShadow);
            return;
        }

        pastedShadow.BlockRecall("This shadow cannot be recalled after unlocking this door.");
        unlocked = true;
        targetDoor.Open();
    }

    private PastedShadowObject GetPastedShadow(Collider2D other)
    {
        PastedShadowObject pastedShadow = other.GetComponent<PastedShadowObject>();
        if (pastedShadow == null)
        {
            pastedShadow = other.GetComponentInParent<PastedShadowObject>();
        }

        return pastedShadow;
    }

    private void ShowFailurePrompt(PastedShadowObject pastedShadow)
    {
        if (pastedShadow == lastRejectedShadow || Time.time < nextFailurePromptTime)
        {
            return;
        }

        lastRejectedShadow = pastedShadow;
        nextFailurePromptTime = Time.time + failurePromptCooldown;
        TutorialFailurePromptController.Show("This shadow cannot unlock this door.");
    }

    private void CacheComponents()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }

        if (targetDoor == null)
        {
            targetDoor = GetComponentInParent<DoorController>();
        }
    }

    private void SetTriggerCollider()
    {
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
}
