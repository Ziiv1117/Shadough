using UnityEngine;

public class TutorialFailurePromptController : MonoBehaviour
{
    private static TutorialFailurePromptController instance;

    [SerializeField] private float defaultDuration = 1.7f;
    [SerializeField] private Vector2 panelSize = new Vector2(520f, 38f);
    [SerializeField] private float bottomOffset = 52f;
    [SerializeField] private bool hideWhileTutorialPromptOpen = true;

    private string currentMessage = string.Empty;
    private float visibleUntilTime;
    private GUIStyle boxStyle;
    private GUIStyle textStyle;

    public static bool HasInstance => instance != null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple TutorialFailurePromptController instances found. Using the latest active instance.");
        }

        instance = this;
    }

    private void OnGUI()
    {
        if (hideWhileTutorialPromptOpen && TutorialSignPromptController.IsPromptOpen)
        {
            return;
        }

        if (string.IsNullOrEmpty(currentMessage) || Time.unscaledTime > visibleUntilTime)
        {
            return;
        }

        EnsureStyles();

        float width = Mathf.Min(panelSize.x, Screen.width - 28f);
        float height = panelSize.y;
        Rect panelRect = new Rect(
            (Screen.width - width) * 0.5f,
            Screen.height - height - bottomOffset,
            width,
            height);

        GUI.Box(panelRect, string.Empty, boxStyle);
        GUI.Label(new Rect(panelRect.x + 14f, panelRect.y + 6f, panelRect.width - 28f, panelRect.height - 10f),
            currentMessage,
            textStyle);
    }

    public static void Show(string message)
    {
        Show(message, 0f);
    }

    public static void Show(string message, float duration)
    {
        if (instance == null || string.IsNullOrEmpty(message))
        {
            return;
        }

        if (instance.hideWhileTutorialPromptOpen && TutorialSignPromptController.IsPromptOpen)
        {
            return;
        }

        instance.currentMessage = message;
        float resolvedDuration = duration > 0f ? duration : instance.defaultDuration;
        instance.visibleUntilTime = Time.unscaledTime + resolvedDuration;
    }

    private void EnsureStyles()
    {
        if (boxStyle != null)
        {
            return;
        }

        boxStyle = new GUIStyle(GUI.skin.box);
        textStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            wordWrap = true
        };
        textStyle.normal.textColor = Color.white;
        textStyle.hover.textColor = Color.white;
        textStyle.active.textColor = Color.white;
        textStyle.focused.textColor = Color.white;
    }

    private void OnValidate()
    {
        defaultDuration = Mathf.Clamp(defaultDuration, 1.5f, 2f);
        panelSize.x = Mathf.Max(260f, panelSize.x);
        panelSize.y = Mathf.Clamp(panelSize.y, 30f, 48f);
        bottomOffset = Mathf.Max(12f, bottomOffset);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
