using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class RealMediaSequence : MonoBehaviour
{
    private static readonly string[] PhotoObjectNames =
    {
        "Photo_01",
        "Photo_02",
        "Photo_03",
        "Photo_04",
        "Photo_05",
        "Photo_06"
    };

    private static readonly string[] PhotoDescriptions =
    {
        "支教课堂 · Placeholder Image",
        "红色研学活动 · Placeholder Image",
        "客家文化活动 · Placeholder Image",
        "课程成果 · Placeholder Image",
        "孩子参与活动 · Placeholder Image",
        "团队实践合照 · Placeholder Image"
    };

    private static readonly string[] Captions =
    {
        "我们曾经以为，三下乡是一场“我们去教他们”的实践。",
        "后来才发现，我们带走的，是一段段故事。",
        "而孩子们留下的，是我们对这片土地新的理解。",
        "青春不是旁观者。",
        "我们来到这里，记录这里，也成为这里故事的一部分。",
        "从一次三下乡，到一场青春与乡土的双向奔赴。"
    };

    private static readonly Color[] PlaceholderColors =
    {
        new Color(0.38f, 0.53f, 0.63f, 1f),
        new Color(0.55f, 0.25f, 0.22f, 1f),
        new Color(0.61f, 0.43f, 0.24f, 1f),
        new Color(0.27f, 0.49f, 0.36f, 1f),
        new Color(0.48f, 0.38f, 0.60f, 1f),
        new Color(0.22f, 0.34f, 0.49f, 1f)
    };

    [SerializeField] private float fadeDuration = 0.55f;
    [SerializeField] private float holdDuration = 1.4f;
    [SerializeField] private string restartSceneName = "Prologue";
    [SerializeField] private string titleSceneName = "TitleScene";

    private GameObject sequenceLayer;
    private GameObject photoStage;
    private GameObject finalScreen;
    private TextMeshProUGUI subtitleText;
    private Button restartButton;
    private Button returnTitleButton;
    private readonly List<GameObject> photoPanels = new List<GameObject>();
    private readonly List<string> playedPhotoNames = new List<string>();
    private readonly List<string> shownCaptions = new List<string>();

    private bool initialized;
    private bool playbackStarted;
    private bool sequenceComplete;
    private bool transitionRequested;
    private int currentPhotoIndex = -1;

    public bool PlaybackStarted => playbackStarted;
    public bool SequenceComplete => sequenceComplete;
    public bool FinalScreenVisible => finalScreen != null && finalScreen.activeSelf;
    public int PlaceholderCount => photoPanels.Count;
    public int CurrentPhotoIndex => currentPhotoIndex;
    public string CurrentSubtitle => subtitleText != null ? subtitleText.text : string.Empty;
    public string ResolvedTitleDestination => Application.CanStreamedLevelBeLoaded(titleSceneName)
        ? titleSceneName
        : restartSceneName;
    public string[] ExpectedPhotoOrder => (string[])PhotoObjectNames.Clone();
    public string[] ExpectedSubtitleOrder => (string[])Captions.Clone();
    public IList<string> PlayedPhotoNames => playedPhotoNames.AsReadOnly();
    public IList<string> ShownCaptions => shownCaptions.AsReadOnly();

    public void Initialize(Transform canvasParent)
    {
        if (initialized)
        {
            return;
        }

        BuildInterface(canvasParent);
        initialized = true;
        ResetSequence();
    }

    public IEnumerator PlaySequence()
    {
        if (!initialized || playbackStarted)
        {
            yield break;
        }

        playbackStarted = true;
        sequenceLayer.SetActive(true);
        photoStage.SetActive(true);
        finalScreen.SetActive(false);
        playedPhotoNames.Clear();
        shownCaptions.Clear();

        for (int i = 0; i < photoPanels.Count; i++)
        {
            currentPhotoIndex = i;
            GameObject photo = photoPanels[i];
            CanvasGroup photoGroup = photo.GetComponent<CanvasGroup>();

            subtitleText.text = Captions[i];
            playedPhotoNames.Add(photo.name);
            shownCaptions.Add(Captions[i]);

            photo.SetActive(true);
            photoGroup.alpha = 0f;
            yield return Fade(photoGroup, 0f, 1f, fadeDuration);
            yield return new WaitForSecondsRealtime(holdDuration);
            yield return Fade(photoGroup, 1f, 0f, fadeDuration);
            photo.SetActive(false);
        }

        ShowFinalScreen();
        sequenceComplete = true;
    }

    public void CompleteImmediatelyForTesting()
    {
        if (!initialized)
        {
            return;
        }

        playbackStarted = true;
        sequenceLayer.SetActive(true);
        photoStage.SetActive(false);
        playedPhotoNames.Clear();
        shownCaptions.Clear();

        for (int i = 0; i < PhotoObjectNames.Length; i++)
        {
            playedPhotoNames.Add(PhotoObjectNames[i]);
            shownCaptions.Add(Captions[i]);
        }

        currentPhotoIndex = PhotoObjectNames.Length - 1;
        subtitleText.text = Captions[Captions.Length - 1];
        ShowFinalScreenImmediately();
        sequenceComplete = true;
    }

    public void ResetSequence()
    {
        playbackStarted = false;
        sequenceComplete = false;
        transitionRequested = false;
        currentPhotoIndex = -1;
        playedPhotoNames.Clear();
        shownCaptions.Clear();

        if (!initialized)
        {
            return;
        }

        subtitleText.text = string.Empty;
        for (int i = 0; i < photoPanels.Count; i++)
        {
            photoPanels[i].SetActive(false);
        }

        restartButton.interactable = true;
        returnTitleButton.interactable = true;
        finalScreen.SetActive(false);
        photoStage.SetActive(true);
        sequenceLayer.SetActive(false);
    }

    public void RestartJourney()
    {
        if (transitionRequested)
        {
            return;
        }

        ClearProgressForRestartTesting();

        if (!Application.CanStreamedLevelBeLoaded(restartSceneName))
        {
            Debug.LogWarning("[Ending] Prologue is not available in Build Settings.");
            return;
        }

        transitionRequested = true;
        DisableFinalButtons();
        SceneManager.LoadScene(restartSceneName);
    }

    public void ReturnToTitle()
    {
        if (transitionRequested)
        {
            return;
        }

        string destination = Application.CanStreamedLevelBeLoaded(titleSceneName)
            ? titleSceneName
            : restartSceneName;

        if (!Application.CanStreamedLevelBeLoaded(destination))
        {
            Debug.LogWarning("[Ending] Neither TitleScene nor Prologue is available in Build Settings.");
            return;
        }

        if (destination == restartSceneName)
        {
            Debug.Log("[Ending] TitleScene is not available; returning to Prologue instead.");
        }

        transitionRequested = true;
        DisableFinalButtons();
        SceneManager.LoadScene(destination);
    }

    public void ClearProgressForRestartTesting()
    {
        GameProgress.Ensure().ResetAllMemories();
    }

    private void ShowFinalScreen()
    {
        photoStage.SetActive(false);
        finalScreen.SetActive(true);
        CanvasGroup finalGroup = finalScreen.GetComponent<CanvasGroup>();
        finalGroup.alpha = 0f;
        finalGroup.blocksRaycasts = true;
        StartCoroutine(Fade(finalGroup, 0f, 1f, fadeDuration * 1.8f));
    }

    private void ShowFinalScreenImmediately()
    {
        photoStage.SetActive(false);
        finalScreen.SetActive(true);
        CanvasGroup finalGroup = finalScreen.GetComponent<CanvasGroup>();
        finalGroup.alpha = 1f;
        finalGroup.blocksRaycasts = true;
    }

    private void DisableFinalButtons()
    {
        restartButton.interactable = false;
        returnTitleButton.interactable = false;
    }

    private void BuildInterface(Transform parent)
    {
        sequenceLayer = CreateContainer("RealMediaSequence", parent);
        CreatePanel("SequenceBackground", sequenceLayer.transform, Color.black, Vector2.zero, Vector2.one);

        photoStage = CreateContainer("PhotoStage", sequenceLayer.transform);
        CreatePanel("PhotoBackdrop", photoStage.transform, new Color(0.015f, 0.025f, 0.04f, 1f),
            Vector2.zero, Vector2.one);

        for (int i = 0; i < PhotoObjectNames.Length; i++)
        {
            GameObject photo = CreatePanel(PhotoObjectNames[i], photoStage.transform, PlaceholderColors[i],
                new Vector2(0.12f, 0.20f), new Vector2(0.88f, 0.88f));
            photo.GetComponent<Image>().raycastTarget = false;

            CreateText("PlaceholderLabel", photo.transform, PhotoDescriptions[i], 36, Color.white,
                TextAlignmentOptions.Center, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.62f));
            CreateText("PlaceholderMark", photo.transform, "PLACEHOLDER", 22,
                new Color(1f, 1f, 1f, 0.68f), TextAlignmentOptions.Center,
                new Vector2(0.08f, 0.30f), new Vector2(0.92f, 0.40f));
            photoPanels.Add(photo);
        }

        GameObject subtitleBar = CreatePanel("NarrationBar", photoStage.transform,
            new Color(0f, 0f, 0f, 0.78f), new Vector2(0.08f, 0.05f), new Vector2(0.92f, 0.19f));
        subtitleText = CreateText("NarrationText", subtitleBar.transform, string.Empty, 30, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f));

        BuildFinalScreen(sequenceLayer.transform);
    }

    private void BuildFinalScreen(Transform parent)
    {
        finalScreen = CreatePanel("FinalScreen", parent, Color.black, Vector2.zero, Vector2.one);

        CreateText("FinalTitle", finalScreen.transform, "《寻迹·青春》", 68,
            new Color(0.96f, 0.87f, 0.66f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.15f, 0.54f), new Vector2(0.85f, 0.72f));

        restartButton = CreateButton("RestartButton", finalScreen.transform, "重新开始",
            new Vector2(0.29f, 0.28f), new Vector2(0.49f, 0.37f));
        returnTitleButton = CreateButton("ReturnTitleButton", finalScreen.transform, "返回标题",
            new Vector2(0.51f, 0.28f), new Vector2(0.71f, 0.37f));

        restartButton.onClick.AddListener(RestartJourney);
        returnTitleButton.onClick.AddListener(ReturnToTitle);
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
        GameObject buttonObject = CreatePanel(name, parent, new Color(0.32f, 0.28f, 0.22f, 1f), min, max);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        CreateText("Label", buttonObject.transform, label, 27, Color.white,
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
        group.blocksRaycasts = to > 0.01f;
    }
}
