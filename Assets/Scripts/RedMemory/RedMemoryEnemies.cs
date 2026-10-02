using System.Collections;
using UnityEngine;

public enum RedMemoryEnemyState { Idle, Patrol, Alert, Attack, Hurt, Recover, Defeated }

public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected int maxHealth = 1;
    [SerializeField] protected float patrolDistance = 2f;
    [SerializeField] protected float moveSpeed = 1.25f;
    protected RedMemoryIntroController chapter;
    protected RedMemoryPlayerController player;
    protected SpriteRenderer sprite;
    protected Collider2D attackCollider;
    protected Collider2D hurtCollider;
    protected Vector3 origin;
    protected int health;
    protected float direction = 1f;
    protected bool acceptingHits = true;
    public RedMemoryEnemyState State { get; protected set; }
    public bool IsDefeated => State == RedMemoryEnemyState.Defeated;
    public bool FacingLeft => direction < 0f;

    public virtual void Configure(RedMemoryIntroController owner, RedMemoryPlayerController target, int hp)
    {
        chapter = owner;
        player = target;
        maxHealth = hp;
        health = hp;
        origin = transform.position;
        sprite = GetComponentInChildren<SpriteRenderer>();
        EnemyHurtbox hurtbox = GetComponentInChildren<EnemyHurtbox>();
        EnemyAttackBox attackBox = GetComponentInChildren<EnemyAttackBox>();
        if (hurtbox != null) { hurtbox.Configure(this); hurtCollider = hurtbox.GetComponent<Collider2D>(); }
        if (attackBox != null) { attackBox.Configure(this, owner); attackCollider = attackBox.GetComponent<Collider2D>(); }
    }

    public bool TakeHit(int damage, bool heavy)
    {
        if (!acceptingHits || IsDefeated) return false;
        health = Mathf.Max(0, health - Mathf.Max(1, damage));
        if (health <= 0) StartCoroutine(DefeatRoutine());
        else StartCoroutine(HurtRoutine());
        return true;
    }

    private IEnumerator HurtRoutine()
    {
        acceptingHits = false;
        State = RedMemoryEnemyState.Hurt;
        Vector3 baseScale = transform.localScale;
        transform.localScale = new Vector3(baseScale.x * 1.12f, baseScale.y * 0.62f, baseScale.z);
        yield return new WaitForSeconds(0.18f);
        transform.localScale = baseScale;
        State = RedMemoryEnemyState.Recover;
        yield return new WaitForSeconds(0.18f);
        acceptingHits = true;
        State = RedMemoryEnemyState.Patrol;
    }

    private IEnumerator DefeatRoutine()
    {
        acceptingHits = false;
        State = RedMemoryEnemyState.Defeated;
        if (attackCollider != null) attackCollider.enabled = false;
        if (hurtCollider != null) hurtCollider.enabled = false;
        chapter.NotifyEnemyDefeated(this);
        Vector3 from = transform.localScale;
        float elapsed = 0f;
        while (elapsed < 0.32f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.32f);
            transform.localScale = Vector3.Lerp(from, new Vector3(from.x * 1.18f, 0.05f, from.z), t);
            if (sprite != null) sprite.color = Color.Lerp(Color.white, new Color(1f, 0.55f, 0.25f, 0f), t);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}

public sealed class MemoryCreeperEnemy : EnemyBase
{
    private void Update()
    {
        if (player == null || State == RedMemoryEnemyState.Hurt || State == RedMemoryEnemyState.Defeated) return;
        State = RedMemoryEnemyState.Patrol;
        transform.position += Vector3.right * (direction * moveSpeed * Time.deltaTime);
        if (Mathf.Abs(transform.position.x - origin.x) >= patrolDistance) direction *= -1f;
    }
}

public sealed class JumpBlobEnemy : EnemyBase
{
    [SerializeField] private float detectionDistance = 4.2f;
    [SerializeField] private float alertDuration = 0.55f;
    [SerializeField] private float attackCooldown = 2.2f;
    private float nextAttack;
    private bool attacking;

    private void Update()
    {
        if (player == null || attacking || IsDefeated || State == RedMemoryEnemyState.Hurt) return;
        if (Time.time >= nextAttack && Vector2.Distance(player.transform.position, transform.position) <= detectionDistance)
            StartCoroutine(JumpAttackRoutine());
        else if (State != RedMemoryEnemyState.Recover) State = RedMemoryEnemyState.Idle;
    }

    private IEnumerator JumpAttackRoutine()
    {
        attacking = true;
        State = RedMemoryEnemyState.Alert;
        chapter.NotifyEnemyAlert(transform.position);
        Vector3 baseScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < alertDuration)
        {
            elapsed += Time.deltaTime;
            float squash = 1f - Mathf.Sin(elapsed * 30f) * 0.035f;
            transform.localScale = new Vector3(baseScale.x * 1.12f, baseScale.y * 0.72f * squash, baseScale.z);
            yield return null;
        }
        State = RedMemoryEnemyState.Attack;
        Vector3 start = transform.position;
        float targetX = player.transform.position.x;
        elapsed = 0f;
        while (elapsed < 0.72f && !IsDefeated)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.72f);
            float arc = Mathf.Sin(t * Mathf.PI) * 2.4f;
            transform.position = new Vector3(Mathf.Lerp(start.x, targetX, t), origin.y + arc, start.z);
            yield return null;
        }
        transform.position = new Vector3(transform.position.x, origin.y, transform.position.z);
        transform.localScale = baseScale;
        State = RedMemoryEnemyState.Recover;
        nextAttack = Time.time + attackCooldown;
        yield return new WaitForSeconds(0.35f);
        attacking = false;
        if (!IsDefeated) State = RedMemoryEnemyState.Idle;
    }
}

[RequireComponent(typeof(Collider2D))]
public sealed class EnemyHurtbox : MonoBehaviour
{
    public EnemyBase Owner { get; private set; }
    public float WorldTop => GetComponent<Collider2D>().bounds.max.y;
    public void Configure(EnemyBase owner) => Owner = owner;
}

[RequireComponent(typeof(Collider2D))]
public sealed class EnemyAttackBox : MonoBehaviour
{
    private EnemyBase owner;
    private RedMemoryIntroController chapter;
    private float nextContact;
    public void Configure(EnemyBase enemy, RedMemoryIntroController game) { owner = enemy; chapter = game; }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (owner == null || owner.IsDefeated || Time.time < nextContact) return;
        RedMemoryPlayerController player = other.GetComponentInParent<RedMemoryPlayerController>();
        if (player == null) return;
        EnemyHurtbox hurt = owner.GetComponentInChildren<EnemyHurtbox>();
        bool playerClearlyAbove = hurt != null && player.ColliderBounds.min.y >= hurt.WorldTop - 0.22f && player.Body.velocity.y < 0f;
        if (playerClearlyAbove) return;
        nextContact = Time.time + 0.25f;
        chapter.HandleHazard(owner.transform.position);
    }
}
