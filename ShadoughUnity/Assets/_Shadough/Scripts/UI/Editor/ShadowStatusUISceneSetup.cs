using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ShadowStatusUISceneSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string AutoSetupRequestPath = "Temp/ShadowStatusUISetup.request";

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Shadow Status UI")]
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
            Debug.Log("Shadow Status UI auto setup complete.");
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

        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UI");
        }

        ShadowStatusUI statusUI = uiRoot.GetComponent<ShadowStatusUI>();
        if (statusUI == null)
        {
            statusUI = uiRoot.AddComponent<ShadowStatusUI>();
        }

        ShadowInventory inventory = Object.FindObjectOfType<ShadowInventory>();
        SerializedObject serializedStatus = new SerializedObject(statusUI);
        serializedStatus.FindProperty("inventory").objectReferenceValue = inventory;
        serializedStatus.FindProperty("panelOffset").vector2Value = new Vector2(14f, 14f);
        serializedStatus.FindProperty("panelSize").vector2Value = new Vector2(360f, 32f);
        serializedStatus.FindProperty("hideWhileTutorialPromptOpen").boolValue = true;
        serializedStatus.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        ValidateScene();
        Debug.Log("Shadow Status UI setup complete.");
    }

    private static void ValidateScene()
    {
        int missingScripts = 0;
        int missingReferences = 0;
        int statusUICount = 0;
        int inventoryCount = 0;

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!sceneObject.scene.IsValid() || sceneObject.scene.path != ScenePath)
            {
                continue;
            }

            if (sceneObject.GetComponent<ShadowStatusUI>() != null)
            {
                statusUICount++;
            }

            if (sceneObject.GetComponent<ShadowInventory>() != null)
            {
                inventoryCount++;
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

        Debug.Log("Shadow Status UI validation: missingScripts=" + missingScripts
            + ", missingReferences=" + missingReferences
            + ", statusUICount=" + statusUICount
            + ", inventoryCount=" + inventoryCount);
    }
}
