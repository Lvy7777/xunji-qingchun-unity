using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class RedMemorySecondPolishTests
{
    private static string Root => Directory.GetParent(Application.dataPath).FullName;
    private static string Read(string path) => File.ReadAllText(Path.Combine(Root, path));

    [Test]
    public void EnemyPresentation_HasReplaceableSlotsAndSeparatedDrivers()
    {
        string controller = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");
        string visuals = Read("Assets/Scripts/RedMemory/RedMemoryEnemyVisuals.cs");
        for (int i = 1; i <= 12; i++) Assert.That(controller, Does.Contain("R_ENEMY_" + i.ToString("000")));
        Assert.That(visuals, Does.Contain("class EnemyVisualBinder"));
        Assert.That(visuals, Does.Contain("class EnemyAnimationDriver"));
        Assert.That(controller, Does.Contain("VisualRoot"));
        Assert.That(controller, Does.Contain("visualRoot.AddComponent<Animator>()"));
        Assert.That(controller, Does.Contain("Hurtbox"));
        Assert.That(controller, Does.Contain("AttackBox"));
    }

    [Test]
    public void TrapPresentation_CoversTelegraphsAndThreeCombinationPatterns()
    {
        string controller = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");
        string traps = Read("Assets/Scripts/RedMemory/RedMemoryTrapPolish.cs");
        string mechanics = Read("Assets/Scripts/RedMemory/RedMemoryPlatformMechanics.cs");
        for (int i = 1; i <= 5; i++) Assert.That(controller, Does.Contain("R_TRAP_" + i.ToString("000")));
        string[] types = { "TrapTelegraphController", "TrapSequenceController", "EnvironmentalSpikeTrap", "SwingingHazard", "MemoryFractureFloor" };
        foreach (string type in types) Assert.That(traps, Does.Contain("class " + type));
        Assert.That(mechanics, Does.Contain("PebbleWarning"));
        Assert.That(mechanics, Does.Contain("ImpactDust"));
        Assert.That(controller, Does.Contain("SEQ_JUMP_ROCK"));
        Assert.That(controller, Does.Contain("SEQ_PLATFORM_SWING"));
        Assert.That(controller, Does.Contain("SEQ_ENEMY_CRUMBLE"));
    }

    [Test]
    public void CheckpointAndGoal_HaveIndependentVisualAndStateComponents()
    {
        string controller = Read("Assets/Scripts/RedMemory/RedMemoryIntroController.cs");
        string presentation = Read("Assets/Scripts/RedMemory/RedMemoryCheckpointGoalVisuals.cs");
        for (int i = 1; i <= 4; i++)
        {
            Assert.That(controller, Does.Contain("R_SAVE_" + i.ToString("000")));
            Assert.That(controller, Does.Contain("R_GOAL_" + i.ToString("000")));
        }
        Assert.That(presentation, Does.Contain("class CheckpointVisualController"));
        Assert.That(presentation, Does.Contain("class CheckpointRespawnPoint"));
        Assert.That(presentation, Does.Contain("class GoalMemoryGatherController"));
        Assert.That(controller, Does.Contain("记忆已记录 · 记录点已更新"));
        Assert.That(controller, Does.Contain("PlayGather(Player.transform)"));
    }
}
