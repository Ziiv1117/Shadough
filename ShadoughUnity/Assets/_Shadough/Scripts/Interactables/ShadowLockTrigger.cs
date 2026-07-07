using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShadowLockTrigger : MonoBehaviour
{
    [SerializeField] private LockController lockController;
    [SerializeField] private bool requireAngleCheck;
    [SerializeField] private float allowedAngleDifference = 20f;
    [SerializeField] private float failurePromptCooldown = 1.5f;

    private Collider2D triggerCollider;
    private PastedShadowObject lastRejectedShadow;
    private float nextFailurePromptTime;

    public bool RequireAngleCheck => requireAngleCheck;
    public float AllowedAngleDifference => allowedAngleDifference;

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
        allowedAngleDifference = Mathf.Max(0f, allowedAngleDifference);
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
        if (lockController == null || lockController.IsUnlocked || triggerCollider == null)
        {
            return;
        }

        Bounds bounds = triggerCollider.bounds;
        Collider2D[] overlaps = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);
        foreach (Collider2D overlap in overlaps)
        {
            if (overlap == null || overlap == triggerCollider)
            {
                continue;
            }

            TryUnlockFrom(overlap);
            if (lockController.IsUnlocked)
            {
                return;
            }
        }
    }

    private void TryUnlockFrom(Collider2D other)
    {
        if (lockController == null || lockController.IsUnlocked || other == null)
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
            ShowFailurePrompt(pastedShadow, "This shadow cannot unlock this door.");
            return;
        }

        if (requireAngleCheck && !IsAngleAccepted(pastedShadow))
        {
            return;
        }

        lockController.Unlock();
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

    private void ShowFailurePrompt(PastedShadowObject pastedShadow, string message)
    {
        if (pastedShadow == lastRejectedShadow || Time.time < nextFailurePromptTime)
        {
            return;
        }

        lastRejectedShadow = pastedShadow;
        nextFailurePromptTime = Time.time + failurePromptCooldown;
        TutorialFailurePromptController.Show(message);
    }

    private bool IsAngleAccepted(PastedShadowObject pastedShadow)
    {
        float lockAngle = transform.eulerAngles.z;
        float shadowAngle = pastedShadow.transform.eulerAngles.z;
        float difference = Mathf.Abs(Mathf.DeltaAngle(lockAngle, shadowAngle));
        return difference <= allowedAngleDifference;
    }

    private void CacheComponents()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }

        if (lockController == null)
        {
            lockController = GetComponent<LockController>();
        }

        if (lockController == null)
        {
            lockController = GetComponentInParent<LockController>();
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
