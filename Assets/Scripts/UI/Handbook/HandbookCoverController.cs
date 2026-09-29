using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class HandbookCoverController : MonoBehaviour
{
    private Button button;
    private RectTransform rect;
    private Image cover;

    public static void ApplyToLoadedScene(Scene scene)
    {
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i].gameObject.scene != scene)
            {
                continue;
            }

            Transform book = canvases[i].transform.Find("DialogueLayer/StudyBookObject");
            if (book != null && book.GetComponent<HandbookCoverController>() == null)
            {
                book.gameObject.AddComponent<HandbookCoverController>();
            }

            Transform panel = canvases[i].transform.Find("DialogueLayer/StudyBookPanel");
            if (panel != null)
            {
                StyleOpenPanel(panel);
            }
        }
    }

    private void Awake()
    {
        button = GetComponent<Button>();
        rect = transform as RectTransform;
        cover = GetComponent<Image>();
        ApplyCover();
    }

    public IEnumerator PlayOpenFeedback()
    {
        if (rect == null)
        {
            yield break;
        }

        Vector3 start = rect.localScale;
        float elapsed = 0f;
        while (elapsed < 0.12f)
        {
            elapsed += Time.unscaledDeltaTime;
            rect.localScale = Vector3.Lerp(start, start * 1.08f, elapsed / 0.12f);
            yield return null;
        }

        SFXController.Play(UISound.PageTurn);
    }

    private void ApplyCover()
    {
        if (rect == null || cover == null)
        {
            return;
        }

        rect.sizeDelta = new Vector2(410f, 500f);
        cover.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        cover.type = Image.Type.Sliced;
        cover.color = new Color(0.94f, 0.75f, 0.42f, 1f);

        Outline outline = GetComponent<Outline>();
        if (outline == null) outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.43f, 0.22f, 0.08f, 0.9f);
        outline.effectDistance = new Vector2(4f, -4f);

        Shadow shadow = GetComponent<Shadow>();
        if (shadow == null) shadow = gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0.12f, 0.06f, 0.02f, 0.34f);
        shadow.effectDistance = new Vector2(10f, -10f);

        CreateDecoration("HandbookSpine", new Vector2(-174f, 0f), new Vector2(28f, 460f), new Color(0.49f, 0.22f, 0.08f, 0.92f));
        CreateDecoration("HandbookRibbon", new Vector2(138f, 184f), new Vector2(58f, 100f), new Color(0.78f, 0.18f, 0.16f, 0.95f));
        CreateLabel("HandbookTitle", "研学手册", new Vector2(12f, 66f), 44f, FontStyles.Bold, new Color(0.31f, 0.16f, 0.07f, 1f));
        CreateLabel("HandbookSubtitle", "寻迹 · 青春", new Vector2(12f, 14f), 23f, FontStyles.Normal, new Color(0.46f, 0.25f, 0.1f, 0.95f));
        CreateLabel("HandbookBadge", "●  ●  ●  ●", new Vector2(12f, -152f), 20f, FontStyles.Normal, new Color(0.71f, 0.31f, 0.18f, 0.9f));
    }

    private void CreateDecoration(string name, Vector2 position, Vector2 size, Color color)
    {
        if (transform.Find(name) != null) return;
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        item.transform.SetParent(transform, false);
        Image image = item.GetComponent<Image>();
        image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        image.color = color;
        image.raycastTarget = false;
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.anchoredPosition = position;
        itemRect.sizeDelta = size;
    }

    private void CreateLabel(string name, string text, Vector2 position, float fontSize, FontStyles style, Color color)
    {
        if (transform.Find(name) != null) return;
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        item.transform.SetParent(transform, false);
        TextMeshProUGUI label = item.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.raycastTarget = false;
        RectTransform itemRect = label.rectTransform;
        itemRect.anchorMin = new Vector2(0.5f, 0.5f);
        itemRect.anchorMax = new Vector2(0.5f, 0.5f);
        itemRect.anchoredPosition = position;
        itemRect.sizeDelta = new Vector2(330f, 64f);
    }

    private static void StyleOpenPanel(Transform panel)
    {
        Image image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = new Color(0.99f, 0.94f, 0.82f, 0.98f);
        }

        Outline outline = panel.GetComponent<Outline>();
        if (outline == null) outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.48f, 0.27f, 0.11f, 0.8f);
        outline.effectDistance = new Vector2(3f, -3f);
    }
}