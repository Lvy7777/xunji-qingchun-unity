using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class HandbookCoverController : MonoBehaviour
{
    [Header("Replaceable P-001 art slots")]
    [SerializeField] private Sprite coverSprite;
    [SerializeField] private Image coverImage;
    [SerializeField] private Image glowImage;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private GameObject contentPanel;

    private RectTransform rect;
    private CanvasGroup group;
    private float baseGlowAlpha;

    public static void ApplyToLoadedScene(Scene scene)
    {
        if (scene.name != "Prologue") return;

        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i].gameObject.scene != scene) continue;

            Transform dialogueLayer = canvases[i].transform.Find("DialogueLayer");
            if (dialogueLayer == null) continue;

            Transform root = dialogueLayer.Find("HandbookCoverRoot")
                ?? dialogueLayer.Find("StudyBookObject");
            if (root != null && root.GetComponent<HandbookCoverController>() == null)
            {
                root.gameObject.AddComponent<HandbookCoverController>();
            }

            Transform panel = dialogueLayer.Find("HandbookContentPanel")
                ?? dialogueLayer.Find("StudyBookPanel");
            if (panel != null) StyleOpenPanel(panel);
        }
    }

    private void Awake()
    {
        rect = transform as RectTransform;
        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        BindChildren();
        ApplyVisuals();
    }

    private void Update()
    {
        if (glowImage == null) return;
        Color color = glowImage.color;
        color.a = baseGlowAlpha + (Mathf.Sin(Time.unscaledTime * 2.4f) + 1f) * 0.08f;
        glowImage.color = color;
    }

    public IEnumerator PlayOpenTransition()
    {
        if (rect == null) yield break;

        PrologueAudioBindings.PlayBookOpen();
        Vector3 baseScale = rect.localScale;
        yield return Scale(baseScale, baseScale * 0.96f, 0.08f);
        yield return Scale(baseScale * 0.96f, baseScale * 1.1f, 0.15f);

        float elapsed = 0f;
        const float unfoldDuration = 0.2f;
        Vector3 from = baseScale * 1.1f;
        Vector3 to = new Vector3(baseScale.x * 1.34f, baseScale.y * 1.05f, baseScale.z);
        while (elapsed < unfoldDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / unfoldDuration));
            rect.localScale = Vector3.LerpUnclamped(from, to, amount);
            group.alpha = Mathf.Lerp(1f, 0.15f, amount);
            yield return null;
        }

        rect.localScale = baseScale;
        group.alpha = 1f;
    }

    private IEnumerator Scale(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            rect.localScale = Vector3.LerpUnclamped(from, to, amount);
            yield return null;
        }
        rect.localScale = to;
    }

    private void BindChildren()
    {
        Transform cover = transform.Find("HandbookCoverImage");
        Transform glow = transform.Find("HandbookGlow");
        Transform hint = transform.Find("HandbookHintText");
        if (coverImage == null && cover != null) coverImage = cover.GetComponent<Image>();
        if (glowImage == null && glow != null) glowImage = glow.GetComponent<Image>();
        if (hintText == null && hint != null) hintText = hint.GetComponent<TMP_Text>();

        if (contentPanel == null && transform.parent != null)
        {
            Transform panel = transform.parent.Find("HandbookContentPanel")
                ?? transform.parent.Find("StudyBookPanel");
            if (panel != null) contentPanel = panel.gameObject;
        }
    }

    private void ApplyVisuals()
    {
        if (rect != null) rect.sizeDelta = new Vector2(410f, 500f);

        if (coverImage != null)
        {
            coverImage.sprite = coverSprite;
            coverImage.preserveAspect = coverSprite != null;
            coverImage.color = coverSprite != null
                ? Color.white
                : new Color(0.94f, 0.75f, 0.42f, 1f);
            coverImage.raycastTarget = false;
        }

        if (glowImage != null)
        {
            baseGlowAlpha = 0.18f;
            glowImage.raycastTarget = false;
        }

        if (hintText != null)
        {
            hintText.text = "点击打开研学手册";
            hintText.raycastTarget = false;
        }
    }

    private static void StyleOpenPanel(Transform panel)
    {
        RectTransform panelRect = panel as RectTransform;
        if (panelRect != null) panelRect.sizeDelta = new Vector2(980f, 680f);

        Image image = panel.GetComponent<Image>();
        if (image != null)
        {
            image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = new Color(0.99f, 0.94f, 0.82f, 0.98f);
        }

        Outline outline = panel.GetComponent<Outline>();
        if (outline == null) outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.48f, 0.27f, 0.11f, 0.8f);
        outline.effectDistance = new Vector2(3f, -3f);
    }
}
