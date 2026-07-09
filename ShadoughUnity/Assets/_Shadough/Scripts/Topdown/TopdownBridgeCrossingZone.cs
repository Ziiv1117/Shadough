using UnityEngine;

public class TopdownBridgeCrossingZone : MonoBehaviour
{
    private const float BridgeOverlapTolerance = 0.02f;

    [Header("Bridge Check")]
    [SerializeField] private float detectionRadius = 1.1f;
    [SerializeField] private LayerMask detectionMask = ~0;

    [Header("Blocking")]
    [SerializeField] private GameObject crossingBlocker;
    [SerializeField] private bool makeBridgeColliderTrigger = true;
    [SerializeField] private Transform playerRoot;

    [Header("Debug")]
    [SerializeField] private bool logStateChanges;

    private PastedShadowObject activeBridgeShadow;
    private Collider2D[] blockerColliders;
    private Collider2D[] playerColliders;
    private bool isOpen;
    private bool blockerCollisionIgnored;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        CacheBlockerColliders();
        CachePlayerColliders();

        if (crossingBlocker != null && !crossingBlocker.activeSelf)
        {
            crossingBlocker.SetActive(true);
        }
    }

    private void Update()
    {
        PastedShadowObject bridgeShadow = FindBridgeShadow();
        SetBridgeShadow(bridgeShadow);
        SetBlockerCollisionIgnored(ShouldLetPlayerCrossOnBridge());
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

    private void SetBridgeShadow(PastedShadowObject bridgeShadow)
    {
        if (activeBridgeShadow != null && activeBridgeShadow != bridgeShadow)
        {
            RestoreBridgeCollider(activeBridgeShadow);
        }

        activeBridgeShadow = bridgeShadow;
        bool open = bridgeShadow != null;
        isOpen = open;

        if (crossingBlocker != null && !crossingBlocker.activeSelf)
        {
            crossingBlocker.SetActive(true);
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

    private bool ShouldLetPlayerCrossOnBridge()
    {
        if (activeBridgeShadow == null || activeBridgeShadow.ShapeCollider == null)
        {
            return false;
        }

        if (playerColliders == null || playerColliders.Length == 0)
        {
            CachePlayerColliders();
        }

        Collider2D bridgeCollider = activeBridgeShadow.ShapeCollider;
        for (int i = 0; i < playerColliders.Length; i++)
        {
            Collider2D playerCollider = playerColliders[i];
            if (playerCollider == null || playerCollider.isTrigger)
            {
                continue;
            }

            if (CollidersTouchOrOverlap(playerCollider, bridgeCollider))
            {
                return true;
            }
        }

        return false;
    }

    private bool CollidersTouchOrOverlap(Collider2D a, Collider2D b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        ColliderDistance2D distance = a.Distance(b);
        return distance.isOverlapped || distance.distance <= BridgeOverlapTolerance;
    }

    private void SetBlockerCollisionIgnored(bool ignored)
    {
        if (blockerCollisionIgnored == ignored)
        {
            return;
        }

        if (blockerColliders == null || blockerColliders.Length == 0)
        {
            CacheBlockerColliders();
        }

        if (playerColliders == null || playerColliders.Length == 0)
        {
            CachePlayerColliders();
        }

        for (int blockerIndex = 0; blockerIndex < blockerColliders.Length; blockerIndex++)
        {
            Collider2D blockerCollider = blockerColliders[blockerIndex];
            if (blockerCollider == null)
            {
                continue;
            }

            for (int playerIndex = 0; playerIndex < playerColliders.Length; playerIndex++)
            {
                Collider2D playerCollider = playerColliders[playerIndex];
                if (playerCollider == null || playerCollider.isTrigger)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(playerCollider, blockerCollider, ignored);
            }
        }

        blockerCollisionIgnored = ignored;
    }

    private void CacheBlockerColliders()
    {
        blockerColliders = crossingBlocker != null
            ? crossingBlocker.GetComponentsInChildren<Collider2D>(true)
            : new Collider2D[0];
    }

    private void CachePlayerColliders()
    {
        if (playerRoot == null)
        {
            TopDownPlayerController player = FindObjectOfType<TopDownPlayerController>();
            if (player != null)
            {
                playerRoot = player.transform;
            }
        }

        playerColliders = playerRoot != null
            ? playerRoot.GetComponentsInChildren<Collider2D>(true)
            : new Collider2D[0];
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
        SetBlockerCollisionIgnored(false);

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
