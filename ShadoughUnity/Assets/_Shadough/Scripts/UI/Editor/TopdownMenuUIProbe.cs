using System.IO;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class TopdownMenuUIProbe
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string RequestPath = "Temp/TopdownMenuUIProbe.request";
    private const string ReportPath = "Logs/TopdownMenuUIProbe.report.txt";

    private static StringBuilder report;
    private static int stage;
    private static double nextStageTime;

    [MenuItem("Shadough/Run Topdown Menu UI Probe")]
    public static void RequestProbeFromMenu()
    {
        File.WriteAllText(FullPath(RequestPath), "menu-ui-probe");
    }

    public static void RunProbeBatch()
    {
        File.WriteAllText(FullPath(RequestPath), "menu-ui-probe");
        EditorApplication.update -= TryAutoProbeUpdate;
        EditorApplication.update += TryAutoProbeUpdate;
        TryAutoProbeUpdate();
    }

    [InitializeOnLoadMethod]
    private static void TryAutoProbe()
    {
        EditorApplication.update -= TryAutoProbeUpdate;
        EditorApplication.update += TryAutoProbeUpdate;
    }

    private static void TryAutoProbeUpdate()
    {
        string requestPath = FullPath(RequestPath);
        if (!File.Exists(requestPath))
        {
            return;
        }

        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
            return;
        }

        if (EditorApplication.isPlaying && report == null)
        {
            StartProbe();
        }
    }

    private static void StartProbe()
    {
        report = new StringBuilder();
        report.AppendLine("Topdown Menu UI Probe");
        report.AppendLine("Scene: " + ScenePath);
        stage = 0;
        nextStageTime = EditorApplication.timeSinceStartup + 0.75d;
        EditorApplication.update -= RunProbeStep;
        EditorApplication.update += RunProbeStep;
    }

    private static void RunProbeStep()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update -= RunProbeStep;
            report = null;
            return;
        }

        if (EditorApplication.timeSinceStartup < nextStageTime)
        {
            return;
        }

        TopdownMenuUIController controller = Object.FindObjectOfType<TopdownMenuUIController>(true);
        switch (stage)
        {
            case 0:
                report.AppendLine("Main menu appears on launch: " + PassFail(controller != null && !controller.IsGameStarted && Time.timeScale == 0f && IsGroupVisible("MainMenuCanvas")));
                report.AppendLine("Correct start screen image: " + PassFail(FindSceneObject("StartScreen_TitleBase") != null));
                report.AppendLine("Main menu fully covers gameplay: " + PassFail(FindSceneObject("Opaque_Black_Backdrop") != null && FindSceneObject("StartScreen_TitleBase") != null));
                report.AppendLine("Start buttons cropped separately: " + PassFail(HasButtonSprite("Button_StartGame") && HasButtonSprite("Button_HowToPlay") && HasButtonSprite("Button_QuitGame")));
                report.AppendLine("Start menu buttons: " + PassFail(FindButton("Button_StartGame") != null && FindButton("Button_HowToPlay") != null && FindButton("Button_QuitGame") != null));
                report.AppendLine("No missing glyph text source: " + PassFail(AllMenuTextIsAscii()));
                report.AppendLine("Gameplay input blocked before START GAME: " + PassFail(IsGameplayBlocked()));
                report.AppendLine("Shadow Reveal sound blocked before START GAME: " + PassFail(IsRevealBlocked()));
                Button howTo = FindButton("Button_HowToPlay");
                howTo?.onClick.Invoke();
                Advance(1, 0.35d);
                break;
            case 1:
                report.AppendLine("How to play panel opens: " + PassFail(IsGroupVisible("HowToPlayPanel")));
                report.AppendLine("Detailed How To Play: " + PassFail(HasText("Move and Light") && HasText("Cut and Paste Shadows") && HasText("Shadow Abilities") && HasText("Shape the shadow. Cross the impossible.")));
                Button back = FindButton("Button_Back");
                back?.onClick.Invoke();
                Advance(2, 0.35d);
                break;
            case 2:
                report.AppendLine("How to play panel returns: " + PassFail(!IsGroupVisible("HowToPlayPanel") && IsGroupVisible("MainMenuCanvas")));
                Button start = FindButton("Button_StartGame");
                start?.onClick.Invoke();
                Advance(3, 0.45d);
                break;
            case 3:
                report.AppendLine("Start button enters gameplay: " + PassFail(controller != null && controller.IsGameStarted && Time.timeScale == 1f && !IsGroupVisible("MainMenuCanvas")));
                report.AppendLine("START GAME enables gameplay: " + PassFail(!IsGameplayBlocked()));
                report.AppendLine("In-game pause button: " + PassFail(IsGroupVisible("GameplayHudCanvas") && FindButton("Button_GameplayPause") != null));
                Button gameplayPause = FindButton("Button_GameplayPause");
                gameplayPause?.onClick.Invoke();
                Advance(4, 0.35d);
                break;
            case 4:
                report.AppendLine("Pause menu opens with Esc: " + PassFail(controller != null && controller.IsPauseOpen && Time.timeScale == 0f && IsGroupVisible("PauseMenuCanvas")));
                report.AppendLine("Pause menu uses provided background: " + PassFail(FindSceneObject("Pause_Background") != null));
                report.AppendLine("Pause blocks gameplay time: " + PassFail(Time.timeScale == 0f));
                report.AppendLine("Gameplay input blocked while paused: " + PassFail(IsGameplayBlocked()));
                Button pauseHowTo = FindButton("Button_HowTo");
                pauseHowTo?.onClick.Invoke();
                Advance(5, 0.35d);
                break;
            case 5:
                report.AppendLine("Pause how to panel opens: " + PassFail(IsGroupVisible("HowToPlayPanel")));
                report.AppendLine("Pause menu How To Play: " + PassFail(HasText("Tree Shadow") && HasText("Player Shadow")));
                Button pauseBack = FindButton("Button_Back");
                pauseBack?.onClick.Invoke();
                Advance(6, 0.35d);
                break;
            case 6:
                Button resume = FindButton("Button_Resume");
                resume?.onClick.Invoke();
                Advance(7, 0.35d);
                break;
            case 7:
                report.AppendLine("Resume button: " + PassFail(controller != null && !controller.IsPauseOpen && Time.timeScale == 1f && !IsGroupVisible("PauseMenuCanvas")));
                report.AppendLine("Resume restores gameplay input: " + PassFail(!IsGameplayBlocked()));
                controller?.OpenPauseMenu();
                Advance(8, 0.25d);
                break;
            case 8:
                Button restart = FindButton("Button_Restart");
                restart?.onClick.Invoke();
                Advance(9, 0.85d);
                break;
            case 9:
                controller = Object.FindObjectOfType<TopdownMenuUIController>(true);
                report.AppendLine("Restart button: " + PassFail(controller != null && controller.IsGameStarted && Time.timeScale == 1f && !IsGroupVisible("MainMenuCanvas")));
                controller?.OpenPauseMenu();
                Advance(10, 0.25d);
                break;
            case 10:
                Button returnMain = FindButton("Button_ReturnMain");
                returnMain?.onClick.Invoke();
                Advance(11, 0.85d);
                break;
            case 11:
                controller = Object.FindObjectOfType<TopdownMenuUIController>(true);
                report.AppendLine("Return to main menu: " + PassFail(controller != null && !controller.IsGameStarted && Time.timeScale == 0f && IsGroupVisible("MainMenuCanvas")));
                report.AppendLine("Return main blocks gameplay input: " + PassFail(IsGameplayBlocked()));
                report.AppendLine("Quit button configured: " + PassFail(FindButton("Button_QuitGame") != null && FindButton("Button_Quit") != null));
                FinishProbe();
                break;
        }
    }

    private static void Advance(int nextStage, double delay)
    {
        stage = nextStage;
        nextStageTime = EditorApplication.timeSinceStartup + delay;
    }

    private static bool IsGroupVisible(string objectName)
    {
        GameObject obj = FindSceneObject(objectName);
        CanvasGroup group = obj != null ? obj.GetComponent<CanvasGroup>() : null;
        return obj != null && obj.activeInHierarchy && group != null && group.alpha > 0.5f;
    }

    private static Button FindButton(string objectName)
    {
        GameObject obj = FindSceneObject(objectName);
        return obj != null ? obj.GetComponent<Button>() : null;
    }

    private static bool HasButtonLabel(string buttonName, string expectedLabel)
    {
        GameObject obj = FindSceneObject(buttonName);
        TMP_Text label = obj != null ? obj.GetComponentInChildren<TMP_Text>(true) : null;
        return label != null && label.text == expectedLabel;
    }

    private static bool HasButtonSprite(string buttonName)
    {
        GameObject obj = FindSceneObject(buttonName);
        Image image = obj != null ? obj.GetComponent<Image>() : null;
        TopdownMenuButtonAnimator animator = obj != null ? obj.GetComponent<TopdownMenuButtonAnimator>() : null;
        if (image == null || image.sprite == null || animator == null)
        {
            return false;
        }

        SerializedObject serializedAnimator = new SerializedObject(animator);
        SerializedProperty labelProperty = serializedAnimator.FindProperty("label");
        return labelProperty != null && labelProperty.objectReferenceValue == null;
    }

    private static bool HasText(string expectedText)
    {
        TMP_Text[] labels = Object.FindObjectsOfType<TMP_Text>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i] != null && labels[i].text.Contains(expectedText))
            {
                return true;
            }
        }

        return false;
    }

    private static bool AllMenuTextIsAscii()
    {
        GameObject root = FindSceneObject("UIRoot_TopdownMenu");
        if (root == null)
        {
            return false;
        }

        TMP_Text[] labels = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            string text = labels[i].text;
            for (int charIndex = 0; charIndex < text.Length; charIndex++)
            {
                if (text[charIndex] > 127)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsGameplayBlocked()
    {
        TopDownPlayerController playerController = Object.FindObjectOfType<TopDownPlayerController>(true);
        ShadowStatusUI shadowStatus = Object.FindObjectOfType<ShadowStatusUI>(true);
        return (playerController == null || !playerController.enabled)
            && (shadowStatus == null || !shadowStatus.enabled);
    }

    private static bool IsRevealBlocked()
    {
        RevealViewController reveal = Object.FindObjectOfType<RevealViewController>(true);
        return reveal == null || (!reveal.enabled && !reveal.IsRevealActive);
    }

    private static GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject obj = objects[i];
            if (obj != null && obj.scene.IsValid() && obj.scene.path == ScenePath && obj.name == objectName)
            {
                return obj;
            }
        }

        return null;
    }

    private static void FinishProbe()
    {
        EditorApplication.update -= RunProbeStep;
        string requestPath = FullPath(RequestPath);
        if (File.Exists(requestPath))
        {
            File.Delete(requestPath);
        }

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown menu UI probe complete. Report: " + FullPath(ReportPath));
        report = null;
        Time.timeScale = 1f;

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
            return;
        }

        EditorApplication.isPlaying = false;
    }

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }

    private static string PassFail(bool passed)
    {
        return passed ? "PASS" : "FAIL";
    }
}
