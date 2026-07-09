using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownTreeShadowKeyVisualSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string RequestPath = "Temp/TopdownTreeShadowKeyVisualSetup.request";
    private const string ReportPath = "Temp/TopdownTreeShadowKeyVisualSetup.report.txt";
    private const string TreeBasePath = "Assets/_Shadough/Sprites/Shadows/Tree/tree_shadow_base.png";
    private const string TreePastedPath = "Assets/_Shadough/Sprites/Shadows/Tree/tree_shadow_pasted.png";
    private const string KeyNormalPath = "Assets/_Shadough/Art/Interactables/Key/Key_Normal.png";

    private static readonly List<ObjectState> ObjectStates = new List<ObjectState>();
    private static readonly List<ColliderState> ColliderStates = new List<ColliderState>();
    private static int initialSignCount;
    private static int initialBlockCount;
    private static int initialAreaCount;

    [InitializeOnLoadMethod]
    private static void RegisterRequestHandler()
    {
        EditorApplication.update -= TryRunRequestedSetup;
        EditorApplication.update += TryRunRequestedSetup;
    }

    [MenuItem("Shadough/Assets/Setup Tree Shadow And Key Visual")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    private static void TryRunRequestedSetup()
    {
        if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        string request = FullPath(RequestPath);
        if (!File.Exists(request))
        {
            return;
        }

        File.Delete(request);
        Setup();
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown TreeShadow and key entity visual setup");
        report.AppendLine("Scene: " + ScenePath);

        RemoveLegacySignsAndPlaceholderVisuals(report);
        CaptureSceneState();
        ImportTreeShadowAssets(report);
        BindTreeShadowVisuals(report);
        BindKeyEntityVisual(report);
        Validate(report);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("TreeShadow and key entity visual setup complete: " + FullPath(ReportPath));
    }

    private static void RemoveLegacySignsAndPlaceholderVisuals(StringBuilder report)
    {
        int tutorialSignCount = CountNamesContaining("TutorialSign");
        DestroyHierarchy("TutorialSigns_Topdown");

        int placeholderTreeVisualsRemoved = 0;
        placeholderTreeVisualsRemoved += DestroyVisualOnlyObject("Tree_Trunk") ? 1 : 0;
        placeholderTreeVisualsRemoved += DestroyVisualOnlyObject("Tree_Canopy") ? 1 : 0;

        GameObject treeCollision = Require("Tree_01");
        SpriteRenderer treePlaceholderRenderer = treeCollision.GetComponent<SpriteRenderer>();
        if (treePlaceholderRenderer != null)
        {
            Object.DestroyImmediate(treePlaceholderRenderer);
        }

        GameObject crossingController = Require("CrossingHint_01");
        SpriteRenderer crossingPlaceholderRenderer = crossingController.GetComponent<SpriteRenderer>();
        if (crossingPlaceholderRenderer != null)
        {
            crossingPlaceholderRenderer.enabled = false;
        }

        GameObject deprecatedBlockout = Find("Disabled_Deprecated_Blockout");
        int deprecatedObjectCount = deprecatedBlockout != null
            ? deprecatedBlockout.GetComponentsInChildren<Transform>(true).Length
            : 0;
        if (deprecatedBlockout != null)
        {
            if (deprecatedBlockout.activeSelf)
            {
                throw new System.InvalidOperationException("Disabled_Deprecated_Blockout must be inactive before removal.");
            }

            MonoBehaviour[] behaviours = deprecatedBlockout.GetComponentsInChildren<MonoBehaviour>(true);
            if (behaviours.Length != 0)
            {
                throw new System.InvalidOperationException("Disabled_Deprecated_Blockout contains runtime scripts and was not removed.");
            }

            Object.DestroyImmediate(deprecatedBlockout);
        }

        report.AppendLine("cleanup.tutorialSignsRemoved=" + tutorialSignCount);
        report.AppendLine("cleanup.placeholderTreeVisualsRemoved=" + placeholderTreeVisualsRemoved);
        report.AppendLine("cleanup.treeCollisionPreserved=" + PassFail(treeCollision.GetComponent<Collider2D>() != null));
        report.AppendLine("cleanup.crossingControllerPreserved=" + PassFail(crossingController.GetComponent<MonoBehaviour>() != null));
        report.AppendLine("cleanup.deprecatedObjectsRemoved=" + deprecatedObjectCount);
    }

    private static bool DestroyVisualOnlyObject(string objectName)
    {
        GameObject gameObject = Find(objectName);
        if (gameObject == null)
        {
            return false;
        }

        Component[] components = gameObject.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            if (!(components[i] is Transform) && !(components[i] is SpriteRenderer))
            {
                throw new System.InvalidOperationException(objectName + " is not a visual-only placeholder.");
            }
        }

        Object.DestroyImmediate(gameObject);
        return true;
    }

    private static void DestroyHierarchy(string objectName)
    {
        GameObject gameObject = Find(objectName);
        if (gameObject != null)
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    private static void CaptureSceneState()
    {
        ObjectStates.Clear();
        ColliderStates.Clear();

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (!InTargetScene(objects[i]))
            {
                continue;
            }

            ObjectStates.Add(new ObjectState(objects[i]));
            Collider2D[] colliders = objects[i].GetComponents<Collider2D>();
            for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
            {
                ColliderStates.Add(new ColliderState(colliders[colliderIndex]));
            }
        }

        initialSignCount = CountSigns();
        initialBlockCount = CountBlocks();
        initialAreaCount = CountAreas();
    }

    private static void ImportTreeShadowAssets(StringBuilder report)
    {
        string workspaceRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
        string sourceFolder = Path.Combine(workspaceRoot, "素材库", "03_影子系统");
        CopyAsset(Path.Combine(sourceFolder, "tree_shadow_base.png"), TreeBasePath, report);
        CopyAsset(Path.Combine(sourceFolder, "tree_shadow_pasted.png"), TreePastedPath, report);
        ConfigureTreeSprite(TreeBasePath);
        ConfigureTreeSprite(TreePastedPath);
    }

    private static void BindTreeShadowVisuals(StringBuilder report)
    {
        GameObject treeShadow = Require("TreeShadow_Topdown");
        SpriteRenderer renderer = treeShadow.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            throw new MissingComponentException("TreeShadow_Topdown has no SpriteRenderer.");
        }

        Sprite baseSprite = LoadSprite(TreeBasePath);
        Sprite pastedSprite = LoadSprite(TreePastedPath);
        renderer.sprite = baseSprite;
        EditorUtility.SetDirty(renderer);

        GameObject visualRoot = Require("ShadowVisuals");
        PastedShadowVisualResolver resolver = visualRoot.GetComponent<PastedShadowVisualResolver>();
        if (resolver == null)
        {
            resolver = visualRoot.AddComponent<PastedShadowVisualResolver>();
        }

        SerializedObject serialized = new SerializedObject(resolver);
        SetObject(serialized, "treePastedSprite", pastedSprite);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        report.AppendLine("tree.baseSprite=" + PassFail(renderer.sprite == baseSprite));
        report.AppendLine("tree.pastedSpriteBound=" + PassFail(GetObjectReference(resolver, "treePastedSprite") == pastedSprite));
        report.AppendLine("tree.lightDriven=" + PassFail(treeShadow.GetComponent<LightDrivenShadow>() != null));
        report.AppendLine("tree.shadowInteractable=" + PassFail(treeShadow.GetComponent<ShadowInteractable>() != null));
    }

    private static void BindKeyEntityVisual(StringBuilder report)
    {
        GameObject keySource = Require("KeySource_Topdown");
        SpriteRenderer renderer = keySource.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            throw new MissingComponentException("KeySource_Topdown has no SpriteRenderer.");
        }

        Sprite keySprite = LoadSprite(KeyNormalPath);
        renderer.sprite = keySprite;
        renderer.color = Color.white;
        renderer.enabled = true;
        EditorUtility.SetDirty(renderer);

        ShadowInteractable keyShadow = FindComponent<ShadowInteractable>("KeyShadow_Topdown");
        report.AppendLine("key.entitySprite=" + PassFail(renderer.sprite == keySprite && renderer.enabled));
        report.AppendLine("key.entityCount=" + CountExactName("KeySource_Topdown"));
        report.AppendLine("key.shadowSeparate=" + PassFail(keyShadow != null && keyShadow.gameObject != keySource));
        report.AppendLine("key.shadowCanUnlock=" + PassFail(keyShadow != null && keyShadow.CanUnlock));
    }

    private static void Validate(StringBuilder report)
    {
        CountMissingSceneData(out int missingScripts, out int missingReferences);
        SpriteRenderer crossingRenderer = FindComponent<SpriteRenderer>("CrossingHint_01");
        GameObject treeCollision = Find("Tree_01");

        report.AppendLine("layout.transformsUnchanged=" + PassFail(AllObjectsUnchanged()));
        report.AppendLine("layout.collidersUnchanged=" + PassFail(AllCollidersUnchanged()));
        report.AppendLine("layout.colliderCount=" + ColliderStates.Count);
        report.AppendLine("layout.manualWalls=" + PassFail(Find("Level01_ManualWalls") != null));
        report.AppendLine("layout.mapPresent=" + PassFail(Find("Level01_FinalMap_Reference") != null));
        report.AppendLine("layout.playerPresent=" + PassFail(Find("Player_Topdown") != null));
        report.AppendLine("layout.treeShadowPresent=" + PassFail(Find("TreeShadow_Topdown") != null));
        report.AppendLine("layout.keySourcePresent=" + PassFail(Find("KeySource_Topdown") != null));
        report.AppendLine("tutorialSignsRemaining=" + CountSigns());
        report.AppendLine("placeholderTreeVisualsRemaining=" + CountPlaceholderTreeVisuals());
        report.AppendLine("disabledDeprecatedBlockoutRemaining=" + CountExactName("Disabled_Deprecated_Blockout"));
        report.AppendLine("treeCollisionRendererRemoved=" + PassFail(treeCollision != null && treeCollision.GetComponent<SpriteRenderer>() == null));
        report.AppendLine("crossingPlaceholderHidden=" + PassFail(crossingRenderer == null || !crossingRenderer.enabled));
        report.AppendLine("newSignsAdded=" + (CountSigns() - initialSignCount));
        report.AppendLine("newBlocksAdded=" + (CountBlocks() - initialBlockCount));
        report.AppendLine("newAreasAdded=" + (CountAreas() - initialAreaCount));
        report.AppendLine("treeShadowCount=" + CountExactName("TreeShadow_Topdown"));
        report.AppendLine("keySourceCount=" + CountExactName("KeySource_Topdown"));
        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);

        SpriteRenderer outline = FindComponent<SpriteRenderer>("Level01_OutlineOverlay_Reference");
        report.AppendLine("outlineHidden=" + PassFail(outline != null && !outline.enabled));
    }

    private static void CopyAsset(string source, string targetAssetPath, StringBuilder report)
    {
        if (!File.Exists(source))
        {
            throw new FileNotFoundException("Specified source asset is missing.", source);
        }

        string target = FullPath(targetAssetPath);
        Directory.CreateDirectory(Path.GetDirectoryName(target));
        bool copied = !File.Exists(target) || !SameBytes(source, target);
        if (copied)
        {
            File.Copy(source, target, true);
        }

        AssetDatabase.ImportAsset(targetAssetPath, ImportAssetOptions.ForceUpdate);
        report.AppendLine("import." + Path.GetFileName(source) + "=" + (copied ? "COPIED" : "CURRENT"));
    }

    private static void ConfigureTreeSprite(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            throw new MissingReferenceException("TextureImporter missing: " + assetPath);
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 160f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = new Vector2(0.5f, 0.5f);
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static Object GetObjectReference(Object target, string propertyName)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        return property != null ? property.objectReferenceValue : null;
    }

    private static void CountMissingSceneData(out int missingScripts, out int missingReferences)
    {
        missingScripts = 0;
        missingReferences = 0;

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (!InTargetScene(objects[i]))
            {
                continue;
            }

            Component[] components = objects[i].GetComponents<Component>();
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                Component component = components[componentIndex];
                if (component == null)
                {
                    missingScripts++;
                    continue;
                }

                SerializedObject serialized = new SerializedObject(component);
                SerializedProperty property = serialized.GetIterator();
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

    private static bool AllObjectsUnchanged()
    {
        for (int i = 0; i < ObjectStates.Count; i++)
        {
            if (!ObjectStates[i].Matches())
            {
                return false;
            }
        }

        return true;
    }

    private static bool AllCollidersUnchanged()
    {
        for (int i = 0; i < ColliderStates.Count; i++)
        {
            if (!ColliderStates[i].Matches())
            {
                return false;
            }
        }

        return true;
    }

    private static int CountSigns()
    {
        return CountNamesContaining("TutorialSign");
    }

    private static int CountBlocks()
    {
        return CountNamesContaining("Blockout") + CountNamesContaining("Graybox_Template");
    }

    private static int CountAreas()
    {
        return CountNamesContaining("PasteArea") + CountNamesContaining("LureArea") + CountNamesContaining("EnemyPassage");
    }

    private static int CountPlaceholderTreeVisuals()
    {
        int count = 0;
        if (Find("Tree_Trunk") != null)
        {
            count++;
        }
        if (Find("Tree_Canopy") != null)
        {
            count++;
        }
        return count;
    }

    private static int CountNamesContaining(string token)
    {
        int count = 0;
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (InTargetScene(objects[i])
                && objects[i].name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                count++;
            }
        }
        return count;
    }

    private static int CountExactName(string objectName)
    {
        int count = 0;
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (InTargetScene(objects[i]) && objects[i].name == objectName)
            {
                count++;
            }
        }
        return count;
    }

    private static bool SameBytes(string first, string second)
    {
        byte[] a = File.ReadAllBytes(first);
        byte[] b = File.ReadAllBytes(second);
        if (a.Length != b.Length)
        {
            return false;
        }

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }

        return true;
    }

    private static Sprite LoadSprite(string assetPath)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
        {
            throw new MissingReferenceException("Sprite missing: " + assetPath);
        }

        return sprite;
    }

    private static GameObject Require(string objectName)
    {
        GameObject gameObject = Find(objectName);
        if (gameObject == null)
        {
            throw new MissingReferenceException("Scene object missing: " + objectName);
        }

        return gameObject;
    }

    private static GameObject Find(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (InTargetScene(objects[i]) && objects[i].name == objectName)
            {
                return objects[i];
            }
        }

        return null;
    }

    private static T FindComponent<T>(string objectName) where T : Component
    {
        GameObject gameObject = Find(objectName);
        return gameObject != null ? gameObject.GetComponent<T>() : null;
    }

    private static bool InTargetScene(GameObject gameObject)
    {
        return gameObject != null && gameObject.scene.IsValid() && gameObject.scene.path == ScenePath;
    }

    private static void SetObject(SerializedObject serialized, string propertyName, Object value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            throw new System.MissingFieldException(serialized.targetObject.GetType().Name, propertyName);
        }

        property.objectReferenceValue = value;
    }

    private static string FullPath(string path)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
    }

    private static string PassFail(bool value)
    {
        return value ? "PASS" : "FAIL";
    }

    private sealed class ObjectState
    {
        private readonly GameObject gameObject;
        private readonly Transform parent;
        private readonly int siblingIndex;
        private readonly Vector3 localPosition;
        private readonly Quaternion localRotation;
        private readonly Vector3 localScale;
        private readonly bool activeSelf;

        public ObjectState(GameObject source)
        {
            gameObject = source;
            parent = source.transform.parent;
            siblingIndex = source.transform.GetSiblingIndex();
            localPosition = source.transform.localPosition;
            localRotation = source.transform.localRotation;
            localScale = source.transform.localScale;
            activeSelf = source.activeSelf;
        }

        public bool Matches()
        {
            return gameObject != null
                && gameObject.transform.parent == parent
                && gameObject.transform.GetSiblingIndex() == siblingIndex
                && gameObject.transform.localPosition == localPosition
                && gameObject.transform.localRotation == localRotation
                && gameObject.transform.localScale == localScale
                && gameObject.activeSelf == activeSelf;
        }
    }

    private sealed class ColliderState
    {
        private readonly Collider2D collider;
        private readonly string serializedState;

        public ColliderState(Collider2D source)
        {
            collider = source;
            serializedState = EditorJsonUtility.ToJson(source);
        }

        public bool Matches()
        {
            return collider != null && EditorJsonUtility.ToJson(collider) == serializedState;
        }
    }
}
