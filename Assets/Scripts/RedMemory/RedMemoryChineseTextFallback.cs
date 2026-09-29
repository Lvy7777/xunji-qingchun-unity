using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class RedMemoryChineseTextFallback : MonoBehaviour
{
    [SerializeField] private Font chineseFont;

    private readonly List<TMP_Text> sourceTexts = new List<TMP_Text>();
    private readonly List<Text> fallbackTexts = new List<Text>();

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        if (chineseFont == null)
        {
            Debug.LogError("[RedMemory] Chinese fallback font is not assigned.");
            yield break;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[RedMemory] UI canvas was not created.");
            yield break;
        }

        TMP_Text[] texts = canvas.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text source = texts[i];
            Text fallback = source.GetComponent<Text>();
            if (fallback == null)
            {
                fallback = source.gameObject.AddComponent<Text>();
            }

            fallback.font = chineseFont;
            fallback.fontSize = Mathf.RoundToInt(source.fontSize);
            fallback.color = source.color;
            fallback.alignment = ToLegacyAlignment(source.alignment);
            fallback.horizontalOverflow = HorizontalWrapMode.Wrap;
            fallback.verticalOverflow = VerticalWrapMode.Overflow;
            fallback.supportRichText = false;
            fallback.raycastTarget = false;

            source.enabled = false;
            sourceTexts.Add(source);
            fallbackTexts.Add(fallback);
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < sourceTexts.Count; i++)
        {
            TMP_Text source = sourceTexts[i];
            Text fallback = fallbackTexts[i];
            if (source == null || fallback == null)
            {
                continue;
            }

            fallback.text = source.text;
            fallback.color = source.color;
            fallback.gameObject.SetActive(source.gameObject.activeSelf);
        }
    }

    private static TextAnchor ToLegacyAlignment(TextAlignmentOptions alignment)
    {
        switch (alignment)
        {
            case TextAlignmentOptions.TopLeft:
                return TextAnchor.UpperLeft;
            case TextAlignmentOptions.Top:
                return TextAnchor.UpperCenter;
            case TextAlignmentOptions.TopRight:
                return TextAnchor.UpperRight;
            case TextAlignmentOptions.Left:
                return TextAnchor.MiddleLeft;
            case TextAlignmentOptions.Right:
                return TextAnchor.MiddleRight;
            case TextAlignmentOptions.BottomLeft:
                return TextAnchor.LowerLeft;
            case TextAlignmentOptions.Bottom:
                return TextAnchor.LowerCenter;
            case TextAlignmentOptions.BottomRight:
                return TextAnchor.LowerRight;
            default:
                return TextAnchor.MiddleCenter;
        }
    }
}