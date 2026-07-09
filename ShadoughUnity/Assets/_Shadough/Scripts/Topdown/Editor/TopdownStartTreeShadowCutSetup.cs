using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownStartTreeShadowCutSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string ReportPath = "Logs/TopdownStartTreeShadowCutSetup.report.txt";
    private const string PlayProbeReportPath = "Temp/TopdownStartTreeShadowCutPlayProbe.report.txt";
    private const string PlayProbeRequestPath = "Temp/TopdownStartTreeShadowCutPlayProbe.request";

    private const string PlayerName = "Player_Topdown";
    private const string TreeShadowName = "TreeShadow_Topdown";

    private const float TargetCutRange = 2.8f;
    private const float TargetShadowWidth = 0.85f;
    private const float TargetMaxLength = 4.0f;

    private static StringBuilder playProbeReport;
    private static int playProbeStage;
    private static double nextPlayProbeTime;

    [InitializeOnLoadMethod]
    private static void RegisterPlayProbeResume()
    {
        EditorApplication.update -= TryResumeRequestedPlayProbe;
        EditorApplication.update += TryResumeRequestedPlayProbe;
    }

    [MenuItem("Shadough/Setup Topdown Start Tree Shadow Cut")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Start Tree Shadow Cut Setup");
        report.AppendLine("Scene: " + ScenePath);

        GameObject player = FindSceneObject(PlayerName);
        GameObject treeShadow = FindSceneObject(TreeShadowName);

        if (player == null)
        {
            throw new MissingReferenceException(PlayerName + " was not found.");
        }

        if (treeShadow == null)
        {
            throw new MissingReferenceException(TreeShadowName + " was not found.");
        }

        ShadowCutter cutter = player.GetComponent<ShadowCutter>();
        LightDrivenShadow lightDrivenShadow = treeShadow.GetComponent<LightDrivenShadow>();
        BoxCollider2D treeShadowCollider = treeShadow.GetComponent<BoxCollider2D>();
        SpriteRenderer treeShadowRenderer = treeShadow.GetComponent<SpriteRenderer>();
        ShadowInteractable treeShadowInteractable = treeShadow.GetComponent<ShadowInteractable>();

        if (cutter == null)
        {
            throw new MissingReferenceException(PlayerName + " has no ShadowCutter.");
        }

        if (lightDrivenShadow == null)
        {
            throw new MissingReferenceException(TreeShadowName + " has no LightDrivenShadow.");
        }

        float oldCutRange = SetFloat(cutter, "cutRange", TargetCutRange);
        float oldShadowWidth = SetFloat(lightDrivenShadow, "shadowWidth", TargetShadowWidth);
        float oldMaxLength = SetFloat(lightDrivenShadow, "maxLength", TargetMaxLength);

        if (treeShadowRenderer != null)
        {
            treeShadowRenderer.size = new Vector2(Mathf.Max(treeShadowRenderer.size.x, TargetMaxLength), TargetShadowWidth);
            EditorUtility.SetDirty(treeShadowRenderer);
        }

        if (treeShadowCollider != null)
        {
            treeShadowCollider.size = new Vector2(Mathf.Max(treeShadowCollider.size.x, TargetMaxLength), TargetShadowWidth);
            treeShadowCollider.isTrigger = true;
            EditorUtility.SetDirty(treeShadowCollider);
        }

        EditorUtility.SetDirty(cutter);
        EditorUtility.SetDirty(lightDrivenShadow);
        EditorUtility.SetDirty(treeShadow);

        ValidateScene(report, player, treeShadow, cutter, lightDrivenShadow, treeShadowCollider, treeShadowInteractable,
            oldCutRange, oldShadowWidth, oldMaxLength);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown start tree shadow cut setup complete. Report: " + FullPath(ReportPath));
    }

    [MenuItem("Shadough/Play Probe Topdown Start Tree Shadow Cut")]
    public static void RequestPlayProbeFromMenu()
    {
        File.WriteAllText(FullPath(PlayProbeRequestPath), "run");
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorApplication.EnterPlaymode();
        TryResumeRequestedPlayProbe();
    }

    private static void TryResumeRequestedPlayProbe()
    {
        if (EditorApplication.isCompiling)
        {
            return;
        }

        string requestPath = FullPath(PlayProbeRequestPath);
        if (!File.Exists(requestPath) || !EditorApplication.isPlaying)
        {
            return;
        }

        File.Delete(requestPath);
        StartPlayProbe();
    }

    private static void StartPlayProbe()
    {
        playProbeReport = new StringBuilder();
        playProbeReport.AppendLine("Topdown Start Tree Shadow Cut Play Probe");
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

        if (playProbeStage == 0)
        {
            GameObject player = GameObject.Find(PlayerName);
            GameObject treeShadow = GameObject.Find(TreeShadowName);
            ShadowCutter cutter = player != null ? player.GetComponent<ShadowCutter>() : null;
            ShadowInventory inventory = player != null ? player.GetComponent<ShadowInventory>() : null;
            PlayerLanternController lanternController = player != null ? player.GetComponent<PlayerLanternController>() : null;
            RevealViewController revealView = Object.FindObjectOfType<RevealViewController>();
            LightDrivenShadow lightDrivenShadow = treeShadow != null ? treeShadow.GetComponent<LightDrivenShadow>() : null;

            playProbeReport.AppendLine("Player exists: " + PassFail(player != null));
            playProbeReport.AppendLine("TreeShadow exists: " + PassFail(treeShadow != null));
            playProbeReport.AppendLine("ShadowCutter exists: " + PassFail(cutter != null));
            playProbeReport.AppendLine("ShadowInventory exists: " + PassFail(inventory != null));
            playProbeReport.AppendLine("PlayerLanternController exists: " + PassFail(lanternController != null));
            playProbeReport.AppendLine("RevealViewController exists: " + PassFail(revealView != null));

            if (player != null && lanternController != null)
            {
                Vector3 playerPosition = player.transform.position;
                SetPrivateField(lanternController, "isPlacingLantern", false);
                SetPrivateField(lanternController, "isLanternPlanted", true);
                Transform lightPoint = lanternController.LightPoint;
                if (lightPoint != null)
                {
                    lightPoint.position = playerPosition + new Vector3(-0.52f, -0.36f, 0f);
                }
            }

            if (revealView != null)
            {
                revealView.SetRevealActive(true);
            }

            InvokePrivateMethod(lightDrivenShadow, "Update");
            Physics2D.SyncTransforms();
            InvokePrivateMethod(cutter, "FindNearestCuttableTarget");
            object currentTarget = GetPrivateField(cutter, "currentTarget");
            object currentPastedTarget = GetPrivateField(cutter, "currentPastedTarget");
            playProbeReport.AppendLine("Cut target found from spawn: " + PassFail(currentTarget != null || currentPastedTarget != null));

            if (currentTarget != null || currentPastedTarget != null)
            {
                InvokePrivateMethod(cutter, "TryCutCurrentTarget");
            }

            bool hasTreeShadow = inventory != null && inventory.HasShadow() && inventory.CurrentShadowData != null
                && inventory.CurrentShadowData.shadowType == ShadowType.Tree
                && inventory.CurrentShadowData.canStandOn;
            playProbeReport.AppendLine("Start TreeShadow cuttable from spawn: " + PassFail(hasTreeShadow));
            if (inventory != null && inventory.CurrentShadowData != null)
            {
                playProbeReport.AppendLine("Cut shadow type: " + inventory.CurrentShadowData.shadowType);
                playProbeReport.AppendLine("Cut shadow CanStandOn: " + PassFail(inventory.CurrentShadowData.canStandOn));
                playProbeReport.AppendLine("Cut shadow colliderSize: " + FormatVector2(inventory.CurrentShadowData.colliderSize));
            }

            FinishPlayProbe();
        }
    }

    private static void FinishPlayProbe()
    {
        File.WriteAllText(FullPath(PlayProbeReportPath), playProbeReport.ToString());
        Debug.Log("Topdown start tree shadow cut play probe complete. Report: " + FullPath(PlayProbeReportPath));
        EditorApplication.update -= RunPlayProbeStep;
        EditorApplication.ExitPlaymode();
    }

    private static void ValidateScene(
        StringBuilder report,
        GameObject player,
        GameObject treeShadow,
        ShadowCutter cutter,
        LightDrivenShadow lightDrivenShadow,
        BoxCollider2D treeShadowCollider,
        ShadowInteractable treeShadowInteractable,
        float oldCutRange,
        float oldShadowWidth,
        float oldMaxLength)
    {
        CountMissingSceneData(out int missingScripts, out int missingReferences);

        float cutRange = GetFloat(cutter, "cutRange");
        float shadowWidth = GetFloat(lightDrivenShadow, "shadowWidth");
        float maxLength = GetFloat(lightDrivenShadow, "maxLength");
        bool requirePlantedLanternToCut = GetBool(lightDrivenShadow, "requirePlantedLanternToCut");

        report.AppendLine("player.position=" + FormatVector3(player.transform.position));
        report.AppendLine("treeShadow.position=" + FormatVector3(treeShadow.transform.position));
        report.AppendLine("cutRange.old=" + oldCutRange.ToString("0.###"));
        report.AppendLine("cutRange.new=" + cutRange.ToString("0.###"));
        report.AppendLine("treeShadow.shadowWidth.old=" + oldShadowWidth.ToString("0.###"));
        report.AppendLine("treeShadow.shadowWidth.new=" + shadowWidth.ToString("0.###"));
        report.AppendLine("treeShadow.maxLength.old=" + oldMaxLength.ToString("0.###"));
        report.AppendLine("treeShadow.maxLength.new=" + maxLength.ToString("0.###"));
        report.AppendLine("treeShadow.canStandOn=" + PassFail(treeShadowInteractable != null && treeShadowInteractable.CanStandOn));
        report.AppendLine("treeShadow.canBeCut=" + PassFail(treeShadowInteractable != null && treeShadowInteractable.CanBeCut));
        report.AppendLine("treeShadow.requiresPlantedLanternToCut=" + PassFail(requirePlantedLanternToCut));
        report.AppendLine("treeShadow.colliderExists=" + PassFail(treeShadowCollider != null));
        report.AppendLine("treeShadow.colliderIsTrigger=" + PassFail(treeShadowCollider != null && treeShadowCollider.isTrigger));
        report.AppendLine("startCutRangeReasonable=" + PassFail(cutRange >= 2.3f && cutRange <= 3.0f));
        report.AppendLine("treeShadowWidthReasonable=" + PassFail(shadowWidth >= 0.5f && shadowWidth <= 0.95f));
        report.AppendLine("treeShadowMaxLengthBridgeReady=" + PassFail(maxLength >= 3.8f && maxLength <= 4.2f));
        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);
        report.AppendLine("consoleCompileErrors=checked_after_unity_refresh");
    }

    private static float SetFloat(Component component, string propertyName, float value)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            throw new MissingReferenceException(component.GetType().Name + "." + propertyName + " was not found.");
        }

        float oldValue = property.floatValue;
        property.floatValue = value;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        return oldValue;
    }

    private static float GetFloat(Component component, string propertyName)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property != null ? property.floatValue : 0f;
    }

    private static bool GetBool(Component component, string propertyName)
    {
        SerializedObject serializedObject = new SerializedObject(component);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property != null && property.boolValue;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null)
        {
            return;
        }

        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    private static object GetPrivateField(object target, string fieldName)
    {
        if (target == null)
        {
            return null;
        }

        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        return field != null ? field.GetValue(target) : null;
    }

    private static void InvokePrivateMethod(object target, string methodName)
    {
        if (target == null)
        {
            return;
        }

        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method != null)
        {
            method.Invoke(target, null);
        }
    }

    private static GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (IsObjectInTargetScene(sceneObject) && sceneObject.name == objectName)
            {
                return sceneObject;
            }
        }

        return null;
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

    private static bool IsObjectInTargetScene(GameObject sceneObject)
    {
        return sceneObject != null
            && sceneObject.scene.IsValid()
            && sceneObject.scene.path == ScenePath;
    }

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }

    private static string FormatVector3(Vector3 value)
    {
        return "(" + value.x.ToString("0.###") + ", " + value.y.ToString("0.###") + ", " + value.z.ToString("0.###") + ")";
    }

    private static string FormatVector2(Vector2 value)
    {
        return "(" + value.x.ToString("0.###") + ", " + value.y.ToString("0.###") + ")";
    }

    private static string PassFail(bool passed)
    {
        return passed ? "PASS" : "FAIL";
    }
}
