using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class HometownInvestigationManager : MonoBehaviour
{
    private HometownHotspot[] hotspots;
    private ClueCardUI clueCard;
    private CanvasGroup dimPanel;
    private TMP_Text progressText;
    private GameObject interactionHint;
    private RectTransform interactionHintRect;
    private GameObject completionPanel;
    private GameObject memoryMatchPanel;
    private Button organizeButton;
    private RectTransform canvasRect;

    private bool investigationActive;
    private int foundCount;

    private void Awake()
    {
        BindSceneReferences();
        BindHotspots();
        UpdateProgress();
    }

    public void BeginInvestigation()
    {
        investigationActive = true;
        foundCount = 0;

        completionPanel.SetActive(false);
        memoryMatchPanel.SetActive(false);
        interactionHint.SetActive(false);

        for (int i = 0; i < hotspots.Length; i++)
        {
            hotspots[i].SetAvailable(true);
        }

        UpdateProgress();
    }

    public void Inspect(HometownHotspot hotspot)
    {
        if (!investigationActive || hotspot == null)
        {
            return;
        }

        bool isNewClue = hotspot.MarkInvestigated();

        if (isNewClue)
        {
            foundCount++;
            UpdateProgress();

            if (foundCount >= hotspots.Length)
            {
                completionPanel.SetActive(true);
            }
        }

        interactionHint.SetActive(false);
        dimPanel.gameObject.SetActive(true);
        dimPanel.alpha = 0.45f;
        clueCard.Show(hotspot.ClueTitle, "正式文化介绍将在此处补充。");
    }

    public void CloseClueCard()
    {
        dimPanel.alpha = 0f;
        dimPanel.gameObject.SetActive(false);
    }

    public void ShowInteractionHint(Vector2 screenPosition)
    {
        if (!investigationActive)
        {
            return;
        }

        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            null,
            out localPosition);

        interactionHintRect.anchoredPosition = localPosition + new Vector2(0f, 42f);
        interactionHint.SetActive(true);
    }

    public void HideInteractionHint()
    {
        if (interactionHint != null)
        {
            interactionHint.SetActive(false);
        }
    }

    private void OnOrganizeFindingsClicked()
    {
        memoryMatchPanel.SetActive(true);
    }

    private void BindSceneReferences()
    {
        Transform uiRoot = transform.Find("UIRoot");
        Transform investigationHud = uiRoot != null ? uiRoot.Find("InvestigationHUD") : null;

        clueCard = GetComponentInChildren<ClueCardUI>(true);

        if (clueCard != null)
        {
            clueCard.Initialize(this);
        }
        dimPanel = GetCanvasGroup("UIRoot/DimPanel");
        progressText = GetText("UIRoot/InvestigationHUD/ProgressText");
        interactionHint = FindObject(uiRoot, "InteractionHint");
        interactionHintRect = interactionHint != null
            ? interactionHint.GetComponent<RectTransform>()
            : null;
        completionPanel = FindObject(uiRoot, "InvestigationHUD/CompletionPanel");
        memoryMatchPanel = FindObject(uiRoot, "MemoryMatchPanel");
        organizeButton = FindButton(investigationHud, "CompletionPanel/OrganizeButton");
        canvasRect = transform as RectTransform;

        if (organizeButton != null)
        {
            organizeButton.onClick.RemoveListener(OnOrganizeFindingsClicked);
            organizeButton.onClick.AddListener(OnOrganizeFindingsClicked);
        }
    }

    private void BindHotspots()
    {
        hotspots = GetComponentsInChildren<HometownHotspot>(true);

        for (int i = 0; i < hotspots.Length; i++)
        {
            hotspots[i].Initialize(this);
        }
    }

    private void UpdateProgress()
    {
        if (progressText != null && hotspots != null)
        {
            progressText.text = $"线索：{foundCount} / {hotspots.Length}";
        }
    }

    private CanvasGroup GetCanvasGroup(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<CanvasGroup>() : null;
    }

    private TMP_Text GetText(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private static GameObject FindObject(Transform root, string path)
    {
        Transform found = root != null ? root.Find(path) : null;
        return found != null ? found.gameObject : null;
    }

    private static Button FindButton(Transform root, string path)
    {
        Transform found = root != null ? root.Find(path) : null;
        return found != null ? found.GetComponent<Button>() : null;
    }

    private void OnDestroy()
    {
        if (organizeButton != null)
        {
            organizeButton.onClick.RemoveListener(OnOrganizeFindingsClicked);
        }
    }
}
