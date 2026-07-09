using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    [Header("Footstep Audio")]
    [SerializeField] private float footstepInterval = 0.4f;
    private float footstepTimer;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void Update()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        HandleFootstep();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    private void HandleFootstep()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.footstep);
                }

                footstepTimer = footstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void OnValidate()
    {
        moveSpeed = Mathf.Max(0.1f, moveSpeed);
        footstepInterval = Mathf.Max(0.05f, footstepInterval);
    }
}