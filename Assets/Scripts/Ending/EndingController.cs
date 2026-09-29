using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum EndingPhase
{
    Opening,
    StudyBook,
    FinalDialogue,
    RealMedia,
    FinalScreen
}

[DisallowMultipleComponent]
[RequireComponent(typeof(ChineseTextRuntimeFallback))]
public sealed class EndingController : MonoBehaviour
{
    private static readonly string[] MemoryNames =
    {
        "红色记忆",
        "乡土记忆",
        "青春记忆",
        "乡村记忆"
    };

    private static readonly DialogueLine[] FinalDialogueLines =
    {
        new DialogueLine(DialogueSpeaker.XiaoHe, "老师，我们已经把家乡的故事都写进去了。"),
        new DialogueLine(DialogueSpeaker.Volunteer, "嗯。"),
        new DialogueLine(DialogueSpeaker.XiaoHe, "那以后……还会有人记得吗？"),
        new DialogueLine(DialogueSpeaker.Volunteer, "如果我们愿意记下来，愿意讲给更多人听，它就不会消失。"),
        new DialogueLine(DialogueSpeaker.XiaoHe, "那我以后也要把这些故事讲给别人听。"),
        new DialogueLine(DialogueSpeaker.System, "研学手册完成")
    };

    private static readonly Color[] MemoryColors =
    {
        new Color(0.82f, 0.25f, 0.20f, 1f),
        new Color(0.75f, 0.51f, 0.22f, 1f),
        new Color(0.22f, 0.53f, 0.78f, 1f),
        new Color(0.23f, 0.62f, 0.34f, 1f)
    };

    [SerializeField] private float openingFadeDuration = 1.25f;
    [SerializeField] private float openingHoldDuration = 1.1f;
    [SerializeField, Range(0.4f, 0.6f)] private float memoryRevealInterval = 0.5f;
    [SerializeField] private float bookFadeDuration = 0.45f;
    [SerializeField] private float mandatoryPauseDuration = 1f;
    [SerializeField] private float backgroundMusicVolume = 0.55f;

    private GameObject canvasObject;
    private GameObject openingPanel;
    private GameObject studyBookPanel;
    private GameObject dialogueLayer;
    private GameObject characterLayer;
    private CanvasGroup openingMessageGroup;
    private CanvasGroup studyBookGroup;
    private TextMeshProUGUI bookCompleteText;
    private TextMeshProUGUI dialogueNameText;
    private TextMeshProUGUI dialogueBodyText;
    private Button dialogueNextButton;
    private readonly List<Image> memorySlotImages = new List<Image>();
    private readonly List<RectTransform> memorySlotRects = new List<RectTransform>();
    private readonly List<string> revealedMemoryNames = new List<string>();
    private readonly List<string> shownDialogueHistory = new List<string>();

    private DialogueManager dialogueManager;
    private RealMediaSequence realMediaSequence;
    private AudioSource backgroundMusic;
    private bool initialized;
    private bool sequenceStarted;
    private bool progressWasComplete;
    private bool progressWarningIssued;
    private bool bookCompleted;
    private bool mandatoryPauseActive;
    private EndingPhase currentPhase = EndingPhase.Opening;

    public EndingPhase CurrentPhase => currentPhase;
    public bool ProgressWasComplete => progressWasComplete;
    public bool ProgressWarningIssued => progressWarningIssued;
    public bool BookCompleted => bookCompleted;
    public bool MandatoryPauseActive => mandatoryPauseActive;
    public int LitMemoryCount => revealedMemoryNames.Count;
    public float MusicVolume => backgroundMusic != null ? backgroundMusic.volume : 0f;
    public string OpeningMessage => "四段记忆已经全部找到";
    public string BookCollectionText => "记忆收集：4 / 4";
    public string[] ExpectedMemoryOrder => (string[])MemoryNames.Clone();
    public IList<string> RevealedMemoryNames => revealedMemoryNames.AsReadOnly();
    public IList<string> ShownDialogueHistory => shownDialogueHistory.AsReadOnly();
    public RealMediaSequence MediaSequence => realMediaSequence;

