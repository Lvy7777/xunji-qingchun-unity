using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DialogueManager : MonoBehaviour
{
    [SerializeField] private float characterDelay = 0.035f;
    [SerializeField] private float characterTransitionDuration = 0.12f;

    private TMP_Text nameText;
    private TMP_Text dialogueText;
    private TMP_Text continueIcon;
    private Button nextButton;
    private RectTransform xiaoHeRect;
    private RectTransform volunteerRect;
    private CanvasGroup xiaoHeGroup;
    private CanvasGroup volunteerGroup;

    private bool isTyping;
    private bool revealImmediately;
    private bool advanceRequested;
    private bool inputEnabled = true;
    private Coroutine speakerTransition;

private void Awake()
    {
        BindSceneReferences();
        ApplyRandomSceneVariant();
    }

    public void BindSceneReferences()
    {
        GameObject namedCanvas = GameObject.Find("Canvas");
        Transform canvas = namedCanvas != null ? namedCanvas.transform : null;

        if (canvas == null)
        {
            Canvas sceneCanvas = FindObjectOfType<Canvas>();
            canvas = sceneCanvas != null ? sceneCanvas.transform : null;
        }

        if (canvas == null)
        {
            return;
        }

        Transform dialoguePanel = canvas.Find("DialogueLayer/DialoguePanel");

        if (dialoguePanel == null)
        {
            dialoguePanel = canvas.Find("UIRoot/DialogueLayer/DialoguePanel");
        }
        nameText = dialoguePanel != null
            ? dialoguePanel.Find("NameText").GetComponent<TMP_Text>()
            : null;
        dialogueText = dialoguePanel != null
            ? dialoguePanel.Find("DialogueText").GetComponent<TMP_Text>()
            : null;
        continueIcon = dialoguePanel != null
            ? dialoguePanel.Find("ContinueIcon").GetComponent<TMP_Text>()
            : null;
        nextButton = dialoguePanel != null
            ? dialoguePanel.Find("NextButton").GetComponent<Button>()
            : null;

        Transform xiaoHe = canvas.Find("CharacterLayer/XiaoHe");
        Transform volunteer = canvas.Find("CharacterLayer/Volunteer");

        if (xiaoHe == null || volunteer == null)
        {
            xiaoHe = canvas.Find("Characters/XiaoHe");
            volunteer = canvas.Find("Characters/Volunteer");
        }
        xiaoHeRect = xiaoHe as RectTransform;
        volunteerRect = volunteer as RectTransform;
        xiaoHeGroup = xiaoHe != null ? xiaoHe.GetComponent<CanvasGroup>() : null;
        volunteerGroup = volunteer != null ? volunteer.GetComponent<CanvasGroup>() : null;

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(RequestAdvance);
            nextButton.onClick.AddListener(RequestAdvance);
        }
    }

    public IEnumerator ShowLine(DialogueLine line)
    {
        if (!ReferencesAreReady())
        {
            BindSceneReferences();
        }

        if (!ReferencesAreReady())
        {
            Debug.LogError("[Prologue] DialogueManager could not find its required UI references.");
            yield break;
        }

        SetInputEnabled(true);
        SetSpeakerVisual(line.Speaker);
        nameText.text = SpeakerName(line.Speaker);
        dialogueText.text = line.Text;
        dialogueText.maxVisibleCharacters = 0;
        continueIcon.gameObject.SetActive(false);

        isTyping = true;
        revealImmediately = false;
        advanceRequested = false;

        for (int i = 0; i < line.Text.Length; i++)
        {
            if (revealImmediately)
            {
                break;
            }

            dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSecondsRealtime(characterDelay);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        isTyping = false;
        continueIcon.gameObject.SetActive(true);

        while (!advanceRequested)
        {
            yield return null;
        }

        continueIcon.gameObject.SetActive(false);
    }

    public void RequestAdvance()
    {
        if (!inputEnabled)
        {
            return;
        }

        if (isTyping)
        {
            revealImmediately = true;
        }
        else
        {
            advanceRequested = true;
        }
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;

        if (nextButton != null)
        {
            nextButton.interactable = enabled;
        }
    }

    public void SetSpeakerVisual(DialogueSpeaker speaker)
    {
        if (speakerTransition != null)
        {
            StopCoroutine(speakerTransition);
        }

        speakerTransition = StartCoroutine(AnimateSpeakerVisuals(speaker));
    }

    public void SetCharactersVisibleForExploration()
    {
        if (speakerTransition != null)
        {
            StopCoroutine(speakerTransition);
        }

        speakerTransition = StartCoroutine(AnimateExplorationVisuals());
    }

private IEnumerator AnimateSpeakerVisuals(DialogueSpeaker speaker)
    {
        bool hideAllCharacters = speaker == DialogueSpeaker.System;
        bool showXiaoHe = !hideAllCharacters;
        bool showVolunteer = !hideAllCharacters;

        PrepareCharacterForTransition(xiaoHeGroup, showXiaoHe);
        PrepareCharacterForTransition(volunteerGroup, showVolunteer);

        float xiaoHeStartAlpha = xiaoHeGroup.alpha;
        float volunteerStartAlpha = volunteerGroup.alpha;
        Vector3 xiaoHeStartScale = xiaoHeRect.localScale;
        Vector3 volunteerStartScale = volunteerRect.localScale;

        float xiaoHeTargetAlpha = speaker == DialogueSpeaker.XiaoHe ? 1f : 0f;
        float volunteerTargetAlpha = speaker == DialogueSpeaker.Volunteer ? 1f : 0f;
        Vector3 xiaoHeTargetScale = speaker == DialogueSpeaker.XiaoHe ? Vector3.one * 1.03f : Vector3.one;
        Vector3 volunteerTargetScale = speaker == DialogueSpeaker.Volunteer ? Vector3.one * 1.03f : Vector3.one;

        float elapsed = 0f;

        while (elapsed < characterTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.Clamp01(elapsed / characterTransitionDuration);

            xiaoHeGroup.alpha = Mathf.Lerp(xiaoHeStartAlpha, xiaoHeTargetAlpha, amount);
            volunteerGroup.alpha = Mathf.Lerp(volunteerStartAlpha, volunteerTargetAlpha, amount);
            xiaoHeRect.localScale = Vector3.Lerp(xiaoHeStartScale, xiaoHeTargetScale, amount);
            volunteerRect.localScale = Vector3.Lerp(volunteerStartScale, volunteerTargetScale, amount);
            yield return null;
        }

        FinishCharacterTransition(xiaoHeGroup, xiaoHeRect, xiaoHeTargetAlpha, xiaoHeTargetScale);
        FinishCharacterTransition(volunteerGroup, volunteerRect, volunteerTargetAlpha, volunteerTargetScale);
        speakerTransition = null;
    }

    private IEnumerator AnimateExplorationVisuals()
    {
        PrepareCharacterForTransition(xiaoHeGroup, true);
        PrepareCharacterForTransition(volunteerGroup, true);

        float xiaoHeStartAlpha = xiaoHeGroup.alpha;
        float volunteerStartAlpha = volunteerGroup.alpha;
        Vector3 xiaoHeStartScale = xiaoHeRect.localScale;
        Vector3 volunteerStartScale = volunteerRect.localScale;

        float elapsed = 0f;
        while (elapsed < characterTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.Clamp01(elapsed / characterTransitionDuration);
            xiaoHeGroup.alpha = Mathf.Lerp(xiaoHeStartAlpha, 0f, amount);
            volunteerGroup.alpha = Mathf.Lerp(volunteerStartAlpha, 0f, amount);
            xiaoHeRect.localScale = Vector3.Lerp(xiaoHeStartScale, Vector3.one, amount);
            volunteerRect.localScale = Vector3.Lerp(volunteerStartScale, Vector3.one, amount);
            yield return null;
        }

        FinishCharacterTransition(xiaoHeGroup, xiaoHeRect, 0f, Vector3.one);
        FinishCharacterTransition(volunteerGroup, volunteerRect, 0f, Vector3.one);
        speakerTransition = null;
    }

    private bool ReferencesAreReady()
    {
        return nameText != null
            && dialogueText != null
            && continueIcon != null
            && nextButton != null
            && xiaoHeRect != null
            && volunteerRect != null
            && xiaoHeGroup != null
            && volunteerGroup != null;
    }

    private static string SpeakerName(DialogueSpeaker speaker)
    {
        switch (speaker)
        {
            case DialogueSpeaker.XiaoHe:
                return "小禾";
            case DialogueSpeaker.Volunteer:
                return "志愿者";
            default:
                return "系统";
        }
    }

    private void OnDestroy()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(RequestAdvance);
        }
    }


