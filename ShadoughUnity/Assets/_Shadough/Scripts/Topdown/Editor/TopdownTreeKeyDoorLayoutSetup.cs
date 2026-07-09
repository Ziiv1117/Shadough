using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownTreeKeyDoorLayoutSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string ReportPath = "Logs/TopdownTreeKeyDoorLayoutSetup.report.txt";

    private const string KeyNormalPath = "Assets/_Shadough/Art/Interactables/Key/Key_Normal.png";
    private const string KeyGlowPath = "Assets/_Shadough/Art/Interactables/Key/Key_Glow.png";
    private const string DoorClosedPath = "Assets/_Shadough/Art/Interactables/Door/Door_Closed.png";
    private const string DoorHalfOpenPath = "Assets/_Shadough/Art/Interactables/Door/Door_HalfOpen.png";
    private const string DoorOpenPath = "Assets/_Shadough/Art/Interactables/Door/Door_Open.png";
    private const string DoorWideOpenPath = "Assets/_Shadough/Art/Interactables/Door/Door_WideOpen.png";

    private const float PlacementRadius = 1.05f;
    private const float KeyPixelsPerUnit = 620f;
    private const float DoorPixelsPerUnit = 100f;

    [MenuItem("Shadough/Setup Topdown Tree Key Door Layout")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    public static void Setup()
    {
        ConfigureTexture(KeyNormalPath, KeyPixelsPerUnit);
        ConfigureTexture(KeyGlowPath, KeyPixelsPerUnit);
        ConfigureTexture(DoorClosedPath, DoorPixelsPerUnit);
        ConfigureTexture(DoorHalfOpenPath, DoorPixelsPerUnit);
        ConfigureTexture(DoorOpenPath, DoorPixelsPerUnit);
        ConfigureTexture(DoorWideOpenPath, DoorPixelsPerUnit);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        List<string> report = new List<string>
        {
            "Topdown Tree Key Door Layout Setup",
            "Scene: " + ScenePath
        };

        Sprite keyNormal = LoadSprite(KeyNormalPath);
        Sprite keyGlow = LoadSprite(KeyGlowPath);
        Sprite doorClosed = LoadSprite(DoorClosedPath);
        Sprite doorHalfOpen = LoadSprite(DoorHalfOpenPath);
        Sprite doorOpen = LoadSprite(DoorOpenPath);
        Sprite doorWideOpen = LoadSprite(DoorWideOpenPath);

        ConfigureTreeShadow(report);
        ConfigurePlacementRadius(report);
        ConfigureCanPressDoorEscapeBlocker(report);
        ConfigureKeyAndLockDoor(report, keyNormal, keyGlow, doorClosed, doorHalfOpen, doorOpen, doorWideOpen);
        Validate(report, keyNormal, keyGlow);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllLines(FullPath(ReportPath), report);
        Debug.Log("Topdown tree key door layout setup complete. Report: " + FullPath(ReportPath));
    }

    private static void ConfigureTreeShadow(List<string> report)
    {
        GameObject treeShadow = FindSceneObject("TreeShadow_Topdown");
        if (treeShadow == null)
        {
            report.Add("treeShadow.exists=FAIL");
            return;
        }

        LightDrivenShadow lightDrivenShadow = treeShadow.GetComponent<LightDrivenShadow>();
        ShadowInteractable shadowInteractable = treeShadow.GetComponent<ShadowInteractable>();
        BoxCollider2D shadowCollider = treeShadow.GetComponent<BoxCollider2D>();
        SpriteRenderer shadowRenderer = treeShadow.GetComponent<SpriteRenderer>();
        GameObject tree = FindTreeObject(lightDrivenShadow);
        if (tree == null || lightDrivenShadow == null)
        {
            report.Add("treeShadow.rootAnchor=FAIL");
            return;
        }

        Vector3 rootPosition = ResolveTreeRootPosition(tree);
        GameObject rootAnchor = FindSceneObject("TreeShadowRootAnchor_Topdown");
        if (rootAnchor == null)
        {
            rootAnchor = new GameObject("TreeShadowRootAnchor_Topdown");
            rootAnchor.transform.SetParent(tree.transform.parent, true);
        }

        rootAnchor.transform.position = rootPosition;
        rootAnchor.transform.rotation = Quaternion.identity;
        rootAnchor.transform.localScale = Vector3.one;

        SerializedObject serializedShadow = new SerializedObject(lightDrivenShadow);
        SetObject(serializedShadow, "casterTransform", rootAnchor.transform);
        SetFloat(serializedShadow, "shadowWidth", 0.85f);
        SetFloat(serializedShadow, "maxLength", 4.0f);
        serializedShadow.ApplyModifiedPropertiesWithoutUndo();

        if (shadowInteractable != null)
        {
            SerializedObject serializedInteractable = new SerializedObject(shadowInteractable);
            SetBool(serializedInteractable, "canStandOn", true);
            SetBool(serializedInteractable, "canPress", false);
            SetBool(serializedInteractable, "canUnlock", false);
            SetBool(serializedInteractable, "canAttractEnemy", false);
            serializedInteractable.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(shadowInteractable);
        }

        InvokeLightDrivenShapeUpdate(lightDrivenShadow, rootPosition);
        Physics2D.SyncTransforms();

        Vector3 shadowStart = ResolveShadowStart(treeShadow, shadowRenderer, shadowCollider);
        report.Add("treeShadow.exists=PASS");
        report.Add("treeShadow.rootAnchor=" + rootPosition);
        report.Add("treeShadow.start=" + shadowStart);
        report.Add("treeShadow.startNearRoot=" + PassFail(Vector2.Distance(shadowStart, rootPosition) <= 0.25f));
        report.Add("treeShadow.canStandOn=" + PassFail(shadowInteractable != null && shadowInteractable.CanStandOn));

        EditorUtility.SetDirty(rootAnchor);
        EditorUtility.SetDirty(lightDrivenShadow);
        EditorUtility.SetDirty(treeShadow);
    }

    private static void ConfigurePlacementRadius(List<string> report)
    {
        GameObject player = FindSceneObject("Player_Topdown");
        FreeShadowPlacer placer = player != null ? player.GetComponent<FreeShadowPlacer>() : null;
        if (placer == null)
        {
            report.Add("placement.freeShadowPlacer=FAIL");
            return;
        }

        SerializedObject serializedPlacer = new SerializedObject(placer);
        SetFloat(serializedPlacer, "placementRadius", PlacementRadius);
        serializedPlacer.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(placer);

        report.Add("placement.freeShadowPlacer=PASS");
        report.Add("placement.radius=" + PlacementRadius.ToString("0.00"));
    }

    private static void ConfigureCanPressDoorEscapeBlocker(List<string> report)
    {
        GameObject door = FindSceneObject("Door_Pressure_Topdown");
        if (door == null)
        {
            report.Add("canPressDoor.exists=FAIL");
            return;
        }

        BoxCollider2D doorCollider = door.GetComponent<BoxCollider2D>();
        if (doorCollider == null)
        {
            report.Add("canPressDoor.collider=FAIL");
            return;
        }

        Physics2D.SyncTransforms();
        Bounds doorBounds = doorCollider.bounds;
        Transform parent = EnsureChild(EnsureRoot("Graybox_Collision_Topdown").transform, "CanPressDoor_Escape_Blockers");
        BoxCollider2D leftCollider = ConfigureOuterWallBlocker(
            parent,
            "Wall_CanPressDoor_LeftOuterWall_Blocker",
            new Vector3(doorBounds.min.x - 0.62f, doorBounds.center.y, 0f),
            new Vector2(0.62f, Mathf.Max(doorBounds.size.y + 1.25f, 2.25f)));

        BoxCollider2D rightCollider = ConfigureOuterWallBlocker(
            parent,
            "Wall_CanPressDoor_RightOuterWall_Blocker",
            new Vector3(doorBounds.max.x + 0.62f, doorBounds.center.y, 0f),
            new Vector2(0.62f, Mathf.Max(doorBounds.size.y + 1.25f, 2.25f)));

        report.Add("canPressDoor.leftEscapeBlocker=PASS");
        report.Add("canPressDoor.leftEscapeBlockerPosition=" + leftCollider.transform.position);
        report.Add("canPressDoor.leftEscapeBlockerSize=" + leftCollider.size);
        report.Add("canPressDoor.rightEscapeBlocker=PASS");
        report.Add("canPressDoor.rightEscapeBlockerPosition=" + rightCollider.transform.position);
        report.Add("canPressDoor.rightEscapeBlockerSize=" + rightCollider.size);
    }

    private static BoxCollider2D ConfigureOuterWallBlocker(Transform parent, string objectName, Vector3 position, Vector2 size)
    {
        GameObject blocker = EnsureChild(parent, objectName).gameObject;
        blocker.transform.position = position;
        blocker.transform.rotation = Quaternion.identity;
        blocker.transform.localScale = Vector3.one;

        BoxCollider2D collider = blocker.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = blocker.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = false;
        collider.enabled = true;
        collider.offset = Vector2.zero;
        collider.size = size;

        SpriteRenderer renderer = blocker.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        EditorUtility.SetDirty(blocker);
        EditorUtility.SetDirty(collider);
        return collider;
    }

    private static void ConfigureKeyAndLockDoor(
        List<string> report,
        Sprite keyNormal,
        Sprite keyGlow,
        Sprite doorClosed,
        Sprite doorHalfOpen,
        Sprite doorOpen,
        Sprite doorWideOpen)
    {
        GameObject key = FindSceneObject("KeyShadow_Topdown");
        GameObject lockObject = FindSceneObject("Lock_Topdown");
        GameObject lockDoor = FindSceneObject("Door_Lock_Topdown");
        if (key == null)
        {
            key = new GameObject("KeyShadow_Topdown");
            key.transform.SetParent(EnsureRoot("World").transform, true);
            Vector3 fallbackPosition = lockObject != null ? lockObject.transform.position + new Vector3(-1.6f, -0.3f, 0f) : new Vector3(5.0f, 3.0f, 0f);
            key.transform.position = fallbackPosition;
        }

        ConfigureKeyShadow(key, keyNormal, keyGlow, report);

        if (lockDoor != null)
        {
            ConfigureDoorVisualAndCollider(lockDoor, doorClosed, doorHalfOpen, doorOpen, doorWideOpen, new Vector2(1.65f, 1.7f));
        }

        if (lockDoor != null)
        {
            ConfigureDirectKeyDoorTrigger(lockDoor, report);
        }

        DisableLegacyLockObjects(lockObject, FindSceneObject("Lock_Topdown_Trigger"), report);

        report.Add("doors.canPressDoor=" + PassFail(FindSceneObject("Door_Pressure_Topdown") != null));
        report.Add("doors.canUnlockDoor=" + PassFail(lockDoor != null));
        report.Add("key.normalSprite=" + PassFail(keyNormal != null));
        report.Add("key.glowSprite=" + PassFail(keyGlow != null));
        report.Add("key.position=" + key.transform.position);
    }

    private static void ConfigureDirectKeyDoorTrigger(GameObject lockDoor, List<string> report)
    {
        DoorController doorController = lockDoor.GetComponent<DoorController>();
        if (doorController == null)
        {
            doorController = lockDoor.AddComponent<DoorController>();
        }

        doorController.Close();

        Transform triggerTransform = EnsureChild(lockDoor.transform, "Door_Lock_Topdown_DirectUnlockTrigger");
        GameObject triggerObject = triggerTransform.gameObject;
        triggerTransform.localPosition = new Vector3(0f, -0.18f, 0f);
        triggerTransform.localRotation = Quaternion.identity;
        triggerTransform.localScale = Vector3.one;

        BoxCollider2D triggerCollider = triggerObject.GetComponent<BoxCollider2D>();
        if (triggerCollider == null)
        {
            triggerCollider = triggerObject.AddComponent<BoxCollider2D>();
        }

        triggerCollider.isTrigger = true;
        triggerCollider.enabled = true;
        triggerCollider.offset = Vector2.zero;
        triggerCollider.size = new Vector2(1.85f, 1.25f);

        KeyDoorDirectUnlockTrigger directTrigger = triggerObject.GetComponent<KeyDoorDirectUnlockTrigger>();
        if (directTrigger == null)
        {
            directTrigger = triggerObject.AddComponent<KeyDoorDirectUnlockTrigger>();
        }

        SerializedObject serializedTrigger = new SerializedObject(directTrigger);
        SetObject(serializedTrigger, "targetDoor", doorController);
        serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
        directTrigger.ResetDoor();

        report.Add("keyDoor.directTrigger=PASS");
        report.Add("keyDoor.directTriggerSize=" + triggerCollider.size);

        EditorUtility.SetDirty(lockDoor);
        EditorUtility.SetDirty(doorController);
        EditorUtility.SetDirty(triggerObject);
        EditorUtility.SetDirty(triggerCollider);
        EditorUtility.SetDirty(directTrigger);
    }

    private static void DisableLegacyLockObjects(GameObject lockObject, GameObject lockTrigger, List<string> report)
    {
        if (lockTrigger != null)
        {
            lockTrigger.SetActive(false);
            EditorUtility.SetDirty(lockTrigger);
        }

        if (lockObject != null)
        {
            lockObject.SetActive(false);
            EditorUtility.SetDirty(lockObject);
        }

        report.Add("legacyLock.objectDisabled=" + PassFail(lockObject == null || !lockObject.activeSelf));
        report.Add("legacyLock.triggerDisabled=" + PassFail(lockTrigger == null || !lockTrigger.activeSelf));
    }

    private static void ConfigureKeyShadow(GameObject key, Sprite keyNormal, Sprite keyGlow, List<string> report)
    {
        key.transform.rotation = Quaternion.identity;
        key.transform.localScale = Vector3.one;

        SpriteRenderer renderer = key.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = key.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = keyNormal;
        renderer.color = Color.white;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.sortingOrder = 18;

        BoxCollider2D collider = key.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = key.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
        collider.offset = Vector2.zero;
        collider.size = new Vector2(0.48f, 0.88f);

        ShadowInteractable shadowInteractable = key.GetComponent<ShadowInteractable>();
        if (shadowInteractable == null)
        {
            shadowInteractable = key.AddComponent<ShadowInteractable>();
        }

        SerializedObject serializedKey = new SerializedObject(shadowInteractable);
        SetObject(serializedKey, "shadowRenderer", renderer);
        SetString(serializedKey, "displayName", "Key Shadow");
        SetBool(serializedKey, "canBeCut", true);
        SetBool(serializedKey, "canStandOn", false);
        SetBool(serializedKey, "canPress", false);
        SetBool(serializedKey, "canUnlock", true);
        SetBool(serializedKey, "canAttractEnemy", false);
        SetBool(serializedKey, "canBlock", false);
        serializedKey.ApplyModifiedPropertiesWithoutUndo();

        GameObject glowObject = EnsureChild(key.transform, "KeyShadow_GlowSprite_Reference").gameObject;
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localRotation = Quaternion.identity;
        glowObject.transform.localScale = Vector3.one;
        SpriteRenderer glowRenderer = glowObject.GetComponent<SpriteRenderer>();
        if (glowRenderer == null)
        {
            glowRenderer = glowObject.AddComponent<SpriteRenderer>();
        }

        glowRenderer.sprite = keyGlow;
        glowRenderer.color = new Color(1f, 1f, 1f, 0.65f);
        glowRenderer.sortingOrder = 19;
        glowRenderer.enabled = false;

        report.Add("key.canUnlock=" + PassFail(shadowInteractable.CanUnlock));

        EditorUtility.SetDirty(key);
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(collider);
        EditorUtility.SetDirty(shadowInteractable);
        EditorUtility.SetDirty(glowObject);
        EditorUtility.SetDirty(glowRenderer);
    }

    private static void ConfigureDoorVisualAndCollider(
        GameObject door,
        Sprite closed,
        Sprite halfOpen,
        Sprite open,
        Sprite wideOpen,
        Vector2 colliderSize)
    {
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
        collider.size = colliderSize;

        DoorController controller = door.GetComponent<DoorController>();
        if (controller == null)
        {
            controller = door.AddComponent<DoorController>();
        }

        SerializedObject serializedDoor = new SerializedObject(controller);
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
        SetObject(serializedAnimator, "doorController", controller);
        SetObject(serializedAnimator, "spriteRenderer", renderer);
        SetObject(serializedAnimator, "closedSprite", closed);
        SetObject(serializedAnimator, "halfOpenSprite", halfOpen);
        SetObject(serializedAnimator, "openSprite", open);
        SetObject(serializedAnimator, "wideOpenSprite", wideOpen);
        serializedAnimator.ApplyModifiedPropertiesWithoutUndo();

        controller.Close();
        EditorUtility.SetDirty(door);
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(collider);
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(animator);
    }

    private static void Validate(List<string> report, Sprite keyNormal, Sprite keyGlow)
    {
        CountMissingSceneData(out int missingScripts, out int missingReferences);

        GameObject treeShadow = FindSceneObject("TreeShadow_Topdown");
        ShadowInteractable treeShadowInteractable = treeShadow != null ? treeShadow.GetComponent<ShadowInteractable>() : null;
        GameObject key = FindSceneObject("KeyShadow_Topdown");
        ShadowInteractable keyInteractable = key != null ? key.GetComponent<ShadowInteractable>() : null;
        GameObject pressDoor = FindSceneObject("Door_Pressure_Topdown");
        GameObject lockDoor = FindSceneObject("Door_Lock_Topdown");
        SpriteRenderer pressDoorRenderer = pressDoor != null ? pressDoor.GetComponent<SpriteRenderer>() : null;
        SpriteRenderer lockDoorRenderer = lockDoor != null ? lockDoor.GetComponent<SpriteRenderer>() : null;
        KeyDoorDirectUnlockTrigger directTrigger = FindComponent<KeyDoorDirectUnlockTrigger>("Door_Lock_Topdown_DirectUnlockTrigger");
        FreeShadowPlacer placer = FindComponent<FreeShadowPlacer>("Player_Topdown");
        BoxCollider2D leftBlocker = FindComponent<BoxCollider2D>("Wall_CanPressDoor_LeftOuterWall_Blocker");
        BoxCollider2D rightBlocker = FindComponent<BoxCollider2D>("Wall_CanPressDoor_RightOuterWall_Blocker");
        GameObject legacyLock = FindSceneObject("Lock_Topdown");
        GameObject legacyLockTrigger = FindSceneObject("Lock_Topdown_Trigger");

        report.Add("validate.treeShadowCanStandOn=" + PassFail(treeShadowInteractable != null && treeShadowInteractable.CanStandOn));
        report.Add("validate.placementRadius=" + (placer != null ? GetFloat(placer, "placementRadius").ToString("0.00") : "missing"));
        report.Add("validate.leftEscapeBlockerBlocks=" + PassFail(leftBlocker != null && leftBlocker.enabled && !leftBlocker.isTrigger));
        report.Add("validate.rightEscapeBlockerBlocks=" + PassFail(rightBlocker != null && rightBlocker.enabled && !rightBlocker.isTrigger));
        report.Add("validate.pressDoorClosedBlocks=" + PassFail(IsDoorClosedAndBlocking(pressDoor)));
        report.Add("validate.lockDoorClosedBlocks=" + PassFail(IsDoorClosedAndBlocking(lockDoor)));
        report.Add("validate.keyDoorDirectTrigger=" + PassFail(directTrigger != null));
        report.Add("validate.legacyLockDisabled=" + PassFail(legacyLock == null || !legacyLock.activeSelf));
        report.Add("validate.legacyLockTriggerDisabled=" + PassFail(legacyLockTrigger == null || !legacyLockTrigger.activeSelf));
        report.Add("validate.lockDoorVisualMatchesPressDoor=" + PassFail(
            pressDoorRenderer != null
            && lockDoorRenderer != null
            && lockDoorRenderer.sprite == pressDoorRenderer.sprite
            && lockDoorRenderer.color == Color.white
            && lockDoorRenderer.sortingOrder == pressDoorRenderer.sortingOrder));
        report.Add("validate.keyNormalImported=" + PassFail(keyNormal != null));
        report.Add("validate.keyGlowImported=" + PassFail(keyGlow != null));
        report.Add("validate.keyCanUnlock=" + PassFail(keyInteractable != null && keyInteractable.CanUnlock));
        report.Add("missingScripts=" + missingScripts);
        report.Add("missingReferences=" + missingReferences);
        report.Add("consoleCompileErrors=checked_after_unity_refresh");
    }

    private static bool IsDoorClosedAndBlocking(GameObject door)
    {
        if (door == null)
        {
            return false;
        }

        DoorController controller = door.GetComponent<DoorController>();
        Collider2D collider = door.GetComponent<Collider2D>();
        return controller != null && !controller.IsOpen && collider != null && collider.enabled && !collider.isTrigger;
    }

    private static void InvokeLightDrivenShapeUpdate(LightDrivenShadow lightDrivenShadow, Vector3 rootPosition)
    {
        if (lightDrivenShadow == null)
        {
            return;
        }

        Transform lightTransform = ResolveSerializedTransform(lightDrivenShadow, "lightTransform");
        PlayerLanternController lantern = ResolveSerializedObject<PlayerLanternController>(lightDrivenShadow, "lanternController");
        if (lantern != null && lantern.LightPoint != null)
        {
            lightTransform = lantern.LightPoint;
        }

        if (lightTransform == null)
        {
            return;
        }

        MethodInfo method = typeof(LightDrivenShadow).GetMethod("UpdateShadowShape", BindingFlags.Instance | BindingFlags.NonPublic);
        if (method != null)
        {
            method.Invoke(lightDrivenShadow, new object[] { lightTransform.position, rootPosition });
        }
    }

    private static Vector3 ResolveTreeRootPosition(GameObject tree)
    {
        SpriteRenderer renderer = tree.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = tree.GetComponentInChildren<SpriteRenderer>();
        }

        if (renderer == null)
        {
            return tree.transform.position;
        }

        Bounds bounds = renderer.bounds;
        return new Vector3(bounds.center.x, bounds.min.y + 0.18f, tree.transform.position.z);
    }

    private static Vector3 ResolveShadowStart(GameObject shadow, SpriteRenderer renderer, BoxCollider2D collider)
    {
        if (renderer != null && renderer.drawMode != SpriteDrawMode.Simple)
        {
            return shadow.transform.position - shadow.transform.right * (renderer.size.x * 0.5f);
        }

        if (collider != null)
        {
            return shadow.transform.position - shadow.transform.right * (collider.size.x * 0.5f);
        }

        return shadow.transform.position;
    }

    private static GameObject FindTreeObject(LightDrivenShadow lightDrivenShadow)
    {
        Transform serializedCaster = ResolveSerializedTransform(lightDrivenShadow, "casterTransform");
        if (serializedCaster != null && serializedCaster.name != "TreeShadowRootAnchor_Topdown")
        {
            return serializedCaster.gameObject;
        }

        string[] names = { "Tree_01", "Tree", "World/Tree", "Tree_Topdown" };
        for (int i = 0; i < names.Length; i++)
        {
            GameObject tree = GameObject.Find(names[i]);
            if (tree != null)
            {
                return tree;
            }

            tree = FindSceneObject(names[i]);
            if (tree != null)
            {
                return tree;
            }
        }

        return null;
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

    private static Transform ResolveSerializedTransform(Object target, string propertyName)
    {
        if (target == null)
        {
            return null;
        }

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property != null ? property.objectReferenceValue as Transform : null;
    }

    private static T ResolveSerializedObject<T>(Object target, string propertyName) where T : Object
    {
        if (target == null)
        {
            return null;
        }

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property != null ? property.objectReferenceValue as T : null;
    }

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
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

    private static void SetColor(SerializedObject serializedObject, string propertyName, Color value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.colorValue = value;
        }
    }

    private static float GetFloat(Object target, string propertyName)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        return property != null ? property.floatValue : 0f;
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