    private void Awake()
    {
        InitializeInterface();
    }

    private void Start()
    {
        if (!sequenceStarted)
        {
            sequenceStarted = true;
            StartCoroutine(PlayEnding());
        }
    }

    public void InitializeInterface()
    {
        if (initialized)
        {
            return;
        }

        EnsureEventSystem();
        BuildInterface();
        ValidateProgress();
        initialized = true;
        ResetVisualState();
    }

    public string[] GetExpectedDialogueTexts()
    {
        string[] result = new string[FinalDialogueLines.Length];
        for (int i = 0; i < FinalDialogueLines.Length; i++)
        {
            result[i] = FinalDialogueLines[i].Text;
        }

        return result;
    }

    public DialogueSpeaker[] GetExpectedDialogueSpeakers()
    {
        DialogueSpeaker[] result = new DialogueSpeaker[FinalDialogueLines.Length];
        for (int i = 0; i < FinalDialogueLines.Length; i++)
        {
            result[i] = FinalDialogueLines[i].Speaker;
        }

        return result;
    }

    public void CompleteBookImmediatelyForTesting()
    {
        InitializeInterface();
        StopAllCoroutines();
        revealedMemoryNames.Clear();

        for (int i = 0; i < MemoryNames.Length; i++)
        {
            RevealMemoryImmediately(i);
        }

        bookCompleted = true;
        bookCompleteText.gameObject.SetActive(true);
        bookCompleteText.alpha = 1f;
        openingPanel.SetActive(false);
        studyBookPanel.SetActive(false);
        ShowDialogueStage();
        currentPhase = EndingPhase.FinalDialogue;
    }

    public void BeginMandatoryPauseForTesting()
    {
        InitializeInterface();
        BeginMandatoryPause();
    }

    public void EndMandatoryPauseForTesting()
    {
        InitializeInterface();
        EndMandatoryPause();
    }

    public void CompleteEntireEndingForTesting()
    {
        CompleteBookImmediatelyForTesting();
        shownDialogueHistory.Clear();

        for (int i = 0; i < FinalDialogueLines.Length; i++)
        {
            shownDialogueHistory.Add(FinalDialogueLines[i].Text);
        }

        dialogueNameText.text = "系统";
        dialogueBodyText.text = "研学手册完成";
        dialogueBodyText.maxVisibleCharacters = int.MaxValue;
        dialogueLayer.SetActive(false);
        characterLayer.SetActive(false);
        currentPhase = EndingPhase.RealMedia;
        realMediaSequence.CompleteImmediatelyForTesting();
        currentPhase = EndingPhase.FinalScreen;
    }

    private IEnumerator PlayEnding()
    {
        currentPhase = EndingPhase.Opening;
        openingPanel.SetActive(true);
        openingMessageGroup.alpha = 0f;
        yield return new WaitForSecondsRealtime(0.35f);
        yield return Fade(openingMessageGroup, 0f, 1f, openingFadeDuration);
        yield return new WaitForSecondsRealtime(openingHoldDuration);

        currentPhase = EndingPhase.StudyBook;
        studyBookPanel.SetActive(true);
        studyBookGroup.alpha = 0f;
        yield return Fade(studyBookGroup, 0f, 1f, bookFadeDuration);
        yield return Fade(openingMessageGroup, 1f, 0f, bookFadeDuration);
        openingPanel.SetActive(false);

        for (int i = 0; i < MemoryNames.Length; i++)
        {
            yield return RevealMemory(i);
            yield return new WaitForSecondsRealtime(memoryRevealInterval);
        }

        bookCompleted = true;
        bookCompleteText.gameObject.SetActive(true);
        bookCompleteText.alpha = 0f;
        yield return FadeText(bookCompleteText, 0f, 1f, bookFadeDuration);
        yield return new WaitForSecondsRealtime(0.9f);
        yield return CloseBook();

        studyBookPanel.SetActive(false);
        ShowDialogueStage();
        currentPhase = EndingPhase.FinalDialogue;
        yield return PlayFinalDialogue();

        dialogueLayer.SetActive(false);
        characterLayer.SetActive(false);
        currentPhase = EndingPhase.RealMedia;
        yield return StartCoroutine(realMediaSequence.PlaySequence());
        currentPhase = EndingPhase.FinalScreen;
    }

