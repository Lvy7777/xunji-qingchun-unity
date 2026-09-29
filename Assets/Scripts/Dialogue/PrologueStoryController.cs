using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PrologueStoryStepType
{
    Dialogue,
    Choice,
    ShowObject,
    WaitForClick,
    SystemMessage
}

[Serializable]
public sealed class PrologueStoryStep
{
    public PrologueStoryStepType Type;
    public DialogueLine Line;
    public string Message;

    public PrologueStoryStep(PrologueStoryStepType type)
    {
        Type = type;
    }

    public PrologueStoryStep(DialogueSpeaker speaker, string text)
    {
        Type = PrologueStoryStepType.Dialogue;
        Line = new DialogueLine(speaker, text);
    }

    public PrologueStoryStep(PrologueStoryStepType type, string message)
    {
        Type = type;
        Message = message;
    }
}

public sealed class PrologueStoryController : MonoBehaviour
{
    public bool IsCompleted { get; private set; }

    private const string IntroCaption = "七月 · 一次特别的乡村实践";
    private const string LockedMemoryMessage = "这段记忆还没有被找到……";

    private DialogueManager dialogueManager;

    private GameObject dialoguePanel;
    private GameObject choicePanel;
    private GameObject studyBookObject;
    private GameObject interactionHint;
    private GameObject studyBookPanel;
    private GameObject systemMessagePanel;
    private GameObject taskPopup;

    private CanvasGroup fadePanel;
    private CanvasGroup chapterTitle;
    private CanvasGroup studyBookGroup;
    private CanvasGroup backgroundGroup;
    private CanvasGroup dialogueGroup;
    private CanvasGroup xiaoHeGroup;
    private CanvasGroup volunteerGroup;

    private Button screenAdvanceButton;
    private Button firstChoiceButton;
    private Button secondChoiceButton;
    private Button studyBookButton;
    private Button closeBookButton;
    private Button[] memoryButtons;

    private TMP_Text systemMessageText;
    private TMP_Text interactionHintText;
    private TMP_Text studyBookTitle;
    private TMP_Text memoryProgressText;
    private RectTransform studyBookRect;

    private CanvasGroup titleGroup;
    private CanvasGroup closeButtonGroup;
    private CanvasGroup[] memoryBlockGroups;
    private CanvasGroup[] memoryStatusGroups;
    private RectTransform[] memoryStatusRects;

    private int selectedChoice = -1;
    private bool studyBookClicked;
    private bool bookClosed;
    private Coroutine systemMessageRoutine;

    private void Awake()
    {
        BindSceneReferences();

        if (!ReferencesAreReady())
        {
            return;
        }

        ConfigurePresentationComponents();
        ConfigureStudyBookContent();
        BindButtons();
    }

    private IEnumerator Start()
    {
        if (!ReferencesAreReady())
        {
            Debug.LogError("[Prologue] PrologueStoryController could not find its required UI references.");
            yield break;
        }

        ResetPresentation();
        yield return PlayOpeningSequence();

        yield return RunStep(new PrologueStoryStep(
            DialogueSpeaker.XiaoHe,
            "老师老师！你们今天要教我们什么呀？"));

        yield return RunStep(new PrologueStoryStep(
            DialogueSpeaker.Volunteer,
            "今天，我们要一起完成一本特别的研学手册。"));

        yield return RunStep(new PrologueStoryStep(PrologueStoryStepType.Choice));

        if (selectedChoice == 0)
        {
            yield return RunStep(new PrologueStoryStep(
                DialogueSpeaker.Volunteer,
                "好，我们先了解一下它，再一起打开看看。"));
        }

        yield return RunStep(new PrologueStoryStep(
            DialogueSpeaker.XiaoHe,
            "研学手册是什么？"));

        yield return RunStep(new PrologueStoryStep(
            DialogueSpeaker.Volunteer,
            "就是把我们今天看到、听到、学到的东西记录下来。"));

        yield return RunStep(new PrologueStoryStep(PrologueStoryStepType.ShowObject));
        yield return RunStep(new PrologueStoryStep(PrologueStoryStepType.WaitForClick));

        yield return RunStep(new PrologueStoryStep(
            DialogueSpeaker.XiaoHe,
            "可是……这里什么都没有呀。"));

        yield return RunStep(new PrologueStoryStep(
            DialogueSpeaker.Volunteer,
            "那我们就一起把它填满。"));

        dialogueManager.SetInputEnabled(false);
        IsCompleted = true;
        Debug.Log("[Prologue] Interactive prologue flow completed.");
    }

