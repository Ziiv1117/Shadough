using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TopdownMenuButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image targetImage;
    [SerializeField] private TMP_Text label;
    [SerializeField] private float hoverScale = 1.045f;
    [SerializeField] private float pressedScale = 0.97f;
    [SerializeField] private float animationSpeed = 13f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 0.91f, 0.64f, 1f);
    [SerializeField] private Color pressedColor = new Color(1f, 0.78f, 0.42f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.55f, 0.48f, 0.38f, 0.75f);
    [SerializeField] private Color normalImageColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color hoverImageColor = new Color(1f, 0.72f, 0.25f, 0.16f);
    [SerializeField] private Color pressedImageColor = new Color(0.18f, 0.08f, 0.02f, 0.22f);
    [SerializeField] private Color disabledImageColor = new Color(0f, 0f, 0f, 0.08f);

    private Button button;
    private bool hovering;
    private bool pressing;
    private bool selected;
    private Vector3 baseScale = Vector3.one;

    private void Awake()
    {
        CacheComponents();
        baseScale = transform.localScale;
    }

    private void Reset()
    {
        CacheComponents();
    }

    private void OnValidate()
    {
        CacheComponents();
        hoverScale = Mathf.Max(1f, hoverScale);
        pressedScale = Mathf.Clamp(pressedScale, 0.8f, 1f);
        animationSpeed = Mathf.Max(1f, animationSpeed);
    }

    private void Update()
    {
        if (button == null)
        {
            return;
        }

        bool interactable = button.interactable;
        float targetScale = 1f;
        Color targetColor = normalColor;
        Color targetImageColor = normalImageColor;

        if (!interactable)
        {
            targetColor = disabledColor;
            targetImageColor = disabledImageColor;
        }
        else if (pressing)
        {
            targetScale = pressedScale;
            targetColor = pressedColor;
            targetImageColor = pressedImageColor;
        }
        else if (hovering || selected)
        {
            targetScale = hoverScale;
            targetColor = hoverColor;
            targetImageColor = hoverImageColor;
        }

        float t = 1f - Mathf.Exp(-animationSpeed * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale * targetScale, t);

        if (label != null)
        {
            label.color = Color.Lerp(label.color, targetColor, t);
        }

        if (targetImage != null)
        {
            targetImage.color = Color.Lerp(targetImage.color, targetImageColor, t);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        pressing = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressing = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressing = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
        pressing = false;
    }

    private void CacheComponents()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (label == null)
        {
            label = GetComponentInChildren<TMP_Text>();
        }
    }
}
