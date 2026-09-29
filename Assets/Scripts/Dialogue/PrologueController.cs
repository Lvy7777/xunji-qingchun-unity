using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class PrologueController : MonoBehaviour
{
    private DialogueManager dialogueManager;
    private GameObject dialoguePanel;
    private GameObject studyBookPanel;
    private GameObject taskPopup;
    private CanvasGroup fadePanel;
    private CanvasGroup chapterTitle;
    private CanvasGroup studyBookGroup;
    private Button startButton;

    private readonly DialogueLine[] openingLines =
    {
        new DialogueLine(DialogueSpeaker.XiaoHe, "老师老师！你们今天要教我们什么呀？"),
        new DialogueLine(DialogueSpeaker.Volunteer, "今天，我们要一起完成一本特别的研学手册。"),
        new DialogueLine(DialogueSpeaker.XiaoHe, "研学手册是什么？"),
        new DialogueLine(DialogueSpeaker.Volunteer, "就是把我们今天看到、听到、学到的东西记录下来。")
    };

    private readonly DialogueLine[] closingLines =
    {
        new DialogueLine(DialogueSpeaker.XiaoHe, "可是……这里什么都没有呀。"),
        new DialogueLine(DialogueSpeaker.Volunteer, "那我们就一起把它填满。")
    };

private void Awake()
    {
        BindSceneReferences();
    }




    private IEnumerator Start()
    {
        if (!ReferencesAreReady())
        {
            Debug.LogError("[Prologue] PrologueController could not find its required UI references.");
            yield break;
        }

        ResetPresentation();

        yield return Fade(fadePanel, 1f, 0f, 1f);
        fadePanel.gameObject.SetActive(false);

        chapterTitle.gameObject.SetActive(true);
        yield return Fade(chapterTitle, 0f, 1f, 0.3f);
        yield return new WaitForSecondsRealtime(1.2f);
        yield return Fade(chapterTitle, 1f, 0f, 0.35f);
        chapterTitle.gameObject.SetActive(false);

        dialoguePanel.SetActive(true);
        for (int i = 0; i < openingLines.Length; i++)
        {
            yield return dialogueManager.ShowLine(openingLines[i]);
        }

        dialogueManager.SetSpeakerVisual(DialogueSpeaker.System);
        dialoguePanel.SetActive(false);
        studyBookPanel.SetActive(true);
        yield return Fade(studyBookGroup, 0f, 1f, 0.25f);
        yield return new WaitForSecondsRealtime(2.4f);
        yield return Fade(studyBookGroup, 1f, 0f, 0.25f);
        studyBookPanel.SetActive(false);

        dialoguePanel.SetActive(true);
        for (int i = 0; i < closingLines.Length; i++)
        {
            yield return dialogueManager.ShowLine(closingLines[i]);
        }

        dialogueManager.SetSpeakerVisual(DialogueSpeaker.System);
        dialoguePanel.SetActive(false);
        taskPopup.SetActive(true);
        yield return PlayTaskPopupAnimation();
    }

    public void BindSceneReferences()
    {
        dialogueManager = GetComponent<DialogueManager>();

        Transform canvas = GameObject.Find("Canvas")?.transform;
        if (canvas == null)
        {
            return;
        }

        dialoguePanel = canvas.Find("DialogueLayer/DialoguePanel")?.gameObject;
        studyBookPanel = canvas.Find("DialogueLayer/StudyBookPanel")?.gameObject;
        taskPopup = canvas.Find("TaskLayer/TaskPopup")?.gameObject;
        fadePanel = canvas.Find("FadePanel")?.GetComponent<CanvasGroup>();
        chapterTitle = canvas.Find("DialogueLayer/ChapterTitle")?.GetComponent<CanvasGroup>();
        studyBookGroup = studyBookPanel?.GetComponent<CanvasGroup>();
        startButton = taskPopup?.transform.Find("StartButton")?.GetComponent<Button>();

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartFindingClicked);
            startButton.onClick.AddListener(OnStartFindingClicked);
        }
    }

    private void ResetPresentation()
    {
        dialogueManager.SetSpeakerVisual(DialogueSpeaker.System);
        dialoguePanel.SetActive(false);
        studyBookPanel.SetActive(false);
        taskPopup.SetActive(false);

        chapterTitle.gameObject.SetActive(false);
        chapterTitle.alpha = 0f;

        fadePanel.gameObject.SetActive(true);
        fadePanel.alpha = 1f;
        fadePanel.blocksRaycasts = true;
    }

    private bool ReferencesAreReady()
    {
        return dialogueManager != null
            && dialoguePanel != null
            && studyBookPanel != null
            && taskPopup != null
            && fadePanel != null
            && chapterTitle != null
            && studyBookGroup != null
            && startButton != null;
    }

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        group.alpha = from;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        group.alpha = to;
        group.blocksRaycasts = to > 0.95f;
    }

    private IEnumerator PlayTaskPopupAnimation()
    {
        RectTransform popupRect = taskPopup.GetComponent<RectTransform>();
        popupRect.localScale = Vector3.one * 0.8f;

        yield return Scale(popupRect, 0.8f, 1.05f, 0.25f);
        yield return Scale(popupRect, 1.05f, 1f, 0.15f);
    }

    private IEnumerator Scale(
        RectTransform target,
        float from,
        float to,
        float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float scale = Mathf.Lerp(from, to, elapsed / duration);
            target.localScale = Vector3.one * scale;
            yield return null;
        }

        target.localScale = Vector3.one * to;
    }

    private void OnStartFindingClicked()
    {
        Debug.Log("Prologue completed. Ready for RedMemory.");
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartFindingClicked);
        }
    }
}
