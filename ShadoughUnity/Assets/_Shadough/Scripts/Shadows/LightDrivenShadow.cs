using UnityEngine;

[RequireComponent(typeof(ShadowInteractable))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class LightDrivenShadow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLanternController lanternController;
    [SerializeField] private Transform lightTransform;
    [SerializeField] private Transform casterTransform;
    [SerializeField] private ShadowInteractable shadowInteractable;
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private BoxCollider2D shadowCollider;

    [Header("Shape")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 6f;
    [SerializeField] private float minLength = 1.2f;
    [SerializeField] private float maxLength = 5.5f;
    [SerializeField] private float shadowWidth = 0.35f;
    [SerializeField] private float minAlpha = 0.25f;
    [SerializeField] private float maxAlpha = 0.7f;
    [SerializeField] private bool updateCollider = true;
    [SerializeField] private bool requirePlantedLanternToCut = true;

    public bool RequiresPlantedLanternToCut => requirePlantedLanternToCut;
    public bool CanCutNow => !requirePlantedLanternToCut || lanternController != null && lanternController.IsLanternPlanted;

    private void Awake()
    {
        CacheReferences();
        ConfigureRenderer();
    }

    private void Update()
    {
        CacheReferences();

        if (shadowInteractable != null && shadowInteractable.IsCut)
        {
            return;
        }

        Transform activeLight = ResolveLightTransform();
        if (activeLight == null || casterTransform == null)
        {
            return;
        }

        UpdateShadowShape(activeLight.position, casterTransform.position);
    }

    private Transform ResolveLightTransform()
    {
        if (lanternController != null && lanternController.LightPoint != null)
        {
            return lanternController.LightPoint;
        }

        return lightTransform;
    }

    private void UpdateShadowShape(Vector3 lightPosition, Vector3 casterPosition)
    {
        Vector2 fromLightToCaster = casterPosition - lightPosition;
        if (fromLightToCaster.sqrMagnitude < 0.0001f)
        {
            fromLightToCaster = Vector2.right;
        }

        Vector2 shadowDirection = fromLightToCaster.normalized;
        float distance = Mathf.Clamp(fromLightToCaster.magnitude, minDistance, maxDistance);
        float t = Mathf.InverseLerp(minDistance, maxDistance, distance);
        float length = Mathf.Lerp(maxLength, minLength, t);
        float alpha = Mathf.Lerp(maxAlpha, minAlpha, t);

        Vector3 shadowCenter = casterPosition + (Vector3)(shadowDirection * (length * 0.5f));
        shadowCenter.z = transform.position.z;
        transform.position = shadowCenter;

        float angle = Mathf.Atan2(shadowDirection.y, shadowDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        transform.localScale = Vector3.one;

        if (shadowRenderer != null)
        {
            shadowRenderer.drawMode = SpriteDrawMode.Sliced;
            shadowRenderer.size = new Vector2(length, shadowWidth);
            Color color = shadowRenderer.color;
            color.a = alpha;
            shadowRenderer.color = color;
        }

        if (updateCollider && shadowCollider != null)
        {
            shadowCollider.size = new Vector2(length, shadowWidth);
            shadowCollider.offset = Vector2.zero;
            shadowCollider.isTrigger = true;
        }
    }

    private void CacheReferences()
    {
        if (lanternController == null)
        {
            lanternController = FindObjectOfType<PlayerLanternController>();
        }

        if (casterTransform == null)
        {
            GameObject tree = GameObject.Find("Tree");
            if (tree == null)
            {
                tree = GameObject.Find("World/Tree");
            }

            if (tree != null)
            {
                casterTransform = tree.transform;
            }
        }

        if (shadowInteractable == null)
        {
            shadowInteractable = GetComponent<ShadowInteractable>();
        }

        if (shadowRenderer == null)
        {
            shadowRenderer = GetComponent<SpriteRenderer>();
        }

        if (shadowCollider == null)
        {
            shadowCollider = GetComponent<BoxCollider2D>();
        }
    }

    private void ConfigureRenderer()
    {
        if (shadowRenderer != null)
        {
            shadowRenderer.drawMode = SpriteDrawMode.Sliced;
        }
    }

    private void OnValidate()
    {
        minDistance = Mathf.Max(0.01f, minDistance);
        maxDistance = Mathf.Max(minDistance + 0.01f, maxDistance);
        minLength = Mathf.Max(0.01f, minLength);
        maxLength = Mathf.Max(minLength, maxLength);
        shadowWidth = Mathf.Max(0.01f, shadowWidth);
        minAlpha = Mathf.Clamp01(minAlpha);
        maxAlpha = Mathf.Clamp01(maxAlpha);
    }
}
