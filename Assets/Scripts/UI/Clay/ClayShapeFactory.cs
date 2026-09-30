using System.Collections.Generic;
using UnityEngine;

public static class ClayShapeFactory
{
    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    public static Sprite RoundedSurface(int width = 128, int height = 72, float roundness = 0.34f, int seed = 1)
    {
        string key = "clay_round_" + width + "_" + height + "_" + roundness + "_" + seed;
        if (Cache.TryGetValue(key, out Sprite cached)) return cached;

        float radius = Mathf.Max(4f, Mathf.Min(width, height) * roundness);
        Sprite sprite = Build(key, width, height, seed, (x, y) =>
        {
            float dx = Mathf.Max(Mathf.Abs(x - (width - 1f) * 0.5f) - (width * 0.5f - radius), 0f);
            float dy = Mathf.Max(Mathf.Abs(y - (height - 1f) * 0.5f) - (height * 0.5f - radius), 0f);
            return Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 1f);
        }, new Vector4(radius * 0.72f, radius * 0.72f, radius * 0.72f, radius * 0.72f));
        Cache[key] = sprite;
        return sprite;
    }

    public static Sprite Circle(int size = 96, int seed = 1)
    {
        string key = "clay_circle_" + size + "_" + seed;
        if (Cache.TryGetValue(key, out Sprite cached)) return cached;
        float center = (size - 1f) * 0.5f;
        float radius = center - 1f;
        Sprite sprite = Build(key, size, size, seed, (x, y) =>
        {
            float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
            return Mathf.Clamp01(radius - distance + 1f);
        }, Vector4.zero);
        Cache[key] = sprite;
        return sprite;
    }

    public static Sprite Star(int size = 96, int seed = 1)
    {
        string key = "clay_star_" + size + "_" + seed;
        if (Cache.TryGetValue(key, out Sprite cached)) return cached;
        Vector2 center = Vector2.one * (size - 1f) * 0.5f;
        float outer = size * 0.45f;
        float inner = outer * 0.52f;
        Vector2[] points = new Vector2[10];
        for (int i = 0; i < points.Length; i++)
        {
            float angle = Mathf.Deg2Rad * (-90f + i * 36f);
            float radius = (i & 1) == 0 ? outer : inner;
            points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        Sprite sprite = Build(key, size, size, seed, (x, y) =>
        {
            Vector2 point = new Vector2(x, y);
            bool inside = false;
            for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                if ((points[i].y > point.y) != (points[j].y > point.y) &&
                    point.x < (points[j].x - points[i].x) * (point.y - points[i].y) /
                    (points[j].y - points[i].y) + points[i].x)
                {
                    inside = !inside;
                }
            }
            return inside ? 1f : 0f;
        }, Vector4.zero);
        Cache[key] = sprite;
        return sprite;
    }

    private static Sprite Build(
        string name,
        int width,
        int height,
        int seed,
        System.Func<float, float, float> alphaAt,
        Vector4 border)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = name,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float alpha = alphaAt(x, y);
                float grain = (Mathf.PerlinNoise((x + seed * 17) * 0.075f, (y + seed * 11) * 0.075f) - 0.5f) * 0.055f;
                float light = Mathf.Lerp(-0.06f, 0.08f, y / (float)Mathf.Max(1, height - 1)) + grain;
                float value = Mathf.Clamp01(0.96f + light);
                pixels[y * width + x] = new Color(value, value, value, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            border);
        sprite.name = name;
        return sprite;
    }
}
