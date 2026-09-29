using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class HometownHotspot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [SerializeField] private string clueTitle;

    private HometownInvestigationManager investigationManager;
    private Image hotspotImage;
    private TMP_Text checkmark;
    private Color normalColor;
    private bool available;
    private bool investigated;

    public string ClueTitle => clueTitle;

    public void Configure(string title)
    {
        clueTitle = title;
    }

    public void Initialize(HometownInvestigationManager owner)
    {
        investigationManager = owner;
        hotspotImage = GetComponent<Image>();

        Transform checkmarkTransform = transform.Find("Checkmark");
        checkmark = checkmarkTransform != null
            ? checkmarkTransform.GetComponent<TMP_Text>()
            : null;

        normalColor = hotspotImage.color;

        if (checkmark != null)
        {
            checkmark.gameObject.SetActive(investigated);
        }
    }

    public void SetAvailable(bool value)
    {
        available = value;
        gameObject.SetActive(value);

        if (!value)
        {
            return;
        }

        if (hotspotImage != null)
        {
            hotspotImage.color = investigated
                ? new Color(0.32f, 0.62f, 0.42f, 0.48f)
                : normalColor;
        }

        if (checkmark != null)
        {
            checkmark.gameObject.SetActive(investigated);
        }
    }

    public bool MarkInvestigated()
    {
        if (investigated)
        {
            return false;
        }

        investigated = true;
        hotspotImage.color = new Color(0.32f, 0.62f, 0.42f, 0.48f);

        if (checkmark != null)
        {
            checkmark.gameObject.SetActive(true);
        }

        transform.localScale = Vector3.one;
        return true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!available)
        {
            return;
        }

        if (!investigated)
        {
            hotspotImage.color = new Color(1f, 0.83f, 0.26f, 0.68f);
        }

        investigationManager.ShowInteractionHint(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!available)
        {
            return;
        }

        if (!investigated)
        {
            hotspotImage.color = normalColor;
        }

        investigationManager.HideInteractionHint();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (available)
        {
            investigationManager.Inspect(this);
        }
    }

    private void Update()
    {
        if (!available || investigated)
        {
            return;
        }

        float scale = 1f + Mathf.Sin(Time.unscaledTime * 2.4f) * 0.025f;
        transform.localScale = Vector3.one * scale;
    }

    private void OnDisable()
    {
        transform.localScale = Vector3.one;
    }
}
