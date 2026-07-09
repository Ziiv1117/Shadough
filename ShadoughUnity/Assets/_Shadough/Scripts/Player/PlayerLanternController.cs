using System;
using System.Reflection;
using UnityEngine;

public class PlayerLanternController : MonoBehaviour
{
    private enum FacingDirection
    {
        Down,
        Left,
        Right,
        Up
    }

    [Header("Input")]
    [SerializeField] private KeyCode toggleLanternKey = KeyCode.G;

    [Header("State")]
    [SerializeField] private bool isLanternPlanted;
    [SerializeField] private Transform heldLanternPoint;
    [SerializeField] private GameObject lanternObject;
    [SerializeField] private Transform lightPoint;
    [SerializeField] private float retrieveRange = 0.8f;
    [SerializeField] private float placeAnimationDelay = 0.38f;

    [Header("Held Offsets")]
    [SerializeField] private Vector2 heldLanternOffsetUp = new Vector2(0.48f, -0.42f);
    [SerializeField] private Vector2 heldLanternOffsetDown = new Vector2(-0.62f, -0.4f);
    [SerializeField] private Vector2 heldLanternOffsetLeft = new Vector2(-0.58f, -0.4f);
    [SerializeField] private Vector2 heldLanternOffsetRight = new Vector2(0.18f, -0.4f);

    [Header("Placed Offsets")]
    [SerializeField] private Vector2 placedLanternOffsetUp = new Vector2(0.58f, -0.36f);
    [SerializeField] private Vector2 placedLanternOffsetDown = new Vector2(-0.52f, -0.36f);
    [SerializeField] private Vector2 placedLanternOffsetLeft = new Vector2(-0.48f, -0.36f);
    [SerializeField] private Vector2 placedLanternOffsetRight = new Vector2(0.34f, -0.36f);

    [Header("Visual")]
    [SerializeField] private SpriteRenderer lanternRenderer;
    [SerializeField] private Collider2D lanternCollider;
    [SerializeField] private Color heldColor = new Color(1f, 0.86f, 0.45f, 1f);
    [SerializeField] private Color plantedColor = new Color(1f, 0.62f, 0.22f, 1f);

    [Header("Facing")]
    [SerializeField] private float moveThreshold = 0.01f;

    private FacingDirection facingDirection = FacingDirection.Down;
    private FacingDirection pendingPlaceDirection = FacingDirection.Down;
    private bool isPlacingLantern;
    private float placeCompleteTime;

    public bool IsLanternPlanted => isLanternPlanted;
    public bool IsPlacingLantern => isPlacingLantern;
    public Transform LightPoint => lightPoint != null ? lightPoint : lanternObject != null ? lanternObject.transform : null;

    private void Awake()
    {
        EnsureHeldLanternPoint();
        EnsureLanternObject();
        SetHeldState();
    }

    private void Update()
    {
        UpdateFacingDirection();

        if (Input.GetKeyDown(toggleLanternKey))
        {
            ToggleLantern();
        }

        if (isPlacingLantern && Time.time >= placeCompleteTime)
        {
            CompletePlantLantern();
        }

        if (!isLanternPlanted)
        {
            FollowHeldPoint();
        }
    }

    private void ToggleLantern()
    {
        if (isPlacingLantern)
        {
            return;
        }

        if (!isLanternPlanted)
        {
            BeginPlantLantern();
            return;
        }

        if (lanternObject == null)
        {
            SetHeldState();
            return;
        }

        float distance = Vector2.Distance(transform.position, lanternObject.transform.position);
        if (distance > retrieveRange)
        {
            Debug.Log("Move closer to retrieve lantern");
            return;
        }

        RetrieveLantern();
    }

    private void BeginPlantLantern()
    {
        isPlacingLantern = true;
        pendingPlaceDirection = facingDirection;
        placeCompleteTime = Time.time + placeAnimationDelay;
        FollowHeldPoint();
        ApplyVisualState();

        if (placeAnimationDelay <= 0f)
        {
            CompletePlantLantern();
        }
    }

