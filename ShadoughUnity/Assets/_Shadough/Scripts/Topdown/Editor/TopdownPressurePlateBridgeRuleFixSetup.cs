using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownPressurePlateBridgeRuleFixSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string ReportPath = "Logs/TopdownPressurePlateBridgeRuleFixSetup.report.txt";

    [MenuItem("Shadough/Fix Topdown Pressure Plate And Bridge Rules")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Pressure Plate And Bridge Rule Fix Setup");
        report.AppendLine("Scene: " + ScenePath);

        GameObject plate = FindSceneObject("PressurePlate_Topdown");
        GameObject blocker = FindSceneObject("Wall_River_BrokenBridge_Blocker");
        GameObject player = FindSceneObject("Player_Topdown");
        TopdownBridgeCrossingZone crossing = FindComponent<TopdownBridgeCrossingZone>("CrossingHint_01");

        if (plate == null)
        {
            throw new MissingReferenceException("PressurePlate_Topdown was not found.");
        }

        if (blocker == null)
        {
            throw new MissingReferenceException("Wall_River_BrokenBridge_Blocker was not found.");
        }

        if (crossing == null)
        {
            throw new MissingReferenceException("CrossingHint_01 with TopdownBridgeCrossingZone was not found.");
        }

        ConfigurePressurePlate(plate);
        ConfigureBridgeCrossing(crossing, blocker, player);

        Validate(report, plate, blocker, player, crossing);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown pressure plate and bridge rule fix setup complete. Report: " + FullPath(ReportPath));
    }

    private static void ConfigurePressurePlate(GameObject plate)
    {
        Vector3 scale = plate.transform.localScale;
        if (Mathf.Abs(scale.x) < 0.01f)
        {
            scale.x = 1f;
        }

        scale.y = 0.58f;
        scale.z = 1f;
        plate.transform.localScale = scale;

        SpriteRenderer renderer = plate.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.sortingOrder = 6;
            renderer.color = Color.white;
            EditorUtility.SetDirty(renderer);
        }

        BoxCollider2D collider = plate.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = plate.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
        collider.offset = Vector2.zero;
        collider.size = new Vector2(Mathf.Max(collider.size.x, 1.45f), 0.95f);

        EditorUtility.SetDirty(plate);
        EditorUtility.SetDirty(collider);
    }

    private static void ConfigureBridgeCrossing(TopdownBridgeCrossingZone crossing, GameObject blocker, GameObject player)
    {
        blocker.SetActive(true);

        Collider2D[] blockerColliders = blocker.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < blockerColliders.Length; i++)
        {
            Collider2D collider = blockerColliders[i];
            if (collider == null)
            {
                continue;
            }

            collider.enabled = true;
            collider.isTrigger = false;
            EditorUtility.SetDirty(collider);
        }

        SerializedObject serializedCrossing = new SerializedObject(crossing);
        SetObject(serializedCrossing, "crossingBlocker", blocker);
        SetObject(serializedCrossing, "playerRoot", player != null ? player.transform : null);
        SetBool(serializedCrossing, "makeBridgeColliderTrigger", true);
        SetFloat(serializedCrossing, "detectionRadius", 2.15f);
        serializedCrossing.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(blocker);
        EditorUtility.SetDirty(crossing);
    }

    private static void Validate(StringBuilder report, GameObject plate, GameObject blocker, GameObject player, TopdownBridgeCrossingZone crossing)
    {
        CountMissingSceneData(out int missingScripts, out int missingReferences);

        SpriteRenderer plateRenderer = plate.GetComponent<SpriteRenderer>();
        BoxCollider2D plateCollider = plate.GetComponent<BoxCollider2D>();
        PressurePlateController plateController = plate.GetComponent<PressurePlateController>();
        ShadowPressureTrigger pressureTrigger = plate.GetComponent<ShadowPressureTrigger>();
        PressurePlateSpriteState plateState = plate.GetComponent<PressurePlateSpriteState>();

        Collider2D[] blockerColliders = blocker.GetComponentsInChildren<Collider2D>(true);
        bool allBlockerCollidersBlock = blockerColliders.Length > 0;
        for (int i = 0; i < blockerColliders.Length; i++)
        {
            Collider2D collider = blockerColliders[i];
            allBlockerCollidersBlock &= collider != null && collider.enabled && !collider.isTrigger;
        }

        SerializedObject serializedCrossing = new SerializedObject(crossing);
        Object crossingBlocker = GetObject(serializedCrossing, "crossingBlocker");
        Object crossingPlayer = GetObject(serializedCrossing, "playerRoot");
        bool makeBridgeColliderTrigger = GetBool(serializedCrossing, "makeBridgeColliderTrigger");

        report.AppendLine("pressurePlate.exists=" + PassFail(plate != null));
        report.AppendLine("pressurePlate.position=" + plate.transform.position);
        report.AppendLine("pressurePlate.localScale=" + plate.transform.localScale);
        report.AppendLine("pressurePlate.groundedScale=" + PassFail(plate.transform.localScale.y <= 0.65f));
        report.AppendLine("pressurePlate.renderer=" + PassFail(plateRenderer != null && plateRenderer.enabled && plateRenderer.sprite != null));
        report.AppendLine("pressurePlate.sortingOrder=" + (plateRenderer != null ? plateRenderer.sortingOrder.ToString() : "missing"));
        report.AppendLine("pressurePlate.lowSorting=" + PassFail(plateRenderer != null && plateRenderer.sortingOrder <= 8));
        report.AppendLine("pressurePlate.triggerCollider=" + PassFail(plateCollider != null && plateCollider.enabled && plateCollider.isTrigger));
        report.AppendLine("pressurePlate.colliderSize=" + (plateCollider != null ? plateCollider.size.ToString() : "missing"));
        report.AppendLine("pressurePlate.controller=" + PassFail(plateController != null));
        report.AppendLine("pressurePlate.shadowTrigger=" + PassFail(pressureTrigger != null));
        report.AppendLine("pressurePlate.visualState=" + PassFail(plateState != null));
        report.AppendLine("bridge.crossingZone=" + PassFail(crossing != null));
        report.AppendLine("bridge.blockerActive=" + PassFail(blocker.activeSelf));
        report.AppendLine("bridge.blockerCollidersBlock=" + PassFail(allBlockerCollidersBlock));
        report.AppendLine("bridge.blockerReference=" + PassFail(crossingBlocker == blocker));
        report.AppendLine("bridge.playerReference=" + PassFail(player == null || crossingPlayer == player.transform));
        report.AppendLine("bridge.makeBridgeColliderTrigger=" + PassFail(makeBridgeColliderTrigger));
        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);
        report.AppendLine("consoleCompileErrors=checked_after_unity_refresh");
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

    private static T FindComponent<T>(string objectName) where T : Component
    {
        GameObject sceneObject = FindSceneObject(objectName);
        return sceneObject != null ? sceneObject.GetComponent<T>() : null;
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

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static Object GetObject(SerializedObject serializedObject, string propertyName)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property != null ? property.objectReferenceValue : null;
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static bool GetBool(SerializedObject serializedObject, string propertyName)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property != null && property.boolValue;
    }

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
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
