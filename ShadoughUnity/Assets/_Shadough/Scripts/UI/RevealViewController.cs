using System.Collections.Generic;
using UnityEngine;

public class RevealViewController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode revealKey = KeyCode.LeftShift;
    [SerializeField] private bool holdToReveal = true;
    [SerializeField] private bool isRevealActive;

    [Header("Overlay")]
    [SerializeField] private CanvasGroup darkOverlayGroup;
    [SerializeField, Range(0f, 1f)] private float overlayAlpha = 0.35f;

    [Header("Highlight Colors")]
    [SerializeField] private Color cuttableHighlightColor = new Color(0.05f, 0.75f, 1f, 1f);
    [SerializeField] private Color pastedHighlightColor = new Color(1f, 0.9f, 0.25f, 0.9f);
    [SerializeField] private Color interactableHighlightColor = new Color(0.35f, 1f, 0.45f, 1f);

    [Header("Input Hint")]
    [SerializeField] private bool showInputHint = true;
    [SerializeField] private Vector2 inputHintPosition = new Vector2(18f, 18f);
    [SerializeField] private Vector2 inputHintSize = new Vector2(300f, 146f);

    private readonly Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
    private Texture2D overlayTexture;

    private bool previousRevealState;
    public static RevealViewController Instance { get; private set; }
    public static bool HasInstance => Instance != null;
    public static bool IsActive => Instance != null && Instance.IsRevealActive;

    public bool IsRevealActive => isRevealActive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple RevealViewController instances found. Using the latest active instance.");
        }

        Instance = this;
        SetOverlayAlpha(0f);
    }

    private void Update()
    {
        bool shouldReveal = holdToReveal ? Input.GetKey(revealKey) : ToggleRequested();

        if (shouldReveal != isRevealActive)
        {
            SetRevealActive(shouldReveal);

            if (shouldReveal && !previousRevealState)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.revealShadow);
                }
            }

            previousRevealState = shouldReveal;
        }

        if (isRevealActive)
        {
            ApplyRevealHighlights();
        }
    }

    private bool ToggleRequested()
    {
        if (!Input.GetKeyDown(revealKey))
        {
            return isRevealActive;
        }

        return !isRevealActive;
    }

    public void SetRevealActive(bool active)
    {
        if (isRevealActive == active)
        {
            return;
        }

        isRevealActive = active;
        SetOverlayAlpha(active ? overlayAlpha : 0f);

        if (active)
        {
            ApplyRevealHighlights();
            return;
        }

        RestoreOriginalColors();
    }

    private void ApplyRevealHighlights()
    {
        ShadowInteractable[] cuttableShadows = FindObjectsOfType<ShadowInteractable>();
        for (int i = 0; i < cuttableShadows.Length; i++)
        {
            SpriteRenderer renderer = cuttableShadows[i].GetComponent<SpriteRenderer>();
            ApplyHighlight(renderer, cuttableHighlightColor);
        }

        PastedShadowObject[] pastedShadows = FindObjectsOfType<PastedShadowObject>();
        for (int i = 0; i < pastedShadows.Length; i++)
        {
            ApplyHighlight(pastedShadows[i].SpriteRenderer, pastedHighlightColor);
        }

        HighlightComponentRenderers<PressurePlateController>();
        HighlightComponentRenderers<LockController>();
        HighlightComponentRenderers<FinalClockCore>();
        HighlightComponentRenderers<EnemyShadowSeeker>();
    }

    private void HighlightComponentRenderers<T>() where T : Component
    {
        T[] components = FindObjectsOfType<T>();
        for (int i = 0; i < components.Length; i++)
        {
            SpriteRenderer renderer = components[i].GetComponent<SpriteRenderer>();
            ApplyHighlight(renderer, interactableHighlightColor);
        }
    }

    private void ApplyHighlight(SpriteRenderer renderer, Color highlightColor)
    {
        if (renderer == null || !renderer.enabled)
        {
            return;
        }

        if (!originalColors.ContainsKey(renderer))
        {
            originalColors.Add(renderer, renderer.color);
        }

        renderer.color = highlightColor;
    }

    private void RestoreOriginalColors()
    {
        foreach (KeyValuePair<SpriteRenderer, Color> entry in originalColors)
        {
            if (entry.Key != null)
            {
                entry.Key.color = entry.Value;
            }
        }

        originalColors.Clear();
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (darkOverlayGroup != null)
        {
            darkOverlayGroup.alpha = alpha;
            darkOverlayGroup.blocksRaycasts = false;
            darkOverlayGroup.interactable = false;
        }
    }

    private void OnGUI()
    {
        if (isRevealActive && overlayAlpha > 0f)
        {
            DrawOverlay();
        }

        if (showInputHint)
        {
            DrawInputHint();
        }
    }

    private void DrawOverlay()
    {
        if (overlayTexture == null)
        {
            overlayTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            overlayTexture.SetPixel(0, 0, Color.white);
            overlayTexture.Apply();
        }

        Color previousColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, overlayAlpha);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), overlayTexture);
        GUI.color = previousColor;
    }

    private void DrawInputHint()
    {
        string revealKeyText = GetRevealKeyDisplayName();
        string hintText =
            "A / D: Move\n" +
            "Space: Jump\n" +
            "Hold " + revealKeyText + ": Reveal Shadows\n" +
            "E while Revealing: Cut Shadow\n" +
            "Q while Revealing: Cut Self Shadow\n" +
            "F: Paste Shadow\n" +
            "G: Plant / Retrieve Lantern\n" +
            "E near Clock: Start Clock";

        Color previousColor = GUI.color;
        GUI.color = Color.white;
        GUI.Label(new Rect(inputHintPosition.x, inputHintPosition.y, inputHintSize.x, inputHintSize.y), hintText);
        GUI.color = previousColor;
    }

    private string GetRevealKeyDisplayName()
    {
        if (revealKey == KeyCode.LeftShift || revealKey == KeyCode.RightShift)
        {
            return "Shift";
        }

        return revealKey.ToString();
    }

    private void OnDisable()
    {
        SetRevealActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        overlayAlpha = Mathf.Clamp01(overlayAlpha);
    }
}
