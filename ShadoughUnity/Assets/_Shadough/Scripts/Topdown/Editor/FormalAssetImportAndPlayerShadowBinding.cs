using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FormalAssetImportAndPlayerShadowBinding
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string ReportPath = "Logs/FormalAssetImportAndPlayerShadowBinding.report.txt";
    private const string PlayProbeRequestPath = "Temp/FormalPlayerShadowPlayProbe.request";
    private const string PlayProbeReportPath = "Logs/FormalPlayerShadowPlayProbe.report.txt";
    private const string PlayerBasePath = "Assets/_Shadough/Sprites/Shadows/Player/player_shadow_base.png";
    private const string PlayerPastedPath = "Assets/_Shadough/Sprites/Shadows/Pasted/player_shadow_lure_pasted.png";
    private const string BeamSourcePath = "Assets/_Shadough/Sprites/Props/Beam/wood_beam_source.png";
    private const string BeamBasePath = "Assets/_Shadough/Sprites/Shadows/Beam/beam_shadow_base.png";
    private const string BeamPastedPath = "Assets/_Shadough/Sprites/Shadows/Pasted/beam_shadow_pasted.png";
    private const string KeyNormalPath = "Assets/_Shadough/Sprites/Interactables/Key/key_normal.png";
    private const string KeyGlowPath = "Assets/_Shadough/Sprites/Interactables/Key/key_glow.png";
    private const string KeyBasePath = "Assets/_Shadough/Sprites/Shadows/Key/key_shadow_base.png";
    private const string KeyPastedPath = "Assets/_Shadough/Sprites/Shadows/Pasted/key_shadow_pasted.png";
    private const string CoreIdlePath = "Assets/_Shadough/Sprites/Interactables/FinalClockCore/final_clock_core_idle.png";
    private const string CoreActivePath = "Assets/_Shadough/Sprites/Interactables/FinalClockCore/final_clock_core_active.png";
    private const string CorePlatformPath = "Assets/_Shadough/Sprites/Interactables/FinalClockCore/final_clock_core_platform.png";

    private static readonly string[] ImportPaths =
    {
        "Assets/_Shadough/Sprites/Shadows/Beam/beam_shadow_base.png",
        "Assets/_Shadough/Sprites/Shadows/Pasted/beam_shadow_pasted.png",
        "Assets/_Shadough/Sprites/Props/Beam/wood_beam_source.png",
        "Assets/_Shadough/Sprites/Shadows/Key/key_shadow_base.png",
        "Assets/_Shadough/Sprites/Shadows/Pasted/key_shadow_pasted.png",
        "Assets/_Shadough/Sprites/Interactables/Key/key_normal.png",
        "Assets/_Shadough/Sprites/Interactables/Key/key_glow.png",
        PlayerBasePath,
        PlayerPastedPath,
        "Assets/_Shadough/Sprites/Shadows/Tree/tree_shadow_base.png",
        "Assets/_Shadough/Sprites/Shadows/Pasted/tree_shadow_pasted.png",
        "Assets/_Shadough/Sprites/Interactables/FinalClockCore/final_clock_core_idle.png",
        "Assets/_Shadough/Sprites/Interactables/FinalClockCore/final_clock_core_active.png",
        "Assets/_Shadough/Sprites/Interactables/FinalClockCore/final_clock_core_platform.png"
    };

    private static readonly string[] ProtectedObjectNames =
    {
        "Level01_MapReference",
        "Level01_FinalMap_Reference",
        "Level01_ManualWalls",
        "Main Camera",
        "Player",
        "Player_Topdown",
        "Tree_01",
        "TreeShadowRootAnchor_Topdown",
        "TreeShadow_Topdown",
        "PressurePlate_Topdown",
        "Door_Pressure_Topdown",
        "KeySource_Topdown",
        "KeyShadow_Topdown",
        "Door_Lock_Topdown",
        "Lock_Topdown",
        "ShadowSeeker_Topdown",
        "ShadowSeeker_Home_Topdown",
        "FinalClockCore_Topdown",
        "BeamSource_Topdown",
        "BeamShadow_Topdown"
    };

    private static readonly string[] ForbiddenObjectFragments =
    {
        "TutorialSign",
        "CrossingHint",
        "PasteArea",
        "LureArea",
        "Enemy Passage",
        "Disabled_Deprecated_Blockout",
        "Interact Marker"
    };

    private static StringBuilder playReport;
    private static int playStage;
    private static double nextPlayStageTime;
    private static Vector3 originalPlayerPosition;
    private static Vector3 movedPlayerPosition;
    private static Vector3 originalDefaultShadowPosition;
    private static Vector3 originalPastedShadowPosition;

    [MenuItem("Shadough/Import Formal Assets And Bind Player Shadow")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new InvalidOperationException("Stop Play Mode before importing formal assets.");
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            throw new InvalidOperationException("Open ClockTower_TopdownPrototype before running this command.");
        }

        StringBuilder report = new StringBuilder();
        Dictionary<string, string> beforeTransforms = CaptureProtectedTransforms(scene);
        Dictionary<string, int> beforeForbidden = CountForbiddenObjects(scene);
        int beforeObjectCount = CountSceneObjects(scene);

        report.AppendLine("Formal Asset Import And Player Shadow Binding");
        report.AppendLine("Scene: " + scene.path);
        report.AppendLine("Scene object count before: " + beforeObjectCount);

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        for (int i = 0; i < ImportPaths.Length; i++)
        {
            ConfigureSpriteImporter(ImportPaths[i]);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Sprite playerBase = AssetDatabase.LoadAssetAtPath<Sprite>(PlayerBasePath);
        Sprite playerPasted = AssetDatabase.LoadAssetAtPath<Sprite>(PlayerPastedPath);
        if (playerBase == null || playerPasted == null)
        {
            throw new InvalidOperationException("Player shadow sprites were not imported as Single sprites.");
        }

        GameObject player = FindSceneObject(scene, "Player_Topdown");
        GameObject playerShadow = FindSceneObject(scene, "Player_Shadow");
        if (player == null || playerShadow == null)
        {
            throw new InvalidOperationException("Player_Topdown or Player_Shadow is missing.");
        }

        SpriteRenderer playerShadowRenderer = playerShadow.GetComponent<SpriteRenderer>();
        PlayerSelfShadowCutter cutter = player.GetComponent<PlayerSelfShadowCutter>();
        if (playerShadowRenderer == null || cutter == null)
        {
            throw new InvalidOperationException("Player shadow SpriteRenderer or PlayerSelfShadowCutter is missing.");
        }

        if (playerShadow.transform.parent != player.transform)
        {
            Undo.SetTransformParent(playerShadow.transform, player.transform, "Parent Player Shadow To Player");
        }

        Undo.RecordObject(playerShadowRenderer, "Bind Formal Player Shadow Base Sprite");
        playerShadowRenderer.sprite = playerBase;
        EditorUtility.SetDirty(playerShadowRenderer);

        Undo.RecordObject(cutter, "Bind Formal Player Shadow Pasted Sprite");
        SerializedObject cutterObject = new SerializedObject(cutter);
        SerializedProperty pastedSpriteProperty = cutterObject.FindProperty("playerShadowSprite");
        if (pastedSpriteProperty == null)
        {
            throw new InvalidOperationException("PlayerSelfShadowCutter.playerShadowSprite was not found.");
        }

        pastedSpriteProperty.objectReferenceValue = playerPasted;
        cutterObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(cutter);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            throw new IOException("Could not save ClockTower_TopdownPrototype.");
        }

        Dictionary<string, string> afterTransforms = CaptureProtectedTransforms(scene);
        Dictionary<string, int> afterForbidden = CountForbiddenObjects(scene);
        int afterObjectCount = CountSceneObjects(scene);

        report.AppendLine("Scene object count after: " + afterObjectCount);
        report.AppendLine("No scene objects created: " + PassFail(beforeObjectCount == afterObjectCount));
        report.AppendLine("Player_Shadow parent: " + GetTransformPath(playerShadow.transform.parent));
        report.AppendLine("Player_Shadow localPosition: " + Format(playerShadow.transform.localPosition));
        report.AppendLine("Player_Shadow localRotation: " + Format(playerShadow.transform.localRotation));
        report.AppendLine("Player_Shadow localScale: " + Format(playerShadow.transform.localScale));
        report.AppendLine("Player base sprite: " + AssetDatabase.GetAssetPath(playerShadowRenderer.sprite));
        report.AppendLine("Player pasted sprite: " + AssetDatabase.GetAssetPath(pastedSpriteProperty.objectReferenceValue));

        ShadowItemData playerShadowData = InvokePlayerShadowData(cutter);
        report.AppendLine("PlayerShadow CanStandOn=false: " + PassFail(playerShadowData != null && !playerShadowData.canStandOn));
        report.AppendLine("PlayerShadow CanPress=false: " + PassFail(playerShadowData != null && !playerShadowData.canPress));
        report.AppendLine("PlayerShadow CanUnlock=false: " + PassFail(playerShadowData != null && !playerShadowData.canUnlock));
        report.AppendLine("PlayerShadow CanAttractEnemy=true: " + PassFail(playerShadowData != null && playerShadowData.canAttractEnemy));
        report.AppendLine("PlayerShadow CanBlock=false: " + PassFail(playerShadowData != null && !playerShadowData.canBlock));

        bool transformsUnchanged = AppendTransformComparison(report, beforeTransforms, afterTransforms);
        bool forbiddenUnchanged = AppendForbiddenComparison(report, beforeForbidden, afterForbidden);
        report.AppendLine("Protected transforms unchanged: " + PassFail(transformsUnchanged));
        report.AppendLine("Deleted/helper object counts unchanged: " + PassFail(forbiddenUnchanged));

        int missingScripts;
        int missingReferences;
        CountMissingSceneData(scene, out missingScripts, out missingReferences);
        report.AppendLine("Missing Script: " + missingScripts);
        report.AppendLine("Missing Reference: " + missingReferences);

        for (int i = 0; i < ImportPaths.Length; i++)
        {
            TextureImporter importer = AssetImporter.GetAtPath(ImportPaths[i]) as TextureImporter;
            report.AppendLine(ImportPaths[i] + ": " + ImporterSummary(importer));
        }

        string fullReportPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ReportPath));
        Directory.CreateDirectory(Path.GetDirectoryName(fullReportPath));
        File.WriteAllText(fullReportPath, report.ToString());
        Debug.Log("Formal asset import and player shadow binding complete. Report: " + fullReportPath);
    }

    [MenuItem("Shadough/Place Formal Key Beam And Final Core")]
    public static void PlaceFormalKeyBeamAndFinalCore()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new InvalidOperationException("Stop Play Mode before placing formal scene objects.");
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            throw new InvalidOperationException("Open ClockTower_TopdownPrototype before running this command.");
        }

        Dictionary<string, string> beforeTransforms = CaptureProtectedTransforms(scene);
        beforeTransforms.Remove("BeamSource_Topdown");
        beforeTransforms.Remove("BeamShadow_Topdown");
        beforeTransforms.Remove("KeySource_Topdown");
        beforeTransforms.Remove("FinalClockCore_Topdown");
        int beforeObjectCount = CountSceneObjects(scene);

        GameObject interactables = FindSceneObject(scene, "Interactables");
        GameObject beamShadow = FindSceneObject(scene, "BeamShadow_Topdown");
        GameObject keyShadow = FindSceneObject(scene, "KeyShadow_Topdown");
        if (interactables == null || beamShadow == null || keyShadow == null)
        {
            throw new InvalidOperationException("Interactables, BeamShadow_Topdown, or KeyShadow_Topdown is missing.");
        }

        Sprite beamSourceSprite = LoadSprite(BeamSourcePath);
        Sprite beamBaseSprite = LoadSprite(BeamBasePath);
        Sprite beamPastedSprite = LoadSprite(BeamPastedPath);
        Sprite keyNormalSprite = LoadSprite(KeyNormalPath);
        Sprite keyGlowSprite = LoadSprite(KeyGlowPath);
        Sprite keyBaseSprite = LoadSprite(KeyBasePath);
        Sprite keyPastedSprite = LoadSprite(KeyPastedPath);
        Sprite coreIdleSprite = LoadSprite(CoreIdlePath);
        Sprite coreActiveSprite = LoadSprite(CoreActivePath);
        Sprite corePlatformSprite = LoadSprite(CorePlatformPath);

        GameObject beamSource = FindSceneObject(scene, "BeamSource_Topdown");
        if (beamSource == null)
        {
            throw new InvalidOperationException("BeamSource_Topdown is missing; placement is intentionally not generated.");
        }

        ConfigureSpriteRenderer(beamSource, beamSourceSprite, 6, true);

        SpriteRenderer beamShadowRenderer = RequireSpriteRenderer(beamShadow);
        Undo.RecordObject(beamShadowRenderer, "Bind Formal Beam Shadow");
        beamShadowRenderer.sprite = beamBaseSprite;
        beamShadowRenderer.color = Color.white;
        beamShadowRenderer.drawMode = SpriteDrawMode.Sliced;
        beamShadowRenderer.size = new Vector2(1.1f, 0.48f);
        EditorUtility.SetDirty(beamShadowRenderer);
        GameObject keySource = FindSceneObject(scene, "KeySource_Topdown");
        if (keySource == null)
        {
            throw new InvalidOperationException("KeySource_Topdown is missing; placement is intentionally not generated.");
        }

        ConfigureSpriteRenderer(keySource, keyNormalSprite, 7, true);
        GameObject keyGlow = FindOrCreateChild(keySource.transform, "KeySource_Glow");
        SetLocalTransform(keyGlow.transform, Vector3.zero, Quaternion.identity, Vector3.one);
        ConfigureSpriteRenderer(keyGlow, keyGlowSprite, 8, false);

        SpriteRenderer keyShadowRenderer = RequireSpriteRenderer(keyShadow);
        Undo.RecordObject(keyShadowRenderer, "Bind Formal Key Shadow");
        keyShadowRenderer.sprite = keyBaseSprite;
        keyShadowRenderer.color = Color.white;
        keyShadowRenderer.drawMode = SpriteDrawMode.Simple;
        EditorUtility.SetDirty(keyShadowRenderer);

        GameObject finalCore = FindSceneObject(scene, "FinalClockCore_Topdown");
        if (finalCore == null)
        {
            throw new InvalidOperationException("FinalClockCore_Topdown is missing; placement is intentionally not generated.");
        }

        SpriteRenderer coreRenderer = ConfigureSpriteRenderer(finalCore, coreIdleSprite, 10, true);
        BoxCollider2D coreTrigger = finalCore.GetComponent<BoxCollider2D>();
        if (coreTrigger == null)
        {
            coreTrigger = Undo.AddComponent<BoxCollider2D>(finalCore);
        }

        Undo.RecordObject(coreTrigger, "Configure Final Clock Core Trigger");
        coreTrigger.isTrigger = true;
        coreTrigger.size = new Vector2(1.45f, 1.45f);
        EditorUtility.SetDirty(coreTrigger);

        TopdownFinalClockCore coreController = finalCore.GetComponent<TopdownFinalClockCore>();
        if (coreController == null)
        {
            coreController = Undo.AddComponent<TopdownFinalClockCore>(finalCore);
        }

        SerializedObject coreObject = new SerializedObject(coreController);
        coreObject.FindProperty("coreRenderer").objectReferenceValue = coreRenderer;
        coreObject.FindProperty("idleSprite").objectReferenceValue = coreIdleSprite;
        coreObject.FindProperty("activeSprite").objectReferenceValue = coreActiveSprite;
        coreObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(coreController);

        GameObject corePlatform = FindOrCreateChild(finalCore.transform, "FinalClockCore_Platform");
        SetLocalTransform(corePlatform.transform, Vector3.zero, Quaternion.identity, Vector3.one);
        ConfigureSpriteRenderer(corePlatform, corePlatformSprite, 9, true);

        PastedShadowVisualResolver resolver = UnityEngine.Object.FindObjectOfType<PastedShadowVisualResolver>(true);
        if (resolver == null)
        {
            GameObject gameManager = FindSceneObject(scene, "GameManager");
            if (gameManager == null)
            {
                throw new InvalidOperationException("PastedShadowVisualResolver and GameManager are missing.");
            }

            resolver = Undo.AddComponent<PastedShadowVisualResolver>(gameManager);
        }

        SerializedObject resolverObject = new SerializedObject(resolver);
        resolverObject.FindProperty("beamPastedSprite").objectReferenceValue = beamPastedSprite;
        resolverObject.FindProperty("keyPastedSprite").objectReferenceValue = keyPastedSprite;
        resolverObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(resolver);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene))
        {
            throw new IOException("Could not save ClockTower_TopdownPrototype.");
        }

        Dictionary<string, string> afterTransforms = CaptureProtectedTransforms(scene);
        bool protectedUnchanged = AppendTransformComparison(new StringBuilder(), beforeTransforms, afterTransforms);
        int missingScripts;
        int missingReferences;
        CountMissingSceneData(scene, out missingScripts, out missingReferences);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Formal Key Beam And Final Core Placement");
        report.AppendLine("Scene: " + scene.path);
        report.AppendLine("Scene objects before: " + beforeObjectCount);
        report.AppendLine("Scene objects after: " + CountSceneObjects(scene));
        report.AppendLine("Existing protected transforms unchanged: " + PassFail(protectedUnchanged));
        report.AppendLine("Existing authored transforms preserved: " + PassFail(protectedUnchanged));
        report.AppendLine("BeamSource_Topdown: " + Snapshot(beamSource.transform));
        report.AppendLine("BeamShadow_Topdown: " + Snapshot(beamShadow.transform));
        report.AppendLine("KeySource_Topdown: " + Snapshot(keySource.transform));
        report.AppendLine("KeyShadow_Topdown: " + Snapshot(keyShadow.transform));
        report.AppendLine("FinalClockCore_Topdown: " + Snapshot(finalCore.transform));
        report.AppendLine("Beam base sprite: " + AssetDatabase.GetAssetPath(beamShadowRenderer.sprite));
        report.AppendLine("Beam pasted sprite: " + AssetDatabase.GetAssetPath(beamPastedSprite));
        report.AppendLine("Key base sprite: " + AssetDatabase.GetAssetPath(keyShadowRenderer.sprite));
        report.AppendLine("Key pasted sprite: " + AssetDatabase.GetAssetPath(keyPastedSprite));
        report.AppendLine("Final core idle sprite: " + AssetDatabase.GetAssetPath(coreIdleSprite));
        report.AppendLine("Final core active sprite: " + AssetDatabase.GetAssetPath(coreActiveSprite));
        report.AppendLine("Final core platform sprite: " + AssetDatabase.GetAssetPath(corePlatformSprite));
        report.AppendLine("Missing Script: " + missingScripts);
        report.AppendLine("Missing Reference: " + missingReferences);

        string reportPath = FullPath("Logs/FormalKeyBeamCorePlacement.report.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
        File.WriteAllText(reportPath, report.ToString());
        Selection.activeGameObject = finalCore;
        SceneView.lastActiveSceneView.FrameSelected();
        Debug.Log("Formal key, beam, and final core placement complete. Report: " + reportPath);
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            throw new FileNotFoundException("Sprite not found", path);
        }

        return sprite;
    }

    private static GameObject FindOrCreateSceneObject(Scene scene, string objectName, Transform parent)
    {
        GameObject obj = FindSceneObject(scene, objectName);
        if (obj == null)
        {
            obj = new GameObject(objectName);
            Undo.RegisterCreatedObjectUndo(obj, "Create " + objectName);
        }

        if (obj.transform.parent != parent)
        {
            Undo.SetTransformParent(obj.transform, parent, "Parent " + objectName);
        }

        return obj;
    }

    private static GameObject FindOrCreateChild(Transform parent, string objectName)
    {
        Transform child = parent.Find(objectName);
        if (child != null)
        {
            return child.gameObject;
        }

        GameObject obj = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(obj, "Create " + objectName);
        Undo.SetTransformParent(obj.transform, parent, "Parent " + objectName);
        return obj;
    }

    private static void SetLocalTransform(Transform transform, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Undo.RecordObject(transform, "Place " + transform.name);
        transform.localPosition = position;
        transform.localRotation = rotation;
        transform.localScale = scale;
        EditorUtility.SetDirty(transform);
    }

    private static SpriteRenderer ConfigureSpriteRenderer(GameObject obj, Sprite sprite, int sortingOrder, bool enabled)
    {
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<SpriteRenderer>(obj);
        }

        Undo.RecordObject(renderer, "Configure " + obj.name + " Renderer");
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.sortingOrder = sortingOrder;
        renderer.enabled = enabled;
        EditorUtility.SetDirty(renderer);
        return renderer;
    }

    private static SpriteRenderer RequireSpriteRenderer(GameObject obj)
    {
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            throw new InvalidOperationException(obj.name + " has no SpriteRenderer.");
        }

        return renderer;
    }

    [MenuItem("Shadough/Run Formal Player Shadow Play Probe")]
    public static void RequestPlayProbe()
    {
        File.WriteAllText(FullPath(PlayProbeRequestPath), "formal-player-shadow-play-probe");
        EditorApplication.update -= TryStartPlayProbe;
        EditorApplication.update += TryStartPlayProbe;
    }

    [InitializeOnLoadMethod]
    private static void WatchForPlayProbeRequest()
    {
        EditorApplication.update -= TryStartPlayProbe;
        EditorApplication.update += TryStartPlayProbe;
    }

    private static void TryStartPlayProbe()
    {
        if (!File.Exists(FullPath(PlayProbeRequestPath)))
        {
            return;
        }

        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            EditorApplication.isPlaying = true;
            return;
        }

        if (EditorApplication.isPlaying && playReport == null)
        {
            playReport = new StringBuilder();
            playReport.AppendLine("Formal Player Shadow Play Probe");
            playReport.AppendLine("Scene: " + ScenePath);
            playStage = 0;
            nextPlayStageTime = EditorApplication.timeSinceStartup + 0.75d;
            EditorApplication.update -= RunPlayProbeStep;
            EditorApplication.update += RunPlayProbeStep;
        }
    }

    private static void RunPlayProbeStep()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update -= RunPlayProbeStep;
            playReport = null;
            return;
        }

        if (EditorApplication.timeSinceStartup < nextPlayStageTime)
        {
            return;
        }

        GameObject player = GameObject.Find("Player_Topdown");
        GameObject defaultShadow = GameObject.Find("Player_Shadow");
        PlayerSelfShadowCutter cutter = player != null ? player.GetComponent<PlayerSelfShadowCutter>() : null;
        ShadowInventory inventory = player != null ? player.GetComponent<ShadowInventory>() : null;
        FreeShadowPlacer placer = player != null ? player.GetComponent<FreeShadowPlacer>() : null;

        if (player == null || defaultShadow == null || cutter == null || inventory == null || placer == null)
        {
            playReport.AppendLine("Required player shadow components: FAIL");
            FinishPlayProbe(player);
            return;
        }

        switch (playStage)
        {
            case 0:
                originalPlayerPosition = player.transform.position;
                originalDefaultShadowPosition = defaultShadow.transform.position;
                SpriteRenderer defaultRenderer = defaultShadow.GetComponent<SpriteRenderer>();
                playReport.AppendLine("Player_Shadow parent is Player_Topdown: " + PassFail(defaultShadow.transform.parent == player.transform));
                playReport.AppendLine("Default sprite is formal base: " + PassFail(defaultRenderer != null && AssetDatabase.GetAssetPath(defaultRenderer.sprite) == PlayerBasePath));
                player.transform.position = originalPlayerPosition + new Vector3(0.35f, 0.2f, 0f);
                movedPlayerPosition = player.transform.position;
                Physics2D.SyncTransforms();
                AdvancePlayProbe(1, 0.2d);
                break;
            case 1:
                Vector3 playerDelta = movedPlayerPosition - originalPlayerPosition;
                Vector3 shadowDelta = defaultShadow.transform.position - originalDefaultShadowPosition;
                playReport.AppendLine("PlayerShadow follows player movement: " + PassFail(Vector3.Distance(playerDelta, shadowDelta) < 0.0001f));
                player.transform.position = originalPlayerPosition;
                Physics2D.SyncTransforms();

                ShadowItemData data = InvokePlayerShadowData(cutter);
                playReport.AppendLine("Cut data uses formal pasted sprite: " + PassFail(data != null && AssetDatabase.GetAssetPath(data.sprite) == PlayerPastedPath));
                playReport.AppendLine("Cut data CanStandOn=false: " + PassFail(data != null && !data.canStandOn));
                playReport.AppendLine("Cut data CanPress=false: " + PassFail(data != null && !data.canPress));
                playReport.AppendLine("Cut data CanUnlock=false: " + PassFail(data != null && !data.canUnlock));
                playReport.AppendLine("Cut data CanAttractEnemy=true: " + PassFail(data != null && data.canAttractEnemy));
                playReport.AppendLine("Cut data CanBlock=false: " + PassFail(data != null && !data.canBlock));

                inventory.ClearShadow();
                bool pickedUp = inventory.PickUpShadow(data);
                playReport.AppendLine("PlayerShadow cutting data enters inventory: " + PassFail(pickedUp && inventory.HasShadow()));
                SetPrivateField(placer, "currentPreviewPosition", originalPlayerPosition + new Vector3(0.75f, 0f, 0f));
                SetPrivateField(placer, "currentPreviewRotation", Quaternion.identity);
                InvokePrivateMethod(placer, "TryPlaceShadow");
                AdvancePlayProbe(2, 0.2d);
                break;
            case 2:
                PastedShadowObject pasted = FindRuntimePlayerShadow();
                bool pastedExists = pasted != null;
                playReport.AppendLine("PlayerShadow pasted object created: " + PassFail(pastedExists));
                playReport.AppendLine("Pasted sprite is formal lure: " + PassFail(pastedExists && AssetDatabase.GetAssetPath(pasted.SpriteRenderer.sprite) == PlayerPastedPath));
                playReport.AppendLine("Pasted shadow independent from player: " + PassFail(pastedExists && !pasted.transform.IsChildOf(player.transform)));
                playReport.AppendLine("Pasted CanAttractEnemy=true: " + PassFail(pastedExists && pasted.CanAttractEnemy));
                playReport.AppendLine("Pasted CanStandOn=false: " + PassFail(pastedExists && !pasted.CanStandOn));
                playReport.AppendLine("Pasted CanPress=false: " + PassFail(pastedExists && !pasted.CanPress));
                playReport.AppendLine("Pasted CanUnlock=false: " + PassFail(pastedExists && !pasted.CanUnlock));
                playReport.AppendLine("Pasted CanBlock=false: " + PassFail(pastedExists && !pasted.CanBlock));
                if (!pastedExists)
                {
                    FinishPlayProbe(player);
                    return;
                }

                originalPastedShadowPosition = pasted.transform.position;
                player.transform.position = originalPlayerPosition + new Vector3(-0.3f, 0.25f, 0f);
                Physics2D.SyncTransforms();
                AdvancePlayProbe(3, 0.2d);
                break;
            case 3:
                PastedShadowObject placedShadow = FindRuntimePlayerShadow();
                playReport.AppendLine("Pasted PlayerShadow stays in place when player moves: " + PassFail(
                    placedShadow != null && Vector3.Distance(placedShadow.transform.position, originalPastedShadowPosition) < 0.0001f));
                FinishPlayProbe(player);
                break;
        }
    }

    private static void AdvancePlayProbe(int nextStage, double delay)
    {
        playStage = nextStage;
        nextPlayStageTime = EditorApplication.timeSinceStartup + delay;
    }

    private static void FinishPlayProbe(GameObject player)
    {
        if (player != null)
        {
            player.transform.position = originalPlayerPosition;
            Physics2D.SyncTransforms();
        }

        EditorApplication.update -= RunPlayProbeStep;
        string requestPath = FullPath(PlayProbeRequestPath);
        if (File.Exists(requestPath))
        {
            File.Delete(requestPath);
        }

        string reportPath = FullPath(PlayProbeReportPath);
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
        File.WriteAllText(reportPath, playReport.ToString());
        Debug.Log("Formal player shadow play probe complete. Report: " + reportPath);
        playReport = null;
        EditorApplication.isPlaying = false;
    }

    private static PastedShadowObject FindRuntimePlayerShadow()
    {
        PastedShadowObject[] pastedShadows = UnityEngine.Object.FindObjectsOfType<PastedShadowObject>();
        for (int i = 0; i < pastedShadows.Length; i++)
        {
            if (pastedShadows[i] != null && pastedShadows[i].ShadowType == ShadowType.Player)
            {
                return pastedShadows[i];
            }
        }

        return null;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
        {
            throw new MissingFieldException(target.GetType().Name, fieldName);
        }

        field.SetValue(target, value);
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method == null)
        {
            throw new MissingMethodException(target.GetType().Name, methodName);
        }

        method.Invoke(target, null);
    }

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }

    private static void ConfigureSpriteImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("TextureImporter not found", path);
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = 160f;
        importer.wrapMode = TextureWrapMode.Clamp;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static ShadowItemData InvokePlayerShadowData(PlayerSelfShadowCutter cutter)
    {
        MethodInfo method = typeof(PlayerSelfShadowCutter).GetMethod("CreateSelfShadowData", BindingFlags.Instance | BindingFlags.NonPublic);
        return method != null ? method.Invoke(cutter, null) as ShadowItemData : null;
    }

    private static Dictionary<string, string> CaptureProtectedTransforms(Scene scene)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        for (int i = 0; i < ProtectedObjectNames.Length; i++)
        {
            GameObject obj = FindSceneObject(scene, ProtectedObjectNames[i]);
            result[ProtectedObjectNames[i]] = obj != null ? Snapshot(obj.transform) : "MISSING";
        }

        return result;
    }

    private static string Snapshot(Transform transform)
    {
        return "parent=" + GetTransformPath(transform.parent)
            + "|localPosition=" + Format(transform.localPosition)
            + "|localRotation=" + Format(transform.localRotation)
            + "|localScale=" + Format(transform.localScale);
    }

    private static bool AppendTransformComparison(StringBuilder report, Dictionary<string, string> before, Dictionary<string, string> after)
    {
        bool allUnchanged = true;
        foreach (KeyValuePair<string, string> pair in before)
        {
            string afterValue;
            after.TryGetValue(pair.Key, out afterValue);
            bool unchanged = pair.Value == afterValue;
            allUnchanged &= unchanged;
            report.AppendLine("Transform " + pair.Key + ": " + PassFail(unchanged));
        }

        return allUnchanged;
    }

    private static Dictionary<string, int> CountForbiddenObjects(Scene scene)
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        for (int i = 0; i < ForbiddenObjectFragments.Length; i++)
        {
            counts[ForbiddenObjectFragments[i]] = 0;
        }

        foreach (GameObject obj in GetSceneObjects(scene))
        {
            for (int i = 0; i < ForbiddenObjectFragments.Length; i++)
            {
                if (obj.name.IndexOf(ForbiddenObjectFragments[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    counts[ForbiddenObjectFragments[i]]++;
                }
            }
        }

        return counts;
    }

    private static bool AppendForbiddenComparison(StringBuilder report, Dictionary<string, int> before, Dictionary<string, int> after)
    {
        bool allUnchanged = true;
        foreach (KeyValuePair<string, int> pair in before)
        {
            int afterCount;
            after.TryGetValue(pair.Key, out afterCount);
            bool unchanged = pair.Value == afterCount;
            allUnchanged &= unchanged;
            report.AppendLine("Helper fragment " + pair.Key + ": before=" + pair.Value + ", after=" + afterCount + ", unchanged=" + PassFail(unchanged));
        }

        return allUnchanged;
    }

    private static void CountMissingSceneData(Scene scene, out int missingScripts, out int missingReferences)
    {
        missingScripts = 0;
        missingReferences = 0;
        foreach (GameObject obj in GetSceneObjects(scene))
        {
            missingScripts += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(obj);
            Component[] components = obj.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty property = serializedObject.GetIterator();
                while (property.NextVisible(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.objectReferenceInstanceIDValue != 0
                        && property.objectReferenceValue == null)
                    {
                        missingReferences++;
                    }
                }
            }
        }
    }

    private static GameObject FindSceneObject(Scene scene, string objectName)
    {
        foreach (GameObject obj in GetSceneObjects(scene))
        {
            if (obj.name == objectName)
            {
                return obj;
            }
        }

        return null;
    }

    private static IEnumerable<GameObject> GetSceneObjects(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                yield return transforms[j].gameObject;
            }
        }
    }

    private static int CountSceneObjects(Scene scene)
    {
        int count = 0;
        foreach (GameObject ignored in GetSceneObjects(scene))
        {
            count++;
        }

        return count;
    }

    private static string ImporterSummary(TextureImporter importer)
    {
        if (importer == null)
        {
            return "MISSING";
        }

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        return "type=" + importer.textureType
            + ", mode=" + importer.spriteImportMode
            + ", alpha=" + importer.alphaIsTransparency
            + ", filter=" + importer.filterMode
            + ", compression=" + importer.textureCompression
            + ", mesh=" + settings.spriteMeshType
            + ", ppu=" + importer.spritePixelsPerUnit
            + ", pivot=Center";
    }

    private static string GetTransformPath(Transform transform)
    {
        if (transform == null)
        {
            return "<root>";
        }

        return transform.name;
    }

    private static string Format(Vector3 value)
    {
        return string.Format("({0:R},{1:R},{2:R})", value.x, value.y, value.z);
    }

    private static string Format(Quaternion value)
    {
        return string.Format("({0:R},{1:R},{2:R},{3:R})", value.x, value.y, value.z, value.w);
    }

    private static string PassFail(bool passed)
    {
        return passed ? "PASS" : "FAIL";
    }
}
