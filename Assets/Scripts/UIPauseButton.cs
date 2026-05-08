using UnityEngine;

public class UIPauseButton : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] private GameObject targetObject;
    [SerializeField] private string pauseMethodName;

    [Header("Sprites")]
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite resumeSprite;

    [Header("Mouse Feedback")]
    private Color defaultColor;
    private Vector3 defaultButtonScale;
    [SerializeField] private Color highlightColor = Color.cyan;
    [SerializeField] private Vector3 buttonPressScale = new Vector3(0.2f, 0.2f, 1.0f);

    private SpriteRenderer _spriteRenderer;
    private bool _isPaused = false;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        defaultColor = _spriteRenderer.color;
        defaultButtonScale = transform.localScale;

        // Make sure the pause sprite is the starting sprite
        if (pauseSprite != null)
            _spriteRenderer.sprite = pauseSprite;
    }

    private void OnMouseEnter()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.color = highlightColor;
    }

    private void OnMouseExit()
    {
        if (_spriteRenderer != null)
            _spriteRenderer.color = defaultColor;
    }

    private void OnMouseDown()
    {

    }

    private void OnMouseUp()
    {
        transform.localScale = defaultButtonScale;

        if (targetObject != null)
            targetObject.SendMessage(pauseMethodName);

        _isPaused = !_isPaused;
        _spriteRenderer.sprite = _isPaused ? resumeSprite : pauseSprite;
    }
}