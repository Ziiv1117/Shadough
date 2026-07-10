using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownPixelBlockoutLayoutSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string AutoSetupRequestPath = "Temp/TopdownPixelBlockoutLayoutSetup.request";
    private const string ReportPath = "Temp/TopdownPixelBlockoutLayoutSetup.report.txt";
    private const string CleanupReportPath = "Temp/TopdownBlockoutCleanup.report.txt";
    private const string PlayProbeRequestPath = "Temp/TopdownPixelBlockoutPlayProbe.request";
    private const string PlayProbeReportPath = "Logs/TopdownPixelBlockoutPlayProbe.report.txt";

    private const string AccurateRootName = "Level01_Blockout_Accurate";
    private const string MapReferenceRootName = "Level01_MapReference";
    private const string GameplayAnchorsRootName = "Level01_GameplayAnchors";
    private const string SimplifiedRootName = "Level01_Blockout_FromPixelManifest";
    private const string PixelShardRootName = "Level01_PixelBlockout_FromMask";
    private const string OldLayoutRootName = "Level01_Blockout_FromMap";
    private const string OldStartTuningRootName = "Level01_StartArea_Tuning";

    private const string FullMapName = "level01_full_map.png";
    private const string PixelClassMaskName = "level01_full_map_pixel_class_mask.png";
    private const string PixelOutlineOverlayName = "level01_full_map_pixel_outline_overlay.png";
    private const string FullMapAssetPath = "Assets/_Shadough/Sprites/Maps/Level01/level01_full_map.png";
    private const string PixelOutlineAssetPath = "Assets/_Shadough/Sprites/Maps/Level01/level01_full_map_pixel_outline_overlay.png";

    private const float CanvasWidth = 1448f;
    private const float CanvasHeight = 1086f;
    private const float PixelsPerUnit = 40f;

    private static readonly Vector3 PlayerSpawn = W(162f, 884f);
    private static readonly Vector3 TreeAnchor = W(127f, 789f);
    private static readonly Vector3 OuterGateAnchor = W(405f, 681f);
    private static readonly Vector3 KeyLockAnchor = W(657f, 552f);
    private static readonly Vector3 PressPlateAnchor = W(697f, 436f);
    private static readonly Vector3 LureMidpoint = W(988f, 338f);
    private static readonly Vector3 FinalClockCoreAnchor = W(1268f, 158f);

    private static readonly Vector3 TreeShadowReadPosition = W(235f, 835f);
    private static readonly Vector3 CrossingPosition = W(282f, 846f);
    private static readonly Vector3 RiverBlockerCenter = W(292f, 864f);

    private static StringBuilder playProbeReport;
    private static int playProbeStage;
    private static double nextPlayProbeTime;
    private static PastedShadowObject bridgeShadow;
    private static PastedShadowObject pressShadow;
    private static PastedShadowObject unlockShadow;
    private static PastedShadowObject wrongLureShadow;
    private static PastedShadowObject playerLureShadow;

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Topdown Pixel Manifest Blockout")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    [MenuItem("Shadough/Cleanup Deprecated Topdown Blockout")]
    public static void CleanupDeprecatedBlockoutFromMenu()
    {
        CleanupDeprecatedBlockout();
    }

    [MenuItem("Shadough/Run Topdown Pixel Manifest Play Probe")]
    public static void RequestPlayProbeFromMenu()
    {
        File.WriteAllText(FullPath(PlayProbeRequestPath), "play-probe");
    }

    public static void RunPlayProbeBatch()
    {
        File.WriteAllText(FullPath(PlayProbeRequestPath), "play-probe");
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
        TryAutoSetup();
    }

    private static void TryAutoSetup()
    {
        if (EditorApplication.isCompiling)
        {
            return;
        }

        string setupRequestPath = FullPath(AutoSetupRequestPath);
        if (File.Exists(setupRequestPath))
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            string request = File.ReadAllText(setupRequestPath).Trim().ToLowerInvariant();
            File.Delete(setupRequestPath);
            if (request == "cleanup")
            {
                CleanupDeprecatedBlockout();
            }
            else
            {
                Setup();
            }

            return;
        }

        string playProbeRequestPath = FullPath(PlayProbeRequestPath);
        if (File.Exists(playProbeRequestPath) && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
            return;
        }

        if (File.Exists(playProbeRequestPath) && EditorApplication.isPlaying && playProbeReport == null)
        {
            StartPlayProbe();
        }
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Accurate Reference Blockout Setup");
        report.AppendLine("Scene: " + ScenePath);
        report.AppendLine("coordinateRule=worldX=(pixelX-width/2)/PPU, worldY=(height/2-pixelY)/PPU");
        report.AppendLine("canvas=" + CanvasWidth + "x" + CanvasHeight + ", ppu=" + PixelsPerUnit);
        report.AppendLine("fullMapReference=" + FindReferenceFile(FullMapName));
        report.AppendLine("pixelClassMaskReference=" + FindReferenceFile(PixelClassMaskName));
        report.AppendLine("pixelOutlineOverlayReference=" + FindReferenceFile(PixelOutlineOverlayName));
        report.AppendLine("pixelMaskMode=referenceOnly_noShardMeshes");

        EnsureCoreRoots();
        EnsureReferenceAssets(report);
        CreateMapReference(report);
        Transform root = RecreateSimplifiedRoot();
        CreateSimplifiedBlockout(root, report);
        RepositionGameplayObjects(report);
        ConfigureCamera(report);
        ValidateScene(report);
        ValidateMapDirection(report);
        ValidateRouteClearance(report);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown accurate reference blockout setup complete. Report: " + FullPath(ReportPath));
    }

    public static void CleanupDeprecatedBlockout()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Deprecated Blockout Cleanup");
        report.AppendLine("Scene: " + ScenePath);
        report.AppendLine("mode=cleanup_only_no_new_walls_no_new_gameplay");

        ClearDeletedBlockerReferences(report);

        int deletedObjects = 0;
        deletedObjects += DestroySceneObjectAndReport(AccurateRootName, report);
        deletedObjects += DestroySceneObjectAndReport(SimplifiedRootName, report);
        deletedObjects += DestroySceneObjectAndReport(PixelShardRootName, report);
        deletedObjects += DestroySceneObjectAndReport(OldLayoutRootName, report);
        deletedObjects += DestroySceneObjectAndReport(OldStartTuningRootName, report);
        deletedObjects += DestroySceneObjectAndReport(GameplayAnchorsRootName, report);
        deletedObjects += DestroySceneObjectAndReport("Level01_ManualWalls", report);
        deletedObjects += DestroyDeprecatedBlockoutObjectsByName(report);

        ValidateScene(report);
        ValidateCleanupState(report);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        report.AppendLine("deletedSceneObjects=" + deletedObjects);
        File.WriteAllText(FullPath(CleanupReportPath), report.ToString());
        Debug.Log("Topdown deprecated blockout cleanup complete. Report: " + FullPath(CleanupReportPath));
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

    private static Transform RecreateSimplifiedRoot()
    {
        DestroySceneObject(AccurateRootName);
        DestroySceneObject(SimplifiedRootName);
        DestroySceneObject(PixelShardRootName);
        DestroySceneObject(OldLayoutRootName);
        DestroySceneObject(OldStartTuningRootName);
        DestroySceneObject(GameplayAnchorsRootName);

        GameObject world = EnsureRoot("World");
        GameObject root = new GameObject(AccurateRootName);
        root.transform.SetParent(world.transform, false);
        return root.transform;
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

        bool shouldCopy = !File.Exists(targetFullPath)
            || new FileInfo(sourcePath).Length != new FileInfo(targetFullPath).Length;
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
    }

    private static void CreateMapReference(StringBuilder report)
    {
        DestroySceneObject(MapReferenceRootName);

        GameObject world = EnsureRoot("World");
        GameObject root = new GameObject(MapReferenceRootName);
        root.transform.SetParent(world.transform, false);
        root.transform.position = Vector3.zero;

        bool fullMapCreated = CreateReferenceSprite(root.transform, "Level01_FinalMap_Reference", FullMapAssetPath, new Color(1f, 1f, 1f, 0.20f), -100);
        bool outlineCreated = CreateReferenceSprite(root.transform, "Level01_OutlineOverlay_Reference", PixelOutlineAssetPath, new Color(1f, 1f, 1f, 0.52f), -99);

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

    private static void CreateSimplifiedBlockout(Transform root, StringBuilder report)
    {
        Transform walkable = CreateChild(root, "Accurate_Walkable_Areas");
        Transform water = CreateChild(root, "Accurate_Water_Blockers");
        Transform blockers = CreateChild(root, "Accurate_Walls_And_Blockers");

        GameObject anchorRoot = new GameObject(GameplayAnchorsRootName);
        anchorRoot.transform.SetParent(EnsureRoot("World").transform, false);
        Transform guides = CreateChild(anchorRoot.transform, "Manifest_Anchors_And_Guides");

        Color grassColor = new Color(0.34f, 0.59f, 0.22f, 0.72f);
        Color stoneColor = new Color(0.58f, 0.57f, 0.52f, 0.72f);
        Color waterColor = new Color(0.06f, 0.36f, 0.58f, 0.92f);
        Color wallColor = new Color(0.11f, 0.11f, 0.10f, 0.95f);
        Color guideColor = new Color(1f, 0.62f, 0.15f, 0.46f);

        CreatePolygon(walkable, "Start_Island_Walkable_Accurate", grassColor, new Vector2[]
        {
            P(62f, 900f), P(86f, 842f), P(150f, 808f), P(225f, 820f),
            P(305f, 872f), P(294f, 936f), P(220f, 992f), P(95f, 986f),
            P(45f, 938f)
        }, 0);

        CreatePolygon(walkable, "East_Bank_Walkable_Accurate", grassColor, new Vector2[]
        {
            P(232f, 735f), P(355f, 705f), P(460f, 742f), P(497f, 812f),
            P(448f, 870f), P(318f, 872f), P(248f, 822f)
        }, 0);

        CreatePolygon(walkable, "Lower_Room_KeyLock_Walkable_Accurate", stoneColor, new Vector2[]
        {
            P(308f, 612f), P(438f, 552f), P(585f, 592f), P(637f, 700f),
            P(575f, 790f), P(380f, 782f), P(305f, 720f)
        }, 0);

        CreatePolygon(walkable, "Connector_Lower_To_Upper_Accurate", stoneColor, new Vector2[]
        {
            P(528f, 548f), P(604f, 505f), P(682f, 548f), P(615f, 635f),
            P(528f, 602f)
        }, 0);

        CreatePolygon(walkable, "Upper_Room_PressPlate_Walkable_Accurate", stoneColor, new Vector2[]
        {
            P(535f, 390f), P(690f, 340f), P(818f, 386f), P(852f, 495f),
            P(735f, 566f), P(572f, 526f), P(515f, 446f)
        }, 0);

        CreatePolygon(walkable, "Seeker_Corridor_Walkable_Accurate", stoneColor, new Vector2[]
        {
            P(755f, 336f), P(872f, 278f), P(1010f, 280f), P(1058f, 246f),
            P(1178f, 258f), P(1210f, 338f), P(1120f, 382f), P(1015f, 350f),
            P(930f, 408f), P(792f, 418f), P(725f, 372f)
        }, 0);

        CreatePolygon(walkable, "Final_Chamber_Walkable_Accurate", stoneColor, new Vector2[]
        {
            P(1175f, 102f), P(1230f, 66f), P(1348f, 68f), P(1398f, 108f),
            P(1390f, 212f), P(1332f, 260f), P(1210f, 254f), P(1158f, 204f)
        }, 0);

        CreatePolygon(water, "River_Water_Accurate", waterColor, new Vector2[]
        {
            P(82f, 792f), P(182f, 758f), P(305f, 780f), P(430f, 828f),
            P(396f, 920f), P(292f, 952f), P(190f, 914f), P(92f, 914f)
        }, 1);

        GameObject riverBlocker = CreateBox(blockers, "River_CrossingBlocker_Accurate", RiverBlockerCenter, new Vector2(5.80f, 2.65f), new Color(0.04f, 0.18f, 0.30f, 0.70f), true, false, 10);

        CreateWallSegment(blockers, "Wall_LowerRoom_NorthWest_Accurate", W(322f, 612f), W(438f, 552f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_LowerRoom_NorthEast_Accurate", W(462f, 558f), W(586f, 592f), 0.35f, wallColor, false, 12);
        CreateWallSegment(blockers, "Wall_LowerRoom_East_Accurate", W(628f, 624f), W(612f, 718f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_LowerRoom_SouthEast_Accurate", W(566f, 780f), W(482f, 790f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_LowerRoom_SouthWest_Accurate", W(350f, 744f), W(326f, 724f), 0.35f, wallColor, false, 12);
        CreateWallSegment(blockers, "Wall_LowerRoom_West_Accurate", W(310f, 694f), W(318f, 636f), 0.35f, wallColor, true, 12);

        CreateWallSegment(blockers, "Wall_Connector_Left_Accurate", W(522f, 604f), W(592f, 646f), 0.32f, wallColor, false, 12);
        CreateWallSegment(blockers, "Wall_Connector_Right_Accurate", W(612f, 502f), W(688f, 548f), 0.32f, wallColor, false, 12);

        CreateWallSegment(blockers, "Wall_UpperRoom_NorthWest_Accurate", W(542f, 390f), W(690f, 342f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_UpperRoom_NorthEast_Accurate", W(713f, 348f), W(815f, 386f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_UpperRoom_East_Accurate", W(846f, 410f), W(846f, 492f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_UpperRoom_SouthEast_Accurate", W(730f, 560f), W(650f, 542f), 0.35f, wallColor, false, 12);
        CreateWallSegment(blockers, "Wall_UpperRoom_West_Accurate", W(530f, 438f), W(560f, 520f), 0.35f, wallColor, true, 12);

        CreateWallSegment(blockers, "Wall_SeekerCorridor_NorthA_Accurate", W(770f, 332f), W(888f, 276f), 0.30f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_SeekerCorridor_NorthB_Accurate", W(914f, 276f), W(1002f, 280f), 0.30f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_SeekerCorridor_NorthC_Accurate", W(1058f, 248f), W(1176f, 258f), 0.30f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_SeekerCorridor_SouthA_Accurate", W(742f, 374f), W(794f, 418f), 0.30f, wallColor, false, 12);
        CreateWallSegment(blockers, "Wall_SeekerCorridor_SouthB_Accurate", W(922f, 408f), W(1018f, 350f), 0.30f, wallColor, false, 12);
        CreateWallSegment(blockers, "Wall_SeekerCorridor_SouthC_Accurate", W(1120f, 382f), W(1200f, 340f), 0.30f, wallColor, false, 12);

        CreateWallSegment(blockers, "Wall_Final_NorthWest_Accurate", W(1178f, 102f), W(1230f, 66f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_Final_North_Accurate", W(1230f, 66f), W(1348f, 68f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_Final_NorthEast_Accurate", W(1348f, 68f), W(1398f, 108f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_Final_East_Accurate", W(1398f, 108f), W(1390f, 212f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_Final_SouthEast_Accurate", W(1390f, 212f), W(1332f, 260f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_Final_South_Accurate", W(1332f, 260f), W(1230f, 255f), 0.35f, wallColor, true, 12);
        CreateWallSegment(blockers, "Wall_Final_West_Accurate", W(1160f, 204f), W(1176f, 112f), 0.35f, wallColor, true, 12);

        CreateBox(guides, "Anchor_PlayerSpawn", PlayerSpawn, new Vector2(0.42f, 0.42f), Color.red, false, true, 20);
        CreateBox(guides, "Anchor_TreeShadowSource", TreeAnchor, new Vector2(0.42f, 0.42f), Color.green, false, true, 20);
        CreateBox(guides, "Anchor_OuterGateTransition", OuterGateAnchor, new Vector2(1.10f, 0.70f), guideColor, false, true, 20);
        CreateBox(guides, "Anchor_KeyLock", KeyLockAnchor, new Vector2(0.65f, 0.65f), Color.yellow, false, true, 20);
        CreateBox(guides, "Anchor_PressPlate", PressPlateAnchor, new Vector2(0.65f, 0.65f), Color.cyan, false, true, 20);
        CreateBox(guides, "Anchor_LureMidpoint", LureMidpoint, new Vector2(0.75f, 0.75f), Color.magenta, false, true, 20);
        CreateBox(guides, "Anchor_FinalClockCore", FinalClockCoreAnchor, new Vector2(0.75f, 0.75f), Color.white, false, true, 20);

        TopdownBridgeCrossingZone crossing = FindComponent<TopdownBridgeCrossingZone>("CrossingHint_01");
        if (crossing != null)
        {
            SerializedObject serializedCrossing = new SerializedObject(crossing);
            SetObject(serializedCrossing, "crossingBlocker", riverBlocker);
            SetFloat(serializedCrossing, "detectionRadius", 2.15f);
            SetBool(serializedCrossing, "makeBridgeColliderTrigger", true);
            serializedCrossing.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(crossing);
        }

        report.AppendLine("accurateRoot=" + AccurateRootName);
        report.AppendLine("gameplayAnchorsRoot=" + GameplayAnchorsRootName);
        report.AppendLine("pixelShardRootRemoved=" + (FindSceneObject(PixelShardRootName) == null));
        report.AppendLine("accurateReferenceBlockout=created");
    }

    private static void RepositionGameplayObjects(StringBuilder report)
    {
        Transform world = EnsureRoot("World").transform;
        Transform interactables = EnsureRoot("Interactables").transform;
        Transform enemies = EnsureRoot("Enemies").transform;
        Transform shadows = EnsureRoot("ShadowLogic").transform;

        MoveObject("Player_Topdown", PlayerSpawn, world, Vector3.one);
        MoveObject("Tree_01", TreeAnchor, world, new Vector3(1.18f, 1.18f, 1f));
        MoveObject("Tree_Trunk", TreeAnchor + new Vector3(0f, -0.12f, 0f), world, new Vector3(0.52f, 1.05f, 1f));
        MoveObject("Tree_Canopy", TreeAnchor + new Vector3(0f, 0.42f, 0f), world, new Vector3(1.42f, 1.08f, 1f));
        GameObject treeShadow = MoveObject("TreeShadow_Topdown", TreeShadowReadPosition, shadows, Vector3.one);
        ConfigureTreeShadow(treeShadow);
        MoveObject("CrossingHint_01", CrossingPosition, interactables, Vector3.one);

        MoveObject("Door_Pressure_Topdown", PressPlateAnchor + new Vector3(2.05f, 0.02f, 0f), interactables, new Vector3(0.28f, 1.25f, 1f));
        MoveObject("PressurePlate_Topdown", PressPlateAnchor, interactables, new Vector3(1.05f, 0.72f, 1f));
        MoveObject("BeamSource_Topdown", W(610f, 470f), interactables, new Vector3(0.75f, 0.75f, 1f));
        MoveObject("BeamShadow_Topdown", W(640f, 455f), shadows, new Vector3(1.45f, 0.65f, 1f));

        MoveObject("Door_Lock_Topdown", KeyLockAnchor + new Vector3(0.34f, 0f, 0f), interactables, new Vector3(0.28f, 1.25f, 1f));
        MoveObject("Lock_Topdown", KeyLockAnchor, interactables, new Vector3(0.85f, 0.85f, 1f));
        MoveObject("Lock_Topdown_Trigger", KeyLockAnchor, interactables, Vector3.one);
        MoveObject("KeySource_Topdown", W(505f, 650f), interactables, new Vector3(0.75f, 0.75f, 1f));
        MoveObject("KeyShadow_Topdown", W(548f, 635f), shadows, new Vector3(0.9f, 0.55f, 1f));

        MoveObject("ShadowSeeker_Topdown", LureMidpoint, enemies, new Vector3(1.8f, 1.8f, 1f));
        MoveObject("ShadowSeeker_Home_Topdown", LureMidpoint, enemies, Vector3.one);
        MoveObject("LureArea_Topdown", LureMidpoint + new Vector3(2.50f, 0.15f, 0f), interactables, new Vector3(1.65f, 0.9f, 1f));
        MoveObject("ShadowSeekerSafePoint_Topdown", W(835f, 430f), world, Vector3.one);
        MoveObject("FinalClockCore_Topdown", FinalClockCoreAnchor, interactables, new Vector3(1.35f, 1.35f, 1f));

        ConfigureGameplayComponents();
        DisableLegacyBlockers();
        FixTutorialSignColliders();

        report.AppendLine("anchors.player_spawn=" + PlayerSpawn);
        report.AppendLine("anchors.tree_shadow_source=" + TreeAnchor);
        report.AppendLine("anchors.outer_gate_transition=" + OuterGateAnchor);
        report.AppendLine("anchors.key_lock_anchor=" + KeyLockAnchor);
        report.AppendLine("anchors.press_plate_anchor=" + PressPlateAnchor);
        report.AppendLine("anchors.lure_midpoint=" + LureMidpoint);
        report.AppendLine("anchors.final_clock_core=" + FinalClockCoreAnchor);
    }

    private static void ConfigureTreeShadow(GameObject treeShadow)
    {
        if (treeShadow == null)
        {
            return;
        }

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

    private static void ConfigureGameplayComponents()
    {
        ConfigureShadowInteractable("TreeShadow_Topdown", "Tree Shadow", true, false, false, false, false);
        ConfigureShadowInteractable("BeamShadow_Topdown", "CanPress Shadow", false, true, false, false, false);
        ConfigureShadowInteractable("KeyShadow_Topdown", "CanUnlock Shadow", false, false, true, false, false);

        PressurePlateController plate = FindComponent<PressurePlateController>("PressurePlate_Topdown");
        DoorController pressureDoor = FindComponent<DoorController>("Door_Pressure_Topdown");
        ShadowPressureTrigger pressureTrigger = FindComponent<ShadowPressureTrigger>("PressurePlate_Topdown");
        if (plate != null)
        {
            SerializedObject serializedPlate = new SerializedObject(plate);
            SetObject(serializedPlate, "targetDoor", pressureDoor);
            serializedPlate.ApplyModifiedPropertiesWithoutUndo();
            plate.SetPressed(false);
            EditorUtility.SetDirty(plate);
        }

        if (pressureDoor != null)
        {
            pressureDoor.SetOpen(false);
            EditorUtility.SetDirty(pressureDoor);
        }

        if (pressureTrigger != null)
        {
            SerializedObject serializedTrigger = new SerializedObject(pressureTrigger);
            SetObject(serializedTrigger, "pressurePlate", plate);
            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(pressureTrigger);
        }

        LockController lockController = FindComponent<LockController>("Lock_Topdown");
        DoorController lockDoor = FindComponent<DoorController>("Door_Lock_Topdown");
        ShadowLockTrigger lockTrigger = FindComponent<ShadowLockTrigger>("Lock_Topdown_Trigger");
        if (lockController != null)
        {
            SerializedObject serializedLock = new SerializedObject(lockController);
            SetObject(serializedLock, "targetDoor", lockDoor);
            serializedLock.ApplyModifiedPropertiesWithoutUndo();
            lockController.SetUnlocked(false);
            EditorUtility.SetDirty(lockController);
        }

        if (lockDoor != null)
        {
            lockDoor.SetOpen(false);
            EditorUtility.SetDirty(lockDoor);
        }

        if (lockTrigger != null)
        {
            SerializedObject serializedTrigger = new SerializedObject(lockTrigger);
            SetObject(serializedTrigger, "lockController", lockController);
            SetBool(serializedTrigger, "requireAngleCheck", false);
            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(lockTrigger);
        }

        ConfigureLocalBoxCollider("PressurePlate_Topdown", new Vector2(1.45f, 1.05f), Vector2.zero, true);
        ConfigureLocalBoxCollider("Door_Pressure_Topdown", new Vector2(0.44f, 1.65f), Vector2.zero, false);
        ConfigureLocalBoxCollider("Lock_Topdown_Trigger", new Vector2(1.55f, 1.35f), Vector2.zero, true);
        ConfigureLocalBoxCollider("Door_Lock_Topdown", new Vector2(0.44f, 1.65f), Vector2.zero, false);
        ConfigureLocalBoxCollider("FinalClockCore_Topdown", new Vector2(1.85f, 1.55f), Vector2.zero, true);
        ConfigureLocalBoxCollider("LureArea_Topdown", new Vector2(2.35f, 1.35f), Vector2.zero, true);

        ConfigureSeeker();
        ConfigureFinalCore();
    }

    private static void ConfigureSeeker()
    {
        EnemyShadowSeeker seeker = FindComponent<EnemyShadowSeeker>("ShadowSeeker_Topdown");
        GameObject home = FindSceneObject("ShadowSeeker_Home_Topdown");
        GameObject player = FindSceneObject("Player_Topdown");
        if (seeker != null)
        {
            SerializedObject serializedSeeker = new SerializedObject(seeker);
            SetObject(serializedSeeker, "homePoint", home != null ? home.transform : null);
            SetObject(serializedSeeker, "playerTarget", player != null ? player.transform : null);
            SetFloat(serializedSeeker, "moveSpeed", 2.0f);
            SetFloat(serializedSeeker, "detectionRadius", 4.35f);
            SetFloat(serializedSeeker, "attackDistance", 0.65f);
            SetFloat(serializedSeeker, "attackCooldown", 1.5f);
            SetBool(serializedSeeker, "ignorePlayerBodyCollision", true);
            SetBool(serializedSeeker, "tintRendererByState", false);
            SetBool(serializedSeeker, "showDebugGizmos", true);
            serializedSeeker.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(seeker);
        }

        PlayerHealth health = FindComponent<PlayerHealth>("Player_Topdown");
        GameObject safePoint = FindSceneObject("ShadowSeekerSafePoint_Topdown");
        if (health != null)
        {
            SerializedObject serializedHealth = new SerializedObject(health);
            SetObject(serializedHealth, "safePoint", safePoint != null ? safePoint.transform : null);
            serializedHealth.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(health);
        }
    }

    private static void ConfigureFinalCore()
    {
        TopdownFinalClockCore finalCore = FindComponent<TopdownFinalClockCore>("FinalClockCore_Topdown");
        if (finalCore != null)
        {
            SerializedObject serializedCore = new SerializedObject(finalCore);
            SetString(serializedCore, "completeText", "Topdown Demo Complete");
            SetString(serializedCore, "logMessage", "Topdown demo complete");
            serializedCore.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(finalCore);
        }
    }

    private static void ConfigureCamera(StringBuilder report)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = FindSceneObject("Main Camera");
            camera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;
        }

        if (camera == null)
        {
            report.AppendLine("camera=missing");
            return;
        }

        camera.orthographic = true;
        camera.orthographicSize = 6.55f;
        camera.transform.position = PlayerSpawn + new Vector3(1.15f, 0.75f, -10f);
        EditorUtility.SetDirty(camera);

        TopdownCameraFollow follow = camera.GetComponent<TopdownCameraFollow>();
        GameObject player = FindSceneObject("Player_Topdown");
        if (follow != null)
        {
            SerializedObject serializedFollow = new SerializedObject(follow);
            SetObject(serializedFollow, "target", player != null ? player.transform : null);
            SetVector3(serializedFollow, "offset", new Vector3(1.15f, 0.75f, -10f));
            SetFloat(serializedFollow, "followSmoothTime", 0.12f);
            SetBool(serializedFollow, "snapOnStart", true);
            serializedFollow.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(follow);
        }

        report.AppendLine("camera=startViewConfigured");
    }

    private static void ValidateScene(StringBuilder report)
    {
        int missingScripts;
        int missingReferences;
        CountMissingSceneData(out missingScripts, out missingReferences);

        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);
        report.AppendLine("required.Player=" + Exists("Player_Topdown"));
        report.AppendLine("required.TreeShadow=" + Exists("TreeShadow_Topdown"));
        report.AppendLine("required.Crossing=" + Exists("CrossingHint_01"));
        report.AppendLine("required.PressurePlate=" + Exists("PressurePlate_Topdown"));
        report.AppendLine("required.Lock=" + Exists("Lock_Topdown"));
        report.AppendLine("required.ShadowSeeker=" + Exists("ShadowSeeker_Topdown"));
        report.AppendLine("required.FinalClockCore=" + Exists("FinalClockCore_Topdown"));
    }

    private static void ValidateMapDirection(StringBuilder report)
    {
        bool startLowerLeft = PlayerSpawn.x < 0f && PlayerSpawn.y < 0f;
        bool finalUpperRight = FinalClockCoreAnchor.x > 0f && FinalClockCoreAnchor.y > 0f;
        bool routeExtendsUpRight = FinalClockCoreAnchor.x > PlayerSpawn.x && FinalClockCoreAnchor.y > PlayerSpawn.y;
        bool riverNearStart = Vector2.Distance(V2(PlayerSpawn), V2(CrossingPosition)) < 4.3f;
        bool treeNearStart = Vector2.Distance(V2(PlayerSpawn), V2(TreeAnchor)) < 3.0f;

        report.AppendLine("direction.startLowerLeft=" + PassFail(startLowerLeft));
        report.AppendLine("direction.finalUpperRight=" + PassFail(finalUpperRight));
        report.AppendLine("direction.routeExtendsUpRight=" + PassFail(routeExtendsUpRight));
        report.AppendLine("direction.riverNearStart=" + PassFail(riverNearStart));
        report.AppendLine("direction.treeNearStart=" + PassFail(treeNearStart));
    }

    private static void ValidateRouteClearance(StringBuilder report)
    {
        Physics2D.SyncTransforms();

        string blockedBy;
        bool routeClear = IsRouteClear(new Vector2[]
        {
            V2(PlayerSpawn),
            V2(CrossingPosition),
            V2(OuterGateAnchor),
            V2(KeyLockAnchor),
            V2(PressPlateAnchor),
            V2(PressPlateAnchor + new Vector3(2.35f, 0.35f, 0f)),
            V2(LureMidpoint - new Vector3(3.45f, 1.05f, 0f)),
            V2(LureMidpoint + new Vector3(2.55f, 0.25f, 0f)),
            new Vector2(11.80f, 5.50f),
            new Vector2(11.95f, 6.90f),
            V2(FinalClockCoreAnchor - new Vector3(1.75f, 1.55f, 0f)),
            V2(FinalClockCoreAnchor)
        }, out blockedBy);

        EnemyShadowSeeker seeker = FindComponent<EnemyShadowSeeker>("ShadowSeeker_Topdown");
        bool safePointOutsideDetection = true;
        bool lureInsideDetection = true;
        GameObject safePoint = FindSceneObject("ShadowSeekerSafePoint_Topdown");
        GameObject lureArea = FindSceneObject("LureArea_Topdown");
        if (seeker != null)
        {
            safePointOutsideDetection = safePoint != null && Vector2.Distance(safePoint.transform.position, seeker.transform.position) > seeker.DetectionRadius;
            lureInsideDetection = lureArea != null && Vector2.Distance(lureArea.transform.position, seeker.transform.position) < seeker.DetectionRadius;
        }

        report.AppendLine("routeClearance=" + PassFail(routeClear) + (routeClear ? string.Empty : " blockedBy=" + blockedBy));
        report.AppendLine("seekerSafePointOutsideDetection=" + PassFail(safePointOutsideDetection));
        report.AppendLine("lureAreaInsideDetection=" + PassFail(lureInsideDetection));
    }

    private static void StartPlayProbe()
    {
        playProbeReport = new StringBuilder();
        playProbeReport.AppendLine("Topdown Pixel Manifest Play Probe");
        playProbeReport.AppendLine("Scene: " + ScenePath);
        playProbeStage = 0;
        nextPlayProbeTime = EditorApplication.timeSinceStartup + 0.75d;

        EditorApplication.update -= RunPlayProbeStep;
        EditorApplication.update += RunPlayProbeStep;
    }

    private static void RunPlayProbeStep()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update -= RunPlayProbeStep;
            playProbeReport = null;
            return;
        }

        if (EditorApplication.timeSinceStartup < nextPlayProbeTime)
        {
            return;
        }

        switch (playProbeStage)
        {
            case 0:
                TopdownMenuUIController menuController = Object.FindObjectOfType<TopdownMenuUIController>(true);
                if (menuController != null)
                {
                    menuController.ForceStartGameplayForAutomatedTest();
                }

                ClearRuntimeProbeShadows();
                AppendRequiredObjectChecks(playProbeReport);
                bridgeShadow = CreatePastedShadowFromSource("TreeShadow_Topdown", CrossingPosition);
                playProbeReport.AppendLine("TreeShadow pasted at bridge: " + PassFail(bridgeShadow != null));
                AdvancePlayProbe(1, 1.00d);
                break;
            case 1:
                TopdownBridgeCrossingZone crossing = FindComponent<TopdownBridgeCrossingZone>("CrossingHint_01");
                GameObject blocker = FindSceneObject("Wall_River_BrokenBridge_Blocker");
                if (blocker == null)
                {
                    blocker = FindSceneObject("River_CrossingBlocker_Accurate");
                }
                GameObject player = FindSceneObject("Player_Topdown");
                if (player != null && bridgeShadow != null)
                {
                    player.transform.position = bridgeShadow.transform.position;
                    Physics2D.SyncTransforms();
                }

                InvokePrivateUpdate(crossing);
                bool bridgeCollisionIgnored = AnyPlayerBlockerCollisionIgnored(player, blocker);
                bool blockerStillActive = blocker != null && blocker.activeSelf;

                if (player != null && bridgeShadow != null && bridgeShadow.ShapeCollider != null)
                {
                    Bounds shadowBounds = bridgeShadow.ShapeCollider.bounds;
                    Bounds playerBounds = GetColliderBounds(player);
                    float yOffset = shadowBounds.extents.y + playerBounds.extents.y + 0.35f;
                    player.transform.position = shadowBounds.center + new Vector3(0f, yOffset, 0f);
                    Physics2D.SyncTransforms();
                }

                InvokePrivateUpdate(crossing);
                bool uncoveredWaterBlocked = !AnyPlayerBlockerCollisionIgnored(player, blocker) && blockerStillActive;
                bool bridgePass = crossing != null && crossing.IsOpen && blockerStillActive && bridgeCollisionIgnored && uncoveredWaterBlocked;
                playProbeReport.AppendLine("TreeShadow crossing open: " + PassFail(bridgePass));
                playProbeReport.AppendLine("River blocker remains active: " + PassFail(blockerStillActive));
                playProbeReport.AppendLine("TreeShadow bridge only ignores blocker while player overlaps shadow: " + PassFail(bridgeCollisionIgnored));
                playProbeReport.AppendLine("Uncovered water keeps blocker collision: " + PassFail(uncoveredWaterBlocked));
                pressShadow = CreatePastedShadowFromSource("BeamShadow_Topdown", PressPlateAnchor);
                playProbeReport.AppendLine("CanPress shadow pasted at plate: " + PassFail(pressShadow != null && pressShadow.CanPress));
                AdvancePlayProbe(2, 1.00d);
                break;
            case 2:
                ShadowPressureTrigger pressureTrigger = FindComponent<ShadowPressureTrigger>("PressurePlate_Topdown");
                DoorController pressureDoor = FindComponent<DoorController>("Door_Pressure_Topdown");
                bool doorBlocksWhenClosed = pressureDoor != null && !pressureDoor.IsOpen && AnyDoorBlockingColliderEnabled(pressureDoor);
                playProbeReport.AppendLine("Door blocks when closed: " + PassFail(doorBlocksWhenClosed));

                if (pressShadow != null)
                {
                    InvokePrivateTrigger(pressureTrigger, "OnTriggerEnter2D", pressShadow.ShapeCollider);
                }

                PressurePlateController plate = FindComponent<PressurePlateController>("PressurePlate_Topdown");
                bool pressPass = plate != null && plate.IsPressed && pressureDoor != null && pressureDoor.IsOpen;
                playProbeReport.AppendLine("CanPress plate opens door: " + PassFail(pressPass));
                bool doorPassableWhenOpen = pressureDoor != null && pressureDoor.IsOpen && !AnyDoorBlockingColliderEnabled(pressureDoor);
                playProbeReport.AppendLine("Door passable when open: " + PassFail(doorPassableWhenOpen));
                bool noHiddenDoorBlocker = pressureDoor != null && !HasEnabledDoorPassageBlocker(pressureDoor.gameObject);
                playProbeReport.AppendLine("No hidden blocker after door opens: " + PassFail(noHiddenDoorBlocker));
                FreeShadowPlacer placer = FindComponent<FreeShadowPlacer>("Player_Topdown");
                playProbeReport.AppendLine("Held shadow placement radius close: " + PassFail(GetSerializedFloat(placer, "placementRadius") <= 1.1f));
                unlockShadow = CreatePastedShadowFromSource("KeyShadow_Topdown", KeyLockAnchor);
                playProbeReport.AppendLine("CanUnlock shadow pasted at key door: " + PassFail(unlockShadow != null && unlockShadow.CanUnlock));
                AdvancePlayProbe(3, 1.00d);
                break;
            case 3:
                KeyDoorDirectUnlockTrigger keyDoorTrigger = FindComponent<KeyDoorDirectUnlockTrigger>("Door_Lock_Topdown_DirectUnlockTrigger");
                DoorController lockDoor = FindComponent<DoorController>("Door_Lock_Topdown");
                keyDoorTrigger?.ResetDoor();

                Collider2D playerCollider = FindComponent<Collider2D>("Player_Topdown");
                InvokePrivateTrigger(keyDoorTrigger, "OnTriggerEnter2D", playerCollider);
                bool playerBodyDoesNotUnlock = keyDoorTrigger != null && !keyDoorTrigger.IsUnlocked && lockDoor != null && !lockDoor.IsOpen;
                playProbeReport.AppendLine("Player body does not unlock key door: " + PassFail(playerBodyDoesNotUnlock));

                if (pressShadow != null)
                {
                    InvokePrivateTrigger(keyDoorTrigger, "OnTriggerEnter2D", pressShadow.ShapeCollider);
                }

                bool canPressDoesNotUnlock = keyDoorTrigger != null && !keyDoorTrigger.IsUnlocked && lockDoor != null && !lockDoor.IsOpen;
                playProbeReport.AppendLine("CanPress shadow does not unlock key door: " + PassFail(canPressDoesNotUnlock));

                PastedShadowObject temporaryPlayerShadow = CreatePlayerLureShadow(KeyLockAnchor + new Vector3(-0.6f, 0.25f, 0f));
                if (temporaryPlayerShadow != null)
                {
                    InvokePrivateTrigger(keyDoorTrigger, "OnTriggerEnter2D", temporaryPlayerShadow.ShapeCollider);
                }

                bool playerShadowDoesNotUnlock = keyDoorTrigger != null && !keyDoorTrigger.IsUnlocked && lockDoor != null && !lockDoor.IsOpen;
                playProbeReport.AppendLine("PlayerShadow does not unlock key door: " + PassFail(playerShadowDoesNotUnlock));
                if (temporaryPlayerShadow != null)
                {
                    Object.Destroy(temporaryPlayerShadow.gameObject);
                }

                if (unlockShadow != null)
                {
                    InvokePrivateTrigger(keyDoorTrigger, "OnTriggerEnter2D", unlockShadow.ShapeCollider);
                }

                bool unlockPass = keyDoorTrigger != null && keyDoorTrigger.IsUnlocked && lockDoor != null && lockDoor.IsOpen;
                playProbeReport.AppendLine("CanUnlock pasted shadow unlocks key door: " + PassFail(unlockPass));
                playProbeReport.AppendLine("CanUnlock opens key door: " + PassFail(unlockPass));
                wrongLureShadow = CreatePastedShadowFromSource("TreeShadow_Topdown", LureMidpoint + new Vector3(2.0f, 0.1f, 0f));
                playProbeReport.AppendLine("Non-attract shadow placed in seeker radius: " + PassFail(wrongLureShadow != null && !wrongLureShadow.CanAttractEnemy));
                AdvancePlayProbe(4, 0.35d);
                break;
            case 4:
                EnemyShadowSeeker seekerBeforeLure = FindComponent<EnemyShadowSeeker>("ShadowSeeker_Topdown");
                InvokePrivateUpdate(seekerBeforeLure);
                bool wrongLureIgnored = seekerBeforeLure != null && seekerBeforeLure.CurrentTarget != wrongLureShadow;
                playProbeReport.AppendLine("Non-attract shadow ignored by seeker: " + PassFail(wrongLureIgnored));
                if (wrongLureShadow != null)
                {
                    Object.Destroy(wrongLureShadow.gameObject);
                }

                playerLureShadow = CreatePlayerLureShadow(LureMidpoint + new Vector3(2.50f, 0.15f, 0f));
                playProbeReport.AppendLine("PlayerShadow lure created in detection radius: " + PassFail(playerLureShadow != null && playerLureShadow.CanAttractEnemy));
                AdvancePlayProbe(5, 0.50d);
                break;
            case 5:
                EnemyShadowSeeker seeker = FindComponent<EnemyShadowSeeker>("ShadowSeeker_Topdown");
                InvokePrivateUpdate(seeker);
                bool lurePass = seeker != null && seeker.CurrentTarget == playerLureShadow && seeker.IsChasingShadow;
                playProbeReport.AppendLine("ShadowSeeker follows PlayerShadow: " + PassFail(lurePass));
                TopdownFinalClockCore finalCore = FindComponent<TopdownFinalClockCore>("FinalClockCore_Topdown");
                if (finalCore != null)
                {
                    finalCore.ActivateClockCore();
                }

                AdvancePlayProbe(6, 0.20d);
                break;
            case 6:
                TopdownFinalClockCore activatedCore = FindComponent<TopdownFinalClockCore>("FinalClockCore_Topdown");
                playProbeReport.AppendLine("FinalClockCore activates without Reveal View: " + PassFail(activatedCore != null && activatedCore.IsActivated));
                FinishPlayProbe();
                break;
        }
    }

    private static void AdvancePlayProbe(int nextStage, double delay)
    {
        playProbeStage = nextStage;
        nextPlayProbeTime = EditorApplication.timeSinceStartup + delay;
    }

    private static void FinishPlayProbe()
    {
        EditorApplication.update -= RunPlayProbeStep;
        string requestPath = FullPath(PlayProbeRequestPath);
        if (File.Exists(requestPath))
        {
            File.Delete(requestPath);
        }

        File.WriteAllText(FullPath(PlayProbeReportPath), playProbeReport.ToString());
        Debug.Log("Topdown pixel manifest play probe complete. Report: " + FullPath(PlayProbeReportPath));
        playProbeReport = null;

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
            return;
        }

        EditorApplication.isPlaying = false;
    }

    private static void InvokePrivateUpdate(MonoBehaviour behaviour)
    {
        if (behaviour == null)
        {
            return;
        }

        System.Reflection.MethodInfo method = behaviour.GetType().GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (method != null)
        {
            method.Invoke(behaviour, null);
        }
    }

    private static void InvokePrivateTrigger(MonoBehaviour behaviour, string methodName, Collider2D collider)
    {
        if (behaviour == null || collider == null)
        {
            return;
        }

        System.Reflection.MethodInfo method = behaviour.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (method != null)
        {
            method.Invoke(behaviour, new object[] { collider });
        }
    }

    private static bool AnyPlayerBlockerCollisionIgnored(GameObject player, GameObject blocker)
    {
        if (player == null || blocker == null)
        {
            return false;
        }

        Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>(true);
        Collider2D[] blockerColliders = blocker.GetComponentsInChildren<Collider2D>(true);
        for (int playerIndex = 0; playerIndex < playerColliders.Length; playerIndex++)
        {
            Collider2D playerCollider = playerColliders[playerIndex];
            if (playerCollider == null || playerCollider.isTrigger)
            {
                continue;
            }

            for (int blockerIndex = 0; blockerIndex < blockerColliders.Length; blockerIndex++)
            {
                Collider2D blockerCollider = blockerColliders[blockerIndex];
                if (blockerCollider != null && Physics2D.GetIgnoreCollision(playerCollider, blockerCollider))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static Bounds GetColliderBounds(GameObject gameObject)
    {
        Collider2D[] colliders = gameObject != null
            ? gameObject.GetComponentsInChildren<Collider2D>(true)
            : new Collider2D[0];

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            if (collider != null && !collider.isTrigger)
            {
                return collider.bounds;
            }
        }

        return new Bounds(gameObject != null ? gameObject.transform.position : Vector3.zero, Vector3.zero);
    }

    private static bool AnyDoorBlockingColliderEnabled(DoorController door)
    {
        if (door == null)
        {
            return false;
        }

        SerializedObject serializedDoor = new SerializedObject(door);
        Collider2D doorCollider = serializedDoor.FindProperty("doorCollider")?.objectReferenceValue as Collider2D;
        if (doorCollider != null && doorCollider.enabled && !doorCollider.isTrigger)
        {
            return true;
        }

        SerializedProperty extras = serializedDoor.FindProperty("additionalClosedColliders");
        if (extras != null)
        {
            for (int i = 0; i < extras.arraySize; i++)
            {
                Collider2D extra = extras.GetArrayElementAtIndex(i).objectReferenceValue as Collider2D;
                if (extra != null && extra.enabled && !extra.isTrigger)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasEnabledDoorPassageBlocker(GameObject door)
    {
        if (door == null)
        {
            return false;
        }

        Collider2D doorCollider = door.GetComponent<Collider2D>();
        if (doorCollider == null)
        {
            return false;
        }

        Bounds passageBounds = doorCollider.bounds;
        passageBounds.Expand(new Vector3(0.2f, 0.2f, 0f));

        Collider2D[] colliders = Object.FindObjectsOfType<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            if (collider == null
                || !collider.enabled
                || collider.isTrigger
                || collider == doorCollider
                || collider.gameObject == door
                || collider.transform.IsChildOf(door.transform)
                || !LooksLikeDoorPassageBlocker(collider.gameObject)
                || !IsCentralDoorBlocker(collider, doorCollider)
                || !collider.bounds.Intersects(passageBounds))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private static bool IsCentralDoorBlocker(Collider2D candidate, Collider2D doorCollider)
    {
        if (candidate == null || doorCollider == null)
        {
            return false;
        }

        Bounds doorBounds = doorCollider.bounds;
        float minX = doorBounds.min.x + doorBounds.size.x * 0.18f;
        float maxX = doorBounds.max.x - doorBounds.size.x * 0.18f;
        float minY = doorBounds.min.y - 0.2f;
        float maxY = doorBounds.max.y + 0.2f;
        Vector3 center = candidate.bounds.center;
        return center.x >= minX && center.x <= maxX && center.y >= minY && center.y <= maxY;
    }

    private static bool LooksLikeDoorPassageBlocker(GameObject gameObject)
    {
        string objectName = gameObject.name;
        if (objectName.Contains("graybox_template")
            || objectName.Contains("Graybox")
            || objectName.Contains("Collision")
            || objectName.Contains("Blocker")
            || objectName.StartsWith("Wall_"))
        {
            return true;
        }

        Transform parent = gameObject.transform.parent;
        while (parent != null)
        {
            string parentName = parent.name;
            if (parentName.Contains("Graybox") || parentName.Contains("Collision"))
            {
                return true;
            }

            parent = parent.parent;
        }

        return false;
    }

    private static float GetSerializedFloat(Component component, string propertyName)
    {
        if (component == null)
        {
            return 0f;
        }

        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property != null ? property.floatValue : 0f;
    }

    private static void AppendRequiredObjectChecks(StringBuilder report)
    {
        report.AppendLine("Player exists: " + PassFail(Exists("Player_Topdown")));
        report.AppendLine("TreeShadow exists: " + PassFail(Exists("TreeShadow_Topdown")));
        report.AppendLine("Bridge trigger exists: " + PassFail(Exists("CrossingHint_01")));
        report.AppendLine("Pressure plate exists: " + PassFail(Exists("PressurePlate_Topdown")));
        report.AppendLine("Lock exists: " + PassFail(Exists("Lock_Topdown")));
        report.AppendLine("ShadowSeeker exists: " + PassFail(Exists("ShadowSeeker_Topdown")));
        report.AppendLine("FinalClockCore exists: " + PassFail(Exists("FinalClockCore_Topdown")));
    }

    private static PastedShadowObject CreatePastedShadowFromSource(string sourceName, Vector3 position)
    {
        ShadowInteractable source = FindComponent<ShadowInteractable>(sourceName);
        if (source == null)
        {
            return null;
        }

        ShadowItemData data = source.CreateItemData();
        if (data == null || !data.IsValid())
        {
            return null;
        }

        return CreatePastedShadow(data, position, sourceName + "_ProbePaste");
    }

    private static PastedShadowObject CreatePlayerLureShadow(Vector3 position)
    {
        ShadowItemData data = new ShadowItemData
        {
            shadowType = ShadowType.Player,
            displayName = "Player Shadow",
            spriteDrawMode = SpriteDrawMode.Simple,
            spriteSize = new Vector2(1f, 1.6f),
            localScale = new Vector3(1.2f, 1.8f, 1f),
            rotation = Quaternion.identity,
            approximateSize = new Vector2(1f, 1.6f),
            colliderSize = new Vector2(1f, 1.6f),
            colliderOffset = Vector2.zero,
            canStandOn = false,
            canPress = false,
            canUnlock = false,
            canAttractEnemy = true,
            canBlock = false,
            canTriggerMechanism = false,
            returnsToPlayer = true,
            recallBlocked = false
        };

        return CreatePastedShadow(data, position, "PlayerShadow_ProbePaste");
    }

    private static PastedShadowObject CreatePastedShadow(ShadowItemData data, Vector3 position, string objectName)
    {
        data.spriteDrawMode = SpriteDrawMode.Simple;

        GameObject pastedObject = new GameObject(objectName);
        GameObject parent = FindSceneObject("ShadowVisuals");
        if (parent != null)
        {
            pastedObject.transform.SetParent(parent.transform, true);
        }

        pastedObject.transform.position = position;
        pastedObject.transform.rotation = data.rotation;
        pastedObject.transform.localScale = data.localScale;

        SpriteRenderer renderer = pastedObject.AddComponent<SpriteRenderer>();
        renderer.sprite = data.sprite;
        renderer.drawMode = data.spriteDrawMode;
        if (data.spriteDrawMode == SpriteDrawMode.Sliced || data.spriteDrawMode == SpriteDrawMode.Tiled)
        {
            renderer.size = data.spriteSize;
        }

        renderer.color = new Color(0f, 0f, 0f, 0.65f);
        renderer.sortingOrder = 80;

        BoxCollider2D collider = pastedObject.AddComponent<BoxCollider2D>();
        collider.size = data.colliderSize;
        collider.offset = data.colliderOffset;
        collider.isTrigger = !data.canStandOn;

        PastedShadowObject pastedShadow = pastedObject.AddComponent<PastedShadowObject>();
        pastedShadow.Initialize(data);
        Physics2D.SyncTransforms();
        return pastedShadow;
    }

    private static void ClearRuntimeProbeShadows()
    {
        PastedShadowObject[] pastedShadows = Object.FindObjectsOfType<PastedShadowObject>();
        for (int i = 0; i < pastedShadows.Length; i++)
        {
            if (pastedShadows[i] != null && pastedShadows[i].name.Contains("ProbePaste"))
            {
                Object.Destroy(pastedShadows[i].gameObject);
            }
        }
    }

    private static void ConfigureShadowInteractable(string objectName, string displayName, bool canStandOn, bool canPress, bool canUnlock, bool canAttractEnemy, bool canBlock)
    {
        ShadowInteractable shadow = FindComponent<ShadowInteractable>(objectName);
        if (shadow == null)
        {
            return;
        }

        SerializedObject serializedShadow = new SerializedObject(shadow);
        SetString(serializedShadow, "displayName", displayName);
        SetBool(serializedShadow, "canStandOn", canStandOn);
        SetBool(serializedShadow, "canPress", canPress);
        SetBool(serializedShadow, "canUnlock", canUnlock);
        SetBool(serializedShadow, "canAttractEnemy", canAttractEnemy);
        SetBool(serializedShadow, "canBlock", canBlock);
        serializedShadow.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(shadow);
    }

    private static void ConfigureLocalBoxCollider(string objectName, Vector2 size, Vector2 offset, bool isTrigger)
    {
        GameObject gameObject = FindSceneObject(objectName);
        if (gameObject == null)
        {
            return;
        }

        BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.size = size;
        boxCollider.offset = offset;
        boxCollider.isTrigger = isTrigger;
        EditorUtility.SetDirty(boxCollider);
    }

    private static void FixTutorialSignColliders()
    {
        TutorialSign[] signs = Resources.FindObjectsOfTypeAll<TutorialSign>();
        for (int i = 0; i < signs.Length; i++)
        {
            TutorialSign sign = signs[i];
            if (sign == null || !sign.gameObject.scene.IsValid() || sign.gameObject.scene.path != ScenePath)
            {
                continue;
            }

            BoxCollider2D collider = sign.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
                EditorUtility.SetDirty(collider);
            }
        }
    }

    private static void DisableLegacyBlockers()
    {
        string[] oldNames =
        {
            "Ground", "Ground_Base", "River_Gap_01", "RiverBlocker_Right", "EnemyPassage_Topdown",
            "CrossingBlocker_01", "Area_01_TreeCrossing", "Area_02_PressureDoor", "Area_03_LockDoor",
            "Area_04_EnemyLure", "Goal_Platform", "River_Bank_Left", "River_Bank_Right",
            "River_CrossingBlocker_FromMap", "River_CrossingBlocker_FromMask",
            "Wall_Room01_West", "Wall_Room01_South", "Wall_Room01_East",
            "Wall_Room02_West", "Wall_Room02_North", "Wall_Room02_East", "Wall_SeekerCorridor_North",
            "Wall_SeekerCorridor_South", "Wall_Final_North", "Wall_Final_East", "Wall_Final_South",
            "Wall_LowerRoom_West_FromMask", "Wall_LowerRoom_South_FromMask", "Wall_LowerRoom_East_FromMask",
            "Wall_UpperRoom_West_FromMask", "Wall_UpperRoom_North_FromMask", "Wall_UpperRoom_East_FromMask",
            "Wall_SeekerCorridor_North_FromMask", "Wall_SeekerCorridor_South_FromMask",
            "Wall_Final_North_FromMask", "Wall_Final_East_FromMask", "Wall_Final_South_FromMask"
        };

        for (int i = 0; i < oldNames.Length; i++)
        {
            GameObject oldObject = FindSceneObject(oldNames[i]);
            if (oldObject != null)
            {
                oldObject.SetActive(false);
                EditorUtility.SetDirty(oldObject);
            }
        }
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
            || objectName == "Level01_MapReference"
            || objectName == "Level01_FinalMap_Reference"
            || objectName == "Level01_OutlineOverlay_Reference";
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

    private static void ValidateCleanupState(StringBuilder report)
    {
        Physics2D.SyncTransforms();

        report.AppendLine("cleanup.Level01_MapReference=" + PassFail(Exists(MapReferenceRootName)));
        report.AppendLine("cleanup.finalMapReference=" + PassFail(Exists("Level01_FinalMap_Reference")));
        report.AppendLine("cleanup.outlineReference=" + PassFail(Exists("Level01_OutlineOverlay_Reference")));
        report.AppendLine("cleanup.AccurateRootRemoved=" + PassFail(!Exists(AccurateRootName)));
        report.AppendLine("cleanup.SimplifiedRootRemoved=" + PassFail(!Exists(SimplifiedRootName)));
        report.AppendLine("cleanup.PixelShardRootRemoved=" + PassFail(!Exists(PixelShardRootName)));
        report.AppendLine("cleanup.GameplayAnchorsRemoved=" + PassFail(!Exists(GameplayAnchorsRootName)));
        report.AppendLine("cleanup.deprecatedBlockoutNameCount=" + CountDeprecatedBlockoutObjects());
        report.AppendLine("cleanup.Player=" + PassFail(Exists("Player_Topdown")));
        report.AppendLine("cleanup.TreeShadow=" + PassFail(Exists("TreeShadow_Topdown")));
        report.AppendLine("cleanup.PressurePlate=" + PassFail(Exists("PressurePlate_Topdown")));
        report.AppendLine("cleanup.Lock=" + PassFail(Exists("Lock_Topdown")));
        report.AppendLine("cleanup.ShadowSeeker=" + PassFail(Exists("ShadowSeeker_Topdown")));
        report.AppendLine("cleanup.FinalClockCore=" + PassFail(Exists("FinalClockCore_Topdown")));
        report.AppendLine("cleanup.playerSpawnBlocked=" + PassFail(!IsPlayerSpawnBlocked()));
    }

    private static void CleanupDeprecatedBlockoutRootsForWallPass(StringBuilder report)
    {
        int deletedObjects = 0;
        deletedObjects += DestroySceneObjectAndReport(AccurateRootName, report);
        deletedObjects += DestroySceneObjectAndReport(SimplifiedRootName, report);
        deletedObjects += DestroySceneObjectAndReport(PixelShardRootName, report);
        deletedObjects += DestroySceneObjectAndReport(OldLayoutRootName, report);
        deletedObjects += DestroySceneObjectAndReport(OldStartTuningRootName, report);
        deletedObjects += DestroySceneObjectAndReport(GameplayAnchorsRootName, report);
        deletedObjects += DestroySceneObjectAndReport("Level01_ManualWalls", report);
        deletedObjects += DestroyDeprecatedBlockoutObjectsByName(report);
        report.AppendLine("wallPass.deletedDeprecatedObjects=" + deletedObjects);
    }

    private static Transform RecreateManualWallsRoot()
    {
        DestroySceneObject("Level01_ManualWalls");
        GameObject world = EnsureRoot("World");
        GameObject manualWalls = new GameObject("Level01_ManualWalls");
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

        report.AppendLine("manualWallsRoot=Level01_ManualWalls");
        report.AppendLine("manualWalls.scope=start_island_and_river_only");
        report.AppendLine("manualWalls.created=7");
    }

    private static void ConfigureStartRiverWallGameplayLinks(StringBuilder report)
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

    private static void ValidateStartRiverWallState(StringBuilder report)
    {
        Physics2D.SyncTransforms();

        report.AppendLine("startRiver.Level01_MapReference=" + PassFail(Exists(MapReferenceRootName)));
        report.AppendLine("startRiver.finalMapReference=" + PassFail(Exists("Level01_FinalMap_Reference")));
        report.AppendLine("startRiver.outlineReference=" + PassFail(Exists("Level01_OutlineOverlay_Reference")));
        report.AppendLine("startRiver.manualWallsRoot=" + PassFail(Exists("Level01_ManualWalls")));
        report.AppendLine("startRiver.manualWallCount=" + CountManualWalls());
        report.AppendLine("startRiver.onlyStartRiverWalls=" + PassFail(NoForbiddenManualWallNames()));
        report.AppendLine("startRiver.playerSpawnBlocked=" + PassFail(!IsPlayerSpawnBlocked()));
        report.AppendLine("startRiver.bridgeBlockerExists=" + PassFail(Exists("Wall_River_BrokenBridge_Blocker")));
        report.AppendLine("startRiver.bridgeBlockerBlocksBeforeShadow=" + PassFail(IsPointBlockedBy("Wall_River_BrokenBridge_Blocker", V2(CrossingPosition))));
        report.AppendLine("startRiver.wallComponentsValid=" + PassFail(ManualWallsHaveRequiredComponents()));
    }

    private static int CountManualWalls()
    {
        GameObject root = FindSceneObject("Level01_ManualWalls");
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
        GameObject root = FindSceneObject("Level01_ManualWalls");
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
        GameObject root = FindSceneObject("Level01_ManualWalls");
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

    private static int CountDeprecatedBlockoutObjects()
    {
        int count = 0;
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (sceneObject != null
                && sceneObject.scene.IsValid()
                && sceneObject.scene.path == ScenePath
                && ShouldDeleteDeprecatedBlockoutObject(sceneObject))
            {
                count++;
            }
        }

        return count;
    }

    private static bool IsPlayerSpawnBlocked()
    {
        GameObject player = FindSceneObject("Player_Topdown");
        if (player == null)
        {
            return true;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(player.transform.position, 0.24f);
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

    private static bool IsRouteClear(Vector2[] points, out string blockedBy)
    {
        blockedBy = string.Empty;
        for (int i = 0; i < points.Length - 1; i++)
        {
            if (!IsSegmentClear(points[i], points[i + 1], out blockedBy))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsSegmentClear(Vector2 from, Vector2 to, out string blockedBy)
    {
        blockedBy = string.Empty;
        float distance = Vector2.Distance(from, to);
        int steps = Mathf.Max(2, Mathf.CeilToInt(distance / 0.35f));
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            Vector2 point = Vector2.Lerp(from, to, t);
            Collider2D[] hits = Physics2D.OverlapCircleAll(point, 0.24f);
            for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                Collider2D hit = hits[hitIndex];
                if (hit == null || hit.isTrigger || IsAllowedRouteCollider(hit))
                {
                    continue;
                }

                blockedBy = hit.name + " at " + point;
                return false;
            }
        }

        return true;
    }

    private static bool IsAllowedRouteCollider(Collider2D collider)
    {
        string objectName = collider.gameObject.name;
        if (objectName == "Player_Topdown"
            || objectName == "PlayerLantern"
            || objectName == "ShadowSeeker_Topdown"
            || objectName == "Door_Pressure_Topdown"
            || objectName == "Door_Lock_Topdown"
            || objectName == "Wall_River_BrokenBridge_Blocker"
            || objectName == "River_CrossingBlocker_Accurate")
        {
            return true;
        }

        if (collider.GetComponent<PastedShadowObject>() != null || collider.GetComponentInParent<PastedShadowObject>() != null)
        {
            return true;
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

    private static GameObject MoveObject(string objectName, Vector3 position, Transform parent, Vector3 scale)
    {
        GameObject gameObject = FindSceneObject(objectName);
        if (gameObject == null)
        {
            return null;
        }

        gameObject.transform.SetParent(parent, true);
        gameObject.transform.position = new Vector3(position.x, position.y, gameObject.transform.position.z);
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localScale = scale;
        EditorUtility.SetDirty(gameObject);
        return gameObject;
    }

    private static GameObject CreateBox(Transform parent, string name, Vector3 center, Vector2 size, Color color, bool collider, bool trigger, int sortingOrder)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);

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

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = CreateMaterial(color);
        meshRenderer.sortingOrder = sortingOrder;

        if (collider)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.offset = new Vector2(center.x, center.y);
            boxCollider.size = size;
            boxCollider.isTrigger = trigger;
        }

        return gameObject;
    }

    private static GameObject CreateWallSegment(Transform parent, string name, Vector3 start, Vector3 end, float thickness, Color color, bool collider, int sortingOrder)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);

        Vector3 center = (start + end) * 0.5f;
        Vector2 delta = new Vector2(end.x - start.x, end.y - start.y);
        float length = delta.magnitude;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

        gameObject.transform.position = center;
        gameObject.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        float halfLength = length * 0.5f;
        float halfThickness = thickness * 0.5f;
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-halfLength, -halfThickness, 0f),
            new Vector3(halfLength, -halfThickness, 0f),
            new Vector3(halfLength, halfThickness, 0f),
            new Vector3(-halfLength, halfThickness, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateBounds();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = CreateMaterial(color);
        meshRenderer.sortingOrder = sortingOrder;

        if (collider)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(length, thickness);
            boxCollider.offset = Vector2.zero;
            boxCollider.isTrigger = false;
        }

        return gameObject;
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

    private static GameObject CreatePolygon(Transform parent, string name, Color color, Vector2[] points, int sortingOrder)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = new Vector3(points[i].x, points[i].y, 0f);
        }

        mesh.vertices = vertices;
        mesh.triangles = Triangulate(points).ToArray();
        mesh.RecalculateBounds();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = CreateMaterial(color);
        meshRenderer.sortingOrder = sortingOrder;
        return gameObject;
    }

    private static List<int> Triangulate(Vector2[] sourcePoints)
    {
        List<int> triangles = new List<int>();
        List<int> indices = new List<int>();
        for (int i = 0; i < sourcePoints.Length; i++)
        {
            indices.Add(i);
        }

        bool ccw = SignedArea(sourcePoints) > 0f;
        int guard = 0;
        while (indices.Count > 3 && guard < 500)
        {
            guard++;
            bool clipped = false;
            for (int i = 0; i < indices.Count; i++)
            {
                int previousIndex = indices[(i - 1 + indices.Count) % indices.Count];
                int currentIndex = indices[i];
                int nextIndex = indices[(i + 1) % indices.Count];
                Vector2 previous = sourcePoints[previousIndex];
                Vector2 current = sourcePoints[currentIndex];
                Vector2 next = sourcePoints[nextIndex];

                if (!IsConvex(previous, current, next, ccw))
                {
                    continue;
                }

                bool containsPoint = false;
                for (int other = 0; other < indices.Count; other++)
                {
                    int otherIndex = indices[other];
                    if (otherIndex == previousIndex || otherIndex == currentIndex || otherIndex == nextIndex)
                    {
                        continue;
                    }

                    if (PointInTriangle(sourcePoints[otherIndex], previous, current, next))
                    {
                        containsPoint = true;
                        break;
                    }
                }

                if (containsPoint)
                {
                    continue;
                }

                if (ccw)
                {
                    triangles.Add(previousIndex);
                    triangles.Add(currentIndex);
                    triangles.Add(nextIndex);
                }
                else
                {
                    triangles.Add(nextIndex);
                    triangles.Add(currentIndex);
                    triangles.Add(previousIndex);
                }

                indices.RemoveAt(i);
                clipped = true;
                break;
            }

            if (!clipped)
            {
                break;
            }
        }

        if (indices.Count == 3)
        {
            if (ccw)
            {
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
            }
            else
            {
                triangles.Add(indices[2]);
                triangles.Add(indices[1]);
                triangles.Add(indices[0]);
            }
        }

        return triangles;
    }

    private static float SignedArea(Vector2[] points)
    {
        float area = 0f;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % points.Length];
            area += current.x * next.y - next.x * current.y;
        }

        return area * 0.5f;
    }

    private static bool IsConvex(Vector2 previous, Vector2 current, Vector2 next, bool ccw)
    {
        float cross = Cross(current - previous, next - current);
        return ccw ? cross > 0f : cross < 0f;
    }

    private static bool PointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = Mathf.Abs(Cross(b - a, c - a));
        float area1 = Mathf.Abs(Cross(a - point, b - point));
        float area2 = Mathf.Abs(Cross(b - point, c - point));
        float area3 = Mathf.Abs(Cross(c - point, a - point));
        return Mathf.Abs(area - (area1 + area2 + area3)) <= 0.0001f;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private static Transform CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        return material;
    }

    private static string FindReferenceFile(string fileName)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string workspaceRoot = Path.GetFullPath(Path.Combine(projectRoot, ".."));
        string[] matches = Directory.GetFiles(workspaceRoot, fileName, SearchOption.AllDirectories);
        return matches.Length > 0 ? matches[0] : string.Empty;
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

    private static Vector3 W(float pixelX, float pixelY)
    {
        return new Vector3((pixelX - CanvasWidth * 0.5f) / PixelsPerUnit, (CanvasHeight * 0.5f - pixelY) / PixelsPerUnit, 0f);
    }

    private static Vector2 P(float pixelX, float pixelY)
    {
        return new Vector2((pixelX - CanvasWidth * 0.5f) / PixelsPerUnit, (CanvasHeight * 0.5f - pixelY) / PixelsPerUnit);
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

    private static void SetString(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
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

    private static void SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.vector3Value = value;
        }
    }
}
