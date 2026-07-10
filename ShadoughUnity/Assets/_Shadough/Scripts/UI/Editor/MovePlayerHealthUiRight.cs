using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MovePlayerHealthUiRight
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string RequestPath = "Temp/MovePlayerHealthUiRight.request";
    private const string ReportPath = "Logs/MovePlayerHealthUiRight.report.txt";
    private static readonly Vector2 Margin = new Vector2(28f, 28f);

    [InitializeOnLoadMethod]
    private static void Register()
    {
        EditorApplication.update -= TryRunRequest;
        EditorApplication.update += TryRunRequest;
    }

    [MenuItem("Shadough/UI/Move Player Health To Top Right")]
    public static void RunFromMenu()
    {
        Run();
    }

    private static void TryRunRequest()
    {
        string request = FullPath(RequestPath);
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        File.Delete(request);
        Run();
    }

    private static void Run()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            throw new InvalidOperationException("Open ClockTower_TopdownPrototype before moving the health UI.");
        }

        GameObject panelObject = FindSceneObject(scene, "PlayerHPPanel");
        GameObject uiObject = FindSceneObject(scene, "UI");
        if (panelObject == null || uiObject == null)
        {
            throw new MissingReferenceException("PlayerHPPanel or UI was not found.");
        }

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        PlayerHealthUI healthUi = uiObject.GetComponent<PlayerHealthUI>();
        if (panel == null || healthUi == null)
        {
            throw new MissingComponentException("PlayerHPPanel RectTransform or PlayerHealthUI is missing.");
        }

        Vector2 originalSize = panel.sizeDelta;
        int originalChildCount = panel.childCount;

        Undo.RecordObject(panel, "Move Player Health UI To Top Right");
        panel.anchorMin = Vector2.one;
        panel.anchorMax = Vector2.one;
        panel.pivot = Vector2.one;
        panel.anchoredPosition = new Vector2(-Margin.x, -Margin.y);
        panel.sizeDelta = originalSize;
        EditorUtility.SetDirty(panel);

        SerializedObject healthUiObject = new SerializedObject(healthUi);
        SerializedProperty anchorTopRight = healthUiObject.FindProperty("anchorTopRight");
        if (anchorTopRight == null)
        {
            throw new MissingFieldException("PlayerHealthUI.anchorTopRight was not found.");
        }

        anchorTopRight.boolValue = true;
        healthUiObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(healthUi);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            throw new IOException("Could not save ClockTower_TopdownPrototype.");
        }

        bool layoutCorrect = panel.anchorMin == Vector2.one
            && panel.anchorMax == Vector2.one
            && panel.pivot == Vector2.one
            && panel.anchoredPosition == new Vector2(-Margin.x, -Margin.y)
            && panel.sizeDelta == originalSize
            && panel.childCount == originalChildCount;

        string report = "Move Player Health UI Right\n"
            + "Scene: " + scene.path + "\n"
            + "Object: GeneratedPlayerHUDCanvas/PlayerHPPanel\n"
            + "Anchor Min: " + panel.anchorMin + "\n"
            + "Anchor Max: " + panel.anchorMax + "\n"
            + "Pivot: " + panel.pivot + "\n"
            + "Anchored Position: " + panel.anchoredPosition + "\n"
            + "Size: " + panel.sizeDelta + "\n"
            + "Child count unchanged: " + (panel.childCount == originalChildCount ? "PASS" : "FAIL") + "\n"
            + "PlayerHealthUI anchorTopRight: " + (anchorTopRight.boolValue ? "PASS" : "FAIL") + "\n"
            + "Layout: " + (layoutCorrect ? "PASS" : "FAIL") + "\n";

        File.WriteAllText(FullPath(ReportPath), report);
        Debug.Log("Player health UI moved to the top right. Report: " + FullPath(ReportPath));
    }

    private static GameObject FindSceneObject(Scene scene, string objectName)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                if (transforms[j].name == objectName)
                {
                    return transforms[j].gameObject;
                }
            }
        }

        return null;
    }

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }
}
