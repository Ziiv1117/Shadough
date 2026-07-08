using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ShadowRecallSceneSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string AutoSetupRequestPath = "Temp/ShadowRecallSetup.request";

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Shadow Recall")]
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
            Debug.Log("Shadow recall auto setup complete.");
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

        GameObject player = GameObject.Find("Player_Topdown");
        if (player == null)
        {
            Debug.LogWarning("Player_Topdown not found. Shadow recall was not attached.");
            return;
        }

        ShadowInventory inventory = player.GetComponent<ShadowInventory>();
        if (inventory == null)
        {
            Debug.LogWarning("Player_Topdown has no ShadowInventory. Shadow recall was not attached.");
            return;
        }

        ShadowRecallController recallController = player.GetComponent<ShadowRecallController>();
        if (recallController == null)
        {
            recallController = player.AddComponent<ShadowRecallController>();
        }

        SerializedObject serializedRecall = new SerializedObject(recallController);
        serializedRecall.FindProperty("inventory").objectReferenceValue = inventory;
        serializedRecall.FindProperty("recallKey").intValue = (int)KeyCode.R;
        serializedRecall.FindProperty("noShadowMessage").stringValue = "No shadow to recall.";
        serializedRecall.FindProperty("recalledMessage").stringValue = "Shadow recalled.";
        serializedRecall.FindProperty("abandonedMessage").stringValue = "Shadow returned.";
        serializedRecall.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        ValidateScene();
        Debug.Log("Shadow recall setup complete.");
    }

    private static void ValidateScene()
    {
        int missingScripts = 0;
        int missingReferences = 0;
        int recallControllerCount = 0;
        int inventoryCount = 0;

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!sceneObject.scene.IsValid() || sceneObject.scene.path != ScenePath)
            {
                continue;
            }

            if (sceneObject.GetComponent<ShadowRecallController>() != null)
            {
                recallControllerCount++;
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

        Debug.Log("Shadow recall validation: missingScripts=" + missingScripts
            + ", missingReferences=" + missingReferences
            + ", recallControllerCount=" + recallControllerCount
            + ", inventoryCount=" + inventoryCount);
    }
}
