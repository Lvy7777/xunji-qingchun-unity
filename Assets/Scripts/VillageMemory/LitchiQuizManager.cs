using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LitchiQuizManager : MonoBehaviour
{
    private const int QuestionCount = 5;
    private const float FeedbackDelay = 0.45f;
    private const float DiscoveryDelay = 0.85f;

    private static readonly string[] Questions =
    {
        "示例题目 1",
        "示例题目 2",
        "示例题目 3",
        "示例题目 4",
        "示例题目 5"
    };

    private static readonly int[] CorrectAnswers = { 0, 1, 2, 0, 1 };

    private VillageMemoryController villageController;
    private GameObject quizPanel;
    private GameObject discoveryCard;
    private GameObject completionCard;
    private TMP_Text progressText;
    private TMP_Text questionText;
    private TMP_Text feedbackText;
    private readonly Button[] answerButtons = new Button[3];
    private int currentQuestionIndex;
    private bool acceptingAnswer;
    private bool pendingCorrectAnswer;
    private bool quizCompleted;
    private bool initialized;
    private string lastFeedback = string.Empty;

    public int CurrentQuestionNumber => currentQuestionIndex + 1;
    public int CompletedQuestionCount => currentQuestionIndex;
    public bool IsAcceptingAnswer => acceptingAnswer;
    public bool IsComplete => quizCompleted;
    public string LastFeedback => lastFeedback;
    public int CurrentCorrectAnswerIndex => CorrectAnswers[Mathf.Clamp(currentQuestionIndex, 0, QuestionCount - 1)];

    public void Initialize(VillageMemoryController controller, Transform canvasParent)
    {
        if (initialized)
        {
            return;
        }

        villageController = controller;
        BuildInterface(canvasParent);
        initialized = true;
        quizPanel.SetActive(false);
    }

    public void OpenQuiz()
    {
        if (!initialized)
        {
            return;
        }

        quizPanel.SetActive(true);
        ResetQuiz();
    }

    public void HideQuiz()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }
    }

    public void ResetQuiz()
    {
        StopAllCoroutines();
        currentQuestionIndex = 0;
        acceptingAnswer = true;
        pendingCorrectAnswer = false;
        quizCompleted = false;
        lastFeedback = string.Empty;

        discoveryCard.SetActive(false);
        completionCard.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        SetAnswerButtonsInteractable(true);
        DisplayCurrentQuestion();
    }

    public bool SubmitAnswer(int answerIndex)
    {
        if (!acceptingAnswer || quizCompleted || answerIndex < 0 || answerIndex >= answerButtons.Length)
        {
            return false;
        }

        acceptingAnswer = false;
        SetAnswerButtonsInteractable(false);

        if (answerIndex == CurrentCorrectAnswerIndex)
        {
            pendingCorrectAnswer = true;
            lastFeedback = "回答正确";
            ShowFeedback(lastFeedback, true);
            StartCoroutine(CorrectAnswerRoutine());
        }
        else
        {
            pendingCorrectAnswer = false;
            lastFeedback = "再想一想";
            ShowFeedback(lastFeedback, false);
            StartCoroutine(WrongAnswerRoutine());
        }

        return true;
    }

    public void CompleteFeedbackForTesting()
    {
        StopAllCoroutines();

        if (pendingCorrectAnswer)
        {
            discoveryCard.SetActive(false);
            AdvanceAfterCorrectAnswer();
        }
        else if (!quizCompleted)
        {
            feedbackText.gameObject.SetActive(false);
            acceptingAnswer = true;
            SetAnswerButtonsInteractable(true);
        }
    }

    public void ReturnToMenuForTesting()
    {
        HideQuiz();
        if (villageController != null)
        {
            villageController.ReturnToActivityMenu();
        }
    }

    private IEnumerator CorrectAnswerRoutine()
    {
        yield return new WaitForSecondsRealtime(FeedbackDelay);
        feedbackText.gameObject.SetActive(false);
        discoveryCard.SetActive(true);
        yield return new WaitForSecondsRealtime(DiscoveryDelay);
        discoveryCard.SetActive(false);
        AdvanceAfterCorrectAnswer();
    }

    private IEnumerator WrongAnswerRoutine()
    {
        yield return new WaitForSecondsRealtime(FeedbackDelay);
        feedbackText.gameObject.SetActive(false);
        acceptingAnswer = true;
        SetAnswerButtonsInteractable(true);
    }

    private void AdvanceAfterCorrectAnswer()
    {
        if (!pendingCorrectAnswer || quizCompleted)
        {
            return;
        }

        pendingCorrectAnswer = false;
        currentQuestionIndex++;

        if (currentQuestionIndex >= QuestionCount)
        {
            CompleteQuiz();
            return;
        }

        acceptingAnswer = true;
        SetAnswerButtonsInteractable(true);
        DisplayCurrentQuestion();
    }

    private void CompleteQuiz()
    {
        if (quizCompleted)
        {
            return;
        }

        quizCompleted = true;
        acceptingAnswer = false;
        SetAnswerButtonsInteractable(false);
        progressText.text = "第 5 关 / 5";
        questionText.text = "荔枝小博士挑战完成";
        feedbackText.gameObject.SetActive(false);
        completionCard.SetActive(true);

        if (villageController != null)
        {
            villageController.CompleteLitchiTask();
        }

        StartCoroutine(ReturnToMenuRoutine());
    }

    private IEnumerator ReturnToMenuRoutine()
    {
        yield return new WaitForSecondsRealtime(1.1f);
        HideQuiz();

        if (villageController != null)
        {
            villageController.ReturnToActivityMenu();
        }
    }

    private void DisplayCurrentQuestion()
    {
        progressText.text = "第 " + (currentQuestionIndex + 1) + " 关 / " + QuestionCount;
        questionText.text = Questions[currentQuestionIndex];
        feedbackText.gameObject.SetActive(false);
        discoveryCard.SetActive(false);
    }

    private void ShowFeedback(string message, bool correct)
    {
        feedbackText.text = message;
        feedbackText.color = correct
            ? new Color(0.18f, 0.62f, 0.26f, 1f)
            : new Color(0.86f, 0.32f, 0.20f, 1f);
        feedbackText.gameObject.SetActive(true);
    }

    private void SetAnswerButtonsInteractable(bool value)
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].interactable = value;
            }
        }
    }

    private void BuildInterface(Transform parent)
    {
        quizPanel = CreatePanel("LitchiQuizPanel", parent, new Color(0.96f, 0.93f, 0.83f, 1f),
            Vector2.zero, Vector2.one);

        CreatePanel("Header", quizPanel.transform, new Color(0.58f, 0.16f, 0.11f, 1f),
            new Vector2(0f, 0.84f), Vector2.one);
        CreateText("QuizTitle", quizPanel.transform, "荔枝小博士", 46, Color.white,
            TextAlignmentOptions.Left, new Vector2(0.07f, 0.865f), new Vector2(0.55f, 0.965f));
        progressText = CreateText("ProgressText", quizPanel.transform, "第 1 关 / 5", 30, Color.white,
            TextAlignmentOptions.Right, new Vector2(0.55f, 0.865f), new Vector2(0.93f, 0.965f));

        GameObject questionCard = CreatePanel("QuestionCard", quizPanel.transform, Color.white,
            new Vector2(0.15f, 0.50f), new Vector2(0.85f, 0.78f));
        questionText = CreateText("QuestionText", questionCard.transform, "示例题目 1", 42,
            new Color(0.25f, 0.13f, 0.08f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.07f, 0.16f), new Vector2(0.93f, 0.84f));

        string[] letters = { "A", "B", "C" };
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int answerIndex = i;
            float minX = 0.12f + i * 0.27f;
            float maxX = minX + 0.22f;
            answerButtons[i] = CreateButton(
                "AnswerButton" + letters[i],
                quizPanel.transform,
                letters[i] + "\n占位答案 " + letters[i],
                new Color(0.84f - i * 0.08f, 0.38f + i * 0.10f, 0.17f + i * 0.16f, 1f),
                new Vector2(minX, 0.25f),
                new Vector2(maxX, 0.45f));
            answerButtons[i].onClick.AddListener(() => SubmitAnswer(answerIndex));
        }

        feedbackText = CreateText("FeedbackText", quizPanel.transform, string.Empty, 34,
            Color.white, TextAlignmentOptions.Center, new Vector2(0.25f, 0.15f), new Vector2(0.75f, 0.23f));
        feedbackText.gameObject.SetActive(false);

        discoveryCard = CreatePanel("DiscoveryCard", quizPanel.transform, new Color(0.96f, 0.80f, 0.33f, 1f),
            new Vector2(0.27f, 0.06f), new Vector2(0.73f, 0.22f));
        CreateText("DiscoveryTitle", discoveryCard.transform, "今日发现", 28,
            new Color(0.35f, 0.17f, 0.03f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.05f, 0.53f), new Vector2(0.95f, 0.90f));
        CreateText("DiscoveryContent", discoveryCard.transform, "正式知识内容将在这里补充", 23,
            new Color(0.35f, 0.17f, 0.03f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.53f));

        completionCard = CreatePanel("CompletionCard", quizPanel.transform, new Color(0.11f, 0.38f, 0.18f, 0.97f),
            new Vector2(0.24f, 0.30f), new Vector2(0.76f, 0.67f));
        CreateText("CompletionText", completionCard.transform, "荔枝小博士挑战完成", 42, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.25f), new Vector2(0.92f, 0.75f));
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
}
