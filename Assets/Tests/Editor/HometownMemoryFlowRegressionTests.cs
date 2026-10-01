using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class HometownMemoryFlowRegressionTests
{
    private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

    [Test]
    public void DialogueManager_HidesInactiveDialogueCharacter()
    {
        string source = ReadSource("Assets/Scripts/Dialogue/DialogueManager.cs");

        Assert.That(source, Does.Contain("speaker == DialogueSpeaker.XiaoHe ? 1f : 0f"));
        Assert.That(source, Does.Contain("speaker == DialogueSpeaker.Volunteer ? 1f : 0f"));
        Assert.That(source, Does.Contain("group.gameObject.SetActive(targetAlpha > 0.01f)"));
        Assert.That(source, Does.Contain("SetCharactersVisibleForExploration"));
    }

    [Test]
    public void InvestigationMode_PreservesAndMovesCharacters()
    {
        string source = ReadSource("Assets/Scripts/HometownMemory/HometownMemoryController.cs");
        string method = GetMethodBody(
            source,
            "private IEnumerator EnterInvestigationMode()",
            "private IEnumerator MoveCharactersToSides()");

        Assert.That(method, Does.Contain("dialogueManager.SetCharactersVisibleForExploration();"));
        Assert.That(method, Does.Contain("yield return MoveCharactersToSides();"));
    }

    [Test]
    public void MatchGame_WaitsForContinueBeforeStartingEnding()
    {
        string source = ReadSource("Assets/Scripts/HometownMemory/HometownMatchGame.cs");
        string finishGame = GetMethodBody(
            source,
            "private void FinishGame()",
            "private void OnContinueClicked()");
        string continueClick = GetMethodBody(
            source,
            "private void OnContinueClicked()",
            "private void BindSceneReferences()");

        Assert.That(finishGame, Does.Not.Contain("OnMatchGameCompleted"));
        Assert.That(finishGame, Does.Contain("continueButton.gameObject.SetActive(true);"));
        Assert.That(continueClick, Does.Contain("if (!gameCompleted || completionRaised)"));
        Assert.That(continueClick, Does.Contain("controller?.OnMatchGameCompleted();"));
    }

    [Test]
    public void RedMemory_ShowsOnlyTheCurrentDialogueSpeaker()
    {
        string source = ReadSource("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");

        Assert.That(source, Does.Contain("yield return FadePortrait(xiaoHeGroup, speaker == \"栗拓拓\");"));
        Assert.That(source, Does.Contain("yield return FadePortrait(volunteerGroup, speaker == \"志愿者\");"));
        Assert.That(source, Does.Contain("if (!visible) group.gameObject.SetActive(false);"));
    }

    [Test]
    public void ChapterCompleteButton_HasAnExplicitYouthMemoryRoute()
    {
        string source = ReadSource("Assets/Scripts/HometownMemory/HometownChapterEnding.cs");
        string scene = ReadSource("Assets/Scenes/HometownMemory.unity");

        Assert.That(source, Does.Contain("public void ContinueToYouthMemory()"));
        Assert.That(source, Does.Contain("Application.CanStreamedLevelBeLoaded(nextSceneName)"));
        Assert.That(source, Does.Contain("SceneManager.LoadScene(nextSceneName);"));
        Assert.That(scene, Does.Contain("m_MethodName: ContinueToYouthMemory"));
        Assert.That(scene, Does.Contain("m_StringArgument: YouthMemory"));
    }

    private static string ReadSource(string relativePath)
    {
        return File.ReadAllText(Path.Combine(ProjectRoot, relativePath));
    }

    private static string GetMethodBody(string source, string startMarker, string nextMethodMarker)
    {
        int start = source.IndexOf(startMarker, StringComparison.Ordinal);
        int end = source.IndexOf(nextMethodMarker, start, StringComparison.Ordinal);

        Assert.That(start, Is.GreaterThanOrEqualTo(0), $"Could not find {startMarker}");
        Assert.That(end, Is.GreaterThan(start), $"Could not find the method following {startMarker}");
        return source.Substring(start, end - start);
    }
}
