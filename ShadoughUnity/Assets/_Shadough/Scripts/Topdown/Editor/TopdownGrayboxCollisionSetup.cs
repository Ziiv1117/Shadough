using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownGrayboxCollisionSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string RequestPath = "Temp/TopdownGrayboxCollisionSetup.request";
    private const string ReportPath = "Logs/TopdownGrayboxCollisionSetup.report.txt";
    private const string RootName = "Graybox_Collision_Topdown";
    private const float CanvasWidth = 1448f;
    private const float CanvasHeight = 1086f;
    private const float PixelsPerUnit = 40f;
    private static readonly Color DebugWallColor = new Color(0.88f, 0.76f, 0.22f, 0.58f);

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Topdown Graybox Collision")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    private static void TryAutoSetup()
    {
        if (EditorApplication.isCompiling)
        {
            return;
        }

        string requestPath = FullPath(RequestPath);
        if (!File.Exists(requestPath))
        {
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        File.Delete(requestPath);
        Setup();
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Graybox Collision Setup");
        report.AppendLine("Scene: " + ScenePath);
        report.AppendLine("root=" + RootName);
        report.AppendLine("coordinateRule=worldX=(pixelX-width/2)/PPU, worldY=(height/2-pixelY)/PPU");
        report.AppendLine("canvas=" + CanvasWidth + "x" + CanvasHeight + ", ppu=" + PixelsPerUnit);

        Transform root = RecreateRoot();
        int created = 0;
        created += CreateStartIslandAndRiver(root);
        created += CreateEastBankAndRuinApproach(root);
        created += CreateLowerRoom(root);
        created += CreateConnectorAndUpperRoom(root);
        created += CreateSeekerCorridor(root);
        created += CreateFinalChamber(root);

        ValidateScene(report, created);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown graybox collision setup complete. Report: " + FullPath(ReportPath));
    }

    private static Transform RecreateRoot()
    {
        DestroySceneObject(RootName);

        GameObject world = EnsureRoot("World");
        GameObject root = new GameObject(RootName);
        root.transform.SetParent(world.transform, false);
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        root.layer = 0;
        return root.transform;
    }

    private static int CreateStartIslandAndRiver(Transform root)
    {
        Transform parent = CreateChild(root, "StartIsland_And_River_Edges");
        float shore = 0.46f;
        float river = 0.42f;

        CreateSegment(parent, "Graybox_StartIsland_West", 62f, 902f, 92f, 982f, shore);
        CreateSegment(parent, "Graybox_StartIsland_South", 92f, 986f, 226f, 996f, shore);
        CreateSegment(parent, "Graybox_StartIsland_East", 292f, 876f, 258f, 966f, shore);

        CreateSegment(parent, "Graybox_River_WestBank", 84f, 798f, 184f, 760f, river);
        CreateSegment(parent, "Graybox_River_EastBank", 305f, 782f, 430f, 832f, river);
        CreateSegment(parent, "Graybox_River_SouthBank", 188f, 918f, 294f, 952f, river);

        return 6;
    }

    private static int CreateEastBankAndRuinApproach(Transform root)
    {
        Transform parent = CreateChild(root, "EastBank_And_FirstRuin_Edges");
        float edge = 0.36f;

        CreateSegment(parent, "Graybox_EastBank_NorthWest", 232f, 735f, 355f, 705f, edge);
        CreateSegment(parent, "Graybox_EastBank_NorthEast", 355f, 705f, 460f, 742f, edge);
        CreateSegment(parent, "Graybox_EastBank_East", 460f, 742f, 497f, 812f, edge);
        CreateSegment(parent, "Graybox_EastBank_SouthEast", 497f, 812f, 448f, 870f, edge);

        return 4;
    }

    private static int CreateLowerRoom(Transform root)
    {
        Transform parent = CreateChild(root, "LowerRoom_Walls");
        float wall = 0.35f;

        CreateSegment(parent, "Graybox_LowerRoom_NorthWest", 322f, 612f, 438f, 552f, wall);
        CreateSegment(parent, "Graybox_LowerRoom_East", 628f, 624f, 612f, 718f, wall);
        CreateSegment(parent, "Graybox_LowerRoom_SouthEast", 566f, 780f, 482f, 790f, wall);
        CreateSegment(parent, "Graybox_LowerRoom_West", 310f, 694f, 318f, 636f, wall);

        return 4;
    }

    private static int CreateConnectorAndUpperRoom(Transform root)
    {
        Transform parent = CreateChild(root, "Connector_And_UpperRoom_Walls");
        float connector = 0.24f;
        float wall = 0.35f;

        CreateSegment(parent, "Graybox_Connector_LowerLeft", 522f, 604f, 592f, 646f, connector);
        CreateSegment(parent, "Graybox_Connector_UpperRight", 612f, 502f, 688f, 548f, connector);

        CreateSegment(parent, "Graybox_UpperRoom_NorthWest", 542f, 390f, 690f, 342f, wall);
        CreateSegment(parent, "Graybox_UpperRoom_NorthEast", 713f, 348f, 815f, 386f, wall);
        CreateSegment(parent, "Graybox_UpperRoom_East", 846f, 410f, 846f, 492f, wall);
        CreateSegment(parent, "Graybox_UpperRoom_West", 530f, 438f, 560f, 520f, wall);

        return 6;
    }

    private static int CreateSeekerCorridor(Transform root)
    {
        Transform parent = CreateChild(root, "SeekerCorridor_Walls");
        float wall = 0.30f;

        CreateSegment(parent, "Graybox_SeekerCorridor_NorthA", 770f, 332f, 888f, 276f, wall);
        CreateSegment(parent, "Graybox_SeekerCorridor_NorthB", 914f, 276f, 1002f, 280f, wall);
        CreateSegment(parent, "Graybox_SeekerCorridor_NorthC", 1058f, 248f, 1176f, 258f, wall);
        CreateSegment(parent, "Graybox_SeekerCorridor_SouthB", 922f, 408f, 1018f, 350f, wall);
        CreateSegment(parent, "Graybox_SeekerCorridor_SouthC", 1120f, 382f, 1200f, 340f, wall);

        return 5;
    }

    private static int CreateFinalChamber(Transform root)
    {
        Transform parent = CreateChild(root, "FinalChamber_Walls");
        float wall = 0.35f;

        CreateSegment(parent, "Graybox_Final_NorthWest", 1178f, 102f, 1230f, 66f, wall);
        CreateSegment(parent, "Graybox_Final_North", 1230f, 66f, 1348f, 68f, wall);
        CreateSegment(parent, "Graybox_Final_NorthEast", 1348f, 68f, 1398f, 108f, wall);
        CreateSegment(parent, "Graybox_Final_East", 1398f, 108f, 1390f, 212f, wall);
        CreateSegment(parent, "Graybox_Final_SouthEast", 1390f, 212f, 1332f, 260f, wall);
        CreateSegment(parent, "Graybox_Final_South", 1332f, 260f, 1230f, 255f, wall);
        CreateSegment(parent, "Graybox_Final_West", 1160f, 204f, 1176f, 112f, wall);

        return 7;
    }

    private static void CreateSegment(Transform parent, string name, float startPixelX, float startPixelY, float endPixelX, float endPixelY, float thickness)
    {
        Vector2 start = P(startPixelX, startPixelY);
        Vector2 end = P(endPixelX, endPixelY);
        Vector2 delta = end - start;
        float length = delta.magnitude;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        GameObject segment = new GameObject(name);
        segment.transform.SetParent(parent, false);
        segment.transform.position = (start + end) * 0.5f;
        segment.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        segment.transform.localScale = Vector3.one;
        segment.layer = 0;

        SpriteRenderer renderer = segment.AddComponent<SpriteRenderer>();
        renderer.sprite = GetDebugSprite();
        renderer.color = DebugWallColor;
        renderer.sortingOrder = 120;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(length, thickness);

        BoxCollider2D collider = segment.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(length, thickness);
        collider.offset = Vector2.zero;
        collider.isTrigger = false;
    }

    private static void ValidateScene(StringBuilder report, int expectedSegmentCount)
    {
        Physics2D.SyncTransforms();

        CountMissingSceneData(out int missingScripts, out int missingReferences);
        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);

        GameObject root = FindSceneObject(RootName);
        int segmentCount = CountGrayboxSegments(root);
        report.AppendLine("grayboxRootExists=" + PassFail(root != null));
        report.AppendLine("grayboxSegmentCount=" + segmentCount);
        report.AppendLine("grayboxExpectedSegmentCount=" + expectedSegmentCount);
        report.AppendLine("grayboxComponentsValid=" + PassFail(root != null && segmentCount == expectedSegmentCount && GrayboxComponentsValid(root)));
        report.AppendLine("grayboxDebugVisualsVisible=" + PassFail(root != null && GrayboxDebugVisualsVisible(root, expectedSegmentCount)));
        report.AppendLine("grayboxDefaultLayer=" + PassFail(root != null && GrayboxUsesDefaultLayer(root)));
        string routeBlockedBy;
        bool routeSampleClear = IsRouteSampleClear(root, out routeBlockedBy);
        report.AppendLine("grayboxRouteSampleClear=" + PassFail(routeSampleClear) + (routeSampleClear ? string.Empty : " blockedBy=" + routeBlockedBy));
        report.AppendLine("grayboxRedLineSamplesBlocked=" + PassFail(AreRedLineSamplesBlocked(root)));
        report.AppendLine("bridgeBlockerStillLinked=" + PassFail(IsBridgeBlockerLinked()));
        report.AppendLine("consoleCompileErrors=checked_after_unity_refresh");
    }

    private static int CountGrayboxSegments(GameObject root)
    {
        if (root == null)
        {
            return 0;
        }

        return root.GetComponentsInChildren<BoxCollider2D>(true).Length;
    }

    private static bool GrayboxComponentsValid(GameObject root)
    {
        BoxCollider2D[] colliders = root.GetComponentsInChildren<BoxCollider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            BoxCollider2D collider = colliders[i];
            if (collider == null || collider.isTrigger || collider.size.x <= 0f || collider.size.y <= 0f)
            {
                return false;
            }
        }

        return colliders.Length > 0;
    }

    private static bool GrayboxDebugVisualsVisible(GameObject root, int expectedSegmentCount)
    {
        SpriteRenderer[] renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length != expectedSegmentCount)
        {
            return false;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer renderer = renderers[i];
            if (renderer == null
                || !renderer.enabled
                || renderer.sprite == null
                || renderer.color.a <= 0.05f
                || renderer.sortingOrder < 100)
            {
                return false;
            }
        }

        return true;
    }

    private static bool GrayboxUsesDefaultLayer(GameObject root)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != null && transforms[i].gameObject.layer != 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsRouteSampleClear(GameObject root, out string blockedBy)
    {
        blockedBy = string.Empty;
        if (root == null)
        {
            return false;
        }

        Vector2[] points =
        {
            P(162f, 884f),
            P(282f, 846f),
            P(405f, 681f),
            P(657f, 552f),
            P(697f, 436f),
            P(988f, 338f),
            P(1125f, 318f),
            P(1268f, 158f)
        };

        for (int i = 0; i < points.Length; i++)
        {
            Collider2D hit = FindGrayboxOverlap(root, points[i], 0.22f);
            if (hit != null)
            {
                blockedBy = hit.gameObject.name + " at routeSampleIndex=" + i + " point=" + points[i];
                return false;
            }
        }

        return true;
    }

    private static bool AreRedLineSamplesBlocked(GameObject root)
    {
        if (root == null)
        {
            return false;
        }

        Vector2[] points =
        {
            P(72f, 940f),
            P(160f, 991f),
            P(235f, 935f),
            P(370f, 720f),
            P(314f, 668f),
            P(620f, 670f),
            P(650f, 365f),
            P(838f, 452f),
            P(960f, 278f),
            P(970f, 380f),
            P(1268f, 67f),
            P(1394f, 160f),
            P(1280f, 258f)
        };

        for (int i = 0; i < points.Length; i++)
        {
            if (!OverlapsGraybox(root, points[i], 0.20f))
            {
                return false;
            }
        }

        return true;
    }

    private static bool OverlapsGraybox(GameObject root, Vector2 point, float radius)
    {
        return FindGrayboxOverlap(root, point, radius) != null;
    }

    private static Collider2D FindGrayboxOverlap(GameObject root, Vector2 point, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit != null && hit.GetComponentInParent<Transform>().IsChildOf(root.transform))
            {
                return hit;
            }
        }

        return null;
    }

    private static bool IsBridgeBlockerLinked()
    {
        TopdownBridgeCrossingZone crossing = FindComponent<TopdownBridgeCrossingZone>("CrossingHint_01");
        GameObject blocker = FindSceneObject("Wall_River_BrokenBridge_Blocker");
        if (crossing == null || blocker == null)
        {
            return false;
        }

        SerializedObject serializedCrossing = new SerializedObject(crossing);
        SerializedProperty property = serializedCrossing.FindProperty("crossingBlocker");
        return property != null && property.objectReferenceValue == blocker;
    }

    private static void CountMissingSceneData(out int missingScripts, out int missingReferences)
    {
        missingScripts = 0;
        missingReferences = 0;

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!IsObjectInTargetScene(sceneObject))
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
    }

    private static Transform CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.position = Vector3.zero;
        child.transform.rotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;
        child.layer = 0;
        return child.transform;
    }

    private static Sprite GetDebugSprite()
    {
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite == null)
        {
            sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        }

        return sprite;
    }

    private static GameObject EnsureRoot(string name)
    {
        GameObject root = FindSceneObject(name);
        if (root == null)
        {
            root = new GameObject(name);
        }

        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        return root;
    }

    private static void DestroySceneObject(string objectName)
    {
        GameObject gameObject = FindSceneObject(objectName);
        if (gameObject != null)
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    private static T FindComponent<T>(string objectName) where T : Component
    {
        GameObject gameObject = FindSceneObject(objectName);
        return gameObject != null ? gameObject.GetComponent<T>() : null;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (sceneObject != null && sceneObject.name == objectName && IsObjectInTargetScene(sceneObject))
            {
                return sceneObject;
            }
        }

        return null;
    }

    private static bool IsObjectInTargetScene(GameObject sceneObject)
    {
        return sceneObject != null
            && sceneObject.scene.IsValid()
            && sceneObject.scene.path == ScenePath;
    }

    private static Vector2 P(float pixelX, float pixelY)
    {
        return new Vector2((pixelX - CanvasWidth * 0.5f) / PixelsPerUnit, (CanvasHeight * 0.5f - pixelY) / PixelsPerUnit);
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
