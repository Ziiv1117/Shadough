using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopdownMenuUIController : MonoBehaviour
{
    private enum HowToReturnTarget
    {
        MainMenu,
        PauseMenu
    }

    private static bool startGameplayAfterReload;

    [Header("Groups")]
    [SerializeField] private CanvasGroup mainMenuGroup;
    [SerializeField] private CanvasGroup pauseMenuGroup;
    [SerializeField] private CanvasGroup howToGroup;
    [SerializeField] private CanvasGroup fadeOverlayGroup;
    [SerializeField] private CanvasGroup gameplayHudGroup;

    [Header("Gameplay Input")]
    [SerializeField] private TopdownGameplayInputGate inputGate;

    [Header("Animated Roots")]
    [SerializeField] private RectTransform titleRoot;
    [SerializeField] private RectTransform pausePanelRoot;
    [SerializeField] private RectTransform howToPanelRoot;
    [SerializeField] private RectTransform[] mainMenuItems;
    [SerializeField] private RectTransform[] pauseMenuItems;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button mainHowToButton;
    [SerializeField] private Button mainQuitButton;
    [SerializeField] private Button gameplayPauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button pauseHowToButton;
    [SerializeField] private Button returnMainButton;
    [SerializeField] private Button pauseQuitButton;
    [SerializeField] private Button howToBackButton;

    [Header("Timing")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private float fadeDuration = 0.28f;
    [SerializeField] private float panelDuration = 0.22f;
    [SerializeField] private float itemStagger = 0.06f;
    [SerializeField] private float titleBreathAmplitude = 0.018f;
    [SerializeField] private float titleBreathSpeed = 1.6f;

    private Coroutine activeTransition;
    private HowToReturnTarget howToReturnTarget;
    private bool gameStarted;
    private bool pauseOpen;

    public bool IsGameStarted => gameStarted;
    public bool IsPauseOpen => pauseOpen;

    private void Awake()
    {
        RegisterButtonCallbacks();
    }

    private void Start()
    {
        if (startGameplayAfterReload)
        {
            startGameplayAfterReload = false;
            EnterGameplayInstant();
            return;
        }

        ShowMainMenuInstant();
    }

    private void Update()
    {
        AnimateTitle();

        if (!gameStarted || TutorialSignPromptController.BlocksPauseMenuThisFrame)
        {
            return;
        }

        if (Input.GetKeyDown(pauseKey))
        {
            if (howToGroup != null && howToGroup.alpha > 0.5f)
            {
                BackFromHowTo();
                return;
            }

            if (pauseOpen)
            {
                ResumeGame();
                return;
            }

            OpenPauseMenu();
        }
    }

    public void StartGame()
    {
        if (gameStarted)
        {
            return;
        }

        StopActiveTransition();
        activeTransition = StartCoroutine(StartGameRoutine());
    }

    public void ForceStartGameplayForAutomatedTest()
    {
        StopActiveTransition();
        EnterGameplayInstant();
    }

    public void OpenPauseMenu()
    {
        if (!gameStarted || pauseOpen)
        {
            return;
        }

        StopActiveTransition();
        activeTransition = StartCoroutine(ShowPauseRoutine());
    }

    public void ResumeGame()
    {
        if (!pauseOpen)
        {
            return;
        }

        StopActiveTransition();
        activeTransition = StartCoroutine(HidePauseRoutine());
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        startGameplayAfterReload = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        startGameplayAfterReload = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowHowTo()
    {
        howToReturnTarget = pauseOpen ? HowToReturnTarget.PauseMenu : HowToReturnTarget.MainMenu;
        StopActiveTransition();
        activeTransition = StartCoroutine(ShowHowToRoutine());
    }

    public void BackFromHowTo()
    {
        StopActiveTransition();
        activeTransition = StartCoroutine(HideHowToRoutine());
    }

    private IEnumerator StartGameRoutine()
    {
        yield return FadeGroup(mainMenuGroup, 1f, 0f, fadeDuration, false, false);
        EnterGameplayInstant();
    }

    private IEnumerator ShowPauseRoutine()
    {
        pauseOpen = true;
        SetGroupVisible(gameplayHudGroup, false, false);
        SetGameplayInputEnabled(false);
        Time.timeScale = 0f;
        SetGroupVisible(pauseMenuGroup, true, false);
        PreparePanel(pausePanelRoot);
        PrepareItems(pauseMenuItems);
        yield return FadeGroup(pauseMenuGroup, 0f, 1f, fadeDuration, true, true);
        yield return AnimatePanelIn(pausePanelRoot);
        yield return AnimateItemsIn(pauseMenuItems);
    }

    private IEnumerator HidePauseRoutine()
    {
        yield return FadeGroup(pauseMenuGroup, pauseMenuGroup.alpha, 0f, fadeDuration, false, false);
        SetGroupVisible(pauseMenuGroup, false, false);
        pauseOpen = false;
        Time.timeScale = 1f;
        SetGameplayInputEnabled(true);
        SetGroupVisible(gameplayHudGroup, true, true);
    }

    private IEnumerator ShowHowToRoutine()
    {
        SetGroupInteractable(mainMenuGroup, false);
        SetGroupInteractable(pauseMenuGroup, false);
        SetGroupVisible(howToGroup, true, false);
        PreparePanel(howToPanelRoot);
        yield return FadeGroup(howToGroup, 0f, 1f, fadeDuration, true, true);
        yield return AnimatePanelIn(howToPanelRoot);
    }

    private IEnumerator HideHowToRoutine()
    {
        yield return FadeGroup(howToGroup, howToGroup.alpha, 0f, fadeDuration, false, false);
        SetGroupVisible(howToGroup, false, false);

        if (howToReturnTarget == HowToReturnTarget.PauseMenu)
        {
            SetGroupInteractable(pauseMenuGroup, true);
        }
        else
        {
            SetGroupInteractable(mainMenuGroup, true);
        }
    }

    private void EnterGameplayInstant()
    {
        gameStarted = true;
        pauseOpen = false;
        Time.timeScale = 1f;
        SetGameplayInputEnabled(true);
        SetGroupVisible(mainMenuGroup, false, false);
        SetGroupVisible(pauseMenuGroup, false, false);
        SetGroupVisible(howToGroup, false, false);
        SetGroupVisible(fadeOverlayGroup, false, false);
        SetGroupVisible(gameplayHudGroup, true, true);
    }

    private void ShowMainMenuInstant()
    {
        gameStarted = false;
        pauseOpen = false;
        Time.timeScale = 0f;
        SetGameplayInputEnabled(false);
        SetGroupVisible(mainMenuGroup, true, true);
        SetGroupVisible(pauseMenuGroup, false, false);
        SetGroupVisible(howToGroup, false, false);
        SetGroupVisible(fadeOverlayGroup, false, false);
        SetGroupVisible(gameplayHudGroup, false, false);
        PrepareItems(mainMenuItems);
        StartCoroutine(AnimateItemsIn(mainMenuItems));
    }

    private void AnimateTitle()
    {
        if (titleRoot == null || !titleRoot.gameObject.activeInHierarchy)
        {
            return;
        }

        float scale = 1f + Mathf.Sin(Time.unscaledTime * titleBreathSpeed) * titleBreathAmplitude;
        titleRoot.localScale = new Vector3(scale, scale, 1f);
    }

    private void PreparePanel(RectTransform panel)
    {
        if (panel == null)
        {
            return;
        }

        panel.localScale = Vector3.one * 0.94f;
    }

    private IEnumerator AnimatePanelIn(RectTransform panel)
    {
        if (panel == null)
        {
            yield break;
        }

        float elapsed = 0f;
        Vector3 start = Vector3.one * 0.94f;
        while (elapsed < panelDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Smooth01(elapsed / panelDuration);
            panel.localScale = Vector3.Lerp(start, Vector3.one, t);
            yield return null;
        }

        panel.localScale = Vector3.one;
    }

    private void PrepareItems(RectTransform[] items)
    {
        if (items == null)
        {
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                continue;
            }

            CanvasGroup group = EnsureCanvasGroup(items[i].gameObject);
            group.alpha = 0f;
            items[i].anchoredPosition += new Vector2(0f, -18f);
        }
    }

    private IEnumerator AnimateItemsIn(RectTransform[] items)
    {
        if (items == null)
        {
            yield break;
        }

        for (int i = 0; i < items.Length; i++)
        {
            RectTransform item = items[i];
            if (item == null)
            {
                continue;
            }

            StartCoroutine(AnimateItemIn(item));
            yield return WaitUnscaled(itemStagger);
        }
    }

    private IEnumerator AnimateItemIn(RectTransform item)
    {
        CanvasGroup group = EnsureCanvasGroup(item.gameObject);
        Vector2 start = item.anchoredPosition;
        Vector2 end = start + new Vector2(0f, 18f);
        float elapsed = 0f;
        while (elapsed < panelDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Smooth01(elapsed / panelDuration);
            group.alpha = Mathf.Lerp(0f, 1f, t);
            item.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        group.alpha = 1f;
        item.anchoredPosition = end;
    }

    private IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration, bool interactable, bool blockRaycasts)
    {
        if (group == null)
        {
            yield break;
        }

        group.gameObject.SetActive(true);
        group.interactable = false;
        group.blocksRaycasts = blockRaycasts;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : Smooth01(elapsed / duration);
            group.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        group.alpha = to;
        group.interactable = interactable;
        group.blocksRaycasts = blockRaycasts;
    }

    private static IEnumerator WaitUnscaled(float seconds)
    {
        float end = Time.unscaledTime + Mathf.Max(0f, seconds);
        while (Time.unscaledTime < end)
        {
            yield return null;
        }
    }

    private void RegisterButtonCallbacks()
    {
        AddListener(startButton, StartGame);
        AddListener(mainHowToButton, ShowHowTo);
        AddListener(mainQuitButton, QuitGame);
        AddListener(gameplayPauseButton, OpenPauseMenu);
        AddListener(resumeButton, ResumeGame);
        AddListener(restartButton, RestartGame);
        AddListener(pauseHowToButton, ShowHowTo);
        AddListener(returnMainButton, ReturnToMainMenu);
        AddListener(pauseQuitButton, QuitGame);
        AddListener(howToBackButton, BackFromHowTo);
    }

    private void OnDestroy()
    {
        RemoveListener(startButton, StartGame);
        RemoveListener(mainHowToButton, ShowHowTo);
        RemoveListener(mainQuitButton, QuitGame);
        RemoveListener(gameplayPauseButton, OpenPauseMenu);
        RemoveListener(resumeButton, ResumeGame);
        RemoveListener(restartButton, RestartGame);
        RemoveListener(pauseHowToButton, ShowHowTo);
        RemoveListener(returnMainButton, ReturnToMainMenu);
        RemoveListener(pauseQuitButton, QuitGame);
        RemoveListener(howToBackButton, BackFromHowTo);
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            SetGameplayInputEnabled(true);
            Time.timeScale = 1f;
        }
    }

    private void SetGameplayInputEnabled(bool enabled)
    {
        if (inputGate != null)
        {
            inputGate.SetGameplayInputEnabled(enabled);
        }
    }

    private void StopActiveTransition()
    {
        if (activeTransition != null)
        {
            StopCoroutine(activeTransition);
            activeTransition = null;
        }
    }

    private static void AddListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private static void RemoveListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(action);
        }
    }

    private static void SetGroupVisible(CanvasGroup group, bool visible, bool interactable)
    {
        if (group == null)
        {
            return;
        }

        group.gameObject.SetActive(visible);
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible && interactable;
        group.blocksRaycasts = visible && interactable;
    }

    private static void SetGroupInteractable(CanvasGroup group, bool interactable)
    {
        if (group == null)
        {
            return;
        }

        group.interactable = interactable;
        group.blocksRaycasts = interactable;
    }

    private static CanvasGroup EnsureCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = target.AddComponent<CanvasGroup>();
        }

        return group;
    }

    private static float Smooth01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }
}
