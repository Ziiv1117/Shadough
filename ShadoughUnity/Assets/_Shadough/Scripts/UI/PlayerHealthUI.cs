using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Vector2 panelOffset = new Vector2(14f, 14f);
    [SerializeField] private Vector2 panelSize = new Vector2(150f, 32f);
    [SerializeField] private bool anchorTopRight = true;
    [SerializeField] private bool hideWhileTutorialPromptOpen = true;

    private GUIStyle boxStyle;
    private GUIStyle textStyle;

    private void Awake()
    {
        EnsurePlayerHealth();
    }

    private void OnGUI()
    {
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
        panelSize.x = Mathf.Max(110f, panelSize.x);
        panelSize.y = Mathf.Clamp(panelSize.y, 28f, 40f);
        panelOffset.x = Mathf.Max(0f, panelOffset.x);
        panelOffset.y = Mathf.Max(0f, panelOffset.y);
    }
}
