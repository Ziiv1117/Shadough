using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownGameplayBackgroundSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string BackgroundAssetPath = "Assets/_Shadough/Art/Backgrounds/Topdown_Gameplay_Background.jpg";
    private const string MapCutoutAssetPath = "Assets/_Shadough/Sprites/Maps/Level01/level01_full_map_cutout.png";
    private const string BackgroundObjectName = "Topdown_Gameplay_Background";
    private const string ReportPath = "Logs/TopdownGameplayBackgroundSetup.report.txt";
    private const string ScreenshotPath = "Exports/Screenshots/Topdown_GlobalScene_Background.png";
    private const int BackgroundSortingOrder = -120;
    private const float PixelsPerUnit = 100f;
    private const int ScreenshotWidth = 1920;
    private const int ScreenshotHeight = 1080;

    [MenuItem("Shadough/Setup Topdown Gameplay Background")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    public static void Setup()
    {
        SetupInternal(true);
    }

    public static void SetupSceneOnly()
    {
        SetupInternal(false);
    }

    public static void ExportScreenshotOnly()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        List<string> report = new List<string>
        {
            "Topdown Gameplay Background Screenshot",
            "Scene: " + ScenePath
        };

        Bounds mapBounds = ResolveMapBounds();
        string screenshotFullPath = ExportGlobalScreenshot(mapBounds, report);
        report.Add("screenshot=" + screenshotFullPath);

        File.WriteAllLines(FullPath(ReportPath), report);
        Debug.Log("Topdown gameplay background screenshot complete. Report: " + FullPath(ReportPath));
    }

    private static void SetupInternal(bool exportScreenshot)
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        List<string> report = new List<string>
        {
            "Topdown Gameplay Background Setup",
            "Scene: " + ScenePath
        };

        string sourcePath = ResolveSourcePath();
        CopyAndImportBackground(sourcePath, report);
        ImportMapCutout(report);

        Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundAssetPath);
        if (backgroundSprite == null)
        {
            throw new MissingReferenceException("Gameplay background sprite was not imported: " + BackgroundAssetPath);
        }

        Bounds mapBounds = ResolveMapBounds();
        ConfigureBackgroundObject(backgroundSprite, mapBounds, report);
        ConfigureMapReference(report);
        ConfigureMainCamera(report);

        CountMissingSceneData(out int missingScripts, out int missingReferences);
        report.Add("missingScripts=" + missingScripts);
        report.Add("missingReferences=" + missingReferences);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        report.Add("scene.saved=true");

        if (exportScreenshot)
        {
            string screenshotFullPath = ExportGlobalScreenshot(mapBounds, report);
            report.Add("screenshot=" + screenshotFullPath);
        }

        File.WriteAllLines(FullPath(ReportPath), report);
        Debug.Log("Topdown gameplay background setup complete. Report: " + FullPath(ReportPath));
    }

    private static string ResolveSourcePath()
    {
        string sourceBackgroundPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
            "Documents",
            "xwechat_files",
            "wxid_031fhjlaclju22_bfa9",
            "temp",
            "RWTemp",
            "2026-07",
            "04fc19e1c2aa7cdffeccf2e4bc220888",
            "f0d457f6288a801d8482dfee0d85e586.jpg");

        string fallbackSourceBackgroundPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
            "Temp",
            "codex-clipboard-6d978a17-3f6e-4131-9f47-8fd63cd7bd52.png");

        if (File.Exists(sourceBackgroundPath))
        {
            return sourceBackgroundPath;
        }

        if (File.Exists(fallbackSourceBackgroundPath))
        {
            return fallbackSourceBackgroundPath;
        }

        throw new FileNotFoundException("Background source image was not found.", sourceBackgroundPath);
    }

    private static void CopyAndImportBackground(string sourcePath, List<string> report)
    {
        string fullAssetPath = FullPath(BackgroundAssetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullAssetPath));
        File.Copy(sourcePath, fullAssetPath, true);
        AssetDatabase.ImportAsset(BackgroundAssetPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(BackgroundAssetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        report.Add("background.source=" + sourcePath);
        report.Add("background.asset=" + BackgroundAssetPath);
    }

    private static void ImportMapCutout(List<string> report)
    {
        AssetDatabase.ImportAsset(MapCutoutAssetPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(MapCutoutAssetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 40f;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        report.Add("map.cutout.asset=" + MapCutoutAssetPath);
    }

    private static Bounds ResolveMapBounds()
    {
        SpriteRenderer mapRenderer = FindComponent<SpriteRenderer>("Level01_FinalMap_Reference");
        if (mapRenderer != null)
        {
            return mapRenderer.bounds;
        }

        return new Bounds(Vector3.zero, new Vector3(36.2f, 27.15f, 0f));
    }

    private static void ConfigureBackgroundObject(Sprite sprite, Bounds mapBounds, List<string> report)
    {
        GameObject world = FindSceneObject("World");
        if (world == null)
        {
            world = new GameObject("World");
        }

        GameObject background = FindSceneObject(BackgroundObjectName);
        if (background == null)
        {
            background = new GameObject(BackgroundObjectName);
        }

        background.transform.SetParent(world.transform, false);
        background.transform.position = new Vector3(mapBounds.center.x, mapBounds.center.y, 0f);
        background.transform.rotation = Quaternion.identity;

        SpriteRenderer renderer = background.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = background.AddComponent<SpriteRenderer>();
        }

        renderer.enabled = true;
        renderer.sprite = sprite;
        renderer.color = new Color(0.84f, 0.78f, 0.67f, 1f);
        renderer.sortingOrder = BackgroundSortingOrder;
        renderer.drawMode = SpriteDrawMode.Simple;

        Bounds spriteBounds = sprite.bounds;
        float targetWidth = mapBounds.size.x + 4f;
        float targetHeight = mapBounds.size.y + 4f;
        float scale = Mathf.Max(targetWidth / spriteBounds.size.x, targetHeight / spriteBounds.size.y);
        background.transform.localScale = new Vector3(scale, scale, 1f);

        Collider2D[] colliders = background.GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }

        report.Add("background.object=" + BackgroundObjectName);
        report.Add("background.sortingOrder=" + BackgroundSortingOrder);
        report.Add("background.colliderCount=" + background.GetComponents<Collider2D>().Length);
        report.Add("background.scale=" + background.transform.localScale);
        report.Add("map.bounds.center=" + mapBounds.center);
        report.Add("map.bounds.size=" + mapBounds.size);

        EditorUtility.SetDirty(background);
        EditorUtility.SetDirty(renderer);
    }

    private static void ConfigureMapReference(List<string> report)
    {
        SpriteRenderer mapRenderer = FindComponent<SpriteRenderer>("Level01_FinalMap_Reference");
        if (mapRenderer == null)
        {
            report.Add("map.reference=missing");
            return;
        }

        Sprite cutoutSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MapCutoutAssetPath);
        if (cutoutSprite == null)
        {
            report.Add("map.cutout.sprite=missing");
            return;
        }

        mapRenderer.sprite = cutoutSprite;
        mapRenderer.color = new Color(1f, 1f, 1f, 0.82f);
        mapRenderer.sortingOrder = -100;
        EditorUtility.SetDirty(mapRenderer);

        report.Add("map.reference.sprite=" + MapCutoutAssetPath);
        report.Add("map.reference.alpha=" + mapRenderer.color.a.ToString("0.00"));
        report.Add("map.reference.sortingOrder=" + mapRenderer.sortingOrder);
    }

    private static void ConfigureMainCamera(List<string> report)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = FindSceneObject("Main Camera");
            camera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;
        }

        if (camera == null)
        {
            report.Add("mainCamera=missing");
            return;
        }

        camera.backgroundColor = new Color(0.08f, 0.07f, 0.055f, 1f);
        EditorUtility.SetDirty(camera);
        report.Add("mainCamera.backgroundColor=" + camera.backgroundColor);
    }

    private static string ExportGlobalScreenshot(Bounds mapBounds, List<string> report)
    {
        string fullScreenshotPath = FullPath(ScreenshotPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullScreenshotPath));

        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        bool[] canvasStates = new bool[canvases.Length];
        for (int i = 0; i < canvases.Length; i++)
        {
            canvasStates[i] = canvases[i].enabled;
            canvases[i].enabled = false;
        }

        GameObject cameraObject = new GameObject("Topdown_GlobalScene_ScreenshotCamera");
        RenderTexture renderTexture = null;
        Texture2D screenshot = null;
        RenderTexture previousActive = RenderTexture.active;

        try
        {
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.07f, 0.055f, 1f);
            camera.cullingMask = ~0;
            camera.transform.position = new Vector3(mapBounds.center.x, mapBounds.center.y, -10f);
            camera.transform.rotation = Quaternion.identity;

            float aspect = (float)ScreenshotWidth / ScreenshotHeight;
            camera.orthographicSize = Mathf.Max((mapBounds.size.y + 3f) * 0.5f, (mapBounds.size.x + 3f) / (2f * aspect));
            Vector3 screenshotCameraPosition = camera.transform.position;
            float screenshotCameraSize = camera.orthographicSize;

            renderTexture = new RenderTexture(ScreenshotWidth, ScreenshotHeight, 24, RenderTextureFormat.ARGB32);
            screenshot = new Texture2D(ScreenshotWidth, ScreenshotHeight, TextureFormat.RGBA32, false);

            camera.targetTexture = renderTexture;
            RenderTexture.active = renderTexture;
            camera.Render();
            screenshot.ReadPixels(new Rect(0, 0, ScreenshotWidth, ScreenshotHeight), 0, 0);
            screenshot.Apply();
            File.WriteAllBytes(fullScreenshotPath, screenshot.EncodeToPNG());

            report.Add("screenshot.width=" + ScreenshotWidth);
            report.Add("screenshot.height=" + ScreenshotHeight);
            report.Add("screenshot.cameraPosition=" + screenshotCameraPosition);
            report.Add("screenshot.orthographicSize=" + screenshotCameraSize.ToString("0.00"));
            return fullScreenshotPath;
        }
        finally
        {
            RenderTexture.active = previousActive;

            if (renderTexture != null)
            {
                Object.DestroyImmediate(renderTexture);
            }

            if (screenshot != null)
            {
                Object.DestroyImmediate(screenshot);
            }

            Object.DestroyImmediate(cameraObject);

            for (int i = 0; i < canvases.Length; i++)
            {
                canvases[i].enabled = canvasStates[i];
            }
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

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }
}
