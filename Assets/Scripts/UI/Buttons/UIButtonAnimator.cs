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
    [SerializeField, Range(0f, 10f)] private float hoverLift = 5f;
    [SerializeField, Range(0f, 3f)] private float hoverTilt = 1.2f;

    private RectTransform rectTransform;
    private Button button;
    private Graphic[] graphics;
    private Color[] baseColors;
    private Vector3 baseScale;
    private Vector2 basePosition;
    private Quaternion baseRotation;
    private Coroutine transition;
    private bool pointerInside;
    private float tiltDirection;

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
        basePosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        baseRotation = transform.localRotation;
        tiltDirection = (ClayThemeTokens.StableHash(name) & 1) == 0 ? -1f : 1f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        pointerInside = true;
        PrologueAudioBindings.PlayUiHover();
        Animate(hoverScale, hoverBrightness, hoverLift, hoverTilt * tiltDirection);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        Animate(1f, 1f, 0f, 0f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsInteractable()) Animate(pressedScale, 1.02f, -2f, 0f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Animate(
            pointerInside ? hoverScale : 1f,
            pointerInside ? hoverBrightness : 1f,
            pointerInside ? hoverLift : 0f,
            pointerInside ? hoverTilt * tiltDirection : 0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsInteractable()) PrologueAudioBindings.PlayUiClick();
    }

    private bool IsInteractable()
    {
        return button == null || button.IsInteractable();
    }

    private void Animate(float scale, float brightness, float lift, float tilt)
    {
        if (!isActiveAndEnabled) return;
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(AnimateRoutine(
            baseScale * scale,
            brightness,
            basePosition + Vector2.up * lift,
            baseRotation * Quaternion.Euler(0f, 0f, tilt)));
    }

    private IEnumerator AnimateRoutine(
        Vector3 targetScale,
        float brightness,
        Vector2 targetPosition,
        Quaternion targetRotation)
    {
        Vector3 startScale = transform.localScale;
        Vector2 startPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
        Quaternion startRotation = transform.localRotation;
        Color[] startColors = new Color[graphics.Length];
        for (int i = 0; i < graphics.Length; i++) startColors[i] = graphics[i].color;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, amount);
            if (rectTransform != null) rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, amount);
            transform.localRotation = Quaternion.SlerpUnclamped(startRotation, targetRotation, amount);
            for (int i = 0; i < graphics.Length; i++)
            {
                Color target = Brighten(baseColors[i], brightness);
                graphics[i].color = Color.LerpUnclamped(startColors[i], target, amount);
            }
            yield return null;
        }

        transform.localScale = targetScale;
        if (rectTransform != null) rectTransform.anchoredPosition = targetPosition;
        transform.localRotation = targetRotation;
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
        if (rectTransform != null) rectTransform.anchoredPosition = basePosition;
        transform.localRotation = baseRotation;
        if (graphics == null) return;
        for (int i = 0; i < graphics.Length; i++) graphics[i].color = baseColors[i];
    }
}
