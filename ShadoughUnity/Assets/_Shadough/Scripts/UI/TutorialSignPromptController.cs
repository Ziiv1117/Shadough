using UnityEngine;

public class TutorialSignPromptController : MonoBehaviour
{
    private static TutorialSignPromptController instance;
    private static int promptClosedFrame = -1;

    [SerializeField] private bool isPromptOpen;
    [SerializeField] private string currentTitle = string.Empty;
    [SerializeField] private string currentText = string.Empty;

    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;

    public static bool IsPromptOpen => instance != null && instance.isPromptOpen;
    public static bool BlocksPauseMenuThisFrame => IsPromptOpen || Time.frameCount == promptClosedFrame;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple TutorialSignPromptController instances found. Using the latest active instance.");
        }

        instance = this;
    }

    private void Update()
    {
        if (isPromptOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePrompt();
        }
    }

    private void OnGUI()
    {
        if (!isPromptOpen)
        {
            return;
        }

        EnsureStyles();

        float panelWidth = Screen.width * 0.6f;
        float panelHeight = Screen.height * 0.6f;
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        GUI.Box(panelRect, string.Empty);

        float padding = Mathf.Max(18f, panelWidth * 0.05f);
        float contentX = panelRect.x + padding;
        float contentY = panelRect.y + padding;
        float contentWidth = panelRect.width - padding * 2f;

        if (!string.IsNullOrEmpty(currentTitle))
        {
            GUI.Label(new Rect(contentX, contentY, contentWidth, 34f), currentTitle, titleStyle);
            contentY += 46f;
        }

        GUI.Label(
            new Rect(contentX, contentY, contentWidth, panelRect.yMax - contentY - padding),
            currentText,
            bodyStyle);
    }

    public static void ShowPrompt(string title, string text)
    {
        if (instance == null)
        {
            GameObject promptObject = new GameObject("TutorialSignPromptController");
            instance = promptObject.AddComponent<TutorialSignPromptController>();
        }

        instance.currentTitle = title;
        instance.currentText = text;
        instance.isPromptOpen = true;
    }

    public static void ClosePrompt()
    {
        if (instance == null || !instance.isPromptOpen)
        {
            return;
        }

        instance.isPromptOpen = false;
        promptClosedFrame = Time.frameCount;
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 18,
            wordWrap = true
        };
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
