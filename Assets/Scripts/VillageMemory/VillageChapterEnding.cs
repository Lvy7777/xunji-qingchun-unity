using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class VillageChapterEnding : MonoBehaviour
{
    [SerializeField] private string endingSceneName = "Ending";
    [SerializeField] private float fadeDuration = 0.28f;
    [SerializeField] private float resultHoldDuration = 0.85f;

    private VillageMemoryController controller;
    private GameObject endingLayer;
    private GameObject dimPanel;
    private GameObject evaluationPanel;
    private GameObject rewardPanel;
    private GameObject studyBookPanel;
    private GameObject completionPanel;
    private TextMeshProUGUI evaluationBadgeText;
    private TextMeshProUGUI evaluationTitleText;
    private TextMeshProUGUI evaluationDetailText;
    private TextMeshProUGUI bookText;
    private Button finishBookButton;

    private bool initialized;
    private bool endingStarted;
    private bool endingCompleted;
    private bool perfectResult;
    private bool rewardAttempted;
    private bool rewardGranted;
    private bool transitionRequested;

    public bool EndingStarted => endingStarted;
    public bool EndingCompleted => endingCompleted;
    public bool IsPerfectResult => perfectResult;
    public bool RewardGranted => rewardGranted;
    public bool CanLoadEndingScene => Application.CanStreamedLevelBeLoaded(endingSceneName);
    public string EvaluationTitle => evaluationTitleText != null ? evaluationTitleText.text : string.Empty;
    public string EvaluationBadge => evaluationBadgeText != null ? evaluationBadgeText.text : string.Empty;
    public string EvaluationDetail => evaluationDetailText != null ? evaluationDetailText.text : string.Empty;
    public string StudyBookSummary => bookText != null ? bookText.text : string.Empty;
    public string NextSceneName => endingSceneName;

    public void Initialize(VillageMemoryController owner, Transform canvasParent)
    {
        if (initialized)
        {
            return;
        }

        controller = owner;
        BuildInterface(canvasParent);
        initialized = true;
        ResetEnding();
    }

    public void BeginEnding(bool isPerfect)
    {
        if (!initialized || endingStarted)
        {
            return;
        }

        endingStarted = true;
        perfectResult = isPerfect;
        ConfigureEvaluation();

        endingLayer.SetActive(true);
        dimPanel.SetActive(true);
        evaluationPanel.SetActive(true);
        rewardPanel.SetActive(false);
        studyBookPanel.SetActive(false);
        completionPanel.SetActive(false);
        finishBookButton.interactable = false;

        StartCoroutine(PlayEnding());
    }

    public void CompleteEndingImmediatelyForTesting()
    {
        if (!endingStarted)
        {
            return;
        }

        StopAllCoroutines();
        GrantVillageMemoryOnce();
        evaluationPanel.SetActive(false);
        rewardPanel.SetActive(false);
        studyBookPanel.SetActive(false);

        CanvasGroup dimGroup = dimPanel.GetComponent<CanvasGroup>();
        dimPanel.SetActive(true);
        dimGroup.alpha = 1f;
        dimGroup.blocksRaycasts = true;

        SetStudyBookContent();
        ShowCompletionImmediately();
        endingCompleted = true;
    }

    public void ResetEnding()
    {
        StopAllCoroutines();
        endingStarted = false;
        endingCompleted = false;
        perfectResult = false;
        rewardAttempted = false;
        rewardGranted = false;
        transitionRequested = false;

        if (!initialized)
        {
            return;
        }

        endingLayer.SetActive(false);
        HidePanel(dimPanel);
        HidePanel(evaluationPanel);
        HidePanel(rewardPanel);
        HidePanel(studyBookPanel);
        HidePanel(completionPanel);
        finishBookButton.interactable = true;
    }

    public void CompleteStudyBook()
    {
        if (!endingCompleted || transitionRequested)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(endingSceneName))
        {
            Debug.LogError("[VillageMemory] Ending scene is not included in Build Settings.");
            return;
        }

        transitionRequested = true;
        finishBookButton.interactable = false;
        SceneManager.LoadScene(endingSceneName);
    }

    private IEnumerator PlayEnding()
    {
        CanvasGroup dimGroup = dimPanel.GetComponent<CanvasGroup>();
        dimGroup.alpha = 0f;
        yield return Fade(dimGroup, 0f, 1f, fadeDuration);

        CanvasGroup resultGroup = evaluationPanel.GetComponent<CanvasGroup>();
        resultGroup.alpha = 0f;
        resultGroup.transform.localScale = Vector3.one * (perfectResult ? 0.72f : 0.88f);

        if (perfectResult)
        {
            yield return FadeAndScale(resultGroup, 0f, 1f, 0.72f, 1.08f, fadeDuration + 0.10f);
            yield return Scale(resultGroup.transform, 1.08f, 1f, 0.16f);
        }
        else
        {
            yield return FadeAndScale(resultGroup, 0f, 1f, 0.88f, 1f, fadeDuration);
        }

        yield return new WaitForSecondsRealtime(resultHoldDuration);
        yield return Fade(resultGroup, 1f, 0f, fadeDuration);
        evaluationPanel.SetActive(false);

        GrantVillageMemoryOnce();
        if (rewardGranted)
        {
            yield return ShowMemoryReward();
        }

        yield return ShowStudyBook();
        ShowCompletion();
        endingCompleted = true;
    }

    private void GrantVillageMemoryOnce()
    {
        if (rewardAttempted)
        {
            return;
        }

        rewardAttempted = true;
        GameProgress progress = GameProgress.Ensure();
        rewardGranted = progress.TryGrantVillageMemory();
    }

    private void ConfigureEvaluation()
    {
        evaluationBadgeText.text = perfectResult ? "PERFECT" : string.Empty;
        evaluationTitleText.text = perfectResult ? "完美完成" : "任务完成";
        evaluationDetailText.text = perfectResult ? "两项乡村实践全部完成" : "已完成一项乡村实践";

        evaluationBadgeText.gameObject.SetActive(perfectResult);
        evaluationTitleText.color = perfectResult
            ? new Color(1f, 0.82f, 0.27f, 1f)
            : new Color(0.46f, 1f, 0.61f, 1f);
    }

    private IEnumerator ShowMemoryReward()
    {
        CanvasGroup rewardGroup = rewardPanel.GetComponent<CanvasGroup>();
        rewardPanel.SetActive(true);
        rewardGroup.alpha = 0f;
        rewardGroup.transform.localScale = Vector3.one * 0.84f;
        yield return FadeAndScale(rewardGroup, 0f, 1f, 0.84f, 1f, fadeDuration);
        yield return new WaitForSecondsRealtime(1f);
        yield return FadeAndScale(rewardGroup, 1f, 0f, 1f, 0.92f, fadeDuration);
        rewardPanel.SetActive(false);
    }

    private IEnumerator ShowStudyBook()
    {
        SetStudyBookContent();
        CanvasGroup bookGroup = studyBookPanel.GetComponent<CanvasGroup>();
        studyBookPanel.SetActive(true);
        bookGroup.alpha = 0f;
        yield return Fade(bookGroup, 0f, 1f, fadeDuration);
        yield return new WaitForSecondsRealtime(1.45f);
        yield return Fade(bookGroup, 1f, 0f, fadeDuration);
        studyBookPanel.SetActive(false);
    }

    private void SetStudyBookContent()
    {
        bookText.text =
            "记忆收集：4 / 4\n\n" +
            "红色记忆  已获得\n" +
            "乡土记忆  已获得\n" +
            "青春记忆  已获得\n" +
            "乡村记忆  已获得";
    }

    private void ShowCompletion()
    {
        CanvasGroup completionGroup = completionPanel.GetComponent<CanvasGroup>();
        completionPanel.SetActive(true);
        completionGroup.alpha = 0f;
        completionGroup.blocksRaycasts = true;
        finishBookButton.interactable = true;
        StartCoroutine(Fade(completionGroup, 0f, 1f, fadeDuration));
    }

    private void ShowCompletionImmediately()
    {
        CanvasGroup completionGroup = completionPanel.GetComponent<CanvasGroup>();
        completionPanel.SetActive(true);
        completionGroup.alpha = 1f;
        completionGroup.blocksRaycasts = true;
        finishBookButton.interactable = true;
    }

    private void BuildInterface(Transform parent)
    {
        endingLayer = CreateContainer("VillageChapterEndingLayer", parent);
        dimPanel = CreatePanel("EndingDim", endingLayer.transform, new Color(0.01f, 0.035f, 0.02f, 0.86f),
            Vector2.zero, Vector2.one);

        BuildEvaluationPanel(endingLayer.transform);
        BuildRewardPanel(endingLayer.transform);
        BuildStudyBookPanel(endingLayer.transform);
        BuildCompletionPanel(endingLayer.transform);
    }

    private void BuildEvaluationPanel(Transform parent)
    {
        evaluationPanel = CreatePanel("PracticeEvaluationPanel", parent, new Color(0.035f, 0.12f, 0.07f, 0.98f),
            new Vector2(0.26f, 0.29f), new Vector2(0.74f, 0.71f));

        evaluationBadgeText = CreateText("PerfectBadge", evaluationPanel.transform, string.Empty, 30,
            new Color(1f, 0.67f, 0.18f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.10f, 0.72f), new Vector2(0.90f, 0.88f));
        evaluationTitleText = CreateText("EvaluationTitle", evaluationPanel.transform, string.Empty, 54,
            Color.white, TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.43f), new Vector2(0.92f, 0.70f));
        evaluationDetailText = CreateText("EvaluationDetail", evaluationPanel.transform, string.Empty, 27,
            new Color(0.82f, 0.94f, 0.78f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.40f));
    }

    private void BuildRewardPanel(Transform parent)
    {
        rewardPanel = CreatePanel("MemoryRewardUI", parent, new Color(0.04f, 0.10f, 0.19f, 0.99f),
            new Vector2(0.28f, 0.30f), new Vector2(0.72f, 0.70f));

        CreateText("RewardTitle", rewardPanel.transform, "记忆获得", 34,
            new Color(1f, 0.84f, 0.33f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.87f));
        CreateText("RewardName", rewardPanel.transform, "乡村记忆", 52, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.65f));
        CreateText("RewardSubtitle", rewardPanel.transform, "记录生活", 27,
            new Color(0.72f, 0.91f, 0.72f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.19f), new Vector2(0.92f, 0.38f));
    }

    private void BuildStudyBookPanel(Transform parent)
    {
        studyBookPanel = CreatePanel("StudyBookPanel", parent, new Color(0.95f, 0.91f, 0.79f, 1f),
            new Vector2(0.23f, 0.20f), new Vector2(0.77f, 0.80f));

        CreateText("BookTitle", studyBookPanel.transform, "研学手册", 38,
            new Color(0.18f, 0.13f, 0.08f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.91f));
        bookText = CreateText("BookText", studyBookPanel.transform, string.Empty, 28,
            new Color(0.18f, 0.13f, 0.08f, 1f), TextAlignmentOptions.Left,
            new Vector2(0.16f, 0.16f), new Vector2(0.84f, 0.74f));
    }

    private void BuildCompletionPanel(Transform parent)
    {
        completionPanel = CreatePanel("ChapterCompletePanel", parent, new Color(0.035f, 0.10f, 0.07f, 0.98f),
            new Vector2(0.25f, 0.27f), new Vector2(0.75f, 0.73f));

        CreateText("CompleteTitle", completionPanel.transform, "四段记忆已全部找到", 45,
            new Color(1f, 0.84f, 0.33f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.58f), new Vector2(0.92f, 0.82f));
        CreateText("CompleteSubtitle", completionPanel.transform, "研学手册 · 4 / 4", 27, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.57f));

        finishBookButton = CreateButton("CompleteStudyBookButton", completionPanel.transform, "完成研学手册",
            new Vector2(0.27f, 0.15f), new Vector2(0.73f, 0.34f));
        finishBookButton.onClick.AddListener(CompleteStudyBook);
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

        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static Button CreateButton(string name, Transform parent, string label, Vector2 min, Vector2 max)
    {
        GameObject buttonObject = CreatePanel(name, parent, new Color(0.20f, 0.55f, 0.30f, 1f), min, max);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        CreateText("Label", buttonObject.transform, label, 26, Color.white,
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

    private static void HidePanel(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }

        panel.SetActive(false);
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

    private static IEnumerator FadeAndScale(
        CanvasGroup group,
        float fromAlpha,
        float toAlpha,
        float fromScale,
        float toScale,
        float duration)
    {
        group.alpha = fromAlpha;
        group.transform.localScale = Vector3.one * fromScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float amount = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(fromAlpha, toAlpha, amount);
            group.transform.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, amount);
            yield return null;
        }

        group.alpha = toAlpha;
        group.transform.localScale = Vector3.one * toScale;
        group.blocksRaycasts = toAlpha > 0.01f;
    }

    private static IEnumerator Scale(Transform target, float from, float to, float duration)
    {
        target.localScale = Vector3.one * from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            target.localScale = Vector3.one * Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        target.localScale = Vector3.one * to;
    }
}
