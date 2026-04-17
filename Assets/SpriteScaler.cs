using UnityEngine;

/// <summary>
/// Scales a sprite to a target world size, maintaining aspect ratio.
/// </summary>
[ExecuteInEditMode]
public class SpriteScaler : MonoBehaviour
{
    [Header("Target World Size")]
    [Tooltip("Desired width in world units")]
    public float targetWidth = 1f;

    [Tooltip("Desired height in world units")]
    public float targetHeight = 1f;

    [Header("Current Sprite Info (Read Only)")]
    public float currentWidth;
    public float currentHeight;
    public float pixelsPerUnit;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        var sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        if (sprite == null)
            return;

        // Calculate current world size
        currentWidth = sprite.rect.width / sprite.pixelsPerUnit;
        currentHeight = sprite.rect.height / sprite.pixelsPerUnit;
        pixelsPerUnit = sprite.pixelsPerUnit;

        // Calculate scale needed to reach target size
        float scaleX = targetWidth / currentWidth;
        float scaleY = targetHeight / currentHeight;

        // Apply uniform scaling (maintain aspect ratio)
        float uniformScale = Mathf.Min(scaleX, scaleY);
        transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
    }
}