using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(ChineseTextRuntimeFallback))]
public sealed class VillageMemoryController : MonoBehaviour
{
    [SerializeField] private bool litchiTaskCompleted;
    [SerializeField] private bool posterTaskCompleted;
    [SerializeField] private float titleFadeDuration = 0.35f;
    [SerializeField] private float titleHoldDuration = 1.5f;

    private CanvasGroup titleGroup;
    private CanvasGroup menuGroup;
    private TMP_Text litchiStatusText;
    private TMP_Text posterStatusText;
    private TMP_Text noticeText;
    private Button litchiStartButton;
    private Button posterStartButton;
    private Button endActivityButton;
    private VillageChapterEnding chapterEnding;
    private bool initialized;
    private string lastSelectedTask = string.Empty;

    public bool LitchiTaskCompleted => litchiTaskCompleted;
    public bool PosterTaskCompleted => posterTaskCompleted;
    public bool CanEndActivity => litchiTaskCompleted || posterTaskCompleted;
    public bool IsPerfectCompletion => litchiTaskCompleted && posterTaskCompleted;
    public bool IsEndButtonEnabled => endActivityButton != null && endActivityButton.interactable;
    public string LastSelectedTask => lastSelectedTask;
    public string LitchiStatus => litchiTaskCompleted ? "已完成" : "未完成";
    public string PosterStatus => posterTaskCompleted ? "已完成" : "未完成";
    public bool IsEndingStarted => chapterEnding != null && chapterEnding.EndingStarted;
    public VillageChapterEnding ChapterEnding => chapterEnding;

    private void Awake()
    {
        InitializeInterface();
    }

    private IEnumerator Start()
    {
        ResetActivity();
        yield return PresentChapterTitle();
    }

public void InitializeInterface()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        EnsureEventSystem();
        BuildInterface();
        EnsureMiniGameManagers();
        EnsureChapterEnding();
        ApplyTaskState();
    }

