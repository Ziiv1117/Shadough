using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class TopdownMenuUISetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string ReportPath = "Logs/TopdownMenuUISetup.report.txt";

    private const string StartScreenPath = "Assets/_Shadough/Art/UI/MainMenu/MainMenuBackground_Clean.png";
    private const string StartTitleOverlayPath = "Assets/_Shadough/Art/UI/MainMenu/MainMenuTitle_Clean.png";
    private const string PauseBackgroundPath = "Assets/_Shadough/Art/UI/MainMenu/PauseMenu_Background.png";
    private const string StartButtonPath = "Assets/_Shadough/Art/UI/MainMenu/Button_StartGame.png";
    private const string HowToButtonPath = "Assets/_Shadough/Art/UI/MainMenu/Button_HowToPlay.png";
    private const string QuitButtonPath = "Assets/_Shadough/Art/UI/MainMenu/Button_QuitGame.png";
    private const string PanelVerticalPath = "Assets/_Shadough/Art/UI/Common/Panel_Vertical.png";
    private const string MistCloudPath = "Assets/_Shadough/Art/UI/Common/Mist_Cloud.png";
    private const string FallingLeafPath = "Assets/_Shadough/Art/UI/Common/Falling_Leaf.png";
    private const string WarmGlowPath = "Assets/_Shadough/Art/UI/Common/Warm_Glow.png";

    [MenuItem("Shadough/Setup Topdown Menu UI")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    public static void Setup()
    {
        EnsureTextMeshProResources();
        ConfigureSprites();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        List<string> report = new List<string>
        {
            "Topdown Menu UI Setup",
            "Scene: " + ScenePath
        };

        DisableLegacyPauseMenus(report);
        EnsureEventSystem(report);

        GameObject existingRoot = FindSceneObject("UIRoot_TopdownMenu");
        if (existingRoot != null)
        {
            Object.DestroyImmediate(existingRoot);
        }

        Sprite startScreen = LoadSprite(StartScreenPath);
        Sprite startTitleOverlay = LoadSprite(StartTitleOverlayPath);
        Sprite pauseBackground = LoadSprite(PauseBackgroundPath);
        Sprite startButtonSprite = LoadSprite(StartButtonPath);
        Sprite howToButtonSprite = LoadSprite(HowToButtonPath);
        Sprite quitButtonSprite = LoadSprite(QuitButtonPath);
        Sprite panelVertical = LoadSprite(PanelVerticalPath);
        Sprite mistCloud = LoadSprite(MistCloudPath);
        Sprite fallingLeaf = LoadSprite(FallingLeafPath);
        Sprite warmGlow = LoadSprite(WarmGlowPath);

        GameObject root = new GameObject("UIRoot_TopdownMenu");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        TopdownGameplayInputGate inputGate = root.AddComponent<TopdownGameplayInputGate>();
        TopdownMenuUIController controller = root.AddComponent<TopdownMenuUIController>();

        CanvasGroup mainGroup = CreateFullGroup(root.transform, "MainMenuCanvas", true);
        CanvasGroup pauseGroup = CreateFullGroup(root.transform, "PauseMenuCanvas", false);
        CanvasGroup howToGroup = CreateFullGroup(root.transform, "HowToPlayPanel", false);
        CanvasGroup fadeGroup = CreateFullGroup(root.transform, "FadeOverlay", false);
        CanvasGroup gameplayHudGroup = CreateFullGroup(root.transform, "GameplayHudCanvas", false);

        RectTransform titleGlow;
        RectTransform[] mainItems;
        Button startButton;
        Button mainHowToButton;
        Button mainQuitButton;
        BuildMainMenu(mainGroup.transform, startScreen, startTitleOverlay, startButtonSprite, howToButtonSprite, quitButtonSprite, mistCloud, fallingLeaf, warmGlow, out titleGlow, out mainItems, out startButton, out mainHowToButton, out mainQuitButton);

        Button gameplayPauseButton;
        BuildGameplayHud(gameplayHudGroup.transform, out gameplayPauseButton);

        RectTransform pausePanel;
        RectTransform[] pauseItems;
        Button resumeButton;
        Button restartButton;
        Button pauseHowToButton;
        Button returnMainButton;
        Button pauseQuitButton;
        BuildPauseMenu(pauseGroup.transform, pauseBackground, panelVertical, mistCloud, warmGlow, out pausePanel, out pauseItems, out resumeButton, out restartButton, out pauseHowToButton, out returnMainButton, out pauseQuitButton);

        RectTransform howToPanel;
        Button howToBackButton;
        BuildHowToPanel(howToGroup.transform, pauseBackground, panelVertical, out howToPanel, out howToBackButton);

        Image fade = fadeGroup.gameObject.AddComponent<Image>();
        fade.color = Color.black;

        AssignGameplayGate(inputGate, report);
        AssignController(controller, inputGate, mainGroup, pauseGroup, howToGroup, fadeGroup, gameplayHudGroup, titleGlow, pausePanel, howToPanel, mainItems, pauseItems, startButton, mainHowToButton, mainQuitButton, gameplayPauseButton, resumeButton, restartButton, pauseHowToButton, returnMainButton, pauseQuitButton, howToBackButton);

        report.Add("ui.root=PASS");
        report.Add("ui.mainMenu=PASS");
        report.Add("ui.pauseMenu=PASS");
        report.Add("ui.howToPanel=PASS");
        report.Add("tmp.essentials=" + PassFail(File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset")));
        report.Add("sprites.startScreen=" + PassFail(startScreen != null));
        report.Add("sprites.startTitleOverlay=" + PassFail(startTitleOverlay != null));
        report.Add("sprites.pauseBackground=" + PassFail(pauseBackground != null));
        report.Add("sprites.startButton=" + PassFail(startButtonSprite != null));
        report.Add("sprites.howToButton=" + PassFail(howToButtonSprite != null));
        report.Add("sprites.quitButton=" + PassFail(quitButtonSprite != null));

        CountMissingSceneData(out int missingScripts, out int missingReferences);
        report.Add("missingScripts=" + missingScripts);
        report.Add("missingReferences=" + missingReferences);
        report.Add("consoleCompileErrors=checked_after_unity_refresh");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllLines(FullPath(ReportPath), report);
        Debug.Log("Topdown menu UI setup complete. Report: " + FullPath(ReportPath));
    }

    private static void BuildMainMenu(
        Transform parent,
        Sprite startScreen,
        Sprite startTitleOverlay,
        Sprite startButtonSprite,
        Sprite howToButtonSprite,
        Sprite quitButtonSprite,
        Sprite mistCloud,
        Sprite fallingLeaf,
        Sprite warmGlow,
        out RectTransform titleGlow,
        out RectTransform[] animatedItems,
        out Button startButton,
        out Button howToButton,
        out Button quitButton)
    {
        Image blackUnderlay = CreateSolidImage(parent, "Opaque_Black_Backdrop", Color.black);
        Stretch(blackUnderlay.rectTransform);

        Image backgroundImage = CreateImage(parent, "StartScreen_TitleBase", startScreen, false);
        StretchCover(backgroundImage.rectTransform, 16f / 9f);
        backgroundImage.color = Color.white;
        backgroundImage.raycastTarget = false;

        Image titleOverlay = CreateImage(parent, "StartTitle_Overlay", startTitleOverlay, false);
        StretchCover(titleOverlay.rectTransform, 16f / 9f);
        titleOverlay.color = Color.white;
        titleOverlay.raycastTarget = false;

        Image vignette = CreateSolidImage(parent, "StartScreen_Vignette", new Color(0f, 0f, 0f, 0.12f));
        Stretch(vignette.rectTransform);

        Image glow = CreateImage(parent, "Title_WarmGlow", warmGlow, false);
        titleGlow = glow.rectTransform;
        titleGlow.anchorMin = titleGlow.anchorMax = new Vector2(0.5f, 1f);
        titleGlow.anchoredPosition = new Vector2(0f, -260f);
        titleGlow.sizeDelta = new Vector2(980f, 310f);
        glow.color = new Color(1f, 0.58f, 0.16f, 0.18f);
        ConfigureAmbient(glow.gameObject, 0, Vector2.zero, 1.15f, 0.08f, 0.025f, 0f, 0.1f);

        AddAmbientImage(parent, "Mist_Cloud_Left", mistCloud, new Vector2(0f, 1f), new Vector2(420f, -210f), new Vector2(780f, 230f), new Color(0.65f, 0.74f, 0.86f, 0.13f), 1, new Vector2(54f, 0f), 0.28f, 0.04f, 0f, 0f, 0.2f);
        AddAmbientImage(parent, "Mist_Cloud_Right", mistCloud, new Vector2(1f, 1f), new Vector2(-430f, -185f), new Vector2(720f, 210f), new Color(0.65f, 0.74f, 0.86f, 0.10f), 1, new Vector2(-46f, 0f), 0.24f, 0.035f, 0f, 0f, 2.2f);

        AddAmbientImage(parent, "Falling_Leaf_01", fallingLeaf, new Vector2(0.5f, 1f), new Vector2(-230f, -270f), new Vector2(32f, 22f), new Color(1f, 0.32f, 0.12f, 0.70f), 2, new Vector2(18f, -12f), 0.9f, 0.08f, 0.05f, 5f, 0.3f);
        AddAmbientImage(parent, "Falling_Leaf_02", fallingLeaf, new Vector2(0.5f, 1f), new Vector2(250f, -350f), new Vector2(27f, 19f), new Color(1f, 0.42f, 0.16f, 0.58f), 2, new Vector2(-22f, 10f), 0.72f, 0.07f, 0.04f, 4f, 1.7f);
        AddAmbientImage(parent, "Falling_Leaf_03", fallingLeaf, new Vector2(0.5f, 0.5f), new Vector2(560f, -10f), new Vector2(25f, 18f), new Color(1f, 0.36f, 0.13f, 0.54f), 2, new Vector2(-18f, -8f), 0.64f, 0.06f, 0.04f, 4f, 3.1f);

        RectTransform buttonRoot = CreateRect(parent, "StartButtons", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(650f, 430f));
        startButton = CreateSpriteButton(buttonRoot, "Button_StartGame", startButtonSprite, new Vector2(0f, -32f), new Vector2(620f, 156f));
        howToButton = CreateSpriteButton(buttonRoot, "Button_HowToPlay", howToButtonSprite, new Vector2(0f, -164f), new Vector2(620f, 156f));
        quitButton = CreateSpriteButton(buttonRoot, "Button_QuitGame", quitButtonSprite, new Vector2(0f, -296f), new Vector2(620f, 156f));

        TMP_Text version = CreateText(parent, "VersionText", "Topdown v0.6 Demo Loop", 22f, new Color(0.86f, 0.70f, 0.42f, 0.88f), TextAlignmentOptions.Left);
        version.rectTransform.anchorMin = version.rectTransform.anchorMax = new Vector2(0f, 0f);
        version.rectTransform.pivot = new Vector2(0f, 0f);
        version.rectTransform.anchoredPosition = new Vector2(42f, 28f);
        version.rectTransform.sizeDelta = new Vector2(480f, 42f);

        TMP_Text hint = CreateText(parent, "BottomHint", "SHAPE THE SHADOW. CROSS THE IMPOSSIBLE.", 24f, new Color(0.95f, 0.74f, 0.40f, 0.92f), TextAlignmentOptions.Center);
        hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        hint.rectTransform.anchoredPosition = new Vector2(0f, 46f);
        hint.rectTransform.sizeDelta = new Vector2(760f, 42f);
        hint.characterSpacing = 4f;
        AddTextOutline(hint.gameObject, new Color(0f, 0f, 0f, 0.72f), new Vector2(1.2f, -1.2f));

        animatedItems = new[]
        {
            startButton.GetComponent<RectTransform>(),
            howToButton.GetComponent<RectTransform>(),
            quitButton.GetComponent<RectTransform>(),
            hint.rectTransform
        };
    }

    private static void BuildGameplayHud(Transform parent, out Button pauseButton)
    {
        pauseButton = CreateTextButton(parent, "Button_GameplayPause", "||", new Vector2(54f, -54f), new Vector2(74f, 58f), 28f, false);
        RectTransform rect = pauseButton.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(54f, -54f);
    }

    private static void BuildPauseMenu(
        Transform parent,
        Sprite pauseBackground,
        Sprite panelVertical,
        Sprite mistCloud,
        Sprite warmGlow,
        out RectTransform panelRoot,
        out RectTransform[] animatedItems,
        out Button resumeButton,
        out Button restartButton,
        out Button howToButton,
        out Button returnMainButton,
        out Button quitButton)
    {
        Image blackUnderlay = CreateSolidImage(parent, "Pause_Opaque_Backdrop", Color.black);
        Stretch(blackUnderlay.rectTransform);

        Image background = CreateImage(parent, "Pause_Background", pauseBackground, false);
        StretchCover(background.rectTransform, 16f / 9f);
        background.color = new Color(1f, 1f, 1f, 0.92f);

        Image dim = CreateSolidImage(parent, "Pause_Dim", new Color(0f, 0f, 0f, 0.42f));
        Stretch(dim.rectTransform);

        AddAmbientImage(parent, "Pause_Mist_Cloud", mistCloud, new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(920f, 260f), new Color(0.65f, 0.74f, 0.86f, 0.10f), 1, new Vector2(64f, 0f), 0.22f, 0.04f, 0f, 0f, 1.2f);

        panelRoot = CreateRect(parent, "PausePanel", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 830f));
        Image panel = CreateImage(panelRoot, "Panel_Background", panelVertical, true);
        Stretch(panel.rectTransform);
        panel.color = new Color(1f, 0.95f, 0.80f, 0.96f);

        Image panelGlow = CreateImage(panelRoot, "Panel_WarmGlow", warmGlow, false);
        panelGlow.rectTransform.anchoredPosition = new Vector2(0f, 230f);
        panelGlow.rectTransform.sizeDelta = new Vector2(430f, 170f);
        panelGlow.color = new Color(1f, 0.58f, 0.18f, 0.12f);
        ConfigureAmbient(panelGlow.gameObject, 0, Vector2.zero, 1.1f, 0.06f, 0.018f, 0f, 0.4f);

        TMP_Text title = CreateText(panelRoot, "Title", "PAUSED", 60f, new Color(0.13f, 0.075f, 0.035f, 1f), TextAlignmentOptions.Center);
        title.rectTransform.anchoredPosition = new Vector2(0f, 235f);
        title.rectTransform.sizeDelta = new Vector2(470f, 82f);
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 8f;

        RectTransform stack = CreateRect(panelRoot, "PauseButtonStack", new Vector2(0.5f, 0.5f), new Vector2(0f, -42f), new Vector2(545f, 500f));
        resumeButton = CreateTextButton(stack, "Button_Resume", "RESUME", new Vector2(0f, 190f), new Vector2(520f, 78f), 32f, false);
        restartButton = CreateTextButton(stack, "Button_Restart", "RESTART", new Vector2(0f, 95f), new Vector2(520f, 78f), 32f, false);
        howToButton = CreateTextButton(stack, "Button_HowTo", "HOW TO PLAY", Vector2.zero, new Vector2(520f, 78f), 32f, false);
        returnMainButton = CreateTextButton(stack, "Button_ReturnMain", "MAIN MENU", new Vector2(0f, -95f), new Vector2(520f, 78f), 32f, false);
        quitButton = CreateTextButton(stack, "Button_Quit", "QUIT GAME", new Vector2(0f, -190f), new Vector2(520f, 78f), 32f, false);

        animatedItems = new[]
        {
            title.rectTransform,
            resumeButton.GetComponent<RectTransform>(),
            restartButton.GetComponent<RectTransform>(),
            howToButton.GetComponent<RectTransform>(),
            returnMainButton.GetComponent<RectTransform>(),
            quitButton.GetComponent<RectTransform>()
        };
    }

    private static void BuildHowToPanel(
        Transform parent,
        Sprite pauseBackground,
        Sprite panelVertical,
        out RectTransform panelRoot,
        out Button backButton)
    {
        Image blackUnderlay = CreateSolidImage(parent, "HowTo_Opaque_Backdrop", Color.black);
        Stretch(blackUnderlay.rectTransform);

        Image background = CreateImage(parent, "HowTo_Background", pauseBackground, false);
        StretchCover(background.rectTransform, 16f / 9f);
        background.color = new Color(1f, 1f, 1f, 0.78f);

        Image dim = CreateSolidImage(parent, "HowTo_Dim", new Color(0f, 0f, 0f, 0.52f));
        Stretch(dim.rectTransform);

        panelRoot = CreateRect(parent, "HowToPanel_Frame", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 900f));
        Image panel = CreateImage(panelRoot, "Panel_Background", panelVertical, true);
        Stretch(panel.rectTransform);
        panel.color = new Color(1f, 0.95f, 0.80f, 0.97f);

        TMP_Text title = CreateText(panelRoot, "Title", "HOW TO PLAY", 54f, new Color(0.13f, 0.075f, 0.035f, 1f), TextAlignmentOptions.Center);
        title.rectTransform.anchoredPosition = new Vector2(0f, 260f);
        title.rectTransform.sizeDelta = new Vector2(540f, 74f);
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 5f;

        TMP_Text leftColumn = CreateText(panelRoot, "Rules_LeftColumn",
            "<b><color=#8A521F>Move and Light</color></b>\n" +
            "<color=#B56D27>WASD</color> - Move\n" +
            "<color=#B56D27>G</color> - Place or recall the lantern\n" +
            "Stay near shadows to interact with them\n\n" +
            "<b><color=#8A521F>Cut and Paste Shadows</color></b>\n" +
            "<color=#B56D27>Hold Shift + E</color> - Cut a revealed shadow\n" +
            "<color=#B56D27>F</color> - Paste the shadow you are holding\n" +
            "Only pasted shadows can activate mechanisms",
            21f,
            new Color(0.16f, 0.095f, 0.052f, 1f),
            TextAlignmentOptions.TopLeft);
        leftColumn.rectTransform.anchoredPosition = new Vector2(-140f, -55f);
        leftColumn.rectTransform.sizeDelta = new Vector2(260f, 560f);
        leftColumn.lineSpacing = 6f;
        leftColumn.enableWordWrapping = true;
        leftColumn.richText = true;

        TMP_Text rightColumn = CreateText(panelRoot, "Rules_RightColumn",
            "<b><color=#8A521F>Shadow Abilities</color></b>\n" +
            "Tree Shadow - Forms a bridge across water\n" +
            "Plate Shadow - Presses floor plates\n" +
            "Key Shadow - Unlocks sealed doors\n" +
            "Player Shadow - Lures the Shadow Seeker\n\n" +
            "<b><color=#8A521F>Goal</color></b>\n" +
            "Use shadows to cross the clock tower ruins\n" +
            "Reach the Final Clock Core and press <color=#B56D27>E</color>",
            21f,
            new Color(0.16f, 0.095f, 0.052f, 1f),
            TextAlignmentOptions.TopLeft);
        rightColumn.rectTransform.anchoredPosition = new Vector2(140f, -55f);
        rightColumn.rectTransform.sizeDelta = new Vector2(260f, 560f);
        rightColumn.lineSpacing = 6f;
        rightColumn.enableWordWrapping = true;
        rightColumn.richText = true;

        TMP_Text hint = CreateText(panelRoot, "Hint", "Shape the shadow. Cross the impossible.", 25f, new Color(0.48f, 0.27f, 0.12f, 0.95f), TextAlignmentOptions.Center);
        hint.rectTransform.anchoredPosition = new Vector2(0f, -260f);
        hint.rectTransform.sizeDelta = new Vector2(610f, 56f);
        hint.characterSpacing = 2f;

        backButton = CreateTextButton(panelRoot, "Button_Back", "BACK", new Vector2(0f, -325f), new Vector2(460f, 74f), 31f, false);
    }

    private static Button CreateTextButton(Transform parent, string name, string label, Vector2 position, Vector2 size, float fontSize, bool transparentBody)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.AddComponent<Image>();
        image.color = transparentBody ? new Color(0f, 0f, 0f, 0.001f) : new Color(0.13f, 0.065f, 0.02f, 0.18f);

        if (!transparentBody)
        {
            Outline frame = buttonObject.AddComponent<Outline>();
            frame.effectColor = new Color(0.94f, 0.67f, 0.28f, 0.55f);
            frame.effectDistance = new Vector2(1.6f, -1.6f);
        }

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        TMP_Text text = CreateText(buttonObject.transform, "Label", label, fontSize, new Color(0.95f, 0.76f, 0.38f, 1f), TextAlignmentOptions.Center);
        Stretch(text.rectTransform);
        text.fontStyle = FontStyles.Bold;
        text.characterSpacing = transparentBody ? 7f : 5f;
        AddTextOutline(text.gameObject, transparentBody ? new Color(0f, 0f, 0f, 0.80f) : new Color(0.05f, 0.025f, 0.01f, 0.75f), new Vector2(1.5f, -1.5f));

        TopdownMenuButtonAnimator animator = buttonObject.AddComponent<TopdownMenuButtonAnimator>();
        ConfigureButtonAnimator(
            animator,
            text,
            image,
            transparentBody ? new Color(0f, 0f, 0f, 0.001f) : new Color(0.13f, 0.065f, 0.02f, 0.18f),
            transparentBody ? new Color(1f, 0.72f, 0.25f, 0.16f) : new Color(0.32f, 0.16f, 0.05f, 0.30f),
            transparentBody ? new Color(0.18f, 0.08f, 0.02f, 0.22f) : new Color(0.10f, 0.045f, 0.01f, 0.32f));
        return button;
    }

    private static Button CreateSpriteButton(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.color = Color.white;
        image.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;

        TopdownMenuButtonAnimator animator = buttonObject.AddComponent<TopdownMenuButtonAnimator>();
        ConfigureButtonAnimator(animator, null, image, Color.white, new Color(1f, 0.90f, 0.58f, 1f), new Color(0.86f, 0.68f, 0.40f, 1f));
        return button;
    }

    private static void ConfigureButtonAnimator(TopdownMenuButtonAnimator animator, TMP_Text label, Image image, Color normalImageColor, Color hoverImageColor, Color pressedImageColor)
    {
        SerializedObject serializedAnimator = new SerializedObject(animator);
        SetObject(serializedAnimator, "targetImage", image);
        SetObject(serializedAnimator, "label", label);
        SetColor(serializedAnimator, "normalImageColor", normalImageColor);
        SetColor(serializedAnimator, "hoverImageColor", hoverImageColor);
        SetColor(serializedAnimator, "pressedImageColor", pressedImageColor);
        SetColor(serializedAnimator, "disabledImageColor", new Color(0.52f, 0.46f, 0.38f, 0.78f));
        serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
    }

    private static CanvasGroup CreateFullGroup(Transform parent, string name, bool visible)
    {
        GameObject groupObject = new GameObject(name);
        groupObject.transform.SetParent(parent, false);
        RectTransform rect = groupObject.AddComponent<RectTransform>();
        Stretch(rect);
        CanvasGroup group = groupObject.AddComponent<CanvasGroup>();
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
        groupObject.SetActive(visible);
        return group;
    }

    private static RectTransform CreateRect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image CreateImage(Transform parent, string name, Sprite sprite, bool sliced)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = sprite != null ? new Vector2(sprite.rect.width, sprite.rect.height) : new Vector2(100f, 100f);
        Image image = obj.AddComponent<Image>();
        image.sprite = sprite;
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = !sliced;
        return image;
    }

    private static Image CreateSolidImage(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        Image image = obj.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420f, 90f);
        TextMeshProUGUI label = obj.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = color;
        label.alignment = alignment;
        label.enableWordWrapping = false;
        label.raycastTarget = false;
        return label;
    }

    private static void AddTextOutline(GameObject target, Color color, Vector2 distance)
    {
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
    }

    private static void AddAmbientImage(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 position, Vector2 size, Color color, int mode, Vector2 travel, float speed, float alphaAmplitude, float scaleAmplitude, float rotationAmplitude, float phase)
    {
        if (sprite == null)
        {
            return;
        }

        Image image = CreateImage(parent, name, sprite, false);
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = anchor;
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = size;
        image.color = color;
        ConfigureAmbient(image.gameObject, mode, travel, speed, alphaAmplitude, scaleAmplitude, rotationAmplitude, phase);
    }

    private static void ConfigureAmbient(GameObject obj, int mode, Vector2 travel, float speed, float alphaAmplitude, float scaleAmplitude, float rotationAmplitude, float phase)
    {
        TopdownMenuAmbientAnimator animator = obj.AddComponent<TopdownMenuAmbientAnimator>();
        SerializedObject serializedAnimator = new SerializedObject(animator);
        SetInt(serializedAnimator, "mode", mode);
        SetVector2(serializedAnimator, "travel", travel);
        SetFloat(serializedAnimator, "speed", speed);
        SetFloat(serializedAnimator, "alphaAmplitude", alphaAmplitude);
        SetFloat(serializedAnimator, "scaleAmplitude", scaleAmplitude);
        SetFloat(serializedAnimator, "rotationAmplitude", rotationAmplitude);
        SetFloat(serializedAnimator, "phase", phase);
        serializedAnimator.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void StretchCover(RectTransform rect, float aspectRatio)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(1920f, 1080f);
        AspectRatioFitter fitter = rect.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = aspectRatio;
    }

    private static void ConfigureSprites()
    {
        ConfigureSprite(StartScreenPath, Vector4.zero, 100f);
        ConfigureSprite(StartTitleOverlayPath, Vector4.zero, 100f);
        ConfigureSprite(PauseBackgroundPath, Vector4.zero, 100f);
        ConfigureSprite(StartButtonPath, Vector4.zero, 100f);
        ConfigureSprite(HowToButtonPath, Vector4.zero, 100f);
        ConfigureSprite(QuitButtonPath, Vector4.zero, 100f);
        ConfigureSprite(PanelVerticalPath, new Vector4(80f, 110f, 80f, 115f), 100f);
        ConfigureSprite(MistCloudPath, Vector4.zero, 100f);
        ConfigureSprite(FallingLeafPath, Vector4.zero, 100f);
        ConfigureSprite(WarmGlowPath, Vector4.zero, 100f);
    }

    private static void EnsureTextMeshProResources()
    {
        if (File.Exists("Assets/TextMesh Pro/Resources/TMP Settings.asset"))
        {
            return;
        }

        TMP_PackageResourceImporter.ImportResources(true, false, false);
        AssetDatabase.Refresh();
    }

    private static void ConfigureSprite(string path, Vector4 border, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    private static void AssignGameplayGate(TopdownGameplayInputGate inputGate, List<string> report)
    {
        List<MonoBehaviour> blocked = new List<MonoBehaviour>();
        AddAll(blocked, Object.FindObjectsOfType<TopDownPlayerController>(true));
        AddAll(blocked, Object.FindObjectsOfType<PlayerLanternController>(true));
        AddAll(blocked, Object.FindObjectsOfType<ShadowCutter>(true));
        AddAll(blocked, Object.FindObjectsOfType<PlayerSelfShadowCutter>(true));
        AddAll(blocked, Object.FindObjectsOfType<FreeShadowPlacer>(true));
        AddAll(blocked, Object.FindObjectsOfType<ShadowRecallController>(true));
        AddAll(blocked, Object.FindObjectsOfType<RevealViewController>(true));
        AddAll(blocked, Object.FindObjectsOfType<ShadowStatusUI>(true));
        AddAll(blocked, Object.FindObjectsOfType<PlayerHealthUI>(true));
        AddAll(blocked, Object.FindObjectsOfType<TopdownPrototypeHint>(true));
        AddAll(blocked, Object.FindObjectsOfType<FinalClockCore>(true));
        AddAll(blocked, Object.FindObjectsOfType<TopdownFinalClockCore>(true));

        SerializedObject serializedGate = new SerializedObject(inputGate);
        SerializedProperty property = serializedGate.FindProperty("blockedBehaviours");
        property.arraySize = blocked.Count;
        for (int i = 0; i < blocked.Count; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = blocked[i];
        }

        serializedGate.ApplyModifiedPropertiesWithoutUndo();
        report.Add("gameplayInputGate.blockedBehaviours=" + blocked.Count);
    }

    private static void AddAll<T>(List<MonoBehaviour> list, T[] behaviours) where T : MonoBehaviour
    {
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null && behaviours[i].gameObject.scene.IsValid() && behaviours[i].gameObject.scene.path == ScenePath)
            {
                list.Add(behaviours[i]);
            }
        }
    }

    private static void AssignController(
        TopdownMenuUIController controller,
        TopdownGameplayInputGate inputGate,
        CanvasGroup mainGroup,
        CanvasGroup pauseGroup,
        CanvasGroup howToGroup,
        CanvasGroup fadeGroup,
        CanvasGroup gameplayHudGroup,
        RectTransform titleRoot,
        RectTransform pausePanel,
        RectTransform howToPanel,
        RectTransform[] mainItems,
        RectTransform[] pauseItems,
        Button startButton,
        Button mainHowToButton,
        Button mainQuitButton,
        Button gameplayPauseButton,
        Button resumeButton,
        Button restartButton,
        Button pauseHowToButton,
        Button returnMainButton,
        Button pauseQuitButton,
        Button howToBackButton)
    {
        SerializedObject serializedController = new SerializedObject(controller);
        SetObject(serializedController, "mainMenuGroup", mainGroup);
        SetObject(serializedController, "pauseMenuGroup", pauseGroup);
        SetObject(serializedController, "howToGroup", howToGroup);
        SetObject(serializedController, "fadeOverlayGroup", fadeGroup);
        SetObject(serializedController, "gameplayHudGroup", gameplayHudGroup);
        SetObject(serializedController, "inputGate", inputGate);
        SetObject(serializedController, "titleRoot", titleRoot);
        SetObject(serializedController, "pausePanelRoot", pausePanel);
        SetObject(serializedController, "howToPanelRoot", howToPanel);
        SetRectTransformArray(serializedController, "mainMenuItems", mainItems);
        SetRectTransformArray(serializedController, "pauseMenuItems", pauseItems);
        SetObject(serializedController, "startButton", startButton);
        SetObject(serializedController, "mainHowToButton", mainHowToButton);
        SetObject(serializedController, "mainQuitButton", mainQuitButton);
        SetObject(serializedController, "gameplayPauseButton", gameplayPauseButton);
        SetObject(serializedController, "resumeButton", resumeButton);
        SetObject(serializedController, "restartButton", restartButton);
        SetObject(serializedController, "pauseHowToButton", pauseHowToButton);
        SetObject(serializedController, "returnMainButton", returnMainButton);
        SetObject(serializedController, "pauseQuitButton", pauseQuitButton);
        SetObject(serializedController, "howToBackButton", howToBackButton);
        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void DisableLegacyPauseMenus(List<string> report)
    {
        PauseMenuController[] legacyMenus = Object.FindObjectsOfType<PauseMenuController>(true);
        int disabledCount = 0;
        for (int i = 0; i < legacyMenus.Length; i++)
        {
            if (legacyMenus[i] != null && legacyMenus[i].enabled)
            {
                legacyMenus[i].enabled = false;
                EditorUtility.SetDirty(legacyMenus[i]);
                disabledCount++;
            }
        }

        report.Add("legacyPauseMenus.disabled=" + disabledCount);
    }

    private static void EnsureEventSystem(List<string> report)
    {
        EventSystem eventSystem = Object.FindObjectOfType<EventSystem>(true);
        if (eventSystem == null)
        {
            GameObject eventObject = new GameObject("EventSystem");
            eventSystem = eventObject.AddComponent<EventSystem>();
            eventObject.AddComponent<StandaloneInputModule>();
        }
        else if (eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        report.Add("eventSystem=PASS");
        EditorUtility.SetDirty(eventSystem.gameObject);
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (sceneObject != null
                && sceneObject.scene.IsValid()
                && sceneObject.scene.path == ScenePath
                && sceneObject.name == objectName)
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
            if (sceneObject == null || !sceneObject.scene.IsValid() || sceneObject.scene.path != ScenePath)
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

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetRectTransformArray(SerializedObject serializedObject, string propertyName, RectTransform[] values)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.enumValueIndex = value;
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

    private static void SetVector2(SerializedObject serializedObject, string propertyName, Vector2 value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.vector2Value = value;
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
