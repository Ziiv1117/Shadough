using UnityEngine;

[RequireComponent(typeof(ShadowInventory))]
public class FreeShadowPlacer : MonoBehaviour
{
    [Header("Placement")]
    [SerializeField] private float placementRadius = 2.5f;
    [SerializeField] private KeyCode pasteKey = KeyCode.F;
    [SerializeField] private Transform effectParent;

    [Header("Paste Area Conflict")]
    [SerializeField] private bool blockNearPasteArea = true;
    [SerializeField] private float pasteAreaCheckRadius = 0.3f;

    [Header("Visuals")]
    [SerializeField] private bool showRangeCircle = true;
    [SerializeField] private int circleSegments = 64;
    [SerializeField] private float circleLineWidth = 0.03f;
    [SerializeField] private Color circleColor = new Color(1f, 1f, 1f, 0.35f);
    [SerializeField] private Color previewColor = new Color(0f, 0f, 0f, 0.45f);

    private ShadowInventory inventory;
    private Camera mainCamera;
    private LineRenderer rangeCircle;
    private SpriteRenderer previewRenderer;
    private ShadowItemData currentPreviewData;
    private Vector3 currentPreviewPosition;
    private Quaternion currentPreviewRotation = Quaternion.identity;
    private Sprite fallbackSprite;

    private void Awake()
    {
        inventory = GetComponent<ShadowInventory>();
        mainCamera = Camera.main;
        fallbackSprite = CreateRuntimeSprite();
        EnsureRangeCircle();
        EnsurePreviewObject();
        HidePlacementVisuals();
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (inventory == null || !inventory.HasShadow())
        {
            if (Input.GetKeyDown(pasteKey))
            {
                TutorialFailurePromptController.Show("No shadow to paste.");
            }

            currentPreviewData = null;
            HidePlacementVisuals();
            return;
        }

        currentPreviewData = inventory.CurrentShadowData;
        UpdatePreviewTransform();
        ShowPlacementVisuals();

        if (Input.GetKeyDown(pasteKey))
        {
            TryPlaceShadow();
        }
    }

    private void UpdatePreviewTransform()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        Vector3 fromPlayerToMouse = mouseWorldPosition - transform.position;
        fromPlayerToMouse.z = 0f;

        Vector3 direction = fromPlayerToMouse.sqrMagnitude > 0.0001f
            ? fromPlayerToMouse.normalized
            : transform.right;

        float distance = Mathf.Min(fromPlayerToMouse.magnitude, placementRadius);
        currentPreviewPosition = transform.position + direction * distance;
        currentPreviewPosition.z = 0f;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        currentPreviewRotation = Quaternion.Euler(0f, 0f, angle);

