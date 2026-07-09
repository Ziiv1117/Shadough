using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider2D))]
public class EnemyShadowSeeker : MonoBehaviour
{
    private enum ShadowSeekerState
    {
        Idle,
        ReturningHome,
        ChasingPlayer,
        ChasingShadow,
        Attacking
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float stopDistance = 0.35f;
    [SerializeField] private Transform homePoint;

    [Header("Detection")]
    [SerializeField] private LayerMask detectionMask = ~0;
    [SerializeField] private Transform playerTarget;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private PastedShadowObject currentTarget;

    [Header("Physics")]
    [SerializeField] private bool ignorePlayerBodyCollision = true;

    [Header("Attack")]
    [SerializeField] private float attackDistance = 0.6f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackFeedbackDuration = 0.25f;
    [SerializeField] private string attackMessage = "The seeker caught you.";
    [SerializeField] private bool showAttackPrompt = true;
    [SerializeField] private bool playPlayerHurtAnimation = true;

    [Header("Feedback")]
    [SerializeField] private SpriteRenderer feedbackRenderer;
    [SerializeField] private bool tintRendererByState;
    [SerializeField] private Color idleColor = new Color(0.48f, 0.5f, 0.58f, 1f);
    [SerializeField] private Color chasingPlayerColor = new Color(1f, 0.22f, 0.16f, 1f);
    [SerializeField] private Color chasingShadowColor = new Color(0.25f, 0.72f, 1f, 1f);
    [SerializeField] private Color attackingColor = new Color(1f, 0.9f, 0.15f, 1f);
    [SerializeField] private bool showStateMarker = true;
    [SerializeField] private Vector3 stateMarkerOffset = new Vector3(0f, 0.75f, 0f);
    [SerializeField] private bool logStateChanges;
    [FormerlySerializedAs("showDetectionGizmo")]
    [SerializeField] private bool showDebugGizmos = true;

    [Header("State")]
    [SerializeField] private ShadowSeekerState currentState = ShadowSeekerState.Idle;
    [SerializeField] private bool isChasingShadow;
    [SerializeField] private bool isChasingPlayer;
    [SerializeField] private Vector3 startPosition;

    public float MoveSpeed => moveSpeed;
    public float DetectionRadius => detectionRadius;
    public float StopDistance => stopDistance;
    public float AttackDistance => attackDistance;
    public float AttackCooldown => attackCooldown;
    public Transform HomePoint => homePoint;
    public LayerMask DetectionMask => detectionMask;
    public PastedShadowObject CurrentTarget => currentTarget;
    public Transform PlayerTarget => playerTarget;
    public bool IsChasingShadow => isChasingShadow;
    public bool IsChasingPlayer => isChasingPlayer;
    public string CurrentStateName => currentState.ToString();
    public Vector3 StartPosition => startPosition;

    private float nextAttackTime;
    private float attackFeedbackUntilTime;
    private TextMesh stateMarker;
    private Collider2D[] seekerColliders;
    private Transform collisionIgnoredPlayer;

    private void Awake()
    {
        startPosition = transform.position;
        CacheSeekerColliders();
        ResolvePlayer();
        ResolveFeedbackRenderer();
        EnsureStateMarker();
        ApplyStateFeedback();
    }

    private void Update()
    {
        RefreshTarget();

        if (currentTarget != null)
        {
            isChasingShadow = true;
            isChasingPlayer = false;
            SetMovementState(ShadowSeekerState.ChasingShadow);
            MoveTowards(currentTarget.transform.position);
            return;
        }

        isChasingShadow = false;
        if (CanDetectPlayer())
        {
            isChasingPlayer = true;
            ChasePlayer();
            return;
        }

        isChasingPlayer = false;
        ReturnHome();
    }

    private void RefreshTarget()
    {
        currentTarget = FindNearestAttractingShadow();
    }

