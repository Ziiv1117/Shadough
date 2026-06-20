using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float groundCheckWidth = 0.65f;
    [SerializeField] private float minGroundNormalY = 0.65f;

    [Header("Camera Follow")]
    [SerializeField] private bool cameraFollowsPlayer = true;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 1.5f, -10f);
    [SerializeField] private float cameraFollowSpeed = 8f;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private float horizontalInput;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);

        if (jumpRequested)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpRequested = false;
        }
    }

    private void LateUpdate()
    {
        if (!cameraFollowsPlayer || cameraTransform == null)
        {
            return;
        }

        Vector3 targetPosition = transform.position + cameraOffset;
        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            targetPosition,
            cameraFollowSpeed * Time.deltaTime);
    }

    private bool IsGrounded()
    {
        Bounds bounds = bodyCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        Vector2 size = new Vector2(bounds.size.x * groundCheckWidth, 0.04f);
        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, size, 0f, Vector2.down, groundCheckDistance, groundLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit2D hit = hits[i];
            if (hit.collider != null
                && hit.collider != bodyCollider
                && hit.normal.y >= minGroundNormalY)
            {
                return true;
            }
        }

        return false;
    }
}
