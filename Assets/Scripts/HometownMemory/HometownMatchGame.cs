using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class HometownMatchGame : MonoBehaviour
{
    private static readonly Color NormalColor = new Color(0.28f, 0.18f, 0.12f, 0.96f);
    private static readonly Color SelectedColor = new Color(0.74f, 0.50f, 0.16f, 1f);
    private static readonly Color LockedColor = new Color(0.25f, 0.56f, 0.36f, 1f);

    private readonly Button[] clueButtons = new Button[3];
    private readonly Button[] targetButtons = new Button[3];
    private readonly UnityAction[] clueActions = new UnityAction[3];
    private readonly UnityAction[] targetActions = new UnityAction[3];

    private TMP_Text statusText;
    private TMP_Text completionText;
    private Button continueButton;
    private HometownMemoryController controller;

    private int selectedClue = -1;
    private int correctCount;
    private bool resolvingError;
    private bool gameCompleted;
    private bool completionRaised;
    private Coroutine errorResetRoutine;

    private void Awake()
    {
        BindSceneReferences();
        BindButtons();
    }

    private void OnEnable()
    {
        if (controller == null)
        {
            BindSceneReferences();
            BindButtons();
        }

        ResetGame();
    }

    public void ResetGame()
    {
        if (!ReferencesAreReady())
        {
            Debug.LogError("[HometownMemory] MemoryMatchPanel is missing a required UI reference.");
            return;
        }

        if (errorResetRoutine != null)
        {
            StopCoroutine(errorResetRoutine);
            errorResetRoutine = null;
        }

        selectedClue = -1;
        correctCount = 0;
        resolvingError = false;
        gameCompleted = false;
        completionRaised = false;
        statusText.text = string.Empty;
        completionText.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        continueButton.interactable = true;

        for (int i = 0; i < 3; i++)
        {
            clueButtons[i].interactable = true;
            targetButtons[i].interactable = true;
            SetButtonColor(clueButtons[i], NormalColor);
            SetButtonColor(targetButtons[i], NormalColor);
        }
    }

    private void OnClueClicked(int clueIndex)
    {
        if (resolvingError || !clueButtons[clueIndex].interactable)
        {
            return;
        }

        selectedClue = clueIndex;
        statusText.text = "请选择对应对象";

        for (int i = 0; i < clueButtons.Length; i++)
        {
            if (clueButtons[i].interactable)
            {
                SetButtonColor(clueButtons[i], i == selectedClue ? SelectedColor : NormalColor);
            }
        }
    }

    private void OnTargetClicked(int targetIndex)
    {
        if (resolvingError || selectedClue < 0 || !targetButtons[targetIndex].interactable)
        {
            return;
        }

        if (selectedClue == targetIndex)
        {
            LockMatchedPair(selectedClue, targetIndex);
            return;
        }

        statusText.text = "好像不是这里……";
        resolvingError = true;
        errorResetRoutine = StartCoroutine(ClearSelectionAfterDelay());
    }

    private void LockMatchedPair(int clueIndex, int targetIndex)
    {
        clueButtons[clueIndex].interactable = false;
        targetButtons[targetIndex].interactable = false;
        SetButtonColor(clueButtons[clueIndex], LockedColor);
        SetButtonColor(targetButtons[targetIndex], LockedColor);

        selectedClue = -1;
        correctCount++;
        statusText.text = "连接成功";

        if (correctCount == 3)
        {
            FinishGame();
        }
    }

    private IEnumerator ClearSelectionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.8f);

        selectedClue = -1;
        resolvingError = false;
        statusText.text = string.Empty;

        for (int i = 0; i < clueButtons.Length; i++)
        {
            if (clueButtons[i].interactable)
            {
                SetButtonColor(clueButtons[i], NormalColor);
            }
        }

        errorResetRoutine = null;
    }

    private void FinishGame()
    {
        if (gameCompleted)
        {
            return;
        }

        gameCompleted = true;
        completionText.text = "乡土的声音已经被重新拼起来了";
        completionText.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);
        continueButton.interactable = true;
    }

    private void OnContinueClicked()
    {
        if (!gameCompleted || completionRaised)
        {
            return;
        }

        completionRaised = true;
        continueButton.interactable = false;
        controller?.OnMatchGameCompleted();
    }

    private void BindSceneReferences()
    {
        controller = GetComponentInParent<HometownMemoryController>(true);
        statusText = GetText("StatusText");
        completionText = GetText("CompletionText");
        continueButton = GetButton("ContinueButton");

        for (int i = 0; i < 3; i++)
        {
            clueButtons[i] = GetButton($"Clues/Clue{(char)('A' + i)}");
            targetButtons[i] = GetButton($"Targets/Target{(char)('A' + i)}");
        }
    }

    private void BindButtons()
    {
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            clueActions[i] = () => OnClueClicked(index);
            targetActions[i] = () => OnTargetClicked(index);

            if (clueButtons[i] != null)
            {
                clueButtons[i].onClick.RemoveAllListeners();
                clueButtons[i].onClick.AddListener(clueActions[i]);
            }

            if (targetButtons[i] != null)
            {
                targetButtons[i].onClick.RemoveAllListeners();
                targetButtons[i].onClick.AddListener(targetActions[i]);
            }
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }

    private bool ReferencesAreReady()
    {
        if (statusText == null || completionText == null || continueButton == null)
        {
            return false;
        }

        for (int i = 0; i < 3; i++)
        {
            if (clueButtons[i] == null || targetButtons[i] == null)
            {
                return false;
            }
        }

        return true;
    }

    private static void SetButtonColor(Button button, Color color)
    {
        Image image = button.targetGraphic as Image;
        if (image != null)
        {
            image.color = color;
        }
    }

    private TMP_Text GetText(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private Button GetButton(string path)
    {
        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<Button>() : null;
    }

    private void OnDestroy()
    {
        for (int i = 0; i < 3; i++)
        {
            if (clueButtons[i] != null && clueActions[i] != null)
            {
                clueButtons[i].onClick.RemoveListener(clueActions[i]);
            }

            if (targetButtons[i] != null && targetActions[i] != null)
            {
                targetButtons[i].onClick.RemoveListener(targetActions[i]);
            }
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
}
