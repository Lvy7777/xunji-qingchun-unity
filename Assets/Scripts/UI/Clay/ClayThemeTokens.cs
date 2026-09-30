using UnityEngine;

public enum ClayRole
{
    Auto,
    Card,
    Dialogue,
    Popup,
    Button,
    Tag,
    AvatarFrame,
    Decoration,
    Ignore
}

public enum ClayTone
{
    Auto,
    Mint,
    Coral,
    Cream,
    Sky,
    Lavender,
    Peach,
    WarmWhite
}

public static class ClayThemeTokens
{
    public static readonly Color Mint = new Color(0.55f, 0.84f, 0.72f, 1f);
    public static readonly Color Coral = new Color(0.96f, 0.52f, 0.48f, 1f);
    public static readonly Color Cream = new Color(1f, 0.86f, 0.48f, 1f);
    public static readonly Color Sky = new Color(0.52f, 0.78f, 0.95f, 1f);
    public static readonly Color Lavender = new Color(0.72f, 0.64f, 0.91f, 1f);
    public static readonly Color Peach = new Color(0.98f, 0.68f, 0.46f, 1f);
    public static readonly Color WarmWhite = new Color(1f, 0.97f, 0.91f, 1f);
    public static readonly Color Ink = new Color(0.20f, 0.17f, 0.25f, 1f);
    public static readonly Color MutedInk = new Color(0.36f, 0.32f, 0.42f, 1f);
    public static readonly Color SoftShadow = new Color(0.18f, 0.14f, 0.24f, 0.24f);
    public static readonly Color SoftOutline = new Color(1f, 1f, 1f, 0.48f);

    private static readonly Color[] Accents =
    {
        Mint, Coral, Cream, Sky, Lavender, Peach
    };

    public static Color Resolve(ClayTone tone, int seed)
    {
        switch (tone)
        {
            case ClayTone.Mint: return Mint;
            case ClayTone.Coral: return Coral;
            case ClayTone.Cream: return Cream;
            case ClayTone.Sky: return Sky;
            case ClayTone.Lavender: return Lavender;
            case ClayTone.Peach: return Peach;
            case ClayTone.WarmWhite: return WarmWhite;
            default: return Accents[Mathf.Abs(seed) % Accents.Length];
        }
    }

    public static int StableHash(string value)
    {
        unchecked
        {
            int hash = 23;
            if (string.IsNullOrEmpty(value)) return hash;
            for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
            return hash;
        }
    }
}
