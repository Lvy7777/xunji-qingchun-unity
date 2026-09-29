using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public sealed class HometownUIFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.045f;
    [SerializeField] private float pressedScale = 0.965f;
    [SerializeField] private float transitionSpeed = 14f;
    [SerializeField] private Color hoverTint = new Color(1f, 0.92f, 0.68f, 1f);

    private Selectable selectable;
    private Graphic graphic;
    private Vector3 baseScale;
    private Color baseColor;
    private bool hovered;
    private bool pressed;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        graphic = GetComponent<Graphic>();
        baseScale = transform.localScale;

        if (graphic != null)
        {
            baseColor = graphic.color;
        }
    }

    private void OnEnable()
    {
        transform.localScale = baseScale;
        ApplyColor(baseColor);
    }

    private void Update()
    {
        float multiplier = pressed ? pressedScale : (hovered ? hoverScale : 1f);
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            baseScale * multiplier,
            Time.unscaledDeltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (selectable != null && selectable.interactable)
        {
            hovered = true;
            ApplyColor(hoverTint);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        pressed = false;
        ApplyColor(baseColor);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectable != null && selectable.interactable)
        {
            pressed = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }

    private void ApplyColor(Color color)
    {
        if (graphic != null)
        {
            graphic.color = color;
        }
    }
}
