using UnityEngine;

[RequireComponent(typeof(ShadowInventory))]
public class PlayerSelfShadowCutter : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode selfCutKey = KeyCode.Q;

    [Header("Player Shadow")]
    [SerializeField] private Sprite playerShadowSprite;
    [SerializeField] private Vector3 shadowScale = new Vector3(1.2f, 1.8f, 1f);
    [SerializeField] private Vector2 colliderSize = new Vector2(1f, 1.6f);
    [SerializeField] private Vector2 colliderOffset = Vector2.zero;
    [SerializeField] private float cooldown;

    private ShadowInventory inventory;
    private Sprite fallbackSprite;
    private float nextAllowedCutTime;
    private bool loggedMissingRevealController;

    private void Awake()
    {
        inventory = GetComponent<ShadowInventory>();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(selfCutKey))
        {
            return;
        }

        TryCutSelfShadow();
    }

    private void TryCutSelfShadow()
    {
        if (inventory == null)
        {
            return;
        }

        if (!CanCutSelfInRevealView())
        {
            Debug.Log("Hold Shift to reveal your shadow");
            return;
        }

        if (inventory.HasShadow())
        {
            Debug.Log("Shadow slot full");
            return;
        }

        if (Time.time < nextAllowedCutTime)
        {
            return;
        }

        ShadowItemData data = CreateSelfShadowData();
        if (!inventory.PickUpShadow(data))
        {
            Debug.Log("Shadow slot full");
            return;
        }

        nextAllowedCutTime = Time.time + cooldown;
        Debug.Log("Cut self shadow");
    }

    private bool CanCutSelfInRevealView()
    {
        if (RevealViewController.HasInstance)
        {
            return RevealViewController.IsActive;
        }

        if (!loggedMissingRevealController)
        {
            Debug.Log("RevealViewController not found. Cannot cut self shadow without Reveal View.");
            loggedMissingRevealController = true;
        }

        return false;
    }

    private ShadowItemData CreateSelfShadowData()
    {
        return new ShadowItemData
        {
            shadowType = ShadowType.Player,
            displayName = "Player Shadow",
            sprite = playerShadowSprite != null ? playerShadowSprite : GetFallbackSprite(),
            spriteDrawMode = SpriteDrawMode.Simple,
            spriteSize = colliderSize,
            localScale = shadowScale,
            rotation = Quaternion.identity,
            approximateSize = colliderSize,
            colliderSize = colliderSize,
            colliderOffset = colliderOffset,
            canStandOn = false,
            canPress = false,
            canUnlock = false,
            canAttractEnemy = true,
            canBlock = false,
            canTriggerMechanism = false
        };
    }

    private Sprite GetFallbackSprite()
    {
        if (fallbackSprite != null)
        {
            return fallbackSprite;
        }

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        fallbackSprite.name = "PlayerShadow_FallbackSprite";
        return fallbackSprite;
    }

    private void OnValidate()
    {
        shadowScale.x = Mathf.Max(0.01f, shadowScale.x);
        shadowScale.y = Mathf.Max(0.01f, shadowScale.y);
        shadowScale.z = Mathf.Max(0.01f, shadowScale.z);
        colliderSize.x = Mathf.Max(0.01f, colliderSize.x);
        colliderSize.y = Mathf.Max(0.01f, colliderSize.y);
        cooldown = Mathf.Max(0f, cooldown);
    }
}
