using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class RedMemoryXiaohePortrait : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null;

        Texture2D portraitTexture = Resources.Load<Texture2D>("Characters/LiTuoTuo_Front");
        GameObject actor = GameObject.Find("XiaoHe");
        Transform placeholder = actor != null ? actor.transform.Find("PortraitPlaceholder") : null;

        if (portraitTexture == null || placeholder == null)
        {
            Debug.LogWarning("[RedMemory] LiTuoTuo portrait could not be bound.");
            yield break;
        }

        Image placeholderImage = placeholder.GetComponent<Image>();
        if (placeholderImage != null)
        {
            placeholderImage.enabled = false;
        }

        Transform existingArt = placeholder.Find("XiaoHeArtwork");
        GameObject art = existingArt != null
            ? existingArt.gameObject
            : new GameObject("XiaoHeArtwork", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(AspectRatioFitter));
        art.transform.SetParent(placeholder, false);

        RectTransform artRect = art.GetComponent<RectTransform>();
        artRect.anchorMin = Vector2.zero;
        artRect.anchorMax = Vector2.one;
        artRect.offsetMin = Vector2.zero;
        artRect.offsetMax = Vector2.zero;

        RawImage portrait = art.GetComponent<RawImage>();

        portrait.texture = portraitTexture;
        portrait.color = Color.white;
        portrait.raycastTarget = false;

        AspectRatioFitter fitter = art.GetComponent<AspectRatioFitter>();

        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = portraitTexture.width / (float)portraitTexture.height;
    }
}
