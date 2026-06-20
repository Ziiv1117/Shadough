using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShadowLeverTrigger : MonoBehaviour
{
    [SerializeField] private LeverController leverController;
    [SerializeField] private ShadowType requiredShadowType = ShadowType.Hand;

    private Collider2D triggerCollider;

    private void Awake()
    {
        CacheComponents();
        SetTrigger();
    }

    private void Reset()
    {
        CacheComponents();
        SetTrigger();
    }

    private void OnValidate()
    {
        CacheComponents();
        SetTrigger();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryActivateFrom(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryActivateFrom(other);
    }

    private void TryActivateFrom(Collider2D other)
    {
        if (leverController == null || leverController.IsActivated)
        {
            return;
        }

        PastedShadowObject pastedShadow = other.GetComponent<PastedShadowObject>();
        if (pastedShadow == null)
        {
            pastedShadow = other.GetComponentInParent<PastedShadowObject>();
        }

        if (pastedShadow == null || pastedShadow.ShadowType != requiredShadowType)
        {
            return;
        }

        leverController.Activate();
    }

    private void CacheComponents()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider2D>();
        }

        if (leverController == null)
        {
            leverController = GetComponentInParent<LeverController>();
        }
    }

    private void SetTrigger()
    {
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
}