    private IEnumerator PlayFinalDialogue()
    {
        for (int i = 0; i < FinalDialogueLines.Length; i++)
        {
            if (i == 3)
            {
                BeginMandatoryPause();
                yield return new WaitForSecondsRealtime(mandatoryPauseDuration);
                EndMandatoryPause();
            }

            shownDialogueHistory.Add(FinalDialogueLines[i].Text);
            yield return dialogueManager.ShowLine(FinalDialogueLines[i]);
        }
    }

    private void BeginMandatoryPause()
    {
        mandatoryPauseActive = true;
        dialogueManager.SetInputEnabled(false);
        backgroundMusic.volume = backgroundMusicVolume * 0.35f;
        dialogueNextButton.interactable = false;
    }

    private void EndMandatoryPause()
    {
        mandatoryPauseActive = false;
        backgroundMusic.volume = backgroundMusicVolume * 0.72f;
        dialogueManager.SetInputEnabled(true);
    }

    private IEnumerator RevealMemory(int index)
    {
        RevealMemoryImmediately(index);
        RectTransform slotRect = memorySlotRects[index];
        Image slotImage = memorySlotImages[index];
        Vector3 startScale = Vector3.one * 0.86f;
        slotRect.localScale = startScale;

        float elapsed = 0f;
        const float duration = 0.24f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.Clamp01(elapsed / duration);
            slotRect.localScale = Vector3.Lerp(startScale, Vector3.one, amount);
            slotImage.color = Color.Lerp(new Color(0.23f, 0.23f, 0.22f, 1f), MemoryColors[index], amount);
            yield return null;
        }

