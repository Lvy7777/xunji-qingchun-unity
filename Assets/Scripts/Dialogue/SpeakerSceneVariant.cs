using UnityEngine;
using UnityEngine.UI;

public sealed class SpeakerSceneVariant : MonoBehaviour
{
    private Canvas canvas;
    private Image overlay;
    private GameObject xiaoHe;
    private GameObject volunteer;
    private bool lastXiaoHe;
    private bool lastVolunteer;
    private int lastVariant = -1;

    private readonly Color[] palette =
    {
        new Color(0.08f, 0.13f, 0.20f, 0.10f),
        new Color(0.23f, 0.08f, 0.06f, 0.12f),
        new Color(0.05f, 0.20f, 0.16f, 0.11f),
        new Color(0.24f, 0.15f, 0.04f, 0.10f)
    };

    private void Start()
    {
        CacheSceneObjects();
        ApplyNewVariant();
        lastXiaoHe = IsVisible(xiaoHe);
        lastVolunteer = IsVisible(volunteer);
    }

    private void Update()
    {
        if (canvas == null) CacheSceneObjects();

        bool xiaoHeVisible = IsVisible(xiaoHe);
        bool volunteerVisible = IsVisible(volunteer);
        if ((xiaoHeVisible || volunteerVisible)
            && (xiaoHeVisible != lastXiaoHe || volunteerVisible != lastVolunteer))
        {
            ApplyNewVariant();
        }

        lastXiaoHe = xiaoHeVisible;
        lastVolunteer = volunteerVisible;
    }

    private void CacheSceneObjects()
    {
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        Transform root = canvas.transform;
        Transform characterRoot = root.Find("CharacterLayer") ?? root.Find("Characters");
        if (characterRoot != null)
        {
            Transform xiao = characterRoot.Find("XiaoHe");
            Transform vol = characterRoot.Find("Volunteer");
            xiaoHe = xiao != null ? xiao.gameObject : null;
            volunteer = vol != null ? vol.gameObject : null;
        }

        Transform existing = root.Find("RandomAtmosphereOverlay");
        if (existing != null)
        {
            overlay = existing.GetComponent<Image>();
            return;
        }

        GameObject layer = new GameObject("RandomAtmosphereOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        layer.transform.SetParent(root, false);
        layer.transform.SetSiblingIndex(1);
        RectTransform rect = layer.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        overlay = layer.GetComponent<Image>();
        overlay.raycastTarget = false;
    }

    private void ApplyNewVariant()
    {
        if (overlay == null) return;

        int variant = Random.Range(0, palette.Length - 1);
        if (variant >= lastVariant) variant++;
        lastVariant = variant;
        overlay.color = palette[variant];

        Transform background = canvas.transform.Find("Background")
            ?? canvas.transform.Find("Environment/HakkaHouseBackground")
            ?? canvas.transform.Find("Environment/Background");
        RectTransform rect = background as RectTransform;
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(Random.Range(-16f, 17f), Random.Range(-6f, 7f));
        }
    }

    private static bool IsVisible(GameObject item)
    {
        return item != null && item.activeInHierarchy;
    }
}