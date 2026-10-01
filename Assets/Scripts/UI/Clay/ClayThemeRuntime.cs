using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ClayThemeRuntime
{
    public static void ApplyToLoadedScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded) return;

        Canvas[] canvases = UnityEngine.Object.FindObjectsOfType<Canvas>(true);
        Canvas decorationCanvas = null;
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas.gameObject.scene != scene || canvas.renderMode == RenderMode.WorldSpace) continue;
            if (decorationCanvas == null && !Contains(canvas.name, "cursor")) decorationCanvas = canvas;
        }

        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas.gameObject.scene != scene || canvas.renderMode == RenderMode.WorldSpace) continue;
            ApplyCanvasInternal(canvas, scene.name, canvas == decorationCanvas);
        }
    }

    public static void ApplyCanvas(Canvas canvas, string sceneName = null)
    {
        if (canvas == null) return;
        ApplyCanvasInternal(
            canvas,
            string.IsNullOrEmpty(sceneName) ? canvas.gameObject.scene.name : sceneName,
            true);
    }

    private static void ApplyCanvasInternal(Canvas canvas, string sceneName, bool addDecorations)
    {
        if (canvas.GetComponent<ClayThemeOptOut>() != null) return;

        Image[] images = canvas.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (IsGenerated(images[i].name)) continue;
            ClayRole role = ResolveRole(images[i]);
            if (role != ClayRole.Auto && role != ClayRole.Ignore) ApplySurface(images[i], role);
        }

        Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            ApplyButton(buttons[i]);
        }

        TMP_Text[] texts = canvas.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (!IsGenerated(texts[i].name)) ApplyText(texts[i]);
        }

        if (addDecorations) EnsureDecorations(canvas, sceneName);
    }

    private static ClayRole ResolveRole(Image image)
    {
        ClayElement explicitStyle = image.GetComponent<ClayElement>();
        if (explicitStyle != null && explicitStyle.Role != ClayRole.Auto) return explicitStyle.Role;

        string name = image.name.ToLowerInvariant();
        if (ContainsAny(name, "background", "backdrop", "fade", "dimmer", "overlay", "artwork", "photo", "floor", "wall", "sky", "hill",
            "screenadvance", "clickcatcher", "inputblocker", "raycastblocker", "invisiblebutton"))
            return ClayRole.Ignore;
        if (image.GetComponent<Button>() != null) return ClayRole.Button;
        if (ContainsAny(name, "portraitframe", "avatarframe", "portraitplaceholder")) return ClayRole.AvatarFrame;
        if (ContainsAny(name, "dialoguepanel", "choicepanel")) return ClayRole.Dialogue;
        if (ContainsAny(name, "popup", "completionpanel", "completepanel", "missionpanel", "chaptertitlepanel", "studybookpanel", "handbookcontentpanel", "finalscreen"))
            return ClayRole.Popup;
        if (ContainsAny(name, "badge", "status", "tag", "hint")) return ClayRole.Tag;
        if (ContainsAny(name, "card", "panel", "hud", "statusbar", "taskbar", "materialbar", "menu")) return ClayRole.Card;
        return ClayRole.Auto;
    }

    private static void ApplySurface(Image image, ClayRole role)
    {
        ClayElement explicitStyle = image.GetComponent<ClayElement>();
        int seed = ClayThemeTokens.StableHash(image.gameObject.scene.name + "/" + image.name);
        ClayTone tone = explicitStyle != null ? explicitStyle.Tone : ClayTone.Auto;
        Color tint;
        switch (role)
        {
            case ClayRole.Dialogue:
            case ClayRole.Popup:
                tint = ClayThemeTokens.WarmWhite;
                break;
            case ClayRole.AvatarFrame:
                tint = ClayThemeTokens.Cream;
                break;
            case ClayRole.Tag:
                tint = Color.Lerp(ClayThemeTokens.Resolve(tone, seed), ClayThemeTokens.WarmWhite, 0.18f);
                break;
            default:
                tint = Color.Lerp(ClayThemeTokens.Resolve(tone, seed), ClayThemeTokens.WarmWhite, 0.58f);
                break;
        }

        float originalAlpha = image.color.a;
        if (role != ClayRole.Button && originalAlpha < 0.06f) return;
        tint.a = role == ClayRole.Button ? Mathf.Max(0.94f, originalAlpha) : originalAlpha;

        bool keepSprite = explicitStyle != null && explicitStyle.KeepSprite;
        bool keepColor = explicitStyle != null && explicitStyle.KeepColor;
        if (!keepSprite && (role == ClayRole.Button || image.sprite == null))
        {
            image.sprite = ClayShapeFactory.RoundedSurface(128, 72, role == ClayRole.Tag ? 0.48f : 0.34f, seed);
            image.type = Image.Type.Sliced;
        }
        if (!keepColor) image.color = tint;

        image.pixelsPerUnitMultiplier = 1f;
        AddDepth(image.gameObject, role);
        EnsureInnerHighlight(image.transform, role);
        if (role == ClayRole.Card && Contains(image.name, "card") &&
            (explicitStyle == null || !explicitStyle.DisableMotion))
        {
            ClayCardMotion motion = image.GetComponent<ClayCardMotion>();
            if (motion == null)
            {
                motion = image.gameObject.AddComponent<ClayCardMotion>();
                motion.Configure(seed);
            }
        }
    }

    private static void ApplyButton(Button button)
    {
        Image image = button.targetGraphic as Image;
        if (image == null) image = button.GetComponent<Image>();
        if (IsPassThroughButton(button, image)) return;
        if (image != null) ApplySurface(image, ClayRole.Button);

        Color baseColor = image != null ? image.color : ClayThemeTokens.Mint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.Lerp(Color.white, ClayThemeTokens.WarmWhite, 0.2f);
        colors.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.72f, 0.72f, 0.72f, 0.62f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        if (image != null) image.color = ResolveButtonColor(button.name, baseColor);

        MonoBehaviour[] behaviours = button.GetComponents<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || behaviour is UIButtonAnimator) continue;
            string typeName = behaviour.GetType().Name;
            if (typeName == "UIButtonFeedback" || typeName == "HometownUIFeedback") behaviour.enabled = false;
        }

        if (button.GetComponent<UIButtonAnimator>() == null) button.gameObject.AddComponent<UIButtonAnimator>();
    }

    private static bool IsPassThroughButton(Button button, Image image)
    {
        string name = button.name.ToLowerInvariant();
        if (ContainsAny(name, "screenadvance", "clickcatcher", "inputblocker", "raycastblocker", "invisiblebutton"))
            return true;

        return image != null && image.sprite == null && image.color.a < 0.08f;
    }

    private static Color ResolveButtonColor(string name, Color fallback)
    {
        string value = name.ToLowerInvariant();
        if (ContainsAny(value, "close", "cancel", "retry")) return ClayThemeTokens.Coral;
        if (ContainsAny(value, "next", "continue", "start", "finish", "confirm", "complete", "end")) return ClayThemeTokens.Mint;
        if (ContainsAny(value, "choice", "answer"))
            return (ClayThemeTokens.StableHash(name) & 1) == 0 ? ClayThemeTokens.Sky : ClayThemeTokens.Lavender;
        if (fallback.a > 0.5f && fallback.maxColorComponent > 0.45f) return Color.Lerp(fallback, ClayThemeTokens.WarmWhite, 0.12f);
        return ClayThemeTokens.Peach;
    }

    private static void ApplyText(TMP_Text text)
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/HYAoJiaoTiJian/HYAoJiaoTiJian");
        if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts/NotoSansSC-GameTextV3");
        if (font != null) text.font = font;

        string name = text.name.ToLowerInvariant();
        bool title = ContainsAny(name, "title", "chaptername", "heading", "booktitle", "gametitle");
        bool label = ContainsAny(name, "label", "tag", "status", "hint", "subtitle", "progress");
        Image surface = FindStyledSurface(text.transform.parent);
        Color surfaceText = ClayThemeTokens.Ink;
        if (surface != null)
        {
            Color fill = surface.color;
            float luminance = fill.r * 0.2126f + fill.g * 0.7152f + fill.b * 0.0722f;
            surfaceText = luminance < 0.48f ? ClayThemeTokens.WarmWhite : ClayThemeTokens.Ink;
        }
        if (title)
        {
            text.fontStyle |= FontStyles.Bold;
            text.characterSpacing = Mathf.Max(text.characterSpacing, 1.5f);
            if (surface != null) text.color = surfaceText;
        }
        else if (surface != null && label)
        {
            text.color = surfaceText == ClayThemeTokens.Ink ? ClayThemeTokens.MutedInk : surfaceText;
        }
        else if (surface != null && text.color.a > 0.3f)
        {
            text.color = surfaceText;
        }

        text.enableWordWrapping = true;
    }

    private static Image FindStyledSurface(Transform current)
    {
        while (current != null)
        {
            Image image = current.GetComponent<Image>();
            if (image != null && current.Find("ClayInnerHighlight") != null) return image;
            current = current.parent;
        }
        return null;
    }

    private static void AddDepth(GameObject target, ClayRole role)
    {
        Shadow shadow = null;
        Shadow[] shadows = target.GetComponents<Shadow>();
        for (int i = 0; i < shadows.Length; i++)
        {
            if (shadows[i].GetType() == typeof(Shadow)) shadow = shadows[i];
        }
        if (shadow == null) shadow = target.AddComponent<Shadow>();
        shadow.effectColor = ClayThemeTokens.SoftShadow;
        shadow.effectDistance = role == ClayRole.Button ? new Vector2(0f, -7f) : new Vector2(0f, -9f);
        shadow.useGraphicAlpha = true;

        Outline outline = target.GetComponent<Outline>();
        if (outline == null) outline = target.AddComponent<Outline>();
        outline.effectColor = ClayThemeTokens.SoftOutline;
        outline.effectDistance = role == ClayRole.Button ? new Vector2(2f, -2f) : new Vector2(1.5f, -1.5f);
        outline.useGraphicAlpha = true;
    }

    private static void EnsureInnerHighlight(Transform surface, ClayRole role)
    {
        if (surface.Find("ClayInnerHighlight") != null) return;
        GameObject highlight = new GameObject(
            "ClayInnerHighlight",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        highlight.transform.SetParent(surface, false);
        highlight.transform.SetAsFirstSibling();
        RectTransform rect = highlight.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.06f, 0.78f);
        rect.anchorMax = new Vector2(0.94f, 0.94f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = highlight.GetComponent<Image>();
        image.sprite = ClayShapeFactory.RoundedSurface(96, 24, 0.48f, 3);
        image.type = Image.Type.Sliced;
        image.color = new Color(1f, 1f, 1f, role == ClayRole.Button ? 0.20f : 0.14f);
        image.raycastTarget = false;
    }

    private static void EnsureDecorations(Canvas canvas, string sceneName)
    {
        Transform existing = canvas.transform.Find("ClayDecorationLayer");
        ClayDecorationLayer layer;
        if (existing == null)
        {
            GameObject root = new GameObject("ClayDecorationLayer", typeof(RectTransform), typeof(ClayDecorationLayer));
            root.transform.SetParent(canvas.transform, false);
            root.transform.SetSiblingIndex(Mathf.Min(1, canvas.transform.childCount - 1));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            layer = root.GetComponent<ClayDecorationLayer>();
        }
        else
        {
            layer = existing.GetComponent<ClayDecorationLayer>();
        }
        layer.Populate(sceneName);
    }

    private static bool IsGenerated(string name) => name.StartsWith("Clay", StringComparison.Ordinal);

    private static bool Contains(string value, string part) =>
        value.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0;

    private static bool ContainsAny(string value, params string[] parts)
    {
        for (int i = 0; i < parts.Length; i++)
        {
            if (value.IndexOf(parts[i], StringComparison.OrdinalIgnoreCase) >= 0) return true;
        }
        return false;
    }
}
