using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HometownMemoryController : MonoBehaviour
{
    [SerializeField] private float titleFadeDuration = 0.25f;
    [SerializeField] private float titleStayDuration = 0.35f;
    [SerializeField] private float dialogueFadeDuration = 0.35f;

    private DialogueManager dialogueManager;
    private HometownInvestigationManager investigationManager;

    private GameObject dialoguePanel;
    private GameObject choicePanel;
    private GameObject investigationHud;
    private CanvasGroup dialogueGroup;
    private CanvasGroup fadePanel;
    private CanvasGroup chapterTitleGroup;
    private TMP_Text chapterTitleText;
    private Button screenAdvanceButton;
    private Button firstChoiceButton;
    private Button secondChoiceButton;
    private AudioSource ambienceSource;
    private RectTransform xiaoHeRect;
    private RectTransform volunteerRect;

    private int selectedChoice = -1;
    private bool matchGameCompleted;

    private void Awake()
    {
        BindSceneReferences();
        BindButtons();
    }

    private IEnumerator Start()
    {
        if (!ReferencesAreReady())
        {
            Debug.LogError("[HometownMemory] Required scene references are missing.");
            yield break;
        }

        ResetPresentation();

        yield return Fade(fadePanel, 1f, 0f, 0.8f);
        fadePanel.gameObject.SetActive(false);

        yield return ShowChapterTitle("第二章");
        yield return ShowChapterTitle("乡土记忆");
        yield return ShowChapterTitle("藏在围屋里的声音");

        dialoguePanel.SetActive(true);
        dialogueGroup.alpha = 1f;

        yield return ShowDialogue(DialogueSpeaker.XiaoHe, "老师，那我们家乡还有什么特别的？");
        yield return ShowDialogue(DialogueSpeaker.Volunteer, "当然有。");
        yield return new WaitForSecondsRealtime(0.5f);

        yield return LowerAmbienceAndSpeak();
        yield return ShowChoice();
        yield return PlayChoiceFollowUp();
        yield return ShowDialogue(DialogueSpeaker.Volunteer, "我们自己去找找看吧。");

        yield return EnterInvestigationMode();
    }

    private void BindSceneReferences()
    {
        Transform uiRoot = transform.Find("UIRoot");
        Transform dialogueLayer = uiRoot != null ? uiRoot.Find("DialogueLayer") : null;

        dialogueManager = GetComponent<DialogueManager>();
        investigationManager = GetComponent<HometownInvestigationManager>();

        dialoguePanel = FindObject(dialogueLayer, "DialoguePanel");
        choicePanel = FindObject(dialogueLayer, "ChoicePanel");
        investigationHud = FindObject(uiRoot, "InvestigationHUD");

        dialogueGroup = dialoguePanel != null
            ? dialoguePanel.GetComponent<CanvasGroup>()
            : null;
        fadePanel = GetCanvasGroup("UIRoot/FadePanel");
        chapterTitleGroup = GetCanvasGroup("UIRoot/ChapterTitlePanel");
        chapterTitleText = FindText("UIRoot/ChapterTitlePanel/ChapterTitleText");

        screenAdvanceButton = FindButton(dialogueLayer, "ScreenAdvanceButton");
        firstChoiceButton = FindButton(dialogueLayer, "ChoicePanel/ChoiceButtonOne");
        secondChoiceButton = FindButton(dialogueLayer, "ChoicePanel/ChoiceButtonTwo");

        Transform ambience = transform.Find("Environment/AmbiencePlaceholder");
        ambienceSource = ambience != null ? ambience.GetComponent<AudioSource>() : null;

        Transform xiaoHe = transform.Find("Characters/XiaoHe");
        Transform volunteer = transform.Find("Characters/Volunteer");
        xiaoHeRect = xiaoHe as RectTransform;
        volunteerRect = volunteer as RectTransform;
    }

    private IEnumerator ShowChapterTitle(string title)
    {
        chapterTitleText.text = title;
        chapterTitleGroup.gameObject.SetActive(true);
        yield return Fade(chapterTitleGroup, 0f, 1f, titleFadeDuration);
        yield return new WaitForSecondsRealtime(titleStayDuration);
        yield return Fade(chapterTitleGroup, 1f, 0f, titleFadeDuration);
        chapterTitleGroup.gameObject.SetActive(false);
    }

    private IEnumerator LowerAmbienceAndSpeak()
    {
        if (ambienceSource != null)
        {
            yield return FadeAudio(ambienceSource, ambienceSource.volume, 0.25f, 0.2f);
        }

        yield return ShowDialogue(DialogueSpeaker.Volunteer, "你听。");
    }

    private IEnumerator ShowChoice()
    {
        selectedChoice = -1;
        dialogueManager.SetInputEnabled(false);
        screenAdvanceButton.interactable = false;
        choicePanel.SetActive(true);

        while (selectedChoice < 0)
        {
            yield return null;
        }

        choicePanel.SetActive(false);
        screenAdvanceButton.interactable = true;
        dialogueManager.SetInputEnabled(true);
    }

    private IEnumerator PlayChoiceFollowUp()
    {
        if (selectedChoice == 0)
        {
            yield return ShowDialogue(
                DialogueSpeaker.XiaoHe,
                "我好像听到了很多东西，可是又说不出来是什么。");
            yield break;
        }

        yield return ShowDialogue(DialogueSpeaker.XiaoHe, "声音也能变成记忆吗？");
        yield return ShowDialogue(DialogueSpeaker.Volunteer, "有些东西不一定写在书里。");
    }

private IEnumerator EnterInvestigationMode()
    {
        dialogueManager.SetInputEnabled(false);
        screenAdvanceButton.interactable = false;

        yield return Fade(dialogueGroup, 1f, 0f, dialogueFadeDuration);
        dialoguePanel.SetActive(false);

        dialogueManager.SetCharactersVisibleForExploration();
        yield return new WaitForSecondsRealtime(0.12f);
        yield return MoveCharactersToSides();

        investigationHud.SetActive(true);
        investigationManager.BeginInvestigation();
    }

    private IEnumerator MoveCharactersToSides()
    {
        Vector2 xiaoHeStart = xiaoHeRect.anchoredPosition;
        Vector2 volunteerStart = volunteerRect.anchoredPosition;
        Vector2 xiaoHeTarget = new Vector2(-460f, xiaoHeStart.y);
        Vector2 volunteerTarget = new Vector2(460f, volunteerStart.y);
        const float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.Clamp01(elapsed / duration);
            xiaoHeRect.anchoredPosition = Vector2.Lerp(xiaoHeStart, xiaoHeTarget, amount);
            volunteerRect.anchoredPosition = Vector2.Lerp(volunteerStart, volunteerTarget, amount);
            yield return null;
        }

        xiaoHeRect.anchoredPosition = xiaoHeTarget;
        volunteerRect.anchoredPosition = volunteerTarget;
    }

    private IEnumerator ShowDialogue(DialogueSpeaker speaker, string text)
    {
        yield return dialogueManager.ShowLine(new DialogueLine(speaker, text));
    }

    private void OnFirstChoiceClicked()
    {
        if (selectedChoice < 0)
        {
            selectedChoice = 0;
        }
    }

    private void OnSecondChoiceClicked()
    {
        if (selectedChoice < 0)
        {
            selectedChoice = 1;
        }
    }

    private void BindButtons()
    {
        if (screenAdvanceButton != null && dialogueManager != null)
        {
            screenAdvanceButton.onClick.RemoveListener(dialogueManager.RequestAdvance);
            screenAdvanceButton.onClick.AddListener(dialogueManager.RequestAdvance);
        }

        if (firstChoiceButton != null)
        {
            firstChoiceButton.onClick.RemoveListener(OnFirstChoiceClicked);
            firstChoiceButton.onClick.AddListener(OnFirstChoiceClicked);
        }

        if (secondChoiceButton != null)
        {
            secondChoiceButton.onClick.RemoveListener(OnSecondChoiceClicked);
            secondChoiceButton.onClick.AddListener(OnSecondChoiceClicked);
        }
    }

    private void ResetPresentation()
    {
        dialogueManager.SetSpeakerVisual(DialogueSpeaker.System);
        dialogueManager.SetInputEnabled(true);

        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        investigationHud.SetActive(false);

        chapterTitleGroup.gameObject.SetActive(false);
        chapterTitleGroup.alpha = 0f;

        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 1f;
        fadePanel.blocksRaycasts = true;
        fadePanel.interactable = false;

        screenAdvanceButton.interactable = true;
    }

    private bool ReferencesAreReady()
    {
        return dialogueManager != null
            && investigationManager != null
            && dialoguePanel != null
            && choicePanel != null
            && investigationHud != null
            && dialogueGroup != null
            && fadePanel != null
            && chapterTitleGroup != null
            && chapterTitleText != null
            && screenAdvanceButton != null
            && firstChoiceButton != null
            && secondChoiceButton != null
            && xiaoHeRect != null
            && volunteerRect != null;
    }

    private CanvasGroup GetCanvasGroup(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<CanvasGroup>() : null;
    }

    private TMP_Text FindText(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private static GameObject FindObject(Transform root, string path)
    {
        Transform found = root != null ? root.Find(path) : null;
        return found != null ? found.gameObject : null;
    }

    private static Button FindButton(Transform root, string path)
    {
        Transform found = root != null ? root.Find(path) : null;
        return found != null ? found.GetComponent<Button>() : null;
    }

    private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        group.alpha = from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        group.alpha = to;
        group.blocksRaycasts = to > 0.01f;
    }

    private static IEnumerator FadeAudio(AudioSource source, float from, float to, float duration)
    {
        source.volume = from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        source.volume = to;
    }

public void OnMatchGameCompleted()
    {
        if (matchGameCompleted)
        {
            return;
        }

        matchGameCompleted = true;
        Debug.Log("[HometownMemory] Match game completed.");

        HometownChapterEnding ending = GetComponent<HometownChapterEnding>();
        if (ending == null)
        {
            Debug.LogError("[HometownMemory] HometownChapterEnding is missing.");
            return;
        }

        ending.BeginEnding();
    }

    private void OnDestroy()
    {
        if (screenAdvanceButton != null && dialogueManager != null)
        {
            screenAdvanceButton.onClick.RemoveListener(dialogueManager.RequestAdvance);
        }

        if (firstChoiceButton != null)
        {
            firstChoiceButton.onClick.RemoveListener(OnFirstChoiceClicked);
        }

        if (secondChoiceButton != null)
        {
            secondChoiceButton.onClick.RemoveListener(OnSecondChoiceClicked);
        }
    }
}
