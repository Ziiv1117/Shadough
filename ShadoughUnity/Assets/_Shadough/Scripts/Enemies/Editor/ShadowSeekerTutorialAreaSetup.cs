using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ShadowSeekerTutorialAreaSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string AutoSetupRequestPath = "Temp/ShadowSeekerTutorialAreaSetup.request";

    private static readonly Vector3 RecommendedLurePosition = new Vector3(15.6f, 7.15f, 0f);
    private static readonly Vector3 RecommendedLureScale = new Vector3(1.25f, 0.75f, 1f);
    private static readonly Color RecommendedLureColor = new Color(0.45f, 0.72f, 1f, 0.45f);

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Shadow Seeker Tutorial Area")]
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
            Debug.Log("ShadowSeeker tutorial area auto setup complete.");
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

        ConfigureShadowSeeker();
        ConfigureLureArea();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        ValidateScene();
    }

    private static void ConfigureShadowSeeker()
    {
        GameObject seekerObject = GameObject.Find("ShadowSeeker_Topdown");
        if (seekerObject == null)
        {
            Debug.LogWarning("ShadowSeeker_Topdown not found. ShadowSeeker tutorial area was not configured.");
            return;
        }

        EnemyShadowSeeker seeker = seekerObject.GetComponent<EnemyShadowSeeker>();
        if (seeker == null)
        {
            Debug.LogWarning("ShadowSeeker_Topdown has no EnemyShadowSeeker component.");
            return;
        }

        SerializedObject serializedSeeker = new SerializedObject(seeker);
        SetFloat(serializedSeeker, "detectionRadius", 3.8f);
        SetFloat(serializedSeeker, "attackDistance", 0.6f);
        SetFloat(serializedSeeker, "attackCooldown", 1.5f);
        SetString(serializedSeeker, "playerTag", "Player");
        SetBool(serializedSeeker, "showDebugGizmos", true);
        serializedSeeker.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ConfigureLureArea()
    {
        GameObject lureArea = GameObject.Find("LureArea_Topdown");
        if (lureArea == null)
        {
            Debug.LogWarning("LureArea_Topdown not found. Recommended lure point was not configured.");
            return;
        }

        lureArea.transform.position = RecommendedLurePosition;
        lureArea.transform.localScale = RecommendedLureScale;

        SpriteRenderer renderer = lureArea.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = RecommendedLureColor;
            renderer.sortingOrder = 4;
        }

        BoxCollider2D collider = lureArea.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private static void ValidateScene()
    {
        GameObject seekerObject = GameObject.Find("ShadowSeeker_Topdown");
        GameObject lureArea = GameObject.Find("LureArea_Topdown");
        EnemyShadowSeeker seeker = seekerObject != null ? seekerObject.GetComponent<EnemyShadowSeeker>() : null;

        float lureDistance = seekerObject != null && lureArea != null
            ? Vector2.Distance(seekerObject.transform.position, lureArea.transform.position)
            : -1f;

        int missingScripts = 0;
        int missingReferences = 0;
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!sceneObject.scene.IsValid() || sceneObject.scene.path != ScenePath)
            {
                continue;
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

        Debug.Log("ShadowSeeker tutorial validation: detectionRadius="
            + (seeker != null ? seeker.DetectionRadius.ToString("0.00") : "missing")
            + ", lureDistance=" + lureDistance.ToString("0.00")
            + ", missingScripts=" + missingScripts
            + ", missingReferences=" + missingReferences);
    }

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void SetString(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }
}
