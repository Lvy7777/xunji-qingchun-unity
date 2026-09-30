using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CursorEffectController : MonoBehaviour
{
    [SerializeField, Range(8, 32)] private int trailCount = 20;
    [SerializeField] private float activeAlpha = 0.86f;
    [SerializeField] private float idleAlpha = 0.035f;
    [SerializeField] private float motionThreshold = 24f;
    [SerializeField] private float trailSpread = 10f;

    private static readonly Color[] Palette =
    {
        new Color(0.35f, 0.88f, 0.74f),
        new Color(1f, 0.52f, 0.58f),
        new Color(1f, 0.82f, 0.36f),
        new Color(0.38f, 0.72f, 1f),
        new Color(0.72f, 0.58f, 1f),
        new Color(1f, 0.64f, 0.43f)
    };

    private RectTransform canvasRect;
    private Image head;
    private TrailPoint[] trail;
    private Vector2 lastMousePosition;
    private Vector2 filteredMousePosition;
    private float activity;
    private float elapsed;

    private sealed class TrailPoint
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 Velocity;
        public float Phase;
    }

    private void Awake() { CreateOverlay(); }

    private void Update()
    {
        if (canvasRect == null || trail == null) return;

        elapsed += Time.unscaledDeltaTime;
        Vector2 point;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out point);

        float deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        float speed = Vector2.Distance(point, lastMousePosition) / deltaTime;
        lastMousePosition = point;

        float targetActivity = Mathf.InverseLerp(motionThreshold, 900f, speed);
        activity = Mathf.MoveTowards(activity, targetActivity, deltaTime * (targetActivity > activity ? 5.5f : 1.65f));
        filteredMousePosition = Vector2.Lerp(filteredMousePosition, point, 1f - Mathf.Exp(-26f * deltaTime));

        RectTransform headRect = head.rectTransform;
        headRect.anchoredPosition = filteredMousePosition;
        headRect.localScale = Vector3.one * (1f + Mathf.Sin(elapsed * 4.2f) * 0.08f);
        Color headColor = Color.Lerp(Palette[0], Palette[2], 0.45f + Mathf.Sin(elapsed * 1.7f) * 0.2f);
        headColor.a = Mathf.Lerp(idleAlpha * 2.2f, activeAlpha, activity);
        head.color = headColor;

        Vector2 previous = filteredMousePosition;
        Vector2 motion = point - filteredMousePosition;
        Vector2 perpendicular = motion.sqrMagnitude > 0.001f ? new Vector2(-motion.y, motion.x).normalized : Vector2.up;

        for (int i = 0; i < trail.Length; i++)
        {
            TrailPoint particle = trail[i];
            float t = (i + 1f) / trail.Length;
            float wave = Mathf.Sin(elapsed * (3.3f + t) - particle.Phase) * trailSpread * activity * t;
            Vector2 target = previous + perpendicular * wave;
            float smoothTime = 0.025f + i * 0.0065f;
            particle.Rect.anchoredPosition = Vector2.SmoothDamp(
                particle.Rect.anchoredPosition, target, ref particle.Velocity, smoothTime, Mathf.Infinity, deltaTime);
            previous = particle.Rect.anchoredPosition;

            float alpha = Mathf.Lerp(idleAlpha, activeAlpha, activity) * Mathf.Pow(1f - t, 1.35f);
            Color color = Palette[i % Palette.Length];
            color.a = alpha;
            particle.Image.color = color;

            float stretch = 1f + Mathf.Clamp01(speed / 1100f) * (1f - t) * 0.45f;
            particle.Rect.localScale = new Vector3(stretch, 1f / stretch, 1f);
            particle.Rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(particle.Velocity.y, particle.Velocity.x) * Mathf.Rad2Deg);
        }
    }

    private void CreateOverlay()
    {
        GameObject canvasObject = new GameObject(
            "CursorEffectRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;
        canvasObject.GetComponent<GraphicRaycaster>().enabled = false;
        canvasRect = canvasObject.GetComponent<RectTransform>();

        Sprite sprite = CreateSoftDotSprite();
        head = CreateMarker(canvasRect, "CursorGlow", sprite, 26f, Color.clear);
        trail = new TrailPoint[Mathf.Max(8, trailCount)];
        for (int i = 0; i < trail.Length; i++)
        {
            float t = i / (float)Mathf.Max(1, trail.Length - 1);
            Image image = CreateMarker(canvasRect, "CursorTrail_" + i.ToString("00"), sprite, Mathf.Lerp(14f, 3.5f, t), Color.clear);
            trail[i] = new TrailPoint
            {
                Rect = image.rectTransform,
                Image = image,
                Velocity = Vector2.zero,
                Phase = i * 0.72f
            };
        }
    }

    private static Image CreateMarker(RectTransform parent, string markerName, Sprite sprite, float size, Color color)
    {
        GameObject marker = new GameObject(markerName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        marker.transform.SetParent(parent, false);
        Image image = marker.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        image.rectTransform.sizeDelta = Vector2.one * size;
        return image;
    }

    private static Sprite CreateSoftDotSprite()
    {
        const int size = 48;
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
                float alpha = Mathf.Pow(Mathf.SmoothStep(1f, 0f, distance), 1.55f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
