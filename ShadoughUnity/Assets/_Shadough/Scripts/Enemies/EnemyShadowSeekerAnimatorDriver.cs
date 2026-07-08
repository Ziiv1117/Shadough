using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyShadowSeekerAnimatorDriver : MonoBehaviour
{
    private enum FacingDirection
    {
        Down,
        Left,
        Right,
        Up
    }

    [Header("References")]
    [SerializeField] private EnemyShadowSeeker seeker;
    [SerializeField] private Animator animator;

    [Header("Timing")]
    [SerializeField] private float alertDuration = 0.25f;
    [SerializeField] private float moveThreshold = 0.01f;
    [SerializeField] private float lureReachedDistancePadding = 0.08f;

    private FacingDirection facingDirection = FacingDirection.Down;
    private Vector3 lastPosition;
    private string lastSeekerState = string.Empty;
    private float alertUntilTime;
    private int currentStateHash;

    private void Awake()
    {
        ResolveReferences();
        lastPosition = transform.position;
        PlayAction("Idle", true);
    }

    private void LateUpdate()
    {
        ResolveReferences();
        UpdateFacingDirection();

        string seekerState = seeker != null ? seeker.CurrentStateName : "Idle";
        if (seekerState != lastSeekerState)
        {
            if (seekerState == "ChasingPlayer")
            {
                alertUntilTime = Time.time + alertDuration;
            }

            lastSeekerState = seekerState;
        }

        PlayAction(ResolveActionName(seekerState), false);
        lastPosition = transform.position;
    }

    private string ResolveActionName(string seekerState)
    {
        switch (seekerState)
        {
            case "ChasingPlayer":
                return Time.time < alertUntilTime ? "Alert" : "Chase";
            case "ChasingShadow":
                return HasReachedLureTarget() ? "LureReached" : "Attracted";
            case "Attacking":
                return "Alert";
            case "ReturningHome":
            case "Idle":
            default:
                return "Idle";
        }
    }

    private bool HasReachedLureTarget()
    {
        if (seeker == null || seeker.CurrentTarget == null)
        {
            return false;
        }

        float reachedDistance = seeker.StopDistance + lureReachedDistancePadding;
        return Vector2.Distance(transform.position, seeker.CurrentTarget.transform.position) <= reachedDistance;
    }

    private void UpdateFacingDirection()
    {
        Vector3 delta = transform.position - lastPosition;
        delta.z = 0f;
        if (delta.sqrMagnitude <= moveThreshold * moveThreshold)
        {
            return;
        }

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            facingDirection = delta.x < 0f ? FacingDirection.Left : FacingDirection.Right;
        }
        else
        {
            facingDirection = delta.y < 0f ? FacingDirection.Down : FacingDirection.Up;
        }
    }

    private void PlayAction(string actionName, bool force)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        string stateName = "ShadowSeeker_" + actionName + "_" + facingDirection;
        int stateHash = Animator.StringToHash(stateName);
        if (!animator.HasState(0, stateHash))
        {
            return;
        }

        if (!force && currentStateHash == stateHash)
        {
            return;
        }

        animator.Play(stateHash, 0, 0f);
        currentStateHash = stateHash;
    }

    private void ResolveReferences()
    {
        if (seeker == null)
        {
            seeker = GetComponent<EnemyShadowSeeker>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnValidate()
    {
        alertDuration = Mathf.Max(0f, alertDuration);
        moveThreshold = Mathf.Max(0.001f, moveThreshold);
        lureReachedDistancePadding = Mathf.Max(0f, lureReachedDistancePadding);
        ResolveReferences();
    }
}
