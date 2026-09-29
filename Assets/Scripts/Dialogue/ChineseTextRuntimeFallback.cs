using TMPro;
using UnityEngine;

// Applies the project-owned pre-baked Noto Sans SC asset to every dialogue label.
// Keeping it static avoids TMP atlas mutation errors during Play Mode.
public sealed class ChineseTextRuntimeFallback : MonoBehaviour
{
    private TMP_FontAsset chineseFontAsset;

    private void Awake()
    {
        chineseFontAsset = Resources.Load<TMP_FontAsset>("Fonts/NotoSansSC-GameTextV3");
        if (chineseFontAsset == null)
        {
            Debug.LogError("[Text] NotoSansSC-GameTextV3 font asset is missing.");
        }
    }

    private void LateUpdate()
    {
        if (chineseFontAsset == null)
        {
            return;
        }

        TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>(true);
        for (int index = 0; index < allTexts.Length; index++)
        {
            TMP_Text text = allTexts[index];
            if (text == null)
            {
                continue;
            }

            if (text.font != chineseFontAsset)
            {
                text.font = chineseFontAsset;
            }

            text.ForceMeshUpdate();
        }
    }
}
