using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PastedShadowVisualResolver : MonoBehaviour
{
    [SerializeField] private Sprite treePastedSprite;
    [SerializeField] private float refreshInterval = 0.1f;

    private readonly HashSet<int> configuredObjects = new HashSet<int>();
    private float nextRefreshTime;

    private void LateUpdate()
    {
        if (Time.unscaledTime < nextRefreshTime)
        {
            return;
        }

        nextRefreshTime = Time.unscaledTime + refreshInterval;
        PastedShadowObject[] pastedShadows = FindObjectsOfType<PastedShadowObject>();
        for (int i = 0; i < pastedShadows.Length; i++)
        {
            ApplyVisual(pastedShadows[i]);
        }
    }

    private void ApplyVisual(PastedShadowObject pastedShadow)
    {
        if (pastedShadow == null || !configuredObjects.Add(pastedShadow.GetInstanceID()))
        {
            return;
        }

        if (!IsPastedTreeShadow(pastedShadow.SourceData))
        {
            return;
        }

        SpriteRenderer renderer = pastedShadow.SpriteRenderer;
        if (treePastedSprite == null || renderer == null)
        {
            return;
        }

        renderer.sprite = treePastedSprite;
        renderer.color = Color.white;
    }

    private static bool IsPastedTreeShadow(ShadowItemData data)
    {
        if (data == null)
        {
            return false;
        }

        string sourceName = data.sourceInteractable != null ? data.sourceInteractable.name : string.Empty;
        string spriteName = data.sprite != null ? data.sprite.name : string.Empty;
        return sourceName.IndexOf("TreeShadow", System.StringComparison.OrdinalIgnoreCase) >= 0
            || spriteName.IndexOf("tree_shadow", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void OnValidate()
    {
        refreshInterval = Mathf.Max(0.02f, refreshInterval);
    }
}
