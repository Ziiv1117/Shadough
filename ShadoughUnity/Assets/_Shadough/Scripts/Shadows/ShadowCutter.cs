using UnityEngine;

[RequireComponent(typeof(ShadowInventory))]
public class ShadowCutter : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float cutRange = 2f;
    [SerializeField] private LayerMask shadowLayer = ~0;

    [Header("Input")]
    [SerializeField] private KeyCode cutKey = KeyCode.E;

    [Header("Prompt")]
    [SerializeField] private bool showDebugPrompt = true;
    [SerializeField] private Vector2 promptPosition = new Vector2(24f, 24f);

    [Header("Selection Outline")]
    [SerializeField] private bool showSelectionOutline = true;
    [SerializeField] private LineRenderer selectionOutline;
    [SerializeField] private float outlineWidth = 0.04f;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private float outlinePadding = 0.08f;

    private ShadowInventory inventory;
    private ShadowInteractable currentTarget;
    private PastedShadowObject currentPastedTarget;
    private string promptText;
    private float promptUntilTime;

    private void Awake()
    {
        inventory = GetComponent<ShadowInventory>();
        EnsureSelectionOutline();
    }

    private void Update()
    {
        FindNearestCuttableTarget();
        UpdateSelectionOutline();
        UpdatePromptText();

        if ((currentTarget != null || currentPastedTarget != null) && Input.GetKeyDown(cutKey))
        {
            TryCutCurrentTarget();
        }
    }

    private void OnDisable()
    {
        if (selectionOutline != null)
        {
            selectionOutline.enabled = false;
        }
    }

    private void OnGUI()
    {
        if (!showDebugPrompt || string.IsNullOrEmpty(promptText))
        {
            return;
        }

        GUI.Label(new Rect(promptPosition.x, promptPosition.y, 260f, 32f), promptText);
    }

    private void FindNearestCuttableTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, cutRange, shadowLayer);
        currentTarget = null;
        currentPastedTarget = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            ShadowInteractable shadow = hits[i].GetComponent<ShadowInteractable>();
            if (shadow != null && shadow.CanBeCut)
            {
                float distanceSqr = (shadow.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    currentTarget = shadow;
                    currentPastedTarget = null;
                    nearestDistanceSqr = distanceSqr;
                }
            }

            PastedShadowObject pastedShadow = hits[i].GetComponent<PastedShadowObject>();
            if (pastedShadow == null)
            {
                pastedShadow = hits[i].GetComponentInParent<PastedShadowObject>();
            }

            if (pastedShadow != null)
            {
                float distanceSqr = (pastedShadow.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    currentTarget = null;
                    currentPastedTarget = pastedShadow;
                    nearestDistanceSqr = distanceSqr;
                }
            }
        }
    }

    private void TryCutCurrentTarget()
    {
        if (!inventory.CanCarry())
        {
            ShowTemporaryPrompt("Shadow slot full", 1.2f);
            return;
        }

        if (currentPastedTarget != null)
        {
            TryCutPastedTarget();
            return;
        }

        ShadowType type = currentTarget.ShadowType;
        if (type == ShadowType.None)
        {
            return;
        }

        ShadowItemData itemData = currentTarget.CreateItemData();
        if (!currentTarget.Cut())
        {
            ShowTemporaryPrompt("Cannot cut shadow", 1.2f);
            return;
        }

        if (!inventory.PickUpShadow(itemData))
        {
            currentTarget.Restore();
            ShowTemporaryPrompt("Shadow slot full", 1.2f);
            return;
        }

        ShowTemporaryPrompt("Cut " + type, 1.2f);
    }

    private void TryCutPastedTarget()
    {
        ShadowItemData itemData = currentPastedTarget.CreateItemData();
        if (itemData == null || !itemData.IsValid())
        {
            ShowTemporaryPrompt("Cannot cut shadow", 1.2f);
            return;
        }

        if (!inventory.PickUpShadow(itemData))
        {
            ShowTemporaryPrompt("Shadow slot full", 1.2f);
            return;
        }

        ShadowType type = itemData.shadowType;
        Destroy(currentPastedTarget.gameObject);
        currentPastedTarget = null;
        ShowTemporaryPrompt("Cut " + type, 1.2f);
    }

    private void UpdatePromptText()
    {
        if (Time.time < promptUntilTime)
        {
            return;
        }

        promptText = currentTarget != null || currentPastedTarget != null ? "Press E to Cut" : string.Empty;
    }

    private void ShowTemporaryPrompt(string text, float duration)
    {
        promptText = text;
        promptUntilTime = Time.time + duration;
    }

    private void EnsureSelectionOutline()
    {
        if (selectionOutline == null)
        {
            selectionOutline = GetComponent<LineRenderer>();
        }

        if (selectionOutline == null)
        {
            selectionOutline = gameObject.AddComponent<LineRenderer>();
        }

        Material material = selectionOutline.sharedMaterial;
        if (material == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                material = new Material(shader);
                selectionOutline.sharedMaterial = material;
            }
        }

        selectionOutline.positionCount = 5;
        selectionOutline.startWidth = outlineWidth;
        selectionOutline.endWidth = outlineWidth;
        selectionOutline.startColor = outlineColor;
        selectionOutline.endColor = outlineColor;
        selectionOutline.useWorldSpace = true;
        selectionOutline.loop = false;
        selectionOutline.enabled = false;
    }

    private void UpdateSelectionOutline()
    {
        if (selectionOutline == null || !showSelectionOutline)
        {
            if (selectionOutline != null)
            {
                selectionOutline.enabled = false;
            }
            return;
        }

        Component target = currentTarget != null ? currentTarget : currentPastedTarget;
        bool hasTarget = target != null;
        selectionOutline.enabled = hasTarget;

        if (!hasTarget)
        {
            return;
        }

        Bounds bounds = GetTargetBounds(target);
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        min.x -= outlinePadding;
        min.y -= outlinePadding;
        max.x += outlinePadding;
        max.y += outlinePadding;

        selectionOutline.positionCount = 5;
        selectionOutline.SetPosition(0, new Vector3(min.x, min.y, target.transform.position.z));
        selectionOutline.SetPosition(1, new Vector3(min.x, max.y, target.transform.position.z));
        selectionOutline.SetPosition(2, new Vector3(max.x, max.y, target.transform.position.z));
        selectionOutline.SetPosition(3, new Vector3(max.x, min.y, target.transform.position.z));
        selectionOutline.SetPosition(4, new Vector3(min.x, min.y, target.transform.position.z));
        selectionOutline.startWidth = outlineWidth;
        selectionOutline.endWidth = outlineWidth;
        selectionOutline.startColor = outlineColor;
        selectionOutline.endColor = outlineColor;
    }

    private Bounds GetTargetBounds(Component target)
    {
        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }

        Collider2D collider = target.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds;
        }

        return new Bounds(target.transform.position, Vector3.one);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, cutRange);
    }
}
