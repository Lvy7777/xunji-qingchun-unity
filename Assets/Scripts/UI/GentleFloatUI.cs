using UnityEngine;

[DisallowMultipleComponent]
public sealed class GentleFloatUI : MonoBehaviour
{
    [SerializeField] private float amplitude = 3f;
    [SerializeField] private float cyclesPerSecond = 0.22f;
    [SerializeField] private float phaseOffset;

    private RectTransform rectTransform;
    private Vector2 basePosition;

    public void Configure(float newAmplitude, float newCyclesPerSecond, float newPhaseOffset)
    {
        amplitude = newAmplitude;
        cyclesPerSecond = newCyclesPerSecond;
        phaseOffset = newPhaseOffset;
    }

    private void Awake()
    {
        rectTransform = transform as RectTransform;
    }

    private void OnEnable()
    {
        if (rectTransform == null)
        {
            rectTransform = transform as RectTransform;
        }

        if (rectTransform != null)
        {
            basePosition = rectTransform.anchoredPosition;
        }
    }

    private void Update()
    {
        if (rectTransform == null)
        {
            return;
        }

        float wave = Mathf.Sin((Time.unscaledTime + phaseOffset) * cyclesPerSecond * Mathf.PI * 2f);
        rectTransform.anchoredPosition = basePosition + Vector2.up * (wave * amplitude);
    }

    private void OnDisable()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = basePosition;
        }
    }
}