    private PastedShadowObject FindNearestAttractingShadow()
    {
        PastedShadowObject[] pastedShadows = FindObjectsOfType<PastedShadowObject>();
        PastedShadowObject nearestTarget = null;
        float nearestDistanceSqr = float.MaxValue;
        float detectionRadiusSqr = detectionRadius * detectionRadius;

        for (int i = 0; i < pastedShadows.Length; i++)
        {
            PastedShadowObject pastedShadow = pastedShadows[i];

            if (!IsValidTarget(pastedShadow))
            {
                continue;
            }

            float distanceSqr = (pastedShadow.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr > detectionRadiusSqr)
            {
                continue;
            }

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

    private bool CanDetectPlayer()
    {
        ResolvePlayer();
        if (playerTarget == null)
        {
            return false;
        }

        return Vector2.Distance(transform.position, playerTarget.position) <= detectionRadius;
    }

    private void ResolvePlayer()
    {
        if (playerTarget != null)
        {
            ConfigurePlayerCollision();
            return;
        }

        if (string.IsNullOrEmpty(playerTag))
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            playerTarget = playerObject.transform;
            ConfigurePlayerCollision();
        }
    }

    private void CacheSeekerColliders()
    {
        seekerColliders = GetComponentsInChildren<Collider2D>();
    }

    private void ConfigurePlayerCollision()
    {
        if (!ignorePlayerBodyCollision || playerTarget == null || collisionIgnoredPlayer == playerTarget)
        {
            return;
        }

        if (seekerColliders == null || seekerColliders.Length == 0)
        {
            CacheSeekerColliders();
        }

        Collider2D[] playerColliders = playerTarget.GetComponentsInChildren<Collider2D>();
        for (int seekerIndex = 0; seekerIndex < seekerColliders.Length; seekerIndex++)
        {
            Collider2D seekerCollider = seekerColliders[seekerIndex];
            if (seekerCollider == null)
            {
                continue;
            }

            for (int playerIndex = 0; playerIndex < playerColliders.Length; playerIndex++)
            {
                Collider2D playerCollider = playerColliders[playerIndex];
                if (playerCollider != null)
                {
                    Physics2D.IgnoreCollision(seekerCollider, playerCollider, true);
                }
            }
        }

        collisionIgnoredPlayer = playerTarget;
    }

    private void ChasePlayer()
    {
        if (playerTarget == null)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTarget.position);
        SetMovementState(ShadowSeekerState.ChasingPlayer);

        if (distance > attackDistance)
        {
            MoveTowards(playerTarget.position, attackDistance);
        }

        if (distance <= attackDistance)
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        if (Time.time < nextAttackTime)
        {
            return;
        }

        nextAttackTime = Time.time + attackCooldown;
        attackFeedbackUntilTime = Time.time + attackFeedbackDuration;
        SetState(ShadowSeekerState.Attacking);

        if (TryDamagePlayer())
        {
            return;
        }

        if (showAttackPrompt)
        {
            TutorialFailurePromptController.Show(attackMessage);
        }

        if (playPlayerHurtAnimation && playerTarget != null)
        {
            HeroPlayerAnimatorDriver animatorDriver = playerTarget.GetComponent<HeroPlayerAnimatorDriver>();
            if (animatorDriver != null)
            {
                animatorDriver.PlayHurtFeedback();
            }
        }
    }

    private bool TryDamagePlayer()
    {
        if (playerTarget == null)
        {
            return false;
        }

        PlayerHealth playerHealth = playerTarget.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            return false;
        }

        playerHealth.TakeDamage(1, attackMessage);
        return true;
    }

    private void ReturnHome()
    {
        Vector3 homePosition = GetHomePosition();
        if (Vector2.Distance(transform.position, homePosition) <= stopDistance)
        {
            SetMovementState(ShadowSeekerState.Idle);
            return;
        }

        SetMovementState(ShadowSeekerState.ReturningHome);
        MoveTowards(homePosition);
    }

    public void ResetToHome()
    {
        transform.position = GetHomePosition();
        currentTarget = null;
        isChasingShadow = false;
        isChasingPlayer = false;
        nextAttackTime = Time.time + attackCooldown;
        attackFeedbackUntilTime = 0f;
        SetState(ShadowSeekerState.Idle);
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        MoveTowards(targetPosition, stopDistance);
    }

