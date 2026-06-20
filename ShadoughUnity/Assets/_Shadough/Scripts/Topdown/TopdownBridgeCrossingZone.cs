using UnityEngine;

public class TopdownBridgeCrossingZone : MonoBehaviour
{
    [Header("Bridge Check")]
    [SerializeField] private float detectionRadius = 1.1f;
    [SerializeField] private LayerMask detectionMask = ~0;

    [Header("Blocking")]
    [SerializeField] private GameObject crossingBlocker;
    [SerializeField] private bool makeBridgeColliderTrigger = true;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges;

    private PastedShadowObject activeBridgeShadow;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Update()
    {
        PastedShadowObject bridgeShadow = FindBridgeShadow();
        SetOpen(bridgeShadow != null, bridgeShadow);
    }

    private PastedShadowObject FindBridgeShadow()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionMask);
        for (int i = 0; i < hits.Length; i++)
        {
            PastedShadowObject pastedShadow = hits[i].GetComponent<PastedShadowObject>();
            if (pastedShadow == null)
            {
                pastedShadow = hits[i].GetComponentInParent<PastedShadowObject>();
            }

            if (pastedShadow != null && pastedShadow.CanStandOn)
            {
                return pastedShadow;
            }
        }

        return null;
    }

    private void SetOpen(bool open, PastedShadowObject bridgeShadow)
    {
        if (activeBridgeShadow != null && activeBridgeShadow != bridgeShadow)
        {
            RestoreBridgeCollider(activeBridgeShadow);
        }

        activeBridgeShadow = bridgeShadow;
        isOpen = open;

        if (crossingBlocker != null && crossingBlocker.activeSelf == open)
        {
            crossingBlocker.SetActive(!open);
        }

        if (open && makeBridgeColliderTrigger && activeBridgeShadow != null && activeBridgeShadow.ShapeCollider != null)
        {
            activeBridgeShadow.ShapeCollider.isTrigger = true;
        }

        if (logStateChanges)
        {
            Debug.Log(open ? "Topdown bridge crossing opened" : "Topdown bridge crossing closed");
        }
    }

    private void RestoreBridgeCollider(PastedShadowObject bridgeShadow)
    {
        if (bridgeShadow != null && bridgeShadow.ShapeCollider != null)
        {
            bridgeShadow.ShapeCollider.isTrigger = !bridgeShadow.CanStandOn;
        }
    }

    private void OnDisable()
    {
        if (activeBridgeShadow != null)
        {
            RestoreBridgeCollider(activeBridgeShadow);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