        if (previewRenderer != null && currentPreviewData != null)
        {
            previewRenderer.transform.position = currentPreviewPosition;
            previewRenderer.transform.rotation = currentPreviewRotation;
            previewRenderer.transform.localScale = currentPreviewData.localScale;
            previewRenderer.sprite = currentPreviewData.sprite != null ? currentPreviewData.sprite : fallbackSprite;
            ApplyRendererShape(previewRenderer, currentPreviewData);
        }
    }

    private void TryPlaceShadow()
    {
        if (inventory == null || !inventory.HasShadow())
        {
            return;
        }

        if (blockNearPasteArea && IsNearShadowPasteArea())
        {
            Debug.Log("FreeShadowPlacer skipped because Player is near a ShadowPasteArea. ShadowPasteArea handles F in special zones.");
            return;
        }

        ShadowItemData shadowData = inventory.ConsumeShadowData();
        if (shadowData == null || !shadowData.IsValid())
        {
            return;
        }

        RemoveExistingPlayerShadowIfNeeded(shadowData);
        CreatePastedShadowObject(shadowData, currentPreviewPosition, currentPreviewRotation);
        Debug.Log("Placed free shadow: " + shadowData.shadowType + " at " + currentPreviewPosition);
    }

    private void RemoveExistingPlayerShadowIfNeeded(ShadowItemData data)
    {
        if (data.shadowType != ShadowType.Player)
        {
            return;
        }

        PastedShadowObject[] pastedShadows = FindObjectsOfType<PastedShadowObject>();
        for (int i = 0; i < pastedShadows.Length; i++)
        {
            PastedShadowObject pastedShadow = pastedShadows[i];
            if (pastedShadow != null && pastedShadow.ShadowType == ShadowType.Player)
            {
                Destroy(pastedShadow.gameObject);
            }
        }
    }

    private void CreatePastedShadowObject(ShadowItemData data, Vector3 position, Quaternion rotation)
    {
        Transform parent = ResolveEffectParent();
        GameObject pastedObject = new GameObject("PastedShadowObject_" + data.shadowType);
        pastedObject.transform.SetParent(parent, true);
        pastedObject.transform.position = position;
        pastedObject.transform.rotation = rotation;
        pastedObject.transform.localScale = data.localScale;

        SpriteRenderer renderer = pastedObject.AddComponent<SpriteRenderer>();
        renderer.sprite = data.sprite != null ? data.sprite : fallbackSprite;
        ApplyRendererShape(renderer, data);
        renderer.color = new Color(0f, 0f, 0f, 0.65f);
        renderer.sortingOrder = 20;

        BoxCollider2D collider = pastedObject.AddComponent<BoxCollider2D>();
        collider.size = data.colliderSize;
        collider.offset = data.colliderOffset;
        collider.isTrigger = !data.canStandOn;

        PastedShadowObject pastedShadow = pastedObject.AddComponent<PastedShadowObject>();
        pastedShadow.Initialize(data);
    }

    private void ApplyRendererShape(SpriteRenderer renderer, ShadowItemData data)
    {
        renderer.drawMode = data.spriteDrawMode;

        if (data.spriteDrawMode == SpriteDrawMode.Sliced || data.spriteDrawMode == SpriteDrawMode.Tiled)
        {
            Vector2 size = data.spriteSize;
            if (size.x <= 0f || size.y <= 0f)
            {
                size = data.colliderSize;
            }

            renderer.size = size;
        }
    }

    private bool IsNearShadowPasteArea()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pasteAreaCheckRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].GetComponent<ShadowPasteArea>() != null || hits[i].GetComponentInParent<ShadowPasteArea>() != null)
            {
                return true;
            }
        }

        return false;
    }

    private Transform ResolveEffectParent()
    {
        if (effectParent != null)
        {
            return effectParent;
        }

        GameObject shadowVisuals = GameObject.Find("ShadowVisuals");
        if (shadowVisuals != null)
        {
            effectParent = shadowVisuals.transform;
            return effectParent;
        }

        return null;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            return transform.position;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    private void EnsureRangeCircle()
    {
        GameObject circleObject = new GameObject("FreeShadowPlace_RangeCircle");
        circleObject.transform.SetParent(transform, false);

        rangeCircle = circleObject.AddComponent<LineRenderer>();
        rangeCircle.useWorldSpace = false;
        rangeCircle.loop = true;
        rangeCircle.positionCount = Mathf.Max(8, circleSegments);
        rangeCircle.startWidth = circleLineWidth;
        rangeCircle.endWidth = circleLineWidth;
        rangeCircle.startColor = circleColor;
        rangeCircle.endColor = circleColor;
        rangeCircle.sortingOrder = 30;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            rangeCircle.sharedMaterial = new Material(shader);
        }

        RebuildRangeCircle();
    }

    private void RebuildRangeCircle()
    {
        if (rangeCircle == null)
        {
            return;
        }

        int segmentCount = Mathf.Max(8, circleSegments);
        rangeCircle.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / segmentCount;
            float angle = t * Mathf.PI * 2f;
            Vector3 point = new Vector3(Mathf.Cos(angle) * placementRadius, Mathf.Sin(angle) * placementRadius, 0f);
            rangeCircle.SetPosition(i, point);
        }
    }

    private void EnsurePreviewObject()
    {
        GameObject previewObject = new GameObject("FreeShadowPlace_Preview");
        previewObject.transform.SetParent(transform, false);

        previewRenderer = previewObject.AddComponent<SpriteRenderer>();
        previewRenderer.sprite = fallbackSprite;
        previewRenderer.color = previewColor;
        previewRenderer.sortingOrder = 31;
    }

    private Sprite CreateRuntimeSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    private void ShowPlacementVisuals()
    {
        if (rangeCircle != null)
        {
            rangeCircle.enabled = showRangeCircle;
            rangeCircle.startWidth = circleLineWidth;
            rangeCircle.endWidth = circleLineWidth;
            rangeCircle.startColor = circleColor;
            rangeCircle.endColor = circleColor;

            if (rangeCircle.positionCount != Mathf.Max(8, circleSegments))
            {
                RebuildRangeCircle();
            }
        }

        if (previewRenderer != null)
        {
            previewRenderer.enabled = true;
            previewRenderer.color = previewColor;
        }
    }

    private void HidePlacementVisuals()
    {
        if (rangeCircle != null)
        {
            rangeCircle.enabled = false;
        }

        if (previewRenderer != null)
        {
            previewRenderer.enabled = false;
        }
    }

    private void OnValidate()
    {
        placementRadius = Mathf.Max(0.1f, placementRadius);
        circleSegments = Mathf.Max(8, circleSegments);
        circleLineWidth = Mathf.Max(0.001f, circleLineWidth);
        pasteAreaCheckRadius = Mathf.Max(0.05f, pasteAreaCheckRadius);
    }
}
