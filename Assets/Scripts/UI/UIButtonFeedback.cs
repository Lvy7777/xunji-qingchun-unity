using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class UIButtonFeedback : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField, Range(1f, 1.1f)] private float hoverScale = 1.035f;
    [SerializeField, Range(0.9f, 1f)] private float pressedScale = 0.96f;
    [SerializeField] private float transitionDuration = 0.1f;

    private RectTransform rectTransform;
    private Button button;
    private Vector3 baseScale;
    private Coroutine scaleRoutine;
    private bool pointerInside;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        button = GetComponent<Button>();
        baseScale = rectTransform != null ? rectTransform.localScale : transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        AnimateTo(hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        AnimateTo(1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsInteractable())
        {
            AnimateTo(pressedScale);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnimateTo(pointerInside ? hoverScale : 1f);
    }

    private bool IsInteractable()
    {
        return button == null || button.IsInteractable();
    }

    private void AnimateTo(float multiplier)
    {
        if (!isActiveAndEnabled || !IsInteractable())
        {
            return;
        }

        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
        }

        scaleRoutine = StartCoroutine(ScaleTo(baseScale * multiplier));
    }

    private IEnumerator ScaleTo(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionDuration));
            transform.localScale = Vector3.LerpUnclamped(start, target, amount);
            yield return null;
        }

        transform.localScale = target;
        scaleRoutine = null;
    }

    private void OnDisable()
    {
        if (scaleRoutine != null)
        {
            StopCoroutine(scaleRoutine);
            scaleRoutine = null;
        }

        pointerInside = false;
        transform.localScale = baseScale;
    }
}
