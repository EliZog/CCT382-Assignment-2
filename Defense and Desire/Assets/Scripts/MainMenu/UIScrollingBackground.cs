using UnityEngine;
using UnityEngine.UI;

public class UIScrollingBackground : MonoBehaviour
{
    public float speedX = -0.05f;
    public float speedY = 0.05f;
    public float tileX = 1f;
    public float tileY = 1f;

    private RawImage rawImage;
    private Vector2 offset;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rawImage.uvRect = new Rect(0f, 0f, tileX, tileY);
    }

    void Update()
    {
        offset.x += speedX * Time.unscaledDeltaTime;
        offset.y += speedY * Time.unscaledDeltaTime;

        // so nums dont grow for ever
        if (offset.x > 1f) offset.x -= 1f;
        if (offset.y > 1f) offset.y -= 1f;

        rawImage.uvRect = new Rect(offset.x, offset.y, tileX, tileY);
    }
}
