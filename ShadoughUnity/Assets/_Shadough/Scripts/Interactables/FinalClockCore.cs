using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class FinalClockCore : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("State")]
    [SerializeField] private bool playerInRange;
    [SerializeField] private bool isActivated;

    [Header("UI")]
    [SerializeField] private string promptText = "Press E to Start Clock";
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Vector2 promptPosition = new Vector2(24f, 24f);
    [SerializeField] private Vector2 promptSize = new Vector2(320f, 32f);

    [Header("Scene")]
    [SerializeField] private string winSceneName = "WinScene";
    [SerializeField] private bool loadWinScene;

    public bool PlayerInRange => playerInRange;
    public bool IsActivated => isActivated;

    private void Awake()
    {
        SetTriggerCollider();

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isActivated && playerInRange && Input.GetKeyDown(interactKey))
        {
            ActivateClock();
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
            GUI.Label(GetBottomLeftPromptRect(), promptText);
        }

        if (isActivated)
        {
            GUI.Box(new Rect(Screen.width * 0.5f - 150f, Screen.height * 0.5f - 55f, 300f, 110f), string.Empty);
            GUI.Label(new Rect(Screen.width * 0.5f - 90f, Screen.height * 0.5f - 35f, 220f, 24f), "Demo Complete");
            GUI.Label(new Rect(Screen.width * 0.5f - 110f, Screen.height * 0.5f - 10f, 240f, 24f), "The clock begins to move.");
        }
    }

    public void ActivateClock()
    {
        if (isActivated)
        {
            return;
        }

        isActivated = true;
        Debug.Log("Demo Complete");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (loadWinScene && Application.CanStreamedLevelBeLoaded(winSceneName))
        {
            SceneManager.LoadScene(winSceneName);
        }
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.GetComponent<PlayerController>() != null
            || other.GetComponentInParent<PlayerController>() != null;
    }

    private Rect GetBottomLeftPromptRect()
    {
        float x = promptPosition.x;
        float y = Screen.height - promptPosition.y - promptSize.y;
        return new Rect(x, y, promptSize.x, promptSize.y);
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
