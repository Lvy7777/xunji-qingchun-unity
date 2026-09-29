using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(ActionPromptUI))]
public sealed class YouthActionGameManager : MonoBehaviour
{
    private const int StageOneTarget = 4;
    private const int StageTwoTarget = 6;
    private const int StageThreeSequenceTarget = 3;
    private const int SequenceLength = 3;
    private const int StartingLives = 3;
    private const float FeedbackDuration = 0.45f;
    private const float ClearDisplayDuration = 1.0f;
    private const float PerfectRatio = 0.4f;

    private static readonly KeyCode[] PromptKeys =
    {
        KeyCode.A, KeyCode.D, KeyCode.W, KeyCode.Space
    };

    private static readonly string[] PromptLabels =
    {
        "A", "D", "W", "SPACE"
    };

    private enum GameState
    {
        Inactive,
        AwaitingInput,
        ShowingFeedback,
        StageClear,
        Failed
    }

    public event Action ChallengeCompleted;

    private readonly KeyCode[] sequence = new KeyCode[SequenceLength];
    private ActionPromptUI promptUI;
    private GameState state = GameState.Inactive;
    private int stageNumber;
    private int stageProgress;
    private int combo;
    private int highestCombo;
    private int lives;
    private int currentPromptIndex = -1;
    private int sequencePosition;
    private bool sequenceActive;
    private bool completionNotified;
    private bool completionTransitionStarted;
    private float inputStartedAt;
    private float feedbackEndsAt;

    public int StageNumber => stageNumber;
    public int StageProgress => stageProgress;
    public int StageTarget => GetStageTarget(stageNumber);
    public int Combo => combo;
    public int HighestCombo => highestCombo;
    public int Lives => lives;
    public int SequencePosition => sequencePosition;
    public int SequenceLengthValue => SequenceLength;
    public float CurrentResponseTime => GetResponseTime(stageNumber);
    public bool IsAwaitingInput => state == GameState.AwaitingInput;
    public bool IsStageClear => state == GameState.StageClear;
    public bool IsFailed => state == GameState.Failed;
    public bool IsSequenceStage => stageNumber == 3;
    public KeyCode CurrentExpectedKey => IsSequenceStage
        ? (sequenceActive ? sequence[sequencePosition] : KeyCode.None)
        : (currentPromptIndex < 0 ? KeyCode.None : PromptKeys[currentPromptIndex]);
    public string CurrentPrompt => IsSequenceStage ? FormatSequence() :
        (currentPromptIndex < 0 ? string.Empty : PromptLabels[currentPromptIndex]);

    private void Awake()
    {
        promptUI = GetComponent<ActionPromptUI>();
    }

    private void Update()
    {
        if (state == GameState.AwaitingInput)
        {
            if (TryReadPromptKey(out KeyCode key))
            {
                SubmitKey(key);
            }
            else if (Time.unscaledTime - inputStartedAt >= CurrentResponseTime)
            {
                RegisterMiss();
            }

            return;
        }

        if (state == GameState.ShowingFeedback && Time.unscaledTime >= feedbackEndsAt)
        {
            ContinueAfterFeedback();
        }
    }

    public void StartGame()
    {
        EnsureUI();
        StopAllCoroutines();

        combo = 0;
        highestCombo = 0;
        lives = StartingLives;
        currentPromptIndex = -1;
        sequencePosition = 0;
        sequenceActive = false;
        completionNotified = false;
        completionTransitionStarted = false;

        promptUI.SetVisible(true);
        promptUI.HideRetryButton();
        promptUI.SetStatus(combo, lives, 1, 0, StageOneTarget);
        BeginStage(1);
    }

    public bool SubmitKey(KeyCode key)
    {
        if (state != GameState.AwaitingInput || !IsPromptKey(key))
        {
            return false;
        }

        if (key == CurrentExpectedKey)
        {
            RegisterCorrect(Time.unscaledTime - inputStartedAt);
        }
        else
        {
            RegisterMiss();
        }

        return true;
    }

    public void ForceTimeoutForTesting()
    {
        if (state == GameState.AwaitingInput)
        {
            RegisterMiss();
        }
    }

    public void CompleteFeedbackForTesting()
    {
        if (state == GameState.ShowingFeedback)
        {
            ContinueAfterFeedback();
        }
    }

    public void FinishStageClearForTesting()
    {
        if (state == GameState.StageClear)
        {
            NotifyChallengeCompleted();
        }
    }

    private void EnsureUI()
    {
        if (promptUI == null)
        {
            promptUI = GetComponent<ActionPromptUI>();
        }

        if (promptUI == null)
        {
            promptUI = gameObject.AddComponent<ActionPromptUI>();
        }

        promptUI.Initialize();
        promptUI.RetryRequested -= StartGame;
        promptUI.RetryRequested += StartGame;
    }

    private void BeginStage(int newStage)
    {
        stageNumber = newStage;
        stageProgress = 0;
        sequenceActive = false;
        sequencePosition = 0;

        promptUI.SetStage(stageNumber, GetStageName(stageNumber), CurrentResponseTime);
        promptUI.SetStatus(combo, lives, stageNumber, stageProgress, StageTarget);
        ShowNextChallenge();
    }

    private void ShowNextChallenge()
    {
        if (stageNumber == 3)
        {
            StartNewSequence();
        }
        else
        {
            StartSinglePrompt();
        }
    }

