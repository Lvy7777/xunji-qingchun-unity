using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ClayDecorationLayer : MonoBehaviour
{
    private static readonly Vector2[] Anchors =
    {
        new Vector2(0.045f, 0.88f),
        new Vector2(0.94f, 0.90f),
        new Vector2(0.075f, 0.12f),
        new Vector2(0.93f, 0.17f),
        new Vector2(0.16f, 0.94f),
        new Vector2(0.84f, 0.07f)
    };

    public void Populate(string sceneName)
    {
        if (transform.childCount > 0) return;
        int sceneSeed = ClayThemeTokens.StableHash(sceneName);
        for (int i = 0; i < Anchors.Length; i++)
        {
            bool star = (i % 3) != 0;
            GameObject item = new GameObject(
                star ? "ClayStar" : "ClayBubble",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(ClayAmbientMotion));
            item.transform.SetParent(transform, false);

            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = Anchors[i];
            rect.anchorMax = Anchors[i];
            rect.pivot = new Vector2(0.5f, 0.5f);
            float size = star ? 34f + i * 4f : 96f + i * 9f;
            rect.sizeDelta = Vector2.one * size;
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.Euler(0f, 0f, (i - 2) * 7f);

            Image image = item.GetComponent<Image>();
            image.sprite = star
                ? ClayShapeFactory.Star(96, sceneSeed + i)
                : ClayShapeFactory.Circle(96, sceneSeed + i);
            Color tint = ClayThemeTokens.Resolve(ClayTone.Auto, sceneSeed + i * 13);
            tint.a = star ? 0.72f : 0.22f;
            image.color = tint;
            image.raycastTarget = false;

            ClayAmbientMotion motion = item.GetComponent<ClayAmbientMotion>();
            motion.Configure(star ? 5f : 9f, star ? 3f : 1.2f, 4.5f + i * 0.45f, sceneSeed + i);
        }
    }
}
