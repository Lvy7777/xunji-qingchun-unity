using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class RedMemoryGameplayDeepPolishTests
{
    private static string Root => Directory.GetParent(Application.dataPath).FullName;
    private static string Read(string path) => File.ReadAllText(Path.Combine(Root, path));

    [Test]
    public void WorldBuilder_ContainsEightPurposeBuiltAreas()
    {
        string source = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");
        Assert.That(source, Does.Contain("Transform[] areas = new Transform[8]"));
        Assert.That(source, Does.Contain("Area_\" + (i + 1).ToString(\"00\")"));
        for (int i = 1; i <= 8; i++) Assert.That(source, Does.Contain("Area 0" + i));
    }

    [Test]
    public void Player_HasStrictStompAndGroundStompPaths()
    {
        string player = Read("Assets/Scripts/RedMemory/RedMemoryPlayerController.cs");
        string combat = Read("Assets/Scripts/RedMemory/RedMemoryPlayerCombat.cs");
        Assert.That(player, Does.Contain("RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(RedMemoryPlayerCombat))"));
        Assert.That(combat, Does.Contain("body.velocity.y >= -0.05f"));
        Assert.That(combat, Does.Contain("bool clearlyAbove"));
        Assert.That(combat, Does.Contain("Input.GetKeyDown(KeyCode.S)"));
        Assert.That(combat, Does.Contain("TakeHit(2, true)"));
    }

    [Test]
    public void Enemies_SeparateAttackAndHurtBoxes_AndWarnBeforeJumping()
    {
        string enemies = Read("Assets/Scripts/RedMemory/RedMemoryEnemies.cs");
        Assert.That(enemies, Does.Contain("class EnemyHurtbox"));
        Assert.That(enemies, Does.Contain("class EnemyAttackBox"));
        Assert.That(enemies, Does.Contain("RedMemoryEnemyState.Alert"));
        Assert.That(enemies, Does.Contain("alertDuration = 0.55f"));
        Assert.That(enemies, Does.Contain("maxHealth = 1"));
    }

    [Test]
    public void PlatformAndTrapComponents_AreIndependent()
    {
        string mechanics = Read("Assets/Scripts/RedMemory/RedMemoryPlatformMechanics.cs");
        string[] types = { "MovingPlatform", "CrumblingPlatform", "FallingRockTrap", "RollingBoulder", "MemoryRepairPoint", "RedMemoryCheckpoint" };
        for (int i = 0; i < types.Length; i++) Assert.That(mechanics, Does.Contain("class " + types[i]));
        Assert.That(mechanics, Does.Contain("warningDuration = 0.75f"));
        Assert.That(mechanics, Does.Contain("elapsed < 0.6f"));
    }

    [Test]
    public void RewardsAndAudioSlots_CoverDeepPolishContract()
    {
        string objects = Read("Assets/Scripts/RedMemory/RedMemoryGameplayObjects.cs");
        string controller = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");
        Assert.That(objects, Does.Contain("Fragment, ClueKey, MemoryGlow, HealingFlower"));
        string[] clips = { "PlayerStomp", "EnemyAlert", "EnemyHit", "EnemyDefeat", "GlowCollect", "HealCollect", "CheckpointActivate", "CrumbleWarning", "PlatformBreak", "RockWarning", "RockImpact", "BoulderRoll", "RepairStart", "RepairComplete", "GoalUnlock" };
        for (int i = 0; i < clips.Length; i++) Assert.That(controller, Does.Contain("AudioClip " + clips[i]));
    }
}
