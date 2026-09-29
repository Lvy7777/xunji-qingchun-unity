using UnityEngine;
using UnityEngine.EventSystems;

public sealed class PosterDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static readonly Vector2 PlacedSize = new Vector2(180f, 76f);

    private PosterDIYManager manager;
    private RectTransform posterCanvas;
    private RectTransform dragSurface;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform homeParent;
    private Vector2 homeAnchorMin;
    private Vector2 homeAnchorMax;
    private Vector2 homeOffsetMin;
    private Vector2 homeOffsetMax;
    private Transform dragStartParent;
    private Vector2 dragStartAnchorMin;
    private Vector2 dragStartAnchorMax;
    private Vector2 dragStartOffsetMin;
    private Vector2 dragStartOffsetMax;
    private int dragStartSiblingIndex;
    private bool placed;

    public bool IsPlaced => placed;

    public void Initialize(PosterDIYManager owner, RectTransform targetCanvas, RectTransform surface)
    {
        manager = owner;
        posterCanvas = targetCanvas;
        dragSurface = surface;
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        homeParent = rectTransform.parent;
        homeAnchorMin = rectTransform.anchorMin;
        homeAnchorMax = rectTransform.anchorMax;
        homeOffsetMin = rectTransform.offsetMin;
        homeOffsetMax = rectTransform.offsetMax;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (manager == null)
        {
            return;
        }

        dragStartParent = rectTransform.parent;
        dragStartAnchorMin = rectTransform.anchorMin;
        dragStartAnchorMax = rectTransform.anchorMax;
        dragStartOffsetMin = rectTransform.offsetMin;
        dragStartOffsetMax = rectTransform.offsetMax;
        dragStartSiblingIndex = rectTransform.GetSiblingIndex();
        canvasGroup.blocksRaycasts = false;

        rectTransform.SetParent(dragSurface, true);
        SetPositionFromScreenPoint(eventData.position, eventData.pressEventCamera, dragSurface);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (manager != null)
        {
            SetPositionFromScreenPoint(eventData.position, eventData.pressEventCamera, dragSurface);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (manager == null)
        {
            return;
        }

        canvasGroup.blocksRaycasts = true;

        if (RectTransformUtility.RectangleContainsScreenPoint(posterCanvas, eventData.position, eventData.pressEventCamera))
        {
            rectTransform.SetParent(posterCanvas, false);
            PreparePlacedRect();
            SetPositionFromScreenPoint(eventData.position, eventData.pressEventCamera, posterCanvas);

            if (!placed)
            {
                placed = true;
                manager.RegisterPlacedElement(this);
            }

            return;
        }

        RestoreRect(dragStartParent, dragStartAnchorMin, dragStartAnchorMax, dragStartOffsetMin, dragStartOffsetMax);
        rectTransform.SetSiblingIndex(dragStartSiblingIndex);
    }

    public void PlaceInCanvasForTesting()
    {
        if (manager == null || placed)
        {
            return;
        }

        rectTransform.SetParent(posterCanvas, false);
        PreparePlacedRect();
        rectTransform.anchoredPosition = Vector2.zero;
        placed = true;
        manager.RegisterPlacedElement(this);
    }

    public void ResetToHome()
    {
        if (rectTransform == null)
        {
            return;
        }

        RestoreRect(homeParent, homeAnchorMin, homeAnchorMax, homeOffsetMin, homeOffsetMax);
        placed = false;
        canvasGroup.blocksRaycasts = true;
    }

    private void PreparePlacedRect()
    {
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = PlacedSize;
    }

    private void RestoreRect(
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax)
    {
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
    }

    private void SetPositionFromScreenPoint(Vector2 screenPoint, Camera eventCamera, RectTransform target)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, screenPoint, eventCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }
}
