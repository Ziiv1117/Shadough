using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PastedShadowVisualResolver : MonoBehaviour
{
    [SerializeField] private Sprite treePastedSprite;
    [SerializeField] private Sprite beamPastedSprite;
    [SerializeField] private Sprite keyPastedSprite;
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

        SpriteRenderer renderer = pastedShadow.SpriteRenderer;
        Sprite pastedSprite = ResolvePastedSprite(pastedShadow.SourceData);
        if (pastedSprite == null || renderer == null)
        {
            return;
        }

        renderer.sprite = pastedSprite;
        renderer.color = Color.white;
    }

    private Sprite ResolvePastedSprite(ShadowItemData data)
    {
        if (MatchesSource(data, "TreeShadow", "tree_shadow"))
        {
            return treePastedSprite;
        }

        if (MatchesSource(data, "BeamShadow", "beam_shadow"))
        {
            return beamPastedSprite;
        }

        if (MatchesSource(data, "KeyShadow", "key_shadow"))
        {
            return keyPastedSprite;
        }

        return null;
    }

    private static bool MatchesSource(ShadowItemData data, string sourceToken, string spriteToken)
    {
        if (data == null)
        {
            return false;
        }

        string sourceName = data.sourceInteractable != null ? data.sourceInteractable.name : string.Empty;
        string spriteName = data.sprite != null ? data.sprite.name : string.Empty;
        return sourceName.IndexOf(sourceToken, System.StringComparison.OrdinalIgnoreCase) >= 0
            || spriteName.IndexOf(spriteToken, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private void OnValidate()
    {
        refreshInterval = Mathf.Max(0.02f, refreshInterval);
    }
}
