using System.Collections.Generic;
using UnityEngine;

public static class ClaySpriteFactory
{
    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

    public static Sprite Rounded(Color color, int width = 128, int height = 64, float roundness = 0.30f, int seed = 1)
    {
        string key = "r_" + ColorUtility.ToHtmlStringRGBA(color) + "_" + width + "_" + height + "_" + roundness + "_" + seed;
        if (Cache.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = key;
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float radius = Mathf.Max(2f, Mathf.Min(width, height) * roundness);
        Color[] pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = Mathf.Max(Mathf.Abs(x - width * 0.5f) - (width * 0.5f - radius), 0f);
                float dy = Mathf.Max(Mathf.Abs(y - height * 0.5f) - (height * 0.5f - radius), 0f);
                float distance = Mathf.Sqrt(dx * dx + dy * dy) - radius;
                float alpha = Mathf.Clamp01(1f - distance);

                float grain = (Mathf.PerlinNoise((x + seed * 13) * 0.085f, (y + seed * 7) * 0.085f) - 0.5f) * 0.10f;
                float light = Mathf.Lerp(-0.13f, 0.13f, y / (float)height) + grain;
                Color shaded = new Color(
                    Mathf.Clamp01(color.r + light),
                    Mathf.Clamp01(color.g + light),
                    Mathf.Clamp01(color.b + light),
                    color.a * alpha);
                pixels[y * width + x] = shaded;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = key;
        Cache[key] = sprite;
        return sprite;
    }

    public static Sprite Circle(Color color, int size = 96, int seed = 1)
    {
        string key = "c_" + ColorUtility.ToHtmlStringRGBA(color) + "_" + size + "_" + seed;
        if (Cache.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.name = key;
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        float center = (size - 1f) * 0.5f;
        float radius = center - 1f;
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(radius - distance + 1f);
                float grain = (Mathf.PerlinNoise((x + seed * 11) * 0.10f, (y + seed * 5) * 0.10f) - 0.5f) * 0.10f;
                float light = Mathf.Lerp(-0.12f, 0.15f, y / (float)size) + grain;
                pixels[y * size + x] = new Color(
                    Mathf.Clamp01(color.r + light),
                    Mathf.Clamp01(color.g + light),
                    Mathf.Clamp01(color.b + light),
                    color.a * alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = key;
        Cache[key] = sprite;
        return sprite;
    }

    public static Sprite FromTexture(Texture2D texture, float pixelsPerUnit = 100f)
    {
        if (texture == null)
        {
            return null;
        }

        string key = "t_" + texture.GetInstanceID() + "_" + pixelsPerUnit;
        if (Cache.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        sprite.name = texture.name + "_RuntimeSprite";
        Cache[key] = sprite;
        return sprite;
    }
}
