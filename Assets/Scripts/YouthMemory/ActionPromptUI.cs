using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ActionPromptUI : MonoBehaviour
{
    public event Action RetryRequested;

    private CanvasGroup canvasGroup;
    private TextMeshProUGUI promptText;
    private TextMeshProUGUI stageText;
    private TextMeshProUGUI comboText;
    private TextMeshProUGUI lifeText;
    private TextMeshProUGUI progressText;
    private TextMeshProUGUI feedbackText;
    private TextMeshProUGUI instructionText;
    private TextMeshProUGUI sequenceStepText;
    private GameObject promptCard;
    private Button retryButton;
    private bool initialized;

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        GameObject canvasObject = new GameObject(
            "YouthActionGameCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGroup = canvasObject.AddComponent<CanvasGroup>();
        CreatePanel("Dimmer", canvas.transform, new Color(0.02f, 0.06f, 0.12f, 0.72f), Vector2.zero, Vector2.one);
        BuildHeader(canvas.transform);
        BuildPromptCard(canvas.transform);
        BuildFeedback(canvas.transform);
        BuildInstruction(canvas.transform);
        BuildRetryButton(canvas.transform);
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        if (!initialized)
        {
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }

    public IEnumerator FadeOut(float duration)
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        SetVisible(false);
    }

    public void SetStage(int stageNumber, string stageName, float responseTime)
    {
        stageText.text = "STAGE " + stageNumber + " · " + stageName + " · " + responseTime.ToString("0.0") + " 秒";
    }

    public void SetStatus(int combo, int lives, int stageNumber, int progress, int target)
    {
        comboText.text = "COMBO × " + combo;
        lifeText.text = "生命：" + lives;
        progressText.text = "进度：" + progress + " / " + target;
    }

    public void SetPrompt(string prompt)
    {
        promptCard.SetActive(true);
        promptText.text = prompt;
        sequenceStepText.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        HideRetryButton();
    }

    public void SetSequence(string sequence, int position, int length)
    {
        promptCard.SetActive(true);
        promptText.text = sequence;
        sequenceStepText.text = "第 " + position + " / " + length + " 步";
        sequenceStepText.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(false);
        HideRetryButton();
    }

    public void ShowInstruction(string message)
    {
        instructionText.text = message;
    }

    public void ShowFeedback(string feedback, bool isCorrect)
    {
        feedbackText.text = feedback;
        feedbackText.color = isCorrect
            ? new Color(1f, 0.84f, 0.33f, 1f)
            : new Color(1f, 0.42f, 0.42f, 1f);
        feedbackText.gameObject.SetActive(true);
    }

    public void ShowStageClear(int highestCombo)
    {
        promptCard.SetActive(false);
        feedbackText.text = "STAGE CLEAR";
        feedbackText.color = new Color(0.42f, 1f, 0.63f, 1f);
        feedbackText.gameObject.SetActive(true);
        instructionText.text = "最高连击：" + highestCombo;
        HideRetryButton();
    }

    public void ShowGameOver()
    {
        promptCard.SetActive(false);
        feedbackText.text = "挑战失败";
        feedbackText.color = new Color(1f, 0.42f, 0.42f, 1f);
        feedbackText.gameObject.SetActive(true);
        instructionText.text = "生命耗尽，调整节奏后再试一次";
        retryButton.gameObject.SetActive(true);
    }

    public void HideRetryButton()
    {
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(false);
        }
    }

    private void BuildHeader(Transform parent)
    {
        GameObject header = CreatePanel("StatusBar", parent, new Color(0.04f, 0.10f, 0.19f, 0.94f),
            new Vector2(0.06f, 0.84f), new Vector2(0.94f, 0.96f));

        stageText = CreateText("StageText", header.transform, string.Empty, 24, new Color(0.53f, 0.80f, 1f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.20f, 0.56f), new Vector2(0.80f, 0.94f));
        comboText = CreateText("ComboText", header.transform, "COMBO × 0", 28, new Color(1f, 0.84f, 0.33f, 1f),
            TextAlignmentOptions.Left, new Vector2(0.04f, 0.08f), new Vector2(0.35f, 0.58f));
        lifeText = CreateText("LifeText", header.transform, "生命：3", 28, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.58f));
        progressText = CreateText("ProgressText", header.transform, "进度：0 / 4", 28, Color.white,
            TextAlignmentOptions.Right, new Vector2(0.65f, 0.08f), new Vector2(0.96f, 0.58f));
    }

    private void BuildPromptCard(Transform parent)
    {
        promptCard = CreatePanel("PromptCard", parent, new Color(0.07f, 0.15f, 0.28f, 0.98f),
            new Vector2(0.25f, 0.31f), new Vector2(0.75f, 0.68f));

        CreateText("PromptTitle", promptCard.transform, "动作密码", 30, new Color(0.53f, 0.80f, 1f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.10f, 0.72f), new Vector2(0.90f, 0.90f));
        promptText = CreateText("PromptText", promptCard.transform, string.Empty, 98, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.05f, 0.22f), new Vector2(0.95f, 0.72f));
        sequenceStepText = CreateText("SequenceStepText", promptCard.transform, string.Empty, 24,
            new Color(0.74f, 0.87f, 1f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.10f, 0.07f), new Vector2(0.90f, 0.21f));
    }

    private void BuildFeedback(Transform parent)
    {
        feedbackText = CreateText("FeedbackText", parent, string.Empty, 62, Color.white,
            TextAlignmentOptions.Center, new Vector2(0.16f, 0.18f), new Vector2(0.84f, 0.31f));
        feedbackText.gameObject.SetActive(false);
    }

    private void BuildInstruction(Transform parent)
    {
        instructionText = CreateText("InstructionText", parent, "按下屏幕中央显示的按键", 28,
            new Color(0.84f, 0.90f, 1f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.16f, 0.10f), new Vector2(0.84f, 0.17f));
    }

    private void BuildRetryButton(Transform parent)
    {
        retryButton = CreateButton("RetryButton", parent, "重新挑战",
            new Vector2(0.40f, 0.035f), new Vector2(0.60f, 0.095f));
        retryButton.onClick.AddListener(() => RetryRequested?.Invoke());
        retryButton.gameObject.SetActive(false);
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

    private static Button CreateButton(string name, Transform parent, string label, Vector2 min, Vector2 max)
    {
        GameObject buttonObject = CreatePanel(name, parent, new Color(0.86f, 0.53f, 0.18f, 1f), min, max);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        CreateText("Label", buttonObject.transform, label, 26, Color.white, TextAlignmentOptions.Center,
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
        label.enableWordWrapping = false;
        return label;
    }
}
