using UnityEngine;

// MemoryCard: Represents a playable card in the memory game scene.
// Handles visual state (face/back), user interaction, and communicates selection events to the SceneController.
public class MainCard : MonoBehaviour
{
    [SerializeField] private SceneController sceneController;
    [SerializeField] private GameObject cardBack;
    private SpriteRenderer _spriteRenderer;
    private int _id;
    private bool _interactable = false;

    public int Id => _id;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetVisible(bool visible)
    {
        _spriteRenderer.enabled = visible;
        cardBack.SetActive(visible);
        _interactable = visible;
    }

    public void OnMouseDown()
    {
        if (!_interactable || sceneController.IsPaused) return;
        if (cardBack.activeSelf && sceneController.CanReveal)
        {
            cardBack.SetActive(false);
            sceneController.RevealCard(this);
        }
    }

    public void Unreveal()
    {
        cardBack.SetActive(true);
    }

    public void SetUpCard(CardData cardData)
    {
        _id = cardData.id;

        Texture2D tex = cardData.face;

        float scale = 780f / tex.height;
        int scaledWidth = Mathf.RoundToInt(tex.width * scale);
        int scaledHeight = 780;

        RenderTexture rt = RenderTexture.GetTemporary(scaledWidth, scaledHeight, 0);
        Graphics.Blit(tex, rt);
        RenderTexture.active = rt;

        Texture2D scaled = new Texture2D(scaledWidth, scaledHeight);
        scaled.ReadPixels(new Rect(0, 0, scaledWidth, scaledHeight), 0, 0);
        scaled.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        int cropX = Mathf.Max(0, (scaledWidth - 520) / 2);
        int cropWidth = Mathf.Min(520, scaledWidth);

        Sprite sprite = Sprite.Create(
            scaled,
            new Rect(cropX, 0, cropWidth, scaledHeight),
            new Vector2(0.5f, 0.5f)
        );

        _spriteRenderer.sprite = sprite;
    }
}