public void ResetActivity()
    {
        InitializeInterface();
        litchiTaskCompleted = false;
        posterTaskCompleted = false;
        lastSelectedTask = string.Empty;
        noticeText.text = "请选择一项乡村实践任务";

        LitchiQuizManager quiz = GetComponent<LitchiQuizManager>();
        if (quiz != null)
        {
            quiz.HideQuiz();
            quiz.ResetQuiz();
        }

        PosterDIYManager poster = GetComponent<PosterDIYManager>();
        if (poster != null)
        {
            poster.HidePosterDIY();
            poster.ResetPoster();
        }

        if (chapterEnding != null)
        {
            chapterEnding.ResetEnding();
        }

        ApplyTaskState();
    }

    public void CompleteLitchiTask()
    {
        InitializeInterface();
        litchiTaskCompleted = true;
        ApplyTaskState();
    }

    public void CompletePosterTask()
    {
        InitializeInterface();
        posterTaskCompleted = true;
        ApplyTaskState();
    }

    public void ShowMenuImmediatelyForTesting()
    {
        InitializeInterface();
        StopAllCoroutines();
        titleGroup.gameObject.SetActive(false);
        menuGroup.gameObject.SetActive(true);
        menuGroup.alpha = 1f;
        menuGroup.interactable = true;
        menuGroup.blocksRaycasts = true;
    }

    private IEnumerator PresentChapterTitle()
    {
        menuGroup.gameObject.SetActive(true);
        menuGroup.alpha = 0f;
        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;

        titleGroup.gameObject.SetActive(true);
        titleGroup.alpha = 0f;
        yield return Fade(titleGroup, 0f, 1f, titleFadeDuration);
        yield return new WaitForSecondsRealtime(titleHoldDuration);
        yield return Fade(titleGroup, 1f, 0f, titleFadeDuration);
        titleGroup.gameObject.SetActive(false);

        yield return Fade(menuGroup, 0f, 1f, titleFadeDuration);
        menuGroup.interactable = true;
        menuGroup.blocksRaycasts = true;
    }

    private void BuildInterface()
    {
        GameObject canvasObject = new GameObject(
            "VillageMemoryCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreatePanel("Background", canvas.transform, new Color(0.91f, 0.94f, 0.84f, 1f), Vector2.zero, Vector2.one);
        CreatePanel("TopColorBand", canvas.transform, new Color(0.20f, 0.43f, 0.25f, 1f),
            new Vector2(0f, 0.88f), Vector2.one);
        CreatePanel("BottomColorBand", canvas.transform, new Color(0.73f, 0.48f, 0.22f, 1f),
            Vector2.zero, new Vector2(1f, 0.035f));

        BuildActivityMenu(canvas.transform);
        BuildChapterTitle(canvas.transform);
    }

    private void BuildChapterTitle(Transform parent)
    {
        GameObject titlePanel = CreatePanel("ChapterTitlePanel", parent, new Color(0.03f, 0.09f, 0.06f, 0.96f),
            Vector2.zero, Vector2.one);
        titleGroup = titlePanel.AddComponent<CanvasGroup>();

        CreateText("ChapterNumber", titlePanel.transform, "第四章", 42, new Color(0.95f, 0.76f, 0.31f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.2f, 0.62f), new Vector2(0.8f, 0.74f));
        CreateText("ChapterName", titlePanel.transform, "乡村记忆", 70, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.2f, 0.43f), new Vector2(0.8f, 0.61f));
        CreateText("ChapterSubtitle", titlePanel.transform, "把生活变成课堂", 34, new Color(0.78f, 0.91f, 0.72f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.2f, 0.31f), new Vector2(0.8f, 0.42f));
    }

    private void BuildActivityMenu(Transform parent)
    {
        GameObject menu = CreatePanel("VillageActivityMenu", parent, new Color(0f, 0f, 0f, 0f),
            Vector2.zero, Vector2.one);
        menuGroup = menu.AddComponent<CanvasGroup>();

        CreateText("MenuTitle", menu.transform, "乡村实践任务", 48, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.20f, 0.885f), new Vector2(0.80f, 0.975f));
        CreateText("MenuDescription", menu.transform, "完成任意一个任务即可获得乡村记忆", 30,
            new Color(0.12f, 0.24f, 0.15f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.12f, 0.77f), new Vector2(0.88f, 0.84f));
        CreateText("PerfectHint", menu.transform, "全部完成可获得：完美完成", 25,
            new Color(0.55f, 0.31f, 0.08f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.12f, 0.71f), new Vector2(0.88f, 0.77f));

        BuildLitchiCard(menu.transform);
        BuildPosterCard(menu.transform);

        noticeText = CreateText("SelectionNotice", menu.transform, "请选择一项乡村实践任务", 24,
            new Color(0.24f, 0.34f, 0.23f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.22f, 0.105f), new Vector2(0.78f, 0.16f));

        endActivityButton = CreateButton("EndVillagePracticeButton", menu.transform, "结束乡村实践",
            new Color(0.20f, 0.50f, 0.28f, 1f), new Vector2(0.38f, 0.035f), new Vector2(0.62f, 0.10f));
        endActivityButton.onClick.AddListener(OnEndActivityClicked);
    }

    private void BuildLitchiCard(Transform parent)
    {
        GameObject card = CreatePanel("LitchiTaskCard", parent, new Color(1f, 0.94f, 0.82f, 1f),
            new Vector2(0.10f, 0.20f), new Vector2(0.48f, 0.67f));

        GameObject icon = CreatePanel("PlaceholderIcon", card.transform, new Color(0.86f, 0.29f, 0.18f, 1f),
            new Vector2(0.36f, 0.65f), new Vector2(0.64f, 0.91f));
        CreateText("IconLetter", icon.transform, "A", 54, Color.white, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one);
        CreateText("TaskTitle", card.transform, "荔枝小博士", 36, new Color(0.30f, 0.16f, 0.08f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.49f), new Vector2(0.92f, 0.63f));
        CreateText("TaskType", card.transform, "知识闯关", 24, new Color(0.48f, 0.34f, 0.22f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.49f));

        litchiStatusText = CreateText("StatusText", card.transform, "未完成", 25, new Color(0.60f, 0.34f, 0.12f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.29f), new Vector2(0.92f, 0.39f));
        litchiStartButton = CreateButton("StartChallengeButton", card.transform, "开始挑战",
            new Color(0.86f, 0.40f, 0.20f, 1f), new Vector2(0.24f, 0.09f), new Vector2(0.76f, 0.25f));
        litchiStartButton.onClick.AddListener(OnLitchiTaskSelected);
    }

    private void BuildPosterCard(Transform parent)
    {
        GameObject card = CreatePanel("PosterTaskCard", parent, new Color(0.84f, 0.93f, 1f, 1f),
            new Vector2(0.52f, 0.20f), new Vector2(0.90f, 0.67f));

        GameObject icon = CreatePanel("PlaceholderIcon", card.transform, new Color(0.22f, 0.51f, 0.78f, 1f),
            new Vector2(0.36f, 0.65f), new Vector2(0.64f, 0.91f));
        CreateText("IconLetter", icon.transform, "B", 54, Color.white, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one);
        CreateText("TaskTitle", card.transform, "我的乡村手抄报", 34, new Color(0.09f, 0.23f, 0.38f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.49f), new Vector2(0.92f, 0.63f));
        CreateText("TaskType", card.transform, "DIY 拼贴", 24, new Color(0.20f, 0.39f, 0.55f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.49f));

        posterStatusText = CreateText("StatusText", card.transform, "未完成", 25, new Color(0.20f, 0.39f, 0.55f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.29f), new Vector2(0.92f, 0.39f));
        posterStartButton = CreateButton("StartCreationButton", card.transform, "开始创作",
            new Color(0.22f, 0.51f, 0.78f, 1f), new Vector2(0.24f, 0.09f), new Vector2(0.76f, 0.25f));
        posterStartButton.onClick.AddListener(OnPosterTaskSelected);
    }

