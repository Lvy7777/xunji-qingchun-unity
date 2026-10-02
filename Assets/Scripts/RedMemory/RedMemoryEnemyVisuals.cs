using UnityEngine;

public sealed class EnemyVisualBinder : MonoBehaviour
{
    private SpriteRenderer body;
    private SpriteRenderer accent;
    private Sprite idle;
    private Sprite patrolA;
    private Sprite patrolB;
    private Sprite alert;
    private Sprite attack;
    private Sprite hurt;
    private Sprite defeated;

    public void Configure(SpriteRenderer bodyRenderer, SpriteRenderer accentRenderer, Sprite idleSprite,
        Sprite patrolSpriteA, Sprite patrolSpriteB, Sprite alertSprite, Sprite attackSprite,
        Sprite hurtSprite, Sprite defeatedSprite)
    {
        body = bodyRenderer;
        accent = accentRenderer;
        idle = idleSprite != null ? idleSprite : body.sprite;
        patrolA = patrolSpriteA != null ? patrolSpriteA : idle;
        patrolB = patrolSpriteB != null ? patrolSpriteB : patrolA;
        alert = alertSprite != null ? alertSprite : idle;
        attack = attackSprite != null ? attackSprite : alert;
        hurt = hurtSprite != null ? hurtSprite : idle;
        defeated = defeatedSprite != null ? defeatedSprite : hurt;
        Apply(RedMemoryEnemyState.Idle, false);
    }

    public void Apply(RedMemoryEnemyState state, bool alternatePatrolFrame)
    {
        if (body == null) return;
        switch (state)
        {
            case RedMemoryEnemyState.Patrol: body.sprite = alternatePatrolFrame ? patrolB : patrolA; break;
            case RedMemoryEnemyState.Alert: body.sprite = alert; break;
            case RedMemoryEnemyState.Attack: body.sprite = attack; break;
            case RedMemoryEnemyState.Hurt: body.sprite = hurt; break;
            case RedMemoryEnemyState.Defeated: body.sprite = defeated; break;
            default: body.sprite = idle; break;
        }
    }

    public void SetFacing(bool faceLeft)
    {
        if (body != null) body.flipX = faceLeft;
        if (accent != null) accent.transform.localPosition = new Vector3(faceLeft ? -0.16f : 0.16f, accent.transform.localPosition.y, accent.transform.localPosition.z);
    }

    public void SetTint(Color color)
    {
        if (body != null) body.color = color;
        if (accent != null) accent.color = new Color(color.r, color.g, color.b, accent.color.a);
    }
}

public sealed class EnemyAnimationDriver : MonoBehaviour
{
    private EnemyBase enemy;
    private EnemyVisualBinder binder;
    private Transform visualRoot;
    private Vector3 baseScale;
    private Vector3 basePosition;
    private RedMemoryEnemyState previousState;
    private float stateStartTime;

    public void Configure(EnemyBase owner, EnemyVisualBinder visualBinder, Transform root)
    {
        enemy = owner;
        binder = visualBinder;
        visualRoot = root;
        baseScale = root.localScale;
        basePosition = root.localPosition;
        previousState = owner.State;
        stateStartTime = Time.time;
    }

    private void LateUpdate()
    {
        if (enemy == null || binder == null || visualRoot == null) return;
        RedMemoryEnemyState state = enemy.State;
        if (state != previousState) { previousState = state; stateStartTime = Time.time; }
        float age = Time.time - stateStartTime;
        binder.Apply(state, Mathf.FloorToInt(age * 5f) % 2 == 1);
        binder.SetFacing(enemy.FacingLeft);

        float breathe = Mathf.Sin(Time.time * 3.2f) * 0.025f;
        Vector3 scale = baseScale;
        Vector3 position = basePosition;
        if (state == RedMemoryEnemyState.Alert) scale = new Vector3(baseScale.x * 1.12f, baseScale.y * 0.72f, baseScale.z);
        else if (state == RedMemoryEnemyState.Attack) scale = new Vector3(baseScale.x * 0.92f, baseScale.y * 1.12f, baseScale.z);
        else if (state == RedMemoryEnemyState.Hurt) scale = new Vector3(baseScale.x * 1.1f, baseScale.y * 0.78f, baseScale.z);
        else if (state != RedMemoryEnemyState.Defeated)
        {
            scale = new Vector3(baseScale.x * (1f - breathe), baseScale.y * (1f + breathe), baseScale.z);
            position += Vector3.up * Mathf.Max(0f, Mathf.Sin(Time.time * 3.2f)) * 0.025f;
        }
        visualRoot.localScale = scale;
        visualRoot.localPosition = position;
    }
}
