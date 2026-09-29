using UnityEngine;

[CreateAssetMenu(fileName = "YouthPlatformerDifficulty", menuName = "Game/Youth Memory/Platformer Difficulty")]
public sealed class YouthPlatformerDifficultyProfile : ScriptableObject
{
    [Header("Player")]
    [Min(1)] public int startingLives = 4;
    [Min(1f)] public float moveSpeed = 7.4f;
    [Min(1f)] public float acceleration = 48f;
    [Min(1f)] public float jumpSpeed = 13.2f;
    [Min(0.1f)] public float gravityScale = 3.75f;
    [Range(0f, 0.3f)] public float coyoteTime = 0.13f;
    [Range(0f, 0.3f)] public float jumpBufferTime = 0.15f;
    [Range(0.2f, 0.9f)] public float jumpReleaseMultiplier = 0.48f;

    [Header("Challenges")]
    [Min(0.5f)] public float movingPlatformCycle = 2.8f;
    [Min(0.1f)] public float crumbleWarningTime = 0.55f;
    [Min(0.5f)] public float crumbleRespawnTime = 2.6f;
    [Min(0.1f)] public float hazardRespawnDelay = 0.6f;
    [Min(0f)] public float patrolHazardSpeed = 2.15f;
    public float windAcceleration = -3.0f;
}