    public void BindSceneReferences()
    {
        dialogueManager = GetComponent<DialogueManager>();

        GameObject canvasObject = GameObject.Find("Canvas");
        Transform canvas = canvasObject != null ? canvasObject.transform : null;
        if (canvas == null)
        {
            return;
        }

        Transform dialogueLayer = canvas.Find("DialogueLayer");
        Transform characterLayer = canvas.Find("CharacterLayer");
        Transform background = canvas.Find("Background");

        dialoguePanel = FindObject(dialogueLayer, "DialoguePanel");
        choicePanel = FindObject(dialogueLayer, "ChoicePanel");
        studyBookObject = FindObject(dialogueLayer, "HandbookCoverRoot")
            ?? FindObject(dialogueLayer, "StudyBookObject");
        interactionHint = FindObject(dialogueLayer, "InteractionHint");
        studyBookPanel = FindObject(dialogueLayer, "HandbookContentPanel")
            ?? FindObject(dialogueLayer, "StudyBookPanel");
        systemMessagePanel = FindObject(dialogueLayer, "SystemMessagePanel");
        taskPopup = FindObject(canvas, "TaskLayer/TaskPopup");

        Transform fadeTransform = canvas.Find("FadePanel");
        Transform chapterTransform = dialogueLayer != null ? dialogueLayer.Find("ChapterTitle") : null;
        Transform xiaoHe = characterLayer != null ? characterLayer.Find("XiaoHe") : null;
        Transform volunteer = characterLayer != null ? characterLayer.Find("Volunteer") : null;

        fadePanel = GetOrAddCanvasGroup(fadeTransform);
        chapterTitle = GetOrAddCanvasGroup(chapterTransform);
        studyBookGroup = GetOrAddCanvasGroup(studyBookPanel != null ? studyBookPanel.transform : null);
        backgroundGroup = GetOrAddCanvasGroup(background);
        dialogueGroup = GetOrAddCanvasGroup(dialoguePanel != null ? dialoguePanel.transform : null);
        xiaoHeGroup = GetOrAddCanvasGroup(xiaoHe);
        volunteerGroup = GetOrAddCanvasGroup(volunteer);

        screenAdvanceButton = FindButton(dialogueLayer, "ScreenAdvanceButton");
        firstChoiceButton = FindButton(dialogueLayer, "ChoicePanel/ChoiceButtonOne");
        secondChoiceButton = FindButton(dialogueLayer, "ChoicePanel/ChoiceButtonTwo");
        studyBookButton = studyBookObject != null ? studyBookObject.GetComponent<Button>() : null;
        closeBookButton = studyBookPanel != null
            ? studyBookPanel.transform.Find("CloseButton")?.GetComponent<Button>()
            : null;

        systemMessageText = FindText(systemMessagePanel, "MessageText");
        interactionHintText = interactionHint != null ? interactionHint.GetComponent<TMP_Text>() : null;
        studyBookTitle = FindText(studyBookPanel, "StudyBookTitle");
        memoryProgressText = FindText(studyBookPanel, "MemoryProgressText");
        studyBookRect = studyBookPanel != null ? studyBookPanel.GetComponent<RectTransform>() : null;

        memoryButtons = new[]
        {
            FindButton(studyBookPanel != null ? studyBookPanel.transform : null, "RedMemoryStatus"),
            FindButton(studyBookPanel != null ? studyBookPanel.transform : null, "HometownMemoryStatus"),
            FindButton(studyBookPanel != null ? studyBookPanel.transform : null, "YouthMemoryStatus"),
            FindButton(studyBookPanel != null ? studyBookPanel.transform : null, "VillageMemoryStatus")
        };

        string[] blockNames =
        {
            "RedMemoryBlock",
            "HometownMemoryBlock",
            "YouthMemoryBlock",
            "VillageMemoryBlock"
        };
        string[] statusNames =
        {
            "RedMemoryStatus",
            "HometownMemoryStatus",
            "YouthMemoryStatus",
            "VillageMemoryStatus"
        };

        memoryBlockGroups = new CanvasGroup[4];
        memoryStatusGroups = new CanvasGroup[4];
        memoryStatusRects = new RectTransform[4];

        for (int i = 0; i < 4; i++)
        {
            Transform block = studyBookPanel != null ? studyBookPanel.transform.Find(blockNames[i]) : null;
            Transform status = studyBookPanel != null ? studyBookPanel.transform.Find(statusNames[i]) : null;
            memoryBlockGroups[i] = GetOrAddCanvasGroup(block);
            memoryStatusGroups[i] = GetOrAddCanvasGroup(status);
            memoryStatusRects[i] = status as RectTransform;
        }

        titleGroup = GetOrAddCanvasGroup(studyBookTitle != null ? studyBookTitle.transform : null);
        closeButtonGroup = GetOrAddCanvasGroup(closeBookButton != null ? closeBookButton.transform : null);
    }

