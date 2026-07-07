using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShadowLureFailureTrigger : MonoBehaviour
{
    [SerializeField] private string failureMessage = "This shadow cannot lure the seeker.";
    [SerializeField] private float promptCooldown = 1.5f;
    [SerializeField] private float overlapScanInterval = 0.15f;

    private Collider2D triggerCollider;
    private PastedShadowObject lastRejectedShadow;
    private float nextPromptTime;
    private float nextOverlapScanTime;

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
        promptCooldown = Mathf.Max(0.25f, promptCooldown);
        overlapScanInterval = Mathf.Max(0.05f, overlapScanInterval);
    }

    private void Update()
    {
        if (triggerCollider == null || Time.time < nextOverlapScanTime)
        {
            return;
        }

        nextOverlapScanTime = Time.time + overlapScanInterval;

        Bounds bounds = triggerCollider.bounds;
        Collider2D[] overlaps = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);
        for (int i = 0; i < overlaps.Length; i++)
        {
            if (overlaps[i] == null || overlaps[i] == triggerCollider)
            {
                continue;
            }

            TryShowFailureFrom(overlaps[i]);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryShowFailureFrom(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryShowFailureFrom(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PastedShadowObject pastedShadow = GetPastedShadow(other);
        if (pastedShadow != null && pastedShadow == lastRejectedShadow)
        {
            lastRejectedShadow = null;
        }
    }

    private void TryShowFailureFrom(Collider2D other)
    {
        PastedShadowObject pastedShadow = GetPastedShadow(other);
        if (pastedShadow == null || pastedShadow.CanAttractEnemy)
        {
            return;
        }

        if (pastedShadow == lastRejectedShadow || Time.time < nextPromptTime)
        {
            return;
        }

        lastRejectedShadow = pastedShadow;
        nextPromptTime = Time.time + promptCooldown;
        TutorialFailurePromptController.Show(failureMessage);
    }

    private PastedShadowObject GetPastedShadow(Collider2D other)
    {
        if (other == null)
        {
            return null;
        }

        PastedShadowObject pastedShadow = other.GetComponent<PastedShadowObject>();
        if (pastedShadow == null)
        {
            pastedShadow = other.GetComponentInParent<PastedShadowObject>();
        }

        return pastedShadow;
    }

    private void CacheComponents()
    {
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
}
