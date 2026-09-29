using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class YouthMemoryIntroController : MonoBehaviour
{
    private const float CharacterDelay = 0.035f;
    private const float InactiveCharacterAlpha = 0.5f;

    private Canvas canvas;
    private CanvasGroup fadeGroup;
    private CanvasGroup titleGroup;
    private CanvasGroup dialogueGroup;
    private GameObject titlePanel;
    private GameObject dialoguePanel;
    private GameObject choicePanel;
    private GameObject gameEntryPanel;
    private TMP_Text chapterTitleText;
    private TMP_Text speakerText;
    private TMP_Text dialogueText;
    private TMP_Text continueText;
    private Button advanceButton;
    private Button firstChoiceButton;
    private Button secondChoiceButton;
    private Button startGameButton;
    private RectTransform xiaoHeRect;
    private RectTransform volunteerRect;
    private CanvasGroup xiaoHeGroup;
    private CanvasGroup volunteerGroup;

    private bool revealImmediately;
    private bool advanceRequested;
    private int selectedChoice = -1;
    private bool gameStarted;

private void Awake()
    {
        EnsureEventSystem();

        if (GetComponent<ChineseTextRuntimeFallback>() == null)
        {
            gameObject.AddComponent<ChineseTextRuntimeFallback>();
        }

        BuildInterface();
    }

    private IEnumerator Start()
    {
        ResetPresentation();

        yield return Fade(fadeGroup, 1f, 0f, 0.8f);
        fadeGroup.gameObject.SetActive(false);

        yield return ShowChapterTitle("第三章");
        yield return ShowChapterTitle("青春记忆");
        yield return ShowChapterTitle("原来课堂可以这样玩");

        dialoguePanel.SetActive(true);
        yield return ShowDialogue("小禾", "老师，今天一直学习，好累呀。", false);
        yield return ShowDialogue("志愿者", "那我们换一种方式。", false);
        yield return ShowDialogue("小禾", "什么方式？", false);

        yield return ShowChoice();
        yield return ShowDialogue("志愿者", "玩！", true);

        dialoguePanel.SetActive(false);
        gameEntryPanel.SetActive(true);
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    private void BuildInterface()
    {
        GameObject canvasObject = new GameObject(
            "YouthMemoryCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Transform environment = CreateContainer("Environment", canvas.transform);
        Transform characters = CreateContainer("Characters", canvas.transform);
        Transform uiRoot = CreateContainer("UIRoot", canvas.transform);

        BuildClassroom(environment);
        BuildCharacters(characters);
        BuildTitlePanel(uiRoot);
        BuildDialoguePanel(uiRoot);
        BuildChoicePanel(uiRoot);
        BuildGameEntryPanel(uiRoot);
        BuildFadePanel(uiRoot);
    }

    private void BuildClassroom(Transform parent)
    {
        GameObject classroom = CreatePanel(
            "ClassroomBackground",
            parent,
            new Color(0.74f, 0.84f, 0.92f, 1f),
            Vector2.zero,
            Vector2.one);

        CreatePanel("Wall", classroom.transform, new Color(0.84f, 0.89f, 0.92f, 1f),
            new Vector2(0f, 0.38f), Vector2.one);
        CreatePanel("Floor", classroom.transform, new Color(0.52f, 0.35f, 0.23f, 1f),
            Vector2.zero, new Vector2(1f, 0.38f));

        GameObject blackboard = CreatePanel("Blackboard", classroom.transform, new Color(0.11f, 0.30f, 0.24f, 1f),
            new Vector2(0.26f, 0.50f), new Vector2(0.74f, 0.82f));
        CreateText("BoardText", blackboard.transform, "青春课堂  ·  今日任务", 34,
            new Color(0.85f, 0.96f, 0.82f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.05f, 0.54f), new Vector2(0.95f, 0.84f));
        CreateText("BoardLine", blackboard.transform, "学习，也可以很好玩", 24,
            new Color(0.85f, 0.96f, 0.82f, 0.9f), TextAlignmentOptions.Center,
            new Vector2(0.05f, 0.24f), new Vector2(0.95f, 0.52f));

        GameObject window = CreatePanel("Window", classroom.transform, new Color(0.47f, 0.78f, 0.93f, 0.92f),
            new Vector2(0.06f, 0.56f), new Vector2(0.20f, 0.82f));
        CreatePanel("WindowDividerVertical", window.transform, new Color(0.93f, 0.95f, 0.94f, 1f),
            new Vector2(0.47f, 0f), new Vector2(0.53f, 1f));
        CreatePanel("WindowDividerHorizontal", window.transform, new Color(0.93f, 0.95f, 0.94f, 1f),
            new Vector2(0f, 0.47f), new Vector2(1f, 0.53f));

        CreateDesk(classroom.transform, "DeskLeft", new Vector2(0.10f, 0.14f), new Vector2(0.39f, 0.31f));
        CreateDesk(classroom.transform, "DeskRight", new Vector2(0.61f, 0.14f), new Vector2(0.90f, 0.31f));
    }

    private static void CreateDesk(Transform parent, string name, Vector2 min, Vector2 max)
    {
        GameObject desk = CreatePanel(name, parent, new Color(0.70f, 0.46f, 0.27f, 1f), min, max);
        CreatePanel("DeskTop", desk.transform, new Color(0.86f, 0.64f, 0.37f, 1f),
            new Vector2(0f, 0.72f), Vector2.one);
    }

    private void BuildCharacters(Transform parent)
    {
        GameObject xiaoHe = CreateCharacter(
            "XiaoHe",
            parent,
            "小禾\n占位立绘",
            new Color(0.96f, 0.61f, 0.48f, 1f),
            new Vector2(0.06f, 0.20f),
            new Vector2(0.31f, 0.72f));
        xiaoHeRect = xiaoHe.GetComponent<RectTransform>();
        xiaoHeGroup = xiaoHe.GetComponent<CanvasGroup>();

        GameObject volunteer = CreateCharacter(
            "Volunteer",
            parent,
            "志愿者\n占位立绘",
            new Color(0.37f, 0.62f, 0.85f, 1f),
            new Vector2(0.69f, 0.20f),
            new Vector2(0.94f, 0.72f));
        volunteerRect = volunteer.GetComponent<RectTransform>();
        volunteerGroup = volunteer.GetComponent<CanvasGroup>();
    }

    private static GameObject CreateCharacter(
        string name,
        Transform parent,
        string label,
        Color color,
        Vector2 min,
        Vector2 max)
    {
        GameObject actor = CreatePanel(name, parent, new Color(0f, 0f, 0f, 0f), min, max);
        actor.AddComponent<CanvasGroup>();
        CreatePanel("PortraitPlaceholder", actor.transform, color,
            new Vector2(0.14f, 0.08f), new Vector2(0.86f, 0.86f));
        CreateText("ActorName", actor.transform, label, 30, Color.white, TextAlignmentOptions.Center,
            new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.99f));
        return actor;
    }

    private void BuildTitlePanel(Transform parent)
    {
        titlePanel = CreatePanel("ChapterTitlePanel", parent, new Color(0.02f, 0.06f, 0.12f, 0.82f),
            new Vector2(0.27f, 0.32f), new Vector2(0.73f, 0.68f));
        titleGroup = titlePanel.AddComponent<CanvasGroup>();
        chapterTitleText = CreateText("ChapterTitleText", titlePanel.transform, string.Empty, 52, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.88f));
    }

    private void BuildDialoguePanel(Transform parent)
    {
        dialoguePanel = CreatePanel("DialoguePanel", parent, new Color(0.035f, 0.06f, 0.12f, 0.92f),
            new Vector2(0.11f, 0.035f), new Vector2(0.89f, 0.255f));
        dialogueGroup = dialoguePanel.AddComponent<CanvasGroup>();
        speakerText = CreateText("NameText", dialoguePanel.transform, string.Empty, 30,
            new Color(1f, 0.82f, 0.45f, 1f), TextAlignmentOptions.TopLeft,
            new Vector2(0.05f, 0.62f), new Vector2(0.52f, 0.92f));
        dialogueText = CreateText("DialogueText", dialoguePanel.transform, string.Empty, 30, Color.white,
            TextAlignmentOptions.TopLeft, new Vector2(0.05f, 0.14f), new Vector2(0.79f, 0.66f));
        continueText = CreateText("ContinueIcon", dialoguePanel.transform, "▼", 24,
            new Color(1f, 0.87f, 0.55f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.80f, 0.15f), new Vector2(0.86f, 0.47f));
        advanceButton = CreateButton("ScreenAdvanceButton", dialoguePanel.transform, "继续",
            new Vector2(0.86f, 0.16f), new Vector2(0.96f, 0.62f), OnAdvanceRequested);
    }

    private void BuildChoicePanel(Transform parent)
    {
        choicePanel = CreatePanel("ChoicePanel", parent, new Color(0.035f, 0.06f, 0.12f, 0.96f),
            new Vector2(0.27f, 0.29f), new Vector2(0.73f, 0.70f));
        CreateText("ChoiceTitle", choicePanel.transform, "小禾的提议是……", 30, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.90f));
        firstChoiceButton = CreateButton("ChoiceButtonOne", choicePanel.transform, "① 做个小游戏？",
            new Vector2(0.12f, 0.41f), new Vector2(0.88f, 0.58f), OnFirstChoiceClicked);
        secondChoiceButton = CreateButton("ChoiceButtonTwo", choicePanel.transform, "② 换一种课堂方式？",
            new Vector2(0.12f, 0.19f), new Vector2(0.88f, 0.36f), OnSecondChoiceClicked);
    }