    private void MoveTowards(Vector3 targetPosition, float stopAtDistance)
    {
        Vector2 currentPosition = transform.position;
        Vector2 target = targetPosition;

        if (Vector2.Distance(currentPosition, target) <= stopAtDistance)
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

    private void SetMovementState(ShadowSeekerState nextState)
    {
        if (Time.time < attackFeedbackUntilTime && currentState == ShadowSeekerState.Attacking)
        {
            return;
        }

        SetState(nextState);
    }

    private void SetState(ShadowSeekerState nextState)
    {
        if (currentState == nextState)
        {
            return;
        }

        ShadowSeekerState previousState = currentState;
        currentState = nextState;

        if (previousState != ShadowSeekerState.ChasingPlayer
            && nextState == ShadowSeekerState.ChasingPlayer)
        {
            PlayDetectionSound();
        }

        ApplyStateFeedback();

        if (logStateChanges)
        {
            Debug.Log(name + " state: " + currentState);
        }
    }

    private void ApplyStateFeedback()
    {
        Color stateColor = GetStateColor(currentState);
        if (feedbackRenderer != null)
        {
            feedbackRenderer.color = tintRendererByState ? stateColor : Color.white;
        }

        UpdateStateMarker(stateColor);
    }

    private Color GetStateColor(ShadowSeekerState state)
    {
        switch (state)
        {
            case ShadowSeekerState.ChasingPlayer:
                return chasingPlayerColor;
            case ShadowSeekerState.ChasingShadow:
                return chasingShadowColor;
            case ShadowSeekerState.Attacking:
                return attackingColor;
            case ShadowSeekerState.ReturningHome:
            case ShadowSeekerState.Idle:
            default:
                return idleColor;
        }
    }

    private void ResolveFeedbackRenderer()
    {
        if (feedbackRenderer != null)
        {
            return;
        }

        feedbackRenderer = GetComponent<SpriteRenderer>();
        if (feedbackRenderer == null)
        {
            feedbackRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void EnsureStateMarker()
    {
        if (!showStateMarker)
        {
            return;
        }

        Transform markerTransform = transform.Find("ShadowSeekerStateMarker");
        GameObject markerObject;
        if (markerTransform != null)
        {
            markerObject = markerTransform.gameObject;
            stateMarker = markerObject.GetComponent<TextMesh>();
        }
        else
        {
            markerObject = new GameObject("ShadowSeekerStateMarker");
            markerObject.transform.SetParent(transform, false);
            stateMarker = markerObject.AddComponent<TextMesh>();
        }

        markerObject.transform.localPosition = stateMarkerOffset;
        markerObject.transform.localRotation = Quaternion.identity;
        markerObject.transform.localScale = Vector3.one;

        if (stateMarker == null)
        {
            stateMarker = markerObject.AddComponent<TextMesh>();
        }

        stateMarker.anchor = TextAnchor.MiddleCenter;
        stateMarker.alignment = TextAlignment.Center;
        stateMarker.fontSize = 48;
        stateMarker.characterSize = 0.08f;

        MeshRenderer markerRenderer = markerObject.GetComponent<MeshRenderer>();
        if (markerRenderer != null)
        {
            markerRenderer.sortingOrder = 80;
        }
    }

    private void UpdateStateMarker(Color stateColor)
    {
        if (stateMarker == null)
        {
            return;
        }

        string markerText = GetStateMarkerText(currentState);
        stateMarker.text = markerText;
        stateMarker.color = stateColor;
        stateMarker.gameObject.SetActive(showStateMarker && !string.IsNullOrEmpty(markerText));
    }

    private string GetStateMarkerText(ShadowSeekerState state)
    {
        switch (state)
        {
            case ShadowSeekerState.ChasingPlayer:
                return "!";
            case ShadowSeekerState.ChasingShadow:
                return "?";
            case ShadowSeekerState.Attacking:
                return "X";
            case ShadowSeekerState.ReturningHome:
                return "...";
            case ShadowSeekerState.Idle:
            default:
                return string.Empty;
        }
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0f, moveSpeed);
        detectionRadius = Mathf.Max(0.1f, detectionRadius);
        stopDistance = Mathf.Max(0f, stopDistance);
        attackDistance = Mathf.Max(0.1f, attackDistance);
        attackCooldown = Mathf.Max(0.1f, attackCooldown);
        attackFeedbackDuration = Mathf.Max(0.05f, attackFeedbackDuration);
    }

    private void OnDrawGizmosSelected()
    {
        DrawDetectionGizmo();
    }

    private void OnDrawGizmos()
    {
        if (showDebugGizmos)
        {
            DrawDetectionGizmo();
        }
    }

    private void DrawDetectionGizmo()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }

    private void PlayDetectionSound()
    {
        if (AudioManager.Instance != null
            && AudioManager.Instance.shadowSeeker != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.shadowSeeker);
        }
    }
}
