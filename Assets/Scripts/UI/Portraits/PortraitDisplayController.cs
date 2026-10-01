using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Applies the shared dialogue portrait art and placement without touching story logic.
/// Character visibility remains owned by each scene's CanvasGroup/dialogue controller.
/// </summary>
public sealed class PortraitDisplayController : MonoBehaviour
{
    // Internal hierarchy names stay unchanged for scene compatibility; the visible protagonist is 栗拓拓.
    private const string XiaoHeResource = "Characters/LiTuoTuo_Front";
    private const string VolunteerResource = "Characters/Volunteer_Front";
    private const string ArtworkName = "ClayCharacterArtwork";

    private static Sprite xiaoHeSprite;
    private static Sprite volunteerSprite;

    public static void ApplyToLoadedScene(Scene scene)
    {
        LoadSprites();
        if (xiaoHeSprite == null || volunteerSprite == null)
        {
            return;
        }

        RectTransform[] rects = Object.FindObjectsOfType<RectTransform>(true);
        for (int i = 0; i < rects.Length; i++)
        {
            RectTransform portrait = rects[i];
            if (portrait.gameObject.scene != scene || portrait.GetComponentInParent<Canvas>() == null)
            {
                continue;
            }

            if (portrait.name == "XiaoHe")
            {
                ApplyPortrait(portrait, xiaoHeSprite, false);
            }
            else if (portrait.name == "Volunteer")
            {
                ApplyPortrait(portrait, volunteerSprite, true);
            }
        }
    }

    private static void LoadSprites()
    {
        if (xiaoHeSprite == null) xiaoHeSprite = Resources.Load<Sprite>(XiaoHeResource);
        if (volunteerSprite == null) volunteerSprite = Resources.Load<Sprite>(VolunteerResource);
    }

    private static void ApplyPortrait(RectTransform root, Sprite sprite, bool volunteer)
    {
        ConfigureDialoguePlacement(root, volunteer);
        RemoveLegacyFrameBackground(root);

        Transform existing = root.Find(ArtworkName);
        Image artwork;
        if (existing == null)
        {
            GameObject artworkObject = new GameObject(
                ArtworkName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Shadow));
            artworkObject.transform.SetParent(root, false);
            artwork = artworkObject.GetComponent<Image>();
        }
        else
        {
            artwork = existing.GetComponent<Image>();
        }

        artwork.sprite = sprite;
        artwork.preserveAspect = true;
        artwork.raycastTarget = false;
        artwork.color = Color.white;

        RectTransform artworkRect = artwork.rectTransform;
        artworkRect.anchorMin = Vector2.zero;
        artworkRect.anchorMax = Vector2.one;
        artworkRect.offsetMin = Vector2.zero;
        artworkRect.offsetMax = Vector2.zero;
        artworkRect.localScale = Vector3.one;
        artworkRect.localRotation = Quaternion.identity;
        artworkRect.SetAsLastSibling();

        Shadow shadow = artwork.GetComponent<Shadow>();
        shadow.effectColor = new Color(0.16f, 0.13f, 0.2f, 0.2f);
        shadow.effectDistance = new Vector2(volunteer ? 7f : 6f, -7f);
        shadow.useGraphicAlpha = true;
    }

    private static void ConfigureDialoguePlacement(RectTransform root, bool volunteer)
    {
        root.anchorMin = volunteer ? new Vector2(0.985f, 0.075f) : new Vector2(0.025f, 0.09f);
        root.anchorMax = root.anchorMin;
        root.pivot = volunteer ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
        root.anchoredPosition = volunteer ? new Vector2(-18f, 0f) : new Vector2(18f, 0f);
        root.sizeDelta = volunteer ? new Vector2(400f, 650f) : new Vector2(430f, 610f);
        root.localScale = Vector3.one;
    }

    private static void RemoveLegacyFrameBackground(RectTransform root)
    {
        Image rootImage = root.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.color = new Color(1f, 1f, 1f, 0f);
            rootImage.raycastTarget = false;
        }

        Transform frame = root.Find("PortraitFrame");
        if (frame != null)
        {
            Image frameImage = frame.GetComponent<Image>();
            if (frameImage != null)
            {
                frameImage.color = new Color(1f, 1f, 1f, 0f);
                frameImage.raycastTarget = false;
            }

            RectMask2D mask = frame.GetComponent<RectMask2D>();
            if (mask != null) mask.enabled = false;
        }

        TMP_Text[] labels = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            if (labels[i].name.Contains("Placeholder")) labels[i].gameObject.SetActive(false);
        }

        Image[] images = root.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image.name == ArtworkName || image.transform == root || image.transform == frame) continue;

            string lowerName = image.name.ToLowerInvariant();
            if (lowerName.Contains("portrait") || lowerName.Contains("placeholder")) image.enabled = false;
        }
    }
}
