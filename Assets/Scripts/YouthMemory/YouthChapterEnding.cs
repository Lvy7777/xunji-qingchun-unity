using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class YouthChapterEnding : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "VillageMemory";
    [SerializeField] private float fadeDuration = 0.3f;

    private Transform interfaceParent;
    private GameObject endingLayer;
    private GameObject dialoguePanel;
    private GameObject dimPanel;
    private GameObject rewardPanel;
    private GameObject bookPanel;
    private GameObject completionPanel;
    private TextMeshProUGUI speakerText;
    private TextMeshProUGUI dialogueText;
    private TextMeshProUGUI bookText;
    private Button advanceButton;
    private Button continueJourneyButton;
    private bool advanceRequested;
    private bool endingStarted;
    private bool endingCompleted;
    private bool journeyRequested;

    private void Awake()
    {
        BuildInterface();
    }

    public void BeginEnding()
    {
        if (endingStarted)
        {
            return;
        }

        BuildInterface();
        if (endingLayer == null)
        {
            Debug.LogError("[YouthMemory] Unable to build the chapter ending interface.");
            return;
        }

        endingStarted = true;
        Transform gameEntry = transform.Find("YouthMemoryCanvas/UIRoot/YouthPlatformerEntry");
        if (gameEntry != null)
        {
            gameEntry.gameObject.SetActive(false);
        }

        endingLayer.SetActive(true);
        StartCoroutine(PlayEnding());
    }

    private IEnumerator PlayEnding()
    {
        yield return ShowLine("小禾", "哈哈哈哈！原来上课也可以这么好玩！");
        yield return ShowLine("志愿者", "学习不一定只有一种方式。");
        yield return ShowLine("志愿者", "只要找到属于自己的方式，每个人都能成为课堂的主角。");

        dialoguePanel.SetActive(false);

        GameProgress progress = GameProgress.Ensure();
        if (progress.TryGrantYouthMemory())
        {
            yield return ShowMemoryReward();
        }

        yield return ShowStudyBook(progress);
        ShowCompletion();
        endingCompleted = true;
    }

    public void ContinueJourney()
    {
        if (!endingCompleted || journeyRequested)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError("[YouthMemory] VillageMemory is not included in Build Settings.");
            return;
        }

        journeyRequested = true;
        continueJourneyButton.interactable = false;
        SceneManager.LoadScene(nextSceneName);
    }

    private void BuildInterface()
    {
        if (endingLayer != null)
        {
            return;
        }

        Transform canvas = transform.Find("YouthMemoryCanvas");
        if (canvas == null)
        {
            return;
        }

        interfaceParent = canvas.Find("UIRoot");
        if (interfaceParent == null)
        {
            interfaceParent = canvas;
        }

        endingLayer = CreateContainer("YouthChapterEndingLayer", interfaceParent);
        dimPanel = CreatePanel("EndingDim", endingLayer.transform, new Color(0f, 0f, 0f, 0.62f), Vector2.zero, Vector2.one);
        BuildDialoguePanel(endingLayer.transform);
        BuildRewardPanel(endingLayer.transform);
        BuildStudyBook(endingLayer.transform);
        BuildCompletionPanel(endingLayer.transform);

        dimPanel.SetActive(false);
        rewardPanel.SetActive(false);
        bookPanel.SetActive(false);
        completionPanel.SetActive(false);
        dialoguePanel.SetActive(false);
        endingLayer.SetActive(false);
    }

    private void BuildDialoguePanel(Transform parent)
    {
        dialoguePanel = CreatePanel("YouthEndingDialoguePanel", parent, new Color(0.035f, 0.06f, 0.12f, 0.94f),
            new Vector2(0.11f, 0.035f), new Vector2(0.89f, 0.255f));

        speakerText = CreateText("SpeakerText", dialoguePanel.transform, string.Empty, 30,
            new Color(1f, 0.82f, 0.45f, 1f), TextAlignmentOptions.TopLeft,
            new Vector2(0.05f, 0.62f), new Vector2(0.52f, 0.92f));
        dialogueText = CreateText("DialogueText", dialoguePanel.transform, string.Empty, 29, Color.white,
            TextAlignmentOptions.TopLeft, new Vector2(0.05f, 0.13f), new Vector2(0.78f, 0.66f));

        advanceButton = CreateButton("AdvanceButton", dialoguePanel.transform, "继续",
            new Vector2(0.84f, 0.17f), new Vector2(0.96f, 0.60f));
        advanceButton.onClick.AddListener(() => advanceRequested = true);
    }

    private void BuildRewardPanel(Transform parent)
    {
        rewardPanel = CreatePanel("MemoryRewardUI", parent, new Color(0.04f, 0.10f, 0.19f, 0.98f),
            new Vector2(0.28f, 0.30f), new Vector2(0.72f, 0.70f));

        CreateText("RewardTitle", rewardPanel.transform, "记忆获得", 34, new Color(1f, 0.84f, 0.33f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.68f), new Vector2(0.92f, 0.87f));
        CreateText("RewardName", rewardPanel.transform, "青春记忆", 52, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.40f), new Vector2(0.92f, 0.65f));
        CreateText("RewardSubtitle", rewardPanel.transform, "让学习变得有趣", 26,
            new Color(0.72f, 0.87f, 1f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.19f), new Vector2(0.92f, 0.38f));
    }

    private void BuildStudyBook(Transform parent)
    {
        bookPanel = CreatePanel("StudyBookPanel", parent, new Color(0.95f, 0.91f, 0.79f, 1f),
            new Vector2(0.23f, 0.23f), new Vector2(0.77f, 0.77f));

        CreateText("BookTitle", bookPanel.transform, "研学手册", 38, new Color(0.18f, 0.13f, 0.08f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.74f), new Vector2(0.92f, 0.90f));
        bookText = CreateText("BookText", bookPanel.transform, string.Empty, 28, new Color(0.18f, 0.13f, 0.08f, 1f),
            TextAlignmentOptions.Left, new Vector2(0.16f, 0.18f), new Vector2(0.84f, 0.70f));
    }

    private void BuildCompletionPanel(Transform parent)
    {
        completionPanel = CreatePanel("ChapterCompletePanel", parent, new Color(0.035f, 0.06f, 0.12f, 0.97f),
            new Vector2(0.27f, 0.28f), new Vector2(0.73f, 0.72f));

        CreateText("CompleteTitle", completionPanel.transform, "第三章完成", 46,
            new Color(0.42f, 1f, 0.63f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.87f));
        CreateText("CompleteSubtitle", completionPanel.transform, "青春记忆 · 让学习变得有趣", 28, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.43f), new Vector2(0.92f, 0.61f));

        continueJourneyButton = CreateButton("ContinueJourneyButton", completionPanel.transform, "继续旅程",
            new Vector2(0.30f, 0.16f), new Vector2(0.70f, 0.33f));
        continueJourneyButton.onClick.AddListener(ContinueJourney);
    }

    private IEnumerator ShowLine(string speaker, string line)
    {
        speakerText.text = speaker;
        dialogueText.text = line;
        advanceRequested = false;
        dialoguePanel.SetActive(true);

        while (!advanceRequested)
        {
            yield return null;
        }
    }

    private IEnumerator ShowMemoryReward()
    {
        CanvasGroup dimGroup = GetCanvasGroup(dimPanel);
        CanvasGroup rewardGroup = GetCanvasGroup(rewardPanel);

        dimPanel.SetActive(true);
        dimGroup.alpha = 0f;
        yield return Fade(dimGroup, 0f, 1f, fadeDuration);

        rewardPanel.SetActive(true);
        rewardGroup.alpha = 0f;
        rewardGroup.transform.localScale = Vector3.one * 0.86f;
        yield return FadeAndScale(rewardGroup, 0f, 1f, 0.86f, 1f, fadeDuration);
        yield return new WaitForSecondsRealtime(1.0f);
        yield return FadeAndScale(rewardGroup, 1f, 0f, 1f, 0.90f, fadeDuration);
        rewardPanel.SetActive(false);

        yield return Fade(dimGroup, 1f, 0f, fadeDuration);
        dimPanel.SetActive(false);
    }

    private IEnumerator ShowStudyBook(GameProgress progress)
    {
        bookText.text =
            "记忆收集：" + progress.MemoryCount + " / 4\n\n" +
            "红色记忆  " + (progress.HasRedMemory ? "✅" : "🔒") + "\n" +
            "乡土记忆  " + (progress.HasHometownMemory ? "✅" : "🔒") + "\n" +
            "青春记忆  " + (progress.HasYouthMemory ? "✅" : "🔒") + "\n" +
            "乡村记忆  🔒";

        CanvasGroup bookGroup = GetCanvasGroup(bookPanel);
        bookPanel.SetActive(true);
        bookGroup.alpha = 0f;
        yield return Fade(bookGroup, 0f, 1f, fadeDuration);
        yield return new WaitForSecondsRealtime(1.5f);
        yield return Fade(bookGroup, 1f, 0f, fadeDuration);
        bookPanel.SetActive(false);
    }

    private void ShowCompletion()
    {
        CanvasGroup group = GetCanvasGroup(completionPanel);
        completionPanel.SetActive(true);
        group.alpha = 0f;
        group.blocksRaycasts = true;
        StartCoroutine(Fade(group, 0f, 1f, fadeDuration));
    }

    private static GameObject CreateContainer(string name, Transform parent)
    {
        GameObject container = new GameObject(name, typeof(RectTransform));
        container.transform.SetParent(parent, false);
        Stretch(container.GetComponent<RectTransform>());
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
        GameObject buttonObject = CreatePanel(name, parent, new Color(0.86f, 0.53f, 0.18f, 1f), min, max);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        CreateText("Label", buttonObject.transform, label, 26, Color.white, TextAlignmentOptions.Center, Vector2.zero, Vector2.one);
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

    private static CanvasGroup GetCanvasGroup(GameObject target)
    {
        return target.GetComponent<CanvasGroup>();
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

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
