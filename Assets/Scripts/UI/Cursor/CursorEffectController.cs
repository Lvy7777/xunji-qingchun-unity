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
            "CursorEffectCanvas",
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
        Sprite sprite = null;

        head = CreateMarker(canvasRect, sprite, 18f, new Color(1f, 0.9f, 0.55f, 0.95f)).rectTransform;
        trail = new Image[Mathf.Max(4, trailCount)];
        for (int i = 0; i < trail.Length; i++)
        {
            trail[i] = CreateMarker(canvasRect, sprite, Mathf.Lerp(13f, 4f, i / (float)trail.Length), Color.clear);
        }
    }

    private static Image CreateMarker(RectTransform parent, Sprite sprite, float size, Color color)
    {
        GameObject marker = new GameObject("CursorGlow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        marker.transform.SetParent(parent, false);
        Image image = marker.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        RectTransform rect = image.rectTransform;
        rect.sizeDelta = Vector2.one * size;
        return image;
    }
}