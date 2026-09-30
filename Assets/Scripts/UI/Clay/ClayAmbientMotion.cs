using UnityEngine;

[DisallowMultipleComponent]
public sealed class ClayAmbientMotion : MonoBehaviour
{
    [SerializeField] private float floatAmplitude = 7f;
    [SerializeField] private float rotationAmplitude = 2f;
    [SerializeField] private float duration = 4.8f;
    [SerializeField] private int seed = 1;

    private RectTransform rect;
    private Vector2 basePosition;
    private Quaternion baseRotation;
    private float phase;

    public void Configure(float amplitude, float rotation, float seconds, int motionSeed)
    {
        floatAmplitude = amplitude;
        rotationAmplitude = rotation;
        duration = Mathf.Max(1f, seconds);
        seed = motionSeed;
        phase = Mathf.Abs(seed % 97) * 0.37f;
    }

    private void Awake()
    {
        rect = transform as RectTransform;
        CaptureBasePose();
    }

    private void OnEnable()
    {
        if (rect == null) rect = transform as RectTransform;
        CaptureBasePose();
    }

    private void Update()
    {
        if (rect == null) return;
        float wave = Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2f / duration) + phase);
        rect.anchoredPosition = basePosition + Vector2.up * (wave * floatAmplitude);
        rect.localRotation = baseRotation * Quaternion.Euler(0f, 0f, wave * rotationAmplitude);
    }

    private void OnDisable()
    {
        if (rect == null) return;
        rect.anchoredPosition = basePosition;
        rect.localRotation = baseRotation;
    }

    private void CaptureBasePose()
    {
        if (rect == null) return;
        basePosition = rect.anchoredPosition;
        baseRotation = rect.localRotation;
        phase = Mathf.Abs(seed % 97) * 0.37f;
    }
}
