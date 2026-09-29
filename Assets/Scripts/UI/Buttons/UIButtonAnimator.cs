using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UIButtonAnimator : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    [SerializeField, Range(1.03f, 1.05f)] private float hoverScale = 1.045f;
    [SerializeField, Range(0.95f, 0.98f)] private float pressedScale = 0.965f;
    [SerializeField, Range(0.05f, 0.2f)] private float transitionDuration = 0.1f;
    [SerializeField, Range(1f, 1.25f)] private float hoverBrightness = 1.1f;

    private RectTransform rectTransform;
    private Button button;
    private Graphic[] graphics;
    private Color[] baseColors;
    private Vector3 baseScale;
    private Coroutine transition;
    private bool pointerInside;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        button = GetComponent<Button>();
        graphics = GetComponentsInChildren<Graphic>(true);
        baseColors = new Color[graphics.Length];
        for (int i = 0; i < graphics.Length; i++)
        {
            baseColors[i] = graphics[i].color;
        }

        baseScale = rectTransform != null ? rectTransform.localScale : transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        pointerInside = true;
        PrologueAudioBindings.PlayUiHover();
        Animate(hoverScale, hoverBrightness);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        Animate(1f, 1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsInteractable()) Animate(pressedScale, 1.04f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Animate(pointerInside ? hoverScale : 1f, pointerInside ? hoverBrightness : 1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsInteractable()) PrologueAudioBindings.PlayUiClick();
    }

    private bool IsInteractable()
    {
        return button == null || button.IsInteractable();
    }

    private void Animate(float scale, float brightness)
    {
        if (!isActiveAndEnabled) return;
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(AnimateRoutine(baseScale * scale, brightness));
    }

    private IEnumerator AnimateRoutine(Vector3 targetScale, float brightness)
    {
        Vector3 startScale = transform.localScale;
        Color[] startColors = new Color[graphics.Length];
        for (int i = 0; i < graphics.Length; i++) startColors[i] = graphics[i].color;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, amount);
            for (int i = 0; i < graphics.Length; i++)
            {
                Color target = Brighten(baseColors[i], brightness);
                graphics[i].color = Color.LerpUnclamped(startColors[i], target, amount);
            }
            yield return null;
        }

        transform.localScale = targetScale;
        for (int i = 0; i < graphics.Length; i++) graphics[i].color = Brighten(baseColors[i], brightness);
        transition = null;
    }

    private static Color Brighten(Color color, float multiplier)
    {
        return new Color(
            Mathf.Clamp01(color.r * multiplier),
            Mathf.Clamp01(color.g * multiplier),
            Mathf.Clamp01(color.b * multiplier),
            color.a);
    }

    private void OnDisable()
    {
        if (transition != null) StopCoroutine(transition);
        transition = null;
        pointerInside = false;
        transform.localScale = baseScale;
        if (graphics == null) return;
        for (int i = 0; i < graphics.Length; i++) graphics[i].color = baseColors[i];
    }
}
