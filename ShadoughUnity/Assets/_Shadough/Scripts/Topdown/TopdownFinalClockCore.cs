using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TopdownFinalClockCore : MonoBehaviour
{
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string promptText = "Press E to start Clock Core";
    [SerializeField] private string completeText = "Topdown Demo Complete";
    [SerializeField] private string logMessage = "Topdown demo complete";
    [SerializeField] private Vector2 promptPosition = new Vector2(24f, 56f);
    [SerializeField] private Vector2 promptSize = new Vector2(320f, 32f);
    [SerializeField] private Vector2 completeSize = new Vector2(340f, 80f);

    private bool playerInRange;
    private bool isActivated;

    public bool PlayerInRange => playerInRange;
    public bool IsActivated => isActivated;

    private void Awake()
    {
        SetTriggerCollider();
    }

    private void Update()
    {
        if (!isActivated && playerInRange && Input.GetKeyDown(interactKey))
        {
            ActivateClockCore();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerInRange = false;
        }
    }

    private void OnGUI()
    {
        if (!isActivated && playerInRange && !string.IsNullOrEmpty(promptText))
        {
            Rect promptRect = new Rect(promptPosition.x, Screen.height - promptPosition.y - promptSize.y, promptSize.x, promptSize.y);
            GUI.Label(promptRect, promptText);
        }

        if (isActivated)
        {
            Rect boxRect = new Rect((Screen.width - completeSize.x) * 0.5f, (Screen.height - completeSize.y) * 0.5f, completeSize.x, completeSize.y);
            GUI.Box(boxRect, string.Empty);
            GUI.Label(new Rect(boxRect.x + 86f, boxRect.y + 28f, 220f, 24f), completeText);
        }
    }

    public void ActivateClockCore()
    {
        if (isActivated)
        {
            return;
        }

        isActivated = true;
        Debug.Log(logMessage);
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag(playerTag)
            || other.GetComponent<TopDownPlayerController>() != null
            || other.GetComponentInParent<TopDownPlayerController>() != null;
    }

    private void Reset()
    {
        SetTriggerCollider();
    }

    private void OnValidate()
    {
        SetTriggerCollider();
    }

    private void SetTriggerCollider()
    {
        Collider2D triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
}
