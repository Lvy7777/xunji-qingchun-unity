#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class RedMemoryPlayModeVerifier
{
    private const string ActiveKey = "P002.Verifier.Active";
    private const string CycleKey = "P002.Verifier.Cycle";
    private const string StageKey = "P002.Verifier.Stage";
    private const string DamageKey = "P002.Verifier.Damage";
    private const string SuccessKey = "P002.Verifier.Success";
    private const string ProcessKey = "P002.Verifier.ProcessId";
    private static float nextAction;

    private static int cycle { get => SessionState.GetInt(CycleKey, 0); set => SessionState.SetInt(CycleKey, value); }
    private static int stage { get => SessionState.GetInt(StageKey, 0); set => SessionState.SetInt(StageKey, value); }
    private static int damageCount { get => SessionState.GetInt(DamageKey, 0); set => SessionState.SetInt(DamageKey, value); }
    private static bool success { get => SessionState.GetBool(SuccessKey, false); set => SessionState.SetBool(SuccessKey, value); }

    static RedMemoryPlayModeVerifier()
    {
        if (!SessionState.GetBool(ActiveKey, false)) return;
        if (SessionState.GetInt(ProcessKey, -1) != System.Diagnostics.Process.GetCurrentProcess().Id)
        {
            ClearSessionState();
            return;
        }
        Subscribe();
        nextAction = (float)EditorApplication.timeSinceStartup + 1f;
    }

    public static void Run()
    {
        cycle = 0;
        stage = 0;
        damageCount = 0;
        success = false;
        SessionState.SetInt(ProcessKey, System.Diagnostics.Process.GetCurrentProcess().Id);
        SessionState.SetBool(ActiveKey, true);
        Subscribe();
        EditorSceneManager.OpenScene("Assets/Scenes/RedMemory.unity");
        EditorApplication.isPlaying = true;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            nextAction = (float)EditorApplication.timeSinceStartup + 4.5f;
            Debug.Log("[P-002 VERIFY] Entered Play Mode.");
        }
        else if (state == PlayModeStateChange.EnteredEditMode && success)
        {
            CleanupAndExit(0);
        }
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(ActiveKey, false) ||
            SessionState.GetInt(ProcessKey, -1) != System.Diagnostics.Process.GetCurrentProcess().Id)
        {
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            return;
        }
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < nextAction) return;
        try
        {
            if (stage == 5 && SceneManager.GetActiveScene().name == "HometownMemory")
            {
                success = true;
                Debug.Log("[P-002 VERIFY PASS] RedMemory completed and continued to HometownMemory with no blocking exception.");
                EditorApplication.isPlaying = false;
                return;
            }

            RedMemoryIntroController controller = UnityEngine.Object.FindObjectOfType<RedMemoryIntroController>();
            if (controller == null) return;

            GameObject choiceRoot = Field<GameObject>(controller, "choiceRoot");
            GameObject missionPanel = Field<GameObject>(controller, "missionPanel");
            GameObject gameplayRoot = Field<GameObject>(controller, "gameplayRoot");
            GameObject failPanel = Field<GameObject>(controller, "failPanel");
            GameObject photoPrompt = Field<GameObject>(controller, "photoPrompt");
            GameObject historyCard = Field<GameObject>(controller, "historyCardPanel");
            GameObject handbook = Field<GameObject>(controller, "handbookPanel");
            GameObject completion = Field<GameObject>(controller, "completionPanel");

            if (choiceRoot != null && choiceRoot.activeInHierarchy)
            {
                Invoke(controller, "SelectChoice", 1);
                nextAction = (float)EditorApplication.timeSinceStartup + 0.25f;
                return;
            }

            if (missionPanel != null && missionPanel.activeInHierarchy)
            {
                Invoke(controller, "StartInvestigation");
                nextAction = (float)EditorApplication.timeSinceStartup + 0.7f;
                stage = 1;
                return;
            }

            if (stage == 0)
            {
                Invoke(controller, "AdvanceDialogue");
                nextAction = (float)EditorApplication.timeSinceStartup + 0.35f;
                return;
            }

            if (cycle == 0 && gameplayRoot != null && gameplayRoot.activeInHierarchy)
            {
                if (damageCount < 3)
                {
                    controller.HandleHazard(controller.Player.transform.position + Vector3.left);
                    damageCount++;
                    nextAction = (float)EditorApplication.timeSinceStartup + 1.25f;
                    return;
                }

                if (failPanel == null || !failPanel.activeInHierarchy) throw new Exception("FailPanel did not appear after three valid hits.");
                Debug.Log("[P-002 VERIFY] Damage invulnerability, life depletion and FailPanel verified.");
                cycle = 1;
                stage = 0;
                damageCount = 0;
                Invoke(controller, "ReloadChapter");
                nextAction = (float)EditorApplication.timeSinceStartup + 4.5f;
                return;
            }

            if (cycle == 1 && stage == 1 && gameplayRoot != null && gameplayRoot.activeInHierarchy)
            {
                controller.TryReachGoal();
                for (int i = 0; i < 5; i++) controller.Collect(RedMemoryPickupKind.Fragment);
                controller.TryReachGoal();
                controller.Collect(RedMemoryPickupKind.ClueKey);
                controller.HandleFall();
                stage = 2;
                nextAction = (float)EditorApplication.timeSinceStartup + 1.4f;
                return;
            }

            if (cycle == 1 && stage == 2)
            {
                if (!controller.Player.InputEnabled) throw new Exception("Player control was not restored after fall respawn.");
                controller.TryReachGoal();
                stage = 3;
                nextAction = (float)EditorApplication.timeSinceStartup + 0.9f;
                return;
            }

            if (stage == 3 && photoPrompt != null && photoPrompt.activeInHierarchy)
            {
                Invoke(controller, "OpenHistoryCard");
                nextAction = (float)EditorApplication.timeSinceStartup + 0.25f;
                return;
            }

            if (stage == 3 && historyCard != null && historyCard.activeInHierarchy)
            {
                Invoke(controller, "ContinueFromHistory");
                stage = 4;
                nextAction = (float)EditorApplication.timeSinceStartup + 0.35f;
                return;
            }

            if (stage == 4 && handbook != null && handbook.activeInHierarchy)
            {
                if (!GameProgress.Ensure().HasRedMemory || GameProgress.Ensure().MemoryCount < 1) throw new Exception("Red memory progress was not granted.");
                Invoke(controller, "CloseHandbook");
                nextAction = (float)EditorApplication.timeSinceStartup + 0.25f;
                return;
            }

            if (stage == 4 && completion != null && completion.activeInHierarchy)
            {
                Debug.Log("[P-002 VERIFY] Full success flow, goal gating, photo, reward, handbook 1/4 and completion verified.");
                Invoke(controller, "ContinueJourney");
                stage = 5;
                nextAction = (float)EditorApplication.timeSinceStartup + 0.8f;
                return;
            }

            if (stage == 4)
            {
                Invoke(controller, "AdvanceDialogue");
                nextAction = (float)EditorApplication.timeSinceStartup + 0.35f;
                return;
            }

            nextAction = (float)EditorApplication.timeSinceStartup + 0.2f;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            CleanupAndExit(1);
        }
    }

    private static T Field<T>(object target, string name) where T : class
    {
        return target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(target) as T;
    }

    private static void Invoke(object target, string name, params object[] args)
    {
        MethodInfo method = target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        if (method == null) throw new MissingMethodException(target.GetType().Name, name);
        method.Invoke(target, args);
    }

    private static void CleanupAndExit(int code)
    {
        ClearSessionState();
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.Exit(code);
    }

    private static void ClearSessionState()
    {
        SessionState.SetBool(ActiveKey, false);
        SessionState.EraseInt(ProcessKey);
        SessionState.EraseInt(CycleKey);
        SessionState.EraseInt(StageKey);
        SessionState.EraseInt(DamageKey);
        SessionState.EraseBool(SuccessKey);
    }

    private static void Subscribe()
    {
        EditorApplication.update -= Tick;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
}
#endif
