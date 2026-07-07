using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class TutorialSign : MonoBehaviour
{
    private static Sprite generatedSprite;

    [SerializeField] private string signTitle = string.Empty;
    [SerializeField, TextArea(2, 5)] private string promptText = string.Empty;
    [SerializeField] private Color signColor = new Color(0.72f, 0.64f, 0.42f, 1f);

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        EnsureVisual();
    }

    private void OnMouseDown()
    {
        TutorialSignPromptController.ShowPrompt(signTitle, promptText);
    }

    public void Configure(string title, string text)
    {
        signTitle = title;
        promptText = text;
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = signColor;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = signColor;
        Gizmos.DrawCube(transform.position, new Vector3(0.8f, 0.55f, 0.05f));
    }

    private void EnsureVisual()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            return;
        }

        if (generatedSprite == null)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            generatedSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            generatedSprite.name = "GeneratedTutorialSignSprite";
        }

        if (spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = generatedSprite;
        }

        spriteRenderer.color = signColor;
    }
}
