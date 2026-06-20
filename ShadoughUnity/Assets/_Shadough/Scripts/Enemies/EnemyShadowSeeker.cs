using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyShadowSeeker : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float stopDistance = 0.35f;
    [SerializeField] private Transform homePoint;

    [Header("Detection")]
    [SerializeField] private LayerMask detectionMask = ~0;
    [SerializeField] private PastedShadowObject currentTarget;

    [Header("State")]
    [SerializeField] private bool isChasingShadow;
    [SerializeField] private Vector3 startPosition;

    public float MoveSpeed => moveSpeed;
    public float DetectionRadius => detectionRadius;
    public float StopDistance => stopDistance;
    public Transform HomePoint => homePoint;
    public LayerMask DetectionMask => detectionMask;
    public PastedShadowObject CurrentTarget => currentTarget;
    public bool IsChasingShadow => isChasingShadow;
    public Vector3 StartPosition => startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        RefreshTarget();

        if (currentTarget != null)
        {
            isChasingShadow = true;
            MoveTowards(currentTarget.transform.position);
            return;
        }

        isChasingShadow = false;
        MoveTowards(GetHomePosition());
    }

    private void RefreshTarget()
    {
        if (IsValidTarget(currentTarget))
        {
            return;
        }

        currentTarget = FindNearestAttractingShadow();
    }

    private PastedShadowObject FindNearestAttractingShadow()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionMask);
        PastedShadowObject nearestTarget = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            PastedShadowObject pastedShadow = hits[i].GetComponent<PastedShadowObject>();
            if (pastedShadow == null)
            {
                pastedShadow = hits[i].GetComponentInParent<PastedShadowObject>();
            }

            if (!IsValidTarget(pastedShadow))
            {
                continue;
            }

            float distanceSqr = (pastedShadow.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestTarget = pastedShadow;
            }
        }

        return nearestTarget;
    }

    private bool IsValidTarget(PastedShadowObject pastedShadow)
    {
        return pastedShadow != null
            && pastedShadow.isActiveAndEnabled
            && pastedShadow.gameObject.activeInHierarchy
            && pastedShadow.CanAttractEnemy;
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector2 currentPosition = transform.position;
        Vector2 target = targetPosition;

        if (Vector2.Distance(currentPosition, target) <= stopDistance)
        {
            return;
        }

        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, target, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
    }

    private Vector3 GetHomePosition()
    {
        return homePoint != null ? homePoint.position : startPosition;
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        detectionRadius = Mathf.Max(0.1f, detectionRadius);
        stopDistance = Mathf.Max(0f, stopDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