private void BuildGameEntryPanel(Transform parent)
    {
        gameEntryPanel = CreatePanel("YouthPlatformerEntry", parent, new Color(0.07f, 0.11f, 0.13f, 0.93f),
            new Vector2(0.12f, 0.20f), new Vector2(0.50f, 0.76f));
        CreateText("GameTitle", gameEntryPanel.transform, "黏土田野挑战", 46,
            new Color(1f, 0.48f, 0.18f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.90f));
        CreateText("GamePrompt", gameEntryPanel.transform,
            "穿过田野、村落与青春试炼\n移动平台 · 坍塌木台 · 逆风区 · 巡逻泥团\n收集四枚青春记忆徽章", 25, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.07f, 0.30f), new Vector2(0.93f, 0.68f));
        startGameButton = CreateButton("StartButton", gameEntryPanel.transform, "▶  接受挑战",
            new Vector2(0.20f, 0.10f), new Vector2(0.80f, 0.27f), OnStartGameClicked);
    }

    private void BuildFadePanel(Transform parent)
    {
        GameObject fadePanel = CreatePanel("FadePanel", parent, Color.black, Vector2.zero, Vector2.one);
        fadePanel.transform.SetAsLastSibling();
        fadeGroup = fadePanel.AddComponent<CanvasGroup>();
    }

    private IEnumerator ShowChapterTitle(string title)
    {
        chapterTitleText.text = title;
        titlePanel.SetActive(true);
        yield return Fade(titleGroup, 0f, 1f, 0.25f);
        yield return new WaitForSecondsRealtime(0.45f);
        yield return Fade(titleGroup, 1f, 0f, 0.25f);
        titlePanel.SetActive(false);
    }

    private IEnumerator ShowDialogue(string speaker, string content, bool emphasise)
    {
        SetSpeakerVisual(speaker);
        speakerText.text = speaker;
        dialogueText.text = content;
        dialogueText.maxVisibleCharacters = 0;
        continueText.gameObject.SetActive(false);
        advanceButton.interactable = true;
        revealImmediately = false;
        advanceRequested = false;

        if (emphasise)
        {
            StartCoroutine(PunchDialogue());
            dialogueText.transform.localScale = Vector3.one * 1.1f;
        }
        else
        {
            dialogueText.transform.localScale = Vector3.one;
        }

        for (int i = 0; i < content.Length; i++)
        {
            if (revealImmediately)
            {
                break;
            }

            dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSecondsRealtime(CharacterDelay);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        continueText.gameObject.SetActive(true);

        while (!advanceRequested)
        {
            yield return null;
        }

        continueText.gameObject.SetActive(false);
        dialogueText.transform.localScale = Vector3.one;
    }

    private IEnumerator ShowChoice()
    {
        selectedChoice = -1;
        advanceButton.interactable = false;
        choicePanel.SetActive(true);

        while (selectedChoice < 0)
        {
            yield return null;
        }

        choicePanel.SetActive(false);
        advanceButton.interactable = true;
    }

    private void SetSpeakerVisual(string speaker)
    {
        bool xiaoHeSpeaking = speaker == "小禾";
        xiaoHeGroup.alpha = xiaoHeSpeaking ? 1f : InactiveCharacterAlpha;
        volunteerGroup.alpha = xiaoHeSpeaking ? InactiveCharacterAlpha : 1f;
        xiaoHeRect.localScale = xiaoHeSpeaking ? Vector3.one * 1.03f : Vector3.one;
        volunteerRect.localScale = xiaoHeSpeaking ? Vector3.one : Vector3.one * 1.03f;
    }

    private IEnumerator PunchDialogue()
    {
        dialoguePanel.transform.localScale = Vector3.one * 0.98f;
        yield return Scale(dialoguePanel.transform, 0.98f, 1.04f, 0.11f);
        yield return Scale(dialoguePanel.transform, 1.04f, 1f, 0.14f);
    }

    private void OnAdvanceRequested()
    {
        if (dialogueText.maxVisibleCharacters < dialogueText.text.Length)
        {
            revealImmediately = true;
            return;
        }

        advanceRequested = true;
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

private void OnStartGameClicked()
    {
        if (gameStarted)
        {
            return;
        }

        gameStarted = true;
        startGameButton.interactable = false;
        gameEntryPanel.SetActive(false);

        YouthActionGameManager legacyPromptGame = GetComponent<YouthActionGameManager>();
        if (legacyPromptGame != null)
        {
            legacyPromptGame.enabled = false;
        }

        YouthPlatformerGameManager legacyUiPlatformer = GetComponent<YouthPlatformerGameManager>();
        if (legacyUiPlatformer != null)
        {
            legacyUiPlatformer.enabled = false;
        }

        YouthClayPlatformerManager manager = GetComponent<YouthClayPlatformerManager>();
        if (manager == null)
        {
            manager = gameObject.AddComponent<YouthClayPlatformerManager>();
        }

        manager.ChallengeCompleted -= BeginYouthChapterEnding;
        manager.ChallengeCompleted += BeginYouthChapterEnding;
        manager.StartGame();
    }

private void BeginYouthChapterEnding()
    {
        YouthChapterEnding ending = GetComponent<YouthChapterEnding>();
        if (ending == null)
        {
            ending = gameObject.AddComponent<YouthChapterEnding>();
        }

        ending.BeginEnding();
    }


    private void ResetPresentation()
    {
        titlePanel.SetActive(false);
        dialoguePanel.SetActive(false);
        choicePanel.SetActive(false);
        gameEntryPanel.SetActive(false);
        continueText.gameObject.SetActive(false);
        dialoguePanel.transform.localScale = Vector3.one;

        fadeGroup.gameObject.SetActive(true);
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = true;
        fadeGroup.interactable = false;
    }

    private static Transform CreateContainer(string name, Transform parent)
    {
        GameObject container = new GameObject(name, typeof(RectTransform));
        container.transform.SetParent(parent, false);
        Stretch(container.GetComponent<RectTransform>());
        return container.transform;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
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

    private static TMP_Text CreateText(
        string name,
        Transform parent,
        string text,
        float size,
        Color color,
        TextAlignmentOptions alignment,
        Vector2 min,
        Vector2 max)
    {
        GameObject labelObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.enableWordWrapping = true;
        return label;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        string label,
        Vector2 min,
        Vector2 max,
        UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = CreatePanel(name, parent, new Color(0.86f, 0.53f, 0.18f, 1f), min, max);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        button.onClick.AddListener(action);
        CreateText("Label", buttonObject.transform, label, 25, Color.white, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one);
        return button;
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
        group.blocksRaycasts = to > 0.01f;
    }

    private static IEnumerator Scale(Transform target, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            target.localScale = Vector3.one * Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        target.localScale = Vector3.one * to;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
