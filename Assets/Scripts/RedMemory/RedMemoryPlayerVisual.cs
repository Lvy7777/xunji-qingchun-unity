using UnityEngine;

/// <summary>
/// Lightweight sprite-state driver for LiTuoTuo's RedMemory gameplay form.
/// Movement and collisions remain owned by RedMemoryPlayerController.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class RedMemoryPlayerVisual : MonoBehaviour
{
    public enum VisualState { Idle, Run, Jump, Fall, Hurt, GroundStomp, Defeat, Cheer }

    private SpriteRenderer spriteRenderer;
    private RedMemoryPlayerController player;
    private Sprite idle;
    private Sprite run01;
    private Sprite run02;
    private Sprite jump;
    private Sprite fall;
    private Sprite hurt;
    private Sprite cheer;
    private float stateLockUntil;
    private VisualState lockedState;

    public VisualState CurrentState { get; private set; }

    public void Configure(
        RedMemoryPlayerController owner,
        Sprite idleSprite,
        Sprite runSprite01,
        Sprite runSprite02,
        Sprite jumpSprite,
        Sprite fallSprite,
        Sprite hurtSprite,
        Sprite cheerSprite)
    {
        player = owner;
        idle = idleSprite;
        run01 = runSprite01;
        run02 = runSprite02;
        jump = jumpSprite;
        fall = fallSprite != null ? fallSprite : jumpSprite;
        hurt = hurtSprite != null ? hurtSprite : idleSprite;
        cheer = cheerSprite != null ? cheerSprite : idleSprite;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = idle;
        CurrentState = VisualState.Idle;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (player == null || spriteRenderer == null) return;

        VisualState target;
        if (Time.time < stateLockUntil)
        {
            target = lockedState;
        }
        else if (!player.IsGrounded)
        {
            target = player.Body.velocity.y > 0.05f ? VisualState.Jump : VisualState.Fall;
        }
        else if (Mathf.Abs(player.Body.velocity.x) > 0.18f)
        {
            target = VisualState.Run;
        }
        else
        {
            target = VisualState.Idle;
        }

        CurrentState = target;
        spriteRenderer.sprite = ResolveSprite(target);

        float idleBreath = target == VisualState.Idle ? Mathf.Sin(Time.time * 2.5f) * 0.012f : 0f;
        transform.localPosition = new Vector3(0f, -0.72f + idleBreath, 0f);
    }

    private Sprite ResolveSprite(VisualState state)
    {
        switch (state)
        {
            case VisualState.Run:
                if (run01 == null) return idle;
                return run02 != null && Mathf.FloorToInt(Time.time * 9f) % 2 != 0 ? run02 : run01;
            case VisualState.Jump: return jump != null ? jump : idle;
            case VisualState.Fall: return fall != null ? fall : jump;
            case VisualState.Hurt: return hurt;
            case VisualState.GroundStomp: return fall != null ? fall : jump;
            case VisualState.Defeat: return hurt != null ? hurt : idle;
            case VisualState.Cheer: return cheer;
            default: return idle;
        }
    }

    public void SetFacing(float direction)
    {
        if (spriteRenderer != null && Mathf.Abs(direction) > 0.01f) spriteRenderer.flipX = direction < 0f;
    }

    public void PlayHurt(float duration = 0.38f)
    {
        lockedState = VisualState.Hurt;
        stateLockUntil = Time.time + duration;
    }

    public void PlayCheer(float duration = 0.7f)
    {
        lockedState = VisualState.Cheer;
        stateLockUntil = Time.time + duration;
    }

    public void PlayGroundStomp(float duration = 0.35f)
    {
        lockedState = VisualState.GroundStomp;
        stateLockUntil = Time.time + duration;
    }

    public void PlayDefeat(float duration = 10f)
    {
        lockedState = VisualState.Defeat;
        stateLockUntil = Time.time + duration;
    }
}
