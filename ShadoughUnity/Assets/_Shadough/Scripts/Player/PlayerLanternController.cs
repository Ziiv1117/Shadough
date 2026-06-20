using System;
using System.Reflection;
using UnityEngine;

public class PlayerLanternController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleLanternKey = KeyCode.G;

    [Header("State")]
    [SerializeField] private bool isLanternPlanted;
    [SerializeField] private Transform heldLanternPoint;
    [SerializeField] private GameObject lanternObject;
    [SerializeField] private Transform lightPoint;
    [SerializeField] private float retrieveRange = 1.5f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer lanternRenderer;
    [SerializeField] private Color heldColor = new Color(1f, 0.86f, 0.45f, 1f);
    [SerializeField] private Color plantedColor = new Color(1f, 0.62f, 0.22f, 1f);

    public bool IsLanternPlanted => isLanternPlanted;
    public Transform LightPoint => lightPoint != null ? lightPoint : lanternObject != null ? lanternObject.transform : null;

    private void Awake()
    {
        EnsureHeldLanternPoint();
        EnsureLanternObject();
        SetHeldState();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleLanternKey))
        {
            ToggleLantern();
        }

        if (!isLanternPlanted)
        {
            FollowHeldPoint();
        }
    }

    private void ToggleLantern()
    {
        if (!isLanternPlanted)
        {
            PlantLantern();
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

    private void PlantLantern()
    {
        isLanternPlanted = true;
        if (lanternObject != null)
        {
            lanternObject.transform.SetParent(GetLightsRoot(), true);
        }

        ApplyVisualState();
        Debug.Log("Lantern planted");
    }

    private void RetrieveLantern()
    {
        SetHeldState();
        Debug.Log("Lantern retrieved");
    }

    private void SetHeldState()
    {
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

        lanternObject.transform.SetParent(heldLanternPoint, false);
        lanternObject.transform.localPosition = Vector3.zero;
        lanternObject.transform.localRotation = Quaternion.identity;
        lanternObject.transform.localScale = Vector3.one;
    }

    private void EnsureHeldLanternPoint()
    {
        if (heldLanternPoint != null)
        {
            return;
        }

        Transform existingPoint = transform.Find("HeldLanternPoint");
        if (existingPoint != null)
        {
            heldLanternPoint = existingPoint;
            return;
        }

        GameObject pointObject = new GameObject("HeldLanternPoint");
        pointObject.transform.SetParent(transform, false);
        pointObject.transform.localPosition = new Vector3(0.7f, 0.2f, 0f);
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
        if (light2DType == null || target.GetComponent(light2DType) != null)
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
        if (lanternRenderer != null)
        {
            lanternRenderer.color = isLanternPlanted ? plantedColor : heldColor;
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
    }
}
