using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CursorEffectController : MonoBehaviour
{
    [SerializeField] private int trailCount = 10;
    [SerializeField] private float followSpeed = 18f;
    [SerializeField] private float idleAlpha = 0.18f;

    private RectTransform canvasRect;
    private RectTransform head;
    private Image[] trail;
    private Vector2 lastPosition;
    private float speed;

    private void Awake()
    {
        CreateOverlay();
    }

    private void Update()
    {
        if (canvasRect == null)
        {
            return;
        }

        Vector2 point;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            null,
            out point);

        speed = Vector2.Distance(point, lastPosition) / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        lastPosition = point;
        head.anchoredPosition = point;

        float visibility = Mathf.Lerp(idleAlpha, 0.75f, Mathf.Clamp01(speed / 900f));
        for (int i = 0; i < trail.Length; i++)
        {
            RectTransform marker = trail[i].rectTransform;
            float lag = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime / (i + 1));
            marker.anchoredPosition = Vector2.Lerp(marker.anchoredPosition, point, lag);
            float alpha = visibility * (1f - i / (float)trail.Length);
            trail[i].color = new Color(1f, 0.82f, 0.43f, alpha);
        }
    }

    private void CreateOverlay()
    {
        GameObject canvasObject = new GameObject(
            "CursorEffectRoot",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        canvasObject.GetComponent<GraphicRaycaster>().enabled = false;

        canvasRect = canvasObject.GetComponent<RectTransform>();
        Sprite sprite = CreateSoftDotSprite();

        head = CreateMarker(canvasRect, "CursorGlow", sprite, 22f, new Color(1f, 0.9f, 0.55f, 0.88f)).rectTransform;
        trail = new Image[Mathf.Max(4, trailCount)];
        for (int i = 0; i < trail.Length; i++)
        {
            trail[i] = CreateMarker(
                canvasRect,
                "CursorTrail_" + i.ToString("00"),
                sprite,
                Mathf.Lerp(13f, 4f, i / (float)trail.Length),
                Color.clear);
        }
    }

    private static Image CreateMarker(
        RectTransform parent,
        string markerName,
        Sprite sprite,
        float size,
        Color color)
    {
        GameObject marker = new GameObject(markerName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        marker.transform.SetParent(parent, false);
        Image image = marker.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        RectTransform rect = image.rectTransform;
        rect.sizeDelta = Vector2.one * size;
        return image;
    }

    private static Sprite CreateSoftDotSprite()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "CursorSoftDot_Runtime",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[size * size];
        Vector2 center = Vector2.one * (size - 1) * 0.5f;
        float radius = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 1.8f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size);
    }
}
