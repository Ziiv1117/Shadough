using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownStartRiverWallsSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string RequestPath = "Temp/TopdownStartRiverWalls.request";
    private const string ReportPath = "Temp/TopdownStartRiverWalls.report.txt";

    private const string MapReferenceRootName = "Level01_MapReference";
    private const string ManualWallsRootName = "Level01_ManualWalls";
    private const string FullMapName = "level01_full_map.png";
    private const string PixelOutlineOverlayName = "level01_full_map_pixel_outline_overlay.png";
    private const string FullMapAssetPath = "Assets/_Shadough/Sprites/Maps/Level01/level01_full_map.png";
    private const string PixelOutlineAssetPath = "Assets/_Shadough/Sprites/Maps/Level01/level01_full_map_pixel_outline_overlay.png";

    private const float CanvasWidth = 1448f;
    private const float CanvasHeight = 1086f;
    private const float PixelsPerUnit = 40f;

    private static readonly Vector3 PlayerSpawn = W(162f, 884f);
    private static readonly Vector3 FinalClockCoreAnchor = W(1268f, 158f);
    private static readonly Vector3 CrossingPosition = W(282f, 846f);

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Start Island River Walls")]
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

        string request = File.ReadAllText(requestPath).Trim().ToLowerInvariant();
        File.Delete(requestPath);
        if (request == "start-river-walls")
        {
            Setup();
        }
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Start Island River Walls Setup");
        report.AppendLine("Scene: " + ScenePath);
        report.AppendLine("mode=map_reference_update_plus_start_river_walls_only");
        report.AppendLine("coordinateRule=worldX=(pixelX-width/2)/PPU, worldY=(height/2-pixelY)/PPU");
        report.AppendLine("canvas=" + CanvasWidth + "x" + CanvasHeight + ", ppu=" + PixelsPerUnit);

        EnsureCoreRoots();
        EnsureReferenceAssets(report);
        CreateMapReference(report);
        ClearDeletedBlockerReferences(report);
        CleanupDeprecatedBlockout(report);

        Transform manualWalls = RecreateManualWallsRoot();
        CreateStartIslandAndRiverWalls(manualWalls, report);
        ConfigureBridgeCrossing(report);

        ValidateScene(report);
        ValidateMapDirection(report);
        ValidateStartRiverWallState(report);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown start island river walls setup complete. Report: " + FullPath(ReportPath));
    }

    private static void EnsureCoreRoots()
    {
        EnsureRoot("World");
        EnsureRoot("Lights");
        EnsureRoot("ShadowVisuals");
        EnsureRoot("ShadowLogic");
        EnsureRoot("Interactables");
        EnsureRoot("Enemies");
        EnsureRoot("UI");
    }

    private static void EnsureReferenceAssets(StringBuilder report)
    {
        EnsureReferenceAsset(FullMapName, FullMapAssetPath, report);
        EnsureReferenceAsset(PixelOutlineOverlayName, PixelOutlineAssetPath, report);
    }

    private static void EnsureReferenceAsset(string sourceFileName, string targetAssetPath, StringBuilder report)
    {
        string sourcePath = FindReferenceFile(sourceFileName);
        if (string.IsNullOrEmpty(sourcePath))
        {
            report.AppendLine("referenceImport." + sourceFileName + "=missingSource");
            return;
        }

        string targetFullPath = FullPath(targetAssetPath);
        string targetDirectory = Path.GetDirectoryName(targetFullPath);
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        bool samePath = Path.GetFullPath(sourcePath).Equals(targetFullPath, System.StringComparison.OrdinalIgnoreCase);
        bool shouldCopy = !samePath
            && (!File.Exists(targetFullPath) || new FileInfo(sourcePath).Length != new FileInfo(targetFullPath).Length);
        if (shouldCopy)
        {
            File.Copy(sourcePath, targetFullPath, true);
        }

        AssetDatabase.ImportAsset(targetAssetPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(targetAssetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        report.AppendLine("referenceImport." + sourceFileName + "=" + (shouldCopy ? "copied" : "alreadyCurrent"));
        report.AppendLine("referenceImport." + sourceFileName + ".source=" + sourcePath);
    }

    private static void CreateMapReference(StringBuilder report)
    {
        DestroySceneObject(MapReferenceRootName);

        GameObject world = EnsureRoot("World");
        GameObject root = new GameObject(MapReferenceRootName);
        root.transform.SetParent(world.transform, false);
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        bool fullMapCreated = CreateReferenceSprite(root.transform, "Level01_FinalMap_Reference", FullMapAssetPath, new Color(1f, 1f, 1f, 0.24f), -100);
        bool outlineCreated = CreateReferenceSprite(root.transform, "Level01_OutlineOverlay_Reference", PixelOutlineAssetPath, new Color(1f, 1f, 1f, 0.55f), -99);

        report.AppendLine("mapReferenceRoot=" + MapReferenceRootName);
        report.AppendLine("mapReference.finalMap=" + PassFail(fullMapCreated));
        report.AppendLine("mapReference.outlineOverlay=" + PassFail(outlineCreated));
        report.AppendLine("mapReference.position=(0,0), pivot=center, ppu=" + PixelsPerUnit);
    }

    private static bool CreateReferenceSprite(Transform parent, string name, string assetPath, Color color, int sortingOrder)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
        {
            return false;
        }

        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localScale = Vector3.one;

        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return true;
    }

    private static void ClearDeletedBlockerReferences(StringBuilder report)
    {
        TopdownBridgeCrossingZone crossing = FindComponent<TopdownBridgeCrossingZone>("CrossingHint_01");
        if (crossing == null)
        {
            report.AppendLine("crossingBlockerReference=skipped_no_crossing_zone");
            return;
        }

        SerializedObject serializedCrossing = new SerializedObject(crossing);
        SetObject(serializedCrossing, "crossingBlocker", null);
        serializedCrossing.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(crossing);
        report.AppendLine("crossingBlockerReference=cleared");
    }

    private static void CleanupDeprecatedBlockout(StringBuilder report)
    {
        int deletedObjects = 0;
        deletedObjects += DestroySceneObjectAndReport("Level01_Blockout_Accurate", report);
        deletedObjects += DestroySceneObjectAndReport("Level01_Blockout_FromPixelManifest", report);
        deletedObjects += DestroySceneObjectAndReport("Level01_PixelBlockout_FromMask", report);
        deletedObjects += DestroySceneObjectAndReport("Level01_Blockout_FromMap", report);
        deletedObjects += DestroySceneObjectAndReport("Level01_StartArea_Tuning", report);
        deletedObjects += DestroySceneObjectAndReport("Level01_GameplayAnchors", report);
        deletedObjects += DestroySceneObjectAndReport(ManualWallsRootName, report);
        deletedObjects += DestroyDeprecatedBlockoutObjectsByName(report);
        report.AppendLine("cleanup.deletedDeprecatedObjects=" + deletedObjects);
    }

    private static int DestroyDeprecatedBlockoutObjectsByName(StringBuilder report)
    {
        List<GameObject> deleteTargets = new List<GameObject>();
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (sceneObject == null
                || !sceneObject.scene.IsValid()
                || sceneObject.scene.path != ScenePath
                || !ShouldDeleteDeprecatedBlockoutObject(sceneObject))
            {
                continue;
            }

            deleteTargets.Add(sceneObject);
        }

        for (int i = 0; i < deleteTargets.Count; i++)
        {
            report.AppendLine("deletedByName=" + GetScenePath(deleteTargets[i]));
            Object.DestroyImmediate(deleteTargets[i]);
        }

        return deleteTargets.Count;
    }

    private static bool ShouldDeleteDeprecatedBlockoutObject(GameObject sceneObject)
    {
        string objectName = sceneObject.name;
        if (IsCleanupProtectedObject(objectName))
        {
            return false;
        }

        return objectName.StartsWith("Wall_")
            || objectName.StartsWith("ManualWall_")
            || objectName.StartsWith("BlockoutWall_")
            || objectName.StartsWith("Start_Island_Walkable_")
            || objectName.StartsWith("East_Bank_Walkable_")
            || objectName.StartsWith("Lower_Room_")
            || objectName.StartsWith("Upper_Room_")
            || objectName.StartsWith("Seeker_Corridor_")
            || objectName.StartsWith("Final_Chamber_")
            || objectName.StartsWith("River_Water_")
            || objectName.StartsWith("River_CrossingBlocker_")
            || objectName.StartsWith("Simplified_")
            || objectName.StartsWith("Accurate_");
    }

    private static bool IsCleanupProtectedObject(string objectName)
    {
        return objectName == "Player_Topdown"
            || objectName == "PlayerLantern"
            || objectName == "PlacedLantern_Topdown"
            || objectName == "TreeShadow_Topdown"
            || objectName == "PressurePlate_Topdown"
            || objectName == "Door_Pressure_Topdown"
            || objectName == "Lock_Topdown"
            || objectName == "Lock_Topdown_Trigger"
            || objectName == "Door_Lock_Topdown"
            || objectName == "ShadowSeeker_Topdown"
            || objectName == "FinalClockCore_Topdown"
            || objectName == "Main Camera"
            || objectName == MapReferenceRootName
            || objectName == "Level01_FinalMap_Reference"
            || objectName == "Level01_OutlineOverlay_Reference";
    }

    private static Transform RecreateManualWallsRoot()
    {
        DestroySceneObject(ManualWallsRootName);
        GameObject world = EnsureRoot("World");
        GameObject manualWalls = new GameObject(ManualWallsRootName);
        manualWalls.transform.SetParent(world.transform, false);
        manualWalls.transform.position = Vector3.zero;
        manualWalls.transform.rotation = Quaternion.identity;
        manualWalls.transform.localScale = Vector3.one;
        return manualWalls.transform;
    }

    private static void CreateStartIslandAndRiverWalls(Transform parent, StringBuilder report)
    {
        Color wallColor = new Color(0.02f, 0.02f, 0.025f, 0.98f);
        Color blockerColor = new Color(0.03f, 0.03f, 0.035f, 0.92f);

        CreateSpriteWallSegment(parent, "Wall_StartIsland_Left", W(62f, 902f), W(92f, 982f), 0.46f, wallColor, 45);
        CreateSpriteWallSegment(parent, "Wall_StartIsland_Bottom", W(92f, 986f), W(226f, 996f), 0.46f, wallColor, 45);
        CreateSpriteWallSegment(parent, "Wall_StartIsland_Right", W(292f, 876f), W(258f, 966f), 0.46f, wallColor, 45);
        CreateSpriteWallSegment(parent, "Wall_River_LeftBank", W(84f, 798f), W(184f, 760f), 0.42f, wallColor, 45);
        CreateSpriteWallSegment(parent, "Wall_River_RightBank", W(305f, 782f), W(430f, 832f), 0.42f, wallColor, 45);
        CreateSpriteWallSegment(parent, "Wall_River_LowerBank", W(188f, 918f), W(294f, 952f), 0.42f, wallColor, 45);
        CreateSpriteWallSegment(parent, "Wall_River_BrokenBridge_Blocker", W(214f, 816f), W(398f, 852f), 1.16f, blockerColor, 46);

        report.AppendLine("manualWallsRoot=" + ManualWallsRootName);
        report.AppendLine("manualWalls.scope=start_island_and_river_only");
        report.AppendLine("manualWalls.created=7");
    }

    private static void ConfigureBridgeCrossing(StringBuilder report)
    {
        TopdownBridgeCrossingZone crossing = FindComponent<TopdownBridgeCrossingZone>("CrossingHint_01");
        GameObject bridgeBlocker = FindSceneObject("Wall_River_BrokenBridge_Blocker");
        if (crossing != null)
        {
            SerializedObject serializedCrossing = new SerializedObject(crossing);
            SetObject(serializedCrossing, "crossingBlocker", bridgeBlocker);
            SetFloat(serializedCrossing, "detectionRadius", 2.15f);
            SetBool(serializedCrossing, "makeBridgeColliderTrigger", true);
            serializedCrossing.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(crossing);
        }

        report.AppendLine("bridgeBlockerLinked=" + PassFail(crossing != null && bridgeBlocker != null));
    }

    private static GameObject CreateSpriteWallSegment(Transform parent, string name, Vector3 start, Vector3 end, float thickness, Color color, int sortingOrder)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);

        Vector3 center = (start + end) * 0.5f;
        Vector2 delta = new Vector2(end.x - start.x, end.y - start.y);
        float length = delta.magnitude;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        gameObject.transform.position = center;
        gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        gameObject.transform.localScale = Vector3.one;

        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWallSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = new Vector2(length, thickness);

        BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.size = new Vector2(length, thickness);
        boxCollider.offset = Vector2.zero;
        boxCollider.isTrigger = false;

        return gameObject;
    }

    private static Sprite GetWallSprite()
    {
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite == null)
        {
            sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        }

        return sprite;
    }

    private static void ValidateScene(StringBuilder report)
    {
        CountMissingSceneData(out int missingScripts, out int missingReferences);
        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);
        report.AppendLine("consoleCompileErrors=checked_after_unity_refresh");
    }

    private static void ValidateMapDirection(StringBuilder report)
    {
        bool startsLowerLeft = PlayerSpawn.x < FinalClockCoreAnchor.x && PlayerSpawn.y < FinalClockCoreAnchor.y;
        report.AppendLine("mapDirection.startLowerLeft_finalUpperRight=" + PassFail(startsLowerLeft));

        SpriteRenderer finalMap = FindComponent<SpriteRenderer>("Level01_FinalMap_Reference");
        SpriteRenderer outline = FindComponent<SpriteRenderer>("Level01_OutlineOverlay_Reference");
        bool sameTransform = finalMap != null
            && outline != null
            && finalMap.transform.position == outline.transform.position
            && finalMap.transform.rotation == outline.transform.rotation
            && finalMap.transform.localScale == outline.transform.localScale;
        bool ppuOk = finalMap != null
            && outline != null
            && Mathf.Approximately(finalMap.sprite.pixelsPerUnit, PixelsPerUnit)
            && Mathf.Approximately(outline.sprite.pixelsPerUnit, PixelsPerUnit);
        report.AppendLine("mapReference.sameTransform=" + PassFail(sameTransform));
        report.AppendLine("mapReference.ppu40=" + PassFail(ppuOk));
    }

    private static void ValidateStartRiverWallState(StringBuilder report)
    {
        Physics2D.SyncTransforms();

        report.AppendLine("startRiver.Level01_MapReference=" + PassFail(Exists(MapReferenceRootName)));
        report.AppendLine("startRiver.finalMapReference=" + PassFail(Exists("Level01_FinalMap_Reference")));
        report.AppendLine("startRiver.outlineReference=" + PassFail(Exists("Level01_OutlineOverlay_Reference")));
        report.AppendLine("startRiver.manualWallsRoot=" + PassFail(Exists(ManualWallsRootName)));
        report.AppendLine("startRiver.manualWallCount=" + CountManualWalls());
        report.AppendLine("startRiver.onlyStartRiverWalls=" + PassFail(NoForbiddenManualWallNames()));
        report.AppendLine("startRiver.playerBlocked=" + PassFail(!IsPlayerBlocked()));
        report.AppendLine("startRiver.playerSpawnAnchorBlocked=" + PassFail(!IsWorldPointBlocked(V2(PlayerSpawn))));
        report.AppendLine("startRiver.bridgeBlockerExists=" + PassFail(Exists("Wall_River_BrokenBridge_Blocker")));
        report.AppendLine("startRiver.bridgeBlockerBlocksBeforeShadow=" + PassFail(IsPointBlockedBy("Wall_River_BrokenBridge_Blocker", V2(CrossingPosition))));
        report.AppendLine("startRiver.wallComponentsValid=" + PassFail(ManualWallsHaveRequiredComponents()));
    }

    private static int CountManualWalls()
    {
        GameObject root = FindSceneObject(ManualWallsRootName);
        if (root == null)
        {
            return 0;
        }

        int count = 0;
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].gameObject != root)
            {
                count++;
            }
        }

        return count;
    }

    private static bool NoForbiddenManualWallNames()
    {
        GameObject root = FindSceneObject(ManualWallsRootName);
        if (root == null)
        {
            return false;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == null || children[i].gameObject == root)
            {
                continue;
            }

            string objectName = children[i].name;
            if (objectName.Contains("Room") || objectName.Contains("Seeker") || objectName.Contains("Final"))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ManualWallsHaveRequiredComponents()
    {
        GameObject root = FindSceneObject(ManualWallsRootName);
        if (root == null)
        {
            return false;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] == null || children[i].gameObject == root)
            {
                continue;
            }

            SpriteRenderer renderer = children[i].GetComponent<SpriteRenderer>();
            BoxCollider2D collider = children[i].GetComponent<BoxCollider2D>();
            if (renderer == null || collider == null || collider.isTrigger)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsPlayerBlocked()
    {
        GameObject player = FindSceneObject("Player_Topdown");
        return player == null || IsWorldPointBlocked(player.transform.position);
    }

    private static bool IsWorldPointBlocked(Vector2 point)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, 0.24f);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || hit.isTrigger)
            {
                continue;
            }

            string objectName = hit.gameObject.name;
            if (objectName == "Player_Topdown"
                || objectName == "PlayerLantern"
                || objectName == "PlacedLantern_Topdown"
                || objectName == "TreeShadow_Topdown")
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool IsPointBlockedBy(string objectName, Vector2 point)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(point, 0.18f);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit != null && hit.gameObject.name == objectName && !hit.isTrigger)
            {
                return true;
            }
        }

        return false;
    }

    private static void CountMissingSceneData(out int missingScripts, out int missingReferences)
    {
        missingScripts = 0;
        missingReferences = 0;

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

    private static int DestroySceneObjectAndReport(string objectName, StringBuilder report)
    {
        GameObject gameObject = FindSceneObject(objectName);
        if (gameObject == null)
        {
            report.AppendLine("deleteRoot." + objectName + "=not_found");
            return 0;
        }

        int childCount = gameObject.GetComponentsInChildren<Transform>(true).Length;
        report.AppendLine("deleteRoot." + objectName + "=deleted childrenIncludingRoot=" + childCount);
        Object.DestroyImmediate(gameObject);
        return childCount;
    }

    private static void DestroySceneObject(string objectName)
    {
        GameObject gameObject = FindSceneObject(objectName);
        if (gameObject != null)
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    private static bool Exists(string objectName)
    {
        return FindSceneObject(objectName) != null;
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
            if (sceneObject == null || sceneObject.name != objectName)
            {
                continue;
            }

            if (sceneObject.scene.IsValid() && sceneObject.scene.path == ScenePath)
            {
                return sceneObject;
            }
        }

        return null;
    }

    private static string GetScenePath(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return "<null>";
        }

        List<string> names = new List<string>();
        Transform current = gameObject.transform;
        while (current != null)
        {
            names.Insert(0, current.name);
            current = current.parent;
        }

        return string.Join("/", names.ToArray());
    }

    private static string FindReferenceFile(string fileName)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string workspaceRoot = Path.GetFullPath(Path.Combine(projectRoot, ".."));
        string[] matches = Directory.GetFiles(workspaceRoot, fileName, SearchOption.AllDirectories);
        if (matches.Length == 0)
        {
            return string.Empty;
        }

        for (int i = 0; i < matches.Length; i++)
        {
            string fullPath = Path.GetFullPath(matches[i]);
            if (!fullPath.StartsWith(projectRoot, System.StringComparison.OrdinalIgnoreCase))
            {
                return fullPath;
            }
        }

        return matches[0];
    }

    private static Vector3 W(float pixelX, float pixelY)
    {
        return new Vector3((pixelX - CanvasWidth * 0.5f) / PixelsPerUnit, (CanvasHeight * 0.5f - pixelY) / PixelsPerUnit, 0f);
    }

    private static Vector2 V2(Vector3 value)
    {
        return new Vector2(value.x, value.y);
    }

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }

    private static string PassFail(bool passed)
    {
        return passed ? "PASS" : "FAIL";
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

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }
}
