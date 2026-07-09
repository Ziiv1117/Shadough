using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private Text titleText;
    [SerializeField] private Text valueText;
    [SerializeField] private Text marksText;
    [SerializeField] private Image panelImage;
    [SerializeField] private Vector2 panelOffset = new Vector2(24f, 24f);
    [SerializeField] private Vector2 panelSize = new Vector2(280f, 92f);
    [SerializeField] private bool anchorTopRight;
    [SerializeField] private bool hideWhileTutorialPromptOpen = true;
    [SerializeField] private Color fullMarkColor = new Color(0.82f, 0.18f, 0.12f);
    [SerializeField] private Color emptyMarkColor = new Color(0.18f, 0.13f, 0.1f, 0.74f);

    private readonly StringBuilder marksBuilder = new StringBuilder(96);
    private GUIStyle boxStyle;
    private GUIStyle textStyle;
    private bool subscribedToHealth;

    private void Awake()
    {
        EnsurePlayerHealth();
        SubscribeToHealth();
        RefreshHealth();
    }

    private void OnEnable()
    {
        EnsurePlayerHealth();
        SubscribeToHealth();
        RefreshHealth();
    }

    private void Update()
    {
        if (canvasGroup == null && panelRoot == null)
        {
            return;
        }

        bool hidden = hideWhileTutorialPromptOpen && TutorialSignPromptController.IsPromptOpen;
        SetCanvasVisible(!hidden);
    }

    private void OnDisable()
    {
        UnsubscribeFromHealth();
    }

    private void OnGUI()
    {
        if (HasCanvasUI())
        {
            return;
        }

        if (hideWhileTutorialPromptOpen && TutorialSignPromptController.IsPromptOpen)
        {
            return;
        }

        EnsurePlayerHealth();
        if (playerHealth == null)
        {
            return;
        }

        EnsureStyles();

        float x = anchorTopRight
            ? Screen.width - panelSize.x - panelOffset.x
            : panelOffset.x;

        Rect panelRect = new Rect(x, panelOffset.y, panelSize.x, panelSize.y);
        GUI.Box(panelRect, string.Empty, boxStyle);
        GUI.Label(new Rect(panelRect.x + 10f, panelRect.y + 6f, panelRect.width - 20f, panelRect.height - 8f),
            BuildHealthText(),
            textStyle);
    }

    private string BuildHealthText()
    {
        int current = playerHealth.CurrentHealth;
        int max = playerHealth.MaxHealth;
        string marks = string.Empty;

        for (int i = 0; i < max; i++)
        {
            marks += i < current ? "#" : "-";
        }

        return "HP: [" + marks + "]";
    }

    private void HandleHealthChanged(int current, int max)
    {
        RefreshHealth(current, max);
    }

    private void RefreshHealth()
    {
        EnsurePlayerHealth();
        if (playerHealth == null)
        {
            RefreshHealth(0, 0);
            return;
        }

        RefreshHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void RefreshHealth(int current, int max)
    {
        if (titleText != null)
        {
            titleText.text = "HP";
        }

        if (valueText != null)
        {
            valueText.text = Mathf.Max(0, current) + " / " + Mathf.Max(0, max);
        }

        if (marksText != null)
        {
            marksText.supportRichText = true;
            marksText.text = BuildRichHealthMarks(current, max);
        }
    }

    private string BuildRichHealthMarks(int current, int max)
    {
        marksBuilder.Length = 0;
        string fullColor = ColorUtility.ToHtmlStringRGB(fullMarkColor);
        string emptyColor = ColorUtility.ToHtmlStringRGB(emptyMarkColor);

        for (int i = 0; i < max; i++)
        {
            string color = i < current ? fullColor : emptyColor;
            marksBuilder.Append("<color=#");
            marksBuilder.Append(color);
            marksBuilder.Append(">●</color>");
            if (i < max - 1)
            {
                marksBuilder.Append(" ");
            }
        }

        return marksBuilder.ToString();
    }

    private bool HasCanvasUI()
    {
        return panelRoot != null || titleText != null || valueText != null || marksText != null || panelImage != null;
    }

    private void SetCanvasVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        if (panelRoot != null && canvasGroup == null)
        {
            panelRoot.gameObject.SetActive(visible);
        }
    }

    private void SubscribeToHealth()
    {
        if (subscribedToHealth || playerHealth == null)
        {
            return;
        }

        playerHealth.OnHealthChanged += HandleHealthChanged;
        subscribedToHealth = true;
    }

    private void UnsubscribeFromHealth()
    {
        if (!subscribedToHealth || playerHealth == null)
        {
            return;
        }

        playerHealth.OnHealthChanged -= HandleHealthChanged;
        subscribedToHealth = false;
    }

    private void EnsurePlayerHealth()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }
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
            alignment = TextAnchor.MiddleLeft,
            fontSize = 14,
            wordWrap = false
        };
        textStyle.normal.textColor = Color.white;
        textStyle.hover.textColor = Color.white;
        textStyle.active.textColor = Color.white;
        textStyle.focused.textColor = Color.white;
    }

    private void OnValidate()
    {
        panelSize.x = Mathf.Max(180f, panelSize.x);
        panelSize.y = Mathf.Clamp(panelSize.y, 56f, 140f);
        panelOffset.x = Mathf.Max(0f, panelOffset.x);
        panelOffset.y = Mathf.Max(0f, panelOffset.y);
        RefreshHealth();
    }
}
