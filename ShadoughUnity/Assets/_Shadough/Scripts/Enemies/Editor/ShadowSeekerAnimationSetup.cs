using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ShadowSeekerAnimationSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string SpriteFolder = "Assets/_Shadough/Sprites/Enemies/ShadowSeeker";
    private const string AnimationFolder = "Assets/_Shadough/Animations/Enemies/ShadowSeeker";
    private const string ControllerPath = AnimationFolder + "/ShadowSeeker.controller";
    private const string AutoSetupRequestPath = "Temp/ShadowSeekerAnimationSetup.request";
    private const int FrameSize = 256;
    private const int GridSize = 4;
    private const float PixelsPerUnit = 160f;
    private static readonly Vector3 SceneVisualScale = new Vector3(0.85f, 0.85f, 1f);

    private static readonly DirectionInfo[] Directions =
    {
        new DirectionInfo("Down", 0),
        new DirectionInfo("Left", 1),
        new DirectionInfo("Right", 2),
        new DirectionInfo("Up", 3)
    };

    private static readonly SheetInfo[] Sheets =
    {
        new SheetInfo("shadowseeker_idle_4dir_v1", "Idle", true, 5f),
        new SheetInfo("shadowseeker_patrol_move_4dir_v1", "PatrolMove", true, 8f),
        new SheetInfo("shadowseeker_alert_4dir_v1", "Alert", false, 8f),
        new SheetInfo("shadowseeker_chase_4dir_v1", "Chase", true, 8f),
        new SheetInfo("shadowseeker_attracted_4dir_v1", "Attracted", true, 8f),
        new SheetInfo("shadowseeker_lure_reached_4dir_v1", "LureReached", true, 6f),
        new SheetInfo("shadowseeker_stunned_4dir_v1", "Stunned", true, 6f),
        new SheetInfo("shadowseeker_hurt_4dir_v1", "Hurt", false, 8f),
        new SheetInfo("shadowseeker_dissolve_4dir_v1", "Dissolve", false, 8f)
    };

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Enemies/Setup ShadowSeeker Animation")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    private static void TryAutoSetup()
    {
        string requestPath = GetAutoSetupRequestFullPath();
        if (!File.Exists(requestPath))
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
            File.Delete(requestPath);
            Debug.Log("ShadowSeeker animation auto setup complete.");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static string GetAutoSetupRequestFullPath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", AutoSetupRequestPath));
    }

    public static void Setup()
    {
        AssetDatabase.Refresh();
        EnsureFolder(SpriteFolder);
        EnsureFolder(AnimationFolder);

        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        for (int i = 0; i < Sheets.Length; i++)
        {
            ConfigureTextureImporter(Sheets[i]);
            LoadSheetSprites(Sheets[i], sprites);
        }

        List<AnimationClip> clips = new List<AnimationClip>();
        for (int i = 0; i < Sheets.Length; i++)
        {
            for (int d = 0; d < Directions.Length; d++)
            {
                clips.Add(CreateOrUpdateClip(Sheets[i], Directions[d], sprites));
            }
        }

        AnimatorController controller = CreateOrUpdateController(clips);
        IntegrateSceneSeeker(controller, sprites);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ValidateAssetsAndScene();
        Debug.Log("ShadowSeeker animation setup complete.");
    }

    private static void ConfigureTextureImporter(SheetInfo sheet)
    {
        TextureImporter importer = AssetImporter.GetAtPath(sheet.AssetPath) as TextureImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("ShadowSeeker sheet was not found in the Unity project.", sheet.AssetPath);
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.isReadable = false;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = new Vector2(0.5f, 0f);
        importer.SetTextureSettings(settings);
        SetUncompressedPlatform(importer, "DefaultTexturePlatform", false);
        SetUncompressedPlatform(importer, "Standalone", true);

        SpriteMetaData[] metadata = new SpriteMetaData[GridSize * GridSize];
        int index = 0;
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                SpriteMetaData spriteData = new SpriteMetaData();
                spriteData.name = GetSpriteName(sheet.FileBase, Directions[row], col);
                spriteData.rect = new Rect(col * FrameSize, (GridSize - 1 - row) * FrameSize, FrameSize, FrameSize);
                spriteData.alignment = (int)SpriteAlignment.Custom;
                spriteData.pivot = new Vector2(0.5f, 0f);
                metadata[index] = spriteData;
                index++;
            }
        }

        importer.spritesheet = metadata;
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

    private static void LoadSheetSprites(SheetInfo sheet, Dictionary<string, Sprite> sprites)
    {
        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(sheet.AssetPath);
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;
            if (sprite != null)
            {
                sprites[sprite.name] = sprite;
            }
        }
    }

    private static AnimationClip CreateOrUpdateClip(SheetInfo sheet, DirectionInfo direction, Dictionary<string, Sprite> sprites)
    {
        string stateName = GetStateName(sheet.ActionName, direction.Name);
        string clipPath = AnimationFolder + "/" + stateName + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        clip.name = stateName;
        clip.frameRate = sheet.FrameRate;
        clip.wrapMode = sheet.Loop ? WrapMode.Loop : WrapMode.Once;
        clip.ClearCurves();

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = string.Empty,
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[GridSize + 1];
        for (int frame = 0; frame < GridSize; frame++)
        {
            string spriteName = GetSpriteName(sheet.FileBase, direction, frame);
            Sprite sprite = null;
            if (!sprites.TryGetValue(spriteName, out sprite) || sprite == null)
            {
                throw new MissingReferenceException("Missing sliced ShadowSeeker sprite: " + spriteName);
            }

            keyframes[frame] = new ObjectReferenceKeyframe
            {
                time = frame / sheet.FrameRate,
                value = sprite
            };
        }

        keyframes[GridSize] = new ObjectReferenceKeyframe
        {
            time = GridSize / sheet.FrameRate,
            value = sheet.Loop ? keyframes[0].value : keyframes[GridSize - 1].value
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = sheet.Loop;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static AnimatorController CreateOrUpdateController(List<AnimationClip> clips)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        for (int i = 0; i < clips.Count; i++)
        {
            AnimationClip clip = clips[i];
            AnimatorState state = FindState(stateMachine, clip.name);
            if (state == null)
            {
                state = stateMachine.AddState(clip.name);
            }

            state.motion = clip;
        }

        AnimatorState defaultState = FindState(stateMachine, "ShadowSeeker_Idle_Down");
        if (defaultState != null)
        {
            stateMachine.defaultState = defaultState;
        }

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static void IntegrateSceneSeeker(AnimatorController controller, Dictionary<string, Sprite> sprites)
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject seekerObject = GameObject.Find("ShadowSeeker_Topdown");
        if (seekerObject == null)
        {
            throw new MissingReferenceException("ShadowSeeker_Topdown was not found in " + ScenePath);
        }

        seekerObject.transform.localScale = SceneVisualScale;

        SpriteRenderer renderer = seekerObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = seekerObject.AddComponent<SpriteRenderer>();
        }

        Sprite defaultSprite = null;
        sprites.TryGetValue(GetSpriteName("shadowseeker_idle_4dir_v1", Directions[0], 0), out defaultSprite);
        renderer.sprite = defaultSprite;
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.sortingOrder = 8;

        Animator animator = seekerObject.GetComponent<Animator>();
        if (animator == null)
        {
            animator = seekerObject.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;

        EnemyShadowSeekerAnimatorDriver driver = seekerObject.GetComponent<EnemyShadowSeekerAnimatorDriver>();
        if (driver == null)
        {
            driver = seekerObject.AddComponent<EnemyShadowSeekerAnimatorDriver>();
        }

        SerializedObject serializedDriver = new SerializedObject(driver);
        serializedDriver.FindProperty("seeker").objectReferenceValue = seekerObject.GetComponent<EnemyShadowSeeker>();
        serializedDriver.FindProperty("animator").objectReferenceValue = animator;
        serializedDriver.FindProperty("alertDuration").floatValue = 0.25f;
        serializedDriver.FindProperty("moveThreshold").floatValue = 0.01f;
        serializedDriver.FindProperty("lureReachedDistancePadding").floatValue = 0.08f;
        serializedDriver.ApplyModifiedPropertiesWithoutUndo();

        EnemyShadowSeeker seeker = seekerObject.GetComponent<EnemyShadowSeeker>();
        if (seeker != null)
        {
            SerializedObject serializedSeeker = new SerializedObject(seeker);
            SerializedProperty feedbackRenderer = serializedSeeker.FindProperty("feedbackRenderer");
            if (feedbackRenderer != null)
            {
                feedbackRenderer.objectReferenceValue = renderer;
            }

            serializedSeeker.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static void ValidateAssetsAndScene()
    {
        int spriteSheetCount = 0;
        int clipCount = 0;
        for (int i = 0; i < Sheets.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(Sheets[i].AssetPath) != null)
            {
                spriteSheetCount++;
            }

            for (int d = 0; d < Directions.Length; d++)
            {
                if (AssetDatabase.LoadAssetAtPath<AnimationClip>(AnimationFolder + "/" + GetStateName(Sheets[i].ActionName, Directions[d].Name) + ".anim") != null)
                {
                    clipCount++;
                }
            }
        }

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        GameObject seekerObject = GameObject.Find("ShadowSeeker_Topdown");
        Animator animator = seekerObject != null ? seekerObject.GetComponent<Animator>() : null;
        EnemyShadowSeekerAnimatorDriver driver = seekerObject != null ? seekerObject.GetComponent<EnemyShadowSeekerAnimatorDriver>() : null;

        int missingScripts = 0;
        int missingReferences = 0;
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

        Debug.Log("ShadowSeeker animation validation: spriteSheets=" + spriteSheetCount
            + ", clips=" + clipCount
            + ", controller=" + (controller != null)
            + ", animatorBound=" + (animator != null && animator.runtimeAnimatorController == controller)
            + ", driver=" + (driver != null)
            + ", missingScripts=" + missingScripts
            + ", missingReferences=" + missingReferences);
    }

    private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
    {
        ChildAnimatorState[] states = stateMachine.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state != null && states[i].state.name == stateName)
            {
                return states[i].state;
            }
        }

        return null;
    }

    private static string GetSpriteName(string fileBase, DirectionInfo direction, int frame)
    {
        return fileBase + "_" + direction.Name + "_" + frame;
    }

    private static string GetStateName(string actionName, string directionName)
    {
        return "ShadowSeeker_" + actionName + "_" + directionName;
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
        string name = Path.GetFileName(folder);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private struct SheetInfo
    {
        public readonly string FileBase;
        public readonly string ActionName;
        public readonly bool Loop;
        public readonly float FrameRate;

        public string AssetPath => SpriteFolder + "/" + FileBase + ".png";

        public SheetInfo(string fileBase, string actionName, bool loop, float frameRate)
        {
            FileBase = fileBase;
            ActionName = actionName;
            Loop = loop;
            FrameRate = frameRate;
        }
    }

    private struct DirectionInfo
    {
        public readonly string Name;
        public readonly int Row;

        public DirectionInfo(string name, int row)
        {
            Name = name;
            Row = row;
        }
    }
}
