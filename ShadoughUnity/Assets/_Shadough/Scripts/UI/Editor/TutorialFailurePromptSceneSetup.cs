using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TutorialFailurePromptSceneSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string AutoSetupRequestPath = "Temp/TutorialFailurePromptSetup.request";

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Tutorial Failure Prompt")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    private static void TryAutoSetup()
    {
        string requestPath = GetAutoSetupRequestFullPath();
        if (!File.Exists(requestPath))
        {
            EditorApplication.update -= TryAutoSetup;
            return;
        }

        if (EditorApplication.isCompiling)
        {
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        EditorApplication.update -= TryAutoSetup;

        try
        {
            Setup();
            File.Delete(requestPath);
            Debug.Log("Tutorial failure prompt auto setup complete.");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static string GetAutoSetupRequestFullPath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", AutoSetupRequestPath));
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        EnsurePromptController();
        EnsureLureFailureTrigger();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        ValidateScene();
        Debug.Log("Tutorial failure prompt setup complete.");
    }

    private static void EnsurePromptController()
    {
        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UI");
        }

        TutorialFailurePromptController promptController = uiRoot.GetComponent<TutorialFailurePromptController>();
        if (promptController == null)
        {
            promptController = uiRoot.AddComponent<TutorialFailurePromptController>();
        }

        SerializedObject serializedPrompt = new SerializedObject(promptController);
        serializedPrompt.FindProperty("defaultDuration").floatValue = 1.7f;
        serializedPrompt.FindProperty("panelSize").vector2Value = new Vector2(520f, 38f);
        serializedPrompt.FindProperty("bottomOffset").floatValue = 52f;
        serializedPrompt.FindProperty("hideWhileTutorialPromptOpen").boolValue = true;
        serializedPrompt.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void EnsureLureFailureTrigger()
    {
        GameObject lureArea = GameObject.Find("LureArea_Topdown");
        if (lureArea == null)
        {
            Debug.LogWarning("LureArea_Topdown not found. Shadow lure failure prompt was not attached.");
            return;
        }

        BoxCollider2D collider = lureArea.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = lureArea.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;

        SpriteRenderer renderer = lureArea.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            collider.size = renderer.size;
            collider.offset = Vector2.zero;
        }
        else
        {
            collider.size = new Vector2(1.2f, 0.6f);
            collider.offset = Vector2.zero;
        }

        ShadowLureFailureTrigger trigger = lureArea.GetComponent<ShadowLureFailureTrigger>();
        if (trigger == null)
        {
            trigger = lureArea.AddComponent<ShadowLureFailureTrigger>();
        }

        SerializedObject serializedTrigger = new SerializedObject(trigger);
        serializedTrigger.FindProperty("failureMessage").stringValue = "This shadow cannot lure the seeker.";
        serializedTrigger.FindProperty("promptCooldown").floatValue = 1.5f;
        serializedTrigger.FindProperty("overlapScanInterval").floatValue = 0.15f;
        serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ValidateScene()
    {
        int missingScripts = 0;
        int missingReferences = 0;
        int failurePromptCount = 0;
        int lureFailureTriggerCount = 0;

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!sceneObject.scene.IsValid() || sceneObject.scene.path != ScenePath)
            {
                continue;
            }

            if (sceneObject.GetComponent<TutorialFailurePromptController>() != null)
            {
                failurePromptCount++;
            }

            if (sceneObject.GetComponent<ShadowLureFailureTrigger>() != null)
            {
                lureFailureTriggerCount++;
            }

            Component[] components = sceneObject.GetComponents<Component>();
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                Component component = components[componentIndex];
                if (component == null)
                {
                    missingScripts++;
                    continue;
                }

                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty property = serializedObject.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.objectReferenceValue == null
                        && property.objectReferenceInstanceIDValue != 0)
                    {
                        missingReferences++;
                    }
                }
            }
        }

        Debug.Log("Tutorial failure prompt validation: missingScripts=" + missingScripts
            + ", missingReferences=" + missingReferences
            + ", failurePromptCount=" + failurePromptCount
            + ", lureFailureTriggerCount=" + lureFailureTriggerCount);
    }
}
