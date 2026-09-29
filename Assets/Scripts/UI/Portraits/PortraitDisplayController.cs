using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class PortraitDisplayController : MonoBehaviour
{
    public static void ApplyToLoadedScene(Scene scene)
    {
        if (scene.name != "Prologue") return;

        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i].gameObject.scene != scene) continue;
            Transform layer = canvases[i].transform.Find("CharacterLayer");
            if (layer == null) continue;

            ApplyFrame(layer.Find("XiaoHe"), false);
            ApplyFrame(layer.Find("Volunteer"), true);
        }
    }

    private static void ApplyFrame(Transform portrait, bool volunteer)
    {
        if (portrait == null) return;

        Transform frame = portrait.Find("PortraitFrame");
        if (frame == null)
        {
            GameObject frameObject = new GameObject(
                "PortraitFrame",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Outline),
                typeof(Shadow),
                typeof(RectMask2D));
            frameObject.transform.SetParent(portrait, false);
            frameObject.transform.SetAsFirstSibling();
            frame = frameObject.transform;

            RectTransform frameRect = frameObject.GetComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.offsetMin = new Vector2(10f, 10f);
            frameRect.offsetMax = new Vector2(-10f, -10f);

            for (int i = portrait.childCount - 1; i >= 0; i--)
            {
                Transform child = portrait.GetChild(i);
                if (child != frame && child.name.Contains("Portrait"))
                {
                    child.SetParent(frame, true);
                }
            }
        }

        StyleFrame(frame.gameObject);

        Image rootImage = portrait.GetComponent<Image>();
        if (rootImage != null && !volunteer)
        {
            Color rootColor = rootImage.color;
            rootColor.a = 0f;
            rootImage.color = rootColor;
            rootImage.raycastTarget = false;
        }

        if (volunteer)
        {
            EnsureVolunteerPlaceholder(frame);
        }
    }

    private static void StyleFrame(GameObject frame)
    {
        Image image = frame.GetComponent<Image>();
        image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = new Color(0.98f, 0.9f, 0.74f, 0.96f);
        image.raycastTarget = false;

        Outline outline = frame.GetComponent<Outline>();
        outline.effectColor = new Color(0.53f, 0.28f, 0.13f, 0.58f);
        outline.effectDistance = new Vector2(2f, -2f);

        Shadow shadow = frame.GetComponent<Shadow>();
        shadow.effectColor = new Color(0.18f, 0.1f, 0.04f, 0.28f);
        shadow.effectDistance = new Vector2(7f, -7f);
    }

    private static void EnsureVolunteerPlaceholder(Transform frame)
    {
        Image portraitImage = frame.parent.GetComponent<Image>();
        if (portraitImage != null && portraitImage.sprite != null) return;

        Transform existing = frame.Find("VolunteerPlaceholder");
        TextMeshProUGUI label;
        if (existing == null)
        {
            GameObject labelObject = new GameObject(
                "VolunteerPlaceholder",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(frame, false);
            label = labelObject.GetComponent<TextMeshProUGUI>();
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            label = existing.GetComponent<TextMeshProUGUI>();
        }

        label.text = "志愿者\n形象待补充";
        TMP_FontAsset fallbackFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansSC-TMP");
        if (fallbackFont != null)
        {
            label.font = fallbackFont;
        }
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 28f;
        label.color = new Color(0.42f, 0.25f, 0.15f, 0.88f);
        label.raycastTarget = false;
    }
}
