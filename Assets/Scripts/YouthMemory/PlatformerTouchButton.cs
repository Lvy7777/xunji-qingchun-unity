using System;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class PlatformerTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public event Action<bool> HeldChanged;

    public void OnPointerDown(PointerEventData eventData)
    {
        HeldChanged?.Invoke(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        HeldChanged?.Invoke(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HeldChanged?.Invoke(false);
    }
}
