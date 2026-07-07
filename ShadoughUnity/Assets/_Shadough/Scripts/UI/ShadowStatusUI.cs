using System.Text;
using UnityEngine;

public class ShadowStatusUI : MonoBehaviour
{
    [SerializeField] private ShadowInventory inventory;
    [SerializeField] private Vector2 panelOffset = new Vector2(14f, 14f);
    [SerializeField] private Vector2 panelSize = new Vector2(360f, 32f);
    [SerializeField] private bool hideWhileTutorialPromptOpen = true;

    private readonly StringBuilder textBuilder = new StringBuilder(160);
    private GUIStyle boxStyle;
    private GUIStyle textStyle;

    private void Awake()
    {
        EnsureInventory();
    }

    private void OnGUI()
    {
        if (hideWhileTutorialPromptOpen && TutorialSignPromptController.IsPromptOpen)
        {
            return;
        }

        EnsureInventory();
        EnsureStyles();

        Rect panelRect = new Rect(
            panelOffset.x,
            Screen.height - panelSize.y - panelOffset.y,
            panelSize.x,
            panelSize.y);

        GUI.Box(panelRect, string.Empty, boxStyle);
        GUI.Label(new Rect(panelRect.x + 10f, panelRect.y + 6f, panelRect.width - 20f, panelRect.height - 8f),
            BuildStatusText(),
            textStyle);
    }

    private string BuildStatusText()
    {
        textBuilder.Length = 0;

        if (inventory == null || !inventory.HasShadow())
        {
            textBuilder.Append("No Shadow");
            return textBuilder.ToString();
        }

        ShadowItemData data = inventory.CurrentShadowData;
        string shadowName = !string.IsNullOrEmpty(data.displayName) ? data.displayName : data.shadowType.ToString();
        textBuilder.Append(shadowName);
        textBuilder.Append(" | ");

        bool hasAbility = false;
        AppendAbility(data.canStandOn, "CanStandOn", ref hasAbility);
        AppendAbility(data.canPress, "CanPress", ref hasAbility);
        AppendAbility(data.canUnlock, "CanUnlock", ref hasAbility);
        AppendAbility(data.canAttractEnemy, "CanAttractEnemy", ref hasAbility);
        AppendAbility(data.canBlock, "CanBlock", ref hasAbility);

        if (!hasAbility)
        {
            textBuilder.Append("None");
        }

        return textBuilder.ToString();
    }

    private void AppendAbility(bool enabled, string abilityName, ref bool hasAbility)
    {
        if (!enabled)
        {
            return;
        }

        if (hasAbility)
        {
            textBuilder.Append(", ");
        }

        textBuilder.Append(abilityName);
        hasAbility = true;
    }

    private void EnsureInventory()
    {
        if (inventory != null)
        {
            return;
        }

        inventory = FindObjectOfType<ShadowInventory>();
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
    }

    private void OnValidate()
    {
        panelSize.x = Mathf.Max(160f, panelSize.x);
        panelSize.y = Mathf.Clamp(panelSize.y, 28f, 40f);
        panelOffset.x = Mathf.Max(0f, panelOffset.x);
        panelOffset.y = Mathf.Max(0f, panelOffset.y);
    }
}
