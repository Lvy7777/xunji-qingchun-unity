using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class PortraitDisplayController : MonoBehaviour
{
    public static void ApplyToLoadedScene(Scene scene)
    {
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i].gameObject.scene != scene)
            {
                continue;
            }

            Transform layer = canvases[i].transform.Find("CharacterLayer");
            if (layer == null)
            {
                continue;
            }

            ApplyFrame(layer.Find("XiaoHe"), "小禾");
            ApplyFrame(layer.Find("Volunteer"), "志愿者\n形象待补充");
        }
    }

    private static void ApplyFrame(Transform portrait, string placeholder)
    {
        if (portrait == null || portrait.parent == null)
        {
            return;
        }

        string frameName = "PortraitFrame_" + portrait.name;
        Transform existing = portrait.parent.Find(frameName);
        if (existing == null)
        {
            GameObject frame = new GameObject(frameName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline), typeof(Shadow));
            frame.transform.SetParent(portrait.parent, false);
            frame.transform.SetSiblingIndex(portrait.GetSiblingIndex());

            RectTransform frameRect = frame.GetComponent<RectTransform>();
            RectTransform portraitRect = portrait as RectTransform;
            frameRect.anchorMin = portraitRect.anchorMin;
            frameRect.anchorMax = portraitRect.anchorMax;
            frameRect.pivot = portraitRect.pivot;
            frameRect.anchoredPosition = portraitRect.anchoredPosition;
            frameRect.sizeDelta = portraitRect.sizeDelta + new Vector2(26f, 26f);
            frameRect.localScale = portraitRect.localScale;

            Image image = frame.GetComponent<Image>();
            image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.98f, 0.87f, 0.65f, 0.9f);
            image.raycastTarget = false;

            Outline outline = frame.GetComponent<Outline>();
            outline.effectColor = new Color(0.53f, 0.28f, 0.13f, 0.65f);
            outline.effectDistance = new Vector2(2f, -2f);

            Shadow shadow = frame.GetComponent<Shadow>();
            shadow.effectColor = new Color(0.18f, 0.1f, 0.04f, 0.28f);
            shadow.effectDistance = new Vector2(7f, -7f);
        }

        if (portrait.name == "Volunteer")
        {
            Image image = portrait.GetComponent<Image>();
            if (image != null && image.sprite == null)
            {
                TMP_Text label = portrait.GetComponentInChildren<TMP_Text>(true);
                if (label == null)
                {
                    GameObject labelObject = new GameObject("VolunteerPlaceholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                    labelObject.transform.SetParent(portrait, false);
                    label = labelObject.GetComponent<TextMeshProUGUI>();
                    label.alignment = TextAlignmentOptions.Center;
                    label.fontSize = 28f;
                    label.color = new Color(0.42f, 0.25f, 0.15f, 0.85f);
                    label.rectTransform.anchorMin = Vector2.zero;
                    label.rectTransform.anchorMax = Vector2.one;
                    label.rectTransform.offsetMin = Vector2.zero;
                    label.rectTransform.offsetMax = Vector2.zero;
                }
                label.text = placeholder;
            }
        }
    }
}