using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HeroPlayerAssetIntegrator
{
    private const string RequestPath = "Temp/HeroPlayerAssetIntegrator.request";
    private const string SpriteFolder = "Assets/_Shadough/Sprites/Player/Hero";
    private const string AnimationFolder = "Assets/_Shadough/Animations/Player/Hero";
    private const string ControllerPath = AnimationFolder + "/HeroPlayer.controller";
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const int FrameSize = 256;
    private const int GridSize = 4;
    private const float PixelsPerUnit = 160f;

    private static readonly DirectionInfo[] Directions =
    {
        new DirectionInfo("Down", 0),
        new DirectionInfo("Left", 1),
        new DirectionInfo("Right", 2),
        new DirectionInfo("Up", 3)
    };

    private static readonly SheetInfo[] Sheets =
    {
        new SheetInfo("hero_idle_lantern_4dir_v2", "Idle", true, true, 5f),
        new SheetInfo("hero_walk_lantern_4dir_v2", "Walk", true, true, 8f),
        new SheetInfo("hero_idle_no_lantern_4dir_v1", "Idle", false, true, 5f),
        new SheetInfo("hero_walk_no_lantern_4dir_v2", "Walk", false, true, 8f),
        new SheetInfo("hero_place_lantern_4dir_v1", "PlaceLantern", true, false, 8f),
        new SheetInfo("hero_pickup_lantern_4dir_v1", "PickupLantern", true, false, 8f),
        new SheetInfo("hero_reveal_focus_lantern_4dir_v1", "RevealFocus", true, true, 5f),
        new SheetInfo("hero_reveal_focus_no_lantern_4dir_v1", "RevealFocus", false, true, 5f),
        new SheetInfo("hero_cut_shadow_no_lantern_4dir_v1", "CutShadow", false, false, 8f),
        new SheetInfo("hero_paste_shadow_no_lantern_4dir_v1", "PasteShadow", false, false, 8f),
        new SheetInfo("hero_interact_no_lantern_4dir_v1", "Interact", false, false, 8f),
        new SheetInfo("hero_hurt_no_lantern_4dir_v5", "Hurt", false, false, 8f),
        new SheetInfo("hero_hurt_lantern_4dir_v1", "Hurt", true, false, 8f),
        new SheetInfo("hero_activate_core_lantern_4dir_v3", "ActivateCore", true, false, 8f)
    };

    [InitializeOnLoadMethod]
    private static void RunRequestedIntegration()
    {
        EditorApplication.delayCall += delegate
        {
            if (!File.Exists(RequestPath))
            {
                return;
            }

            File.Delete(RequestPath);
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Hero player integration skipped because Unity is in Play Mode.");
                return;
            }

            IntegrateHeroPlayer();
        };
    }

    [MenuItem("Shadough/Player/Integrate Hero Player")]
    public static void IntegrateHeroPlayer()
    {
        AssetDatabase.Refresh();
        EnsureFolder(SpriteFolder);
        EnsureFolder(AnimationFolder);

        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        for (int i = 0; i < Sheets.Length; i++)
        {
            SheetInfo sheet = Sheets[i];
            ConfigureTextureImporter(sheet);
            LoadSheetSprites(sheet, sprites);
        }

        List<AnimationClip> clips = new List<AnimationClip>();
        for (int i = 0; i < Sheets.Length; i++)
        {
            SheetInfo sheet = Sheets[i];
            for (int d = 0; d < Directions.Length; d++)
            {
                AnimationClip clip = CreateOrUpdateClip(sheet, Directions[d], sprites);
                clips.Add(clip);
            }
        }

        AnimatorController controller = CreateOrUpdateController(clips);
        IntegrateScenePlayer(controller, sprites);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Hero player visual integration complete.");
    }

    private static void ConfigureTextureImporter(SheetInfo sheet)
    {
        string assetPath = sheet.AssetPath;
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            throw new FileNotFoundException("Hero sheet was not found in the Unity project.", assetPath);
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
        string stateName = GetStateName(sheet.ActionName, sheet.LanternHeld, direction.Name);
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

        EditorCurveBinding binding = new EditorCurveBinding();
        binding.type = typeof(SpriteRenderer);
        binding.path = string.Empty;
        binding.propertyName = "m_Sprite";

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[GridSize + 1];
        for (int frame = 0; frame < GridSize; frame++)
        {
            string spriteName = GetSpriteName(sheet.FileBase, direction, frame);
            Sprite sprite = null;
            if (!sprites.TryGetValue(spriteName, out sprite) || sprite == null)
            {
                throw new MissingReferenceException("Missing sliced sprite: " + spriteName);
            }

            keyframes[frame] = new ObjectReferenceKeyframe();
            keyframes[frame].time = frame / sheet.FrameRate;
            keyframes[frame].value = sprite;
        }

        keyframes[GridSize] = new ObjectReferenceKeyframe();
        keyframes[GridSize].time = GridSize / sheet.FrameRate;
        keyframes[GridSize].value = sheet.Loop ? keyframes[0].value : keyframes[GridSize - 1].value;
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

        AnimatorState defaultState = FindState(stateMachine, "Hero_Idle_Lantern_Down");
        if (defaultState != null)
        {
            stateMachine.defaultState = defaultState;
        }

        EditorUtility.SetDirty(controller);
        return controller;
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

    private static void IntegrateScenePlayer(AnimatorController controller, Dictionary<string, Sprite> sprites)
    {
        EditorSceneManager.OpenScene(ScenePath);

        GameObject player = GameObject.Find("Player_Topdown");
        if (player == null)
        {
            TopDownPlayerController controllerComponent = Object.FindObjectOfType<TopDownPlayerController>();
            if (controllerComponent != null)
            {
                player = controllerComponent.gameObject;
            }
        }

        if (player == null)
        {
            throw new MissingReferenceException("Player_Topdown was not found in " + ScenePath);
        }

        SpriteRenderer grayboxRenderer = player.GetComponent<SpriteRenderer>();
        Transform visualTransform = player.transform.Find("HeroVisual");
        if (visualTransform == null)
        {
            GameObject visualObject = new GameObject("HeroVisual");
            visualTransform = visualObject.transform;
            visualTransform.SetParent(player.transform, false);
        }

        BoxCollider2D playerCollider = player.GetComponent<BoxCollider2D>();
        float visualY = -0.475f;
        if (playerCollider != null)
        {
            visualY = playerCollider.offset.y - playerCollider.size.y * 0.5f;
        }

        visualTransform.localPosition = new Vector3(0f, visualY, 0f);
        visualTransform.localRotation = Quaternion.identity;
        visualTransform.localScale = Vector3.one;

        SpriteRenderer visualRenderer = visualTransform.GetComponent<SpriteRenderer>();
        if (visualRenderer == null)
        {
            visualRenderer = visualTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        Sprite idleSprite = null;
        sprites.TryGetValue(GetSpriteName("hero_idle_lantern_4dir_v2", Directions[0], 0), out idleSprite);
        visualRenderer.sprite = idleSprite;
        visualRenderer.color = Color.white;
        visualRenderer.drawMode = SpriteDrawMode.Simple;
        visualRenderer.sortingLayerID = grayboxRenderer != null ? grayboxRenderer.sortingLayerID : 0;
        visualRenderer.sortingOrder = grayboxRenderer != null ? grayboxRenderer.sortingOrder : 10;
        visualRenderer.flipX = false;
        visualRenderer.flipY = false;

        Animator animator = visualTransform.GetComponent<Animator>();
        if (animator == null)
        {
            animator = visualTransform.gameObject.AddComponent<Animator>();
        }

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        HeroPlayerAnimatorDriver driver = player.GetComponent<HeroPlayerAnimatorDriver>();
        if (driver == null)
        {
            driver = player.AddComponent<HeroPlayerAnimatorDriver>();
        }

        SerializedObject serializedDriver = new SerializedObject(driver);
        serializedDriver.FindProperty("animator").objectReferenceValue = animator;
        serializedDriver.FindProperty("lanternController").objectReferenceValue = player.GetComponent<PlayerLanternController>();
        serializedDriver.FindProperty("shadowInventory").objectReferenceValue = player.GetComponent<ShadowInventory>();
        serializedDriver.ApplyModifiedPropertiesWithoutUndo();

        if (grayboxRenderer != null)
        {
            grayboxRenderer.enabled = false;
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }

    private static void EnsureFolder(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return;
        }

        string fullPath = Path.GetFullPath(assetPath);
        if (Directory.Exists(fullPath))
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }
        }

        string parent = Path.GetDirectoryName(assetPath);
        string name = Path.GetFileName(assetPath);
        if (!string.IsNullOrEmpty(parent))
        {
            parent = parent.Replace("\\", "/");
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static string GetSpriteName(string fileBase, DirectionInfo direction, int frameIndex)
    {
        return fileBase + "_" + direction.Name + "_" + (frameIndex + 1).ToString("00");
    }

    private static string GetStateName(string actionName, bool lanternHeld, string directionName)
    {
        string lanternPart = lanternHeld ? "Lantern" : "NoLantern";
        return "Hero_" + actionName + "_" + lanternPart + "_" + directionName;
    }

    private sealed class DirectionInfo
    {
        public readonly string Name;
        public readonly int Row;

        public DirectionInfo(string name, int row)
        {
            Name = name;
            Row = row;
        }
    }

    private sealed class SheetInfo
    {
        public readonly string FileBase;
        public readonly string ActionName;
        public readonly bool LanternHeld;
        public readonly bool Loop;
        public readonly float FrameRate;

        public string AssetPath
        {
            get { return SpriteFolder + "/" + FileBase + ".png"; }
        }

        public SheetInfo(string fileBase, string actionName, bool lanternHeld, bool loop, float frameRate)
        {
            FileBase = fileBase;
            ActionName = actionName;
            LanternHeld = lanternHeld;
            Loop = loop;
            FrameRate = frameRate;
        }
    }
}
