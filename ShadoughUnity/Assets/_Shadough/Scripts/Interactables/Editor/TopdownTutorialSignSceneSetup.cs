using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public static class TopdownTutorialSignSceneSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string PrefabPath = "Assets/_Shadough/Prefabs/Interactables/TutorialSign_Topdown.prefab";
    private const string SignRootName = "TutorialSigns_Topdown";
    private const string AutoSetupRequestPath = "Temp/TopdownTutorialSignSetup.request";

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Topdown Tutorial Signs")]
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
            Debug.Log("Topdown tutorial sign auto setup complete.");
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
        GameObject prefab = EnsurePrefab();
        GameObject parent = EnsureSignRoot();

        CreateSign(prefab, parent.transform, "TutorialSign_01_MoveLantern", new Vector3(-5.8f, -1.4f, 0f),
            "\u79fb\u52a8\u548c\u957f\u706f",
            "WASD \u56db\u65b9\u5411\u79fb\u52a8\u3002\nG \u653e\u4e0b/\u56de\u6536\u957f\u706f\u3002");

        CreateSign(prefab, parent.transform, "TutorialSign_02_CutTreeShadow", new Vector3(-2.9f, -1.45f, 0f),
            "\u526a\u4e0b\u6811\u5f71",
            "\u5148\u653e\u4e0b\u957f\u706f\u3002\n\u6309\u4f4f Shift + E \u526a\u4e0b\u7a33\u5b9a\u7684 TreeShadow\u3002");

        CreateSign(prefab, parent.transform, "TutorialSign_03_PasteTreeShadow", new Vector3(-0.8f, 1.35f, 0f),
            "\u8d34\u51fa\u6811\u5f71",
            "F \u8d34\u51fa\u6811\u5f71\u3002\n\u8d34\u51fa\u540e\u4fdd\u6301\u526a\u4e0b\u65f6\u7684\u5f62\u72b6\u3002");

        CreateSign(prefab, parent.transform, "TutorialSign_04_PressurePlate", new Vector3(4.45f, 1.25f, 0f),
            "\u538b\u529b\u677f",
            "\u628a CanPress \u5f71\u5b50\u8d34\u5230\u538b\u529b\u677f\u4e0a\u3002\n\u95e8\u4f1a\u6253\u5f00\u3002");

        CreateSign(prefab, parent.transform, "TutorialSign_05_LockDoor", new Vector3(8.25f, 5.15f, 0f),
            "\u9501\u95e8",
            "\u628a CanUnlock \u94a5\u5319\u5f71\u8d34\u8fdb\u9501\u5b54\u533a\u57df\u3002\n\u9501\u95e8\u4f1a\u6253\u5f00\u3002");

        CreateSign(prefab, parent.transform, "TutorialSign_06_PlayerShadowLure", new Vector3(16.25f, 7.05f, 0f),
            "\u8bf1\u5f00\u5bfb\u5f71\u517d",
            "\u6309\u4f4f Shift + Q \u526a\u81ea\u5df1\u7684\u5f71\u5b50\u3002\nPlayerShadow \u53ea\u80fd\u5438\u5f15\u654c\u4eba\u3002");

        CreateSign(prefab, parent.transform, "TutorialSign_07_FinalClockCore", new Vector3(22.45f, 10.5f, 0f),
            "\u949f\u6838",
            "\u9760\u8fd1 FinalClockCore \u6309 E\u3002\n\u5b8c\u6210 Topdown v0.6 Demo Loop\u3002");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        ValidateScene();
        Debug.Log("Topdown tutorial sign setup complete.");
    }

    private static void ValidateScene()
    {
        int missingScripts = 0;
        int missingReferences = 0;
        int signCount = 0;
        int promptControllerCount = 0;

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!sceneObject.scene.IsValid() || sceneObject.scene.path != ScenePath)
            {
                continue;
            }

            if (sceneObject.GetComponent<TutorialSign>() != null)
            {
                signCount++;
            }

            if (sceneObject.GetComponent<TutorialSignPromptController>() != null)
            {
                promptControllerCount++;
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

        Debug.Log("Topdown tutorial sign validation: missingScripts=" + missingScripts
            + ", missingReferences=" + missingReferences
            + ", tutorialSigns=" + signCount
            + ", promptControllers=" + promptControllerCount);
    }

    private static void EnsurePromptController()
    {
        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UI");
        }

        if (uiRoot.GetComponent<TutorialSignPromptController>() == null)
        {
            uiRoot.AddComponent<TutorialSignPromptController>();
        }
    }

    private static GameObject EnsurePrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        GameObject signObject = new GameObject("TutorialSign_Topdown");
        signObject.transform.localScale = new Vector3(0.8f, 0.55f, 1f);

        SpriteRenderer renderer = signObject.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 35;

        BoxCollider2D collider = signObject.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

        signObject.AddComponent<TutorialSign>();

        GameObject createdPrefab = PrefabUtility.SaveAsPrefabAsset(signObject, PrefabPath);
        Object.DestroyImmediate(signObject);
        return createdPrefab;
    }

    private static GameObject EnsureSignRoot()
    {
        GameObject existingRoot = GameObject.Find(SignRootName);
        if (existingRoot != null)
        {
            Object.DestroyImmediate(existingRoot);
        }

        GameObject interactablesRoot = GameObject.Find("Interactables");
        GameObject root = new GameObject(SignRootName);
        if (interactablesRoot != null)
        {
            root.transform.SetParent(interactablesRoot.transform, false);
        }

        return root;
    }

    private static void CreateSign(GameObject prefab, Transform parent, string name, Vector3 position, string title, string body)
    {
        GameObject sign = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        sign.name = name;
        sign.transform.SetParent(parent, false);
        sign.transform.position = position;
        sign.transform.localScale = new Vector3(0.8f, 0.55f, 1f);

        TutorialSign tutorialSign = sign.GetComponent<TutorialSign>();
        tutorialSign.Configure(title, body);

        SpriteRenderer renderer = sign.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = 35;
    }
}