    private void StartSinglePrompt()
    {
        int nextIndex = UnityEngine.Random.Range(0, PromptKeys.Length);
        if (PromptKeys.Length > 1 && nextIndex == currentPromptIndex)
        {
            nextIndex = (nextIndex + 1 + UnityEngine.Random.Range(0, PromptKeys.Length - 1)) % PromptKeys.Length;
        }

        currentPromptIndex = nextIndex;
        state = GameState.AwaitingInput;
        inputStartedAt = Time.unscaledTime;
        promptUI.SetPrompt(CurrentPrompt);
        promptUI.ShowInstruction("按下对应按键");
    }

    private void StartNewSequence()
    {
        for (int i = 0; i < sequence.Length; i++)
        {
            sequence[i] = PromptKeys[UnityEngine.Random.Range(0, PromptKeys.Length)];
        }

        sequencePosition = 0;
        sequenceActive = true;
        ResumeSequenceStep();
    }

    private void ResumeSequenceStep()
    {
        state = GameState.AwaitingInput;
        inputStartedAt = Time.unscaledTime;
        promptUI.SetSequence(CurrentPrompt, sequencePosition + 1, SequenceLength);
        promptUI.ShowInstruction("按顺序完成动作序列");
    }

    private void RegisterCorrect(float elapsed)
    {
        combo++;
        highestCombo = Mathf.Max(highestCombo, combo);

        if (stageNumber == 3)
        {
            sequencePosition++;
            if (sequencePosition >= SequenceLength)
            {
                stageProgress++;
                sequenceActive = false;
            }
        }
        else
        {
            stageProgress++;
        }

        state = GameState.ShowingFeedback;
        feedbackEndsAt = Time.unscaledTime + FeedbackDuration;
        promptUI.SetStatus(combo, lives, stageNumber, stageProgress, StageTarget);

        string rating = elapsed <= CurrentResponseTime * PerfectRatio ? "Perfect!" : "Good!";
        if (combo == 5)
        {
            rating += "\nNice!";
        }
        else if (combo == 10)
        {
            rating += "\nAmazing!";
        }

        promptUI.ShowFeedback(rating, true);
    }

    private void RegisterMiss()
    {
        combo = 0;
        lives = Mathf.Max(0, lives - 1);
        sequenceActive = false;
        sequencePosition = 0;
        state = GameState.ShowingFeedback;
        feedbackEndsAt = Time.unscaledTime + FeedbackDuration;

        promptUI.SetStatus(combo, lives, stageNumber, stageProgress, StageTarget);
        promptUI.ShowFeedback("Miss", false);
    }

    private void ContinueAfterFeedback()
    {
        if (lives <= 0)
        {
            state = GameState.Failed;
            promptUI.ShowGameOver();
            return;
        }

        if (stageProgress >= StageTarget)
        {
            if (stageNumber < 3)
            {
                BeginStage(stageNumber + 1);
            }
            else
            {
                BeginStageClear();
            }

            return;
        }

        if (stageNumber == 3 && sequenceActive)
        {
            ResumeSequenceStep();
        }
        else
        {
            ShowNextChallenge();
        }
    }

    private void BeginStageClear()
    {
        state = GameState.StageClear;
        promptUI.ShowStageClear(highestCombo);

        if (!completionTransitionStarted)
        {
            completionTransitionStarted = true;
            StartCoroutine(CompleteChallengeRoutine());
        }
    }

    private IEnumerator CompleteChallengeRoutine()
    {
        yield return new WaitForSecondsRealtime(ClearDisplayDuration);
        yield return promptUI.FadeOut(0.35f);
        NotifyChallengeCompleted();
    }

    private void NotifyChallengeCompleted()
    {
        if (completionNotified)
        {
            return;
        }

        completionNotified = true;
        ChallengeCompleted?.Invoke();
    }

    private static int GetStageTarget(int stage)
    {
        switch (stage)
        {
            case 1: return StageOneTarget;
            case 2: return StageTwoTarget;
            case 3: return StageThreeSequenceTarget;
            default: return 0;
        }
    }

    private static float GetResponseTime(int stage)
    {
        switch (stage)
        {
            case 1: return 1.5f;
            case 2: return 1.1f;
            case 3: return 0.9f;
            default: return 0f;
        }
    }

    private static string GetStageName(int stage)
    {
        switch (stage)
        {
            case 1: return "热身";
            case 2: return "加速";
            case 3: return "动作序列";
            default: return string.Empty;
        }
    }

    private string FormatSequence()
    {
        string value = string.Empty;
        for (int i = 0; i < sequence.Length; i++)
        {
            if (i > 0)
            {
                value += " → ";
            }

            value += PromptLabels[GetPromptIndex(sequence[i])];
        }

        return value;
    }

    private static int GetPromptIndex(KeyCode key)
    {
        for (int i = 0; i < PromptKeys.Length; i++)
        {
            if (PromptKeys[i] == key)
            {
                return i;
            }
        }

        return 0;
    }

    private static bool IsPromptKey(KeyCode key)
    {
        return key == KeyCode.A || key == KeyCode.D || key == KeyCode.W || key == KeyCode.Space;
    }

    private static bool TryReadPromptKey(out KeyCode key)
    {
        if (Input.GetKeyDown(KeyCode.A)) { key = KeyCode.A; return true; }
        if (Input.GetKeyDown(KeyCode.D)) { key = KeyCode.D; return true; }
        if (Input.GetKeyDown(KeyCode.W)) { key = KeyCode.W; return true; }
        if (Input.GetKeyDown(KeyCode.Space)) { key = KeyCode.Space; return true; }

        key = KeyCode.None;
        return false;
    }
}