        slotRect.localScale = Vector3.one;
        slotImage.color = MemoryColors[index];
    }

    private void RevealMemoryImmediately(int index)
    {
        if (!revealedMemoryNames.Contains(MemoryNames[index]))
        {
            revealedMemoryNames.Add(MemoryNames[index]);
        }

        memorySlotImages[index].color = MemoryColors[index];
        memorySlotRects[index].localScale = Vector3.one;
        Transform status = memorySlotRects[index].Find("Status");
        if (status != null)
        {
            status.GetComponent<TMP_Text>().text = "已获得";
        }
    }

    private IEnumerator CloseBook()
    {
        Vector3 startScale = Vector3.one;
        Vector3 endScale = new Vector3(0.02f, 1f, 1f);
        float elapsed = 0f;

        while (elapsed < bookFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.Clamp01(elapsed / bookFadeDuration);
            studyBookGroup.alpha = Mathf.Lerp(1f, 0f, amount);
            studyBookGroup.transform.localScale = Vector3.Lerp(startScale, endScale, amount);
            yield return null;
        }

        studyBookGroup.alpha = 0f;
        studyBookGroup.transform.localScale = endScale;
    }

    private void ShowDialogueStage()
    {
        characterLayer.SetActive(true);
        dialogueLayer.SetActive(true);
        dialogueManager.BindSceneReferences();
        dialogueManager.SetCharactersVisibleForExploration();
        dialogueManager.SetInputEnabled(true);
    }

    private void ValidateProgress()
    {
        GameProgress progress = GameProgress.Ensure();
        progressWasComplete = progress.HasRedMemory
            && progress.HasHometownMemory
            && progress.HasYouthMemory
            && progress.HasVillageMemory;

        if (progressWasComplete)
        {
            return;
        }

        progressWarningIssued = true;
        List<string> missing = new List<string>();
        if (!progress.HasRedMemory) missing.Add("红色记忆");
        if (!progress.HasHometownMemory) missing.Add("乡土记忆");
        if (!progress.HasYouthMemory) missing.Add("青春记忆");
        if (!progress.HasVillageMemory) missing.Add("乡村记忆");

        Debug.LogWarning("[Ending] GameProgress is not 4/4. Missing: " + string.Join("、", missing.ToArray()) + ". Ending will continue safely.");
    }

    private void BuildInterface()
    {
        canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreatePanel("EndingBackground", canvas.transform, new Color(0.025f, 0.045f, 0.055f, 1f),
            Vector2.zero, Vector2.one);

        BuildOpening(canvas.transform);
        BuildStudyBook(canvas.transform);
        BuildCharacterLayer(canvas.transform);
        BuildDialogueLayer(canvas.transform);

        backgroundMusic = gameObject.GetComponent<AudioSource>();
        if (backgroundMusic == null)
        {
            backgroundMusic = gameObject.AddComponent<AudioSource>();
        }
        backgroundMusic.playOnAwake = true;
        backgroundMusic.loop = true;
        backgroundMusic.volume = backgroundMusicVolume;

        dialogueManager = gameObject.GetComponent<DialogueManager>();
        if (dialogueManager == null)
        {
            dialogueManager = gameObject.AddComponent<DialogueManager>();
        }
        dialogueManager.BindSceneReferences();

        realMediaSequence = gameObject.GetComponent<RealMediaSequence>();
        if (realMediaSequence == null)
        {
            realMediaSequence = gameObject.AddComponent<RealMediaSequence>();
        }
        realMediaSequence.Initialize(canvas.transform);
    }

    private void BuildOpening(Transform parent)
    {
        openingPanel = CreatePanel("OpeningBlackScreen", parent, Color.black, Vector2.zero, Vector2.one);
        TextMeshProUGUI openingText = CreateText("OpeningMessage", openingPanel.transform,
            "四段记忆已经全部找到", 48, new Color(0.94f, 0.88f, 0.72f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.15f, 0.42f), new Vector2(0.85f, 0.58f));
        openingMessageGroup = openingText.gameObject.AddComponent<CanvasGroup>();
    }

    private void BuildStudyBook(Transform parent)
    {
        studyBookPanel = CreatePanel("StudyBookPanel", parent, new Color(0.10f, 0.075f, 0.05f, 0.98f),
            new Vector2(0.18f, 0.12f), new Vector2(0.82f, 0.88f));
        studyBookGroup = studyBookPanel.GetComponent<CanvasGroup>();

        CreateText("CollectionText", studyBookPanel.transform, "记忆收集：4 / 4", 38,
            new Color(0.96f, 0.84f, 0.57f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.85f), new Vector2(0.92f, 0.95f));
        CreateText("BookTitle", studyBookPanel.transform, "研学手册", 48, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.73f), new Vector2(0.92f, 0.84f));

        for (int i = 0; i < MemoryNames.Length; i++)
        {
            float top = 0.68f - i * 0.135f;
            GameObject slot = CreatePanel("MemorySlot_0" + (i + 1), studyBookPanel.transform,
                new Color(0.23f, 0.23f, 0.22f, 1f),
                new Vector2(0.18f, top - 0.105f), new Vector2(0.82f, top));
            CreateText("MemoryName", slot.transform, MemoryNames[i], 29, Color.white,
                TextAlignmentOptions.MidlineLeft, new Vector2(0.07f, 0f), new Vector2(0.64f, 1f));
            CreateText("Status", slot.transform, "等待汇聚", 23, new Color(0.88f, 0.86f, 0.78f, 1f),
                TextAlignmentOptions.MidlineRight, new Vector2(0.62f, 0f), new Vector2(0.93f, 1f));

            memorySlotImages.Add(slot.GetComponent<Image>());
            memorySlotRects.Add(slot.GetComponent<RectTransform>());
        }

        bookCompleteText = CreateText("BookCompleteText", studyBookPanel.transform, "研学手册完成", 38,
            new Color(1f, 0.78f, 0.25f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.10f, 0.04f), new Vector2(0.90f, 0.14f));
    }

    private void BuildCharacterLayer(Transform parent)
    {
        characterLayer = CreateContainer("CharacterLayer", parent);
        BuildCharacter("XiaoHe", characterLayer.transform, "小禾", new Color(0.34f, 0.58f, 0.78f, 1f),
            new Vector2(0.08f, 0.24f), new Vector2(0.36f, 0.86f));
        BuildCharacter("Volunteer", characterLayer.transform, "志愿者", new Color(0.36f, 0.64f, 0.42f, 1f),
            new Vector2(0.64f, 0.24f), new Vector2(0.92f, 0.86f));
    }

    private void BuildCharacter(string objectName, Transform parent, string label, Color color, Vector2 min, Vector2 max)
    {
        GameObject character = CreatePanel(objectName, parent, color, min, max);
        character.GetComponent<Image>().raycastTarget = false;
        CreateText("CharacterLabel", character.transform, label + "\n角色占位", 32, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.63f));
    }

    private void BuildDialogueLayer(Transform parent)
    {
        dialogueLayer = CreateContainer("DialogueLayer", parent);
        GameObject panel = CreatePanel("DialoguePanel", dialogueLayer.transform,
            new Color(0.025f, 0.045f, 0.075f, 0.96f),
            new Vector2(0.10f, 0.035f), new Vector2(0.90f, 0.255f));

        dialogueNameText = CreateText("NameText", panel.transform, string.Empty, 29,
            new Color(1f, 0.82f, 0.42f, 1f), TextAlignmentOptions.TopLeft,
            new Vector2(0.045f, 0.62f), new Vector2(0.42f, 0.91f));
        dialogueBodyText = CreateText("DialogueText", panel.transform, string.Empty, 29, Color.white,
            TextAlignmentOptions.TopLeft, new Vector2(0.045f, 0.12f), new Vector2(0.79f, 0.67f));
        CreateText("ContinueIcon", panel.transform, "▼", 24, new Color(1f, 1f, 1f, 0.82f),
            TextAlignmentOptions.Center, new Vector2(0.89f, 0.12f), new Vector2(0.96f, 0.30f));

        dialogueNextButton = CreateButton("NextButton", panel.transform, "继续",
            new Vector2(0.82f, 0.32f), new Vector2(0.96f, 0.66f));
    }

    private void ResetVisualState()
    {
        currentPhase = EndingPhase.Opening;
        sequenceStarted = false;
        bookCompleted = false;
        mandatoryPauseActive = false;
        revealedMemoryNames.Clear();
        shownDialogueHistory.Clear();

        openingPanel.SetActive(true);
        openingMessageGroup.alpha = 0f;
        studyBookPanel.SetActive(false);
        studyBookGroup.alpha = 0f;
        studyBookGroup.transform.localScale = Vector3.one;
        bookCompleteText.gameObject.SetActive(false);
        dialogueLayer.SetActive(false);
        characterLayer.SetActive(false);
        backgroundMusic.volume = backgroundMusicVolume;

        for (int i = 0; i < memorySlotImages.Count; i++)
        {
            memorySlotImages[i].color = new Color(0.23f, 0.23f, 0.22f, 1f);
            memorySlotRects[i].localScale = Vector3.one;
            Transform status = memorySlotRects[i].Find("Status");
            if (status != null)
            {
                status.GetComponent<TMP_Text>().text = "等待汇聚";
            }
        }

        realMediaSequence.ResetSequence();
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    private static GameObject CreateContainer(string name, Transform parent)
    {
        GameObject container = new GameObject(name, typeof(RectTransform));
        container.transform.SetParent(parent, false);
        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return container;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        return panel;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 min, Vector2 max)
    {
        GameObject buttonObject = CreatePanel(name, parent, new Color(0.35f, 0.32f, 0.26f, 1f), min, max);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        CreateText("Label", buttonObject.transform, label, 24, Color.white,
            TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
        return button;
    }

    private static TextMeshProUGUI CreateText(
        string name,
        Transform parent,
        string content,
        float size,
        Color color,
        TextAlignmentOptions alignment,
        Vector2 min,
        Vector2 max)
    {
        GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = content;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.enableWordWrapping = true;
        return label;
    }

    private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        group.alpha = from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = to;
    }

    private static IEnumerator FadeText(TMP_Text text, float from, float to, float duration)
    {
        text.alpha = from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            text.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        text.alpha = to;
    }
}