private void OnLitchiTaskSelected()
    {
        if (litchiTaskCompleted)
        {
            return;
        }

        lastSelectedTask = "A";
        noticeText.text = "已选择：荔枝小博士";
        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;
        menuGroup.gameObject.SetActive(false);

        LitchiQuizManager quiz = GetComponent<LitchiQuizManager>();
        if (quiz != null)
        {
            quiz.OpenQuiz();
        }

        Debug.Log("[VillageMemory] Litchi task selected.");
    }

private void OnPosterTaskSelected()
    {
        if (posterTaskCompleted)
        {
            return;
        }

        lastSelectedTask = "B";
        noticeText.text = "已选择：我的乡村手抄报";
        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;
        menuGroup.gameObject.SetActive(false);

        PosterDIYManager poster = GetComponent<PosterDIYManager>();
        if (poster != null)
        {
            poster.OpenPosterDIY();
        }

        Debug.Log("[VillageMemory] Poster task selected.");
    }

private void OnEndActivityClicked()
    {
        if (!CanEndActivity || chapterEnding == null || chapterEnding.EndingStarted)
        {
            return;
        }

        menuGroup.interactable = false;
        menuGroup.blocksRaycasts = false;
        menuGroup.gameObject.SetActive(false);
        endActivityButton.interactable = false;
        chapterEnding.BeginEnding(IsPerfectCompletion);

        Debug.Log(IsPerfectCompletion
            ? "[VillageMemory] Practice completed perfectly."
            : "[VillageMemory] Practice completed.");
    }

    private void ApplyTaskState()
    {
        if (litchiStatusText == null || posterStatusText == null || endActivityButton == null)
        {
            return;
        }

        litchiStatusText.text = LitchiStatus;
        litchiStatusText.color = litchiTaskCompleted
            ? new Color(0.15f, 0.55f, 0.23f, 1f)
            : new Color(0.60f, 0.34f, 0.12f, 1f);

        posterStatusText.text = PosterStatus;
        posterStatusText.color = posterTaskCompleted
            ? new Color(0.15f, 0.55f, 0.23f, 1f)
            : new Color(0.20f, 0.39f, 0.55f, 1f);

        litchiStartButton.interactable = !litchiTaskCompleted;
        posterStartButton.interactable = !posterTaskCompleted;
        endActivityButton.interactable = CanEndActivity
            && (chapterEnding == null || !chapterEnding.EndingStarted);
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
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

        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static Button CreateButton(
        string name,
        Transform parent,
        string label,
        Color color,
        Vector2 min,
        Vector2 max)
    {
        GameObject buttonObject = CreatePanel(name, parent, color, min, max);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        CreateText("Label", buttonObject.transform, label, 25, Color.white, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one);
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


private void EnsureMiniGameManagers()
    {
        Transform canvas = transform.Find("VillageMemoryCanvas");
        if (canvas == null)
        {
            return;
        }

        LitchiQuizManager quiz = GetComponent<LitchiQuizManager>();
        if (quiz == null)
        {
            quiz = gameObject.AddComponent<LitchiQuizManager>();
        }

        PosterDIYManager poster = GetComponent<PosterDIYManager>();
        if (poster == null)
        {
            poster = gameObject.AddComponent<PosterDIYManager>();
        }

        quiz.Initialize(this, canvas);
        poster.Initialize(this, canvas);
    }

private void EnsureChapterEnding()
    {
        Transform canvas = transform.Find("VillageMemoryCanvas");
        if (canvas == null)
        {
            return;
        }

        chapterEnding = GetComponent<VillageChapterEnding>();
        if (chapterEnding == null)
        {
            chapterEnding = gameObject.AddComponent<VillageChapterEnding>();
        }

        chapterEnding.Initialize(this, canvas);
    }



public void ReturnToActivityMenu()
    {
        InitializeInterface();

        LitchiQuizManager quiz = GetComponent<LitchiQuizManager>();
        if (quiz != null)
        {
            quiz.HideQuiz();
        }

        PosterDIYManager poster = GetComponent<PosterDIYManager>();
        if (poster != null)
        {
            poster.HidePosterDIY();
        }

        titleGroup.gameObject.SetActive(false);
        menuGroup.gameObject.SetActive(true);
        menuGroup.alpha = 1f;
        menuGroup.interactable = true;
        menuGroup.blocksRaycasts = true;
        noticeText.text = IsPerfectCompletion
            ? "全部任务完成：完美完成"
            : (CanEndActivity ? "已完成一项任务，可以结束乡村实践" : "请选择一项乡村实践任务");
        ApplyTaskState();
    }
}
