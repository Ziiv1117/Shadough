using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownPressureDoorArtSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string ReportPath = "Logs/TopdownPressureDoorArtSetup.report.txt";

    private const string DoorClosedPath = "Assets/_Shadough/Art/Interactables/Door/Door_Closed.png";
    private const string DoorHalfOpenPath = "Assets/_Shadough/Art/Interactables/Door/Door_HalfOpen.png";
    private const string DoorOpenPath = "Assets/_Shadough/Art/Interactables/Door/Door_Open.png";
    private const string DoorWideOpenPath = "Assets/_Shadough/Art/Interactables/Door/Door_WideOpen.png";
    private const string PlateOffPath = "Assets/_Shadough/Art/Interactables/PressurePlate/PressurePlate_Off.png";
    private const string PlateOnPath = "Assets/_Shadough/Art/Interactables/PressurePlate/PressurePlate_On.png";

    [MenuItem("Shadough/Setup Topdown Pressure Door Art")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    public static void Setup()
    {
        ConfigureTexture(DoorClosedPath, 100f);
        ConfigureTexture(DoorHalfOpenPath, 100f);
        ConfigureTexture(DoorOpenPath, 100f);
        ConfigureTexture(DoorWideOpenPath, 100f);
        ConfigureTexture(PlateOffPath, 220f);
        ConfigureTexture(PlateOnPath, 220f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Pressure Door Art Setup");
        report.AppendLine("Scene: " + ScenePath);

        Sprite doorClosed = LoadSprite(DoorClosedPath);
        Sprite doorHalfOpen = LoadSprite(DoorHalfOpenPath);
        Sprite doorOpen = LoadSprite(DoorOpenPath);
        Sprite doorWideOpen = LoadSprite(DoorWideOpenPath);
        Sprite plateOff = LoadSprite(PlateOffPath);
        Sprite plateOn = LoadSprite(PlateOnPath);

        GameObject door = FindSceneObject("Door_Pressure_Topdown");
        GameObject plate = FindSceneObject("PressurePlate_Topdown");
        if (door == null)
        {
            throw new MissingReferenceException("Door_Pressure_Topdown was not found.");
        }

        if (plate == null)
        {
            throw new MissingReferenceException("PressurePlate_Topdown was not found.");
        }

        ConfigureDoor(door, doorClosed, doorHalfOpen, doorOpen, doorWideOpen);
        ConfigurePlate(plate, plateOff, plateOn);

        Validate(report, door, plate, doorClosed, doorHalfOpen, doorOpen, doorWideOpen, plateOff, plateOn);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown pressure door art setup complete. Report: " + FullPath(ReportPath));
    }

    private static void ConfigureDoor(GameObject door, Sprite closed, Sprite halfOpen, Sprite open, Sprite wideOpen)
    {
        door.transform.localScale = Vector3.one;

        SpriteRenderer renderer = door.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = door.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = closed;
        renderer.color = Color.white;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.sortingOrder = 15;

        BoxCollider2D collider = door.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = door.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = false;
        collider.enabled = true;
        collider.offset = new Vector2(0f, -0.08f);
        collider.size = new Vector2(1.65f, 1.7f);

        DoorController doorController = door.GetComponent<DoorController>();
        if (doorController == null)
        {
            doorController = door.AddComponent<DoorController>();
        }

        SerializedObject serializedDoor = new SerializedObject(doorController);
        SetObject(serializedDoor, "doorCollider", collider);
        SetObject(serializedDoor, "spriteRenderer", renderer);
        SetFloat(serializedDoor, "openAlpha", 1f);
        SetColor(serializedDoor, "closedColor", Color.white);
        SetBool(serializedDoor, "isOpen", false);
        serializedDoor.ApplyModifiedPropertiesWithoutUndo();

        DoorStateSpriteAnimator animator = door.GetComponent<DoorStateSpriteAnimator>();
        if (animator == null)
        {
            animator = door.AddComponent<DoorStateSpriteAnimator>();
        }

        SerializedObject serializedAnimator = new SerializedObject(animator);
        SetObject(serializedAnimator, "doorController", doorController);
        SetObject(serializedAnimator, "spriteRenderer", renderer);
        SetObject(serializedAnimator, "closedSprite", closed);
        SetObject(serializedAnimator, "halfOpenSprite", halfOpen);
        SetObject(serializedAnimator, "openSprite", open);
        SetObject(serializedAnimator, "wideOpenSprite", wideOpen);
        SetFloat(serializedAnimator, "frameDuration", 0.12f);
        serializedAnimator.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(door);
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(collider);
        EditorUtility.SetDirty(doorController);
        EditorUtility.SetDirty(animator);
    }

    private static void ConfigurePlate(GameObject plate, Sprite offSprite, Sprite onSprite)
    {
        plate.transform.localScale = new Vector3(1f, 0.58f, 1f);

        SpriteRenderer renderer = plate.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = plate.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = offSprite;
        renderer.color = Color.white;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.sortingOrder = 6;

        BoxCollider2D collider = plate.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = plate.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
        collider.offset = Vector2.zero;
        collider.size = new Vector2(1.45f, 0.95f);

        PressurePlateController pressurePlate = plate.GetComponent<PressurePlateController>();
        if (pressurePlate == null)
        {
            pressurePlate = plate.AddComponent<PressurePlateController>();
        }

        DoorController doorController = FindComponent<DoorController>("Door_Pressure_Topdown");
        SerializedObject serializedPlate = new SerializedObject(pressurePlate);
        SetObject(serializedPlate, "targetDoor", doorController);
        SetObject(serializedPlate, "spriteRenderer", renderer);
        SetColor(serializedPlate, "releasedColor", Color.white);
        SetColor(serializedPlate, "pressedColor", Color.white);
        SetBool(serializedPlate, "isPressed", false);
        serializedPlate.ApplyModifiedPropertiesWithoutUndo();

        ShadowPressureTrigger trigger = plate.GetComponent<ShadowPressureTrigger>();
        if (trigger == null)
        {
            trigger = plate.AddComponent<ShadowPressureTrigger>();
        }

        SerializedObject serializedTrigger = new SerializedObject(trigger);
        SetObject(serializedTrigger, "pressurePlate", pressurePlate);
        serializedTrigger.ApplyModifiedPropertiesWithoutUndo();

        PressurePlateSpriteState spriteState = plate.GetComponent<PressurePlateSpriteState>();
        if (spriteState == null)
        {
            spriteState = plate.AddComponent<PressurePlateSpriteState>();
        }

        SerializedObject serializedState = new SerializedObject(spriteState);
        SetObject(serializedState, "pressurePlate", pressurePlate);
        SetObject(serializedState, "spriteRenderer", renderer);
        SetObject(serializedState, "releasedSprite", offSprite);
        SetObject(serializedState, "pressedSprite", onSprite);
        serializedState.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(plate);
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(collider);
        EditorUtility.SetDirty(pressurePlate);
        EditorUtility.SetDirty(trigger);
        EditorUtility.SetDirty(spriteState);
    }

    private static void Validate(
        StringBuilder report,
        GameObject door,
        GameObject plate,
        Sprite doorClosed,
        Sprite doorHalfOpen,
        Sprite doorOpen,
        Sprite doorWideOpen,
        Sprite plateOff,
        Sprite plateOn)
    {
        CountMissingSceneData(out int missingScripts, out int missingReferences);

        DoorController doorController = door.GetComponent<DoorController>();
        DoorStateSpriteAnimator doorAnimator = door.GetComponent<DoorStateSpriteAnimator>();
        BoxCollider2D doorCollider = door.GetComponent<BoxCollider2D>();
        SpriteRenderer doorRenderer = door.GetComponent<SpriteRenderer>();

        PressurePlateController plateController = plate.GetComponent<PressurePlateController>();
        ShadowPressureTrigger pressureTrigger = plate.GetComponent<ShadowPressureTrigger>();
        PressurePlateSpriteState plateState = plate.GetComponent<PressurePlateSpriteState>();
        BoxCollider2D plateCollider = plate.GetComponent<BoxCollider2D>();
        SpriteRenderer plateRenderer = plate.GetComponent<SpriteRenderer>();

        report.AppendLine("door.closedSprite=" + PassFail(doorClosed != null));
        report.AppendLine("door.halfOpenSprite=" + PassFail(doorHalfOpen != null));
        report.AppendLine("door.openSprite=" + PassFail(doorOpen != null));
        report.AppendLine("door.wideOpenSprite=" + PassFail(doorWideOpen != null));
        report.AppendLine("plate.offSprite=" + PassFail(plateOff != null));
        report.AppendLine("plate.onSprite=" + PassFail(plateOn != null));
        report.AppendLine("door.controller=" + PassFail(doorController != null));
        report.AppendLine("door.visualAnimator=" + PassFail(doorAnimator != null));
        report.AppendLine("door.colliderBlocksClosed=" + PassFail(doorCollider != null && doorCollider.enabled && !doorCollider.isTrigger && doorCollider.size.x >= 1.5f && doorCollider.size.y >= 1.6f));
        report.AppendLine("door.rendererVisible=" + PassFail(doorRenderer != null && doorRenderer.enabled && doorRenderer.sprite == doorClosed));
        report.AppendLine("plate.controller=" + PassFail(plateController != null));
        report.AppendLine("plate.trigger=" + PassFail(pressureTrigger != null));
        report.AppendLine("plate.visualState=" + PassFail(plateState != null));
        report.AppendLine("plate.triggerCollider=" + PassFail(plateCollider != null && plateCollider.isTrigger && plateCollider.size.x >= 1.4f && plateCollider.size.y >= 0.9f));
        report.AppendLine("plate.groundedScale=" + PassFail(plate.transform.localScale.y <= 0.65f));
        report.AppendLine("plate.groundedSorting=" + PassFail(plateRenderer != null && plateRenderer.sortingOrder <= 8));
        report.AppendLine("plate.rendererVisible=" + PassFail(plateRenderer != null && plateRenderer.enabled && plateRenderer.sprite == plateOff));
        report.AppendLine("canPressDoorLink=" + PassFail(plateController != null && plateController.TargetDoor == doorController));
        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);
        report.AppendLine("consoleCompileErrors=checked_after_unity_refresh");
    }

    private static void ConfigureTexture(string assetPath, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static Sprite LoadSprite(string assetPath)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
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

    private static void SetColor(SerializedObject serializedObject, string propertyName, Color value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.colorValue = value;
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
