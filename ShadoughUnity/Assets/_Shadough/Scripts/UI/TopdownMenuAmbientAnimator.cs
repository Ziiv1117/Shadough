using UnityEngine;
using UnityEngine.UI;

public class TopdownMenuAmbientAnimator : MonoBehaviour
{
    private enum MotionMode
    {
        Breathe,
        Drift,
        Sway
    }

    [SerializeField] private MotionMode mode = MotionMode.Breathe;
    [SerializeField] private Vector2 travel = new Vector2(36f, 0f);
    [SerializeField] private float speed = 0.4f;
    [SerializeField] private float alphaAmplitude = 0.12f;
    [SerializeField] private float scaleAmplitude = 0.018f;
    [SerializeField] private float rotationAmplitude = 3f;
    [SerializeField] private float phase;

    private RectTransform rectTransform;
    private Graphic graphic;
    private Vector2 basePosition;
    private Vector3 baseScale;
    private Quaternion baseRotation;
    private Color baseColor;

    private void Awake()
    {
        CacheBaseState();
    }

    private void OnEnable()
    {
        CacheBaseState();
    }

    private void Update()
    {
        if (rectTransform == null)
        {
            CacheBaseState();
        }

        float wave = Mathf.Sin((Time.unscaledTime + phase) * speed);

        if (mode == MotionMode.Drift)
        {
            rectTransform.anchoredPosition = basePosition + travel * wave;
        }
        else if (mode == MotionMode.Sway)
        {
            rectTransform.anchoredPosition = basePosition + travel * wave;
            rectTransform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, rotationAmplitude * wave);
        }

        if (mode == MotionMode.Breathe || mode == MotionMode.Sway)
        {
            float scale = 1f + wave * scaleAmplitude;
            rectTransform.localScale = new Vector3(baseScale.x * scale, baseScale.y * scale, baseScale.z);
        }

        if (graphic != null)
        {
            Color color = baseColor;
            color.a = Mathf.Clamp01(baseColor.a + wave * alphaAmplitude);
            graphic.color = color;
        }
    }

    private void CacheBaseState()
    {
        rectTransform = GetComponent<RectTransform>();
        graphic = GetComponent<Graphic>();

        if (rectTransform == null)
        {
            return;
        }

        basePosition = rectTransform.anchoredPosition;
        baseScale = rectTransform.localScale;
        baseRotation = rectTransform.localRotation;
        baseColor = graphic != null ? graphic.color : Color.white;
    }
}
