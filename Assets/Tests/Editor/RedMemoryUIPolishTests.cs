using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class RedMemoryUIPolishTests
{
    private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

    [Test]
    public void RedMemory_HasChapterScopedResponsivePresentation()
    {
        string source = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");

        Assert.That(source, Does.Contain("typeof(ClayThemeOptOut)"));
        Assert.That(source, Does.Contain("CanvasScaler.ScaleMode.ScaleWithScreenSize"));
        Assert.That(source, Does.Contain("scaler.matchWidthOrHeight = 0.5f"));
        Assert.That(source, Does.Contain("ApplyRedMemoryPresentation();"));
    }

    [Test]
    public void RedMemory_HasCentralBodyFontReplacementPoint()
    {
        string source = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");

        Assert.That(source, Does.Contain("[SerializeField] private TMP_FontAsset bodyFont;"));
        Assert.That(source, Does.Contain("Fonts/HYAoJiaoTiJian/HYAoJiaoTiJian"));
        Assert.That(source, Does.Contain("Fonts/NotoSansSC-GameTextV3"));
        Assert.That(source, Does.Contain("texts[i].font = isChapterTitle ? titleFont : bodyFont;"));
    }

    [Test]
    public void RedMemory_AllGeneratedButtonsUseSharedAnimator()
    {
        string source = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");

        Assert.That(source, Does.Contain("image.gameObject.AddComponent<UIButtonAnimator>()"));
        Assert.That(source, Does.Contain("if (buttons[i].GetComponent<UIButtonAnimator>() == null)"));
        Assert.That(source, Does.Not.Contain("new GameObject(\"RetryButton\""));
        Assert.That(source, Does.Not.Contain("new GameObject(\"ContinueJourneyButton\""));
    }

    [Test]
    public void RedMemory_KeepsReplaceableArtSlots()
    {
        string source = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");

        for (int i = 1; i <= 14; i++)
        {
            Assert.That(source, Does.Contain("R_ART_" + i.ToString("000") + "_"));
        }
    }

    [Test]
    public void RedMemory_UsesTheCompleteXiaoHeGameplaySpriteSet()
    {
        string controller = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");
        string visual = Read("Assets/Scripts/RedMemory/RedMemoryPlayerVisual.cs");
        string scene = Read("Assets/Scenes/RedMemory.unity");

        string[] states = { "Idle", "Run_01", "Run_02", "Jump", "Fall", "Hurt", "Cheer" };
        for (int i = 0; i < states.Length; i++)
        {
            Assert.That(controller, Does.Contain("XH_Player_" + states[i]));
            Assert.That(scene, Does.Contain("XH_Player_" + states[i] + ": {fileID: 21300000"));
        }

        Assert.That(visual, Does.Contain("public enum VisualState { Idle, Run, Jump, Fall, Hurt, GroundStomp, Defeat, Cheer }"));
        Assert.That(visual, Does.Contain("spriteRenderer.flipX = direction < 0f"));
    }

    [Test]
    public void RedMemory_PlayerMovementAndColliderRemainOwnedByTheExistingController()
    {
        string controller = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");
        string player = Read("Assets/Scripts/RedMemory/RedMemoryPlayerController.cs");

        Assert.That(controller, Does.Contain("playerCollider.size = new Vector2(0.75f, 1.45f);"));
        Assert.That(controller, Does.Contain("Player.Configure(this, visuals.transform);"));
        Assert.That(player, Does.Contain("body.velocity = velocity;"));
        Assert.That(player, Does.Contain("playerVisual.PlayHurt();"));
        Assert.That(player, Does.Contain("public void PlayCheer()"));
    }

    [Test]
    public void PrologueAndRedMemory_UseLiTuoTuoNamePortraitAndGameplaySprites()
    {
        string dialogue = Read("Assets/Scripts/Dialogue/DialogueManager.cs");
        string portraits = Read("Assets/Scripts/UI/Portraits/PortraitDisplayController.cs");
        string redMemory = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");
        string scene = Read("Assets/Scenes/RedMemory.unity");

        Assert.That(dialogue, Does.Contain("return \"栗拓拓\";"));
        Assert.That(portraits, Does.Contain("Characters/LiTuoTuo_Front"));
        Assert.That(redMemory, Does.Contain("ShowLine(\"栗拓拓\""));
        Assert.That(redMemory, Does.Not.Contain("ShowLine(\"小禾\""));
        Assert.That(File.Exists(Path.Combine(ProjectRoot, "Assets/Resources/Characters/LiTuoTuo_Front.png")), Is.True);

        string[] guids =
        {
            "33a0fef242cbe4740b5902467d557979",
            "97b9a5d9c2511a340b460b9112d684b6",
            "a818a05a699358d4c99e7f9e934d317f",
            "de6d8a50f58dfd54bafa91025ac86d17",
            "71fc88a05dc5fff4896ac9392e4e231d",
            "b9e82d6e3df372a46806135e65ac2a33",
            "fd136d33ca6515c4f9bb848e53a8b48f"
        };
        for (int i = 0; i < guids.Length; i++) Assert.That(scene, Does.Contain(guids[i]));
    }

    private static string Read(string relativePath) => File.ReadAllText(Path.Combine(ProjectRoot, relativePath));
}