private void ApplyRandomSceneVariant()
    {
        const int variantCount = 3;
        int variant = Random.Range(0, variantCount);
        Color[] tints =
        {
            Color.white,
            new Color(0.82f, 0.90f, 1f, 1f),
            new Color(1f, 0.88f, 0.76f, 1f)
        };
        Color[] overlayColors =
        {
            new Color(0.04f, 0.08f, 0.15f, 0.04f),
            new Color(0.08f, 0.18f, 0.32f, 0.16f),
            new Color(0.36f, 0.16f, 0.08f, 0.13f)
        };
        Vector2[] offsets =
        {
            Vector2.zero,
            new Vector2(-16f, 4f),
            new Vector2(16f, -3f)
        };

        GameObject namedCanvas = GameObject.Find("Canvas");
        Transform canvas = namedCanvas != null ? namedCanvas.transform : null;

        if (canvas == null)
        {
            Canvas sceneCanvas = FindObjectOfType<Canvas>();
            canvas = sceneCanvas != null ? sceneCanvas.transform : null;
        }

        if (canvas == null)
        {
            return;
        }

        Transform background = canvas.Find("Environment/HakkaHouseBackground")
            ?? canvas.Find("Environment/Background")
            ?? canvas.Find("Background")
            ?? canvas.Find("SceneBackground");

        if (background == null)
        {
            return;
        }

        RawImage rawImage = background.GetComponentInChildren<RawImage>(true);
        Image image = rawImage == null
            ? background.GetComponentInChildren<Image>(true)
            : null;

        if (rawImage != null)
        {
            rawImage.color = tints[variant];
        }
        else if (image != null)
        {
            image.color = tints[variant];
        }

        RectTransform rect = background as RectTransform;
        if (rect != null)
        {
            rect.anchoredPosition = offsets[variant];
        }

        Transform existingOverlay = canvas.Find("RandomAtmosphereOverlay");
        GameObject overlay = existingOverlay != null
            ? existingOverlay.gameObject
            : new GameObject(
                "RandomAtmosphereOverlay",
                typeof(RectTransform),
                typeof(Image));
        overlay.transform.SetParent(canvas, false);

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = overlayColors[variant];
        overlayImage.raycastTarget = false;
        overlay.transform.SetSiblingIndex(background.GetSiblingIndex() + 1);
    }

    private static void PrepareCharacterForTransition(CanvasGroup group, bool shouldShow)
    {
        if (shouldShow && !group.gameObject.activeSelf)
        {
            group.gameObject.SetActive(true);
            group.alpha = 0f;
        }
    }

    private static void FinishCharacterTransition(
        CanvasGroup group,
        RectTransform rect,
        float targetAlpha,
        Vector3 targetScale)
    {
        group.alpha = targetAlpha;
        rect.localScale = targetScale;
        group.gameObject.SetActive(targetAlpha > 0.01f);
    }
}
