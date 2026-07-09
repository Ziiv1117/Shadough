using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownDemoLoopAuditAndFix
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string FixRequestPath = "Temp/TopdownDemoLoopAuditAndFix.request";
    private const string PlayProbeRequestPath = "Temp/TopdownDemoLoopPlayProbe.request";
    private const string EditReportPath = "Temp/TopdownDemoLoopAuditAndFix.report.txt";
    private const string PlayReportPath = "Temp/TopdownDemoLoopPlayProbe.report.txt";

    private static readonly Vector3 PlayerStartPosition = new Vector3(-14.35f, -8.80f, 0f);
    private static readonly Vector3 CameraStartOffset = new Vector3(1.15f, 0.75f, -10f);
    private const float CameraOrthographicSize = 6.55f;

    private static readonly Vector3 TreePosition = new Vector3(-14.25f, -6.75f, 0f);
    private static readonly Vector3 TreeTrunkPosition = new Vector3(-14.25f, -6.85f, 0f);
    private static readonly Vector3 TreeCanopyPosition = new Vector3(-14.25f, -6.45f, 0f);
    private static readonly Vector3 TreeShadowReadPosition = new Vector3(-12.05f, -7.38f, 0f);
    private static readonly Vector3 CrossingPosition = new Vector3(-11.85f, -7.35f, 0f);

    private static readonly Vector3 PressurePlatePosition = new Vector3(-4.40f, -2.75f, 0f);
    private static readonly Vector3 PressureDoorPosition = new Vector3(-1.475f, -0.125f, 0f);
    private static readonly Vector3 BeamSourcePosition = new Vector3(-6.725f, -1.925f, 0f);
    private static readonly Vector3 BeamShadowPosition = new Vector3(-5.40f, -2.425f, 0f);

    private static readonly Vector3 LockDoorPosition = new Vector3(2.525f, 3.825f, 0f);
    private static readonly Vector3 LockPosition = new Vector3(-0.70f, 2.675f, 0f);
    private static readonly Vector3 KeySourcePosition = new Vector3(-2.975f, 2.20f, 0f);
    private static readonly Vector3 KeyShadowPosition = new Vector3(-2.10f, 1.775f, 0f);

    private static readonly Vector3 SeekerHomePosition = new Vector3(6.60f, 5.125f, 0f);
    private static readonly Vector3 LureAreaPosition = new Vector3(10.025f, 5.625f, 0f);
    private static readonly Vector3 SeekerSafePointPosition = new Vector3(2.15f, 3.25f, 0f);
    private static readonly Vector3 FinalCorePosition = new Vector3(13.60f, 9.625f, 0f);

    private static StringBuilder playReport;
    private static int probeStage;
    private static double nextProbeTime;
    private static PastedShadowObject bridgeShadow;
    private static PastedShadowObject pressShadow;
    private static PastedShadowObject unlockShadow;
    private static PastedShadowObject wrongLureShadow;
    private static PastedShadowObject playerLureShadow;

    [InitializeOnLoadMethod]
    private static void RegisterAutoRun()
    {
        EditorApplication.update -= TryAutoRun;
        EditorApplication.update += TryAutoRun;
    }

    [MenuItem("Shadough/Audit And Fix Topdown Demo Loop")]
    public static void AuditAndFixFromMenu()
    {
        AuditAndFix();
    }

    [MenuItem("Shadough/Run Topdown Demo Loop Play Probe")]
    public static void RunPlayProbeFromMenu()
    {
        RequestPlayProbe();
    }

    private static void TryAutoRun()
    {
        if (EditorApplication.isCompiling)
        {
            return;
        }

        string fixRequest = FullPath(FixRequestPath);
        if (File.Exists(fixRequest))
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            File.Delete(fixRequest);
            AuditAndFix();
            return;
        }

        string playRequest = FullPath(PlayProbeRequestPath);
        if (File.Exists(playRequest) && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
            return;
        }

        if (File.Exists(playRequest) && EditorApplication.isPlaying && playReport == null)
        {
            StartPlayProbe();
        }
    }

    public static void AuditAndFix()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Demo Loop Audit And Fix");
        report.AppendLine("Scene: " + ScenePath);

        FixStartArea(report);
        FixRiverCrossing(report);
        FixPressureRoom(report);
        FixLockRoom(report);
        FixSeekerArea(report);
        FixFinalCore(report);
        FixTutorialSignColliders(report);
        DisableOldBlockers(report);
        ConfigureCamera(report);
        ValidateScene(report);
        ValidateRouteClearance(report);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(EditReportPath), report.ToString());
        Debug.Log("Topdown demo loop audit/fix complete. Report: " + FullPath(EditReportPath));
    }

    public static void RequestPlayProbe()
    {
        File.WriteAllText(FullPath(PlayProbeRequestPath), "play-probe");
    }

    private static void StartPlayProbe()
    {
        playReport = new StringBuilder();
        playReport.AppendLine("Topdown Demo Loop Play Probe");
        playReport.AppendLine("Scene: " + ScenePath);
        probeStage = 0;
        nextProbeTime = EditorApplication.timeSinceStartup + 0.75d;

        EditorApplication.update -= RunPlayProbeStep;
        EditorApplication.update += RunPlayProbeStep;
    }

    private static void RunPlayProbeStep()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update -= RunPlayProbeStep;
            playReport = null;
            return;
        }

        if (EditorApplication.timeSinceStartup < nextProbeTime)
        {
            return;
        }

        switch (probeStage)
        {
            case 0:
                ClearRuntimeProbeShadows();
                AppendRequiredObjectChecks(playReport);
                bridgeShadow = CreatePastedShadowFromSource("TreeShadow_Topdown", CrossingPosition);
                playReport.AppendLine("TreeShadow pasted at bridge: " + PassFail(bridgeShadow != null));
                AdvanceProbe(1, 0.35d);
                break;
            case 1:
                TopdownBridgeCrossingZone crossing = FindComponent<TopdownBridgeCrossingZone>("CrossingHint_01");
                bool bridgeOpen = crossing != null && crossing.IsOpen;
                GameObject blocker = FindSceneObject("River_CrossingBlocker_FromMap");
                bool blockerOpen = blocker == null || !blocker.activeSelf;
                playReport.AppendLine("TreeShadow crossing open: " + PassFail(bridgeOpen && blockerOpen));
                pressShadow = CreatePastedShadowFromSource("BeamShadow_Topdown", PressurePlatePosition);
                playReport.AppendLine("CanPress shadow pasted at plate: " + PassFail(pressShadow != null && pressShadow.CanPress));
                AdvanceProbe(2, 0.35d);
                break;
            case 2:
                PressurePlateController plate = FindComponent<PressurePlateController>("PressurePlate_Topdown");
                DoorController pressureDoor = FindComponent<DoorController>("Door_Pressure_Topdown");
                bool pressurePass = plate != null && plate.IsPressed && pressureDoor != null && pressureDoor.IsOpen;
                playReport.AppendLine("CanPress plate opens door: " + PassFail(pressurePass));
                unlockShadow = CreatePastedShadowFromSource("KeyShadow_Topdown", LockPosition);
                playReport.AppendLine("CanUnlock shadow pasted at lock: " + PassFail(unlockShadow != null && unlockShadow.CanUnlock));
                AdvanceProbe(3, 0.35d);
                break;
            case 3:
                LockController lockController = FindComponent<LockController>("Lock_Topdown");
                DoorController lockDoor = FindComponent<DoorController>("Door_Lock_Topdown");
                bool unlockPass = lockController != null && lockController.IsUnlocked && lockDoor != null && lockDoor.IsOpen;
                playReport.AppendLine("CanUnlock opens lock door: " + PassFail(unlockPass));
                wrongLureShadow = CreatePastedShadowFromSource("TreeShadow_Topdown", LureAreaPosition);
                playReport.AppendLine("Non-attract shadow placed in seeker radius: " + PassFail(wrongLureShadow != null && !wrongLureShadow.CanAttractEnemy));
                AdvanceProbe(4, 0.35d);
                break;
            case 4:
                EnemyShadowSeeker seekerBeforeLure = FindComponent<EnemyShadowSeeker>("ShadowSeeker_Topdown");
                bool wrongLureIgnored = seekerBeforeLure != null && seekerBeforeLure.CurrentTarget != wrongLureShadow;
                playReport.AppendLine("Non-attract shadow ignored by seeker: " + PassFail(wrongLureIgnored));
                if (wrongLureShadow != null)
                {
                    UnityEngine.Object.Destroy(wrongLureShadow.gameObject);
                }

                playerLureShadow = CreatePlayerLureShadow(LureAreaPosition);
                playReport.AppendLine("PlayerShadow lure created in detection radius: " + PassFail(playerLureShadow != null && playerLureShadow.CanAttractEnemy));
                AdvanceProbe(5, 0.50d);
                break;
            case 5:
                EnemyShadowSeeker seeker = FindComponent<EnemyShadowSeeker>("ShadowSeeker_Topdown");
                bool lurePass = seeker != null && seeker.CurrentTarget == playerLureShadow && seeker.IsChasingShadow;
                playReport.AppendLine("ShadowSeeker follows PlayerShadow: " + PassFail(lurePass));
                TopdownFinalClockCore finalCore = FindComponent<TopdownFinalClockCore>("FinalClockCore_Topdown");
                if (finalCore != null)
                {
                    finalCore.ActivateClockCore();
                }

                AdvanceProbe(6, 0.20d);
                break;
            case 6:
                TopdownFinalClockCore activatedCore = FindComponent<TopdownFinalClockCore>("FinalClockCore_Topdown");
                playReport.AppendLine("FinalClockCore activates without Reveal View: " + PassFail(activatedCore != null && activatedCore.IsActivated));
                FinishPlayProbe();
                break;
        }
    }

    private static void AdvanceProbe(int nextStage, double delay)
    {
        probeStage = nextStage;
        nextProbeTime = EditorApplication.timeSinceStartup + delay;
    }

    private static void FinishPlayProbe()
    {
        EditorApplication.update -= RunPlayProbeStep;
        string requestPath = FullPath(PlayProbeRequestPath);
        if (File.Exists(requestPath))
        {
            File.Delete(requestPath);
        }

        File.WriteAllText(FullPath(PlayReportPath), playReport.ToString());
        Debug.Log("Topdown demo loop play probe complete. Report: " + FullPath(PlayReportPath));
        playReport = null;
        EditorApplication.isPlaying = false;
    }

    private static void FixStartArea(StringBuilder report)
    {
        MoveObject("Player_Topdown", PlayerStartPosition, Vector3.one);
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

        ConfigureShadowInteractable("TreeShadow_Topdown", "Tree Shadow", true, false, false, false, false);
        report.AppendLine("Start area tuned.");
    }

    private static void FixRiverCrossing(StringBuilder report)
    {
        MoveObject("CrossingHint_01", CrossingPosition, Vector3.one);
        TopdownBridgeCrossingZone crossing = FindComponent<TopdownBridgeCrossingZone>("CrossingHint_01");
        GameObject blocker = FindSceneObject("River_CrossingBlocker_FromMap");
        if (crossing != null)
        {
            SerializedObject serializedCrossing = new SerializedObject(crossing);
            SetObject(serializedCrossing, "crossingBlocker", blocker);
            SetFloat(serializedCrossing, "detectionRadius", 2.0f);
            SetBool(serializedCrossing, "makeBridgeColliderTrigger", true);
            serializedCrossing.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(crossing);
        }

        BoxCollider2D blockerCollider = blocker != null ? blocker.GetComponent<BoxCollider2D>() : null;
        if (blockerCollider != null)
        {
            blockerCollider.offset = new Vector2(-11.95f, -7.30f);
            blockerCollider.size = new Vector2(4.60f, 1.65f);
            blockerCollider.isTrigger = false;
            EditorUtility.SetDirty(blockerCollider);
        }

        report.AppendLine("River crossing radius/blocker tuned.");
    }

    private static void FixPressureRoom(StringBuilder report)
    {
        MoveObject("Door_Pressure_Topdown", PressureDoorPosition, new Vector3(1.1f, 0.25f, 1f));
        MoveObject("PressurePlate_Topdown", PressurePlatePosition, new Vector3(1.05f, 0.72f, 1f));
        MoveObject("BeamSource_Topdown", BeamSourcePosition, new Vector3(0.75f, 0.75f, 1f));
        MoveObject("BeamShadow_Topdown", BeamShadowPosition, new Vector3(1.45f, 0.65f, 1f));

        PressurePlateController plate = FindComponent<PressurePlateController>("PressurePlate_Topdown");
        DoorController door = FindComponent<DoorController>("Door_Pressure_Topdown");
        ShadowPressureTrigger trigger = FindComponent<ShadowPressureTrigger>("PressurePlate_Topdown");
        if (plate != null)
        {
            SerializedObject serializedPlate = new SerializedObject(plate);
            SetObject(serializedPlate, "targetDoor", door);
            serializedPlate.ApplyModifiedPropertiesWithoutUndo();
            plate.SetPressed(false);
            EditorUtility.SetDirty(plate);
        }

        if (door != null)
        {
            door.SetOpen(false);
            EditorUtility.SetDirty(door);
        }

        if (trigger != null)
        {
            SerializedObject serializedTrigger = new SerializedObject(trigger);
            SetObject(serializedTrigger, "pressurePlate", plate);
            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(trigger);
        }

        ConfigureLocalBoxCollider("PressurePlate_Topdown", new Vector2(1.35f, 0.95f), Vector2.zero, true);
        ConfigureLocalBoxCollider("Door_Pressure_Topdown", new Vector2(1.35f, 0.36f), Vector2.zero, false);
        ConfigureShadowInteractable("BeamShadow_Topdown", "CanPress Shadow", false, true, false, false, false);
        report.AppendLine("Pressure room tuned.");
    }

    private static void FixLockRoom(StringBuilder report)
    {
        MoveObject("Door_Lock_Topdown", LockDoorPosition, new Vector3(0.28f, 1.25f, 1f));
        MoveObject("Lock_Topdown", LockPosition, new Vector3(0.85f, 0.85f, 1f));
        MoveObject("Lock_Topdown_Trigger", LockPosition, Vector3.one);
        MoveObject("KeySource_Topdown", KeySourcePosition, new Vector3(0.75f, 0.75f, 1f));
        MoveObject("KeyShadow_Topdown", KeyShadowPosition, new Vector3(0.9f, 0.55f, 1f));

        LockController lockController = FindComponent<LockController>("Lock_Topdown");
        DoorController door = FindComponent<DoorController>("Door_Lock_Topdown");
        ShadowLockTrigger trigger = FindComponent<ShadowLockTrigger>("Lock_Topdown_Trigger");
        if (lockController != null)
        {
            SerializedObject serializedLock = new SerializedObject(lockController);
            SetObject(serializedLock, "targetDoor", door);
            serializedLock.ApplyModifiedPropertiesWithoutUndo();
            lockController.SetUnlocked(false);
            EditorUtility.SetDirty(lockController);
        }

        if (door != null)
        {
            door.SetOpen(false);
            EditorUtility.SetDirty(door);
        }

        if (trigger != null)
        {
            SerializedObject serializedTrigger = new SerializedObject(trigger);
            SetObject(serializedTrigger, "lockController", lockController);
            SetBool(serializedTrigger, "requireAngleCheck", false);
            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(trigger);
        }

        ConfigureLocalBoxCollider("Lock_Topdown_Trigger", new Vector2(1.55f, 1.35f), Vector2.zero, true);
        ConfigureLocalBoxCollider("Door_Lock_Topdown", new Vector2(0.44f, 1.65f), Vector2.zero, false);
        ConfigureWorldBoxObject("Wall_Room02_East", new Vector2(2.40f, 1.70f), new Vector2(0.35f, 1.90f), false);
        ConfigureShadowInteractable("KeyShadow_Topdown", "CanUnlock Shadow", false, false, true, false, false);
        report.AppendLine("Lock room tuned.");
    }

    private static void FixSeekerArea(StringBuilder report)
    {
        MoveObject("ShadowSeeker_Topdown", SeekerHomePosition, new Vector3(1.8f, 1.8f, 1f));
        MoveObject("ShadowSeeker_Home_Topdown", SeekerHomePosition, Vector3.one);
        MoveObject("LureArea_Topdown", LureAreaPosition, new Vector3(1.65f, 0.9f, 1f));
        MoveObject("ShadowSeekerSafePoint_Topdown", SeekerSafePointPosition, Vector3.one);

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

        ConfigureLocalBoxCollider("LureArea_Topdown", new Vector2(2.35f, 1.35f), Vector2.zero, true);
        PlayerHealth health = FindComponent<PlayerHealth>("Player_Topdown");
        GameObject safePoint = FindSceneObject("ShadowSeekerSafePoint_Topdown");
        if (health != null)
        {
            SerializedObject serializedHealth = new SerializedObject(health);
            SetObject(serializedHealth, "safePoint", safePoint != null ? safePoint.transform : null);
            serializedHealth.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(health);
        }

        report.AppendLine("ShadowSeeker area tuned.");
    }

    private static void FixFinalCore(StringBuilder report)
    {
        MoveObject("FinalClockCore_Topdown", FinalCorePosition, new Vector3(1.35f, 1.35f, 1f));
        TopdownFinalClockCore finalCore = FindComponent<TopdownFinalClockCore>("FinalClockCore_Topdown");
        if (finalCore != null)
        {
            SerializedObject serializedCore = new SerializedObject(finalCore);
            SetString(serializedCore, "completeText", "Topdown Demo Complete");
            SetString(serializedCore, "logMessage", "Topdown demo complete");
            serializedCore.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(finalCore);
        }

        ConfigureLocalBoxCollider("FinalClockCore_Topdown", new Vector2(1.85f, 1.55f), Vector2.zero, true);
        ConfigureWorldBoxObject("Wall_Final_South", new Vector2(14.30f, 6.70f), new Vector2(3.80f, 0.35f), false);
        report.AppendLine("Final clock core tuned.");
    }

    private static void FixTutorialSignColliders(StringBuilder report)
    {
        int fixedCount = 0;
        TutorialSign[] signs = Resources.FindObjectsOfTypeAll<TutorialSign>();
        for (int i = 0; i < signs.Length; i++)
        {
            TutorialSign sign = signs[i];
            if (sign == null || !sign.gameObject.scene.IsValid() || sign.gameObject.scene.path != ScenePath)
            {
                continue;
            }

            BoxCollider2D collider = sign.GetComponent<BoxCollider2D>();
            if (collider != null && !collider.isTrigger)
            {
                collider.isTrigger = true;
                fixedCount++;
                EditorUtility.SetDirty(collider);
            }
        }

        report.AppendLine("Tutorial sign colliders set to trigger: " + fixedCount);
    }

    private static void DisableOldBlockers(StringBuilder report)
    {
        string[] oldNames =
        {
            "Ground", "Ground_Base", "River_Gap_01", "RiverBlocker_Right", "EnemyPassage_Topdown",
            "CrossingBlocker_01", "Area_01_TreeCrossing", "Area_02_PressureDoor", "Area_03_LockDoor",
            "Area_04_EnemyLure", "Goal_Platform", "River_Bank_Left", "River_Bank_Right"
        };

        int disabled = 0;
        for (int i = 0; i < oldNames.Length; i++)
        {
            GameObject oldObject = FindSceneObject(oldNames[i]);
            if (oldObject != null && oldObject.activeSelf)
            {
                oldObject.SetActive(false);
                disabled++;
            }
        }

        report.AppendLine("Old blocker objects disabled: " + disabled);
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
            report.AppendLine("Camera missing.");
            return;
        }

        camera.orthographic = true;
        camera.orthographicSize = CameraOrthographicSize;
        camera.transform.position = PlayerStartPosition + CameraStartOffset;
        EditorUtility.SetDirty(camera);

        TopdownCameraFollow follow = camera.GetComponent<TopdownCameraFollow>();
        GameObject player = FindSceneObject("Player_Topdown");
        if (follow != null)
        {
            SerializedObject serializedFollow = new SerializedObject(follow);
            SetObject(serializedFollow, "target", player != null ? player.transform : null);
            SetVector3(serializedFollow, "offset", CameraStartOffset);
            SetFloat(serializedFollow, "followSmoothTime", 0.12f);
            SetBool(serializedFollow, "snapOnStart", true);
            serializedFollow.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(follow);
        }

        report.AppendLine("Initial camera tuned.");
    }

    private static void ValidateScene(StringBuilder report)
    {
        int missingScripts = 0;
        int missingReferences = 0;
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

    private static void ValidateRouteClearance(StringBuilder report)
    {
        Physics2D.SyncTransforms();

        string blockedBy;
        bool routeClear = IsRouteClear(new Vector2[]
        {
            new Vector2(-14.35f, -8.80f),
            new Vector2(-12.90f, -7.90f),
            new Vector2(-10.60f, -6.20f),
            new Vector2(-7.00f, -4.20f),
            new Vector2(-4.40f, -2.75f),
            new Vector2(-1.55f, -0.10f),
            new Vector2(-0.70f, 2.675f),
            new Vector2(1.45f, 3.55f),
            new Vector2(2.525f, 3.825f),
            new Vector2(2.15f, 3.25f),
            new Vector2(2.85f, 3.95f),
            new Vector2(6.00f, 4.60f),
            new Vector2(9.50f, 5.70f),
            new Vector2(11.75f, 6.35f),
            new Vector2(12.85f, 8.80f),
            new Vector2(13.60f, 9.625f)
        }, out blockedBy);

        EnemyShadowSeeker seeker = FindComponent<EnemyShadowSeeker>("ShadowSeeker_Topdown");
        bool safePointOutsideDetection = true;
        bool lureInsideDetection = true;
        if (seeker != null)
        {
            safePointOutsideDetection = Vector2.Distance(SeekerSafePointPosition, SeekerHomePosition) > seeker.DetectionRadius;
            lureInsideDetection = Vector2.Distance(LureAreaPosition, SeekerHomePosition) < seeker.DetectionRadius;
        }

        report.AppendLine("routeClearance=" + PassFail(routeClear) + (routeClear ? string.Empty : " blockedBy=" + blockedBy));
        report.AppendLine("seekerSafePointOutsideDetection=" + PassFail(safePointOutsideDetection));
        report.AppendLine("lureAreaInsideDetection=" + PassFail(lureInsideDetection));
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
            || objectName == "River_CrossingBlocker_FromMap")
        {
            return true;
        }

        if (collider.GetComponent<PastedShadowObject>() != null || collider.GetComponentInParent<PastedShadowObject>() != null)
        {
            return true;
        }

        return false;
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
        return pastedShadow;
    }

    private static void ClearRuntimeProbeShadows()
    {
        PastedShadowObject[] pastedShadows = UnityEngine.Object.FindObjectsOfType<PastedShadowObject>();
        for (int i = 0; i < pastedShadows.Length; i++)
        {
            if (pastedShadows[i] != null && pastedShadows[i].name.Contains("ProbePaste"))
            {
                UnityEngine.Object.Destroy(pastedShadows[i].gameObject);
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

    private static GameObject MoveObject(string objectName, Vector3 position, Vector3 scale)
    {
        GameObject gameObject = FindSceneObject(objectName);
        if (gameObject == null)
        {
            return null;
        }

        gameObject.transform.position = new Vector3(position.x, position.y, gameObject.transform.position.z);
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localScale = scale;
        EditorUtility.SetDirty(gameObject);
        return gameObject;
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
            boxCollider = gameObject.GetComponent<Collider2D>() as BoxCollider2D;
        }

        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.size = size;
        boxCollider.offset = offset;
        boxCollider.isTrigger = isTrigger;
        EditorUtility.SetDirty(boxCollider);
    }

    private static void ConfigureWorldBoxObject(string objectName, Vector2 center, Vector2 size, bool isTrigger)
    {
        GameObject gameObject = FindSceneObject(objectName);
        if (gameObject == null)
        {
            return;
        }

        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
            {
                mesh = new Mesh();
                meshFilter.sharedMesh = mesh;
            }

            Vector2 halfSize = size * 0.5f;
            mesh.vertices = new Vector3[]
            {
                new Vector3(center.x - halfSize.x, center.y - halfSize.y, 0f),
                new Vector3(center.x + halfSize.x, center.y - halfSize.y, 0f),
                new Vector3(center.x + halfSize.x, center.y + halfSize.y, 0f),
                new Vector3(center.x - halfSize.x, center.y + halfSize.y, 0f)
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
        }

        BoxCollider2D boxCollider = gameObject.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.offset = center;
        boxCollider.size = size;
        boxCollider.isTrigger = isTrigger;
        EditorUtility.SetDirty(boxCollider);
        EditorUtility.SetDirty(gameObject);
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

    private static string PassFail(bool passed)
    {
        return passed ? "PASS" : "FAIL";
    }

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
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
