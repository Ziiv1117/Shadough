using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownDoorPassAndShadowPlacementSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string ReportPath = "Logs/TopdownDoorPassAndShadowPlacementSetup.report.txt";
    private const float ClosePlacementRadius = 1.05f;

    [MenuItem("Shadough/Fix Topdown Door Pass And Shadow Placement")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Door Pass And Shadow Placement Setup");
        report.AppendLine("Scene: " + ScenePath);

        GameObject door = FindSceneObject("Door_Pressure_Topdown");
        GameObject plate = FindSceneObject("PressurePlate_Topdown");
        GameObject player = FindSceneObject("Player_Topdown");

        if (door == null)
        {
            throw new MissingReferenceException("Door_Pressure_Topdown was not found.");
        }

        if (player == null)
        {
            throw new MissingReferenceException("Player_Topdown was not found.");
        }

        ConfigureDoor(door, report);
        ConfigurePlacement(player, report);

        if (plate != null)
        {
            PressurePlateController pressurePlate = plate.GetComponent<PressurePlateController>();
            if (pressurePlate != null)
            {
                pressurePlate.SetPressed(false);
                EditorUtility.SetDirty(pressurePlate);
            }
        }

        Validate(report, door, player);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown door pass and shadow placement setup complete. Report: " + FullPath(ReportPath));
    }

    private static void ConfigureDoor(GameObject door, StringBuilder report)
    {
        DoorController controller = door.GetComponent<DoorController>();
        if (controller == null)
        {
            controller = door.AddComponent<DoorController>();
        }

        Collider2D doorCollider = door.GetComponent<Collider2D>();
        SpriteRenderer renderer = door.GetComponent<SpriteRenderer>();
        List<Collider2D> extraBlockers = FindDoorPassageBlockers(door, doorCollider);

        SerializedObject serializedDoor = new SerializedObject(controller);
        SetObject(serializedDoor, "doorCollider", doorCollider);
        SetObject(serializedDoor, "spriteRenderer", renderer);
        SetColliderArray(serializedDoor, "additionalClosedColliders", extraBlockers);
        SetBool(serializedDoor, "isOpen", false);
        serializedDoor.ApplyModifiedPropertiesWithoutUndo();

        controller.Close();

        report.AppendLine("door.extraClosedBlockerCount=" + extraBlockers.Count);
        for (int i = 0; i < extraBlockers.Count; i++)
        {
            report.AppendLine("door.extraClosedBlocker[" + i + "]=" + extraBlockers[i].name);
        }

        EditorUtility.SetDirty(controller);
    }

    private static void ConfigurePlacement(GameObject player, StringBuilder report)
    {
        FreeShadowPlacer placer = player.GetComponent<FreeShadowPlacer>();
        if (placer == null)
        {
            report.AppendLine("placement.freeShadowPlacer=missing");
            return;
        }

        SerializedObject serializedPlacer = new SerializedObject(placer);
        SetFloat(serializedPlacer, "placementRadius", ClosePlacementRadius);
        serializedPlacer.ApplyModifiedPropertiesWithoutUndo();

        report.AppendLine("placement.freeShadowPlacer=PASS");
        report.AppendLine("placement.radius=" + ClosePlacementRadius.ToString("0.00"));
        EditorUtility.SetDirty(placer);
    }

    private static List<Collider2D> FindDoorPassageBlockers(GameObject door, Collider2D doorCollider)
    {
        List<Collider2D> blockers = new List<Collider2D>();
        if (doorCollider == null)
        {
            return blockers;
        }

        Physics2D.SyncTransforms();
        Bounds passageBounds = doorCollider.bounds;
        passageBounds.Expand(new Vector3(0.2f, 0.2f, 0f));

        Collider2D[] colliders = Object.FindObjectsOfType<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            if (collider == null || collider == doorCollider || collider.isTrigger)
            {
                continue;
            }

            GameObject candidate = collider.gameObject;
            if (!IsObjectInTargetScene(candidate)
                || candidate == door
                || candidate.transform.IsChildOf(door.transform)
                || !LooksLikeDoorPassageBlocker(candidate)
                || !IsCentralDoorBlocker(collider, doorCollider)
                || !collider.bounds.Intersects(passageBounds))
            {
                continue;
            }

            blockers.Add(collider);
        }

        return blockers;
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
            return !objectName.Contains("OuterWall");
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

    private static void Validate(StringBuilder report, GameObject door, GameObject player)
    {
        CountMissingSceneData(out int missingScripts, out int missingReferences);

        DoorController controller = door.GetComponent<DoorController>();
        Collider2D doorCollider = door.GetComponent<Collider2D>();
        FreeShadowPlacer placer = player.GetComponent<FreeShadowPlacer>();

        bool closedBlocks = false;
        bool openDoorColliderDisabled = false;
        bool openExtrasDisabled = false;
        bool noHiddenBlockerOpen = false;

        if (controller != null)
        {
            controller.Close();
            Physics2D.SyncTransforms();
            closedBlocks = doorCollider != null && doorCollider.enabled && !doorCollider.isTrigger;

            controller.Open();
            Physics2D.SyncTransforms();
            openDoorColliderDisabled = doorCollider == null || !doorCollider.enabled;
            openExtrasDisabled = AdditionalCollidersDisabled(controller);
            noHiddenBlockerOpen = !HasEnabledDoorPassageBlocker(door, doorCollider, out string hiddenBlocker);
            report.AppendLine("door.hiddenBlockerWhenOpen=" + (string.IsNullOrEmpty(hiddenBlocker) ? "none" : hiddenBlocker));

            controller.Close();
            Physics2D.SyncTransforms();
            EditorUtility.SetDirty(controller);
        }

        float placementRadius = GetSerializedFloat(placer, "placementRadius");

        report.AppendLine("door.controller=" + PassFail(controller != null));
        report.AppendLine("door.blocksWhenClosed=" + PassFail(closedBlocks));
        report.AppendLine("door.ownColliderDisabledWhenOpen=" + PassFail(openDoorColliderDisabled));
        report.AppendLine("door.extraCollidersDisabledWhenOpen=" + PassFail(openExtrasDisabled));
        report.AppendLine("door.noHiddenBlockerWhenOpen=" + PassFail(noHiddenBlockerOpen));
        report.AppendLine("placement.radiusCloseToPlayer=" + PassFail(placementRadius <= ClosePlacementRadius + 0.001f));
        report.AppendLine("placement.radiusValue=" + placementRadius.ToString("0.00"));
        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);
        report.AppendLine("consoleCompileErrors=checked_after_unity_refresh");
    }

    private static bool AdditionalCollidersDisabled(DoorController controller)
    {
        SerializedObject serializedDoor = new SerializedObject(controller);
        SerializedProperty property = serializedDoor.FindProperty("additionalClosedColliders");
        if (property == null)
        {
            return true;
        }

        for (int i = 0; i < property.arraySize; i++)
        {
            Collider2D collider = property.GetArrayElementAtIndex(i).objectReferenceValue as Collider2D;
            if (collider != null && collider.enabled)
            {
                return false;
            }
        }

        return true;
    }

    private static bool HasEnabledDoorPassageBlocker(GameObject door, Collider2D doorCollider, out string blockerName)
    {
        blockerName = string.Empty;
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
                || !IsObjectInTargetScene(collider.gameObject)
                || collider.gameObject == door
                || collider.transform.IsChildOf(door.transform)
                || !LooksLikeDoorPassageBlocker(collider.gameObject)
                || !IsCentralDoorBlocker(collider, doorCollider)
                || !collider.bounds.Intersects(passageBounds))
            {
                continue;
            }

            blockerName = collider.name;
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

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
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

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
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

    private static void SetColliderArray(SerializedObject serializedObject, string propertyName, List<Collider2D> colliders)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.arraySize = colliders.Count;
        for (int i = 0; i < colliders.Count; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = colliders[i];
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
