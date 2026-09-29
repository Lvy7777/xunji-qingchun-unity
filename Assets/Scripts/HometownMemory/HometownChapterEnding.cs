using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class HometownChapterEnding : MonoBehaviour
{
    [SerializeField] private float panelFadeDuration = 0.3f;
    [SerializeField] private string nextSceneName = "YouthMemory";

    private DialogueManager dialogueManager;
    private GameObject dialoguePanel;
    private GameObject investigationHud;
    private GameObject characters;
    private CanvasGroup memoryMatchGroup;
    private CanvasGroup dialogueGroup;
    private CanvasGroup dimGroup;
    private CanvasGroup rewardGroup;
    private CanvasGroup studyBookGroup;
    private CanvasGroup completionGroup;
    private Button continueJourneyButton;

    private bool endingStarted;
    private bool endingCompleted;
    private bool transitionRequested;

    private void Awake()
    {
        BindSceneReferences();
        BindButtons();
        HideEndingPanels();
    }

    public void BeginEnding()
    {
        if (endingStarted)
        {
            return;
        }

        BindSceneReferences();
        BindButtons();

        if (!ReferencesAreReady())
        {
            Debug.LogError("[HometownMemory] Ending UI references are missing.");
            return;
        }

        endingStarted = true;
        StartCoroutine(PlayEnding());
    }

    private IEnumerator PlayEnding()
    {
        if (memoryMatchGroup != null)
        {
            yield return Fade(memoryMatchGroup, memoryMatchGroup.alpha, 0f, panelFadeDuration);
            memoryMatchGroup.gameObject.SetActive(false);
        }

        characters.SetActive(true);
        investigationHud.SetActive(false);
        dialoguePanel.SetActive(true);
        dialogueGroup.alpha = 1f;
        dialogueGroup.blocksRaycasts = true;
        dialogueManager.SetInputEnabled(true);

        yield return dialogueManager.ShowLine(new DialogueLine(
            DialogueSpeaker.XiaoHe, "原来文化不仅仅是书本上的东西。"));
        yield return dialogueManager.ShowLine(new DialogueLine(DialogueSpeaker.Volunteer, "对。"));
        yield return new WaitForSecondsRealtime(0.5f);
        yield return dialogueManager.ShowLine(new DialogueLine(
            DialogueSpeaker.Volunteer,
            "它也可以是一句话、一首歌、一栋房子，甚至是我们每天的生活。"));

        dialogueManager.SetInputEnabled(false);
        yield return Fade(dialogueGroup, 1f, 0f, panelFadeDuration);
        dialoguePanel.SetActive(false);

        GameProgress progress = GameProgress.Ensure();
        bool newlyGranted = progress.TryGrantHometownMemory();

        if (newlyGranted)
        {
            yield return ShowMemoryReward();
        }

        yield return ShowStudyBook(progress);
        ShowCompletion();

        endingCompleted = true;
    }

    private IEnumerator ShowMemoryReward()
    {
        dimGroup.gameObject.SetActive(true);
        dimGroup.alpha = 0f;
        dimGroup.blocksRaycasts = true;
        yield return Fade(dimGroup, 0f, 0.58f, panelFadeDuration);

        rewardGroup.gameObject.SetActive(true);
        rewardGroup.alpha = 0f;
        rewardGroup.transform.localScale = Vector3.one * 0.82f;
        yield return FadeAndScale(rewardGroup, 0f, 1f, 0.82f, 1f, panelFadeDuration);
        yield return new WaitForSecondsRealtime(0.9f);
        yield return FadeAndScale(rewardGroup, 1f, 0f, 1f, 0.92f, panelFadeDuration);
        rewardGroup.gameObject.SetActive(false);

        yield return Fade(dimGroup, dimGroup.alpha, 0f, panelFadeDuration);
        dimGroup.gameObject.SetActive(false);
    }

    private IEnumerator ShowStudyBook(GameProgress progress)
    {
        SetText("UIRoot/StudyBookPanel/BookTitleText", "记忆收集：" + progress.MemoryCount + " / 4");
        SetText("UIRoot/StudyBookPanel/BookEntriesText",
            "红色记忆 —— 已获得\n乡土记忆 —— 已获得\n青春记忆 —— 未解锁\n乡村记忆 —— 未解锁");

        studyBookGroup.gameObject.SetActive(true);
        studyBookGroup.alpha = 0f;
        yield return Fade(studyBookGroup, 0f, 1f, panelFadeDuration);
        yield return new WaitForSecondsRealtime(1.1f);
        yield return Fade(studyBookGroup, 1f, 0f, panelFadeDuration);
        studyBookGroup.gameObject.SetActive(false);
    }

    private void ShowCompletion()
    {
        completionGroup.gameObject.SetActive(true);
        completionGroup.alpha = 0f;
        completionGroup.blocksRaycasts = true;
        StartCoroutine(Fade(completionGroup, 0f, 1f, panelFadeDuration));
    }

    public void ContinueToYouthMemory()
    {
        if (!endingCompleted || transitionRequested)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError($"[HometownMemory] Next scene '{nextSceneName}' is not in Build Settings.");
            return;
        }

        transitionRequested = true;
        if (continueJourneyButton != null)
        {
            continueJourneyButton.interactable = false;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void BindSceneReferences()
    {
        dialogueManager = GetComponent<DialogueManager>();
        dialoguePanel = FindObject("UIRoot/DialogueLayer/DialoguePanel");
        investigationHud = FindObject("UIRoot/InvestigationHUD");
        characters = FindObject("Characters");
        memoryMatchGroup = FindCanvasGroup("UIRoot/MemoryMatchPanel");
        dialogueGroup = FindCanvasGroup("UIRoot/DialogueLayer/DialoguePanel");
        dimGroup = FindCanvasGroup("UIRoot/DimPanel");
        rewardGroup = FindCanvasGroup("UIRoot/MemoryRewardUI");
        studyBookGroup = FindCanvasGroup("UIRoot/StudyBookPanel");
        completionGroup = FindCanvasGroup("UIRoot/ChapterCompletePanel");

        Transform continueButton = transform.Find("UIRoot/ChapterCompletePanel/ContinueJourneyButton");
        continueJourneyButton = continueButton != null ? continueButton.GetComponent<Button>() : null;
    }

    private void BindButtons()
    {
        if (continueJourneyButton == null)
        {
            return;
        }

        continueJourneyButton.onClick.RemoveListener(ContinueToYouthMemory);
        continueJourneyButton.onClick.AddListener(ContinueToYouthMemory);
    }

    private void HideEndingPanels()
    {
        HidePanel(rewardGroup);
        HidePanel(studyBookGroup);
        HidePanel(completionGroup);
    }

    private static void HidePanel(CanvasGroup group)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);
    }

    private bool ReferencesAreReady()
    {
        return dialogueManager != null
            && dialoguePanel != null
            && investigationHud != null
            && characters != null
            && dialogueGroup != null
            && dimGroup != null
            && rewardGroup != null
            && studyBookGroup != null
            && completionGroup != null
            && continueJourneyButton != null;
    }

    private GameObject FindObject(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.gameObject : null;
    }

    private CanvasGroup FindCanvasGroup(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<CanvasGroup>() : null;
    }

    private void SetText(string path, string value)
    {
        Transform found = transform.Find(path);
        TMP_Text text = found != null ? found.GetComponent<TMP_Text>() : null;

        if (text != null)
        {
            text.text = value;
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
}
