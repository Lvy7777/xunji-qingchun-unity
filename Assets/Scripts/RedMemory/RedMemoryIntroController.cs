using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed class RedMemoryIntroController : MonoBehaviour
{
    private Canvas canvas;
    private Image background;
    private Image fade;
    private TMP_Text titleText;
    private GameObject titlePanel;
    private GameObject xiaoHe;
    private GameObject volunteer;
    private TMP_Text speakerText;
    private TMP_Text dialogueText;
    private Button nextButton;
    private GameObject choicePanel;
    private Button choiceOne;
    private Button choiceTwo;
    private GameObject missionPanel;
    private Button missionStartButton;
    private bool reveal;
    private bool advance;
    private int selectedChoice = -1;
    private int lastTheme = -1;

    private readonly Color[] sceneColors =
    {
        new Color(0.34f, 0.12f, 0.10f, 1f),
        new Color(0.18f, 0.20f, 0.27f, 1f),
        new Color(0.32f, 0.22f, 0.14f, 1f),
        new Color(0.23f, 0.11f, 0.16f, 1f)
    };

    private void Awake()
    {
        BuildInterface();
    }

    private IEnumerator Start()
    {
        fade.gameObject.SetActive(true);
        fade.color = Color.black;
        yield return Fade(fade, 1f, 0f, 0.8f);
        fade.gameObject.SetActive(false);

        yield return ShowTitle("第一章", 0.8f);
        yield return ShowTitle("红色记忆", 0.8f);
        yield return ShowTitle("那段不能被忘记的故事", 1.5f);

        yield return ShowLine("小禾", "老师，他们是谁？");
        yield return ShowLine("志愿者", "他们，是曾经生活在这片土地上的人。");
        yield return ShowLine("小禾", "他们做过什么？");

        yield return ShowChoice();
        if (selectedChoice == 0)
        {
            yield return ShowLine("志愿者", "这个问题……我们一起去找答案吧。");
        }
        else
        {
            yield return ShowLine("志愿者", "也许答案，就藏在我们接下来看到的东西里。");
            yield return ShowLine("志愿者", "我们一起去找答案吧。");
        }

        SetSpeaker(string.Empty);
        missionPanel.SetActive(true);
    }

    private void BuildInterface()
    {
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        GameObject canvasObject = new GameObject("RedMemoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        background = CreateImage("RedMemoryBackground", canvas.transform, sceneColors[0], Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CreateText("PlaceLabel", background.transform, "红色文化实践点  ·  占位场景", 30, new Color(1f, 0.88f, 0.68f, 0.88f), TextAlignmentOptions.TopLeft,
            new Vector2(0.06f, 0.86f), new Vector2(0.55f, 0.95f), Vector2.zero, Vector2.zero);

        titlePanel = CreatePanel("ChapterTitlePanel", canvas.transform, new Color(0f, 0f, 0f, 0.5f), new Vector2(0.27f, 0.31f), new Vector2(0.73f, 0.69f));
        titleText = CreateText("ChapterTitleText", titlePanel.transform, string.Empty, 54, Color.white, TextAlignmentOptions.Center,
            new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
        titlePanel.SetActive(false);

        xiaoHe = CreateActor("XiaoHe", "小禾", new Color(0.95f, 0.63f, 0.52f, 1f), new Vector2(0.07f, 0.19f), new Vector2(0.34f, 0.76f));
        volunteer = CreateActor("Volunteer", "志愿者", new Color(0.42f, 0.67f, 0.84f, 1f), new Vector2(0.66f, 0.19f), new Vector2(0.93f, 0.76f));
        xiaoHe.SetActive(false);
        volunteer.SetActive(false);

        GameObject dialoguePanel = CreatePanel("DialoguePanel", canvas.transform, new Color(0.04f, 0.04f, 0.07f, 0.88f), new Vector2(0.12f, 0.04f), new Vector2(0.88f, 0.25f));
        speakerText = CreateText("SpeakerText", dialoguePanel.transform, string.Empty, 32, new Color(1f, 0.78f, 0.47f, 1f), TextAlignmentOptions.TopLeft,
            new Vector2(0.05f, 0.67f), new Vector2(0.45f, 0.94f), Vector2.zero, Vector2.zero);
        dialogueText = CreateText("DialogueText", dialoguePanel.transform, string.Empty, 31, Color.white, TextAlignmentOptions.TopLeft,
            new Vector2(0.05f, 0.11f), new Vector2(0.82f, 0.68f), Vector2.zero, Vector2.zero);
        nextButton = CreateButton("NextButton", dialoguePanel.transform, "继续", new Vector2(0.84f, 0.18f), new Vector2(0.96f, 0.66f), OnAdvance);
        nextButton.gameObject.SetActive(false);

        choicePanel = CreatePanel("ChoicePanel", canvas.transform, new Color(0.04f, 0.04f, 0.07f, 0.93f), new Vector2(0.27f, 0.31f), new Vector2(0.73f, 0.69f));
        CreateText("ChoicePrompt", choicePanel.transform, "小禾的疑问，值得我们亲自去寻找答案。", 28, Color.white, TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.67f), new Vector2(0.92f, 0.9f), Vector2.zero, Vector2.zero);
        choiceOne = CreateButton("ChoiceOne", choicePanel.transform, "① 我们一起去找答案吧", new Vector2(0.12f, 0.39f), new Vector2(0.88f, 0.57f), OnChoiceOne);
        choiceTwo = CreateButton("ChoiceTwo", choicePanel.transform, "② 先看看这里留下了什么", new Vector2(0.12f, 0.17f), new Vector2(0.88f, 0.35f), OnChoiceTwo);
        choicePanel.SetActive(false);

        missionPanel = CreatePanel("MissionPanel", canvas.transform, new Color(0.08f, 0.03f, 0.03f, 0.94f), new Vector2(0.27f, 0.22f), new Vector2(0.73f, 0.78f));
        CreateText("MissionTitle", missionPanel.transform, "红色记忆 · 寻迹", 40, new Color(1f, 0.75f, 0.42f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.08f, 0.73f), new Vector2(0.92f, 0.91f), Vector2.zero, Vector2.zero);
        CreateText("MissionContent", missionPanel.transform, "寻找散落的红色记忆碎片\n找到隐藏的线索钥匙\n抵达终点\n\n记忆碎片：0 / 5\n线索钥匙：未获得", 28, Color.white, TextAlignmentOptions.Center,
            new Vector2(0.10f, 0.29f), new Vector2(0.90f, 0.70f), Vector2.zero, Vector2.zero);
        missionStartButton = CreateButton("StartInvestigationButton", missionPanel.transform, "开始寻迹", new Vector2(0.30f, 0.08f), new Vector2(0.70f, 0.22f), StartInvestigation);
        missionPanel.SetActive(false);

        fade = CreateImage("FadePanel", canvas.transform, Color.black, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        fade.transform.SetAsLastSibling();
    }

    private IEnumerator ShowTitle(string text, float displayTime)
    {
        titleText.text = text;
        titlePanel.SetActive(true);
        CanvasGroup group = titlePanel.GetComponent<CanvasGroup>();
        yield return Fade(group, 0f, 1f, 0.25f);
        yield return new WaitForSecondsRealtime(displayTime);
        yield return Fade(group, 1f, 0f, 0.25f);
        titlePanel.SetActive(false);
    }

    private IEnumerator ShowLine(string speaker, string line)
    {
        ChooseSceneTheme();
        SetSpeaker(speaker);
        speakerText.text = speaker;
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        nextButton.gameObject.SetActive(false);
        reveal = false;
        advance = false;

        for (int i = 0; i < line.Length; i++)
        {
            if (reveal) break;
            dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSecondsRealtime(0.035f);
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;
        nextButton.gameObject.SetActive(true);
        while (!advance) yield return null;
        nextButton.gameObject.SetActive(false);
    }

    private IEnumerator ShowChoice()
    {
        selectedChoice = -1;
        choicePanel.SetActive(true);
        while (selectedChoice < 0) yield return null;
        choicePanel.SetActive(false);
    }

    private void SetSpeaker(string speaker)
    {
        bool hideAllActors = string.IsNullOrEmpty(speaker);
        SetActorVisual(xiaoHe, speaker == "小禾", hideAllActors);
        SetActorVisual(volunteer, speaker == "志愿者", hideAllActors);
    }

    private static void SetActorVisual(GameObject actor, bool isSpeaking, bool hideActor)
    {
        if (hideActor)
        {
            actor.SetActive(false);
            return;
        }

        actor.SetActive(true);
        CanvasGroup group = actor.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = isSpeaking ? 1f : 0.5f;
        }

        actor.transform.localScale = isSpeaking ? Vector3.one * 1.03f : Vector3.one;
    }

    private void ChooseSceneTheme()
    {
        int next = Random.Range(0, sceneColors.Length - 1);
        if (next >= lastTheme) next++;
        lastTheme = next;
        background.color = sceneColors[next];
    }

    private void OnAdvance()
    {
        if (dialogueText.maxVisibleCharacters < dialogueText.text.Length) reveal = true;
        else advance = true;
    }

    private void OnChoiceOne() { if (selectedChoice < 0) selectedChoice = 0; }
    private void OnChoiceTwo() { if (selectedChoice < 0) selectedChoice = 1; }

    private void StartInvestigation()
    {
        if (!missionStartButton.interactable) return;
        missionStartButton.interactable = false;
        missionPanel.SetActive(false);
        xiaoHe.SetActive(false);
        volunteer.SetActive(false);
        SceneManager.LoadScene("HometownMemory");
    }

    private static GameObject CreateActor(string name, string label, Color color, Vector2 min, Vector2 max)
    {
        GameObject actor = CreatePanel(name, FindObjectOfType<Canvas>().transform, new Color(0f, 0f, 0f, 0f), min, max);
        Image portrait = CreateImage("PortraitPlaceholder", actor.transform, color, new Vector2(0.14f, 0.08f), new Vector2(0.86f, 0.86f), Vector2.zero, Vector2.zero);
        CreateText("ActorName", actor.transform, label, 36, Color.white, TextAlignmentOptions.Center,
            new Vector2(0.05f, 0.86f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero);
        return actor;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        Image image = CreateImage(name, parent, color, min, max, Vector2.zero, Vector2.zero);
        CanvasGroup group = image.gameObject.AddComponent<CanvasGroup>();
        return image.gameObject;
    }

    private static Image CreateImage(string name, Transform parent, Color color, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        Image image = item.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, float size, Color color, TextAlignmentOptions alignment, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        TextMeshProUGUI label = item.GetComponent<TextMeshProUGUI>();
        label.font = TMP_Settings.defaultFontAsset;
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.enableWordWrapping = true;
        return label;
    }

    private static Button CreateButton(string name, Transform parent, string text, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction action)
    {
        Image image = CreateImage(name, parent, new Color(0.66f, 0.18f, 0.15f, 0.96f), min, max, Vector2.zero, Vector2.zero);
        Button button = image.gameObject.AddComponent<Button>();
        button.onClick.AddListener(action);
        TMP_Text label = CreateText("Label", image.transform, text, 25, Color.white, TextAlignmentOptions.Center,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
    }

    private static IEnumerator Fade(Image image, float from, float to, float duration)
    {
        Color color = image.color;
        color.a = from;
        image.color = color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            image.color = color;
            yield return null;
        }
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
}
