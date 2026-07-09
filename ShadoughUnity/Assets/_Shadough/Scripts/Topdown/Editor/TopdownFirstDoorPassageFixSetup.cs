using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownFirstDoorPassageFixSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string ReportPath = "Logs/TopdownFirstDoorPassageFixSetup.report.txt";
    private const float SideWallThickness = 0.48f;
    private const float PassageWidth = 1.45f;
    private const float PassageLength = 3.2f;

    [MenuItem("Shadough/Fix Topdown First Door Passage")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        List<string> report = new List<string>
        {
            "Topdown First Door Passage Fix Setup",
            "Scene: " + ScenePath
        };

        GameObject door = FindSceneObject("Door_Pressure_Topdown");
        GameObject nextDoor = FindSceneObject("Door_Lock_Topdown");
        GameObject player = FindSceneObject("Player_Topdown");
        if (door == null)
        {
            throw new MissingReferenceException("Door_Pressure_Topdown was not found.");
        }

        DoorController doorController = door.GetComponent<DoorController>();
        BoxCollider2D doorCollider = door.GetComponent<BoxCollider2D>();
        if (doorController == null || doorCollider == null)
        {
            throw new MissingReferenceException("Door_Pressure_Topdown is missing DoorController or BoxCollider2D.");
        }

        Vector2 routeDirection = ResolveRouteDirection(door, nextDoor);
        Vector2 sideDirection = new Vector2(-routeDirection.y, routeDirection.x);
        Transform blockerParent = EnsureChild(EnsureRoot("Graybox_Collision_Topdown").transform, "CanPressDoor_Escape_Blockers");

        Physics2D.SyncTransforms();
        Vector2 doorCenter = doorCollider.bounds.center;
        ConfigureSideWall(blockerParent, "Wall_CanPressDoor_LeftOuterWall_Blocker", doorCenter + sideDirection * (PassageWidth * 0.5f + SideWallThickness * 0.5f), routeDirection, sideDirection);
        ConfigureSideWall(blockerParent, "Wall_CanPressDoor_RightOuterWall_Blocker", doorCenter - sideDirection * (PassageWidth * 0.5f + SideWallThickness * 0.5f), routeDirection, sideDirection);

        List<Collider2D> centralBlockers = FindCentralDoorBlockers(door, doorCollider, routeDirection, sideDirection);
        SerializedObject serializedDoor = new SerializedObject(doorController);
        SetObject(serializedDoor, "doorCollider", doorCollider);
        SetColliderArray(serializedDoor, "additionalClosedColliders", centralBlockers);
        SetBool(serializedDoor, "isOpen", false);
        serializedDoor.ApplyModifiedPropertiesWithoutUndo();
        doorController.Close();

        report.Add("door.position=" + door.transform.position);
        report.Add("routeDirection=" + routeDirection);
        report.Add("sideDirection=" + sideDirection);
        report.Add("passage.width=" + PassageWidth.ToString("0.00"));
        report.Add("passage.length=" + PassageLength.ToString("0.00"));
        report.Add("door.centralBlockerCount=" + centralBlockers.Count);
        for (int i = 0; i < centralBlockers.Count; i++)
        {
            report.Add("door.centralBlocker[" + i + "]=" + centralBlockers[i].name);
        }

        Validate(report, doorController, doorCollider, player, routeDirection, sideDirection);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllLines(FullPath(ReportPath), report);
        Debug.Log("Topdown first door passage fix complete. Report: " + FullPath(ReportPath));
    }

    private static Vector2 ResolveRouteDirection(GameObject door, GameObject nextDoor)
    {
        Vector2 route = nextDoor != null
            ? (Vector2)(nextDoor.transform.position - door.transform.position)
            : Vector2.right;

        if (route.sqrMagnitude < 0.001f)
        {
            route = Vector2.right;
        }

        return route.normalized;
    }

    private static void ConfigureSideWall(Transform parent, string objectName, Vector2 position, Vector2 routeDirection, Vector2 sideDirection)
    {
        GameObject wall = EnsureChild(parent, objectName).gameObject;
        wall.transform.position = new Vector3(position.x, position.y, 0f);
        wall.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(sideDirection.y, sideDirection.x) * Mathf.Rad2Deg);
        wall.transform.localScale = Vector3.one;

        BoxCollider2D collider = wall.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = wall.AddComponent<BoxCollider2D>();
        }

        collider.enabled = true;
        collider.isTrigger = false;
        collider.offset = Vector2.zero;
        collider.size = new Vector2(SideWallThickness, PassageLength);

        SpriteRenderer renderer = wall.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        EditorUtility.SetDirty(wall);
        EditorUtility.SetDirty(collider);
    }

    private static List<Collider2D> FindCentralDoorBlockers(GameObject door, Collider2D doorCollider, Vector2 routeDirection, Vector2 sideDirection)
    {
        List<Collider2D> blockers = new List<Collider2D>();
        Collider2D[] colliders = Object.FindObjectsOfType<Collider2D>(true);
        Vector2 center = doorCollider.bounds.center;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            if (collider == null
                || collider == doorCollider
                || collider.isTrigger
                || !IsObjectInTargetScene(collider.gameObject)
                || collider.gameObject == door
                || collider.transform.IsChildOf(door.transform)
                || IsOuterWallBlocker(collider.gameObject)
                || !LooksLikeDoorBlocker(collider.gameObject))
            {
                continue;
            }

            Vector2 delta = (Vector2)collider.bounds.center - center;
            float routeDistance = Mathf.Abs(Vector2.Dot(delta, routeDirection));
            float sideDistance = Mathf.Abs(Vector2.Dot(delta, sideDirection));
            if (routeDistance <= PassageLength * 0.5f && sideDistance <= PassageWidth * 0.5f)
            {
                blockers.Add(collider);
            }
        }

        return blockers;
    }

    private static void Validate(List<string> report, DoorController doorController, Collider2D doorCollider, GameObject player, Vector2 routeDirection, Vector2 sideDirection)
    {
        CountMissingSceneData(out int missingScripts, out int missingReferences);

        doorController.Close();
        Physics2D.SyncTransforms();
        bool doorBlocksWhenClosed = doorCollider != null && doorCollider.enabled && !doorCollider.isTrigger;

        doorController.Open();
        Physics2D.SyncTransforms();
        bool doorColliderOffWhenOpen = doorCollider == null || !doorCollider.enabled;
        string blockerInPassage;
        bool passageClear = !HasEnabledCentralBlocker(doorCollider, routeDirection, sideDirection, out blockerInPassage);
        bool sideWallsBlock = IsSideWallBlocking("Wall_CanPressDoor_LeftOuterWall_Blocker") && IsSideWallBlocking("Wall_CanPressDoor_RightOuterWall_Blocker");

        float playerWidth = ResolvePlayerWidth(player);
        bool passageWideEnough = PassageWidth >= playerWidth + 0.35f;

        doorController.Close();
        Physics2D.SyncTransforms();
        EditorUtility.SetDirty(doorController);

        report.Add("validate.doorBlocksWhenClosed=" + PassFail(doorBlocksWhenClosed));
        report.Add("validate.doorColliderOffWhenOpen=" + PassFail(doorColliderOffWhenOpen));
        report.Add("validate.passageClearWhenOpen=" + PassFail(passageClear));
        report.Add("validate.blockerInPassageWhenOpen=" + (string.IsNullOrEmpty(blockerInPassage) ? "none" : blockerInPassage));
        report.Add("validate.sideWallsStillBlock=" + PassFail(sideWallsBlock));
        report.Add("validate.playerWidth=" + playerWidth.ToString("0.00"));
        report.Add("validate.passageWideEnough=" + PassFail(passageWideEnough));
        report.Add("missingScripts=" + missingScripts);
        report.Add("missingReferences=" + missingReferences);
        report.Add("consoleCompileErrors=checked_after_unity_refresh");
    }

    private static bool HasEnabledCentralBlocker(Collider2D doorCollider, Vector2 routeDirection, Vector2 sideDirection, out string blockerName)
    {
        blockerName = string.Empty;
        if (doorCollider == null)
        {
            return false;
        }

        Vector2 center = doorCollider.bounds.center;
        Collider2D[] colliders = Object.FindObjectsOfType<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D collider = colliders[i];
            if (collider == null
                || !collider.enabled
                || collider.isTrigger
                || collider == doorCollider
                || IsOuterWallBlocker(collider.gameObject)
                || !LooksLikeDoorBlocker(collider.gameObject)
                || !IsObjectInTargetScene(collider.gameObject))
            {
                continue;
            }

            Vector2 delta = (Vector2)collider.bounds.center - center;
            float routeDistance = Mathf.Abs(Vector2.Dot(delta, routeDirection));
            float sideDistance = Mathf.Abs(Vector2.Dot(delta, sideDirection));
            if (routeDistance <= PassageLength * 0.5f && sideDistance <= PassageWidth * 0.5f)
            {
                blockerName = collider.name;
                return true;
            }
        }

        return false;
    }

    private static bool IsSideWallBlocking(string objectName)
    {
        BoxCollider2D collider = FindComponent<BoxCollider2D>(objectName);
        return collider != null && collider.enabled && !collider.isTrigger;
    }

    private static float ResolvePlayerWidth(GameObject player)
    {
        Collider2D collider = player != null ? player.GetComponentInChildren<Collider2D>() : null;
        return collider != null ? Mathf.Min(collider.bounds.size.x, collider.bounds.size.y) : 0.7f;
    }

    private static bool LooksLikeDoorBlocker(GameObject gameObject)
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

    private static bool IsOuterWallBlocker(GameObject gameObject)
    {
        return gameObject != null && gameObject.name.Contains("OuterWall");
    }

    private static GameObject EnsureRoot(string rootName)
    {
        GameObject root = FindSceneObject(rootName);
        if (root == null)
        {
            root = new GameObject(rootName);
        }

        return root;
    }

    private static Transform EnsureChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child == null)
        {
            GameObject childObject = new GameObject(childName);
            childObject.transform.SetParent(parent, false);
            child = childObject.transform;
        }

        return child;
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

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
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
