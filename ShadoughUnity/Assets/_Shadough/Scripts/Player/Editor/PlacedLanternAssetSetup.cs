using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlacedLanternAssetSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string SpritePath = "Assets/_Shadough/Sprites/Props/Lantern/placed_lantern_topdown.png";
    private const string PrefabPath = "Assets/_Shadough/Prefabs/Interactables/PlacedLantern_Topdown.prefab";
    private const string AutoSetupRequestPath = "Temp/PlacedLanternSetup.request";
    private static readonly Vector3 LightLocalPosition = new Vector3(0.11f, 0.58f, 0f);
    private static readonly Vector2 HeldLanternOffsetUp = new Vector2(0.48f, -0.42f);
    private static readonly Vector2 HeldLanternOffsetDown = new Vector2(-0.62f, -0.4f);
    private static readonly Vector2 HeldLanternOffsetLeft = new Vector2(-0.58f, -0.4f);
    private static readonly Vector2 HeldLanternOffsetRight = new Vector2(0.18f, -0.4f);
    private static readonly Vector2 PlacedLanternOffsetUp = new Vector2(0.58f, -0.36f);
    private static readonly Vector2 PlacedLanternOffsetDown = new Vector2(-0.52f, -0.36f);
    private static readonly Vector2 PlacedLanternOffsetLeft = new Vector2(-0.48f, -0.36f);
    private static readonly Vector2 PlacedLanternOffsetRight = new Vector2(0.34f, -0.36f);
    private const float PickupDistance = 0.8f;
    private const float PlaceAnimationDelay = 0.38f;
    private const float LanternTransitionLockDuration = 0.45f;

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Placed Lantern Asset")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    private static void TryAutoSetup()
    {
        string requestPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..", AutoSetupRequestPath));
        if (!System.IO.File.Exists(requestPath))
        {
            EditorApplication.update -= TryAutoSetup;
            return;
        }

        if (EditorApplication.isCompiling)
        {
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        EditorApplication.update -= TryAutoSetup;

        try
        {
            Setup();
            System.IO.File.Delete(requestPath);
            Debug.Log("Placed lantern auto setup complete.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public static void Setup()
    {
        ConfigureSpriteImport();
        Sprite lanternSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        if (lanternSprite == null)
        {
            Debug.LogWarning("Placed lantern sprite not found: " + SpritePath);
            return;
        }

        EnsurePrefab(lanternSprite);
        ConfigureSceneLantern(lanternSprite);
        ValidateSceneAndPrefab();
        Debug.Log("Placed lantern setup complete.");
    }

    private static void ConfigureSpriteImport()
    {
        TextureImporter importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("TextureImporter not found for placed lantern sprite: " + SpritePath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 160f;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        settings.spritePivot = new Vector2(0.5f, 0f);
        importer.SetTextureSettings(settings);
        SetUncompressedPlatform(importer, "DefaultTexturePlatform", false);
        SetUncompressedPlatform(importer, "Standalone", true);
        importer.SaveAndReimport();
    }

    private static void SetUncompressedPlatform(TextureImporter importer, string platformName, bool overridden)
    {
        TextureImporterPlatformSettings platformSettings = importer.GetPlatformTextureSettings(platformName);
        platformSettings.name = platformName;
        platformSettings.overridden = overridden;
        platformSettings.maxTextureSize = 2048;
        platformSettings.format = TextureImporterFormat.RGBA32;
        platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
        platformSettings.crunchedCompression = false;
        importer.SetPlatformTextureSettings(platformSettings);
    }

    private static void EnsurePrefab(Sprite lanternSprite)
    {
        GameObject prefabRoot = new GameObject("PlacedLantern_Topdown");
        ConfigureLanternObject(prefabRoot, lanternSprite, true);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        UnityEngine.Object.DestroyImmediate(prefabRoot);
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureSceneLantern(Sprite lanternSprite)
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject player = GameObject.Find("Player_Topdown");
        if (player == null)
        {
            Debug.LogWarning("Player_Topdown not found. Placed lantern scene binding skipped.");
            return;
        }

        PlayerLanternController lanternController = player.GetComponent<PlayerLanternController>();
        if (lanternController == null)
        {
            Debug.LogWarning("Player_Topdown has no PlayerLanternController. Placed lantern scene binding skipped.");
            return;
        }

        Transform heldLanternPoint = player.transform.Find("HeldLanternPoint");
        if (heldLanternPoint == null)
        {
            GameObject heldPointObject = new GameObject("HeldLanternPoint");
            heldPointObject.transform.SetParent(player.transform, false);
            heldPointObject.transform.localPosition = HeldLanternOffsetDown;
            heldLanternPoint = heldPointObject.transform;
        }
        else
        {
            heldLanternPoint.localPosition = HeldLanternOffsetDown;
        }

        GameObject lanternObject = GameObject.Find("Topdown_PlayerLantern");
        if (lanternObject == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            lanternObject = prefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
                : new GameObject("Topdown_PlayerLantern");
            lanternObject.name = "Topdown_PlayerLantern";
        }

        ConfigureLanternObject(lanternObject, lanternSprite, true);
        lanternObject.transform.SetParent(heldLanternPoint, false);
        lanternObject.transform.localPosition = Vector3.zero;
        lanternObject.transform.localRotation = Quaternion.identity;
        lanternObject.transform.localScale = Vector3.one;

        SpriteRenderer renderer = lanternObject.GetComponent<SpriteRenderer>();
        Collider2D collider = lanternObject.GetComponent<Collider2D>();
        Transform lightPoint = EnsureLightPoint(lanternObject);
        SetLightEnabled(lightPoint.gameObject, true);

        if (renderer != null)
        {
            renderer.enabled = false;
        }

        if (collider != null)
        {
            collider.enabled = false;
        }

        SerializedObject serializedController = new SerializedObject(lanternController);
        serializedController.FindProperty("isLanternPlanted").boolValue = false;
        serializedController.FindProperty("heldLanternPoint").objectReferenceValue = heldLanternPoint;
        serializedController.FindProperty("lanternObject").objectReferenceValue = lanternObject;
        serializedController.FindProperty("lightPoint").objectReferenceValue = lightPoint;
        serializedController.FindProperty("retrieveRange").floatValue = PickupDistance;
        serializedController.FindProperty("placeAnimationDelay").floatValue = PlaceAnimationDelay;
        serializedController.FindProperty("heldLanternOffsetUp").vector2Value = HeldLanternOffsetUp;
        serializedController.FindProperty("heldLanternOffsetDown").vector2Value = HeldLanternOffsetDown;
        serializedController.FindProperty("heldLanternOffsetLeft").vector2Value = HeldLanternOffsetLeft;
        serializedController.FindProperty("heldLanternOffsetRight").vector2Value = HeldLanternOffsetRight;
        serializedController.FindProperty("placedLanternOffsetUp").vector2Value = PlacedLanternOffsetUp;
        serializedController.FindProperty("placedLanternOffsetDown").vector2Value = PlacedLanternOffsetDown;
        serializedController.FindProperty("placedLanternOffsetLeft").vector2Value = PlacedLanternOffsetLeft;
        serializedController.FindProperty("placedLanternOffsetRight").vector2Value = PlacedLanternOffsetRight;
        serializedController.FindProperty("moveThreshold").floatValue = 0.01f;
        serializedController.FindProperty("lanternRenderer").objectReferenceValue = renderer;
        serializedController.FindProperty("lanternCollider").objectReferenceValue = collider;
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        HeroPlayerAnimatorDriver animatorDriver = player.GetComponent<HeroPlayerAnimatorDriver>();
        if (animatorDriver != null)
        {
            SerializedObject serializedDriver = new SerializedObject(animatorDriver);
            serializedDriver.FindProperty("lanternTransitionLockDuration").floatValue = LanternTransitionLockDuration;
            serializedDriver.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
    }

    private static void ConfigureLanternObject(GameObject lanternObject, Sprite lanternSprite, bool lightEnabled)
    {
        SpriteRenderer renderer = lanternObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = lanternObject.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = lanternSprite;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.sortingOrder = 18;
        renderer.color = new Color(1f, 0.62f, 0.22f, 1f);

        BoxCollider2D collider = lanternObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = lanternObject.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
        collider.size = new Vector2(0.45f, 1f);
        collider.offset = new Vector2(0f, 0.5f);

        Transform lightPoint = EnsureLightPoint(lanternObject);
        SetLightEnabled(lightPoint.gameObject, lightEnabled);
    }

    private static Transform EnsureLightPoint(GameObject lanternObject)
    {
        Transform lightPoint = lanternObject.transform.Find("LanternLightPoint");
        if (lightPoint == null)
        {
            GameObject lightPointObject = new GameObject("LanternLightPoint");
            lightPointObject.transform.SetParent(lanternObject.transform, false);
            lightPoint = lightPointObject.transform;
        }

        lightPoint.localPosition = LightLocalPosition;
        lightPoint.localRotation = Quaternion.identity;
        lightPoint.localScale = Vector3.one;

        Component lightComponent = EnsureLight2D(lightPoint.gameObject);
        ConfigureLight2D(lightComponent);
        return lightPoint;
    }

    private static Component EnsureLight2D(GameObject target)
    {
        Type light2DType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (light2DType == null)
        {
            return null;
        }

        Component lightComponent = target.GetComponent(light2DType);
        if (lightComponent == null)
        {
            lightComponent = target.AddComponent(light2DType);
        }

        return lightComponent;
    }

    private static void ConfigureLight2D(Component lightComponent)
    {
        SetLightProperty(lightComponent, "lightType", 3);
        SetLightProperty(lightComponent, "color", new Color(1f, 0.78f, 0.42f, 1f));
        SetLightProperty(lightComponent, "intensity", 1.35f);
        SetLightProperty(lightComponent, "pointLightInnerRadius", 0.2f);
        SetLightProperty(lightComponent, "pointLightOuterRadius", 5f);
        SetLightProperty(lightComponent, "shadowsEnabled", true);
        SetLightProperty(lightComponent, "shadowIntensity", 0.85f);
    }

    private static void SetLightProperty(Component component, string propertyName, object value)
    {
        if (component == null)
        {
            return;
        }

        PropertyInfo property = component.GetType().GetProperty(propertyName);
        if (property == null || !property.CanWrite)
        {
            return;
        }

        object convertedValue = value;
        if (property.PropertyType.IsEnum && value is int intValue)
        {
            convertedValue = Enum.ToObject(property.PropertyType, intValue);
        }

        property.SetValue(component, convertedValue, null);
    }

    private static void SetLightEnabled(GameObject lightObject, bool enabled)
    {
        Type light2DType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (light2DType == null || lightObject == null)
        {
            return;
        }

        Component lightComponent = lightObject.GetComponent(light2DType);
        if (lightComponent is Behaviour behaviour)
        {
            behaviour.enabled = enabled;
        }
    }

    private static void ValidateSceneAndPrefab()
    {
        int missingScripts = 0;
        int missingReferences = 0;
        int sceneLanternCount = 0;
        int sceneLightCount = 0;
        int prefabRendererCount = 0;
        int prefabColliderCount = 0;
        int prefabLightCount = 0;
        Type light2DType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab != null)
        {
            prefabRendererCount = prefab.GetComponentsInChildren<SpriteRenderer>(true).Length;
            prefabColliderCount = prefab.GetComponentsInChildren<Collider2D>(true).Length;
            prefabLightCount = light2DType != null ? prefab.GetComponentsInChildren(light2DType, true).Length : 0;
        }

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!sceneObject.scene.IsValid() || sceneObject.scene.path != ScenePath)
            {
                continue;
            }

            if (sceneObject.name == "Topdown_PlayerLantern")
            {
                sceneLanternCount++;
            }

            if (light2DType != null && sceneObject.GetComponent(light2DType) != null)
            {
                sceneLightCount++;
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

        Debug.Log("Placed lantern validation: missingScripts=" + missingScripts
            + ", missingReferences=" + missingReferences
            + ", sceneLanternCount=" + sceneLanternCount
            + ", sceneLightCount=" + sceneLightCount
            + ", prefabRendererCount=" + prefabRendererCount
            + ", prefabColliderCount=" + prefabColliderCount
            + ", prefabLightCount=" + prefabLightCount);
    }
}