    private void CompletePlantLantern()
    {
        if (!isPlacingLantern && isLanternPlanted)
        {
            return;
        }

        isPlacingLantern = false;
        isLanternPlanted = true;
        if (lanternObject != null)
        {
            Vector3 plantedPosition = transform.position + (Vector3)GetPlacedLanternOffset(pendingPlaceDirection);
            plantedPosition.z = lanternObject.transform.position.z;
            lanternObject.transform.SetParent(GetLightsRoot(), true);
            lanternObject.transform.position = plantedPosition;
            lanternObject.transform.rotation = Quaternion.identity;
            lanternObject.transform.localScale = Vector3.one;
        }

        ApplyVisualState();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.lantern);
        }

        Debug.Log("Lantern planted");
    }

    private void RetrieveLantern()
    {
        SetHeldState();
        Debug.Log("Lantern retrieved");
    }

    private void SetHeldState()
    {
        isPlacingLantern = false;
        isLanternPlanted = false;
        FollowHeldPoint();
        ApplyVisualState();
    }

    private void FollowHeldPoint()
    {
        if (lanternObject == null || heldLanternPoint == null)
        {
            return;
        }

        heldLanternPoint.localPosition = GetHeldLanternOffset();
        lanternObject.transform.SetParent(heldLanternPoint, false);
        lanternObject.transform.localPosition = Vector3.zero;
        lanternObject.transform.localRotation = Quaternion.identity;
        lanternObject.transform.localScale = Vector3.one;
    }

    private void UpdateFacingDirection()
    {
        Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (moveInput.sqrMagnitude <= moveThreshold * moveThreshold)
        {
            return;
        }

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            facingDirection = moveInput.x < 0f ? FacingDirection.Left : FacingDirection.Right;
        }
        else
        {
            facingDirection = moveInput.y < 0f ? FacingDirection.Down : FacingDirection.Up;
        }
    }

    private Vector2 GetHeldLanternOffset()
    {
        switch (facingDirection)
        {
            case FacingDirection.Up:
                return heldLanternOffsetUp;
            case FacingDirection.Left:
                return heldLanternOffsetLeft;
            case FacingDirection.Right:
                return heldLanternOffsetRight;
            default:
                return heldLanternOffsetDown;
        }
    }

    private Vector2 GetPlacedLanternOffset()
    {
        return GetPlacedLanternOffset(facingDirection);
    }

    private Vector2 GetPlacedLanternOffset(FacingDirection direction)
    {
        switch (direction)
        {
            case FacingDirection.Up:
                return placedLanternOffsetUp;
            case FacingDirection.Left:
                return placedLanternOffsetLeft;
            case FacingDirection.Right:
                return placedLanternOffsetRight;
            default:
                return placedLanternOffsetDown;
        }
    }

    private void EnsureHeldLanternPoint()
    {
        if (heldLanternPoint != null)
        {
            heldLanternPoint.localPosition = GetHeldLanternOffset();
            return;
        }

        Transform existingPoint = transform.Find("HeldLanternPoint");
        if (existingPoint != null)
        {
            heldLanternPoint = existingPoint;
            heldLanternPoint.localPosition = GetHeldLanternOffset();
            return;
        }

        GameObject pointObject = new GameObject("HeldLanternPoint");
        pointObject.transform.SetParent(transform, false);
        pointObject.transform.localPosition = GetHeldLanternOffset();
        heldLanternPoint = pointObject.transform;
    }

    private void EnsureLanternObject()
    {
        if (lanternObject == null)
        {
            GameObject existingLantern = GameObject.Find("PlayerLantern");
            lanternObject = existingLantern != null ? existingLantern : new GameObject("PlayerLantern");
        }

        if (lanternRenderer == null)
        {
            lanternRenderer = lanternObject.GetComponent<SpriteRenderer>();
        }

        if (lanternRenderer == null)
        {
            lanternRenderer = lanternObject.AddComponent<SpriteRenderer>();
            lanternRenderer.sprite = CreateRuntimeSprite();
            lanternRenderer.drawMode = SpriteDrawMode.Sliced;
            lanternRenderer.size = new Vector2(0.32f, 0.72f);
            lanternRenderer.sortingOrder = 18;
        }

        if (lanternCollider == null)
        {
            lanternCollider = lanternObject.GetComponent<Collider2D>();
        }

        if (lanternCollider == null)
        {
            BoxCollider2D boxCollider = lanternObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(0.35f, 0.8f);
            boxCollider.offset = new Vector2(0f, 0.4f);
            lanternCollider = boxCollider;
        }

        EnsureOptionalLight2D(lanternObject);

        if (lightPoint == null)
        {
            Transform existingLightPoint = lanternObject.transform.Find("LanternLightPoint");
            if (existingLightPoint != null)
            {
                lightPoint = existingLightPoint;
            }
            else
            {
                lightPoint = lanternObject.transform;
            }
        }
    }

    private void EnsureOptionalLight2D(GameObject target)
    {
        Type light2DType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (light2DType == null || target.GetComponentInChildren(light2DType, true) != null)
        {
            return;
        }

        Component lightComponent = target.AddComponent(light2DType);
        SetLightProperty(lightComponent, "lightType", 3);
        SetLightProperty(lightComponent, "color", new Color(1f, 0.88f, 0.62f, 1f));
        SetLightProperty(lightComponent, "intensity", 1.2f);
        SetLightProperty(lightComponent, "pointLightInnerRadius", 0.2f);
        SetLightProperty(lightComponent, "pointLightOuterRadius", 5f);
        SetLightProperty(lightComponent, "shadowsEnabled", true);
        SetLightProperty(lightComponent, "shadowIntensity", 0.85f);
    }

    private void SetLightProperty(Component component, string propertyName, object value)
    {
        if (component == null)
        {
            return;
        }

        PropertyInfo property = component.GetType().GetProperty(propertyName);
        if (property == null || !property.CanWrite)
        {
            return;
        }

        try
        {
            object convertedValue = value;
            if (property.PropertyType.IsEnum && value is int intValue)
            {
                convertedValue = Enum.ToObject(property.PropertyType, intValue);
            }

            property.SetValue(component, convertedValue, null);
        }
        catch
        {
            // Light2D property names can vary between URP versions; the visual lantern still works without them.
        }
    }

    private Transform GetLightsRoot()
    {
        GameObject lightsRoot = GameObject.Find("Lights");
        if (lightsRoot != null)
        {
            return lightsRoot.transform;
        }

        return null;
    }

    private void ApplyVisualState()
    {
        bool showPlacedLantern = isLanternPlanted && !isPlacingLantern;

        if (lanternRenderer != null)
        {
            lanternRenderer.enabled = showPlacedLantern;
            lanternRenderer.color = isLanternPlanted ? plantedColor : heldColor;
        }

        if (lanternCollider != null)
        {
            lanternCollider.enabled = showPlacedLantern;
        }

        SetLanternLightEnabled(true);
    }

    private void SetLanternLightEnabled(bool enabled)
    {
        if (lanternObject == null)
        {
            return;
        }

        Type light2DType = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (light2DType == null)
        {
            return;
        }

        Component[] lightComponents = lanternObject.GetComponentsInChildren(light2DType, true);
        for (int i = 0; i < lightComponents.Length; i++)
        {
            if (lightComponents[i] is Behaviour behaviour)
            {
                behaviour.enabled = enabled;
            }
        }
    }

    private Sprite CreateRuntimeSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    private void OnValidate()
    {
        retrieveRange = Mathf.Max(0.1f, retrieveRange);
        placeAnimationDelay = Mathf.Max(0f, placeAnimationDelay);
        moveThreshold = Mathf.Max(0.001f, moveThreshold);
    }
}