    private void ConfigurePresentationComponents()
    {
        TMP_Text chapterText = chapterTitle.GetComponent<TMP_Text>();
        if (chapterText != null)
        {
            chapterText.text = IntroCaption;
        }

        chapterTitle.transform.SetParent(fadePanel.transform, false);
        chapterTitle.transform.SetAsLastSibling();
        RectTransform chapterRect = chapterTitle.transform as RectTransform;
        if (chapterRect != null)
        {
            chapterRect.anchorMin = new Vector2(0.5f, 0.5f);
            chapterRect.anchorMax = new Vector2(0.5f, 0.5f);
            chapterRect.anchoredPosition = Vector2.zero;
            chapterRect.sizeDelta = new Vector2(1400f, 140f);
        }

        Transform characterLayer = xiaoHeGroup.transform.parent;
        GentleFloatUI xiaoFloat = xiaoHeGroup.GetComponent<GentleFloatUI>();
        if (xiaoFloat == null)
        {
            xiaoFloat = xiaoHeGroup.gameObject.AddComponent<GentleFloatUI>();
        }
        xiaoFloat.Configure(3f, 0.18f, 0f);

        GentleFloatUI volunteerFloat = volunteerGroup.GetComponent<GentleFloatUI>();
        if (volunteerFloat == null)
        {
            volunteerFloat = volunteerGroup.gameObject.AddComponent<GentleFloatUI>();
        }
        volunteerFloat.Configure(2.5f, 0.18f, 1.4f);

        Button[] buttons = characterLayer.root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].GetComponent<UIButtonAnimator>() == null)
            {
                buttons[i].gameObject.AddComponent<UIButtonAnimator>();
            }
        }
    }

    private void ConfigureStudyBookContent()
    {
        interactionHintText.text = "点击研学手册查看";
        studyBookTitle.text = "研学手册";
        memoryProgressText.text = "记忆收集 0 / 4";

        string[] labels =
        {
            "红色记忆 —— 未解锁",
            "乡土记忆 —— 未解锁",
            "青春记忆 —— 未解锁",
            "乡村记忆 —— 未解锁"
        };

        for (int i = 0; i < memoryButtons.Length; i++)
        {
            TMP_Text label = memoryButtons[i].GetComponent<TMP_Text>();
            if (label != null)
            {
                label.text = labels[i];
            }
        }
    }

    private IEnumerator PlayOpeningSequence()
    {
        TMP_Text caption = chapterTitle.GetComponent<TMP_Text>();
        if (caption != null)
        {
            caption.text = IntroCaption;
        }

        chapterTitle.gameObject.SetActive(true);
        yield return Fade(chapterTitle, 0f, 1f, 0.5f);
        yield return new WaitForSecondsRealtime(0.9f);
        yield return Fade(chapterTitle, 1f, 0f, 0.45f);
        chapterTitle.gameObject.SetActive(false);

        backgroundGroup.alpha = 1f;
        yield return Fade(fadePanel, 1f, 0f, 0.65f);
        fadePanel.gameObject.SetActive(false);

        xiaoHeGroup.alpha = 0f;
        volunteerGroup.alpha = 0f;
        xiaoHeGroup.gameObject.SetActive(false);
        volunteerGroup.gameObject.SetActive(false);

        dialoguePanel.SetActive(true);
        yield return Fade(dialogueGroup, 0f, 1f, 0.32f);
    }

    private IEnumerator RunStep(PrologueStoryStep step)
    {
        switch (step.Type)
        {
            case PrologueStoryStepType.Dialogue:
                yield return dialogueManager.ShowLine(step.Line);
                break;

            case PrologueStoryStepType.Choice:
                yield return ShowChoice();
                break;

            case PrologueStoryStepType.ShowObject:
                yield return ShowStudyBookObject();
                break;

            case PrologueStoryStepType.WaitForClick:
                yield return WaitForStudyBook();
                break;

            case PrologueStoryStepType.SystemMessage:
                yield return ShowSystemMessage(step.Message);
                break;
        }
    }

    private IEnumerator ShowChoice()
    {
        selectedChoice = -1;
        dialogueManager.SetInputEnabled(false);
        screenAdvanceButton.interactable = false;
        firstChoiceButton.interactable = true;
        secondChoiceButton.interactable = true;
        choicePanel.SetActive(true);

        while (selectedChoice < 0)
        {
            yield return null;
        }

        choicePanel.SetActive(false);
        screenAdvanceButton.interactable = true;
        dialogueManager.SetInputEnabled(true);
    }

    private IEnumerator ShowStudyBookObject()
    {
        dialogueManager.SetSpeakerVisual(DialogueSpeaker.System);
        dialogueManager.SetInputEnabled(false);
        screenAdvanceButton.interactable = false;

        yield return Fade(dialogueGroup, 1f, 0f, 0.22f);
        dialoguePanel.SetActive(false);

        interactionHint.SetActive(true);
        studyBookButton.interactable = true;
        studyBookObject.SetActive(true);

        CanvasGroup bookGroup = GetOrAddCanvasGroup(studyBookObject.transform);
        yield return FadeAndScale(
            bookGroup,
            studyBookObject.GetComponent<RectTransform>(),
            0f,
            1f,
            0.92f,
            1f,
            0.28f);
    }

    private IEnumerator WaitForStudyBook()
    {
        studyBookClicked = false;

        while (!studyBookClicked)
        {
            yield return null;
        }

        HandbookCoverController coverController = studyBookObject.GetComponent<HandbookCoverController>();
        if (coverController != null)
        {
            yield return coverController.PlayOpenTransition();
        }

        studyBookObject.SetActive(false);
        interactionHint.SetActive(false);

        bookClosed = false;
        closeBookButton.interactable = false;
        SetMemoryButtonsInteractable(false);

        PrepareStudyBookAnimation();
        studyBookPanel.SetActive(true);

        yield return FadeAndScale(
            studyBookGroup,
            studyBookRect,
            0f,
            1f,
            0.92f,
            1f,
            0.28f);

        yield return Fade(titleGroup, 0f, 1f, 0.2f);

        for (int i = 0; i < memoryStatusGroups.Length; i++)
        {
            yield return RevealMemoryCard(i);
            yield return new WaitForSecondsRealtime(0.06f);
        }

        yield return Fade(closeButtonGroup, 0f, 1f, 0.18f);
        closeBookButton.interactable = true;
        SetMemoryButtonsInteractable(true);

        while (!bookClosed)
        {
            yield return null;
        }

        if (systemMessageRoutine != null)
        {
            StopCoroutine(systemMessageRoutine);
            systemMessageRoutine = null;
        }

        systemMessagePanel.SetActive(false);
        yield return FadeAndScale(
            studyBookGroup,
            studyBookRect,
            1f,
            0f,
            1f,
            0.96f,
            0.2f);
        studyBookPanel.SetActive(false);

        dialoguePanel.SetActive(true);
        yield return Fade(dialogueGroup, 0f, 1f, 0.22f);
        screenAdvanceButton.interactable = true;
        dialogueManager.SetInputEnabled(true);
    }

    private void PrepareStudyBookAnimation()
    {
        studyBookGroup.alpha = 0f;
        studyBookGroup.blocksRaycasts = false;
        studyBookGroup.interactable = false;
        studyBookRect.localScale = Vector3.one * 0.92f;

        titleGroup.alpha = 0f;
        closeButtonGroup.alpha = 0f;

        for (int i = 0; i < memoryStatusGroups.Length; i++)
        {
            memoryBlockGroups[i].alpha = 0f;
            memoryStatusGroups[i].alpha = 0f;
            memoryStatusRects[i].localScale = Vector3.one * 0.92f;
        }
    }

    private IEnumerator RevealMemoryCard(int index)
    {
        CanvasGroup blockGroup = memoryBlockGroups[index];
        CanvasGroup statusGroup = memoryStatusGroups[index];
        RectTransform statusRect = memoryStatusRects[index];

        float elapsed = 0f;
        const float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            blockGroup.alpha = amount;
            statusGroup.alpha = amount;
            statusRect.localScale = Vector3.one * Mathf.LerpUnclamped(0.92f, 1f, amount);
            yield return null;
        }

        blockGroup.alpha = 1f;
        statusGroup.alpha = 1f;
        statusRect.localScale = Vector3.one;
    }

    private IEnumerator ShowSystemMessage(string message)
    {
        CanvasGroup messageGroup = GetOrAddCanvasGroup(systemMessagePanel.transform);
        systemMessageText.text = message;
        systemMessagePanel.SetActive(true);
        yield return Fade(messageGroup, 0f, 1f, 0.12f);
        yield return new WaitForSecondsRealtime(1f);
        yield return Fade(messageGroup, 1f, 0f, 0.18f);
        systemMessagePanel.SetActive(false);
        systemMessageRoutine = null;
    }

    private void OnFirstChoiceClicked()
    {
        if (selectedChoice >= 0)
        {
            return;
        }

        selectedChoice = 0;
        firstChoiceButton.interactable = false;
        secondChoiceButton.interactable = false;
    }

    private void OnSecondChoiceClicked()
    {
        if (selectedChoice >= 0)
        {
            return;
        }

        selectedChoice = 1;
        firstChoiceButton.interactable = false;
        secondChoiceButton.interactable = false;
    }

    private void OnStudyBookClicked()
    {
        if (studyBookClicked || !studyBookButton.interactable)
        {
            return;
        }

        studyBookClicked = true;
        studyBookButton.interactable = false;
    }

    private void OnCloseBookClicked()
    {
        if (bookClosed || !closeBookButton.interactable)
        {
            return;
        }

        bookClosed = true;
        closeBookButton.interactable = false;
        SetMemoryButtonsInteractable(false);
    }

    private void OnLockedMemoryClicked()
    {
        if (systemMessageRoutine != null)
        {
            return;
        }

        systemMessageRoutine = StartCoroutine(ShowSystemMessage(LockedMemoryMessage));
    }

    private void BindButtons()
    {
        screenAdvanceButton.onClick.RemoveListener(dialogueManager.RequestAdvance);
        screenAdvanceButton.onClick.AddListener(dialogueManager.RequestAdvance);

        firstChoiceButton.onClick.RemoveListener(OnFirstChoiceClicked);
        firstChoiceButton.onClick.AddListener(OnFirstChoiceClicked);

        secondChoiceButton.onClick.RemoveListener(OnSecondChoiceClicked);
        secondChoiceButton.onClick.AddListener(OnSecondChoiceClicked);

        studyBookButton.onClick.RemoveListener(OnStudyBookClicked);
        studyBookButton.onClick.AddListener(OnStudyBookClicked);

        closeBookButton.onClick.RemoveListener(OnCloseBookClicked);
        closeBookButton.onClick.AddListener(OnCloseBookClicked);

        for (int i = 0; i < memoryButtons.Length; i++)
        {
            memoryButtons[i].onClick.RemoveListener(OnLockedMemoryClicked);
            memoryButtons[i].onClick.AddListener(OnLockedMemoryClicked);
        }
    }

    private void ResetPresentation()
    {
        IsCompleted = false;
        dialogueManager.SetInputEnabled(false);

        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        studyBookObject.SetActive(false);
        interactionHint.SetActive(false);
        studyBookPanel.SetActive(false);
        systemMessagePanel.SetActive(false);
        taskPopup.SetActive(false);

        screenAdvanceButton.interactable = false;

        chapterTitle.gameObject.SetActive(false);
        chapterTitle.alpha = 0f;

        backgroundGroup.alpha = 1f;
        xiaoHeGroup.gameObject.SetActive(true);
        volunteerGroup.gameObject.SetActive(true);
        xiaoHeGroup.alpha = 0f;
        volunteerGroup.alpha = 0f;

        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 1f;
        fadePanel.blocksRaycasts = true;
        fadePanel.interactable = false;

        Image fadeImage = fadePanel.GetComponent<Image>();
        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
        }
    }

    private void SetMemoryButtonsInteractable(bool interactable)
    {
        for (int i = 0; i < memoryButtons.Length; i++)
        {
            memoryButtons[i].interactable = interactable;
        }
    }

    private bool ReferencesAreReady()
    {
        if (dialogueManager == null
            || dialoguePanel == null
            || choicePanel == null
            || studyBookObject == null
            || interactionHint == null
            || studyBookPanel == null
            || systemMessagePanel == null
            || taskPopup == null
            || fadePanel == null
            || chapterTitle == null
            || studyBookGroup == null
            || backgroundGroup == null
            || dialogueGroup == null
            || xiaoHeGroup == null
            || volunteerGroup == null
            || screenAdvanceButton == null
            || firstChoiceButton == null
            || secondChoiceButton == null
            || studyBookButton == null
            || closeBookButton == null
            || systemMessageText == null
            || interactionHintText == null
            || studyBookTitle == null
            || memoryProgressText == null
            || studyBookRect == null
            || memoryButtons == null
            || memoryButtons.Length != 4)
        {
            return false;
        }

        for (int i = 0; i < memoryButtons.Length; i++)
        {
            if (memoryButtons[i] == null
                || memoryBlockGroups[i] == null
                || memoryStatusGroups[i] == null
                || memoryStatusRects[i] == null)
            {
                return false;
            }
        }

        return true;
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

    private static TMP_Text FindText(GameObject root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        Transform found = root.transform.Find(childName);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private static CanvasGroup GetOrAddCanvasGroup(Transform target)
    {
        if (target == null)
        {
            return null;
        }

        CanvasGroup group = target.GetComponent<CanvasGroup>();
        return group != null ? group : target.gameObject.AddComponent<CanvasGroup>();
    }

    private static IEnumerator Fade(
        CanvasGroup group,
        float from,
        float to,
        float duration)
    {
        group.alpha = from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            group.alpha = Mathf.LerpUnclamped(from, to, amount);
            yield return null;
        }

        group.alpha = to;
        group.blocksRaycasts = to > 0.01f;
        group.interactable = to > 0.01f;
    }

    private static IEnumerator FadeAndScale(
        CanvasGroup group,
        RectTransform rect,
        float fromAlpha,
        float toAlpha,
        float fromScale,
        float toScale,
        float duration)
    {
        group.alpha = fromAlpha;
        rect.localScale = Vector3.one * fromScale;
        group.blocksRaycasts = false;
        group.interactable = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            group.alpha = Mathf.LerpUnclamped(fromAlpha, toAlpha, amount);
            rect.localScale = Vector3.one * Mathf.LerpUnclamped(fromScale, toScale, amount);
            yield return null;
        }

        group.alpha = toAlpha;
        rect.localScale = Vector3.one * toScale;
        group.blocksRaycasts = toAlpha > 0.01f;
        group.interactable = toAlpha > 0.01f;
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

        if (studyBookButton != null)
        {
            studyBookButton.onClick.RemoveListener(OnStudyBookClicked);
        }

        if (closeBookButton != null)
        {
            closeBookButton.onClick.RemoveListener(OnCloseBookClicked);
        }

        if (memoryButtons != null)
        {
            for (int i = 0; i < memoryButtons.Length; i++)
            {
                if (memoryButtons[i] != null)
                {
                    memoryButtons[i].onClick.RemoveListener(OnLockedMemoryClicked);
                }
            }
        }
    }
}
