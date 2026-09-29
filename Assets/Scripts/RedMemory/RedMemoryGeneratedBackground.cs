using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class RedMemoryGeneratedBackground : MonoBehaviour
{
    [SerializeField] private Texture2D backgroundTexture;

    private Image colorLayer;

    private IEnumerator Start()
    {
        yield return null;

        GameObject target = GameObject.Find("RedMemoryBackground");
        if (target == null || backgroundTexture == null)
        {
            Debug.LogError("[RedMemory] Generated background could not be bound.");
            yield break;
        }

        Transform existingArt = target.transform.Find("GeneratedBackgroundArt");
        GameObject art = existingArt != null
            ? existingArt.gameObject
            : new GameObject("GeneratedBackgroundArt", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        art.transform.SetParent(target.transform, false);

        RectTransform artRect = art.GetComponent<RectTransform>();
        artRect.anchorMin = Vector2.zero;
        artRect.anchorMax = Vector2.one;
        artRect.offsetMin = Vector2.zero;
        artRect.offsetMax = Vector2.zero;

        RawImage image = art.GetComponent<RawImage>();

        image.texture = backgroundTexture;
        image.color = Color.white;
        image.raycastTarget = false;

        colorLayer = target.GetComponent<Image>();
        if (colorLayer != null)
        {
            colorLayer.color = Color.clear;
        }
    }

    private void LateUpdate()
    {
        if (colorLayer != null)
        {
            colorLayer.color = Color.clear;
        }
    }
}
