using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private KeyCode menuKey = KeyCode.Escape;
    [SerializeField] private bool isMenuOpen;

    private float previousTimeScale = 1f;
    private bool showIntro;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle buttonStyle;

    private void Update()
    {
        if (TutorialSignPromptController.BlocksPauseMenuThisFrame)
        {
            return;
        }

        if (Input.GetKeyDown(menuKey))
        {
            SetMenuOpen(!isMenuOpen);
        }
    }

    private void OnGUI()
    {
        if (!isMenuOpen)
        {
            return;
        }

        EnsureStyles();

        float panelWidth = Mathf.Min(460f, Screen.width - 48f);
        float panelHeight = Mathf.Min(showIntro ? 430f : 286f, Screen.height - 48f);
        Rect panelRect = new Rect(
            (Screen.width - panelWidth) * 0.5f,
            (Screen.height - panelHeight) * 0.5f,
            panelWidth,
            panelHeight);

        GUI.Box(panelRect, string.Empty);

        float x = panelRect.x + 28f;
        float y = panelRect.y + 24f;
        float width = panelRect.width - 56f;

        GUI.Label(new Rect(x, y, width, 36f), "Shadough", titleStyle);
        y += 48f;

        if (GUI.Button(new Rect(x, y, width, 34f), "\u73a9\u6cd5\u7b80\u4ecb", buttonStyle))
        {
            showIntro = !showIntro;
        }

        y += 44f;

        if (GUI.Button(new Rect(x, y, width, 34f), "\u7ee7\u7eed\u6e38\u620f", buttonStyle))
        {
            SetMenuOpen(false);
        }

        y += 44f;

        if (GUI.Button(new Rect(x, y, width, 34f), "\u91cd\u65b0\u6e38\u620f", buttonStyle))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        y += 44f;

        if (showIntro)
        {
            string intro =
                "WASD \u4e0a\u4e0b\u5de6\u53f3\u79fb\u52a8\n" +
                "Shift \u8fdb\u5165\u63ed\u5f71\n" +
                "G \u653e\u4e0b/\u56de\u6536\u706f\u7b3c\n" +
                "E \u526a\u5f71\n" +
                "Q \u526a\u81ea\u5df1\u5f71";

            GUI.Label(new Rect(x, y, width, 122f), intro, bodyStyle);
        }
    }

    private void SetMenuOpen(bool open)
    {
        if (isMenuOpen == open)
        {
            return;
        }

        isMenuOpen = open;

        if (isMenuOpen)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            return;
        }

        showIntro = false;
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
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
            fontSize = 26,
            fontStyle = FontStyle.Bold
        };

        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            fontSize = 16,
            wordWrap = true
        };

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 16
        };
    }

    private void OnDisable()
    {
        if (isMenuOpen)
        {
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
            isMenuOpen = false;
            showIntro = false;
        }
    }
}
