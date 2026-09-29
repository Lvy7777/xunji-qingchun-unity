using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PosterDIYManager : MonoBehaviour
{
    private const int RequiredElementCount = 3;

    private readonly List<PosterDragItem> dragItems = new List<PosterDragItem>(5);
    private readonly HashSet<PosterDragItem> placedItems = new HashSet<PosterDragItem>();

    private VillageMemoryController villageController;
    private GameObject posterPanel;
    private RectTransform posterCanvas;
    private RectTransform dragSurface;
    private TMP_Text placedCountText;
    private TMP_Text completionText;
    private Button finishButton;
    private bool initialized;
    private bool workCompleted;

    public int PlacedElementCount => placedItems.Count;
    public int RequiredElements => RequiredElementCount;
    public bool CanFinish => placedItems.Count >= RequiredElementCount;
    public bool IsComplete => workCompleted;
    public bool IsFinishButtonEnabled => finishButton != null && finishButton.interactable;
    public IReadOnlyList<PosterDragItem> DragItems => dragItems;

    public void Initialize(VillageMemoryController controller, Transform canvasParent)
    {
        if (initialized)
        {
            return;
        }

        villageController = controller;
        BuildInterface(canvasParent);
        initialized = true;
        posterPanel.SetActive(false);
    }

    public void OpenPosterDIY()
    {
        if (!initialized)
        {
            return;
        }

        posterPanel.SetActive(true);
        ResetPoster();
    }

    public void HidePosterDIY()
    {
        if (posterPanel != null)
        {
            posterPanel.SetActive(false);
        }
    }

    public void ResetPoster()
    {
        StopAllCoroutines();
        placedItems.Clear();
        workCompleted = false;

        for (int i = 0; i < dragItems.Count; i++)
        {
            dragItems[i].ResetToHome();
        }

        completionText.gameObject.SetActive(false);
        UpdateCompletionState();
    }

    public void RegisterPlacedElement(PosterDragItem item)
    {
        if (item == null || workCompleted || !placedItems.Add(item))
        {
            return;
        }

        UpdateCompletionState();
    }

    public bool CompleteArtwork()
    {
        if (!CanFinish || workCompleted)
        {
            return false;
        }

        workCompleted = true;
        finishButton.interactable = false;
        completionText.text = "作品完成！";
        completionText.gameObject.SetActive(true);

        if (villageController != null)
        {
            villageController.CompletePosterTask();
        }

        StartCoroutine(ReturnToMenuRoutine());
        return true;
    }

    public void ReturnToMenuForTesting()
    {
        HidePosterDIY();
        if (villageController != null)
        {
            villageController.ReturnToActivityMenu();
        }
    }

    private IEnumerator ReturnToMenuRoutine()
    {
        yield return new WaitForSecondsRealtime(1.1f);
        HidePosterDIY();

        if (villageController != null)
        {
            villageController.ReturnToActivityMenu();
        }
    }

    private void UpdateCompletionState()
    {
        placedCountText.text = "已放置：" + placedItems.Count + " / " + RequiredElementCount;
        finishButton.interactable = CanFinish && !workCompleted;
    }

    private void BuildInterface(Transform parent)
    {
        posterPanel = CreatePanel("PosterDIYPanel", parent, new Color(0.89f, 0.94f, 0.96f, 1f),
            Vector2.zero, Vector2.one);
        dragSurface = posterPanel.GetComponent<RectTransform>();

        CreatePanel("Header", posterPanel.transform, new Color(0.10f, 0.32f, 0.48f, 1f),
            new Vector2(0f, 0.87f), Vector2.one);
        CreateText("PosterTitle", posterPanel.transform, "我的乡村手抄报", 44, Color.white,
            TextAlignmentOptions.Left, new Vector2(0.05f, 0.89f), new Vector2(0.60f, 0.97f));
        placedCountText = CreateText("PlacedCountText", posterPanel.transform, "已放置：0 / 3", 28, Color.white,
            TextAlignmentOptions.Right, new Vector2(0.60f, 0.89f), new Vector2(0.94f, 0.97f));

        GameObject materialBar = CreatePanel("MaterialBar", posterPanel.transform, new Color(0.95f, 0.90f, 0.77f, 1f),
            new Vector2(0.03f, 0.12f), new Vector2(0.25f, 0.83f));
        CreateText("MaterialTitle", materialBar.transform, "素材栏", 30, new Color(0.25f, 0.18f, 0.10f, 1f),
            TextAlignmentOptions.Center, new Vector2(0.08f, 0.89f), new Vector2(0.92f, 0.98f));

        GameObject posterCanvasObject = CreatePanel("PosterCanvas", posterPanel.transform, Color.white,
            new Vector2(0.30f, 0.17f), new Vector2(0.95f, 0.82f));
        posterCanvas = posterCanvasObject.GetComponent<RectTransform>();
        CreateText("CanvasHint", posterCanvasObject.transform, "把素材拖到这里", 30,
            new Color(0.70f, 0.74f, 0.77f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.20f, 0.42f), new Vector2(0.80f, 0.58f));

        string[] names =
        {
            "Decoration01",
            "Decoration02",
            "Decoration03",
            "TextSticker01",
            "TextSticker02"
        };

        string[] labels =
        {
            "装饰 01",
            "装饰 02",
            "装饰 03",
            "文字贴纸 01",
            "文字贴纸 02"
        };

        Color[] colors =
        {
            new Color(0.88f, 0.35f, 0.23f, 1f),
            new Color(0.96f, 0.68f, 0.24f, 1f),
            new Color(0.32f, 0.62f, 0.30f, 1f),
            new Color(0.25f, 0.52f, 0.78f, 1f),
            new Color(0.56f, 0.39f, 0.72f, 1f)
        };

        for (int i = 0; i < names.Length; i++)
        {
            float maxY = 0.86f - i * 0.165f;
            float minY = maxY - 0.13f;
            GameObject item = CreatePanel(names[i], materialBar.transform, colors[i],
                new Vector2(0.10f, minY), new Vector2(0.90f, maxY));
            CreateText("Label", item.transform, labels[i], 21, Color.white,
                TextAlignmentOptions.Center, Vector2.zero, Vector2.one);

            PosterDragItem dragItem = item.AddComponent<PosterDragItem>();
            dragItem.Initialize(this, posterCanvas, dragSurface);
            dragItems.Add(dragItem);
        }

        finishButton = CreateButton("FinishArtworkButton", posterPanel.transform, "完成作品",
            new Color(0.18f, 0.52f, 0.30f, 1f), new Vector2(0.69f, 0.055f), new Vector2(0.90f, 0.125f));
        finishButton.onClick.AddListener(() => CompleteArtwork());

        completionText = CreateText("CompletionText", posterPanel.transform, "作品完成！", 48,
            new Color(0.12f, 0.58f, 0.25f, 1f), TextAlignmentOptions.Center,
            new Vector2(0.32f, 0.055f), new Vector2(0.66f, 0.13f));
        completionText.gameObject.SetActive(false);

        UpdateCompletionState();
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
