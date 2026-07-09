using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownStartAreaTuningSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string AutoSetupRequestPath = "Temp/TopdownStartAreaTuningSetup.request";
    private const string TuningRootName = "Level01_StartArea_Tuning";

    private static readonly Vector3 PlayerStartPosition = new Vector3(-14.35f, -8.80f, 0f);
    private static readonly Vector3 CameraStartOffset = new Vector3(1.15f, 0.75f, -10f);
    private const float CameraOrthographicSize = 6.55f;

    private static readonly Vector3 TreePosition = new Vector3(-14.25f, -6.75f, 0f);
    private static readonly Vector3 TreeTrunkPosition = new Vector3(-14.25f, -6.85f, 0f);
    private static readonly Vector3 TreeCanopyPosition = new Vector3(-14.25f, -6.45f, 0f);
    private static readonly Vector3 TreeShadowReadPosition = new Vector3(-12.05f, -7.38f, 0f);

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Tune Topdown Start Area")]
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
            Debug.Log("Topdown start area tuning auto setup complete.");
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

        TunePlayerStart();
        TuneStartTeachingObjects();
        TuneInitialCamera();
        RebuildStartTuningVisuals();
        ValidateScene();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Topdown start area tuning setup complete.");
    }

    private static void TunePlayerStart()
    {
        GameObject player = GameObject.Find("Player_Topdown");
        if (player == null)
        {
            Debug.LogWarning("Player_Topdown not found. Start position was not tuned.");
            return;
        }

        player.transform.position = PlayerStartPosition;
        player.transform.rotation = Quaternion.identity;

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.position = PlayerStartPosition;
        }
    }

    private static void TuneStartTeachingObjects()
    {
        MoveObject("Tree_01", TreePosition, new Vector3(1.18f, 1.18f, 1f));
        MoveObject("Tree_Trunk", TreeTrunkPosition, new Vector3(0.52f, 1.05f, 1f));
        MoveObject("Tree_Canopy", TreeCanopyPosition, new Vector3(1.42f, 1.08f, 1f));

        GameObject treeShadow = MoveObject("TreeShadow_Topdown", TreeShadowReadPosition, Vector3.one);
        if (treeShadow != null)
        {
            treeShadow.transform.rotation = Quaternion.Euler(0f, 0f, -10f);

            SpriteRenderer renderer = treeShadow.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = new Vector2(3.9f, 0.42f);
                renderer.sortingOrder = 8;
            }

            BoxCollider2D collider = treeShadow.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = new Vector2(3.9f, 0.42f);
                collider.offset = Vector2.zero;
                collider.isTrigger = true;
            }
        }

        GameObject crossingHint = GameObject.Find("CrossingHint_01");
        if (crossingHint != null)
        {
            crossingHint.transform.position = new Vector3(-11.85f, -7.35f, crossingHint.transform.position.z);
        }
    }

    private static void TuneInitialCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = GameObject.Find("Main Camera");
            camera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;
        }

        if (camera == null)
        {
            Debug.LogWarning("Main Camera not found. Initial camera was not tuned.");
            return;
        }

        camera.orthographic = true;
        camera.orthographicSize = CameraOrthographicSize;
        camera.transform.position = PlayerStartPosition + CameraStartOffset;

        TopdownCameraFollow follow = camera.GetComponent<TopdownCameraFollow>();
        if (follow != null)
        {
            SerializedObject serializedFollow = new SerializedObject(follow);
            SetObject(serializedFollow, "target", GameObject.Find("Player_Topdown") != null ? GameObject.Find("Player_Topdown").transform : null);
            SetVector3(serializedFollow, "offset", CameraStartOffset);
            SetFloat(serializedFollow, "followSmoothTime", 0.12f);
            SetBool(serializedFollow, "snapOnStart", true);
            serializedFollow.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void RebuildStartTuningVisuals()
    {
        GameObject existing = GameObject.Find(TuningRootName);
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        GameObject world = GameObject.Find("World");
        GameObject root = new GameObject(TuningRootName);
        if (world != null)
        {
            root.transform.SetParent(world.transform, false);
        }

        CreateBox(root.transform, "Start_Player_Buffer", new Vector2(-14.35f, -8.80f), new Vector2(2.85f, 2.05f), new Color(0.38f, 0.58f, 0.30f, 0.36f), 3);
        CreateBox(root.transform, "Start_To_River_Readable_Path", new Vector2(-13.25f, -8.02f), new Vector2(1.85f, 2.45f), new Color(0.38f, 0.58f, 0.30f, 0.28f), 3);
        CreateBox(root.transform, "TreeShadow_Target_Guide", new Vector2(-11.95f, -7.42f), new Vector2(4.25f, 0.70f), new Color(0.58f, 0.34f, 0.88f, 0.35f), 4);
    }

    private static void ValidateScene()
    {
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

        bool cameraShowsStart = CameraBoundsContain(new Vector2(-16.98f, -11.23f))
            && CameraBoundsContain(new Vector2(-10.48f, -5.97f))
            && CameraBoundsContain(new Vector2(-9.50f, -7.50f));

        Debug.Log("Topdown start area tuning validation: missingScripts=" + missingScripts
            + ", missingReferences=" + missingReferences
            + ", cameraShowsStart=" + cameraShowsStart
            + ", playerStart=" + PlayerStartPosition
            + ", cameraSize=" + CameraOrthographicSize);
    }

    private static bool CameraBoundsContain(Vector2 point)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return false;
        }

        float halfHeight = camera.orthographicSize;
        float halfWidth = halfHeight * camera.aspect;
        Vector3 center = camera.transform.position;
        return point.x >= center.x - halfWidth
            && point.x <= center.x + halfWidth
            && point.y >= center.y - halfHeight
            && point.y <= center.y + halfHeight;
    }

    private static GameObject MoveObject(string name, Vector3 position, Vector3 scale)
    {
        GameObject gameObject = GameObject.Find(name);
        if (gameObject == null)
        {
            return null;
        }

        gameObject.transform.position = new Vector3(position.x, position.y, gameObject.transform.position.z);
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localScale = scale;
        return gameObject;
    }

    private static void CreateBox(Transform parent, string name, Vector2 center, Vector2 size, Color color, int sortingOrder)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = CreateMaterial(color);
        meshRenderer.sortingOrder = sortingOrder;

        Vector2 half = size * 0.5f;
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(center.x - half.x, center.y - half.y, 0f),
            new Vector3(center.x + half.x, center.y - half.y, 0f),
            new Vector3(center.x + half.x, center.y + half.y, 0f),
            new Vector3(center.x - half.x, center.y + half.y, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateBounds();
        meshFilter.sharedMesh = mesh;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        return material;
    }

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.vector3Value = value;
        }
    }

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
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
