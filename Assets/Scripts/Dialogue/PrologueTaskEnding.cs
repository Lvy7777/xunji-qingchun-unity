using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class PrologueTaskEnding : MonoBehaviour
{
    private const string FinalLine = "那我们就一起把它填满。";
    private const string TargetScene = "RedMemory";

    private PrologueStoryController storyController;
    private GameObject dialoguePanel;
    private TMP_Text nameText;
    private TMP_Text dialogueText;
    private Button nextButton;

    private GameObject taskPopup;
    private RectTransform popupRect;
    private CanvasGroup popupGroup;
    private Button startButton;
    private CanvasGroup fadePanel;

    private Image dimImage;
    private CanvasGroup dimGroup;

    private CanvasGroup taskTitleGroup;
    private CanvasGroup taskSubtitleGroup;
    private CanvasGroup startButtonGroup;
    private CanvasGroup[] cardGroups;
    private RectTransform[] cardRects;

    private TMP_Text transitionText;
    private CanvasGroup transitionTextGroup;

    private bool started;
    private bool transitioning;

    private void Awake()
    {
        Bind();
        ConfigurePopup();
    }

    private IEnumerator Start()
    {
        if (!enabled)
        {
            yield break;
        }

        while (!StoryHasReachedEnding())
        {
            yield return null;
        }

        if (started)
        {
            yield break;
        }

        started = true;
        yield return new WaitForSecondsRealtime(0.3f);

        CanvasGroup dialogueGroup = GetOrAddCanvasGroup(dialoguePanel.transform);
        yield return Fade(dialogueGroup, 1f, 0f, 0.36f);
        dialoguePanel.SetActive(false);

        dimImage.gameObject.SetActive(true);
        dimImage.color = new Color(0f, 0f, 0f, 0.42f);
        yield return Fade(dimGroup, 0f, 1f, 0.28f);

        PreparePopupAnimation();
        taskPopup.SetActive(true);
        PrologueAudioBindings.PlayQuestPopup();
        yield return FadeAndScale(popupGroup, popupRect, 0f, 1f, 0.78f, 1f, 0.3f);

        yield return Fade(taskTitleGroup, 0f, 1f, 0.18f);
        yield return Fade(taskSubtitleGroup, 0f, 1f, 0.2f);

        for (int i = 0; i < cardGroups.Length; i++)
        {
            yield return PopCard(i);
            yield return new WaitForSecondsRealtime(0.08f);
        }

        yield return HighlightFirstMemory();
        yield return FadeAndScale(
            startButtonGroup,
            startButton.transform as RectTransform,
            0f,
            1f,
            0.94f,
            1f,
            0.2f);

        startButton.interactable = true;
        popupGroup.blocksRaycasts = true;
        popupGroup.interactable = true;
    }

    private void Bind()
    {
        GameObject canvasObject = GameObject.Find("Canvas");
        Transform canvas = canvasObject != null ? canvasObject.transform : null;
        if (canvas == null)
        {
            Debug.LogError("[Prologue] PrologueTaskEnding could not find Canvas.");
            enabled = false;
            return;
        }

        Transform dialogueTransform = canvas.Find("DialogueLayer/DialoguePanel");
        Transform taskTransform = canvas.Find("TaskLayer/TaskPopup");
        Transform fadeTransform = canvas.Find("FadePanel");

        if (dialogueTransform == null || taskTransform == null || fadeTransform == null)
        {
            Debug.LogError("[Prologue] PrologueTaskEnding is missing required scene objects.");
            enabled = false;
            return;
        }

        storyController = GetComponent<PrologueStoryController>();
        dialoguePanel = dialogueTransform.gameObject;
        nameText = dialogueTransform.Find("NameText").GetComponent<TMP_Text>();
        dialogueText = dialogueTransform.Find("DialogueText").GetComponent<TMP_Text>();
        nextButton = dialogueTransform.Find("NextButton").GetComponent<Button>();

        taskPopup = taskTransform.gameObject;
        popupRect = taskTransform as RectTransform;
        popupGroup = GetOrAddCanvasGroup(taskTransform);
        startButton = taskTransform.Find("StartButton").GetComponent<Button>();
        fadePanel = GetOrAddCanvasGroup(fadeTransform);

        dimImage = CreateOrFindDimPanel(canvas);
        dimGroup = GetOrAddCanvasGroup(dimImage.transform);

        taskTitleGroup = GetOrAddCanvasGroup(taskTransform.Find("TaskTitle"));
        taskSubtitleGroup = GetOrAddCanvasGroup(taskTransform.Find("TaskSubtitle"));
        startButtonGroup = GetOrAddCanvasGroup(startButton.transform);

        string[] cardNames =
        {
            "RedMemoryTask",
            "HometownMemoryTask",
            "YouthMemoryTask",
            "VillageMemoryTask"
        };

        cardGroups = new CanvasGroup[cardNames.Length];
        cardRects = new RectTransform[cardNames.Length];

        for (int i = 0; i < cardNames.Length; i++)
        {
            Transform card = taskTransform.Find(cardNames[i]);
            cardGroups[i] = GetOrAddCanvasGroup(card);
            cardRects[i] = card as RectTransform;
        }

        transitionText = CreateOrFindTransitionText(fadeTransform);
        transitionTextGroup = GetOrAddCanvasGroup(transitionText.transform);
    }

    private void ConfigurePopup()
    {
        if (!enabled)
        {
            return;
        }

        SetText("TaskTitle", "任务开启");
        SetText("TaskSubtitle", "寻找属于家乡的四段记忆");
        SetText("RedMemoryTask", "红色记忆  ·  未获得");
        SetText("HometownMemoryTask", "乡土记忆  ·  未获得");
        SetText("YouthMemoryTask", "青春记忆  ·  未获得");
        SetText("VillageMemoryTask", "乡村记忆  ·  未获得");

        TMP_Text buttonText = startButton.GetComponentInChildren<TMP_Text>(true);
        if (buttonText != null)
        {
            buttonText.text = "开始寻找";
        }

        taskPopup.SetActive(false);
        startButton.interactable = false;
        startButton.onClick.RemoveListener(BeginTransition);
        startButton.onClick.AddListener(BeginTransition);

        transitionText.text = "第一站：红色记忆";
        transitionText.gameObject.SetActive(false);

        dimImage.gameObject.SetActive(false);
        dimGroup.alpha = 0f;
    }

    private void PreparePopupAnimation()
    {
        popupGroup.alpha = 0f;
        popupGroup.blocksRaycasts = false;
        popupGroup.interactable = false;
        popupRect.localScale = Vector3.one * 0.78f;

        taskTitleGroup.alpha = 0f;
        taskSubtitleGroup.alpha = 0f;
        startButtonGroup.alpha = 0f;

        for (int i = 0; i < cardGroups.Length; i++)
        {
            cardGroups[i].alpha = 0f;
            cardRects[i].localScale = Vector3.one * 0.84f;
        }
    }

    private bool StoryHasReachedEnding()
    {
        return storyController != null && storyController.IsCompleted;
    }

    private IEnumerator PopCard(int index)
    {
        CanvasGroup group = cardGroups[index];
        RectTransform rect = cardRects[index];
        float elapsed = 0f;
        const float duration = 0.22f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - normalized, 3f);
            float overshoot = Mathf.Sin(normalized * Mathf.PI) * 0.06f;
            group.alpha = eased;
            rect.localScale = Vector3.one * (Mathf.LerpUnclamped(0.84f, 1f, eased) + overshoot);
            yield return null;
        }

        group.alpha = 1f;
        rect.localScale = Vector3.one;
    }

    private IEnumerator HighlightFirstMemory()
    {
        TMP_Text text = cardRects[0].GetComponent<TMP_Text>();
        if (text != null)
        {
            text.color = new Color(1f, 0.66f, 0.35f, 1f);
        }

        Vector3 start = Vector3.one;
        Vector3 peak = Vector3.one * 1.08f;
        float elapsed = 0f;

        while (elapsed < 0.18f)
        {
            elapsed += Time.unscaledDeltaTime;
            cardRects[0].localScale = Vector3.Lerp(start, peak, elapsed / 0.18f);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 0.14f)
        {
            elapsed += Time.unscaledDeltaTime;
            cardRects[0].localScale = Vector3.Lerp(peak, Vector3.one * 1.04f, elapsed / 0.14f);
            yield return null;
        }

        cardRects[0].localScale = Vector3.one * 1.04f;
    }

    private void BeginTransition()
    {
        if (transitioning || !startButton.interactable)
        {
            return;
        }

        transitioning = true;
        startButton.interactable = false;
        popupGroup.blocksRaycasts = false;
        popupGroup.interactable = false;
        StartCoroutine(TransitionToRedMemory());
    }

    private IEnumerator TransitionToRedMemory()
    {
        yield return Fade(popupGroup, 1f, 0f, 0.26f);
        taskPopup.SetActive(false);

        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = true;
        fadePanel.interactable = true;

        Image fadeImage = fadePanel.GetComponent<Image>();
        if (fadeImage != null)
        {
            fadeImage.color = Color.black;
        }

        yield return Fade(fadePanel, 0f, 1f, 0.58f);

        transitionText.gameObject.SetActive(true);
        yield return Fade(transitionTextGroup, 0f, 1f, 0.22f);
        yield return new WaitForSecondsRealtime(0.72f);
        yield return Fade(transitionTextGroup, 1f, 0f, 0.2f);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(TargetScene);
        if (loadOperation == null)
        {
            SceneManager.LoadScene(TargetScene);
        }
    }

    private Image CreateOrFindDimPanel(Transform canvas)
    {
        Transform existing = canvas.Find("TaskDimPanel");
        if (existing != null)
        {
            return existing.GetComponent<Image>();
        }

        GameObject panel = new GameObject(
            "TaskDimPanel",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup));
        panel.transform.SetParent(canvas, false);
        panel.transform.SetSiblingIndex(1);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.raycastTarget = false;
        image.color = new Color(0f, 0f, 0f, 0.42f);
        panel.SetActive(false);
        return image;
    }

    private TMP_Text CreateOrFindTransitionText(Transform fadeTransform)
    {
        Transform existing = fadeTransform.Find("TransitionTitle");
        if (existing != null)
        {
            return existing.GetComponent<TMP_Text>();
        }

        GameObject titleObject = new GameObject(
            "TransitionTitle",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI),
            typeof(CanvasGroup));
        titleObject.transform.SetParent(fadeTransform, false);

        RectTransform rect = titleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1100f, 120f);
        rect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text = titleObject.GetComponent<TextMeshProUGUI>();
        TMP_Text source = taskPopup.transform.Find("TaskTitle").GetComponent<TMP_Text>();
        text.font = source.font;
        text.fontSize = 52f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.92f, 0.78f, 1f);
        text.raycastTarget = false;
        return text;
    }

    private void SetText(string childName, string value)
    {
        Transform child = taskPopup.transform.Find(childName);
        if (child == null)
        {
            return;
        }

        TMP_Text text = child.GetComponent<TMP_Text>();
        if (text != null)
        {
            text.text = value;
        }
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

    private static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
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
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(BeginTransition);
        }
    }
}
