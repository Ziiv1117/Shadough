using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ShadowPasteArea : MonoBehaviour
{
    [Header("Requirement")]
    [SerializeField] private ShadowType requiredShadowType = ShadowType.None;
    [SerializeField] private bool activated;

    [Header("Messages")]
    [SerializeField] private string promptMessage = "Press F to Paste";
    [SerializeField] private string wrongShadowMessage = "Wrong shadow";

    [Header("Events")]
    [SerializeField] private UnityEvent onActivated;

    [Header("Debug Prompt")]
    [SerializeField] private bool showDebugPrompt = true;
    [SerializeField] private Vector2 promptPosition = new Vector2(24f, 56f);
    [SerializeField] private Vector2 promptSize = new Vector2(320f, 32f);

    private Collider2D triggerCollider;
    private ShadowInventory currentInventory;
    private string currentMessage;
    private float messageUntilTime;

    public ShadowType RequiredShadowType => requiredShadowType;
    public bool Activated => activated;
    public UnityEvent OnActivated => onActivated;

    private void Awake()
    {
        CacheCollider();
        SetTriggerCollider();
    }

    private void Reset()
    {
        CacheCollider();
        SetTriggerCollider();
    }

    private void OnValidate()
    {
        CacheCollider();
        SetTriggerCollider();
    }

    private void Update()
    {
        if (activated || currentInventory == null || !currentInventory.HasShadow())
        {
            return;
        }

        currentMessage = promptMessage;

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPaste(currentInventory);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ShadowInventory inventory = other.GetComponent<ShadowInventory>();
        if (inventory == null)
        {
            inventory = other.GetComponentInParent<ShadowInventory>();
        }

        if (inventory != null)
        {
            currentInventory = inventory;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        ShadowInventory inventory = other.GetComponent<ShadowInventory>();
        if (inventory == null)
        {
            inventory = other.GetComponentInParent<ShadowInventory>();
        }

        if (inventory != null && inventory == currentInventory)
        {
            currentInventory = null;
            currentMessage = string.Empty;
        }
    }

    private void OnGUI()
    {
        if (!showDebugPrompt || string.IsNullOrEmpty(currentMessage))
        {
            return;
        }

        if (messageUntilTime > 0f && Time.time > messageUntilTime)
        {
            currentMessage = string.Empty;
            messageUntilTime = 0f;
            return;
        }

        Rect promptRect = GetBottomLeftPromptRect();
        GUI.Label(promptRect, currentMessage);
    }

    private void TryPaste(ShadowInventory inventory)
    {
        ShadowType currentShadowType = inventory.CurrentShadowType;

        if (currentShadowType != requiredShadowType)
        {
            ShowMessage(wrongShadowMessage, 1.2f);
            Debug.Log($"{name}: {wrongShadowMessage}. Required {requiredShadowType}, got {currentShadowType}.");
            return;
        }

        inventory.ConsumeShadow();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.pasteShadow);
        }

        Activate();
    }

    public void Activate()
    {
        if (activated)
        {
            return;
        }

        activated = true;
        onActivated.Invoke();
        Debug.Log($"ShadowPasteArea activated: {name}, required shadow: {requiredShadowType}");
    }

    private void ShowMessage(string message, float duration)
    {
        currentMessage = message;
        messageUntilTime = Time.time + duration;
    }

    private Rect GetBottomLeftPromptRect()
    {
        float x = promptPosition.x;
        float y = Screen.height - promptPosition.y - promptSize.y;
        return new Rect(x, y, promptSize.x, promptSize.y);
    }

    private void CacheCollider()
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
