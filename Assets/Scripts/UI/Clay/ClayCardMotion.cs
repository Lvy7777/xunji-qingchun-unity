using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public sealed class ClayCardMotion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float baseTilt = 1.2f;
    [SerializeField] private float hoverScale = 1.018f;
    [SerializeField] private float hoverTilt = 2.1f;
    [SerializeField] private float duration = 0.18f;

    private RectTransform rect;
    private Vector3 baseScale;
    private Quaternion restingRotation;
    private Quaternion neutralRotation;
    private Coroutine routine;

    public void Configure(int seed)
    {
        float direction = (seed & 1) == 0 ? -1f : 1f;
        rect = transform as RectTransform;
        baseScale = transform.localScale;
        neutralRotation = transform.localRotation;
        restingRotation = neutralRotation * Quaternion.Euler(0f, 0f, baseTilt * direction);
        transform.localRotation = restingRotation;
        hoverTilt *= -direction;
    }

    private void Awake()
    {
        if (rect == null)
        {
            rect = transform as RectTransform;
            baseScale = transform.localScale;
            neutralRotation = transform.localRotation;
            restingRotation = neutralRotation;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Animate(baseScale * hoverScale, neutralRotation * Quaternion.Euler(0f, 0f, hoverTilt));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Animate(baseScale, restingRotation);
    }

    private void Animate(Vector3 scale, Quaternion rotation)
    {
        if (!isActiveAndEnabled) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(AnimateRoutine(scale, rotation));
    }

    private IEnumerator AnimateRoutine(Vector3 targetScale, Quaternion targetRotation)
    {
        Vector3 startScale = transform.localScale;
        Quaternion startRotation = transform.localRotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, amount);
            transform.localRotation = Quaternion.SlerpUnclamped(startRotation, targetRotation, amount);
            yield return null;
        }
        transform.localScale = targetScale;
        transform.localRotation = targetRotation;
        routine = null;
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        transform.localScale = baseScale;
        transform.localRotation = restingRotation;
    }
}
