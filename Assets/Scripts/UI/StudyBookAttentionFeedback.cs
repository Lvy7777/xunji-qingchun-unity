using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class StudyBookAttentionFeedback : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] private float breatheAmount = 0.025f;
    [SerializeField] private float breatheSpeed = 2.3f;
    [SerializeField] private float hoverAmount = 0.055f;
    [SerializeField] private float responseSpeed = 9f;

    private RectTransform rectTransform;
    private Button button;
    private Outline outline;
    private TMP_Text hintText;
    private Vector3 baseScale;
    private Color baseHintColor;
    private Vector3 baseHintScale;
    private float hoverWeight;
    private float pressOffset;
    private bool pointerInside;

    public void Configure(TMP_Text hint)
    {
        hintText = hint;
        if (hintText != null)
        {
            baseHintColor = hintText.color;
            baseHintScale = hintText.transform.localScale;
        }
    }

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        button = GetComponent<Button>();
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.effectDistance = new Vector2(4f, -4f);
        outline.effectColor = new Color(1f, 0.78f, 0.32f, 0.38f);
        outline.useGraphicAlpha = true;
    }

    private void OnEnable()
    {
        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        baseScale = rectTransform != null ? rectTransform.localScale : transform.localScale;
        pointerInside = false;
        hoverWeight = 0f;
        pressOffset = 0f;
    }

    private void Update()
    {
        if (rectTransform == null)
        {
            return;
        }

        float target = pointerInside && IsInteractable() ? 1f : 0f;
        hoverWeight = Mathf.MoveTowards(hoverWeight, target, responseSpeed * Time.unscaledDeltaTime);

        float breathe = Mathf.Sin(Time.unscaledTime * breatheSpeed) * breatheAmount;
        float multiplier = 1f + breathe + hoverWeight * hoverAmount + pressOffset;
        rectTransform.localScale = baseScale * multiplier;

        float glowPulse = 0.32f + (Mathf.Sin(Time.unscaledTime * 2f) + 1f) * 0.08f;
        outline.effectColor = new Color(1f, 0.78f, 0.32f, glowPulse + hoverWeight * 0.22f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        SetHintEmphasis(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        pressOffset = 0f;
        SetHintEmphasis(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsInteractable())
        {
            pressOffset = -0.025f;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressOffset = 0f;
    }

    private bool IsInteractable()
    {
        return button == null || button.IsInteractable();
    }

    private void SetHintEmphasis(bool emphasized)
    {
        if (hintText == null)
        {
            return;
        }

        hintText.color = emphasized
            ? new Color(1f, 0.86f, 0.46f, 1f)
            : baseHintColor;
        hintText.transform.localScale = baseHintScale * (emphasized ? 1.06f : 1f);
    }

    private void OnDisable()
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = baseScale;
        }

        SetHintEmphasis(false);
    }
}
